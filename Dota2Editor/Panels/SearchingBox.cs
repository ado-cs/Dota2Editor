using System.Text;

namespace Dota2Editor.Panels
{
    public partial class SearchingBox : UserControl
    {
        private readonly List<string> _items = [];
        private readonly HashSet<string> _itemSet = [];
        private readonly CharTree _charTree = new();
        private bool _enableSelection = true;
        private string? _lastSelection = null;
        private Action<string>? _renderAction = null;
        private bool _deleting = false;

        private class CharTree
        {
            private static readonly char END = '\0';
            private static readonly CharTree END_NODE = new();

            private readonly char _current;
            private readonly Dictionary<char, CharTree> _next = [];

            public CharTree() { }

            public CharTree(string str) : this(str, 0) { }

            public void Reset(ICollection<string> items)
            {
                _next.Clear();
                foreach (var item in items) Append(item, 0);
            }

            private CharTree(string str, int startIndex)
            {
                if (str == null || str.Length <= startIndex) _current = END;
                else
                {
                    _current = str[startIndex];
                    Append(str, startIndex + 1);
                }
            }

            private void Append(string str, int startIndex)
            {
                if (str == null) return;
                if (str.Length <= startIndex) _next.TryAdd(END, END_NODE);
                else
                {
                    var key = str[startIndex];
                    if (_next.TryGetValue(key, out var t)) t.Append(str, startIndex + 1);
                    else _next.Add(key, new CharTree(str, startIndex));
                }
            }

            public string[] Search(string str)
            {
                if (string.IsNullOrEmpty(str)) return [];
                var list = new List<string>();
                var tree = this;
                foreach (var c in str)
                {
                    if (tree._next.TryGetValue(c, out var t)) tree = t;
                    else return [];
                }
                tree.Search(new(str), list);
                return [.. list];
            }

            private void Search(StringBuilder sb, List<string> result)
            {
                if (_next.ContainsKey(END)) result.Add(sb.ToString());
                foreach (var v in _next.Values)
                {
                    if (v._current != END)
                    {
                        var pos = sb.Length;
                        sb.Append(v._current);
                        v.Search(sb, result);
                        sb.Remove(pos, sb.Length - pos);
                    }
                }
            }
        }

        public ICollection<string> Items 
        {
            set 
            { 
                _enableSelection = false;
                _items.Clear();
                _itemSet.Clear();
                comboBox1.Items.Clear();
                _items.AddRange(value);
                _charTree.Reset(value);
                _deleting = false;
                _lastSelection = null;
                foreach (var item in value)
                {
                    _itemSet.Add(item);
                    comboBox1.Items.Add(item);
                }
                comboBox1.Text = string.Empty;
                _enableSelection = true; 
            } 
        }

        public Action<string> SelectionHandler { set => _renderAction = value; }

        public string? SelectedItem 
        { 
            get => comboBox1.SelectedItem as string; 
            set 
            {
                if (string.IsNullOrEmpty(value)) comboBox1.SelectedIndex = -1;
                else if (_itemSet.Contains(value)) comboBox1.SelectedItem = value;
            }
        }

        public SearchingBox()
        {
            InitializeComponent();
            BindEvents();
        }

        private void BindEvents()
        {
            comboBox1.KeyPress += (_, e) =>
            {
                _deleting = e.KeyChar == 8 || e.KeyChar == 46;
                if (e.KeyChar == 13)
                {
                    if (comboBox1.SelectionLength > 0 && comboBox1.SelectionStart == comboBox1.Text.Length - comboBox1.SelectionLength)
                    {
                        comboBox1.SelectionLength = 0;
                        comboBox1.SelectionStart = comboBox1.Text.Length;
                    }
                    if (comboBox1.Text.Length > 0 && !Equals(_lastSelection, comboBox1.Text) && _itemSet.Contains(comboBox1.Text))
                    {
                        _lastSelection = comboBox1.Text;
                        _renderAction?.Invoke(comboBox1.Text);
                    }
                    e.Handled = true;
                }
            };
            comboBox1.TextUpdate += (_, _) =>
            {
                if (comboBox1.Text.Length == 0)
                {
                    if (comboBox1.Items.Count == _items.Count) return;
                    comboBox1.Items.Clear();
                    comboBox1.Items.AddRange(_items.ToArray());
                }
                else
                {
                    var lastPos = comboBox1.SelectionStart;
                    var lastLen = comboBox1.SelectionLength;

                    //replace the items with the searching result
                    var result = _charTree.Search(comboBox1.Text);
                    if (result.Length == 0) comboBox1.Items.Clear();
                    else if (comboBox1.Items.Count == 0) comboBox1.Items.AddRange(result);
                    else
                    {
                        var flag = false;
                        if (comboBox1.Items.Count != result.Length) flag = true;
                        else
                        {
                            for (var i = 0; i < result.Length; i++)
                            {
                                if (!Equals(result[i], comboBox1.Items[i]))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            comboBox1.Items.Clear();
                            comboBox1.Items.AddRange(result);
                        }
                    }
                    //fill the text by the first result 
                    if (comboBox1.Items.Count > 0 && !_deleting)
                    {
                        _enableSelection = false;
                        var item = comboBox1.Items[0] as string;
                        if (!comboBox1.Text.Equals(item))
                        {
                            var pos = comboBox1.Text.Length;
                            comboBox1.Text = item;
                            comboBox1.SelectionStart = pos;
                            comboBox1.SelectionLength = comboBox1.Text.Length - pos;
                        }
                        else
                        {
                            comboBox1.SelectionLength = lastLen;
                            comboBox1.SelectionStart = lastPos;
                        }
                        _enableSelection = true;
                    }
                    else
                    {
                        comboBox1.SelectionLength = lastLen;
                        comboBox1.SelectionStart = lastPos;
                    }
                }
            };
            comboBox1.SelectedIndexChanged += (_, e) =>
            {
                if (_enableSelection && comboBox1.SelectedItem is string item && !Equals(_lastSelection, item))
                {
                    _lastSelection = item;
                    _renderAction?.Invoke(item);
                }
            };
        }
    }
}
