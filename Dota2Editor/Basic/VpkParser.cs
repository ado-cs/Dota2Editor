using System.Security.Cryptography;
using System.Text;

namespace Dota2Editor.Basic
{
    public static class VpkParser
    {
        private static readonly Encoding ENCODING = Encoding.UTF8;
        private static readonly uint SIGNATURE = 0x55aa1234;
        private static readonly ushort SUFFIX = 0xffff;
        private static readonly ushort ARCH_INDEX = 0x7fff;
        private static readonly byte END_OF_STRING = 0;

        public static void Extract(string vpkPath, string outputFolder, Func<string, string, string, bool> fileFilter)
        {
            var fsCache = new Dictionary<ushort, FileStream>();

            using (var fs = File.OpenRead(vpkPath))
            {
                var data = fs.ReadBytes(12);
                if (BitConverter.ToUInt32(data, 0) != SIGNATURE) throw new InvalidDataException(Globalization.Get("VpkParser.SignatureMismatch"));
                var version = BitConverter.ToUInt32(data, 4);
                var treeLen = BitConverter.ToUInt32(data, 8);

                int headerLen;
                if (version == 1) headerLen = 12;
                else if (version == 2) headerLen = 28;
                else throw new InvalidDataException(Globalization.Get("VpkParser.UnsupportVersion", version));

                fs.Seek(headerLen, SeekOrigin.Begin);
                while (true)
                {
                    if (fs.Position > treeLen + headerLen) throw new InvalidDataException(Globalization.Get("VpkParser.OutOfBoounds"));
                    if (fs.ReadString(out var ext)) break;
                    while (true)
                    {
                        if (fs.ReadString(out var path)) break;
                        while (true)
                        {
                            if (fs.ReadString(out var name)) break;

                            data = fs.ReadBytes(6);
                            var crc32 = BitConverter.ToUInt32(data, 0);
                            var preloadLen = BitConverter.ToUInt16(data, 4);

                            if (fileFilter(path, name, ext))
                            {
                                data = fs.ReadBytes(12);
                                var archiveIndex = BitConverter.ToUInt16(data, 0);
                                long archiveOffset = BitConverter.ToUInt32(data, 2);
                                var fileLen = BitConverter.ToUInt32(data, 6);
                                var suffix = BitConverter.ToUInt16(data, 10);
                                if (suffix != SUFFIX) throw new InvalidDataException(Globalization.Get("VpkParser.ErrorIndex"));

                                var file = fs;
                                if (archiveIndex == ARCH_INDEX) archiveOffset += headerLen + treeLen;
                                else if (!fsCache.TryGetValue(archiveIndex, out file))
                                {
                                    var dir = archiveIndex.ToString();
                                    while (dir.Length < 3) dir = '0' + dir;
                                    var subPath = vpkPath.Replace("english", "");
                                    var i = subPath.ToLower().LastIndexOf("dir.");
                                    if (i == -1) throw new InvalidDataException(Globalization.Get("VpkParser.NotADir"));
                                    subPath = subPath[..i] + dir + subPath[(i + 3)..];
                                    if (!File.Exists(subPath)) throw new InvalidDataException(Globalization.Get("VpkParser.MissSubFile", subPath));
                                    file = File.OpenRead(subPath);
                                    fsCache.Add(archiveIndex, file);
                                }
                                var preload = fs.ReadBytes(preloadLen);
                                var pos = fs.Position;

                                file.Seek(archiveOffset, SeekOrigin.Begin);
                                Directory.CreateDirectory(Path.Combine(outputFolder, path));
                                using (var f = File.OpenWrite(Path.Combine(outputFolder, path, name + '.' + ext)))
                                {
                                    f.Write(preload);
                                    data = new byte[fileLen];
                                    file.Read(data);
                                    f.Write(data);
                                    f.SetLength(preloadLen + fileLen);
                                }
                                if (file == fs) fs.Seek(pos, SeekOrigin.Begin);
                            }
                            else fs.Seek(preloadLen + 12, SeekOrigin.Current);
                        }
                    }
                }
            }
            foreach (var fs in fsCache.Values) fs.Close();
        }

