using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


//Non Core assemblies
#if WINDOWS_PHONE
using System.IO.IsolatedStorage; 
#elif WINRT
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
#elif IOS
using MonoTouch.Foundation;
#elif MONOMAC
using MonoMac.Foundation;
#endif

namespace Microsoft.Xna.Framework.Utilities
{
	internal static class FileHelpers
	{
		#region internal properties
#if WINDOWS_PHONE
		static IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
#elif ANDROID
		static Dictionary<string, string[]> filesInFolders = new Dictionary<string,string[]>();
#endif
		#endregion

		#region public properties
		
		#if WINRT
		public static char notSeparator = '/';
		public static char separator = '\\';
#else
		public static char notSeparator = '\\';
		public static char separator = Path.DirectorySeparatorChar;
#endif
		#endregion

		#region File Handlers

		public static Stream FileOpen(string filePath, string fileMode, string fileAccess, string fileShare)
		{
			return FileOpen(filePath, (FileMode)Enum.Parse(typeof(FileMode), fileMode, true), (FileAccess)Enum.Parse(typeof(FileAccess), fileAccess, false), (FileShare)Enum.Parse(typeof(FileShare), fileShare, false));
		}

		public static Stream FileOpen(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
#if WINDOWS_STOREAPP
			var folder = ApplicationData.Current.LocalFolder;
			if (fileMode == FileMode.Create || fileMode == FileMode.CreateNew)
			{
				return folder.OpenStreamForWriteAsync(filePath, CreationCollisionOption.ReplaceExisting).GetAwaiter().GetResult();
			}
			else if (fileMode == FileMode.OpenOrCreate)
			{
				if (fileAccess == FileAccess.Read)
					return folder.OpenStreamForReadAsync(filePath).GetAwaiter().GetResult();
				else
				{
					// Not using OpenStreamForReadAsync because the stream position is placed at the end of the file, instead of the beginning
					var f = folder.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists).AsTask().GetAwaiter().GetResult();
					return f.OpenAsync(FileAccessMode.ReadWrite).AsTask().GetAwaiter().GetResult().AsStream();
				}
			}
			else if (fileMode == FileMode.Truncate)
			{
				return folder.OpenStreamForWriteAsync(filePath, CreationCollisionOption.ReplaceExisting).GetAwaiter().GetResult();
			}
			else
			{
				// Not using OpenStreamForReadAsync because the stream position is placed at the end of the file, instead of the beginning
				folder.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists).AsTask().GetAwaiter().GetResult().OpenAsync(FileAccessMode.ReadWrite).AsTask().GetAwaiter().GetResult().AsStream();
				var f = folder.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists).AsTask().GetAwaiter().GetResult();
				return f.OpenAsync(FileAccessMode.ReadWrite).AsTask().GetAwaiter().GetResult().AsStream();
			}
#else
			return File.Open(filePath, fileMode, fileAccess, fileShare);
