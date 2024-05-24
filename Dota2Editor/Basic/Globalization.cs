using System.Collections.Immutable;

namespace Dota2Editor.Basic
{
    internal static class Globalization
    {
        private static readonly Dictionary<string, Dictionary<string, string>> LANG = new()
        {
            { 
                "zh-CN", new() 
                { 
                    { "Gameinfo.FailedInRevicing", "无法解析Gameinfo文件" },
                    { "VpkParser.SignatureMismatch", "验证VPK签名失败" },
                    { "VpkParser.UnsupportVersion", "不支持的VPK版本: {0}" },
                    { "VpkParser.OutOfBoounds", "指针溢出'" },
                    { "VpkParser.ErrorIndex", "读取子文件索引失败" },
                    { "VpkParser.NotADir", "该文件不是主VPK文件" },
                    { "VpkParser.MissSubFile", "丢失子VPK文件: '{0}'" },
                    { "VpkParser.InvalidPath", "非法文件路径: {0}" },
                    { "DSONObject.IllegalChar", "非法字符 '{0}' 在第 {1} 行" },
                    { "DSONObject.DuplicatedKey", "重复键 '{0}' 在第 {1} 行" },
                    { "DSONObject.IllegalPair1", "不合法的键值对在第 {0} 行" },
                    { "DSONObject.IllegalPair2", "不合法的键值对在第 {0} 行 和第 {1} 行" },
                    { "DSONObject.IllegalColons", "不合法的冒号第 {0} 行" },
                    { "DSONObject.UnexpectedEnd", "文件结尾不合法" },
                    { "Form1.SuccessInModification", "游戏数据修改已生效" },
                    { "Form1.FailedInModification1", "无法修改游戏数据，请关闭游戏后再尝试" },
                    { "Form1.FailedInModification2", "无法修改游戏数据，原因: {0}" },
                    { "Form1.SuccessInRecovery", "游戏数据已恢复" },
                    { "Form1.FailedInRecovery1", "无法恢复游戏数据，请关闭游戏后再尝试" },
                    { "Form1.FailedInRecovery2", "无法恢复游戏数据，本地备份文件丢失" },
                    { "Form1.SuccessInUpdating", "更新成功" },
                    { "Form1.FailedInParsingDSON", "无法解析文件 '{0}'，原因: {1}" },
                    { "Form1.SuccessInReplace", "替换成功" },
                    { "Form1.IllegalGamePath", "无法找到Dota2游戏" },
                    { "Form1.IllegalPathChar", "存在非法字符: '{0}'" },
                    { "Form1.GamePathMissing", "请先在 '游戏' 菜单中设置游戏路径" },
                    { "Form1.FailedInParsing", "无法解析文件 '{0}' ，原因: {1}" },
                    { "Form1.FileMissing", "文件 '{0}' 丢失" },
                    { "Form1.DirectoryMissing", "目录 '{0}' 丢失" },
                    { "Form1.RecordMissing", "记录 '{0}' 不存在" },
                    { "Form1.DeplicatedRecord", "记录 '{0}' 已存在" },
                    { "Form1.EmptyName", "名称不能为空" },
                    { "Form1.NoChanges", "无修改可保存" },
                    { "Form1.NewRecord", "请输入记录名称:" },
                    { "Form1.RenameRecord", "请输入记录新名称:" },
                    { "Form1.SuccessInLoadingRecord", "载入成功" },
                    { "Form1.Menu.Game", "游戏" },
                    { "Form1.Menu.Game.Write", "写出" },
                    { "Form1.Menu.Game.Recover", "恢复" },
                    { "Form1.Menu.Game.Update", "更新本地数据" },
                    { "Form1.Menu.Game.Replace", "替换本地数据" },
                    { "Form1.Menu.Game.Setting", "设置游戏路径" },
                    { "Form1.Menu.View", "视图" },
                    { "Form1.Menu.View.Items", "物品" },
                    { "Form1.Menu.View.NeutralItems", "中立物品" },
                    { "Form1.Menu.View.Units", "单位" },
                    { "Form1.Menu.View.Abilities", "单位技能" },
                    { "Form1.Menu.View.Heros", "英雄" },
                    { "Form1.Menu.View.HeroAbilities", "英雄技能" },
                    { "Form1.Menu.Help", "帮助" },
                    { "Form1.Menu.Help.Documentation", "帮助文档" },
                    { "Form1.Menu.Help.About", "关于" },
                    { "Form1.Menu.ListBox.Add", "记录" },
                    { "Form1.Menu.ListBox.Load", "加载" },
                    { "Form1.Menu.ListBox.Rename", "重命名" },
                    { "Form1.Menu.ListBox.Delete", "删除" },
                    { "InputForm.Button.Confirm", "确认" },
                    { "InputForm.Button.Cancel", "取消" },
                    { "BatchModificationForm.Text", "批量修改" },
                    { "BatchModificationForm.Label.Key", "键的模式" },
                    { "BatchModificationForm.Label.Value", "值" },
                    { "BatchModificationForm.ComboBox.Equals", "等于" },
                    { "BatchModificationForm.ComboBox.Increase", "加上" },
                    { "BatchModificationForm.ComboBox.Multiply", "乘以" },
                    { "BatchModificationForm.Button.Apply", "应用" },
                    { "BatchModificationForm.Button.Close", "关闭" },
                    { "BatchModificationForm.SuccessInModification", "共 {0} 项已修改" },
                    { "BatchModificationForm.FailedInParsingNumber", "值不是有效数字" },
                    { "AboutForm.Text", "关于" },
                    { "AboutForm.Introduction", "{0} v{1} 让你可以修改 Dota2 的物品, 英雄和技能.\n修改只在本地离线和人机的游戏.\n\n这个项目开源在" },
                    { "AboutForm.Button", "关闭" },
                } 
            },
            {
                "en-US", new()
                {
                    { "Gameinfo.FailedInRevicing", "Failed to parse the Gameinfo file" },
                    { "VpkParser.SignatureMismatch", "File is not VPK (invalid magic)" },
                    { "VpkParser.UnsupportVersion", "Unsupport vpk version {0}" },
                    { "VpkParser.OutOfBoounds", "Error parsing index (out of bounds)'" },
                    { "VpkParser.ErrorIndex", "Error while parsing the index of sub-vpk" },
                    { "VpkParser.NotADir", "The file is not a dir vpk" },
                    { "VpkParser.MissSubFile", "Missing sub-vpk file '{0}'" },
                    { "VpkParser.InvalidPath", "The path is invalid: {0}" },
                    { "DSONObject.IllegalChar", "Illegal char '{0}' found in line {1}" },
                    { "DSONObject.DuplicatedKey", "Duplicated key '{0}' found in line {1}" },
                    { "DSONObject.IllegalPair1", "Illegal key-value pair found in line {0}" },
                    { "DSONObject.IllegalPair2", "Illegal key-value pair found in line {0} and line {1}" },
                    { "DSONObject.IllegalColons", "Illegal number of colons found in line {0}" },
                    { "DSONObject.UnexpectedEnd", "Unexpected end of file" },
                    { "Form1.SuccessInModification", "Changes have taken effect" },
                    { "Form1.FailedInModification1", "Failed to modify game data. Please close the game and try again" },
                    { "Form1.FailedInModification2", "Failed to modify game data due to: {0}" },
                    { "Form1.SuccessInRecovery", "Game data has been recovered" },
                    { "Form1.FailedInRecovery1", "Failed to recover game data. Please close the game and try again" },
                    { "Form1.FailedInRecovery2", "Local backup file does not exist" },
                    { "Form1.SuccessInUpdating", "Updating successful" },
                    { "Form1.FailedInParsingDSON", "Failed to parse the file '{0}' due to {1}" },
                    { "Form1.SuccessInReplace", "Replacement successful" },
                    { "Form1.IllegalGamePath", "Cannot find Dota2 in this path" },
                    { "Form1.IllegalPathChar", "The character '{0}' is not allowed" },
                    { "Form1.GamePathMissing", "Please set the game path in menu 'Game' first" },
                    { "Form1.FailedInParsing", "Failed to parse file '{0}' due to {1}" },
                    { "Form1.FileMissing", "Missing file '{0}'" },
                    { "Form1.DirectoryMissing", "Missing directory '{0}'" },
                    { "Form1.RecordMissing", "Missing record '{0}'" },
                    { "Form1.DeplicatedRecord", "Record '{0}' already exists" },
                    { "Form1.EmptyName", "The name cannot be empty" },
                    { "Form1.NoChanges", "No changes detected" },
                    { "Form1.NewRecord", "Please input the name of new record:" },
                    { "Form1.RenameRecord", "Please input the new record name:" },
                    { "Form1.SuccessInLoadingRecord", "Loading successful" },
                    { "Form1.Menu.Game", "Game" },
                    { "Form1.Menu.Game.Write", "Write Changes" },
                    { "Form1.Menu.Game.Recover", "Recover Game Data" },
                    { "Form1.Menu.Game.Update", "Update Local Data" },
                    { "Form1.Menu.Game.Replace", "Renew Local Data" },
                    { "Form1.Menu.Game.Setting", "Set Game Path" },
                    { "Form1.Menu.View", "View" },
                    { "Form1.Menu.View.Items", "Items" },
                    { "Form1.Menu.View.NeutralItems", "Neutral Items" },
                    { "Form1.Menu.View.Units", "Units" },
                    { "Form1.Menu.View.Abilities", "Unit Abilities" },
                    { "Form1.Menu.View.Heros", "Heros" },
                    { "Form1.Menu.View.HeroAbilities", "Hero Abilities" },
                    { "Form1.Menu.Help", "Help" },
                    { "Form1.Menu.Help.Documentation", "Documentation" },
                    { "Form1.Menu.Help.About", "About" },
                    { "Form1.Menu.ListBox.Add", "Record Changes" },
                    { "Form1.Menu.ListBox.Load", "Load Changes" },
                    { "Form1.Menu.ListBox.Rename", "Rename the Record" },
                    { "Form1.Menu.ListBox.Delete", "Delete the Record(s)" },
                    { "InputForm.Button.Confirm", "Confirm" },
                    { "InputForm.Button.Cancel", "Cancel" },
                    { "BatchModificationForm.Text", "Batch Modification" },
                    { "BatchModificationForm.Label.Key", "Key Pattern" },
                    { "BatchModificationForm.Label.Value", "Value" },
                    { "BatchModificationForm.ComboBox.Equals", "Equals" },
                    { "BatchModificationForm.ComboBox.Increase", "Increase" },
                    { "BatchModificationForm.ComboBox.Multiply", "Multiply" },
                    { "BatchModificationForm.Button.Apply", "Apply" },
                    { "BatchModificationForm.Button.Close", "Close" },
                    { "BatchModificationForm.SuccessInModification", "{0} item(s) changed" },
                    { "BatchModificationForm.FailedInParsingNumber", "Invalid number format" },
                    { "AboutForm.Text", "About" },
                    { "AboutForm.Introduction", "{0} v{1} allows you to modify items, heros and abilities of Dota2.\nChanges only works on offline games vs bots.\n\nThis project is open source on" },
                    { "AboutForm.Button", "Close" },
                }
            }
        };

