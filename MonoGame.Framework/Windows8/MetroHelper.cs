using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Microsoft.Xna.Framework
{
    static public class MetroHelper
    {
        static public bool AppDataFileExists(string fileName)
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var task = localFolder.GetFileAsync(fileName);
                if (task != null && task.GetResults() != null)
                    return true;
                else
                    return false;
            }
            catch (FileNotFoundException ex)
            {
                return false;
            }
        }
    }
}
