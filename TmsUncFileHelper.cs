using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TmsSecurityCore.FileAccess
{
    public static class TmsUncFileHelper
    {
        public static IEnumerable<string> GetFiles(string path, string pattern, bool searchall = false)
        {
            //return pattern != null ? Directory.GetFiles(path, pattern).AsEnumerable() : Directory.GetFiles(path).AsEnumerable();
            return pattern != null && searchall
                ? Directory.GetFiles(path, pattern, SearchOption.AllDirectories).AsEnumerable()
                : (pattern != null
                    ? Directory.GetFiles(path, pattern).AsEnumerable()
                    : Directory.GetFiles(path)).AsEnumerable();
        }

        public static IEnumerable<string> GetDirectories(string path)
        {
            return Directory.GetDirectories(path).AsEnumerable();
        }

        public static ITmsFileCopyResponse CopyFileTo(string @from, string to, bool overwrite, bool createFolderIfNotExists,
            bool deleteFromSourceOnSuccessCopy)
        {
            var sourceInfo = new FileInfo(@from);
            var sourceFilename = sourceInfo.Name;

            var targetInfo = new FileInfo(to);

            var targetFilename = targetInfo.Name;
            // Does target path has a filename, if not use same name as source filename
            if (String.IsNullOrEmpty(targetFilename) || String.IsNullOrEmpty(targetInfo.Extension))
            {
                targetFilename = sourceFilename;
            }
            var di = targetInfo.Directory;
            if (di != null && !di.Exists)
            {
                if (createFolderIfNotExists)
                {
                    di.Create();
                }
            }
            var response = new TmsFileCopyResponse();
            if (targetInfo.DirectoryName != null)
            {
                var destFileName = Path.Combine(targetInfo.DirectoryName, targetFilename);
                //System.Diagnostics.Debug.WriteLine("File.Copy(\"{0}\",\"{1}\")", from, destFileName);
                File.Copy(@from, destFileName, overwrite);
                if (deleteFromSourceOnSuccessCopy)
                    sourceInfo.Delete();

                response.Success = true;
                response.TargetFileFullPath = destFileName;
                response.TargetFilename = targetFilename;
                response.TargetFilePath = targetInfo.DirectoryName;
            }
            else
            {
                response.Success = false;
            }
            return response;
        }

        public static ITmsFileCopyResponse CopyDirectoryTo(string @from, string to, bool overwrite = false,
            bool createFolderIfNotExists = false, bool deleteFromSourceOnSuccessCopy = false, bool copySubDirs = false)
        {
            var response = new TmsFileCopyResponse();

            try
            {
                // Get the subdirectories for the specified directory.
                var dir = new DirectoryInfo(@from);
                var dirs = dir.GetDirectories();

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + @from);
                }

                // If the destination directory doesn't exist, create it. 
                if (!Directory.Exists(to))
                {
                    Directory.CreateDirectory(to);
                }

                // Get the files in the directory and copy them to the new location.
                var files = dir.GetFiles();
                foreach (var file in files)
                {
                    var temppath = Path.Combine(to, file.Name);
                    file.CopyTo(temppath, true);
                }

                // If copying subdirectories, copy them and their contents to new location. 
                if (copySubDirs)
                {
                    foreach (var subdir in dirs)
                    {
                        var temppath = Path.Combine(to, subdir.Name);
                        CopyDirectoryTo(subdir.FullName, temppath, copySubDirs);
                    }
                }

                var toDirectoryInfo = new DirectoryInfo(to);
                response.Success = true;
                response.TargetFileFullPath = toDirectoryInfo.FullName;
                response.TargetFilename = toDirectoryInfo.Name;
                response.TargetFilePath = toDirectoryInfo.Name;
            }
            catch (Exception exception)
            {
                response.Success = false;
            }
            return response;
        }

        public static void WriteAllText(string filePath, string contents, Encoding encoding = null)
        {
            if (null != encoding)
            {
                File.WriteAllText(filePath, contents, encoding);
            }
            else
            {
                File.WriteAllText(filePath, contents);
            }
        }

        public static string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public static IEnumerable<string> ReadAllLines(string filePath)
        {
            return File.ReadAllLines(filePath);
        }

        public static void MakeDirectoryIfNotExists(string path)
        {
            var di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                di.Create();
            }
        }

        public static StreamReader OpenText(string file)
        {
            return File.OpenText(file);
        }

        public static IEnumerable<string> ReadLines(string filePath)
        {
            return File.ReadLines(filePath); 
        }

        public static bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}