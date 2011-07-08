#region File Description
//-----------------------------------------------------------------------------
// NetworkErrorScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
#endregion

namespace NetworkStateManagement
{
    /// <summary>
    /// Specialized message box subclass, used to display network error messages.
    /// </summary>
    class NetworkErrorScreen : MessageBoxScreen
    {
        #region Initialization


        /// <summary>
        /// Constructs an error message box from the specified exception.
        /// </summary>
        public NetworkErrorScreen(Exception exception)
            : base(GetErrorMessage(exception), false)
        { }


        /// <summary>
        /// Converts a network exception into a user friendly error message.
        /// </summary>
        static string GetErrorMessage(Exception exception)
        {
            Debug.WriteLine("Network operation threw " + exception);

            // Is this a GamerPrivilegeException?
            if (exception is GamerPrivilegeException)
            {
                if (Guide.IsTrialMode)
                    return Resources.ErrorTrialMode;
                else
                    return Resources.ErrorGamerPrivilege;
            }

            // Is it a NetworkSessionJoinException?
            NetworkSessionJoinException joinException = exception as
                                                            NetworkSessionJoinException;

            if (joinException != null)
            {
                switch (joinException.JoinError)
                {
                    case NetworkSessionJoinError.SessionFull:
                        return Resources.ErrorSessionFull;

                    case NetworkSessionJoinError.SessionNotFound:
                        return Resources.ErrorSessionNotFound;

                    case NetworkSessionJoinError.SessionNotJoinable:
                        return Resources.ErrorSessionNotJoinable;
                }
            }

            // Is this a NetworkNotAvailableException?
            if (exception is NetworkNotAvailableException)
            {
                return Resources.ErrorNetworkNotAvailable;
            }

            // Is this a NetworkException?
            if (exception is NetworkException)
            {
                return Resources.ErrorNetwork;
            }

            // Otherwise just a generic error message.
            return Resources.ErrorUnknown;
        }


        #endregion
    }
}
