using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Dota2Editor.Basic
{
    public class DSONObject(IDSONItem? parent) : IDSONItem(parent), IEnumerable<KeyValuePair<string, IDSONItem>>
    {
        public static DSONObject Parse(string text)
        {
            var root = new DSONObject();
            if (string.IsNullOrWhiteSpace(text)) return root;
            var lineNum = 1;
            var sb = new StringBuilder();
            string? key = null;
            var keyLineNum = -1;
            var stack = new Stack<DSONObject>();
            var current = root;
            var lastChar = '\0';
            var flag = 0; //1 = " -1 = //

            foreach (var c in text)
            {
                if (flag == -1)
                {
                    if (lastChar == '\r' && c != '\n') throw new InvalidDataException(Globalization.Get("DSONObject.IllegalChar", "\\r", lineNum));
                    if (c == '\n')
                    {
                        flag = 0;
                        lineNum++;
                    }
                }
                else if (flag == 0)
                {
                    if (lastChar == '\r' && c != '\n') throw new InvalidDataException(Globalization.Get("DSONObject.IllegalChar", "\\r", lineNum));
                    if (c == '\n') lineNum++;
                    else if (lastChar == '/')
                    {
                        if (c != '/') throw new InvalidDataException(Globalization.Get("DSONObject.IllegalChar", c, lineNum));
                        flag = -1;
                    }
                    else if (c != ' ' && c != '\t')
                    {
                        if (c == '"') flag = 1;
                        else if (c == '{')
                        {
                            if (key == null) throw new InvalidDataException(Globalization.Get("DSONObject.IllegalChar", '{', lineNum));
                            stack.Push(current);
                            if (current.ContainsKey(key))
                            {
                                if (current[key] is DSONObject obj) current = obj;
                                else throw new InvalidDataException(Globalization.Get("DSONObject.DuplicatedKey", key, keyLineNum));
                            }
                            else
                            {
                                var obj = new DSONObject(current);
                                current.Add(key, obj);
                                current = obj;
                            }
                            key = null;
                        }
                        else if (c == '}')
                        {
                            if (key != null || stack.Count == 0) throw new InvalidDataException(Globalization.Get("DSONObject.IllegalChar", '}', lineNum));
                            current = stack.Pop();
                        }
                        else if (c != '/' && c != '\r') throw new InvalidDataException(Globalization.Get("DSONObject.IllegalChar", c, lineNum));
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        var str = sb.ToString();
                        sb.Clear();
                        if (key == null)
                        {
                            key = str;
                            keyLineNum = lineNum;
                        }
                        else
                        {
                            current.TryAdd(key, new DSONValue(str, current));
                            key = null;
                        }
                        flag = 0;
                    }
                    else if (c == '\r' || c == '\n') throw new InvalidDataException(Globalization.Get("DSONObject.IllegalColons", lineNum));
                    else sb.Append(c);
                }
                lastChar = c;
            }
            if (stack.Count > 0 || flag == 1 || key != null) throw new InvalidDataException(Globalization.Get("DSONObject.UnexpectedEnd"));
            return root;
        }

        #region implement ordered Dictionary

        private readonly List<string> _orderedKeys = [];
        private readonly Dictionary<string, IDSONItem> _keyValues = [];

        public IDSONItem this[string key] 
        { 
            get => _keyValues[key]; 
            set 
            {
                if (_keyValues.ContainsKey(key)) _keyValues[key] = value;
                else Add(key, value);
            } 
        }
        public ICollection<string> Keys => [.. _orderedKeys];
        public ICollection<IDSONItem> Values => _keyValues.Values;
        public int Count => _keyValues.Count;

        public void Add(string key, IDSONItem value)
        {
            _keyValues.Add(key, value);
            _orderedKeys.Add(key);
        }

        public bool ContainsKey(string key) => _keyValues.ContainsKey(key);

        public bool Remove(string key)
        {
            if (!_keyValues.Remove(key)) return false;
            _orderedKeys.Remove(key);
            return true;
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out IDSONItem value) => _keyValues.TryGetValue(key, out value);

        public void Clear()
        {
            _keyValues.Clear();
            _orderedKeys.Clear();
        }

        public bool TryAdd(string key, IDSONItem value)
        {
            if (!_keyValues.TryAdd(key, value)) return false;
            _orderedKeys.Add(key);
            return true;
        }

        public IEnumerator<KeyValuePair<string, IDSONItem>> GetEnumerator() => new Enumerator(_keyValues, _orderedKeys);

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public struct Enumerator : IEnumerator<KeyValuePair<string, IDSONItem>>
        {
            private KeyValuePair<string, IDSONItem> _current;
            private int _index;
            private readonly Dictionary<string, IDSONItem> _dict;
            private readonly List<string> _orderedKeys;

            public KeyValuePair<string, IDSONItem> Current => _current;

            object IEnumerator.Current => _current;

            internal Enumerator(Dictionary<string, IDSONItem> dict, List<string> orderedKeys)
            {
                _dict = dict;
                _orderedKeys = orderedKeys;
            }

            public void Dispose() {}

            public bool MoveNext()
            {
                if (_index < _orderedKeys.Count)
                {
                    var key = _orderedKeys[_index];
                    _current = new KeyValuePair<string, IDSONItem>(key, _dict[key]);
                    _index++;
                    return true;
                }
                _current = default;
                return false;
            }

            public void Reset()
            {
                _index = 0;
                _current = default;
            }
        }

        #endregion

        public DSONObject() : this(null) { }

        public override string Text { get => ToString(); set { } }

        public string? RootKey => _orderedKeys.Count == 1 ? _orderedKeys[0] : null;

        public DSONObject ExpandedObject => _orderedKeys.Count == 1 && _keyValues[_orderedKeys[0]] is DSONObject o ? o.ExpandedObject : this;

        public DSONObject? ModifiedValues
        {
            get
            {
                if (!Modified) return null;
                var clone = new DSONObject();
                foreach (var pair in this)
                {
                    var val = pair.Value;
                    if (!val.Modified) continue;
                    IDSONItem? item = null;
                    if (val is DSONObject a) item = a.ModifiedValues;
                    else if (val is DSONValue) item = val;
                    if (item != null) clone.Add(pair.Key, item);
                }
                return clone;
            }
        }

        public void UpdateValues(DSONObject newObj)
        {
            foreach (var pair in this)
            {
                if (newObj.TryGetValue(pair.Key, out var val))
                {
                    var v1 = pair.Value;
                    var v2 = val;
                    if (v1 is DSONObject a1 && v2 is DSONObject a2) a1.UpdateValues(a2);
                    else if (v1 is DSONValue && v2 is DSONValue) v1.Text = v2.Text;
                }
            }
        }

        public DSONObject? FindChanges(DSONObject rawObj)
        {
            DSONObject? clone = null;
            foreach (var pair in this)
            {
                if (rawObj.TryGetValue(pair.Key, out var v2))
                {
                    var v1 = pair.Value;
                    IDSONItem? item = null;
                    if (v1 is DSONObject a1 && v2 is DSONObject a2) item = a1.FindChanges(a2);
                    else if (v1 is DSONValue && v2 is DSONValue && !Equals(v1.Text, v2.Text)) item = v1;
                    if (item != null)
                    {
                        clone ??= [];
                        clone.Add(pair.Key, item);
                    }
                }
            }
            return clone;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            AppendString(this, sb, 0);
            return sb.ToString();
        }

        private static void AppendString(DSONObject obj, StringBuilder sb, int indent)
        {
            foreach (var key in obj._orderedKeys)
            {
                for (var i = 0; i < indent; i++) sb.Append('\t');
                sb.Append('"');
                sb.Append(key);
                sb.Append('"');
                var val = obj[key];
                if (val is DSONObject item)
                {
                    sb.AppendLine();
                    sb.AppendLine("{");
                    AppendString(item, sb, indent + 1);
                    sb.AppendLine("}");
                }
                else
                {
                    sb.Append('\t');
                    sb.Append('"');
                    sb.Append(val.ToString());
                    sb.AppendLine("\"");
                }
            }
        }
    }
}
