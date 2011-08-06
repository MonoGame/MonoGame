#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

#region Using Statements
//﻿using System;
//using System.IO;
//using System.Reflection;
//using Microsoft.Xna;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Storage;
#endregion


//namespace Microsoft.Xna.Framework.Storage
//{
//    public class StorageContainer : IDisposable
//    {
//		private readonly string _path;
//		private readonly StorageDevice _device;
//		private readonly string _name;
//
//        public StorageContainer(StorageDevice device, string name)
//        {
//			_device = device;
//			_name = name;
//			_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+System.IO.Path.DirectorySeparatorChar+name;
//			// Creathe the "device" if need
//			if (!Directory.Exists(_path))
//			{
//				Directory.CreateDirectory(_path);
//			}
//        }
//
//        public string Path
//        {
//            get
//            {
//                return _path;
//            }
//        }
//		
//		 public Microsoft.Xna.Framework.Storage.StorageDevice StorageDevice
//        {
//            get
//            {
//				return _device;
//            }
//        }
//
//		public static string TitleLocation 
//		{ 
//			get
//			{
//				return Directory.GetParent(Assembly.GetEntryAssembly().Location).ToString();
//			}
//		}
//		
//        public string TitleName
//        {
//            get
//            {
//				return _name;
//            }
//        }
//
//
//        #region IDisposable Members
//
//        public void Dispose()
//        {
//            
//        }
//
//        #endregion
//    }
//}

