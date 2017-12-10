using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextureSquish
{
    [Flags]
    public enum CompressionMode
    {
        /// <summary>
        /// Use DXT1 compression.
        /// </summary>
        Dxt1 = 1,

        /// <summary>
        /// Use DXT3 compression.
        /// </summary>
        Dxt3 = 2,

        /// <summary>
        /// Use DXT5 compression.
        /// </summary>
        Dxt5 = 4,

        /// <summary>
        /// Use a fast but low quality colour compressor.
        /// </summary>
        ColourRangeFit = 16,

        /// <summary>
        /// Use a slow but high quality colour compressor (the default).
        /// </summary>
        ColourClusterFit = 32,

        /// <summary>
        /// Use a very slow but very high quality colour compressor.
        /// </summary>
        ColourIterativeClusterFit = 64,        

        /// <summary>
        /// Use a perceptual metric for colour error (the default).
        /// </summary>
        ColourMetricPerceptual = 256,

        /// <summary>
        /// Use a uniform metric for colour error.
        /// </summary>
        ColourMetricUniform = 512,

        /// <summary>
        /// Weight the colour by alpha during cluster fit (disabled by default).
        /// </summary>
        WeightColourByAlpha = 1024,

        /// <summary>
        /// Uses multithreading to increase compression speed.
        /// </summary>
        UseParallelProcessing = 2048,        
    }

    static class CompressionModeUtils
    {
        public static CompressionMode FixFlags(this CompressionMode flags)
        {
            // grab the flag bits
            var method = flags & (CompressionMode.Dxt1 | CompressionMode.Dxt3 | CompressionMode.Dxt5);
            var fit = flags & (CompressionMode.ColourIterativeClusterFit | CompressionMode.ColourClusterFit | CompressionMode.ColourRangeFit);
            var metric = flags & (CompressionMode.ColourMetricPerceptual | CompressionMode.ColourMetricUniform);
            var extra = flags & (CompressionMode.WeightColourByAlpha | CompressionMode.UseParallelProcessing);

            // set defaults
            if (method != CompressionMode.Dxt3 && method != CompressionMode.Dxt5) method = CompressionMode.Dxt1;
            if (fit != CompressionMode.ColourRangeFit) fit = CompressionMode.ColourClusterFit;
            if (metric != CompressionMode.ColourMetricUniform) metric = CompressionMode.ColourMetricPerceptual;

            // done
            return method | fit | metric | extra;
        }        
    }
}
