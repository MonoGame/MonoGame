using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace PipelineInstaller
{
	class MainClass
	{
		public static void SaveResourceToDisk(string ResourceName, string FileToExtractTo)
		{
			Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
			using (FileStream resourceFile = new FileStream(FileToExtractTo, FileMode.Create)) {
				byte[] b = new byte[s.Length + 1];
				s.Read (b, 0, Convert.ToInt32 (s.Length));
				resourceFile.Write (b, 0, Convert.ToInt32 (b.Length - 1));
				resourceFile.Flush ();
				resourceFile.Close ();
			}
		}

		public static void Extract(string resource, string dir)
		{
			string[] split = resource.Split ('.');
			string path = split[2];

			for (int i = 3; i < split.Length; i++) {
				if(path == "Templates")
					path += "/" + split [i];
				else
					path += "." + split [i];
			}

			string d = System.IO.Path.GetDirectoryName(dir + path);
			if (!Directory.Exists (d)) 
				Directory.CreateDirectory (d);

			SaveResourceToDisk (resource, dir + path);
		}

		public static void Main (string[] args)
		{
			System.Reflection.Assembly thisExe; 
			thisExe = System.Reflection.Assembly.GetExecutingAssembly();
			string [] resources = thisExe.GetManifestResourceNames();

			string dir = "/tmp/mptemp/";

			Console.WriteLine ("Extracting Installer");

			foreach (string resource in resources) {
				Extract (resource, dir);
				Console.Write (".");
			}
			Console.Write ("\r\n");

			Process proc = new System.Diagnostics.Process();
			proc.StartInfo.FileName = "/bin/bash";
			proc.StartInfo.Arguments = "-c \"cd /tmp/mptemp/ && chmod +x install.sh && sudo ./install.sh\"";
			proc.StartInfo.UseShellExecute = false; 
			proc.StartInfo.RedirectStandardOutput = true;
			proc.Start();

			while (!proc.StandardOutput.EndOfStream) {
				string line = proc.StandardOutput.ReadLine();
				Console.WriteLine (line);
			}
		}
	}
}
