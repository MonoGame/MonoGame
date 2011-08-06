#region File Description
//-----------------------------------------------------------------------------
// RemoteDebugCommands.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

// Remote debugging doesn't work on Windows Phone because it relies on
// Microsoft.Xna.Framework.Net. We therefore can't compile this class
// for the Windows Phone platform.
#if !WINDOWS_PHONE

#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using System.Text.RegularExpressions;

#endregion

namespace HoneycombRush.GameDebugTools
{
    /// <summary>
    /// Remote debug component.
    /// </summary>
    /// <remarks>
    /// This is the game component that supports remote debug command.
    /// When you use "remote" command on Windows side, it connect to Xbox 360 game via
    /// SystemLink.
    /// 
    /// After you connected to Xbox 360, you can type debug command on Windows side that
    /// actually executed on Xbox 360 game. So, you can run debug command without
    /// connect a keyboard to the Xbox 360 console.
    /// 
    /// To quit remote debug command more, simply type 'quit' command.
    /// </remarks>
    public class RemoteDebugCommand : GameComponent,
        IDebugCommandExecutioner, IDebugEchoListner
    {
        #region Properties

        /// <summary>
        /// Sets/Get NetworkSession for Remote Debug Command.
        /// </summary>
        public NetworkSession NetworkSession { get; set; }

        /// <summary>
        /// Represents NetworkSession created by this component or not.
        /// </summary>
        public bool OwnsNetworkSession { get; private set; }

        #endregion

        #region Constants

        const string StartPacketHeader = "RmtStart";
        const string ExecutePacketHeader = "RmtCmd";
        const string EchoPacketHeader = "RmtEcho";
        const string ErrorPacketHeader = "RmtErr";
        const string WarningPacketHeader = "RmtWrn";
        const string QuitPacketHeader = "RmtQuit";

        #endregion

        #region Fields

        IDebugCommandHost commandHost;

#if WINDOWS
        bool IsHost = false;
#else
        bool IsHost = true;
#endif

        Regex packetRe = new Regex(@"\$(?<header>[^$]+)\$:(?<text>.+)");

        PacketReader packetReader = new PacketReader();
        PacketWriter packetWriter = new PacketWriter();

        IAsyncResult asyncResult;

        enum ConnectionPahse
        {
            None,
            EnsureSignedIn,
            FindSessions,
            Joining,
        }

        ConnectionPahse phase = ConnectionPahse.None;

        #endregion

        #region Initialization

        public RemoteDebugCommand(Game game)
            : base(game)
        {
            commandHost =
                game.Services.GetService(typeof(IDebugCommandHost)) as IDebugCommandHost;

            if (!IsHost)
            {
                commandHost.RegisterCommand("remote", "Start remote command",
                                                ExecuteRemoteCommand);
            }
        }

        public override void Initialize()
        {
            if (IsHost)
            {
                commandHost.RegisterEchoListner(this);

                // Create network session if NetworkSession is not set.
                if (NetworkSession == null)
                {
//                    GamerServicesDispatcher.WindowHandle = Game.Window.Handle;
//                    GamerServicesDispatcher.Initialize(Game.Services);
                    NetworkSession =
                        NetworkSession.Create(NetworkSessionType.SystemLink, 1, 2);

                    OwnsNetworkSession = true;
                }
            }

            base.Initialize();
        }

        #endregion

        /// <summary>
        /// Process received packet string.
        /// </summary>
        /// <remarks>You can call this method if you own network session on the game side.
        /// </remarks>
        /// <param name="packetString"></param>
        /// <returns>Processed this packet?</returns>
        public bool ProcessRecievedPacket(string packetString)
        {
            bool processed = false;

            Match mc = packetRe.Match(packetString);
            if (mc.Success)
            {
                string packetHeader = mc.Groups["header"].Value;
                string text = mc.Groups["text"].Value;
                switch (packetHeader)
                {
                    case ExecutePacketHeader:
                        commandHost.ExecuteCommand(text);
                        processed = true;
                        break;
                    case EchoPacketHeader:
                        commandHost.Echo(text);
                        processed = true;
                        break;
                    case ErrorPacketHeader:
                        commandHost.EchoError(text);
                        processed = true;
                        break;
                    case WarningPacketHeader:
                        commandHost.EchoWarning(text);
                        processed = true;
                        break;
                    case StartPacketHeader:
                        ConnectedToRemote();
                        commandHost.Echo(text);
                        processed = true;
                        break;
                    case QuitPacketHeader:
                        commandHost.Echo(text);
                        DisconnectedFromRemote();
                        processed = true;
                        break;
                }
            }

            return processed;
        }

        #region Implementations

        /// <summary>
        /// Update
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Process different phases.
            switch (phase)
            {
                case ConnectionPahse.EnsureSignedIn:
//                    GamerServicesDispatcher.Update();
                    break;

                case ConnectionPahse.FindSessions:
//                    GamerServicesDispatcher.Update();
                    if (asyncResult.IsCompleted)
                    {
                        AvailableNetworkSessionCollection sessions =
                            NetworkSession.EndFind(asyncResult);

                        if (sessions.Count > 0)
                        {
                            asyncResult = NetworkSession.BeginJoin( sessions[0],
                                                                    null, null );
                            commandHost.EchoError("Connecting to the host...");
                            phase = ConnectionPahse.Joining;
                        }
                        else
                        {
                            commandHost.EchoError("Couldn't find a session.");
                            phase = ConnectionPahse.None;
                        }
                    }
                    break;
                case ConnectionPahse.Joining:
//                    GamerServicesDispatcher.Update();
                    if (asyncResult.IsCompleted)
                    {
                        NetworkSession = NetworkSession.EndJoin(asyncResult);
                        NetworkSession.SessionEnded +=
                            new EventHandler<NetworkSessionEndedEventArgs>(
                                                            NetworkSession_SessionEnded);

                        OwnsNetworkSession = true;
                        commandHost.EchoError("Connected to the host.");
                        phase = ConnectionPahse.None;
                        asyncResult = null;

                        ConnectedToRemote();
                    }
                    break;
            }

            // Update Network session.
            if (OwnsNetworkSession)
            {
//                GamerServicesDispatcher.Update();
                NetworkSession.Update();

                if (NetworkSession != null)
                {
                    // Process received packets.
                    foreach (LocalNetworkGamer gamer in NetworkSession.LocalGamers)
                    {
                        while (gamer.IsDataAvailable)
                        {
                            NetworkGamer sender;
                            gamer.ReceiveData(packetReader, out sender);
                            if (!sender.IsLocal)
                                ProcessRecievedPacket(packetReader.ReadString());
                        }
                    }
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Send remote debug command packet.
        /// </summary>
        void SendPacket(string header, string text)
        {
            if (NetworkSession != null)
            {
                packetWriter.Write("$" + header + "$:" + text);
                NetworkSession.LocalGamers[0].SendData(packetWriter,
                    SendDataOptions.ReliableInOrder);
            }
        }

        /// <summary>
        /// Start remote debug command.
        /// </summary>
        void ConnectedToRemote()
        {
            DebugCommandUI commandUI = commandHost as DebugCommandUI;

            if (IsHost)
            {
                if ( commandUI != null )
                    commandUI.Prompt = "[Host]>";
            }
            else
            {
                if (commandUI != null)
                    commandUI.Prompt = "[Client]>";

                commandHost.PushExecutioner(this);

                SendPacket(StartPacketHeader, "Remote Debug Command Started!!");
            }

            commandHost.RegisterCommand("quit", "Quit from remote command",
                                            ExecuteQuitCommand);
        }

        /// <summary>
        /// End remote debug command.
        /// </summary>
        void DisconnectedFromRemote()
        {
            DebugCommandUI commandUI = commandHost as DebugCommandUI;
            if (commandUI != null)
                commandUI.Prompt = DebugCommandUI.DefaultPrompt;

            commandHost.UnregisterCommand("quit");

            if (!IsHost)
            {
                commandHost.PopExecutioner();

                if (OwnsNetworkSession)
                {
                    NetworkSession.Dispose();
                    NetworkSession = null;
                    OwnsNetworkSession = false;
                }
            }
        }

        #region DebugCommand implementations

        private void ExecuteRemoteCommand(IDebugCommandHost host, string command,
                                                            IList<string> arguments)
        {
            if (NetworkSession == null)
            {
                try
                {
//                    GamerServicesDispatcher.WindowHandle = Game.Window.Handle;
//                    GamerServicesDispatcher.Initialize(Game.Services);
                }
                catch { }

                if (SignedInGamer.SignedInGamers.Count > 0)
                {
                    commandHost.Echo("Finding available sessions...");

                    asyncResult = NetworkSession.BeginFind(
                            NetworkSessionType.SystemLink, 1, null, null, null);

                    phase = ConnectionPahse.FindSessions;
                }
                else
                {
                    host.Echo("Please signed in.");
                    phase = ConnectionPahse.EnsureSignedIn;
                }
            }
            else
            {
                ConnectedToRemote();
            }
        }

        private void ExecuteQuitCommand(IDebugCommandHost host, string command,
                                                            IList<string> arguments)
        {
            SendPacket(QuitPacketHeader, "End Remote Debug Command.");
            DisconnectedFromRemote();
        }

        #endregion

        #region IDebugCommandExecutioner and IDebugEchoListner

        public void ExecuteCommand(string command)
        {
            SendPacket(ExecutePacketHeader, command);
        }

        public void Echo(DebugCommandMessage messageType, string text)
        {
            switch (messageType)
            {
                case DebugCommandMessage.Standard:
                    SendPacket(EchoPacketHeader, text );
                    break;
                case DebugCommandMessage.Warning:
                    SendPacket(WarningPacketHeader, text);
                    break;
                case DebugCommandMessage.Error:
                    SendPacket(ErrorPacketHeader, text);
                    break;
            }
        }

        #endregion

        /// <summary>
        /// Handle the case host machine is gone.
        /// </summary>
        void NetworkSession_SessionEnded(object sender, NetworkSessionEndedEventArgs e)
        {
            DisconnectedFromRemote();
            commandHost.EchoWarning("Disconnected from the Host.");
        }

        #endregion
    }
}

#endif