        public static void Save(string folderPath, string outputVpk)
        {
            var tree = new Dictionary<string, Dictionary<string, List<FileMeta>>>();
            var dirList = new Queue<string>();
            dirList.Enqueue(folderPath);
            var treeLen = 1;
            while (dirList.Count > 0)
            {
                var current = dirList.Dequeue();
                foreach (var item in Directory.EnumerateFiles(current))
                {
                    var meta = new FileMeta(item, folderPath);
                    if (!tree.TryGetValue(meta.Ext, out var dict))
                    {
                        tree.Add(meta.Ext, dict = []);
                        treeLen += meta.Ext.Length + 2;
                    }
                    if (!dict.TryGetValue(meta.RelPath, out var list))
                    {
                        dict.Add(meta.RelPath, list = []);
                        treeLen += meta.RelPath.Length + 2;
                    }
                    list.Add(meta);
                    treeLen += meta.Name.Length + 19;
                }
                foreach (var item in Directory.EnumerateDirectories(current)) dirList.Enqueue(item);
            }
            if (File.Exists(outputVpk)) File.Delete(outputVpk);

            using (var fs = new FileStream(outputVpk, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                fs.WriteBytes(SIGNATURE, 2u, (uint)treeLen, 0u, 0u, 48u, 0u);

                var headerLen = fs.Position;
                var dataOffset = headerLen + treeLen;
                var embedChunkLen = 0L;

                foreach (var ext in tree.Keys)
                {
                    fs.WriteBytes(ext);
                    var dict = tree[ext];
                    foreach (var rel in dict.Keys)
                    {
                        fs.WriteBytes(rel);
                        foreach (var meta in dict[rel])
                        {
                            fs.WriteBytes(meta.Name);

                            var metadataOffset = fs.Position;
                            var fileOffset = dataOffset;
                            var data = File.ReadAllBytes(meta.FullPath);
                            var checksum = CRC32.Compute(data);
                            fs.Seek(dataOffset, SeekOrigin.Begin);
                            fs.Write(data);
                            dataOffset = fs.Position;
                            var fileLen = fs.Position - fileOffset;
                            fs.Seek(metadataOffset, SeekOrigin.Begin);
                            embedChunkLen += fileLen;
                            fs.WriteBytes(checksum, (ushort)0, ARCH_INDEX, (uint)(fileOffset - treeLen - headerLen), (uint)fileLen, SUFFIX);
                        }
                        fs.WriteByte(END_OF_STRING);
                    }
                    fs.WriteByte(END_OF_STRING);
                }
                fs.WriteByte(END_OF_STRING);

                fs.Seek(12, SeekOrigin.Begin);
                fs.WriteBytes((uint)embedChunkLen);
                var chunkHash = MD5.HashData(Array.Empty<byte>());

                fs.Seek(0, SeekOrigin.Begin);
                var headerChunk = new byte[headerLen];
                fs.Read(headerChunk);
                var treeChunk = new byte[treeLen];
                fs.Read(treeChunk);
                var embedChunk = new byte[embedChunkLen];
                fs.Read(embedChunk);

                var treeHash = MD5.HashData(treeChunk);
                var fileChunk = new byte[headerLen + treeLen + embedChunkLen + treeHash.Length + chunkHash.Length];
                Array.Copy(headerChunk, fileChunk, headerLen);
                Array.Copy(treeChunk, 0, fileChunk, headerLen, treeLen);
                Array.Copy(embedChunk, 0, fileChunk, headerLen + treeLen, embedChunkLen);
                Array.Copy(treeHash, 0, fileChunk, headerLen + treeLen + embedChunkLen, treeHash.Length);
                Array.Copy(chunkHash, 0, fileChunk, headerLen + treeLen + embedChunkLen + treeHash.Length, chunkHash.Length);

                fs.WriteBytes(treeHash, chunkHash, MD5.HashData(fileChunk));
            }
        }

        private class FileMeta
        {
            public readonly string Name;
            public readonly string Filename;
            public readonly string RelPath;
            public readonly string Ext;
            public readonly string FullPath;

            public FileMeta(string path, string folderPath)
            {
                var i = path.LastIndexOf('.');
                var j = path.LastIndexOf('\\');
                if (i == -1 || j == -1 || i < j) throw new InvalidDataException(Globalization.Get("VpkParser.InvalidPath", path));
                Ext = path[(i + 1)..];
                FullPath = path;
                RelPath = path[(folderPath.Length)..j].TrimStart('\\').Replace('\\', '/');
                Filename = path[(j + 1)..];
                Name = path[(j + 1)..i];
            }
        }

        private static bool ReadString(this FileStream fs, out string str)
        {
            var result = new byte[1024];
            var data = new byte[64];
            var pos = 0;
            while (true)
            {
                fs.Read(data);
                var i = 0;
                for (; i < data.Length; i++)
                {
                    if (data[i] == END_OF_STRING) break;
                    result[pos++] = data[i];
                    if (pos == result.Length)
                    {
                        var newArray = new byte[result.Length + 1024];
                        Array.Copy(result, newArray, result.Length);
                        result = newArray;
                    }
                }
                if (i < data.Length)
                {
                    fs.Seek(fs.Position - (data.Length - i - 1), SeekOrigin.Begin);
                    break;
                }
            }
            if (pos == 0)
            {
                str = string.Empty;
                return true;
            }
            str = ENCODING.GetString(result, 0, pos);
            return false;
        }

        private static byte[] ReadBytes(this FileStream fs, int length)
        {
            var data = new byte[length];
            fs.Read(data);
            return data;
        }

        private static void WriteBytes(this FileStream fs, params object[] items)
        {
            foreach (var item in items)
            {
                if (item is byte[] a) fs.Write(a);
                else if (item is bool b) fs.Write(BitConverter.GetBytes(b));
                else if (item is ulong c) fs.Write(BitConverter.GetBytes(c));
                else if (item is uint d) fs.Write(BitConverter.GetBytes(d));
                else if (item is ushort e) fs.Write(BitConverter.GetBytes(e));
                else if (item is long f) fs.Write(BitConverter.GetBytes(f));
                else if (item is int g) fs.Write(BitConverter.GetBytes(g));
                else if (item is short h) fs.Write(BitConverter.GetBytes(h));
                else if (item is float i) fs.Write(BitConverter.GetBytes(i));
                else if (item is double j) fs.Write(BitConverter.GetBytes(j));
                else if (item is string k) { fs.Write(ENCODING.GetBytes(k)); fs.WriteByte(END_OF_STRING); }
            }
        }
    }
}
