using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Windows.System.Threading;
using System.Collections.Generic;

namespace SaladFuzzTester
{
    // Mono doesn't seem to support blocking (sync) socket operations to we simulate them.
    public class FuzzTesterHelpers
    {
        #region PublicInterface

        // Event message which can be sent to fuzz tester
        public enum FuzzTesterEvent
        {
            // [0,255] RANGE ONLY SUPPORTED NOW BECAUSE 1 BYTE IS SENT!
            PingFromUiThread = 1,
            PingFromGameThread = 2
        }

        public static int GetPingSendIntervalMillis()
        {
            return 3000; // MUST BE MUCH LARGER THAN FUZZ TESTER PING TASK SLEEP AMOUNT FOR THE PING RECIEVER THREAD!!!
        }

        // Creates socket, sends message, closes socket, so no need to handle anything else.
        public static void SendMessageToFuzzTesterBlocking2(FuzzTesterEvent message)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1];

            try
            {
                int port = 43151;
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                connectBlocking(socket, remoteEP);

                sendBlocking(socket, BitConverter.GetBytes((byte)message));

                socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                showErrorDialog("Error: exception trying to send data to Fuzz tester: " + e.ToString());
            }
        }

        class UserData
        {
            public byte[] msg;
            public string timestamp;
        }
        public static void SendMessageToFuzzTesterAsync(FuzzTesterEvent message)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1];

            try
            {
                int port = 43151;
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
                eventArgs.RemoteEndPoint = remoteEP;
                eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onConnectedAsync);
                UserData ud = new UserData();
                ud.msg = BitConverter.GetBytes((byte)message);
                ud.timestamp = DateTime.Now.ToString("HH:mm:ss.ff");
                eventArgs.UserToken = ud;
              
                // returns false if executed sync OR returns true if async (in which case we must wait for callback).
                if (!socket.ConnectAsync(eventArgs))
                {
                    onConnectedAsync(socket, eventArgs);
                }                
            }
            catch (Exception e)
            {
                showErrorDialog("Error: exception trying to send data to Fuzz tester: " + e.ToString());
            }
        }

        // Starts the given task which pings the given message periodicaly. You can call this function multiple times for the 
        // same FuzzTesterEvent value, but if it's already running for that message it won't start again so its safe to call it
        // multiple times with the same FuzzTesterEvent value.
        /*  public static void StartPingTaskOnce(
              FuzzTesterEvent pingMessage,
              int pingPeriodMillis = 2000 // MUST BE MUCH LARGER THAN THE PERIOD IN WHICH THE FUZZ TESTER SLEEPS PING THREADS!
              )
          {
              lock (_startedPingTasks)
              {
                  // return if already added
                  if (_startedPingTasks.Contains(pingMessage))
                  {
                      return;
                  }

                  _startedPingTasks.Add(pingMessage);

                  // start the actual task
                  startPingTask(pingMessage, pingPeriodMillis);
              }
          }*/

        #endregion

        #region Implementation

        static void onConnectedAsync(object sender, SocketAsyncEventArgs e)
        {
            UserData ud = (UserData)e.UserToken;
            byte[] dataToSend = (byte[])ud.msg;
            logToFileBlocking("onConnectedAsync 1 from: " + ud.timestamp);

            //  logToFileBlocking("onConnectedAsync 1");
            if (e.LastOperation != SocketAsyncOperation.Connect || e.SocketError != SocketError.Success)
            {
                showErrorDialog("Failed to connect to Fuzz Tester, onConnectedAsync. Last operation: " + e.LastOperation.ToString() + ", Socket status: " + e.SocketError.ToString()+", data: "+ dataToSend[0]);
            }
            /*
             TODO
            THESE TWO THINGS COULD HAPPEN:
             1) GAME THREAD FREEZA, ENABLE LOGGERS WHEN WE PING FIZZ TESTER
             2) FUZZ TESTER LISTENER LOOP FREEZES, MAKE IT ASYNC + LOG IT
          */
            SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
            eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onDataSentAsync);
            eventArgs.SetBuffer(dataToSend, 0, dataToSend.Length);
            eventArgs.UserToken = e.UserToken;
          //  logToFileBlocking("onConnectedAsync 2");
            Socket newSocket = (Socket)sender;
            logToFileBlocking("onConnectedAsync 2 from: " + ud.timestamp);

            if (!newSocket.SendAsync(eventArgs))
            {
                onDataSentAsync(newSocket, eventArgs);
            }

          //  logToFileBlocking("onConnectedAsync end");
        }

        // required callback because socket is async so we know when its done.
        static async void onDataSentAsync(object sender, SocketAsyncEventArgs e)
        {
            UserData ud = (UserData)e.UserToken;

             logToFileBlocking("onDataSentAsync 1 at: " + ud.timestamp);

            //logToFileBlocking("onDataSentAsync 1");
            if (e.LastOperation != SocketAsyncOperation.Send || e.SocketError != SocketError.Success)
            {
                showErrorDialog("Failed to send data to Fuzz Tester. Last operation: " + e.LastOperation.ToString() + ", Socket status: " + e.SocketError.ToString());
            }
            // logToFileBlocking("onDataSentAsync 2");
            try
            {

                Socket newSocket = (Socket)sender;
                if(newSocket.Connected == false)
                {
                    //newSocket.Shutdown(SocketShutdown.Both);
                    newSocket.Dispose();
                    logToFileBlocking("ERROR: socket failed to connect & send");
                    throw new Exception("ERROR: socket failed to connect & send");
                }
                logToFileBlocking("onDataSentAsync 2 at: " + ud.timestamp);

                await Task.Delay(1000); // for sanity before we kill it just in case

                newSocket.Dispose();

            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                logToFileBlocking(s);
            }
            
        }

        // stores ping tasks that were already started so that we cannot start multiple ping tasks for the same FuzzTesterEvent value.
        static HashSet<FuzzTesterEvent> _startedPingTasks = new HashSet<FuzzTesterEvent>();

        // TODO: REMOVE BLOCKING FUNCTIONS
        static void connectBlocking(Socket socket, IPEndPoint remoteEP)
        {
            SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
            eventArgs.RemoteEndPoint = remoteEP;
            eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onConnected);
            OperationResult opResult = new OperationResult();
            eventArgs.UserToken = opResult;

            // create timeout task because we can't seem to set it otherwise due to mono
            CancellationTokenSource cancelTimeoutTask = CreateTimeoutWaitTask(7000, "ERROR: failed to connect to Fuzz tester");

            // returns false if executed sync OR returns true if async (in which case we must wait for callback).
            bool isWaiting = socket.ConnectAsync(eventArgs);

            // wait until done (in case it executed async)
            while (isWaiting == true && opResult.isOperationComplete == false) ;

            cancelTimeoutTask.Cancel(); // notify that the operation has completed successfuly in time, so cancel the timeout task
        }

        static void sendBlocking(Socket socket, byte[] dataToSend)
        {
            SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
            eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onDataSent);
            OperationResult opResult = new OperationResult();
            eventArgs.UserToken = opResult;
            eventArgs.SetBuffer(dataToSend, 0, dataToSend.Length);

            // create timeout task because we can't seem to set it otherwise due to mono
            CancellationTokenSource cancelTimeoutTask = CreateTimeoutWaitTask(7000, "ERROR: failed to send data to Fuzz tester");

            // returns false if executed sync OR returns true if async (in which case we must wait for callback).
            logToFileBlocking("sending: " + dataToSend[0]);
            bool isWaiting = socket.SendAsync(eventArgs);

            // wait until done (in case it executed async)
            while (isWaiting == true && opResult.isOperationComplete == false) ;

            cancelTimeoutTask.Cancel(); // notify that the operation has completed successfuly in time, so cancel the timeout task
        }

        // required callback because socket is async so we know when its done.
        static void onConnected(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Connect || e.SocketError != SocketError.Success)
            {
                showErrorDialog("Failed to connect to Fuzz Tester. Last operation: " + e.LastOperation.ToString() + ", Socket status: " + e.SocketError.ToString());
            }

            OperationResult opRes = (OperationResult)e.UserToken;
            opRes.isOperationComplete = true;
        }

        // required callback because socket is async so we know when its done.
        static void onDataSent(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Send || e.SocketError != SocketError.Success)
            {
                showErrorDialog("Failed to send data to Fuzz Tester. Last operation: " + e.LastOperation.ToString() + ", Socket status: " + e.SocketError.ToString());
            }

            OperationResult opRes = (OperationResult)e.UserToken;
            opRes.isOperationComplete = true;
        }

        // WARNING: pingPeriodMillis must be much larger than the fuzz tester ping checker thread sleep amount or messages could start to queue up!
        static void startPingTask(FuzzTesterEvent eventToPing, int pingPeriodMillis)
        {
            CancellationToken tokenSource = new CancellationToken();
            Task.Factory.StartNew(
                  async () =>
                  {
                      while (true)
                      {
                          if (tokenSource.IsCancellationRequested)
                          {
                              return;
                          }

                          SendMessageToFuzzTesterBlocking2(eventToPing);
                          await Task.Delay(pingPeriodMillis, tokenSource); // wait
                      }
                  },
                  tokenSource,
                  TaskCreationOptions.LongRunning,
                  TaskScheduler.Default);
        }
        #endregion

        #region MiscHelpers

        // This does not work if the UI thread is blocked, no dialog will be shown!
        static void showErrorDialog(string message)
        {
            string logLine = DateTime.Now.ToString("HH:mm:ss.ff") + ": " + message;

            logToFileBlocking("ERROR Dialog: " + logLine);

            // must run on UI thread
            Windows.Foundation.IAsyncAction action = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                 Windows.UI.Core.CoreDispatcherPriority.High,
              () =>
              {
                  var dialog = new Windows.UI.Popups.MessageDialog(logLine);
                  var a = dialog.ShowAsync();
              });

            throw new Exception("ERROR Dialog: " + logLine);     
        }

        // Sent as user data object to socket async method which sets the flag when the operation completed.
        class OperationResult
        {
            public volatile bool isOperationComplete = false;
        }

        // Creates and starts a wait timer. Use the returned cancellationTokenSource.Cancel() to finish the task
        // if the operation succeeded successfuly. If the task is not canceled in the given time it logs error and tries to throw
        // exception (which does not seem to be triggered if UI thread blocks.)
        public static CancellationTokenSource CreateTimeoutWaitTask(int maxTimeToWaitMillis, string errorLogMessage)
        {
            DateTime operationStartTime = DateTime.Now;
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            TaskCreationOptions taskFlags = TaskCreationOptions.None;
            Task.Factory.StartNew(
              async () =>
              {
                  while (true)
                  {
                      if (tokenSource.IsCancellationRequested || tokenSource.Token.IsCancellationRequested)
                      {
                          return;
                      }

                      if ((DateTime.Now - operationStartTime) > TimeSpan.FromMilliseconds(maxTimeToWaitMillis))
                      {
                          logToFileBlocking(errorLogMessage);
                          throw new Exception(errorLogMessage); // is not thrown because UI thread blocks lol
                      }

                      // sleep a bit to not use CPU
                      await Task.Delay(TimeSpan.FromMilliseconds(300));
                  }

              },
                tokenSource.Token, taskFlags, System.Threading.Tasks.TaskScheduler.Default);

            return tokenSource;
        }


        static object loggerWriteLock = new object(); // to prevent 2 threads writing/reading at same time

        // Writes to C:\Users\<username>\AppData\Local\Packages\<app_package_name>\LocalState\FuzzTester.txt
        public static void logToFileBlocking(string logMessage)
        {
            
            int threadId = Environment.CurrentManagedThreadId;

            WorkItemHandler workItemHandler = action =>
            {
                lock (loggerWriteLock)
                {
                    // Create sample file; replace if exists.
                    Windows.Storage.StorageFolder storageFolder =
                        Windows.Storage.ApplicationData.Current.LocalFolder;

                    Windows.Foundation.IAsyncOperation<Windows.Storage.StorageFile> sampleFile =
                         storageFolder.CreateFileAsync("FuzzTester.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);

                    Task<Windows.Storage.StorageFile> ft = sampleFile.AsTask();

                    ft.Wait();
                    Windows.Storage.StorageFile f = ft.Result;

                    String timeStamp = DateTime.Now.ToString("HH:mm:ss.ff");
                    Windows.Foundation.IAsyncAction aa = Windows.Storage.FileIO.AppendTextAsync(f, timeStamp + ": " + " thread: " + threadId + " : "+ logMessage + "\r\n"); ;
                    while (aa.Status != Windows.Foundation.AsyncStatus.Completed) ;
                }

            };

            Windows.Foundation.IAsyncAction a = ThreadPool.RunAsync(workItemHandler, WorkItemPriority.High, WorkItemOptions.None);
            while (a.Status != Windows.Foundation.AsyncStatus.Completed) ;
            
        }

        #endregion
    }
}
