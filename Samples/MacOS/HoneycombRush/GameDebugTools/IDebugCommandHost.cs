#region File Description
//-----------------------------------------------------------------------------
// IDebugCommandHost.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System.Collections.Generic;

#endregion

namespace HoneycombRush.GameDebugTools
{
    /// <summary>
    /// Message types for debug command.
    /// </summary>
    public enum DebugCommandMessage
    {
        Standard = 1,
        Error = 2,
        Warning = 3
    }

    /// <summary>
    /// Debug command execution delegate.
    /// </summary>
    /// <param name="host">Host who will execute the command.</param>
    /// <param name="command">command name</param>
    /// <param name="arguments">command arguments</param>
    public delegate void DebugCommandExecute(IDebugCommandHost host, string command,
                                                            IList<string> arguments);

    /// <summary>
    /// Interface for debug command executioner.
    /// </summary>
    public interface IDebugCommandExecutioner
    {
        /// <summary>
        /// Execute command
        /// </summary>
        void ExecuteCommand(string command);
    }

    /// <summary>
    /// Interface for debug command message listener.
    /// </summary>
    public interface IDebugEchoListner
    {
        /// <summary>
        /// Output message.
        /// </summary>
        /// <param name="messageType">type of message</param>
        /// <param name="text">message text</param>
        void Echo(DebugCommandMessage messageType, string text);
    }

    /// <summary>
    /// Interface for debug command host.
    /// </summary>
    public interface IDebugCommandHost : IDebugEchoListner, IDebugCommandExecutioner
    {
        /// <summary>
        /// Register new command.
        /// </summary>
        /// <param name="command">command name</param>
        /// <param name="description">description of command</param>
        /// <param name="callback">Execute delegation</param>
        void RegisterCommand(string command, string description,
                                                        DebugCommandExecute callback);

        /// <summary>
        /// Unregister command.
        /// </summary>
        /// <param name="command">command name</param>
        void UnregisterCommand(string command);

        /// <summary>
        /// Output Standard message.
        /// </summary>
        /// <param name="text"></param>
        void Echo(string text);

        /// <summary>
        /// Output Warning message.
        /// </summary>
        /// <param name="text"></param>
        void EchoWarning(string text);

        /// <summary>
        /// Output Error message.
        /// </summary>
        /// <param name="text"></param>
        void EchoError(string text);

        /// <summary>
        /// Register message listener.
        /// </summary>
        /// <param name="listner"></param>
        void RegisterEchoListner(IDebugEchoListner listner);

        /// <summary>
        /// Unregister message listener.
        /// </summary>
        /// <param name="listner"></param>
        void UnregisterEchoListner(IDebugEchoListner listner);

        /// <summary>
        /// Add Command executioner.
        /// </summary>
        void PushExecutioner(IDebugCommandExecutioner executioner);

        /// <summary>
        /// Remote Command executioner.
        /// </summary>
        void PopExecutioner();
    }

}
