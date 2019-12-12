using System;
using System.Runtime.InteropServices;

namespace MonoGame.Utilities
{
    static class SignalWatcher
    {
        public static bool IsExiting;
        private static Action _action;

        private const int SIGERR = -1;
        private const int SIGINT = 2;
        private const int SIGTERM = 15;

        private delegate void sighandler_t(int signal);

        [DllImport("libc")]
        private static extern int signal(int signum, sighandler_t handler);

        private static void SignalHandler(int signal)
        {
            IsExiting = true;
            _action.Invoke();
        }

        public static void Init(Action action)
        {
            _action = action;

            if (CurrentPlatform.OS == OS.Linux && Type.GetType ("Mono.Runtime") != null)
            {
                // This only works when running with mono on Linux
                // .NET Core mapped its SIGTERM and SIGINT events
                // to ProcessExit and CancelKeyPress respectively

                if (signal(SIGTERM, SignalHandler) == SIGERR)
                {
                    Console.WriteLine("Failed to capture: SIGTERM");
                }

                if (signal(SIGINT, SignalHandler) == SIGERR)
                {
                    Console.WriteLine("Failed to capture: SIGINT");
                }
            }
            else
            {
                AppDomain.CurrentDomain.ProcessExit += (o, e) => _action.Invoke();
                Console.CancelKeyPress += (o, e) => _action.Invoke();
            }
        }
    }
}
