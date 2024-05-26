using Dota2Editor.Basic;
using Dota2Editor.Forms;
using System.Diagnostics;

namespace Dota2Editor
{
    public partial class Form1 : Form, IEditor
    {
        public Form1()
        {
            InitializeComponent();
            Initialization();
        }

        private const string RepoUrl = "https://github.com/ado-cs/Dota2Editor";
        private static readonly string[] GameTree = ["steam", "steamapps", "common", "dota 2 beta", "game"];
        private static readonly Dictionary<int, IEditor> EditorForms = [];

        public EditorPanel Panel => editorPanel1;

        private void Initialization()
        {
            if (File.Exists(Common.LocalConfig)) 
            {
                try
                {
                    var conf = DSONObject.Parse(File.ReadAllText(Common.LocalConfig));
                    var path = (conf["path"] as DSONValue)?.Text;
                    if (IsLegalGamePath(path)) Common.GameRoot = path;
                    var lang = (conf["lang"] as DSONValue)?.Text;
                    if (lang != null) Globalization.CurrentLang = lang;
                }
                catch { }
            }
            //fill menu language
            foreach (var key in Globalization.SupportedLanguages.Keys)
                toolStripMenuItemL.DropDownItems.Add(new ToolStripMenuItem(key));
            var len = Common.ViewNames.Length;
            for (var i = 0; i < len; i++) 
                toolStripMenuItemV.DropDownItems.Add(new ToolStripMenuItem());
            editorPanel1.NewEditorFormCreator = CreateNewEditorForm;
            ResetAllText();
            BindEvents();
        }

        private void ResetAllText()
        {
            var langs = Globalization.SupportedLanguages;
            var langName = Globalization.CurrentLang;
            foreach (ToolStripMenuItem item in toolStripMenuItemL.DropDownItems)
                item.Checked = item.Text != null && Equals(langName, langs[item.Text]);
            
            toolStripMenuItemG.Text = Globalization.Get("Form1.Menu.Game") + "(&G)";
            toolStripMenuItemG1.Text = Globalization.Get("Form1.Menu.Game.Write") + "(&S)";
            toolStripMenuItemG2.Text = Globalization.Get("Form1.Menu.Game.Recover") + "(&R)";
            toolStripMenuItemG3.Text = Globalization.Get("Form1.Menu.Game.Update") + "(&U)";
            toolStripMenuItemG4.Text = Globalization.Get("Form1.Menu.Game.Replace") + "(&M)";
            toolStripMenuItemG5.Text = Globalization.Get("Form1.Menu.Game.Setting") + "(&C)";
            toolStripMenuItemV.Text = Globalization.Get("Form1.Menu.View") + "(&V)";
            var i = 0;
            foreach (var name in Common.ViewNames)
                toolStripMenuItemV.DropDownItems[i++].Text = name;
            toolStripMenuItemH.Text = Globalization.Get("Form1.Menu.Help") + "(&H)";
            toolStripMenuItemH1.Text = Globalization.Get("Form1.Menu.Help.Documentation") + "(&D)";
            toolStripMenuItemH2.Text = Globalization.Get("Form1.Menu.Help.About") + "(&A)";
            editorPanel1.ResetAllText();
            foreach (var pair in EditorForms)
            {
                if (pair.Value is EditorForm form && !form.IsDisposed)
                {
                    form.Text = Common.GetViewName(pair.Key);
                    form.Panel.ResetAllText();
                }
            }
        }

