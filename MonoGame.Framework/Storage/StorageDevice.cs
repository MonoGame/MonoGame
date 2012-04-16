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

//﻿using System;
//
//namespace Microsoft.Xna.Framework.Storage
//{
//    public class StorageDevice
//    {
//        public bool IsConnected
//        {
//            get
//            {
//                return true;
//            }
//        }
//
//        public StorageContainer OpenContainer(string containerName)
//        {
//            return new StorageContainer(this,containerName);
//        }
//		
//		public static StorageDevice ShowStorageDeviceGuide()
//		{
//			return new StorageDevice();
//		}
//    }
//}

#region Assembly Microsoft.Xna.Framework.Storage.dll, v4.0.30319
// C:\Program Files (x86)\Microsoft XNA\XNA Game Studio\v4.0\References\Windows\x86\Microsoft.Xna.Framework.Storage.dll
#endregion
using Microsoft.Xna.Framework;
using System;
using System.IO;

#if WINRT
using Windows.Storage;
#else
using System.Runtime.Remoting.Messaging;
#endif

namespace Microsoft.Xna.Framework.Storage
{
	
	// The delegate must have the same signature as the method
	// it will call asynchronously.
	public delegate StorageDevice ShowSelectorAsynchronousShow (PlayerIndex player, int sizeInBytes, int directoryCount);
	// The MonoTouch AOT cannot deal with nullable types in a delegate (or
	// at least not the straightforward implementation), so we define two
	// delegate types.
	public delegate StorageDevice ShowSelectorAsynchronousShowNoPlayer (int sizeInBytes, int directoryCount);

	// The delegate must have the same signature as the method
	// it will call asynchronously.
	public delegate StorageContainer OpenContainerAsynchronous (string displayName);
	
	// Summary:
	//     Represents a storage device for user data, such as a memory unit or hard
	//     drive. Reference page contains links to related conceptual articles.
	public sealed class StorageDevice
	{
		
		PlayerIndex? player;
		int sizeInBytes;
		int directoryCount;
		StorageContainer storageContainer;
		
		internal StorageDevice(PlayerIndex? player, int sizeInBytes, int directoryCount) 
		{
			this.player = player;
			this.sizeInBytes = sizeInBytes;
			this.directoryCount = directoryCount;
		}
		
		// Summary:
		//     Gets the amount of free space on the device.
		public long FreeSpace { 
			get { 
				// I do not know if the DriveInfo is is implemented on Mac or not
				// thus the try catch
				try {
#if WINRT
                    return long.MaxValue;
#else
                    return new DriveInfo(GetDevicePath).AvailableFreeSpace;
#endif
                }
				catch (Exception) {
					StorageDeviceHelper.Path = StorageRoot;
					return StorageDeviceHelper.FreeSpace;
				}
			} 
		}
		//
		// Summary:
		//     Gets whether the device is connected.
		public bool IsConnected { 
			get { 
				// I do not know if the DriveInfo is is implemented on Mac or not
				// thus the try catch
				try {
#if WINRT
                    return true;
#else
					return new DriveInfo(GetDevicePath).IsReady;
#endif
                }
				catch (Exception) {
					return true;
				}
			} 
		}
		//
		// Summary:
		//     Gets the total amount of space on the device.
		public long TotalSpace { 
			get { 
				
				// I do not know if the DriveInfo is is implemented on Mac or not
				// thus the try catch
				try {
#if WINRT
                    return long.MaxValue;
#else

					// Not sure if this should be TotalSize or TotalFreeSize
					return new DriveInfo(GetDevicePath).TotalSize;
#endif
                }
				catch (Exception) {
					StorageDeviceHelper.Path = StorageRoot;
					return StorageDeviceHelper.TotalSpace;
				}
					
			} 
		}
		
		string GetDevicePath
		{
			get {
				// We may not need to store the StorageContainer in the future
				// when we get DeviceChanged events working.
				if (storageContainer == null) {
					return StorageRoot;
				}
				else {
					return storageContainer._storagePath;
				}				
			}
		}
		// Summary:
		//     Occurs when a device is removed or inserted.
		//
		// Parameters:
		//   :
		// TODO: Implement DeviceChanged when we having the graphical implementation
		public static event EventHandler<EventArgs> DeviceChanged;

		// Summary:
		//     Begins the process for opening a StorageContainer containing any files for
		//     the specified title.
		//
		// Parameters:
		//   displayName:
		//     A constant human-readable string that names the file.
		//
		//   callback:
		//     An AsyncCallback that represents the method called when the operation is
		//     complete.
		//
		//   state:
		//     A user-created object used to uniquely identify the request, or null.
		public IAsyncResult BeginOpenContainer (string displayName, AsyncCallback callback, object state)
		{
			return OpenContainer(displayName, callback, state);

		}
		
		private IAsyncResult OpenContainer (string displayName, AsyncCallback callback, object state)
		{
			try {
				OpenContainerAsynchronous AsynchronousOpen = new OpenContainerAsynchronous (Open);
				return AsynchronousOpen.BeginInvoke (displayName, callback, state);
			} finally {
			}
		}
	
		// Private method to handle the creation of the StorageDevice
		private StorageContainer Open (string displayName) 
		{
			storageContainer = new StorageContainer(this, displayName, this.player);
			return storageContainer;
		}
		
