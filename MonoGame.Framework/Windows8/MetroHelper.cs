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
            var result = Task.Run( async () => {

                try
                {
                    var localFolder = ApplicationData.Current.LocalFolder;
                    var storageFile = await localFolder.GetFileAsync(fileName);
                }
                catch (FileNotFoundException)
                {
                    return false;
                }

                return true;

            }).Result;

            return result;
        }
    }
}
