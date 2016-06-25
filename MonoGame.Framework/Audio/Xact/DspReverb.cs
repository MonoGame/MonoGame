// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
    class DspReverb
    {
        public DspReverb(BinaryReader reader)
        {
            ReflectionsDelayMs = new DspParameter(reader);
            ReverbDelayMs = new DspParameter(reader);
            PositionLeft = new DspParameter(reader);
            PositionRight = new DspParameter(reader);
            PositionLeftMatrix = new DspParameter(reader);
            PositionRightMatrix = new DspParameter(reader);
            EarlyDiffusion = new DspParameter(reader);
            LateDiffusion = new DspParameter(reader);
            LowEqGain = new DspParameter(reader);
            LowEqCutoff = new DspParameter(reader);
            HighEqGain = new DspParameter(reader);
            HighEqCutoff = new DspParameter(reader);
            RearDelayMs = new DspParameter(reader);
            RoomFilterFrequencyHz = new DspParameter(reader);
            RoomFilterMainDb = new DspParameter(reader);
            RoomFilterHighFrequencyDb = new DspParameter(reader);
            ReflectionsGainDb = new DspParameter(reader);
            ReverbGainDb = new DspParameter(reader);
            DecayTimeSec = new DspParameter(reader);
            DensityPct = new DspParameter(reader);
            RoomSizeFeet = new DspParameter(reader);
            WetDryMixPct = new DspParameter(reader);
        }

        public DspParameter ReflectionsGainDb;
        public DspParameter ReverbGainDb;
        public DspParameter DecayTimeSec;
        public DspParameter ReflectionsDelayMs;
        public DspParameter ReverbDelayMs;
        public DspParameter RearDelayMs;
        public DspParameter RoomSizeFeet;
        public DspParameter DensityPct;
        public DspParameter LowEqGain;
        public DspParameter LowEqCutoff;
        public DspParameter HighEqGain;
        public DspParameter HighEqCutoff;
        public DspParameter PositionLeft;
        public DspParameter PositionRight;
        public DspParameter PositionLeftMatrix;
        public DspParameter PositionRightMatrix;
        public DspParameter EarlyDiffusion;
        public DspParameter LateDiffusion;
        public DspParameter RoomFilterMainDb;
        public DspParameter RoomFilterFrequencyHz;
        public DspParameter RoomFilterHighFrequencyDb;
        public DspParameter WetDryMixPct;
    }
}