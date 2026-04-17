using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;


namespace UBPC.Web.Common
{
    public class GUIDFileHandle
    {
        public const int BREAK_INTERVAL = 10;
        public string _slashSymbol = "\\";

        #region Properties

        //Make sure default file storage path is preset 
        public string DefaultFileStorage { get; set; }
        

        #endregion

        public GUIDFileHandle(string fileStoragePath)
        {
            DefaultFileStorage = fileStoragePath;
        }

        public string GenerateGUIDPath(string fileName, Guid guidValue)
        {
            string regenDir = string.Empty;
            int result = 0;
            int countDividePath = 0;

            try
            {
                regenDir = DefaultFileStorage;
                countDividePath = Math.DivRem(guidValue.ToString().Length, BREAK_INTERVAL, out result);

                for (int i = 0; i <= (countDividePath * BREAK_INTERVAL); i+= BREAK_INTERVAL)
                {
                    if (i > (guidValue.ToString().Length - BREAK_INTERVAL)) 
                    {
                        regenDir += _slashSymbol + guidValue.ToString().Substring(i, (guidValue.ToString().Length - i));
                        break;
                    }

                    regenDir += _slashSymbol + guidValue.ToString().Substring(i, 10);
                }

                regenDir += Path.GetExtension(Path.GetExtension(fileName));

                return regenDir;
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }

        public bool DeleteFolderRecursive(System.IO.DirectoryInfo baseDir)
        {
            try
            {
                baseDir.Attributes = System.IO.FileAttributes.Normal;
                foreach (var childDir in baseDir.GetDirectories())
                    DeleteFolderRecursive(childDir);

                foreach (var file in baseDir.GetFiles())
                    file.IsReadOnly = false;

                baseDir.Delete(true);

                return true;
            }
            catch (Exception ex) 
            {
                return false;
            }
        }

        public string GetFileName(string fileName, Guid guidValue)
        { 
            string guid = guidValue.ToString();
            string destFileName = guid.Substring(0, BREAK_INTERVAL); // Level 1

            destFileName += _slashSymbol + guid.Substring(BREAK_INTERVAL, BREAK_INTERVAL); // Level 2
            destFileName += _slashSymbol + guid.Substring(BREAK_INTERVAL * 2, BREAK_INTERVAL); // Level 3

            destFileName += _slashSymbol + guid.Substring(BREAK_INTERVAL * 3, guid.Length - (BREAK_INTERVAL * 3)); // FileName

            destFileName += Path.GetExtension(Path.GetExtension(fileName));

            return destFileName;
        }

        public string GetFileNameWithoutExt(Guid guidValue)
        {
            string guid = guidValue.ToString();
            string destFileName = guid.Substring(BREAK_INTERVAL * 3, guid.Length - (BREAK_INTERVAL * 3)); // FileName

            return destFileName;
        }

        public string GetFileDir2(Guid guidValue)
        {
            string guid = guidValue.ToString();
            string destFileDir = guid.Substring(0, BREAK_INTERVAL); // Level 1

            destFileDir += _slashSymbol + guid.Substring(BREAK_INTERVAL, BREAK_INTERVAL); // Level 2
            destFileDir += _slashSymbol + guid.Substring(BREAK_INTERVAL * 2, BREAK_INTERVAL); // Level 3

            return destFileDir;
        }
    }
}
