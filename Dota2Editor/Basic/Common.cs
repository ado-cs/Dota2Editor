using System.Diagnostics;

namespace Dota2Editor.Basic
{
    public static class Common
    {
        public static readonly string Local = Path.Combine(Environment.CurrentDirectory, "data");
        public static readonly string LocalGame = Path.Combine(Local, "game");
        public static readonly string LocalStash = Path.Combine(Local, "stash");
        public static readonly string LocalRecord = Path.Combine(Local, "records");
        public static readonly string LocalConfig = Path.Combine(Local, "conf.txt");
        public static readonly string LocalGameinfo = Path.Combine(Local, "gameinfo.gi");

        public const string TargetGameinfo = "dota\\gameinfo.gi";
        public const string TargetGameVpk = "dota\\pak01_dir.vpk";
        public const string OutputVpkDir = "0test";
        public const string OutputVpkName = "pak01_dir.vpk";
        public const string Ext = "txt";

        private const string PathNPC = "scripts\\npc";
        private static readonly View[] Views = [new("Items", "items"), new("NeutralItems", "neutral_items"), new("Units", "npc_units"),
            new("Abilities", "npc_abilities"), new("Heros", "npc_heroes"), new("HeroAbilities", "heroes", false)];

        public static string? GameRoot { get; set; }

        public static string[] ViewNames
        {
            get
            {
                var names = new string[Views.Length];
                for (var i = 0; i < Views.Length; i++) names[i] = Globalization.Get("Form1.Menu.View." + Views[i].Name);
                return names;
            }
        }

        public static bool IsFolder(int index)
        {
            if (index < 0 || index >= Views.Length) return false;
            return !Views[index].IsFile;
        }

        public static TreeNode? GetAllChanges()
        {
            TreeNode? tree = null;
            for (var i = 0; i < Views.Length; i++)
            {
                var game = ReadData(i, false);
                var stash = ReadData(i, true);
                if (game == null || stash == null) continue;
                var node = stash.BuildTreeByChanges(game);
                if (node == null) continue;
                tree ??= new TreeNode();
                node.Text = Globalization.Get("Form1.Menu.View." + Views[i].Name);
                tree.Nodes.Add(node);
            }
            return tree;
        }

        public static string GetViewName(int index)
        {
            if (index < 0 || index >= Views.Length) return string.Empty;
            return Globalization.Get("Form1.Menu.View." + Views[index].Name);
        }

        public static string GetRecordPath(int index)
        {
            if (index < 0 || index >= Views.Length) return LocalRecord;
            return Path.Combine(LocalRecord, Views[index].Filename);
        }

        public static DSONObject? ReadData(int index, bool fromStash = true)
        {
            if (index < 0 || index >= Views.Length) return null;
            var view = Views[index];
            var relPath = $"{PathNPC}\\{view.Filename}";
            return view.IsFile ? ReadFromFile(relPath + $".{Ext}", fromStash) : ReadFromFolder(relPath, fromStash);
        }

        private static DSONObject? ReadFromFile(string relativePath, bool fromStash)
        {
            var path = Path.Combine(fromStash ? LocalStash : LocalGame, relativePath);
            if (!File.Exists(path))
            {
                if (!ReadGameData(LocalGame)) return null;
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

        private static DSONObject? ReadFromFolder(string relativePath, bool fromStash)
        {
            var path = Path.Combine(fromStash ? LocalStash : LocalGame, relativePath);
            if (!Directory.Exists(path))
            {
                if (!ReadGameData(LocalGame)) return null;
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
                    var data = DSONObject.Parse(File.ReadAllText(file), root);
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

        public static bool ReadGameData(string saveDir)
        {
            if (GameRoot == null) { MessageBox.Show(Globalization.Get("Form1.GamePathMissing")); return false; }
            var vpkPath = Path.Combine(GameRoot, TargetGameVpk);
            if (!File.Exists(vpkPath)) { MessageBox.Show(Globalization.Get("Form1.GamePathMissing")); return false; }
            var filenames = new HashSet<string>();
            var directories = new List<string>();
            foreach (var view in Views) 
            {
                if (view.IsFile) filenames.Add(view.Filename);
                else directories.Add($"{PathNPC.Replace('\\', '/')}/{view.Filename}");
            }
            try
            {
                var relPathNPC = PathNPC.Replace('\\', '/');
                VpkParser.Extract(vpkPath, saveDir, (path, name, ext) =>
                {
                    if (string.Equals(ext, Ext, StringComparison.OrdinalIgnoreCase)) 
                    {
                        if (string.Equals(path, relPathNPC, StringComparison.OrdinalIgnoreCase)) return filenames.Contains(name.ToLower());
                        foreach (var dir in directories)
                        {
                            if (string.Equals(path, dir, StringComparison.OrdinalIgnoreCase)) return true;
                        }
                    }
                    return false;
                });
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(Globalization.Get("Form1.FailedInParsing", vpkPath, e.Message));
                return false;
            }
        }

        public static void CreateDirectory(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        }

        public static void OpenInExplorer(int index, string? key)
        {
            if (index < 0 || index >= Views.Length) return;
            var view = Views[index];
            var path = Path.Combine(LocalStash, $"{PathNPC}\\{view.Filename}");
            if (view.IsFile) path += $".{Ext}";
            else if (key != null) path += $"\\{key}.{Ext}";
            Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = "explorer", Arguments = $"/select,\"{path}\"" });
        }

        public static void Save(int index, DSONObject root)
        {
            if (index < 0 || index >= Views.Length) return;
            var view = Views[index];
            var relPath = $"{PathNPC}\\{view.Filename}";
            if (view.IsFile) File.WriteAllText(Path.Combine(LocalStash, relPath + $".{Ext}"), root.ToString());
            else
            {
                foreach (var pair in root)
                {
                    if (pair.Value is DSONObject obj) File.WriteAllText(Path.Combine(LocalStash, $"{relPath}\\{pair.Key}.{Ext}"), obj.ToString());
                }
            }
        }

        private readonly struct View(string name, string filename, bool isFile)
        {
            public readonly string Name = name;
            public readonly string Filename = filename;
            public readonly bool IsFile = isFile;

            public View(string name, string filename) : this(name, filename, true) { }
        }
    }
}
