// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text.RegularExpressions;

namespace MonoGame.Tools.Pipeline
{
    public enum OutputState
    {
        Initialized,
        BuildBegin,
        Cleaning,
        Skipping,
        BuildAsset,
        BuildError,
        BuildWarning,
        BuildErrorContinue,
        BuildEnd,
        BuildTime,
        BuildTerminated,

        Unknown
    }

    public class OutputParser
    {
        internal OutputState State { get; private set; }
        internal String Filename { get; private set; }
        internal String ErrorMessage { get; private set; }
        internal String BuildBeginTime { get; private set; }
        internal String BuildInfo { get; private set; }
        internal String BuildElapsedTime { get; private set; }


        Regex _reBuildBegin = new Regex(@"^(Build started)\W+(?<buildBeginTime>.*?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex _reCleaning = new Regex(@"^(Cleaning)\W(?<filename>.*?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex _reSkipping = new Regex(@"^(Skipping)\W(?<filename>.*?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex _reBuildAsset = new Regex(@"^(?<filename>([a-zA-Z]:)?/.+?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex _reBuildError = new Regex(@"^(?<filename>([a-zA-Z]:)?/.+?)\W*?:\W*?error\W*?:\W*(?<errorMessage>.*?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex _reFileErrorOrWarningWithLineNum = new Regex(@"^(?<filename>.+?)(\((?<line>\d+),(?<column>\d+)(((-|(,\d+,)))(?<columnEnd>\d+))?\))?:\W*?(?<type>(error|warning))\W*(?<code>[A-Z]+[0-9]+):\s*(?<message>.*?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex _reFileError = new Regex(@"^(?<filename>([a-zA-Z]:)?/.+?)\W*?: (?<errorMessage>.*?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex _reBuildEnd = new Regex(@"^(Build)\W+(?<buildInfo>.*?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex _reBuildTime = new Regex(@"^(Time elapsed)\W+(?<buildElapsedTime>.*?)\.\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        

        public OutputParser()
        {
            Reset();
        }

        internal void Reset()
        {
            State = OutputState.Initialized;
            Filename = null;
            BuildBeginTime = null;
            BuildInfo = null;
            BuildElapsedTime = null;
            ErrorMessage = null;
        }

        internal void Parse(string text)
        {   
            ParseLine(text);
        }


        private void ParseLine(string line)
        {
            /* 
             * Line <-- BuildBegin
             * Line <-- BuildEnd
             * Line <-- BuildTime
             * Line <-- Cleaning
             * Line <-- Skipping
             * Line <-- BuildError (BuildErrorContinue)+
             * Line <-- BuildAsset
             * BuildBegin   <-- "Build" "started" buildBeginTime
             * Cleaning     <-- "Cleaning" filenane
             * Skipping     <-- "Skipping" filenane
             * BuildAsset   <-- filename
             * BuildError   <-- filename ':' "Error" ':' errorMessage
             * BuildErrorContinue <-- errorMessage
             * BuildEnd     <-- "Build" buildInfo
             * BuildTime    <-- "Time" "elapsed" buildElapsedTime
             */

            var prevState = State;
            var prevFilename = Filename;

            State = OutputState.Unknown;
            Filename = null;            
            BuildBeginTime = null;
            BuildInfo = null;
            BuildElapsedTime = null;
            ErrorMessage = null;

            if (line == "Build terminated!")
            {
                State = OutputState.BuildTerminated;
            }
            else if (_reBuildBegin.IsMatch(line))
            {
                State = OutputState.BuildBegin;
                var m = _reBuildBegin.Match(line);
                BuildBeginTime = m.Groups["buildBeginTime"].Value;
            }
            else if (_reBuildEnd.IsMatch(line))
            {
                State = OutputState.BuildEnd;
                var m = _reBuildEnd.Match(line);
                BuildInfo = m.Groups["buildInfo"].Value;
            }
            else if (_reBuildTime.IsMatch(line))
            {
                State = OutputState.BuildTime;
                var m = _reBuildTime.Match(line);
                BuildElapsedTime = m.Groups["buildElapsedTime"].Value;
            }
            else if (_reCleaning.IsMatch(line))
            {
                State = OutputState.Cleaning;
                var m = _reCleaning.Match(line);
                Filename = m.Groups["filename"].Value;
            }
            else if (_reSkipping.IsMatch(line))
            {
                State = OutputState.Skipping;
                var m = _reSkipping.Match(line);
                Filename = m.Groups["filename"].Value;
            }
            else if (_reBuildError.IsMatch(line))
            {
                State = OutputState.BuildError;
                var m = _reBuildError.Match(line);
                Filename = m.Groups["filename"].Value;
                ErrorMessage = m.Groups["errorMessage"].Value;
            }
            else if (_reFileErrorOrWarningWithLineNum.IsMatch(line))
            {
                var m = _reFileErrorOrWarningWithLineNum.Match(line);
                var lineNum = m.Groups["line"];
                var columnBegin = m.Groups["column"];
                var columnEnd = m.Groups["columnEnd"];
                var column = columnBegin.Value;
                if (columnEnd.Success)
                    column += "-" + columnEnd.Value;
                var type = m.Groups["type"];
                var errorCode = m.Groups["code"];
                State = type.Value.Equals("error", StringComparison.OrdinalIgnoreCase) ? OutputState.BuildError : OutputState.BuildWarning;
                Filename = m.Groups["filename"].Value.Replace("\\\\", "/").Replace("\\", "/");
                ErrorMessage = string.Format("{0} {1} ({2},{3}): {4}", type, errorCode, lineNum, column, m.Groups["message"].Value);
            }
            else if (_reFileError.IsMatch(line))
            {
                State = OutputState.BuildError;
                var m = _reFileError.Match(line);
                Filename = m.Groups["filename"].Value;
                ErrorMessage = m.Groups["errorMessage"].Value;
            }
            else if (_reBuildAsset.IsMatch(line))
            {
                State = OutputState.BuildAsset;
                var m = _reBuildAsset.Match(line);
                Filename = m.Groups["filename"].Value;
            }
            else if (prevState == OutputState.BuildError || prevState == OutputState.BuildErrorContinue)
            {
                State = OutputState.BuildErrorContinue;
                Filename = prevFilename;
                ErrorMessage = line.TrimEnd();
            }
            else
            {
                State = OutputState.Unknown;
            }
        }
    }
}
