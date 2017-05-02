using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TmsSecurityCore.Contract;

namespace TmsSecurityCore.FileAccess
{
    public class TmsUncFileAccess : ITmsFileAccess
    {
        public IServiceContext CurrentServiceContext { get; private set;  }
        public TmsUncFileAccess(IServiceContext serviceContext = null)
        {
            this.CurrentServiceContext = serviceContext; 
        }

        public string GetFilePath(string filePath)
        {
            // Just in case we need to do some kungfu to the original file path
            return filePath;
        }

        public IEnumerable<string> GetDirectories(string path)
        {
            return TmsUncFileHelper.GetDirectories(path);
        }

        public IEnumerable<string> GetFiles(string path, string pattern = null, bool searchall = false)
        {
            return pattern != null && searchall
                ? Directory.GetFiles(path, pattern, SearchOption.AllDirectories)
                : (pattern != null
                    ? Directory.GetFiles(path, pattern)
                    : Directory.GetFiles(path));
        }

        /// <summary>
        /// Copy file from source to Target and return TargetFile name
        /// </summary>
        /// <param name="from">Source </param>
        /// <param name="to">Target</param>
        /// <param name="overwrite">Overwrite filename if exists</param>
        /// <param name="createFolderIfNotExists">Create target path if not exists</param>
        /// <param name="deleteFromSourceOnSuccessCopy">Delete from source on successful copy operation</param>
        /// <returns></returns>
        public ITmsFileCopyResponse CopyFileTo(string from, string to, bool overwrite = false, bool createFolderIfNotExists = false, bool deleteFromSourceOnSuccessCopy = false)
        {
            return TmsUncFileHelper.CopyFileTo(@from, to, overwrite, createFolderIfNotExists, deleteFromSourceOnSuccessCopy);
        }

        public void WriteAllText(string filePath, string contents, Encoding encoding = null)
        {
            TmsUncFileHelper.WriteAllText(filePath, contents, encoding);
        }

        public string ReadAllText(string filePath)
        {
            return TmsUncFileHelper.ReadAllText(filePath);
        }

        public IEnumerable<string> ReadAllLines(string filePath)
        {
            return TmsUncFileHelper.ReadAllLines(filePath);
        }

        public IEnumerable<string> ReadLines(string filePath)
        {
            return TmsUncFileHelper.ReadLines(filePath); 
        }

        public void MakeDirectoryIfNotExists(string path)
        {
            TmsUncFileHelper.MakeDirectoryIfNotExists(path);
        }

        public StreamReader OpenText(string file)
        {
            return TmsUncFileHelper.OpenText(file);
        }

        public ITmsFileDeleteResponse DeleteFile(string filePath)
        {
            var rtn = new TmsFileDeleteResponse
                          {
                              SourceFile = filePath
                          };
            
            var fi = new FileInfo(filePath);
            if(!fi.Exists)
            {
                rtn.ErrorMsg = "Source file does not exits.";
            }
            else
            {
                try
                {
                    fi.Delete();
                    rtn.Success = true;
                }catch(Exception ex)
                {
                    rtn.ErrorMsg = ex.Message; 
                }
            }
            return rtn; 
        }

        public ITmsFileCopyResponse CopyDirectoryTo(string @from, string to, bool overwrite = false,
            bool createFolderIfNotExists = false, bool deleteFromSourceOnSuccessCopy = false, bool copySubDirs = false)
        {
            return TmsUncFileHelper.CopyDirectoryTo(@from, to, overwrite, createFolderIfNotExists,
                deleteFromSourceOnSuccessCopy, copySubDirs);
        }

        public bool Exists(string filePath)
        {
            return TmsUncFileHelper.Exists(filePath);
        }
    }
}