using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Microsoft.Xna.Framework
{
    static internal class MetroHelper
    {
        static public bool AppDataFileExists(string fileName)
        {
            var result = Task.Run( async () => {

                try
                {
                    var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(fileName);
                    return file == null ? false : true;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
            }).Result;

            return result;
        }
    }
}
