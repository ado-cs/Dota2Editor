using Dota2Editor.Basic;
using Dota2Editor.Properties;
using System.Diagnostics;
using System.Windows.Forms;

namespace Dota2Editor.Forms
{
    public partial class EditorPanel : UserControl
    {
        public EditorPanel()
        {
            InitializeComponent();
            BindEvents();
            ResetAllText();
        }

        private bool _textMode;
        private int _index = -1;
        private DSONObject? _root;
        private DSONObject? _current;
        private Action? _newEditorFormCreator;

        public Action? NewEditorFormCreator { set => pictureBox4.Visible = (_newEditorFormCreator = value) != null; }

        public void ResetAllText()
        {
            toolStripMenuItem1.Text = Globalization.Get("EditorPanel.ListBox.Add") + "(&A)";
            toolStripMenuItem2.Text = Globalization.Get("EditorPanel.ListBox.Load") + "(&L)";
            toolStripMenuItem3.Text = Globalization.Get("EditorPanel.ListBox.Rename") + "(&R)";
            toolStripMenuItem4.Text = Globalization.Get("EditorPanel.ListBox.Delete") + "(&D)";
            toolStripMenuItem5.Text = Globalization.Get("EditorPanel.ListBox.Detail") + "(&S)";
            toolTip1.SetToolTip(pictureBox4, Globalization.Get("EditorPanel.Tip.New"));
            toolTip1.SetToolTip(pictureBox3, Globalization.Get("EditorPanel.Tip." + (_textMode ? "KeyMode" : "TextMode")));
            toolTip1.SetToolTip(pictureBox2, Globalization.Get("EditorPanel.Tip.Open"));
            toolTip1.SetToolTip(pictureBox1, Globalization.Get("EditorPanel.Tip.Batch"));
        }

        private void BindEvents()
        {
            comboBox1.SelectedIndexChanged += (_, _) => ShowItem(true);
            contextMenuStrip1.Opening += (_, _) =>
            {
                toolStripMenuItem1.Enabled = _root != null;
                toolStripMenuItem3.Enabled = toolStripMenuItem5.Enabled = listBox1.SelectedItems.Count == 1;
                toolStripMenuItem2.Enabled = toolStripMenuItem4.Enabled = listBox1.SelectedItems.Count > 0;
            };
            toolStripMenuItem1.Click += (_, _) => OpenInputDialog("Form1.NewRecord", string.Empty, AddLocalRecord, s => listBox1.Items.Add(s));
            toolStripMenuItem2.Click += (_, _) => LoadLocalRecord(GetSelectedItems());
            toolStripMenuItem3.Click += (_, _) =>
            {
                if (listBox1.SelectedItem is string item) OpenInputDialog("Form1.RenameRecord", item, s => RenameLocalRecord(item, s), s => listBox1.Items[listBox1.SelectedIndex] = s);
            };
            toolStripMenuItem4.Click += (_, _) =>
            {
                var list = GetSelectedItems();
                DeleteLocalRecord(list);
                foreach (var item in list) listBox1.Items.Remove(item);
            };
            toolStripMenuItem5.Click += (_, _) =>
            {
                if (listBox1.SelectedItem is string item) ShowRecordDetails(item);
            };
            pictureBox1.Click += (_, _) =>
            {
                if (_textMode) UpdateRootByText();
                new BatchModificationForm(BatchModify).ShowDialog(); 
                ShowItem(false);
            };
            pictureBox2.Click += (_, _) => OpenEditingFolder();
            pictureBox3.Click += (_, _) => SwithEditorMode();
            pictureBox4.Click += (_, _) => _newEditorFormCreator?.Invoke();
        }

        private void OpenInputDialog(string textKey, string value, Func<string, int> handler, Action<string> consumer)
        {
            while (true)
            {
                var diag = new InputForm(Text, Globalization.Get(textKey), value);
                if (diag.ShowDialog() == DialogResult.OK)
                {
                    var f = handler(diag.Result);
                    if (f == 0) continue;
                    if (f == 1) consumer(diag.Result);
                }
                return;
            }
        }

        private List<string> GetSelectedItems()
        {
            var list = new List<string>();
            foreach (var item in listBox1.SelectedItems)
            {
                if (item is string s) list.Add(s);
            }
            return list;
        }