#endif
		}

		public static Stream FileOpenRead(string Location, string safeName)
		{
#if WINDOWS_PHONE
			return storage.OpenFile(safeName, FileMode.Open, FileAccess.Read);
#elif WINRT
			var stream = Task.Run( () => FileHelpers.OpenStreamAsync(safeName).Result ).Result;
			if (stream == null)
				throw new FileNotFoundException(safeName);

			return stream;
#elif ANDROID
			return Game.Activity.Assets.Open(safeName);
#elif IOS
			var absolutePath = Path.Combine(Location, safeName);
			if (TitleContainer.SupportRetina)
			{
				// Insert the @2x immediately prior to the extension. If this file exists
				// and we are on a Retina device, return this file instead.
				var absolutePath2x = Path.Combine(Path.GetDirectoryName(absolutePath),
												  Path.GetFileNameWithoutExtension(absolutePath)
												  + "@2x" + Path.GetExtension(absolutePath));
				if (File.Exists(absolutePath2x))
					return File.OpenRead(absolutePath2x);
			}
			return File.OpenRead(absolutePath);
#else
			var absolutePath = Path.Combine(Location, safeName);
			return File.OpenRead(absolutePath);
#endif
		}

		public static bool FileExists(string fileName)
		{
#if WINDOWS_PHONE
			if(storage.FileExists(fileName))
				return true;
#elif WINRT
			var result = Task.Run(async () =>
			{
				try
				{
					var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(fileName);
					return file == null ? false : true;
				}
				catch (FileNotFoundException)
				{
					return false;
				}
			}).Result;

			if (result)
			{
				return true;
			}

#elif ANDROID
			int index = fileName.LastIndexOf(Path.DirectorySeparatorChar);
			string path = string.Empty;
			string file = fileName;
			if (index >= 0)
			{
				file = fileName.Substring(index + 1, fileName.Length - index - 1);
				path = fileName.Substring(0, index);
			}

			string[] files = DirectoryGetFiles(path);

			if (files.Any(s => s.ToLower() == file.ToLower()))
				return true;
#else
			if (File.Exists(fileName))
				return true;
#endif
			return false;
		}

		public static Stream FileCreate(string filePath)
		{
#if WINDOWS_STOREAPP
			var folder = ApplicationData.Current.LocalFolder;
			var awaiter = folder.OpenStreamForWriteAsync(filePath, CreationCollisionOption.ReplaceExisting).GetAwaiter();
			return awaiter.GetResult();
#elif WINDOWS_PHONE
			return storage.CreateFile(filePath);
#else
			return File.Create(filePath);
#endif
		}

		public static void FileDelete(string filePath)
		{
#if WINDOWS_STOREAPP
			var folder = ApplicationData.Current.LocalFolder;
			var deleteFile = folder.GetFileAsync(filePath).AsTask().GetAwaiter().GetResult();
			deleteFile.DeleteAsync().AsTask().Wait();
#elif WINDOWS_PHONE
			storage.DeleteFile(filePath);
#else
			File.Delete(filePath);
#endif
		}

		public static string NormalizeFilename(string fileName, string[] extensions)
		{

			if (FileExists(fileName))
				return fileName;

			if (!string.IsNullOrEmpty(Path.GetExtension(fileName)))
				return null;

			foreach (string ext in extensions)
			{
				string fileNamePlusExt = fileName + ext;

				if (FileExists(fileNamePlusExt))
					return fileNamePlusExt;
			}

			return null;
		}

		// Renamed from - public static string GetFilename(string name)
		public static string NormalizeFilePathSeperators(string name)
		{
#if WINRT
			name = name.Replace('/', '\\');
#else
			name = name.Replace('\\', Path.DirectorySeparatorChar);
#endif
			return name;
		}



		#region File Handler reciprocal overloads

		public static Stream FileOpenRead(object storageFile, string Location, string safeName)
		{
			if (storageFile == null)
			{
				throw new NullReferenceException("Must supply a storageFile reference");
			}

#if WINDOWS_PHONE
			storage = (IsolatedStorageFile)storageFile;
#endif

			return FileOpenRead(Location, safeName);

		}

		public static bool FileExists(object storageFile, string fileName)
		{
			if (storageFile == null)
			{
				throw new NullReferenceException("Must supply a storageFile reference");
			}

#if WINDOWS_PHONE
			storage = (IsolatedStorageFile)storageFile;
#endif

			return FileExists(fileName);
		}

		public static Stream FileCreate(object storageFile, string filePath)
		{
			if (storageFile == null)
			{
				throw new NullReferenceException("Must supply a storageFile reference");
			}

#if WINDOWS_PHONE
			storage = (IsolatedStorageFile)storageFile;
#endif

			return FileCreate(filePath);
		}

		public static void FileDelete(object storageFile, string filePath)
		{
			if (storageFile == null)
			{
				throw new NullReferenceException("Must supply a storageFile reference");
			}

#if WINDOWS_PHONE
			storage = (IsolatedStorageFile)storageFile;
#endif

			FileDelete(filePath);
		}

		#endregion

		#endregion

		#region Directory Handlers

		public static bool DirectoryExists(string dirPath)
		{
#if WINDOWS_STOREAPP
			var folder = ApplicationData.Current.LocalFolder;

			try
			{
				var result = folder.GetFolderAsync(dirPath).GetResults();
				return result != null;
			}
			catch
			{
				return false;
			}
#elif WINDOWS_PHONE
			return storage.DirectoryExists(dirPath);
#else
			return Directory.Exists(dirPath);
#endif
		}

		public static string[] DirectoryGetFiles(string storagePath)
		{
#if WINDOWS_STOREAPP
			var folder = ApplicationData.Current.LocalFolder;
			var results = folder.GetFilesAsync().AsTask().GetAwaiter().GetResult();
			return results.Select<StorageFile, string>(e => e.Name).ToArray();
#elif WINDOWS_PHONE
			return storage.GetFileNames(storagePath);
#elif ANDROID
			string[] files = null;
			if (!filesInFolders.TryGetValue(storagePath, out files))
			{
				files = Game.Activity.Assets.List(storagePath);
				filesInFolders[storagePath] = files;
			}
			return filesInFolders[storagePath];
#else
			return Directory.GetFiles(storagePath);
#endif
		}

		public static string[] DirectoryGetFiles(string storagePath, string searchPattern)
		{
			if (string.IsNullOrEmpty(searchPattern))
				throw new ArgumentNullException("Parameter searchPattern must contain a value.");

#if WINDOWS_STOREAPP
			var folder = ApplicationData.Current.LocalFolder;
			var options = new QueryOptions( CommonFileQuery.DefaultQuery, new [] { searchPattern } );
			var query = folder.CreateFileQueryWithOptions(options);
			var files = query.GetFilesAsync().AsTask().GetAwaiter().GetResult();
			return files.Select<StorageFile, string>(e => e.Name).ToArray();
#else
			return Directory.GetFiles(storagePath, searchPattern);
#endif
		}

		public static string[] DirectoryGetDirectories(string storagePath)
		{
#if WINDOWS_STOREAPP
			var folder = ApplicationData.Current.LocalFolder;
			var results = folder.GetFoldersAsync().AsTask().GetAwaiter().GetResult();
			return results.Select<StorageFolder, string>(e => e.Name).ToArray();
#else
			return Directory.GetDirectories(storagePath);
#endif
		}

		public static string[] DirectoryGetDirectories(string storagePath, string searchPattern)
		{
			throw new NotImplementedException();
		}

		public static void DirectoryCreate(string directory)
		{
			if (string.IsNullOrEmpty(directory))
				throw new ArgumentNullException("Parameter directory must contain a value.");

#if WINDOWS_STOREAPP
			var folder = ApplicationData.Current.LocalFolder;
			var task = folder.CreateFolderAsync(directory, CreationCollisionOption.OpenIfExists);
			task.AsTask().Wait();
#else
			Directory.CreateDirectory(directory);
#endif
		}

		/// <summary>
		/// Creates a new directory in the storage-container.
		/// </summary>
		/// <param name="directory">Directory to be created.</param>
		/// <param name="storagePath">Relative Storage path where the directory created.</param>
		public static void DirectoryCreate(string directory, string storagePath)
		{
			if (string.IsNullOrEmpty(directory))
				throw new ArgumentNullException("Parameter directory must contain a value.");

			var dirPath = Path.Combine(storagePath, directory);

			DirectoryCreate(dirPath);
		}

		public static void DirectoryDelete(string dirPath)
		{
#if WINDOWS_STOREAPP
			var folder = ApplicationData.Current.LocalFolder;
			var deleteFolder = folder.GetFolderAsync(dirPath).AsTask().GetAwaiter().GetResult();
			deleteFolder.DeleteAsync().AsTask().Wait();
#else
			Directory.Delete(dirPath);
#endif
		}
		
		public static string GetInstallPath()
		{
			string Location = string.Empty;
#if WINDOWS || LINUX
			Location = AppDomain.CurrentDomain.BaseDirectory;
#elif WINRT
			Location = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
#elif IOS || MONOMAC
			Location = NSBundle.MainBundle.ResourcePath;
#elif PSM
			Location = "/Application";
#else
			Location = string.Empty;
#endif
			return Location;
		}

		#region Directory Handler reciprocal overloads

		public static string[] DirectoryGetFiles(object storageFile, string storagePath)
		{
			if (storageFile == null)
			{
				throw new NullReferenceException("Must supply a storageFile reference");
			}

#if WINDOWS_PHONE
			storage = (IsolatedStorageFile)storageFile;
#endif

			return DirectoryGetFiles(storagePath);
		}

		public static bool DirectoryExists(string dirPath, out object storageFile)
		{

#if WINDOWS_PHONE
			storageFile = storage;
			if (storage == null)
			{
				return false;
			}
#else
			storageFile = null;
#endif

			return DirectoryExists(dirPath);
		}

		#endregion

		#endregion

		#region Stream Handlers

		public static Stream OpenStream(string rootDirectory, string assetName, string extension)
		{
			Stream stream;
			try
			{
				string assetPath = Path.Combine(rootDirectory, assetName) + extension;
				stream = TitleContainer.OpenStream(assetPath);
#if ANDROID
				SeekStreamtoStart(stream, 0);
#else
				SeekStreamtoStart(stream);
#endif
			}
			catch (FileNotFoundException fileNotFound)
			{
				throw new ContentLoadException("The content file was not found.", fileNotFound);
			}
#if !WINRT
			catch (DirectoryNotFoundException directoryNotFound)
			{
				throw new ContentLoadException("The directory was not found.", directoryNotFound);
			}
#endif
			catch (Exception exception)
			{
				throw new ContentLoadException("Opening stream error.", exception);
			}
			return stream;
		}

		public static Stream SeekStreamtoStart(Stream stream, long StartPos, out long pos)
		{
#if ANDROID
				// Android native stream does not support the Position property. LzxDecoder.Decompress also uses
				// Seek.  So we read the entirety of the stream into a memory stream and replace stream with the
				// memory stream.
				MemoryStream memStream = new MemoryStream();
				stream.CopyTo(memStream);
				memStream.Seek(0, SeekOrigin.Begin);
				stream.Dispose();
				stream = memStream;
				pos = 0;
#else
				pos = StartPos;
#endif
				return stream;
		}

		public static void StreamClose(Stream stream)
		{
#if !WINRT
			stream.Close();
#endif
		}

#if !WINDOWS_PHONE && WINRT 

		public static async Task<Stream> OpenStreamAsync(string name)
		{
			var package = Windows.ApplicationModel.Package.Current;

			try
			{
				var storageFile = await package.InstalledLocation.GetFileAsync(name);
				var randomAccessStream = await storageFile.OpenReadAsync();
				return randomAccessStream.AsStreamForRead();
			}
			catch (IOException)
			{
				return null;
			}
		}

#endif

		#region Stream Handler reciprocal overloads

		public static Stream SeekStreamtoStart(Stream stream)
		{
			long StartPos = stream.Position;
			return SeekStreamtoStart(stream, StartPos, out StartPos);
		}

		public static Stream SeekStreamtoStart(Stream stream, long StartPos)
		{
			return SeekStreamtoStart(stream, StartPos, out StartPos);
		}


		#endregion

		#endregion

	}
}