        private void BindEvents()
        {
            toolStripMenuItemG1.Click += (_, _) => PatchGame();
            toolStripMenuItemG2.Click += (_, _) => RecoverGame();
            toolStripMenuItemG3.Click += (_, _) => UpdateLocalData(false);
            toolStripMenuItemG4.Click += (_, _) => UpdateLocalData(true);
            toolStripMenuItemG5.Click += (_, _) => FindGamePath();
            foreach (var item in toolStripMenuItemV.DropDownItems)
                if (item is ToolStripMenuItem it) it.Click += (sender, e) => ChangeView(sender as ToolStripMenuItem);
            foreach (ToolStripMenuItem item in toolStripMenuItemL.DropDownItems)
            {
                item.Click += (sender, _) =>
                {
                    if (sender is ToolStripMenuItem s && s.Text != null && !s.Checked)
                    {
                        var lang = Globalization.SupportedLanguages[s.Text];
                        Globalization.CurrentLang = lang;
                        ResetAllText();
                        TrySaveConfig("lang", lang);
                    }
                };
            }
            toolStripMenuItemG.DropDownOpening += (_, _) =>
            {
                var flag = IsLegalGamePath(Common.GameRoot);
                foreach (var item in toolStripMenuItemG.DropDownItems)
                {
                    if (item is ToolStripMenuItem it && item != toolStripMenuItemG5) it.Enabled = flag;
                }
            };
            toolStripMenuItemV.DropDownOpening += (_, _) =>
            {
                var flag = IsLegalGamePath(Common.GameRoot);
                foreach (var item in toolStripMenuItemV.DropDownItems)
                {
                    if (item is ToolStripMenuItem it) it.Enabled = flag;
                }
            };
            toolStripMenuItemH1.Click += (_, _) => Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = RepoUrl});
            toolStripMenuItemH2.Click += (_, _) => new AboutForm(RepoUrl).ShowDialog();
            FormClosing += (_, _) => StashChanges();
        }

        private void PatchGame()
        {
            if (Common.GameRoot == null) return;
            StashChanges();
            var outputDir = Path.Combine(Common.GameRoot, Common.OutputVpkDir);
            Directory.CreateDirectory(outputDir);
            var gameinfoPath = Path.Combine(Common.GameRoot, Common.TargetGameinfo);
            try
            {
                if (Gameinfo.Activate(gameinfoPath, out var data, Common.OutputVpkDir))
                {
                    Directory.CreateDirectory(Common.Local);
                    File.Move(gameinfoPath, Common.LocalGameinfo, true);
                    File.WriteAllBytes(gameinfoPath, data);
                }
                VpkParser.Save(Common.LocalStash, Path.Combine(outputDir, Common.OutputVpkName));
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

        private static void RecoverGame()
        {
            if (Common.GameRoot == null) return;
            try
            {
                var gameinfoPath = Path.Combine(Common.GameRoot, Common.TargetGameinfo);
                if (File.Exists(gameinfoPath) && !Gameinfo.IsActive(gameinfoPath, Common.OutputVpkDir))
                {
                    Directory.Delete(Path.Combine(Common.GameRoot, Common.OutputVpkDir), true);
                    MessageBox.Show(Globalization.Get("Form1.SuccessInRecovery"));
                    return;
                }
                if (!File.Exists(Common.LocalGameinfo))
                {
                    MessageBox.Show(Globalization.Get("Form1.FailedInRecovery2"));
                    return;
                }
                File.Copy(Common.LocalGameinfo, gameinfoPath, true);
                Directory.Delete(Path.Combine(Common.GameRoot, Common.OutputVpkDir), true);
                MessageBox.Show(Globalization.Get("Form1.SuccessInRecovery"));
            }
            catch (IOException)
            {
                MessageBox.Show(Globalization.Get("Form1.FailedInRecovery1"));
            }
        }

        private void UpdateLocalData(bool replace)
        {
            if (!replace)
            {
                string workingFile = string.Empty;
                var tmpDir = Path.Combine(Common.Local, "tmp");
                if (!Common.ReadGameData(tmpDir)) return;
                try
                {
                    var queue = new Queue<string>();
                    queue.Enqueue(tmpDir);
                    while (queue.Count > 0)
                    {
                        var dir = queue.Dequeue();
                        foreach (var folder in Directory.GetDirectories(dir)) queue.Enqueue(folder);
                        foreach (var newPath in Directory.GetFiles(dir))
                        {
                            var relPath = Path.GetRelativePath(tmpDir, newPath);
                            var gamePath = Path.Combine(Common.LocalGame, relPath);
                            var stashPath = Path.Combine(Common.LocalStash, relPath);
                            if (File.Exists(gamePath) && File.Exists(stashPath))
                            {
                                var newObj = DSONObject.Parse(File.ReadAllText(workingFile = newPath));
                                var rawObj = DSONObject.Parse(File.ReadAllText(workingFile = gamePath));
                                var modObj = DSONObject.Parse(File.ReadAllText(workingFile = stashPath));
                                var changes = modObj.FindChanges(rawObj);
                                if (changes != null) newObj.UpdateValues(changes);
                                File.WriteAllText(stashPath, newObj.ToString());
                            }
                            else
                            {
                                Common.CreateDirectory(gamePath);
                                Common.CreateDirectory(stashPath);
                                File.Copy(newPath, stashPath, true);
                            }
                            File.Move(newPath, gamePath, true);
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
            else if (Common.ReadGameData(Common.LocalGame) && Common.ReadGameData(Common.LocalStash)) 
                MessageBox.Show(Globalization.Get("Form1.SuccessInReplace"));
            if (!editorPanel1.ReloadView())
            {
                foreach (var item in toolStripMenuItemV.DropDownItems)
                {
                    if (item is ToolStripMenuItem it) it.Checked = false;
                }
            }
            foreach (var v in EditorForms.Values)
            {
                if (v is EditorForm form && !form.IsDisposed) form.Panel.ReloadView();
            }
        }

        private static void FindGamePath()
        {
            var diag = new FolderBrowserDialog();
            if (Common.GameRoot != null && Directory.Exists(Common.GameRoot)) diag.SelectedPath = Common.GameRoot;
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
                if (IsLegalGamePath(path)) TrySaveConfig("path", Common.GameRoot = path);
                else MessageBox.Show(Globalization.Get("Form1.IllegalGamePath"));
            }
        }

        private void ChangeView(ToolStripMenuItem? item)
        {
            if (item == null || item.Checked) return;
            //todo activate if the editorform is opened
            var index = toolStripMenuItemV.DropDownItems.IndexOf(item);
            if (index == -1) return;
            if (EditorForms.TryGetValue(index, out var f) && f is EditorForm form && !form.IsDisposed)
            {
                if (form.WindowState == FormWindowState.Minimized) form.WindowState = FormWindowState.Normal;
                form.Activate();
                form.Focus();
            }
            else
            {
                editorPanel1.StashChanges();
                if (editorPanel1.ResetView(index))
                {
                    for (var i = 0; i < toolStripMenuItemV.DropDownItems.Count; i++)
                    {
                        if (toolStripMenuItemV.DropDownItems[i] is ToolStripMenuItem it) it.Checked = index == i;
                    }
                }
            }
        }

        private void CreateNewEditorForm()
        {
            ToolStripMenuItem? item = null;
            var index = -1;
            for (var i = 0; i < toolStripMenuItemV.DropDownItems.Count; i++)
            {
                if (toolStripMenuItemV.DropDownItems[i] is ToolStripMenuItem it && it.Checked) 
                { 
                    index = i;
                    item = it;
                    break; 
                }
            }
            if (item == null) return;
            editorPanel1.StashChanges();
            editorPanel1.ClearView();
            item.Checked = false;
            var editor = new EditorForm(index);
            if (!EditorForms.TryAdd(index, editor)) EditorForms[index] = editor;
            editor.Show();
        }

        private void StashChanges()
        {
            editorPanel1.StashChanges();
            foreach (var v in EditorForms.Values)
            {
                if (v is EditorForm form && !form.IsDisposed) form.Panel.StashChanges();
            }
        }

        private static bool IsLegalGamePath(string? path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return File.Exists(Path.Combine(path, Common.TargetGameinfo)) && File.Exists(Path.Combine(path, Common.TargetGameVpk));
        }

        private static void TrySaveConfig(string key, string value)
        {
            DSONObject obj;
            try
            {
                obj = DSONObject.Parse(File.ReadAllText(Common.LocalConfig));
                obj[key] = new DSONValue(value);
            }
            catch
            {
                obj = new DSONObject { { key, new DSONValue(value) } };
            }
            Directory.CreateDirectory(Common.Local);
            File.WriteAllText(Common.LocalConfig, obj.ToString());
        }
    }
}
