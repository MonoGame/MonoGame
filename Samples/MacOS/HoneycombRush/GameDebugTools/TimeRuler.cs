#region File Description
//-----------------------------------------------------------------------------
// TimeRuler.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace HoneycombRush.GameDebugTools
{
    /// <summary>
    /// Realtime CPU measuring tool
    /// </summary>
    /// <remarks>
    /// You can visually find bottle neck, and know how much you can put more CPU jobs
    /// by using this tool.
    /// Because of this is real time profile, you can find glitches in the game too.
    /// 
    /// TimeRuler provide the following features:
    ///  * Up to 8 bars (Configurable)
    ///  * Change colors for each markers
    ///  * Marker logging.
    ///  * It won't even generate BeginMark/EndMark method calls when you got rid of the
    ///    TRACE constant.
    ///  * It supports up to 32 (Configurable) nested BeginMark method calls.
    ///  * Multithreaded safe
    ///  * Automatically changes display frames based on frame duration.
    ///  
    /// How to use:
    /// Added TimerRuler instance to Game.Components and call timerRuler.StartFrame in
    /// top of the Game.Update method.
    /// 
    /// Then, surround the code that you want measure by BeginMark and EndMark.
    /// 
    /// timeRuler.BeginMark( "Update", Color.Blue );
    /// // process that you want to measure.
    /// timerRuler.EndMark( "Update" );
    /// 
    /// Also, you can specify bar index of marker (default value is 0)
    /// 
    /// timeRuler.BeginMark( 1, "Update", Color.Blue );
    /// 
    /// All profiling methods has CondionalAttribute with "TRACE".
    /// If you not specified "TRACE" constant, it doesn't even generate
    /// method calls for BeginMark/EndMark.
    /// So, don't forget remove "TRACE" constant when you release your game.
    /// 
    /// </remarks>
    public class TimeRuler : DrawableGameComponent
    {
        #region Constants

        /// <summary>
        /// Max bar count.
        /// </summary>
        const int MaxBars = 8;

        /// <summary>
        /// Maximum sample number for each bar.
        /// </summary>
        const int MaxSamples = 256;

        /// <summary>
        /// Maximum nest calls for each bar.
        /// </summary>
        const int MaxNestCall = 32;

        /// <summary>
        /// Maximum display frames.
        /// </summary>
        const int MaxSampleFrames = 4;

        /// <summary>
        /// Duration (in frame count) for take snap shot of log.
        /// </summary>
        const int LogSnapDuration = 120;

        /// <summary>
        /// Height(in pixels) of bar.
        /// </summary>
        const int BarHeight = 8;

        /// <summary>
        /// Padding(in pixels) of bar.
        /// </summary>
        const int BarPadding = 2;

        /// <summary>
        /// Delay frame count for auto display frame adjustment.
        /// </summary>
        const int AutoAdjustDelay = 30;

        #endregion

        #region Properties

        /// <summary>
        /// Gets/Set log display or no.
        /// </summary>
        public bool ShowLog { get; set; }

        /// <summary>
        /// Gets/Sets target sample frames.
        /// </summary>
        public int TargetSampleFrames { get; set; }

        /// <summary>
        /// Gets/Sets TimeRuler rendering position.
        /// </summary>
        public Vector2 Position { get { return position; } set { position = value; } }

        /// <summary>
        /// Gets/Sets timer ruler width.
        /// </summary>
        public int Width { get; set; }

        #endregion

        #region Fields

#if TRACE

        /// <summary>
        /// Marker structure.
        /// </summary>
        private struct Marker
        {
            public int MarkerId;
            public float BeginTime;
            public float EndTime;
            public Color Color;
        }

        /// <summary>
        /// Collection of markers.
        /// </summary>
        private class MarkerCollection
        {
            // Marker collection.
            public Marker[] Markers = new Marker[MaxSamples];
            public int MarkCount;

            // Marker nest information.
            public int[] MarkerNests = new int[MaxNestCall];
            public int NestCount;
        }

        /// <summary>
        /// Frame logging information.
        /// </summary>
        private class FrameLog
        {
            public MarkerCollection[] Bars;

            public FrameLog()
            {
                // Initialize markers.
                Bars = new MarkerCollection[MaxBars];
                for (int i = 0; i < MaxBars; ++i)
                    Bars[i] = new MarkerCollection();
            }
        }

        /// <summary>
        /// Marker information
        /// </summary>
        private class MarkerInfo
        {
            // Name of marker.
            public string Name;

            // Marker log.
            public MarkerLog[] Logs = new MarkerLog[MaxBars];

            public MarkerInfo(string name)
            {
                Name = name;
            }
        }

        /// <summary>
        /// Marker log information.
        /// </summary>
        private struct MarkerLog
        {
            public float SnapMin;
            public float SnapMax;
            public float SnapAvg;

            public float Min;
            public float Max;
            public float Avg;

            public int Samples;

            public Color Color;

            public bool Initialized;
        }

        // Reference of debug manager.
        DebugManager debugManager;

        // Logs for each frames.
        FrameLog[] logs;

        // Previous frame log.
        FrameLog prevLog;

        // Current log.
        FrameLog curLog;

        // Current frame count.
        int frameCount;

        // Stopwatch for measure the time.
        Stopwatch stopwatch = new Stopwatch();

        // Marker information array.
        List<MarkerInfo> markers = new List<MarkerInfo>();

        // Dictionary that maps from marker name to marker id.
        Dictionary<string, int> markerNameToIdMap = new Dictionary<string, int>();

        // Display frame adjust counter.
        int frameAdjust;

        // Current display frame count.
        int sampleFrames;

        // Marker log string.
        StringBuilder logString = new StringBuilder(512);

        // You want to call StartFrame at beginning of Game.Update method.
        // But Game.Update gets calls multiple time when game runs slow in fixed time step mode.
        // In this case, we should ignore StartFrame call.
        // To do this, we just keep tracking of number of StartFrame calls until Draw gets called.
        int updateCount;

#endif
        // TimerRuler draw position.
        Vector2 position;

        #endregion

        #region Initialization

        public TimeRuler(Game game)
            : base(game)
        {
            // Add this as a service.
            Game.Services.AddService(typeof(TimeRuler), this);
        }

        public override void Initialize()
        {
            DrawOrder = int.MaxValue - 2;

#if TRACE
            debugManager =
                Game.Services.GetService(typeof(DebugManager)) as DebugManager;

            if (debugManager == null)
                throw new InvalidOperationException("DebugManager is not registered.");

            // Add "tr" command if DebugCommandHost is registered.
            IDebugCommandHost host =
                                Game.Services.GetService(typeof(IDebugCommandHost))
                                                                    as IDebugCommandHost;
            if (host != null)
            {
                host.RegisterCommand("tr", "TimeRuler", this.CommandExecute);
                this.Visible = false;
            }

            // Initialize Parameters.
            logs = new FrameLog[2];
            for (int i = 0; i < logs.Length; ++i)
                logs[i] = new FrameLog();

            sampleFrames = TargetSampleFrames = 1;

            // Time-Ruler's update method doesn't need to get called.
            this.Enabled = false;
#endif
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Width = (int)(GraphicsDevice.Viewport.Width * 0.8f);

            Layout layout = new Layout(GraphicsDevice.Viewport);
            position = layout.Place(new Vector2(Width, BarHeight),
                                                    0, 0.01f, Alignment.BottomCenter);

            base.LoadContent();
        }

#if TRACE
        /// <summary>
        /// 'tr' command execution.
        /// </summary>
        void CommandExecute(IDebugCommandHost host, string command,
                                                                IList<string> arguments)
        {
            bool previousVisible = Visible;

            if (arguments.Count == 0)
                Visible = !Visible;

            char[] subArgSeparator = new[] { ':' };
            foreach (string orgArg in arguments)
            {
                string arg = orgArg.ToLower();
                string[] subargs = arg.Split(subArgSeparator);
                switch (subargs[0])
                {
                    case "on":
                        Visible = true;
                        break;
                    case "off":
                        Visible = false;
                        break;
                    case "reset":
                        ResetLog();
                        break;
                    case "log":
                        if (subargs.Length > 1)
                        {
                            if (String.Compare(subargs[1], "on") == 0)
                                ShowLog = true;
                            if (String.Compare(subargs[1], "off") == 0)
                                ShowLog = false;
                        }
                        else
                        {
                            ShowLog = !ShowLog;
                        }
                        break;
                    case "frame":
                        int a = Int32.Parse(subargs[1]);
                        a = Math.Max(a, 1);
                        a = Math.Min(a, MaxSampleFrames);
                        TargetSampleFrames = a;
                        break;
                    case "/?":
                    case "--help":
                        host.Echo("tr [log|on|off|reset|frame]");
                        host.Echo("Options:");
                        host.Echo("       on     Display TimeRuler.");
                        host.Echo("       off    Hide TimeRuler.");
                        host.Echo("       log    Show/Hide marker log.");
                        host.Echo("       reset  Reset marker log.");
                        host.Echo("       frame:sampleFrames");
                        host.Echo("              Change target sample frame count");
                        break;
                    default:
                        break;
                }
            }

            // Reset update count when Visible state changed.
            if (Visible != previousVisible)
            {
                Interlocked.Exchange(ref updateCount, 0);
            }
        }
#endif

        #endregion

        #region Measuring methods

        /// <summary>
        /// Start new frame.
        /// </summary>
        [Conditional("TRACE")]
        public void StartFrame()
        {
#if TRACE
            lock (this)
            {
                // We skip reset frame when this method gets called multiple times.
                int count = Interlocked.Increment(ref updateCount);
                if (Visible && (1 < count && count < MaxSampleFrames))
                    return;

                // Update current frame log.
                prevLog = logs[frameCount++ & 0x1];
                curLog = logs[frameCount & 0x1];

                float endFrameTime = (float)stopwatch.Elapsed.TotalMilliseconds;

                // Update marker and create a log.
                for (int barIdx = 0; barIdx < prevLog.Bars.Length; ++barIdx)
                {
                    MarkerCollection prevBar = prevLog.Bars[barIdx];
                    MarkerCollection nextBar = curLog.Bars[barIdx];

                    // Re-open marker that didn't get called EndMark in previous frame.
                    for (int nest = 0; nest < prevBar.NestCount; ++nest)
                    {
                        int markerIdx = prevBar.MarkerNests[nest];

                        prevBar.Markers[markerIdx].EndTime = endFrameTime;

                        nextBar.MarkerNests[nest] = nest;
                        nextBar.Markers[nest].MarkerId =
                            prevBar.Markers[markerIdx].MarkerId;
                        nextBar.Markers[nest].BeginTime = 0;
                        nextBar.Markers[nest].EndTime = -1;
                        nextBar.Markers[nest].Color = prevBar.Markers[markerIdx].Color;
                    }

                    // Update marker log.
                    for (int markerIdx = 0; markerIdx < prevBar.MarkCount; ++markerIdx)
                    {
                        float duration = prevBar.Markers[markerIdx].EndTime -
                                            prevBar.Markers[markerIdx].BeginTime;

                        int markerId = prevBar.Markers[markerIdx].MarkerId;
                        MarkerInfo m = markers[markerId];

                        m.Logs[barIdx].Color = prevBar.Markers[markerIdx].Color;

                        if (!m.Logs[barIdx].Initialized)
                        {
                            // First frame process.
                            m.Logs[barIdx].Min = duration;
                            m.Logs[barIdx].Max = duration;
                            m.Logs[barIdx].Avg = duration;

                            m.Logs[barIdx].Initialized = true;
                        }
                        else
                        {
                            // Process after first frame.
                            m.Logs[barIdx].Min = Math.Min(m.Logs[barIdx].Min, duration);
                            m.Logs[barIdx].Max = Math.Min(m.Logs[barIdx].Max, duration);
                            m.Logs[barIdx].Avg += duration;
                            m.Logs[barIdx].Avg *= 0.5f;

                            if (m.Logs[barIdx].Samples++ >= LogSnapDuration)
                            {
                                m.Logs[barIdx].SnapMin = m.Logs[barIdx].Min;
                                m.Logs[barIdx].SnapMax = m.Logs[barIdx].Max;
                                m.Logs[barIdx].SnapAvg = m.Logs[barIdx].Avg;
                                m.Logs[barIdx].Samples = 0;
                            }
                        }
                    }

                    nextBar.MarkCount = prevBar.NestCount;
                    nextBar.NestCount = prevBar.NestCount;
                }

                // Start measuring.
                stopwatch.Reset();
                stopwatch.Start();
            }
#endif
        }

        /// <summary>
        /// Start measure time.
        /// </summary>
        /// <param name="markerName">name of marker.</param>
        /// <param name="color">color/param>
        [Conditional("TRACE")]
        public void BeginMark(string markerName, Color color)
        {
#if TRACE
            BeginMark(0, markerName, color);
#endif
        }

        /// <summary>
        /// Start measure time.
        /// </summary>
        /// <param name="barIndex">index of bar</param>
        /// <param name="markerName">name of marker.</param>
        /// <param name="color">color/param>
        [Conditional("TRACE")]
        public void BeginMark(int barIndex, string markerName, Color color)
        {
#if TRACE
            lock (this)
            {
                if (barIndex < 0 || barIndex >= MaxBars)
                    throw new ArgumentOutOfRangeException("barIndex");

                MarkerCollection bar = curLog.Bars[barIndex];

                if (bar.MarkCount >= MaxSamples)
                {
                    throw new OverflowException(
                        "Exceeded sample count.\n" +
                        "Either set larger number to TimeRuler.MaxSmpale or" +
                        "lower sample count.");
                }

                if (bar.NestCount >= MaxNestCall)
                {
                    throw new OverflowException(
                        "Exceeded nest count.\n" +
                        "Either set larget number to TimeRuler.MaxNestCall or" +
                        "lower nest calls.");
                }

                // Gets registered marker.
                int markerId;
                if (!markerNameToIdMap.TryGetValue(markerName, out markerId))
                {
                    // Register this if this marker is not registered.
                    markerId = markers.Count;
                    markerNameToIdMap.Add(markerName, markerId);
                    markers.Add(new MarkerInfo(markerName));
                }

                // Start measuring.
                bar.MarkerNests[bar.NestCount++] = bar.MarkCount;

                // Fill marker parameters.
                bar.Markers[bar.MarkCount].MarkerId = markerId;
                bar.Markers[bar.MarkCount].Color = color;
                bar.Markers[bar.MarkCount].BeginTime =
                                        (float)stopwatch.Elapsed.TotalMilliseconds;

                bar.Markers[bar.MarkCount].EndTime = -1;

                bar.MarkCount++;
            }
#endif
        }

        /// <summary>
        /// End measuring.
        /// </summary>
        /// <param name="markerName">Name of marker.</param>
        [Conditional("TRACE")]
        public void EndMark(string markerName)
        {
#if TRACE
            EndMark(0, markerName);
#endif
        }

        /// <summary>
        /// End measuring.
        /// </summary>
        /// <param name="barIndex">Index of bar.</param>
        /// <param name="markerName">Name of marker.</param>
        [Conditional("TRACE")]
        public void EndMark(int barIndex, string markerName)
        {
#if TRACE
            lock (this)
            {
                if (barIndex < 0 || barIndex >= MaxBars)
                    throw new ArgumentOutOfRangeException("barIndex");

                MarkerCollection bar = curLog.Bars[barIndex];

                if (bar.NestCount <= 0)
                {
                    throw new InvalidOperationException(
                        "Call BeingMark method before call EndMark method.");
                }

                int markerId;
                if (!markerNameToIdMap.TryGetValue(markerName, out markerId))
                {
                    throw new InvalidOperationException(
                        String.Format("Maker '{0}' is not registered." +
                            "Make sure you specifed same name as you used for BeginMark" +
                            " method.",
                            markerName));
                }

                int markerIdx = bar.MarkerNests[--bar.NestCount];
                if (bar.Markers[markerIdx].MarkerId != markerId)
                {
                    throw new InvalidOperationException(
                    "Incorrect call order of BeginMark/EndMark method." +
                    "You call it like BeginMark(A), BeginMark(B), EndMark(B), EndMark(A)" +
                    " But you can't call it like " +
                    "BeginMark(A), BeginMark(B), EndMark(A), EndMark(B).");
                }

                bar.Markers[markerIdx].EndTime =
                    (float)stopwatch.Elapsed.TotalMilliseconds;
            }
#endif
        }

        /// <summary>
        /// Get average time of given bar index and marker name.
        /// </summary>
        /// <param name="barIndex">Index of bar</param>
        /// <param name="markerName">name of marker</param>
        /// <returns>average spending time in ms.</returns>
        public float GetAverageTime(int barIndex, string markerName)
        {
#if TRACE
            if (barIndex < 0 || barIndex >= MaxBars)
                throw new ArgumentOutOfRangeException("barIndex");

            float result = 0;
            int markerId;
            if (markerNameToIdMap.TryGetValue(markerName, out markerId))
                result = markers[markerId].Logs[barIndex].Avg;

            return result;
#else
			return 0;
#endif
        }

        /// <summary>
        /// Reset marker log.
        /// </summary>
        [Conditional("TRACE")]
        public void ResetLog()
        {
#if TRACE
            lock (this)
            {
                foreach (MarkerInfo markerInfo in markers)
                {
                    for (int i = 0; i < markerInfo.Logs.Length; ++i)
                    {
                        markerInfo.Logs[i].Initialized = false;
                        markerInfo.Logs[i].SnapMin = 0;
                        markerInfo.Logs[i].SnapMax = 0;
                        markerInfo.Logs[i].SnapAvg = 0;

                        markerInfo.Logs[i].Min = 0;
                        markerInfo.Logs[i].Max = 0;
                        markerInfo.Logs[i].Avg = 0;

                        markerInfo.Logs[i].Samples = 0;
                    }
                }
            }
#endif
        }

        #endregion

        #region Draw

        public override void Draw(GameTime gameTime)
        {

            Draw(position, Width);
            base.Draw(gameTime);
        }

        [Conditional("TRACE")]
        public void Draw(Vector2 position, int width)
        {
#if TRACE
            // Reset update count.
            Interlocked.Exchange(ref updateCount, 0);

            // Gets SpriteBatch, SpriteFont, and WhiteTexture from DebugManager.
            SpriteBatch spriteBatch = debugManager.SpriteBatch;
            SpriteFont font = debugManager.DebugFont;
            Texture2D texture = debugManager.WhiteTexture;

            // Adjust size and position based of number of bars we should draw.
            int height = 0;
            float maxTime = 0;
            foreach (MarkerCollection bar in prevLog.Bars)
            {
                if (bar.MarkCount > 0)
                {
                    height += BarHeight + BarPadding * 2;
                    maxTime = Math.Max(maxTime,
                                            bar.Markers[bar.MarkCount - 1].EndTime);
                }
            }

            // Auto display frame adjustment.
            // For example, if the entire process of frame doesn't finish in less than 16.6ms
            // thin it will adjust display frame duration as 33.3ms.
            const float frameSpan = 1.0f / 60.0f * 1000f;
            float sampleSpan = (float)sampleFrames * frameSpan;

            if (maxTime > sampleSpan)
                frameAdjust = Math.Max(0, frameAdjust) + 1;
            else
                frameAdjust = Math.Min(0, frameAdjust) - 1;

            if (Math.Abs(frameAdjust) > AutoAdjustDelay)
            {
                sampleFrames = Math.Min(MaxSampleFrames, sampleFrames);
                sampleFrames =
                    Math.Max(TargetSampleFrames, (int)(maxTime / frameSpan) + 1);

                frameAdjust = 0;
            }

            // Compute factor that converts from ms to pixel.
            float msToPs = (float)width / sampleSpan;

            // Draw start position.
            int startY = (int)position.Y - (height - BarHeight);

            // Current y position.
            int y = startY;

            spriteBatch.Begin();

            // Draw transparency background.
            Rectangle rc = new Rectangle((int)position.X, y, width, height);
            spriteBatch.Draw(texture, rc, new Color(0, 0, 0, 128));

            // Draw markers for each bars.
            rc.Height = BarHeight;
            foreach (MarkerCollection bar in prevLog.Bars)
            {
                rc.Y = y + BarPadding;
                if (bar.MarkCount > 0)
                {
                    for (int j = 0; j < bar.MarkCount; ++j)
                    {
                        float bt = bar.Markers[j].BeginTime;
                        float et = bar.Markers[j].EndTime;
                        int sx = (int)(position.X + bt * msToPs);
                        int ex = (int)(position.X + et * msToPs);
                        rc.X = sx;
                        rc.Width = Math.Max(ex - sx, 1);

                        spriteBatch.Draw(texture, rc, bar.Markers[j].Color);
                    }
                }

                y += BarHeight + BarPadding;
            }

            // Draw grid lines.
            // Each grid represents ms.
            rc = new Rectangle((int)position.X, (int)startY, 1, height);
            for (float t = 1.0f; t < sampleSpan; t += 1.0f)
            {
                rc.X = (int)(position.X + t * msToPs);
                spriteBatch.Draw(texture, rc, Color.Gray);
            }

            // Draw frame grid.
            for (int i = 0; i <= sampleFrames; ++i)
            {
                rc.X = (int)(position.X + frameSpan * (float)i * msToPs);
                spriteBatch.Draw(texture, rc, Color.White);
            }

            // Draw log.
            if (ShowLog)
            {
                // Generate log string.
                y = startY - font.LineSpacing;
                logString.Length = 0;
                foreach (MarkerInfo markerInfo in markers)
                {
                    for (int i = 0; i < MaxBars; ++i)
                    {
                        if (markerInfo.Logs[i].Initialized)
                        {
                            if (logString.Length > 0)
                                logString.Append("\n");

                            logString.Append(" Bar ");
                            logString.AppendNumber(i);
                            logString.Append(" ");
                            logString.Append(markerInfo.Name);

                            logString.Append(" Avg.:");
                            logString.AppendNumber(markerInfo.Logs[i].SnapAvg);
                            logString.Append("ms ");

                            y -= font.LineSpacing;
                        }
                    }
                }

                // Compute background size and draw it.
                Vector2 size = font.MeasureString(logString);
                rc = new Rectangle((int)position.X, (int)y, (int)size.X + 12, (int)size.Y);
                spriteBatch.Draw(texture, rc, new Color(0, 0, 0, 128));

                // Draw log string.
                spriteBatch.DrawString(font, logString,
                                        new Vector2(position.X + 12, y), Color.White);


                // Draw log color boxes.
                y += (int)((float)font.LineSpacing * 0.3f);
                rc = new Rectangle((int)position.X + 4, y, 10, 10);
                Rectangle rc2 = new Rectangle((int)position.X + 5, y + 1, 8, 8);
                foreach (MarkerInfo markerInfo in markers)
                {
                    for (int i = 0; i < MaxBars; ++i)
                    {
                        if (markerInfo.Logs[i].Initialized)
                        {
                            rc.Y = y;
                            rc2.Y = y + 1;
                            spriteBatch.Draw(texture, rc, Color.White);
                            spriteBatch.Draw(texture, rc2, markerInfo.Logs[i].Color);

                            y += font.LineSpacing;
                        }
                    }
                }


            }

            spriteBatch.End();
#endif
        }

        #endregion

    }
}
