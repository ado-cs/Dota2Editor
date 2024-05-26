using System.Data;

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

        public static string GetViewName(int index)
        {
            if (index < 0 || index >= Views.Length) return string.Empty;
            return Globalization.Get("Form1.Menu.View." + Views[index].Name);
        }

        public static bool GetRelativePath(int index, out string relPath)
        {
            var view = Views[index]; //won't check if index is out of bound
            relPath = $"{PathNPC}\\{view.Filename}";
            if (view.IsFile) relPath += $".{Ext}";
            return view.IsFile;
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

        private readonly struct View(string name, string filename, bool isFile)
        {
            public readonly string Name = name;
            public readonly string Filename = filename;
            public readonly bool IsFile = isFile;

            public View(string name, string filename) : this(name, filename, true) { }
        }
    }
}
