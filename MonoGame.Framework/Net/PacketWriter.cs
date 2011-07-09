#region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
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
using System.IO;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

#endregion Using clause

namespace Microsoft.Xna.Framework.Net
{
	public class PacketWriter : BinaryWriter
	{
	
		// I thought about using an array but that means more code in my opinion and it does not make sense
		//  since the memory stream is perfect for this.
		// Using a memory stream also fits nicely with the constructors see:
		// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.net.packetwriter.packetwriter.aspx
		// We will see when testing begins.
		#region Constructors
		public PacketWriter () : this (0)
		{
		}

		public PacketWriter (int capacity) : base ( new MemoryStream(capacity))
		{
		}

		#endregion

		#region Methods
		public void Write (Color Value)
		{
			this.Write (Value.PackedValue);
		}

		public override void Write (double Value)
		{
			this.Write (Value);
		}
		
		public void Write (Matrix Value)
		{
			// After looking at a captured packet it looks like all the values of 
			//  the matrix are written.  This is different than the Lidgren XNAExtensions
			this.Write (Value.M11);
			this.Write (Value.M12);
			this.Write (Value.M13);
			this.Write (Value.M14);
			this.Write (Value.M21);
			this.Write (Value.M22);
			this.Write (Value.M23);
			this.Write (Value.M24);
			this.Write (Value.M31);
			this.Write (Value.M32);
			this.Write (Value.M33);
			this.Write (Value.M34);
			this.Write (Value.M41);
			this.Write (Value.M42);
			this.Write (Value.M43);
			this.Write (Value.M44);
		}

		public void Write (Quaternion Value)
		{
			// This may need to be corrected as have no test for it
			this.Write(Value.X);
			this.Write(Value.Y);
			this.Write(Value.Z);
			this.Write(Value.W);			
			
		}

		public override void Write (float Value)
		{
			this.Write(Value);
		}

		public void Write (Vector2 Value)
		{
			this.Write(Value.X);
			this.Write(Value.Y);
		}

		public void Write (Vector3 Value)
		{
			this.Write(Value.X);
			this.Write(Value.Y);
			this.Write(Value.Z);
		}

		public void Write (Vector4 Value)
		{
			this.Write(Value.X);
			this.Write(Value.Y);
			this.Write(Value.Z);
			this.Write(Value.W);
		}

		#endregion

		#region Properties
		public int Length { 
			get {
				return (int)BaseStream.Length;
			}
		}

		public int Position { 
			get {
				return (int)BaseStream.Position;
			}
			set {
				if (BaseStream.Position != value)
					BaseStream.Position = value;
			} 
		}
		#endregion
	}
}
