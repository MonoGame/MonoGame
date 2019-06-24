#region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009-2010 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
#endregion License

#region Using clause
using System;

#endregion Using clause

namespace Microsoft.Xna.Framework.Net
{
	public enum NetworkSessionType
	{
		Local,    		// Does not involve any networking traffic, but can be used for split-screen gaming on a single Xbox 360 console. Creating a local network session may also make it easier to share code between local and online game modes. 

		SystemLink,	// Connect multiple Xbox 360 consoles or computers over a local subnet. These machines do not require a connection to Xbox LIVE or any LIVE accounts. However, connection to machines on different subnets is not allowed.
				// If you are a Creators Club developer testing your game, you can use this type to connect an Xbox 360 console to a computer. However, cross-platform networking is not supported in games distributed to non-–Creators Club community players.

		PlayerMatch,	// Uses the Xbox LIVE servers. This enables connection to other machines over the Internet. It requires a LIVE Silver Membership for Windows-based games or a LIVE Gold membership for Xbox 360 games. Games in development will also require an XNA Creators Club premium membership. While in trial mode, Indie games downloaded from Xbox LIVE Markeplace will not have access to LIVE matchmaking.
		Ranked,		// All session matches are ranked. This option is available only for commercial games that have passed Xbox LIVE certification. Due to the competitive nature of the gameplay, this session type does not support join-in-progress.
	}
}