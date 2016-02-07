using System;
using System.IO;
using System.Linq;

namespace TeamworkMsprojectExportTweaker
{
    internal class FileManager
    {
        public static string GetResultFilename(string origFile)
        {
            var newName = Path.Combine(Path.GetDirectoryName(origFile), Path.GetFileNameWithoutExtension(origFile) + "_fixed");
            var newFileName = newName + Path.GetExtension(origFile);
            var counter = 2;
            while (File.Exists(newFileName))
            {
                newFileName = newName + counter++ + Path.GetExtension(origFile);
            }

            return newFileName;
        }
    }
}