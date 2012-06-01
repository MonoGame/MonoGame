using System;
using System.Diagnostics;
using Tester.Properties;
using MonoGame.Tools.VisualStudio;

namespace CustomToolTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing COM registration");
            TestCOMActivation();

            Console.WriteLine("Testing generator");
            TestCodeGenerator();

            Console.WriteLine("Done");

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }

        }

        private static void TestCodeGenerator()
        {
            try
            {
                /// simple test effect
                var effectSource = Resources.SimpleEffect;
                var g = new MgfxGenerator();


                var result = g.GenerateCodeWrapper(null,effectSource);

                Console.WriteLine("Result: {0}", result);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: {0}", ex.Message);
            }
        }

        public static void TestCOMActivation()
        {
            Type type = Type.GetTypeFromCLSID(new Guid("{2AD954E2-F7EC-4E5E-A4B5-C6E0E826E31F}"));
            object o = Activator.CreateInstance(type);

            if (o != null)
                Console.WriteLine("OK");
            else
                Console.WriteLine("COM activation failed.");
        } 
    }
}