        private static readonly string DEFAULT_LANG = "en-US";
        private static string _currentLang;

        public static readonly ImmutableDictionary<string, string> SupportedLanguages;

        static Globalization() 
        {
            var name = Thread.CurrentThread.CurrentCulture.ToString();
            if (LANG.ContainsKey(name)) _currentLang = name;
            else _currentLang = DEFAULT_LANG;
            var supportedLangs = new Dictionary<string, string>() { { "English", "en-US" }, { "中文", "zh-CN" } };
            var langDir = Path.Combine(Environment.CurrentDirectory, "langs");
            if (Directory.Exists(langDir))
            {
                var defaultMap = LANG[DEFAULT_LANG];
                var keys = defaultMap.Keys;
                foreach (var file in  Directory.GetFiles(langDir))
                {
                    if (!file.ToLower().EndsWith(".txt")) continue;
                    var langName = Path.GetFileNameWithoutExtension(file);
                    if (string.IsNullOrEmpty(langName) || LANG.ContainsKey(langName)) continue;
                    DSONObject obj;
                    try { obj = DSONObject.Parse(File.ReadAllText(file)); } catch { continue; }
                    
                    var dict = new Dictionary<string, string>();
                    foreach (var pair in obj.Decomposition())
                    {
                        if (keys.Contains(pair.Key) && pair.Value is DSONValue v && !string.IsNullOrWhiteSpace(v.Text)) 
                            dict.Add(pair.Key, v.Text);
                    }
                    if (dict.Count == 0) continue;

                    foreach (var key in keys) dict.TryAdd(key, defaultMap[key]);

                    var aliasName = obj.RootKey;
                    if (string.IsNullOrEmpty(aliasName) || supportedLangs.ContainsKey(aliasName)) aliasName = langName;
                    LANG.Add(langName, dict);
                    supportedLangs.Add(aliasName, langName);
                }
            }
            SupportedLanguages = supportedLangs.ToImmutableDictionary();
        }

        public static string CurrentLang 
        { 
            get => _currentLang;
            set 
            {
                if (LANG.ContainsKey(value)) _currentLang = value;
            }
        }

        public static string Get(string key, params object[] args)
        {
            if (key != null && LANG[_currentLang].TryGetValue(key, out var str))
            {
                if (args == null || args.Length == 0) return str;
                return string.Format(str, args);
            }
            return string.Empty;
        }
    }
}
