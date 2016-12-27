using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Net.Messages;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net
{
    internal delegate NetworkSession AsyncCreate(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties);
    internal delegate AvailableNetworkSessionCollection AsyncFind(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties);
    internal delegate NetworkSession AsyncJoin(AvailableNetworkSession availableSession);

    public sealed partial class NetworkSession : ISessionBackendListener, IDisposable, IMessageQueue
    {
        private static NetworkSession Session;
        private static AsyncCreate AsyncCreateCaller;
        private static AsyncFind AsyncFindCaller;
        private static AsyncJoin AsyncJoinCaller;

        // Asynchronous session creation
        public static IAsyncResult BeginCreate(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, AsyncCallback callback, Object asyncState)
        {
            if (Session != null || AsyncCreateCaller != null || AsyncFindCaller != null || AsyncJoinCaller != null)
            {
                throw new InvalidOperationException("Only one NetworkSession allowed");
            }
            if (maxGamers < MinSupportedGamers || maxGamers > MaxSupportedGamers)
            {
                throw new ArgumentOutOfRangeException("maxGamers must be in the range [" + MinSupportedGamers + ", " + MaxSupportedGamers + "]");
            }
            if (privateGamerSlots < 0 || privateGamerSlots > maxGamers)
            {
                throw new ArgumentOutOfRangeException("privateGamerSlots must be in the range [0, maxGamers]");
            }
            if (sessionProperties == null)
            {
                sessionProperties = new NetworkSessionProperties();
            }

            AsyncCreateCaller = new AsyncCreate(NetworkSessionImplementation.SessionCreator.Create);

            try
            {
                return AsyncCreateCaller.BeginInvoke(sessionType, localGamers, maxGamers, privateGamerSlots, sessionProperties, callback, asyncState);
            }
            catch
            {
                AsyncCreateCaller = null;
                throw;
            }
        }

        public static IAsyncResult BeginCreate(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, AsyncCallback callback, Object asyncState)
        {
            if (maxLocalGamers < MinSupportedLocalGamers || maxLocalGamers > MaxSupportedLocalGamers)
            {
                throw new ArgumentOutOfRangeException("maxLocalGamers must be in the range [" + MinSupportedLocalGamers + ", " + MaxSupportedLocalGamers + "]");
            }

            List<SignedInGamer> localGamers = new List<SignedInGamer>(SignedInGamer.SignedInGamers);
            if (localGamers.Count > maxLocalGamers)
            {
                localGamers.RemoveRange(maxLocalGamers, localGamers.Count - maxLocalGamers);
            }

            try { return BeginCreate(sessionType, localGamers, maxGamers, privateGamerSlots, sessionProperties, callback, asyncState); }
            catch { throw; }
        }

        public static IAsyncResult BeginCreate(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, AsyncCallback callback, Object asyncState)
        {
            try { return BeginCreate(sessionType, maxLocalGamers, maxGamers, 0, null, callback, asyncState); }
            catch { throw; }
        }

        public static NetworkSession EndCreate(IAsyncResult result)
        {
            try
            {
                Session = AsyncCreateCaller.EndInvoke(result);
                AsyncCreateCaller = null;
                return Session;
            }
            catch
            {
                AsyncCreateCaller = null;
                throw;
            }
        }

        public static IAsyncResult BeginFind(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties, AsyncCallback callback, Object asyncState)
        {
            if (Session != null || AsyncCreateCaller != null || AsyncFindCaller != null || AsyncJoinCaller != null)
            {
                throw new InvalidOperationException("Only one NetworkSession allowed");
            }
            if (sessionType == NetworkSessionType.Local)
            {
                throw new ArgumentException("Find cannot be used with NetworkSessionType.Local");
            }
            if (searchProperties == null)
            {
                searchProperties = new NetworkSessionProperties();
            }

            AsyncFindCaller = new AsyncFind(NetworkSessionImplementation.SessionCreator.Find);

            try
            {
                return AsyncFindCaller.BeginInvoke(sessionType, localGamers, searchProperties, callback, asyncState);
            }
            catch
            {
                AsyncFindCaller = null;
                throw;
            }
        }

        public static IAsyncResult BeginFind(NetworkSessionType sessionType, int maxLocalGamers, NetworkSessionProperties searchProperties, AsyncCallback callback, Object asyncState)
        {
            if (maxLocalGamers < MinSupportedLocalGamers || maxLocalGamers > MaxSupportedLocalGamers)
            {
                throw new ArgumentOutOfRangeException("maxLocalGamers must be in the range [" + MinSupportedLocalGamers + ", " + MaxSupportedLocalGamers + "]");
            }

            List<SignedInGamer> localGamers = new List<SignedInGamer>(SignedInGamer.SignedInGamers);
            if (localGamers.Count > maxLocalGamers)
            {
                localGamers.RemoveRange(maxLocalGamers, localGamers.Count - maxLocalGamers);
            }

            try { return BeginFind(sessionType, localGamers, searchProperties, callback, asyncState); }
            catch { throw; }
        }

        public static AvailableNetworkSessionCollection EndFind(IAsyncResult result)
        {
            try
            {
                AvailableNetworkSessionCollection availableSessions = AsyncFindCaller.EndInvoke(result);
                AsyncFindCaller = null;
                return availableSessions;
            }
            catch
            {
                AsyncFindCaller = null;
                throw;
            }
        }

        public static IAsyncResult BeginJoin(AvailableNetworkSession availableSession, AsyncCallback callback, Object asyncState)
        {
            if (Session != null || AsyncCreateCaller != null || AsyncFindCaller != null || AsyncJoinCaller != null)
            {
                throw new InvalidOperationException("Only one NetworkSession allowed");
            }
            if (availableSession == null)
            {
                throw new ArgumentNullException("availableSession");
            }

            AsyncJoinCaller = new AsyncJoin(NetworkSessionImplementation.SessionCreator.Join);

            try
            {
                return AsyncJoinCaller.BeginInvoke(availableSession, callback, asyncState);
            }
            catch
            {
                AsyncJoinCaller = null;
                throw;
            }
        }

        public static NetworkSession EndJoin(IAsyncResult result)
        {
            try
            {
                Session = AsyncJoinCaller.EndInvoke(result);
                AsyncJoinCaller = null;
                return Session;
            }
            catch
            {
                AsyncJoinCaller = null;
                throw;
            }
        }

        public static IAsyncResult BeginJoinInvited(IEnumerable<SignedInGamer> localGamers, AsyncCallback callback, Object asyncState)
        {
            throw new NotImplementedException();
        }

        public static IAsyncResult BeginJoinInvited(int maxLocalGamers, AsyncCallback callback, Object asyncState)
        {
            throw new NotImplementedException();
        }

        public static NetworkSession EndJoinInvited(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        // Synchronous session creation
        public static NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
        {
            try { return EndCreate(BeginCreate(sessionType, localGamers, maxGamers, privateGamerSlots, sessionProperties, null, null)); }
            catch { throw; }
        }

        public static NetworkSession Create(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers)
        {
            try { return EndCreate(BeginCreate(sessionType, maxLocalGamers, maxGamers, null, null)); }
            catch { throw; }
        }

        public static NetworkSession Create(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
        {
            try { return EndCreate(BeginCreate(sessionType, maxLocalGamers, maxGamers, privateGamerSlots, sessionProperties, null, null)); }
            catch { throw; }
        }

        public static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties)
        {
            try { return EndFind(BeginFind(sessionType, localGamers, searchProperties, null, null)); }
            catch { throw; }
        }

        public static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, int maxLocalGamers, NetworkSessionProperties searchProperties)
        {
            try { return EndFind(BeginFind(sessionType, maxLocalGamers, searchProperties, null, null)); }
            catch { throw; }
        }

        public static NetworkSession Join(AvailableNetworkSession availableSession)
        {
            try { return EndJoin(BeginJoin(availableSession, null, null)); }
            catch { throw; }
        }

        public static NetworkSession JoinInvited(IEnumerable<SignedInGamer> localGamers)
        {
            try { return EndJoinInvited(BeginJoinInvited(localGamers, null, null)); }
            catch { throw; }
        }

        public static NetworkSession JoinInvited(int maxLocalGamers)
        {
            try { return EndJoinInvited(BeginJoinInvited(maxLocalGamers, null, null)); }
            catch { throw; }
        }
    }
}