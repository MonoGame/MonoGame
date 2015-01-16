// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Microsoft.Xna.Framework
{
    static internal class ModernHelper
    {
        static public bool AppDataFileExists(string fileName)
        {
            var result = Task.Run( async () => {

                try
                {
                    var file = await Package.Current.InstalledLocation.GetFileAsync(fileName);
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