        private void SwithEditorMode()
        {
            _textMode = !_textMode;
            if (_textMode)
            {
                if (tableLayoutPanel1.Controls.Contains(flowLayoutPanel1)) tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.Controls.Add(textBox1, 0, 1);
                pictureBox3.Image = Resources.t;
                toolTip1.SetToolTip(pictureBox3, Globalization.Get("EditorPanel.Tip.KeyMode"));
            }
            else
            {
                UpdateRootByText();
                if (tableLayoutPanel1.Controls.Contains(textBox1)) tableLayoutPanel1.Controls.Remove(textBox1);
                tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 1);
                pictureBox3.Image = Resources.k;
                toolTip1.SetToolTip(pictureBox3, Globalization.Get("EditorPanel.Tip.TextMode"));
            }
            ShowItem(false);
        }

        private void UpdateRootByText()
        {
            if (_current == null) return;
            DSONObject obj;
            try { obj = DSONObject.Parse(textBox1.Text); } catch { Debug.WriteLine("?"); return; }
            var changes = obj.FindChanges(_current);
            if (changes != null) _current.UpdateValues(changes);
        }

        private void ShowItem(bool invokedByComboBox)
        {
            if (invokedByComboBox && _textMode) UpdateRootByText();
            if (comboBox1.SelectedItem is string item && _root != null && _root.ExpandedObject.TryGetValue(item, out var v) && v is DSONObject o)
            {
                _current = o;

                if (_textMode)
                {
                    textBox1.ReadOnly = false;
                    textBox1.Text = v.ToString();
                    return;
                }

                flowLayoutPanel1.SuspendLayout();

                var index = 0;
                AddObject(o.ExpandedObject, Common.IsFolder(_index), ref index);

                var flag = true;
                while (index < flowLayoutPanel1.Controls.Count)
                {
                    var control = flowLayoutPanel1.Controls[index];
                    if (index == flowLayoutPanel1.Controls.Count - 1 && control is Label l && Equals(l.Text, string.Empty)) { l.Visible = true; flag = false; break; }
                    control.Visible = false;
                    index++;
                }

                if (flag)
                {
                    var label = new Label() { Text = string.Empty, AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
                    flowLayoutPanel1.Controls.Add(label);
                    flowLayoutPanel1.SetFlowBreak(label, true);
                }

                flowLayoutPanel1.Visible = true;
                flowLayoutPanel1.ResumeLayout(false);
                flowLayoutPanel1.PerformLayout();
            }
            else _current = null;
        }

        private void AddObject(DSONObject obj, bool highlight, ref int index, int indent = 3)
        {
            foreach (var key in obj.Keys)
            {
                if (highlight && "Version".Equals(key)) continue;

                Label? label = null;
                while (flowLayoutPanel1.Controls.Count > index)
                {
                    if (flowLayoutPanel1.Controls[index] is Label l) { label = l; break; }
                    else flowLayoutPanel1.Controls.RemoveAt(index);
                }
                index++;
                if (label == null)
                {
                    flowLayoutPanel1.Controls.Add(label = new Label { AutoSize = true, Margin = new Padding(indent, 8, 0, 0) });
                    flowLayoutPanel1.SetFlowBreak(label, true);
                }

                if (label.Margin.Left != indent) label.Margin = new Padding(indent, 8, 0, 0);
                label.Text = key;
                label.ForeColor = highlight ? Color.OrangeRed : Color.Black;
                label.Visible = true;

                var val = obj[key];
                if (val is DSONObject o) AddObject(o, false, ref index, indent + 40);
                else if (val is DSONValue v)
                {
                    TextBox? tb = null;
                    while (flowLayoutPanel1.Controls.Count > index)
                    {
                        if (flowLayoutPanel1.Controls[index] is TextBox t) { tb = t; break; }
                        else flowLayoutPanel1.Controls.RemoveAt(index);
                    }
                    index++;
                    if (tb == null)
                    {
                        flowLayoutPanel1.Controls.Add(tb = new TextBox { Size = new Size(300, 23), Margin = new Padding(indent, 0, 0, 0) });
                        flowLayoutPanel1.SetFlowBreak(tb, true);
                        tb.TextChanged += (sender, _) =>
                        {
                            if (sender is TextBox t && t.Tag is DSONValue v) v.Text = t.Text ?? string.Empty;
                        };
                    }
                    if (tb.Margin.Left != indent) tb.Margin = new Padding(indent, 0, 0, 0);
                    tb.Tag = null;
                    tb.Text = val.ToString();
                    tb.Tag = val;
                    tb.Visible = true;
                }
            }
        }

        public bool ResetView(int index)
        {
            DSONObject? root = Common.ReadData(index);
            if (root == null) return false;
            _index = index;
            _root = root;
            _current = null;
            flowLayoutPanel1.Visible = false;
            textBox1.ReadOnly = true;
            textBox1.Text = string.Empty;
            splitContainer1.Visible = true;
            UpdateComboBox(false);
            //load records
            var recPath = Common.GetRecordPath(index);
            listBox1.Items.Clear();
            if (Directory.Exists(recPath))
            {
                var ext = "." + Common.Ext;
                foreach (var file in Directory.GetFiles(recPath))
                {
                    if (ext.Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
                        listBox1.Items.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            return true;
        }

        public bool ReloadView(bool keepChanges)
        {
            DSONObject? root = Common.ReadData(_index);
            if (root != null)
            {
                if (keepChanges)
                {
                    var state = _root?.ModifiedValues;
                    if (state != null) root.UpdateValues(state);
                }
                _root = root;
                UpdateComboBox(true);
                return true;
            }
            ClearView();
            return false;
        }

        public void ClearView()
        {
            splitContainer1.Visible = false;
            textBox1.ReadOnly = true;
            textBox1.Text = string.Empty;
            _index = -1;
            _root = null;
            _current = null;
            listBox1.Items.Clear();
            comboBox1.Text = string.Empty;
            comboBox1.Items.Clear();
        }

        private void UpdateComboBox(bool keepSelection)
        {
            if (_root == null) return;
            var item = comboBox1.SelectedItem;
            var obj = _root.ExpandedObject;
            comboBox1.Text = string.Empty;
            comboBox1.Items.Clear();
            var flag = false;
            foreach (var key in obj.Keys)
            {
                if (obj[key] is DSONObject) comboBox1.Items.Add(key);
                if (keepSelection && !flag && item != null && Equals(item, key)) flag = true;
            }
            if (keepSelection && flag) comboBox1.SelectedItem = item;
        }

        private void OpenEditingFolder() => Common.OpenInExplorer(_index, comboBox1.SelectedItem as string);

        private int BatchModify(string key, string value, double val, BatchModificationForm.Operator flag) => 
            _root == null ? 0 : ModifyObject(_root.ExpandedObject, key.Split('.'), 0, value, val, flag, true);

        private static int ModifyObject(DSONObject obj, string[] keys, int keyIndex, string value, double val, BatchModificationForm.Operator flag, bool doExpand)
        {
            if (keys.Length == 0 || keyIndex < 0) return 0;
            var num = 0;
            foreach (var pair in obj)
            {
                if (pair.Value is DSONObject o)
                {
                    if (doExpand) o = o.ExpandedObject;
                    if (keyIndex < keys.Length - 1 && MatchPattern(pair.Key, keys[keyIndex]))
                        num += ModifyObject(o, keys, keyIndex + 1, value, val, flag, false);
                    num += ModifyObject(o, keys, 0, value, val, flag, false);
                }
                else if (pair.Value is DSONValue v && MatchPattern(pair.Key, keys[keyIndex]))
                {
                    if (flag == BatchModificationForm.Operator.Equals)
                    {
                        v.Text = value;
                        num++;
                    }
                    else
                    {
                        if (v.Text.Contains(' '))
                        {
                            var t = false;
                            var s = v.Text.Split(' ');
                            for (var i = 0; i < s.Length; i++)
                            {
                                if (TryModifyValue(s[i], val, flag, out var result))
                                {
                                    s[i] = result;
                                    t = true;
                                }
                            }
                            if (t)
                            {
                                v.Text = string.Join(' ', s);
                                num++;
                            }
                        }
                        else if (TryModifyValue(v.Text, val, flag, out var result))
                        {
                            v.Text = result;
                            num++;
                        }
                    }
                }
            }
            return num;
        }

        private static bool TryModifyValue(string text, double val, BatchModificationForm.Operator flag, out string result)
        {
            if (double.TryParse(text, out var v))
            {
                if (flag == BatchModificationForm.Operator.Increase) v += val;
                else if (flag == BatchModificationForm.Operator.Multiply) v *= val;
                result = v.ToString("F1");
                return true;
            }
            result = string.Empty;
            return false;
        }

        private static bool MatchPattern(string str, string pattern)
        {
            var f = new bool[str.Length + 1, pattern.Length + 1];
            f[0, 0] = true;
            for (var i = 1; i <= pattern.Length; ++i)
            {
                if (pattern[i - 1] == '*') f[0, i] = true;
                else break;
            }
            for (var i = 1; i <= str.Length; ++i)
            {
                for (var j = 1; j <= pattern.Length; ++j)
                {
                    var c = pattern[j - 1];
                    if (c == '*') f[i, j] = f[i, j - 1] || f[i - 1, j];
                    else if (c == '?' || str[i - 1] == pattern[j - 1]) f[i, j] = f[i - 1, j - 1];
                }
            }
            return f[str.Length, pattern.Length];
        }

        private int AddLocalRecord(string name)
        {
            if (_root == null) return -1;
            if (IsIllegalName(name)) return 0;
            var dir = Common.GetRecordPath(_index);
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"{name}.{Common.Ext}");
            if (File.Exists(path))
            {
                MessageBox.Show(Globalization.Get("Form1.DeplicatedRecord", name));
                return 0;
            }
            DSONObject? root = Common.ReadData(_index, false);
            if (root == null) return -1;
            var obj = _root.FindChanges(root);
            if (obj == null)
            {
                MessageBox.Show(Globalization.Get("Form1.NoChanges"));
                return -1;
            }
            var tree = _root.BuildTreeByChanges(root);
            if (tree != null && new ChangesCheckingForm(tree).ShowDialog() != DialogResult.OK) return -1;
            File.WriteAllText(path, obj.ToString());
            return 1;
        }

        private void LoadLocalRecord(ICollection<string> names)
        {
            if (_root == null || names.Count == 0) return;
            var list = new List<DSONObject>();
            var path = string.Empty;
            var dir = Common.GetRecordPath(_index);
            try
            {
                foreach (var name in names)
                {
                    path = Path.Combine(dir, $"{name}.{Common.Ext}");
                    if (!File.Exists(path))
                    {
                        MessageBox.Show(Globalization.Get("Form1.RecordMissing", name));
                        return;
                    }
                    list.Add(DSONObject.Parse(File.ReadAllText(path)));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(Globalization.Get("Form1.FailedInParsing", path, e.Message));
                return;
            }
            DSONObject? root = Common.ReadData(_index, false);
            if (root == null) return;
            foreach (var item in list) root.UpdateValues(item);
            _root = root;
            UpdateComboBox(true);
            MessageBox.Show(Globalization.Get("Form1.SuccessInLoadingRecord"));
        }

        private int RenameLocalRecord(string name, string newName)
        {
            if (_index == -1 || string.IsNullOrEmpty(name)) return -1;
            if (IsIllegalName(name)) return 0;
            if (Equals(name, newName)) return 1;
            var dir = Common.GetRecordPath(_index);

            var path = Path.Combine(dir, $"{name}.{Common.Ext}");
            if (!File.Exists(path))
            {
                MessageBox.Show(Globalization.Get("Form1.RecordMissing", name));
                return -1;
            }
            var newPath = Path.Combine(dir, $"{newName}.{Common.Ext}");
            if (File.Exists(newPath))
            {
                MessageBox.Show(Globalization.Get("Form1.DeplicatedRecord", newName));
                return 0;
            }
            File.Move(path, newPath);
            return 1;
        }

        private void DeleteLocalRecord(ICollection<string> names)
        {
            if (_index == -1) return;
            var dir = Common.GetRecordPath(_index);
            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name)) continue;
                var path = Path.Combine(dir, $"{name}.{Common.Ext}");
                if (File.Exists(path)) File.Delete(path);
            }
        }

        private void ShowRecordDetails(string name)
        {
            if (_index == -1 || string.IsNullOrEmpty(name)) return;

            var path = Path.Combine(Common.GetRecordPath(_index), $"{name}.{Common.Ext}");
            if (!File.Exists(path))
            {
                MessageBox.Show(Globalization.Get("Form1.RecordMissing", name));
                return;
            }
            DSONObject rec;
            try
            {
                rec = DSONObject.Parse(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                MessageBox.Show(Globalization.Get("Form1.FailedInParsing", path, e.Message));
                return;
            }
            DSONObject? root = Common.ReadData(_index, false);
            if (root == null) return;
            var tree = rec.BuildTreeByChanges(root);
            if (tree == null)
            {
                MessageBox.Show(Globalization.Get("Form1.NoChanges"));
                return;
            }
            new ChangesCheckingForm(tree).ShowDialog();
        }

        public void StashChanges()
        {
            if (_root == null || !_root.Modified) return;
            if (_textMode) UpdateRootByText();
            Common.Save(_index, _root);
        }

        private static bool IsIllegalName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show(Globalization.Get("Form1.EmptyName"));
                return true;
            }
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in name)
            {
                if (invalidChars.Contains(c))
                {
                    MessageBox.Show(Globalization.Get("Form1.IllegalPathChar", c));
                    return true;
                }
            }
            return false;
        }
    }
}
