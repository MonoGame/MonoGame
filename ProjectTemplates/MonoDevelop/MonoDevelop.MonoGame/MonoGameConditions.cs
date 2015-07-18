using System;
using Mono.Addins;
using System.IO;

namespace MonoDevelop.MonoGame
{
	public class MonoGameIsWindowsCondition : ConditionType
	{
		public override bool Evaluate (NodeElement conditionNode)
		{
			return Environment.OSVersion.Platform == PlatformID.Win32NT;
		}
	}

	public class MonoGameIsLinuxCondition : ConditionType
	{
		public override bool Evaluate (NodeElement conditionNode)
		{
			return Environment.OSVersion.Platform == PlatformID.Unix
			&& !Directory.Exists ("/Applications")
			&& !Directory.Exists ("/Users")
			&& !Directory.Exists ("/Library");
		}
	}

	public class MonoGameIsMacCondition : ConditionType
	{
		public override bool Evaluate (NodeElement conditionNode)
		{
			return (Environment.OSVersion.Platform == PlatformID.Unix
				&& Directory.Exists("/Applications")
				&& Directory.Exists("/Users")
				&& Directory.Exists("/Library"))
				|| Environment.OSVersion.Platform == PlatformID.MacOSX;
		}
	}
}