#region Assembly Microsoft.Xna.Framework.Storage.dll, v4.0.30319
// C:\Program Files (x86)\Microsoft XNA\XNA Game Studio\v4.0\References\Windows\x86\Microsoft.Xna.Framework.Storage.dll
#endregion
using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace Microsoft.Xna.Framework.Storage
{
	// Summary:
	//     Represents a logical collection of storage files. Reference page contains
	//     links to related conceptual articles.
	
	
	//	What is storage can be found here ->	http://msdn.microsoft.com/en-us/library/bb200105.aspx#ID4EDB
	
	//	Implementation on Windows
	//	
	//	User storage is in the My Documents folder of the user who is currently logged in, in the SavedGames folder. 
	//	A subfolder is created for each game according to the titleName passed to the BeginOpenContainer method. 
	//	When no PlayerIndex is specified, content is saved in the AllPlayers folder. When a PlayerIndex is specified, 
	//	the content is saved in the Player1, Player2, Player3, or Player4 folder, depending on which PlayerIndex 
	//	was passed to BeginShowSelector.

			
	public class StorageContainer : IDisposable
	{
		internal readonly string _storagePath;
		private readonly StorageDevice _device;
		private readonly string _name;
		private readonly PlayerIndex? _playerIndex;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Microsoft.Xna.Framework.Storage.StorageContainer"/> class.
		/// </summary>
		/// <param name='_device'>
		/// _device.
		/// </param>
		/// <param name='_name'>
		/// _name.
		/// </param>
		/// <param name='playerIndex'>
		/// The player index of the player to save the data
		/// </param>
		internal StorageContainer(StorageDevice device, string name, PlayerIndex? playerIndex)
		{

			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("A title name has to be provided in parameter name.");			

			_device = device;
			_name = name;
			_playerIndex = playerIndex;
			
			// From the examples the root is based on MyDocuments folder
			var root = StorageDevice.StorageRoot;
			var saved = Path.Combine(root,"SavedGames");
			_storagePath = Path.Combine(saved,name);
			
			var playerSave = string.Empty;
			if (playerIndex.HasValue) {
				playerSave = Path.Combine(root,"Player" + (int)playerIndex.Value);
			}
			
			if (!string.IsNullOrEmpty(playerSave))
				_storagePath = Path.Combine(root,"Player" + (int)playerIndex);
			
				
			// Creathe the "device" if need be
			if (!Directory.Exists(_storagePath))
			{
				Directory.CreateDirectory(_storagePath);
			}
			
		}
		
		// Summary:
		//     Gets the name of the title.
		public string DisplayName { 
			get { return _name; }
		}
		
		//
		// Summary:
		//     Gets a value that indicates whether the object is disposed.
		public bool IsDisposed { get; private set; }
		//
		// Summary:
		//     Gets the StorageDevice that holds the files in this container.
		public StorageDevice StorageDevice { 
			
			get {return _device; }
		}

		// Summary:
		//     Occurs when Dispose is called or when this object is finalized and collected
		//     by the garbage collector of the Microsoft .NET common language runtime.
		//
		// Parameters:
		//   :
		// TODO: Implement the Disposing function.  Find sample first
		public event EventHandler<EventArgs> Disposing;

		// Summary:
		//     Creates a new directory in the StorageContainer scope.
		//
		// Parameters:
		//   directory:
		//     The relative path of the directory to delete within the StorageContainer
		//     scope.
		public void CreateDirectory (string directory)
		{
			if (string.IsNullOrEmpty(directory))
				throw new ArgumentNullException("Parameter directory must contain a value.");
			
			// relative so combine with our path
			var dirPath = Path.Combine(_storagePath, directory);
			
			// Now let's try to create it
			Directory.CreateDirectory(dirPath);
			
		}
		
		//
		// Summary:
		//     Creates a file at a specified path in the StorageContainer.
		//
		// Parameters:
		//   file:
		//     The relative path of the file to be created in the StorageContainer.
		public Stream CreateFile (string file)
		{
			if (string.IsNullOrEmpty(file))
				throw new ArgumentNullException("Parameter file must contain a value.");
			
			// relative so combine with our path
			var filePath= Path.Combine(_storagePath, file);
			
			// return A new file with read/write access.
			return File.Create(filePath);				
			
		}				
		//
		// Summary:
		//     Deletes a directory in the StorageContainer scope.
		//
		// Parameters:
		//   directory:
		//     The relative path of the directory to delete within the StorageContainer
		//     scope.
		public void DeleteDirectory (string directory)
		{
			if (string.IsNullOrEmpty(directory))
				throw new ArgumentNullException("Parameter directory must contain a value.");
			
			// relative so combine with our path
			var dirPath = Path.Combine(_storagePath, directory);
			
			// Now let's try to delete it
			Directory.Delete(dirPath);
		}				
		//
		// Summary:
		//     Deletes a file in the StorageContainer.
		//
		// Parameters:
		//   file:
		//     The relative path of the file to delete within the StorageContainer.
		public void DeleteFile (string file)
		{
			if (string.IsNullOrEmpty(file))
				throw new ArgumentNullException("Parameter file must contain a value.");
			
			// relative so combine with our path
			var filePath= Path.Combine(_storagePath, file);
			
			// Now let's try to delete it
			File.Delete(filePath);		
		}				
		//
		// Summary:
		//     Determines whether the specified path refers to an existing directory in
		//     the StorageContainer.
		//
		// Parameters:
		//   directory:
		//     The path to test.
		public bool DirectoryExists (string directory)
		{
			if (string.IsNullOrEmpty(directory))
				throw new ArgumentNullException("Parameter directory must contain a value.");
			
			// relative so combine with our path
			var dirPath = Path.Combine(_storagePath, directory);
			
			return Directory.Exists(dirPath);
		}				
		//
		// Summary:
		//     Immediately releases the unmanaged resources used by this object.
		public void Dispose ()
		{

			// Fill this in when we figure out what we should be disposing
			IsDisposed = true;
		}				
		//
		// Summary:
		//     Determines whether the specified path refers to an existing file in the StorageContainer.
		//
		// Parameters:
		//   file:
		//     The path and file name to test.
		public bool FileExists (string file)
		{
			if (string.IsNullOrEmpty(file))
				throw new ArgumentNullException("Parameter file must contain a value.");
			
			// relative so combine with our path
			var filePath= Path.Combine(_storagePath, file);
			
			// return A new file with read/write access.
			return File.Exists(filePath);		
		}				
		//
		// Summary:
		//     Enumerates the directories in the root of a StorageContainer.
		public string[] GetDirectoryNames ()
		{
			return Directory.GetDirectories(_storagePath);
		}				
		//
		// Summary:
		//     Enumerates the directories in the root of a StorageContainer that conform
		//     to a search pattern.
		//
		// Parameters:
		//   searchPattern:
		//     A search pattern. Both single-character ("?") and multicharacter ("*") wildcards
		//     are supported.
		public string[] GetDirectoryNames (string searchPattern)
		{
			throw new NotImplementedException ();
		}
		//
		// Summary:
		//     Enumerates files in the root directory of a StorageContainer.
		public string[] GetFileNames ()
		{
			return Directory.GetFiles(_storagePath);		
		}				
		//
		// Summary:
		//     Enumerates files in the root directory of a StorageContainer that match a
		//     given pattern.
		//
		// Parameters:
		//   searchPattern:
		//     A search pattern. Both single-character ("?") and multicharacter ("*") wildcards
		//     are supported.
		public string[] GetFileNames (string searchPattern)
		{
			if (string.IsNullOrEmpty(searchPattern))
				throw new ArgumentNullException("Parameter searchPattern must contain a value.");
			
			return Directory.GetFiles(_storagePath, searchPattern);
		}				
		//
		// Summary:
		//     Opens a file in the StorageContainer.
		//
		// Parameters:
		//   file:
		//     Relative path of the file within the StorageContainer.
		//
		//   fileMode:
		//     One of the enumeration values that specifies how to open the file.
		public Stream OpenFile (string file, FileMode fileMode)
		{
			return OpenFile(file, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite);
		}				
		//
		// Summary:
		//     Opens a file in the StorageContainer in the designated mode with the specified
		//     read/write access.
		//
		// Parameters:
		//   file:
		//     Relative path of the file within the StorageContainer.
		//
		//   fileMode:
		//     One of the enumeration values that specifies how to open the file.
		//
		//   fileAccess:
		//     One of the enumeration values that specifies whether the file is opened with
		//     read, write, or read/write access.
		public Stream OpenFile (string file, FileMode fileMode, FileAccess fileAccess)
		{
			return OpenFile(file, fileMode, fileAccess, FileShare.ReadWrite);
		}				
		//
		// Summary:
		//     Opens a file in the StorageContainer in the designated mode with the specified
		//     read/write access and sharing permission.
		//
		// Parameters:
		//   file:
		//     Relative path of the file within the StorageContainer.
		//
		//   fileMode:
		//     One of the enumeration values that specifies how to open the file.
		//
		//   fileAccess:
		//     One of the enumeration values that specifies whether the file is opened with
		//     read, write, or read/write access.
		//
		//   fileShare:
		//     A bitwise combination of enumeration values that specify the type of access
		//     other Stream objects have to this file.
		public Stream OpenFile (string file, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			if (string.IsNullOrEmpty(file))
				throw new ArgumentNullException("Parameter file must contain a value.");
		
			// relative so combine with our path
			var filePath= Path.Combine(_storagePath, file);
			
			return File.Open(filePath, fileMode, fileAccess, fileShare);
		}				
	}
}
