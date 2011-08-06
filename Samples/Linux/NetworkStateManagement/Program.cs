using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkStateManagement
{
    class Program
    {
        static void Main(string[] args)
        {
            using (NetworkStateManagementGame game = new NetworkStateManagementGame())
            {
                game.Run();
            }
        }
    }
}
