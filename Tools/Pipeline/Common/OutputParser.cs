// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

        Unknown
    }

    public class OutputParser
    {
        public OutputState State { get; private set; }
        public string Filename { get; private set; }
        public string ErrorMessage { get; private set; }
        public string BuildBeginTime { get; private set; }
        public string BuildInfo { get; private set; }
        public string BuildElapsedTime { get; private set; }

        private readonly Regex _reBuildBegin = new Regex(@"^(Build started)\W+(?<buildBeginTime>.*?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _reBuildAsset = new Regex(@"^(?<filename>([a-zA-Z]:)?/.+?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _reBuildError = new Regex(@"^(?<filename>([a-zA-Z]:)?/.+?)\W*?:\W*?error\W*?:\W*(?<errorMessage>.*?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _reFileErrorWithLineNum = new Regex(@"^(?<filename>.+?)(\((?<line>[0-9]+),(?<column>[0-9]+)\))?:\W*?(error)\W*(?<errorCode>[A-Z][0-9]+):\W*(?<errorMessage>.*?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _reFileWarningWithLineNum = new Regex(@"^(?<filename>.+?)(\((?<line>[0-9]+),(?<column>[0-9]+)\))?:\W*?(warning)\W*(?<warningCode>[A-Z][0-9]+):\W*(?<warningMessage>.*?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _reFileError = new Regex(@"^(?<filename>([a-zA-Z]:)?/.+?)\W*?: (?<errorMessage>.*?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _reBuildEnd = new Regex(@"^(Build)\W+(?<buildInfo>.*?)\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _reBuildTime = new Regex(@"^(Time elapsed)\W+(?<buildElapsedTime>.*?)\.\r?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public OutputParser()
        {
            Reset();
        }

        public void Reset()
        {
            State = OutputState.Initialized;
            Filename = null;
            BuildBeginTime = null;
            BuildInfo = null;
            BuildElapsedTime = null;
            ErrorMessage = null;
        }

        public void Parse(string text)
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

            if (line.StartsWith("Skipping"))
            {
                State = OutputState.Skipping;
                Filename = line.Substring(9);
            }
            else if (line.StartsWith("Cleaning"))
            {
                State = OutputState.Cleaning;
                Filename = line.Substring(9);
            }
            else if (_reBuildAsset.IsMatch(line))
            {
                State = OutputState.BuildAsset;
                var m = _reBuildAsset.Match(line);
                Filename = m.Groups["filename"].Value;
            }
            else if (_reBuildError.IsMatch(line))
            {
                State = OutputState.BuildError;
                var m = _reBuildError.Match(line);
                Filename = m.Groups["filename"].Value;
                ErrorMessage = m.Groups["errorMessage"].Value;
            }
            else if (_reFileError.IsMatch(line))
            {
                State = OutputState.BuildError;
                var m = _reFileError.Match(line);
                Filename = m.Groups["filename"].Value;
                ErrorMessage = m.Groups["errorMessage"].Value;
            }
            else if (_reFileErrorWithLineNum.IsMatch(line))
            {
                State = OutputState.BuildError;
                var m = _reFileErrorWithLineNum.Match(line);
                var lineNum = m.Groups["line"];
                var column = m.Groups["column"];
                var errorCode = m.Groups["errorCode"];
                Filename = m.Groups["filename"].Value.Replace("\\\\", "/").Replace("\\", "/");
                ErrorMessage = string.Format("{0} ({1},{2}): {3}", errorCode, lineNum, column, m.Groups["errorMessage"].Value);
            }
            else if (_reFileWarningWithLineNum.IsMatch(line))
            {
                State = OutputState.BuildWarning;
                var m = _reFileWarningWithLineNum.Match(line);
                var lineNum = m.Groups["line"];
                var column = m.Groups["column"];
                var errorCode = m.Groups["warningCode"];
                Filename = m.Groups["filename"].Value.Replace("\\\\", "/").Replace("\\", "/");
                ErrorMessage = string.Format("{0} ({1},{2}): {3}", errorCode, lineNum, column, m.Groups["warningMessage"].Value);
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
