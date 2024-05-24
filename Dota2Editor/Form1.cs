using Dota2Editor.Basic;
using Dota2Editor.Forms;
using System.Diagnostics;

namespace Dota2Editor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Initialization();
        }

        private static readonly string RepoUrl = "https://www.github.com";

        private static readonly string Local = Path.Combine(Environment.CurrentDirectory, "data");
        private static readonly string LocalGame = Path.Combine(Local, "game");
        private static readonly string LocalStash = Path.Combine(Local, "stash");
        private static readonly string LocalRecord = Path.Combine(Local, "records");
        private static readonly string LocalConfig = Path.Combine(Local, "conf.txt");
        private static readonly string LocalGameinfo = Path.Combine(Local, "gameinfo.gi");

        private static readonly string TargetGameinfo = "dota\\gameinfo.gi";
        private static readonly string GameVpk = "dota\\pak01_dir.vpk";
        private static readonly string OutputVpkDir = "0test";
        private static readonly string OutputVpkName = "pak01_dir.vpk";
        private static readonly string NpcPath = "scripts\\npc";
        private static readonly string HeroPath = "scripts\\npc\\heroes";
        private static readonly string Ext = "txt";
        private static readonly string[] DataFiles = ["items", "neutral_items", "npc_units", "npc_abilities", "npc_heroes"];
        private static readonly string[] GameTree = ["steam", "steamapps", "common", "dota 2 beta", "game"];

        private string? _gamePath;

        private void Initialization()
        {
            if (File.Exists(LocalConfig)) 
            {
                try
                {
                    var conf = DSONObject.Parse(File.ReadAllText(LocalConfig));
                    var path = (conf["path"] as DSONValue)?.Text;
                    if (IsLegalGamePath(path)) _gamePath = path;
                }
                catch { }
            }
            //fill menu language
            foreach (var key in Globalization.SupportedLanguages.Keys)
            {
                toolStripMenuItemL.DropDownItems.Add(new ToolStripMenuItem(key));
            }
            ResetAllText(Globalization.CurrentLang);
            BindEvents();
        }

        private void ResetAllText(string langName)
        {
            var langs = Globalization.SupportedLanguages;
            foreach (ToolStripMenuItem item in toolStripMenuItemL.DropDownItems)
            {
                item.Checked = item.Text != null && Equals(langName, langs[item.Text]);
            }
            Globalization.CurrentLang = langName;
            toolStripMenuItemG.Text = Globalization.Get("Form1.Menu.Game") + "(&G)";
            toolStripMenuItemG1.Text = Globalization.Get("Form1.Menu.Game.Write") + "(&S)";
            toolStripMenuItemG2.Text = Globalization.Get("Form1.Menu.Game.Recover") + "(&R)";
            toolStripMenuItemG3.Text = Globalization.Get("Form1.Menu.Game.Update") + "(&U)";
            toolStripMenuItemG4.Text = Globalization.Get("Form1.Menu.Game.Replace") + "(&M)";
            toolStripMenuItemG5.Text = Globalization.Get("Form1.Menu.Game.Setting") + "(&C)";
            toolStripMenuItemV.Text = Globalization.Get("Form1.Menu.View") + "(&V)";
            toolStripMenuItemV1.Text = Globalization.Get("Form1.Menu.View.Items") + "(&I)";
            toolStripMenuItemV2.Text = Globalization.Get("Form1.Menu.View.NeutralItems") + "(&N)";
            toolStripMenuItemV3.Text = Globalization.Get("Form1.Menu.View.Units") + "(&U)";
            toolStripMenuItemV4.Text = Globalization.Get("Form1.Menu.View.Abilities") + "(&A)";
            toolStripMenuItemV5.Text = Globalization.Get("Form1.Menu.View.Heros") + "(&H)";
            toolStripMenuItemV6.Text = Globalization.Get("Form1.Menu.View.HeroAbilities") + "(&S)";
            toolStripMenuItemH.Text = Globalization.Get("Form1.Menu.Help") + "(&H)";
            toolStripMenuItemH1.Text = Globalization.Get("Form1.Menu.Help.Documentation") + "(&D)";
            toolStripMenuItemH2.Text = Globalization.Get("Form1.Menu.Help.About") + "(&A)";
            toolStripMenuItem1.Text = Globalization.Get("Form1.Menu.ListBox.Add") + "(&A)";
            toolStripMenuItem2.Text = Globalization.Get("Form1.Menu.ListBox.Load") + "(&L)";
            toolStripMenuItem3.Text = Globalization.Get("Form1.Menu.ListBox.Rename") + "(&R)";
            toolStripMenuItem4.Text = Globalization.Get("Form1.Menu.ListBox.Delete") + "(&D)";
        }

        #region Events

        private void BindEvents()
        {
            searchingBox1.SelectionHandler = ShowItem;
            toolStripMenuItemG1.Click += (_, _) => WriteToGameData();
            toolStripMenuItemG2.Click += (_, _) => RecoverGameData();
            toolStripMenuItemG3.Click += (_, _) => UpdateLocalData(false);
            toolStripMenuItemG4.Click += (_, _) => UpdateLocalData(true);
            toolStripMenuItemG5.Click += (_, _) => FindGamePath();
            foreach (var item in toolStripMenuItemV.DropDownItems)
            {
                if (item is ToolStripMenuItem it) it.Click += (sender, e) => ChangeView(sender as ToolStripMenuItem);
            }
            foreach (ToolStripMenuItem item in toolStripMenuItemL.DropDownItems)
            {
                item.Click += (sender, _) =>
                {
                    if (sender is ToolStripMenuItem s && s.Text != null && !s.Checked)
                        ResetAllText(Globalization.SupportedLanguages[s.Text]);
                };
            }

            toolStripMenuItemG.DropDownOpening += (_, _) =>
            {
                var flag = IsLegalGamePath(_gamePath);
                foreach (var item in toolStripMenuItemG.DropDownItems)
                {
                    if (item is ToolStripMenuItem it && item != toolStripMenuItemG5) it.Enabled = flag;
                }
            };
            toolStripMenuItemV.DropDownOpening += (_, _) =>
            {
                var flag = IsLegalGamePath(_gamePath);
                foreach (var item in toolStripMenuItemV.DropDownItems)
                {
                    if (item is ToolStripMenuItem it) it.Enabled = flag;
                }
            };
            toolStripMenuItemH1.Click += (_, _) => Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = RepoUrl});
            toolStripMenuItemH2.Click += (_, _) => new AboutForm(RepoUrl).ShowDialog();
            contextMenuStrip1.Opening += (_, _) =>
            {
                toolStripMenuItem1.Enabled = IsEditing;
                toolStripMenuItem2.Enabled = toolStripMenuItem3.Enabled = listBox1.SelectedItems.Count == 1;
                toolStripMenuItem4.Enabled = listBox1.SelectedItems.Count > 0;
            };
            toolStripMenuItem1.Click += (_, _) =>
            {
                while (true)
                {
                    var diag = new InputForm(Text, Globalization.Get("Form1.NewRecord"), string.Empty);
                    if (diag.ShowDialog() == DialogResult.OK)
                    {
                        var f = AddLocalRecord(diag.Result);
                        if (f == 0) continue;
                        if (f == 1) listBox1.Items.Add(diag.Result);
                    }
                    return;
                }
            };
            toolStripMenuItem2.Click += (_, _) =>
            {
                if (listBox1.SelectedItem is string item) LoadLocalRecord(item);
            };
            toolStripMenuItem3.Click += (_, _) =>
            {
                if (listBox1.SelectedItem is string item)
                {
                    var index = listBox1.SelectedIndex;
                    while (true)
                    {
                        var diag = new InputForm(Text, Globalization.Get("Form1.RenameRecord"), item);
                        if (diag.ShowDialog() == DialogResult.OK)
                        {
                            var f = RenameLocalRecord(item, diag.Result);
                            if (f == 0) continue;
                            if (f == 1) listBox1.Items[index] = diag.Result;
                        }
                        return;
                    }
                }
                
            };
            toolStripMenuItem4.Click += (_, _) =>
            {
                var list = new List<string>();
                foreach (var item in listBox1.SelectedItems)
                {
                    if (item is string s) list.Add(s);
                }
                DeleteLocalRecord(list);
                foreach (var item in list) listBox1.Items.Remove(item);
            };
            button1.Click += (_, _) => new BatchModificationForm(BatchModify).ShowDialog();
            FormClosing += (_, _) => StashChanges();
        }

        private void WriteToGameData()
        {
            StashChanges();
            if (_gamePath == null) return;
            var outputDir = Path.Combine(_gamePath, OutputVpkDir);
            Directory.CreateDirectory(outputDir);
            var gameinfoPath = Path.Combine(_gamePath, TargetGameinfo);
            try
            {
                if (Gameinfo.Activate(gameinfoPath, out var data, OutputVpkDir))
                {
                    Directory.CreateDirectory(Local);
                    File.Move(gameinfoPath, LocalGameinfo, true);
                    File.WriteAllBytes(gameinfoPath, data);
                }
                VpkParser.Save(LocalStash, Path.Combine(outputDir, OutputVpkName));
                MessageBox.Show(Globalization.Get("Form1.SuccessInModification"));
            }
            catch (IOException)
            {
                MessageBox.Show(Globalization.Get("Form1.FailedInModification1"));
            }
            catch (Exception e)
            {
                MessageBox.Show(Globalization.Get("Form1.FailedInModification2", e.Message));
            }
        }

        private void RecoverGameData()
        {
            if (_gamePath == null) return;
            if (!File.Exists(LocalGameinfo))
            {
                MessageBox.Show(Globalization.Get("Form1.FailedInRecovery2"));
                return;
            }
            var targetPath = Path.Combine(_gamePath, TargetGameinfo);
            if (File.Exists(targetPath) && !Gameinfo.IsActive(targetPath, OutputVpkDir))
            {
                Directory.Delete(Path.Combine(_gamePath, OutputVpkDir), true);
                MessageBox.Show(Globalization.Get("Form1.SuccessInRecovery"));
                return;
            }
            try
            {
                File.Move(LocalGameinfo, targetPath, true);
                Directory.Delete(Path.Combine(_gamePath, OutputVpkDir), true);
                MessageBox.Show(Globalization.Get("Form1.SuccessInRecovery"));
            }
            catch (IOException)
            {
                MessageBox.Show(Globalization.Get("Form1.FailedInRecovery1"));
            }
        }

        private void UpdateLocalData(bool replace)
        {
            if (_gamePath == null) return;

            var state = GetCurrentState();
            if (!replace)
            {
                string workingFile = string.Empty;
                var tmpDir = Path.Combine(Local, "tmp");
                if (!ReadGameData(_gamePath, tmpDir)) return;
                try
                {
                    var queue = new Queue<string>();
                    queue.Enqueue(tmpDir);
                    while (queue.Count > 0)
                    {
                        var dir = queue.Dequeue();
                        foreach (var d in Directory.GetDirectories(dir)) queue.Enqueue(d);
                        foreach (var f in Directory.GetFiles(dir))
                        {
                            var relPath = Path.GetRelativePath(tmpDir, f);
                            var gamePath = Path.Combine(LocalGame, relPath);
                            var stashPath = Path.Combine(LocalStash, relPath);
                            CreateDirectory(gamePath);
                            CreateDirectory(stashPath);
                            if (File.Exists(gamePath) && File.Exists(stashPath))
                            {
                                var newObj = DSONObject.Parse(File.ReadAllText(workingFile = f));
                                var oriObj = DSONObject.Parse(File.ReadAllText(workingFile = gamePath));
                                var modObj = DSONObject.Parse(File.ReadAllText(workingFile = stashPath));
                                modObj.FindModifiedValues(oriObj);
                                newObj.UpdateWithModifiedValues(modObj);
                                File.WriteAllText(stashPath, newObj.ToString());
                            }
                            else File.Copy(f, stashPath, true);
                            File.Move(f, gamePath, true);
                        }
                    }
                    Directory.Delete(tmpDir, true);
                    MessageBox.Show(Globalization.Get("Form1.SuccessInUpdating"));
                }
                catch (Exception e)
                {
                    MessageBox.Show(Globalization.Get("Form1.FailedInParsingDSON", workingFile, e.Message));
                    Directory.Delete(tmpDir, true);
                    return;
                }
            }
            else if (ReadGameData(_gamePath, LocalGame) && ReadGameData(_gamePath, LocalStash)) 
                MessageBox.Show(Globalization.Get("Form1.SuccessInReplace"));
            if (!ReloadView(state))
            {
                splitContainer1.Visible = false;
                foreach (var item in toolStripMenuItemV.DropDownItems)
                {
                    if (item is ToolStripMenuItem it) it.Checked = false;
                }
            }
        }

        private void FindGamePath()
        {
            var diag = new FolderBrowserDialog();
            if (_gamePath != null && Directory.Exists(_gamePath)) diag.SelectedPath = _gamePath;
            if (diag.ShowDialog() == DialogResult.OK)
            {
                var path = diag.SelectedPath;
                if (string.IsNullOrEmpty(path)) return;
                path = path.TrimEnd('\\');
                var lowerPath = path.ToLower();
                for (var i = 0; i < GameTree.Length - 1; i++)
                {
                    if (lowerPath.EndsWith(GameTree[i]))
                    {
                        for (var j = i + 1; j < GameTree.Length; j++) path += '\\' + GameTree[j];
                        break;
                    }
                }
                if (IsLegalGamePath(path))
                {
                    _gamePath = path;
                    DSONObject conf;
                    try
                    {
                        conf = DSONObject.Parse(File.ReadAllText(LocalConfig));
                        conf["path"] = new DSONValue(path);
                    }
                    catch
                    {
                        conf = new DSONObject
                        {
                            { "path", new DSONValue(path) }
                        };
                    }
                    Directory.CreateDirectory(Local);
                    File.WriteAllText(LocalConfig, conf.ToString());
                }
                else MessageBox.Show(Globalization.Get("Form1.IllegalGamePath"));
            }
        }

        private void ChangeView(ToolStripMenuItem? item)
        {
            if (item == null || item.Checked) return;
            var index = toolStripMenuItemV.DropDownItems.IndexOf(item);
            if (index == -1) return;
            StashChanges();
            var relPath = index < DataFiles.Length ? $"{NpcPath}\\{DataFiles[index]}.{Ext}" : HeroPath;
            if (ResetView(relPath, index >= DataFiles.Length))
            {
                splitContainer1.Visible = true;
                for (var i = 0; i < toolStripMenuItemV.DropDownItems.Count; i++)
                {
                    if (toolStripMenuItemV.DropDownItems[i] is ToolStripMenuItem it) it.Checked = index == i;
                }
            }
        }

        #endregion

        #region Common

        private static void CreateDirectory(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        }

        private static bool IsLegalGamePath(string? path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return File.Exists(Path.Combine(path, TargetGameinfo)) && File.Exists(Path.Combine(path, GameVpk));
        }

        private static bool ReadGameData(string? gamePath, string saveDir)
        {
            if (gamePath == null) { MessageBox.Show(Globalization.Get("Form1.GamePathMissing")); return false; }
            var vpkPath = Path.Combine(gamePath, GameVpk);
            if (!File.Exists(vpkPath)) { MessageBox.Show(Globalization.Get("Form1.GamePathMissing")); return false; }
            try
            {
                var relPathNpc = NpcPath.Replace('\\', '/');
                var relPathHero = HeroPath.Replace('\\', '/');
                VpkParser.Extract(vpkPath, saveDir, (path, name, ext) => string.Equals(ext, Ext, StringComparison.OrdinalIgnoreCase) &&
                    (string.Equals(path, relPathHero, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(path, relPathNpc, StringComparison.OrdinalIgnoreCase) && DataFiles.Contains(name.ToLower())));
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(Globalization.Get("Form1.FailedInParsing", vpkPath, e.Message));
                return false;
            }
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

        private static List<string> GetKeys(DSONObject obj)
        {
            var keys = new List<string>();
            foreach (var key in obj.Keys)
            {
                if (obj[key] is DSONObject) keys.Add(key);
            }
            return keys;
        }

        #endregion

        #region Editor

        private bool _isDir;
        private string? _relPath;
        private DSONObject? _main;
        private DSONObject? _data;
        private bool _changed = false;
        private string? _currentItem;
        private readonly HashSet<string> _modifiedItems = [];

        private bool IsEditing => _relPath != null && _main != null;

        private void ShowItem(string item)
        {
            if (_data != null && _data.TryGetValue(item, out var v) && v is DSONObject o)
            {
                Debug.WriteLine(item);

                _currentItem = item;
                flowLayoutPanel1.SuspendLayout();
                flowLayoutPanel1.Controls.Clear();

                AddObject(o.Decomposition(), _isDir);
                flowLayoutPanel1.Controls.Add(new Label() { Text = string.Empty, AutoSize = true, Margin = new Padding(0, 12, 0, 0) });

                flowLayoutPanel1.ResumeLayout(false);
                flowLayoutPanel1.PerformLayout();
            }
        }

        private void AddObject(DSONObject obj, bool highlight, int indent = 3)
        {
            var first = true;
            List<TextBox> list = [];
            var left = 0;
            foreach (var key in obj.Keys)
            {
                var val = obj[key];
                var label = new Label
                {
                    Margin = new Padding(indent, first ? 6 : 12, 6, 0),
                    Text = key,
                    AutoSize = true
                };
                if (highlight) label.ForeColor = Color.OrangeRed;
                flowLayoutPanel1.Controls.Add(label);
                if (val is DSONObject o)
                {
                    flowLayoutPanel1.SetFlowBreak(label, true);
                    AddObject(o, false, indent + 40);
                }
                else if (val is DSONValue v)
                {
                    var tb = new TextBox
                    {
                        Margin = new Padding(0, first ? 3 : 9, 3, 3),
                        Size = new Size(197, 23),
                        Text = val.ToString()
                    };
                    tb.TextChanged += (_, _) =>
                    {
                        _changed = true;
                        if (_currentItem != null) _modifiedItems.Add(_currentItem);
                        v.Text = tb.Text ?? string.Empty;
                    };
                    flowLayoutPanel1.Controls.Add(tb);
                    flowLayoutPanel1.SetFlowBreak(tb, true);
                    list.Add(tb);
                    if (tb.Left > left) left = tb.Left;
                }
                first = false;
            }
            /** Alignment
             * 
            foreach (var item in list)
            {
                var gap = left - item.Left;
                if (gap > 0) item.Margin = new Padding(gap, 3, 3, 3);
            }
             */
        }

        private DSONObject? GetCurrentState() => _changed ? _main?.ExtractChangedValues() : null;

        public bool ResetView(string relativePath, bool isDir)
        {
            _isDir = isDir;
            DSONObject? root = ReadData(relativePath);
            if (root == null) return false;
            _relPath = relativePath;
            _main = root;
            _data = root.Decomposition();
            _currentItem = null;
            _changed = false;
            _modifiedItems.Clear();
            flowLayoutPanel1.Controls.Clear();
            searchingBox1.Items = GetKeys(_data);
            //load records
            var recPath = Path.Combine(LocalRecord, relativePath);
            listBox1.Items.Clear();
            if (Directory.Exists(recPath))
            {
                var ext = "." + Ext;
                foreach (var file in Directory.GetFiles(recPath))
                {
                    if (ext.Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
                        listBox1.Items.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            return true;
        }

        private bool ReloadView(DSONObject? state = null)
        {
            if (_relPath != null)
            {
                DSONObject? root = ReadData(_relPath);
                if (root != null)
                {
                    _main = root;
                    if (state != null) _main.UpdateValues(state);
                    _data = root.Decomposition();
                    searchingBox1.Items = GetKeys(_data);
                    if (_currentItem != null) searchingBox1.SelectedItem = _currentItem;
                    return true;
                }
            }
            _relPath = null;
            _main = null;
            _data = null;
            _currentItem = null;
            _changed = false;
            _modifiedItems.Clear();
            listBox1.Items.Clear();
            searchingBox1.Items = [];
            flowLayoutPanel1.Controls.Clear();
            return false;
        }

        private DSONObject? ReadData(string relativePath, bool fromStash = true)
        {
            var path = Path.Combine(fromStash ? LocalStash : LocalGame, relativePath);
            return _isDir ? ReadFromFolder(path, relativePath, fromStash) : ReadFromFile(path, relativePath, fromStash);
        }

        private DSONObject? ReadFromFile(string path, string relativePath, bool fromStash)
        {
            if (!File.Exists(path))
            {
                if (!ReadGameData(_gamePath, LocalGame)) return null;
                var tmpPath = Path.Combine(LocalGame, relativePath);
                if (!File.Exists(tmpPath))
                {
                    MessageBox.Show(Globalization.Get("Form1.FileMissing", tmpPath));
                    return null;
                }
                if (fromStash)
                {
                    CreateDirectory(path);
                    File.Copy(tmpPath, path);
                }
            }
            try
            {
                return DSONObject.Parse(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                MessageBox.Show(Globalization.Get("Form1.FailedInParsing", path, e.Message));
                return null;
            }
        }

        private DSONObject? ReadFromFolder(string path, string relativePath, bool fromStash)
        {
            if (!Directory.Exists(path))
            {
                if (!ReadGameData(_gamePath, LocalGame)) return null;
                var tmpPath = Path.Combine(LocalGame, relativePath);
                if (!Directory.Exists(tmpPath))
                {
                    MessageBox.Show(Globalization.Get("Form1.DirectoryMissing", tmpPath));
                    return null;
                }
                if (fromStash)
                {
                    Directory.CreateDirectory(path);
                    foreach (var file in Directory.GetFiles(tmpPath))
                        File.Copy(file, Path.Combine(path, Path.GetFileName(file)));
                }
            }
            var root = new DSONObject();
            foreach (var file in Directory.GetFiles(path))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                try
                {
                    var data = DSONObject.Parse(File.ReadAllText(file));
                    root.Add(name, data);
                }
                catch (Exception e)
                {
                    MessageBox.Show(Globalization.Get("Form1.FailedInParsing", file, e.Message));
                    return null;
                }
            }
            return root;
        }

        private int BatchModify(string key, string value, double val, BatchModificationForm.Operator flag)
        {
            if (_data == null) return 0;
            var num = 0;
            foreach (var k in _data.Keys)
            {
                if (_data[k] is DSONObject o && ModifyObject(o, key, value, val, flag, ref num)) _modifiedItems.Add(k);
            }
            if (num > 0) _changed = true;
            return num;
        }

        private static bool ModifyObject(DSONObject obj, string key, string value, double val, BatchModificationForm.Operator flag, ref int num)
        {
            var modified = false;
            foreach (var pair in obj)
            {
                if (pair.Value is DSONObject o)
                {
                    if (ModifyObject(o, key, value, val, flag, ref num)) modified = true;
                }
                else if (MatchPattern(pair.Key, key) && pair.Value is DSONValue v)
                {
                    if (flag == BatchModificationForm.Operator.Equals) v.Text = value;
                    else
                    {
                        if (v.Text.Contains(' '))
                        {
                            var t = true;
                            var s = v.Text.Split(' ');
                            for (var i = 0; i < s.Length; i++)
                            {
                                if (TryModifyValue(s[i], val, flag, out var result))
                                {
                                    s[i] = result;
                                    t = false;
                                }
                            }
                            if (t) continue;
                            v.Text = string.Join(' ', s);
                        }
                        else if (TryModifyValue(v.Text, val, flag, out var result)) v.Text = result;
                        else continue;
                    }
                    num++;
                    modified = true;
                }
            }
            return modified;
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
            if (_main == null || _relPath == null) return -1;
            if (IsIllegalName(name)) return 0;
            Directory.CreateDirectory(Path.Combine(LocalRecord, _relPath));
            var path = Path.Combine(LocalRecord, _relPath, $"{name}.{Ext}");
            if (File.Exists(path))
            {
                MessageBox.Show(Globalization.Get("Form1.DeplicatedRecord", name));
                return 0;
            }
            DSONObject? root = ReadData(_relPath, false);
            if (root == null) return -1;
            var obj = _main.ExtractChangedValuesFrom(root);
            if (obj == null)
            {
                MessageBox.Show(Globalization.Get("Form1.NoChanges"));
                return -1;
            }
            File.WriteAllText(path, obj.ToString());
            return 1;
        }

        private void LoadLocalRecord(string name)
        {
            if (_main == null || _relPath == null) return;
            var path = Path.Combine(LocalRecord, _relPath, $"{name}.{Ext}");
            if (!File.Exists(path))
            {
                MessageBox.Show(Globalization.Get("Form1.RecordMissing", name));
                return;
            }
            DSONObject? root = ReadData(_relPath, false);
            if (root == null) return;
            try
            {
                var obj = DSONObject.Parse(File.ReadAllText(path));
                root.UpdateValues(obj);
                _main = root;
                _data = root.Decomposition();
                searchingBox1.Items = GetKeys(_data);
                if (_currentItem != null) searchingBox1.SelectedItem = _currentItem;
                _changed = true;
                _modifiedItems.Clear();
                path = Path.Combine(LocalStash, _relPath);
                if (Directory.Exists(path))
                {
                    foreach (var key in root.Keys) _modifiedItems.Add(key);
                }
                MessageBox.Show(Globalization.Get("Form1.SuccessInLoadingRecord"));
            }
            catch (Exception e)
            {
                MessageBox.Show(Globalization.Get("Form1.FailedInParsing", path, e.Message));
            }
        }

        private int RenameLocalRecord(string name, string newName)
        {
            if (_relPath == null || string.IsNullOrEmpty(name)) return -1;
            if (IsIllegalName(name)) return 0;
            if (Equals(name, newName)) return 1;
            var path = Path.Combine(LocalRecord, _relPath, $"{name}.{Ext}");
            if (!File.Exists(path))
            {
                MessageBox.Show(Globalization.Get("Form1.RecordMissing", name));
                return -1;
            }
            var newPath = Path.Combine(LocalRecord, _relPath, $"{newName}.{Ext}");
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
            if (_relPath == null) return;
            var recPath = Path.Combine(LocalRecord, _relPath);
            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name)) continue;
                var path = Path.Combine(recPath, $"{name}.{Ext}");
                if (File.Exists(path)) File.Delete(path);
            }
        }

        private void StashChanges()
        {
            if (!_changed || _relPath == null || _main == null) return;
            var path = Path.Combine(LocalStash, _relPath);
            if (_isDir)
            {
                foreach (var item in _modifiedItems)
                {
                    if (_main.TryGetValue(item, out var data) && data is DSONObject obj)
                        File.WriteAllText(Path.Combine(path, $"{item}.{Ext}"), obj.ToString());
                }
            }
            else File.WriteAllText(path, _main.ToString());
        }

        #endregion
    }
}
