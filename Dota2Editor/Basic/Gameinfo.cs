using System.Text;

namespace Dota2Editor.Basic
{
    public static class Gameinfo
    {
        private static readonly Encoding ENCODING = Encoding.UTF8;
        private static readonly string SEARCH_BLOCK = "SearchPaths";
        private static readonly string SEARCH_PATH = "\t\t\tGame\t\t\t\t";
        private static readonly int PATCH_LEN = 4;

        public static bool IsActive(string path, string folderName) => ENCODING.GetString(File.ReadAllBytes(path)).Contains(SEARCH_PATH + folderName);

        public static bool Activate(string path, out byte[] data, string folderName)
        {
            data = File.ReadAllBytes(path);
            var text = ENCODING.GetString(data);
            if (text.Contains(SEARCH_PATH + folderName)) return false;

            //append new line 'Game xxx' to block 'SearchPaths' of file 'Gameinfo.gi'

            var commentStart = -1;
            var commentEnd = -1;
            var insertionPos = -1;

            var commentLine = false;
            var blockFlag = 0;

            var lineStart = 0;
            var i = 0;
            while (++i < text.Length)
            {
                if (text[i] == '\n' && text[i - 1] == '\r')
                {
                    if (commentLine)
                    {
                        if (commentStart >= 0 && commentEnd == -1 && i - 1 - commentStart >= SEARCH_PATH.Length + folderName.Length + PATCH_LEN + 2)
                        {
                            commentEnd = i - 1;
                            commentStart = commentEnd - SEARCH_PATH.Length - folderName.Length - PATCH_LEN - 2;
                            if (insertionPos != -1) break;
                        }
                        commentLine = false;
                    }
                    else if (i - 1 == lineStart)
                    {
                        if (blockFlag == 2)
                        {
                            insertionPos = lineStart;
                            if (commentEnd != -1) break;
                        }
                    }
                    else
                    {
                        var line = text[lineStart..(i - 1)];
                        if (blockFlag == 0)
                        {
                            if (line.Contains(SEARCH_BLOCK)) blockFlag = 1;
                        }
                        else if (blockFlag == 1)
                        {
                            if (line.Contains('{')) blockFlag = 2;
                        }
                        else if (blockFlag == 2)
                        {
                            if (line.Contains('}')) blockFlag = 3;
                        }
                    }
                    lineStart = i + 1;
                }
                else if (text[i] == '/' && text[i - 1] == '/')
                {
                    commentLine = true;
                    if (commentEnd == -1) commentStart = i + 1;
                }
            }

            if (insertionPos <= commentEnd) throw new FileFormatException(Globalization.Get("Gameinfo.FailedInRevicing"));

            //Append patch bytes to make the new file have the same CRC32 as the source file

            var targetCrc32 = CRC32.Compute(data);
            var sb = new StringBuilder("//");
            for (i = 0; i < PATCH_LEN; i++) sb.Append(' ');
            data = ENCODING.GetBytes(text[..commentStart] + text[commentEnd..insertionPos] + SEARCH_PATH + folderName + text[insertionPos..] + sb.ToString());
            CRC32.Patch(data, targetCrc32, PATCH_LEN);
            return true;
        }
    }
}