		//
		// Summary:
		//     Begins the process for displaying the storage device selector user interface,
		//     and for specifying a callback implemented when the player chooses a device.
		//     Reference page contains links to related code samples.
		//
		// Parameters:
		//   callback:
		//     An AsyncCallback that represents the method called when the player chooses
		//     a device.
		//
		//   state:
		//     A user-created object used to uniquely identify the request, or null.
		public static IAsyncResult BeginShowSelector (AsyncCallback callback, object state)
		{
			return BeginShowSelector (0, 0, callback, state);
		}
		//
		// Summary:
		//     Begins the process for displaying the storage device selector user interface;
		//     specifies the callback implemented when the player chooses a device. Reference
		//     page contains links to related code samples.
		//
		// Parameters:
		//   player:
		//     The PlayerIndex that represents the player who requested the save operation.
		//     On Windows, the only valid option is PlayerIndex.One.
		//
		//   callback:
		//     An AsyncCallback that represents the method called when the player chooses
		//     a device.
		//
		//   state:
		//     A user-created object used to uniquely identify the request, or null.
		public static IAsyncResult BeginShowSelector (PlayerIndex player, AsyncCallback callback, object state)
		{
			return BeginShowSelector (player, 0, 0, callback, state);
		}
		//
		// Summary:
		//     Begins the process for displaying the storage device selector user interface,
		//     and for specifying the size of the data to be written to the storage device
		//     and the callback implemented when the player chooses a device. Reference
		//     page contains links to related code samples.
		//
		// Parameters:
		//   sizeInBytes:
		//     The size, in bytes, of data to write to the storage device.
		//
		//   directoryCount:
		//     The number of directories to write to the storage device.
		//
		//   callback:
		//     An AsyncCallback that represents the method called when the player chooses
		//     a device.
		//
		//   state:
		//     A user-created object used to uniquely identify the request, or null.
		public static IAsyncResult BeginShowSelector (int sizeInBytes, int directoryCount, AsyncCallback callback, object state)
		{
			var del = new ShowSelectorAsynchronousShowNoPlayer (Show);
			return del.BeginInvoke(sizeInBytes, directoryCount, callback, state);
		}
		
		//
		// Summary:
		//     Begins the process for displaying the storage device selector user interface,
		//     for specifying the player who requested the save operation, for setting the
		//     size of data to be written to the storage device, and for naming the callback
		//     implemented when the player chooses a device. Reference page contains links
		//     to related code samples.
		//
		// Parameters:
		//   player:
		//     The PlayerIndex that represents the player who requested the save operation.
		//     On Windows, the only valid option is PlayerIndex.One.
		//
		//   sizeInBytes:
		//     The size, in bytes, of the data to write to the storage device.
		//
		//   directoryCount:
		//     The number of directories to write to the storage device.
		//
		//   callback:
		//     An AsyncCallback that represents the method called when the player chooses
		//     a device.
		//
		//   state:
		//     A user-created object used to uniquely identify the request, or null.
		public static IAsyncResult BeginShowSelector (PlayerIndex player, int sizeInBytes, int directoryCount, AsyncCallback callback, object state)
		{
			var del = new ShowSelectorAsynchronousShow (Show);
			return del.BeginInvoke(player, sizeInBytes, directoryCount, callback, state);
		}
	
		// Private method to handle the creation of the StorageDevice
		private static StorageDevice Show (PlayerIndex player, int sizeInBytes, int directoryCount)
		{
			return new StorageDevice(player, sizeInBytes, directoryCount);
		}

		private static StorageDevice Show (int sizeInBytes, int directoryCount)
		{
			return new StorageDevice (null, sizeInBytes, directoryCount);
		}
		
		
		//
		//
		// Parameters:
		//   titleName:
		//     The name of the storage container to delete.
		public void DeleteContainer (string titleName)
		{
			throw new NotImplementedException ();

		}			
		//
		// Summary:
		//     Ends the process for opening a StorageContainer.
		//
		// Parameters:
		//   result:
		//     The IAsyncResult returned from BeginOpenContainer.
		public StorageContainer EndOpenContainer (IAsyncResult result)
		{
			StorageContainer returnValue = null;
			try {
				// Retrieve the delegate.
                var asyncResult = result.AsyncState as OpenContainerAsynchronous;

				// Wait for the WaitHandle to become signaled.
				result.AsyncWaitHandle.WaitOne ();

				// Call EndInvoke to retrieve the results.
                if (asyncResult != null)
                    returnValue = asyncResult.EndInvoke(result);
			} finally {
				// Close the wait handle.
				result.AsyncWaitHandle.Dispose ();	 
			}
			
			return returnValue;

		}			
		//
		// Summary:
		//     Ends the display of the storage selector user interface. Reference page contains
		//     links to related code samples.
		//
		// Parameters:
		//   result:
		//     The IAsyncResult returned from BeginShowSelector.
		public static StorageDevice EndShowSelector (IAsyncResult result) 
		{
#if WINRT
            throw new NotImplementedException();
#else


			// Retrieve the delegate.
			AsyncResult asyncResult = (AsyncResult)result;

			if (!result.IsCompleted) {
				// Wait for the WaitHandle to become signaled.
				try {
					result.AsyncWaitHandle.WaitOne ();
				} finally {
					result.AsyncWaitHandle.Close ();
				}
			}

			var del = asyncResult.AsyncDelegate;

			if (del is ShowSelectorAsynchronousShow)
				return (del as ShowSelectorAsynchronousShow).EndInvoke (result);
			else if (del is ShowSelectorAsynchronousShowNoPlayer)
				return (del as ShowSelectorAsynchronousShowNoPlayer).EndInvoke (result);
			else
				throw new ArgumentException ("result");
#endif
		}
		
		internal static string StorageRoot
		{
			get {
#if WINRT
                return ApplicationData.Current.LocalFolder.Path; 
#else
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
            }
		}
	}
}
