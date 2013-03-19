using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;

namespace MonoGame.Tests {
	public class FileServer : IDisposable {
		private readonly HttpListener _listener;

		public FileServer ()
		{
			_listener = new HttpListener();
		}

		~FileServer()
		{
			Dispose (false);
		}

		public ICollection<string> Prefixes {
			get { return _listener.Prefixes; }
		}

		public void Start()
		{
			_listener.Start ();
			_listener.BeginGetContext (ProcessContext, null);
		}

		public void Stop()
		{
			_listener.Stop ();
		}

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				_listener.Close ();
			}
		}

		#endregion

		private void ProcessContext (IAsyncResult result)
		{
			HttpListenerContext context;
			try {
				context = _listener.EndGetContext (result);
			} catch {
				return;
			}

			_listener.BeginGetContext (ProcessContext, null);

			Paths.SetStandardWorkingDirectory ();
			var workingDirUri = new Uri (Path.GetFullPath ("." + Path.DirectorySeparatorChar));

			var fileUri = new Uri(workingDirUri, context.Request.Url.AbsolutePath.TrimStart('/'));
			var fileInfo = new FileInfo(fileUri.LocalPath);

			if (!fileInfo.Exists) {
				var dirInfo = new DirectoryInfo(fileUri.LocalPath);
				if (!dirInfo.Exists) {
					context.Response.StatusCode = 404;
					using (var writer = new StreamWriter(context.Response.OutputStream)) {
						writer.WriteLine("Not found.");
					}
					return;
				}

				context.Response.StatusCode = 200;
				context.Response.ContentType = "text/plain";
				using (var writer = new StreamWriter(context.Response.OutputStream)) {

					Uri baseUri;
					var requestUrlString = context.Request.Url.ToString ();
					if (!requestUrlString.EndsWith("/"))
						baseUri = new Uri(requestUrlString + "/");
					else
						baseUri = context.Request.Url;
					foreach (var fsInfo in dirInfo.GetFileSystemInfos()) {
						if ((fsInfo.Attributes & FileAttributes.Directory) != 0) {
							writer.Write ("d: ");
						} else {
							writer.Write ("f: ");
						}
						writer.WriteLine (new Uri(baseUri, fsInfo.Name).ToString());
					}
				}
				return;
			}

			string contentType = MediaTypeNames.Application.Octet;
			switch (fileInfo.Extension.ToLowerInvariant()) {
			case ".png":
				contentType = "image/png";
				break;
			case ".gif":
				contentType = "image/gif";
				break;
			case ".jpg": case ".jpeg":
				contentType = "image/jpeg";
				break;
			case ".txt":
				contentType = "text/plain";
				break;
			case ".htm": case ".html":
				contentType = "text/html";
				break;
			case ".xml":
				contentType = "text/xml";
				break;
			}

			context.Response.StatusCode = 200;
			context.Response.ContentType = contentType;
			context.Response.ContentLength64 = fileInfo.Length;
			using (var input = fileInfo.OpenRead())
			{
				byte [] buffer = new byte [32 * 1024];
				int bytesRead;
				do {
					bytesRead = input.Read (buffer, 0, buffer.Length);
					context.Response.OutputStream.Write (buffer, 0, bytesRead);
				} while (bytesRead > 0);
			}
			context.Response.OutputStream.Close ();
		}
	}
}

