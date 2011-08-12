using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkPrediction
{
    class Program
    {
        static void Main(string[] args)
        {
            using (NetworkPredictionGame game = new NetworkPredictionGame())
            {
                game.Run();
            }
        }
    }
}
