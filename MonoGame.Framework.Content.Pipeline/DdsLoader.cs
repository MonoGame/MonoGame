// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.IO;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Loader class for DDS format image files.
    /// </summary>
    class DdsLoader
    {
        enum Ddsd : uint
        {
            Caps = 0x1,             // Required in every DDS file
            Height = 0x2,           // Required in every DDS file
            Width = 0x4,            // Required in every DDS file
            Pitch = 0x8,            // Required when pitch is provided for an uncompressed texture
            PixelFormat = 0x1000,   // Required in every DDS file
            MipMapCount = 0x2000,   // Required in a mipmapped texture
            LinearSize = 0x80000,   // Required when pitch is provided for a compressed texture
            Depth = 0x800000,       // Required in a depth texture
        }

        enum DdsCaps : uint
        {
            Complex = 0x8,       // Optional; must be used on any file that contains more than one surface (a mipmap, a cubic environment map, or mipmapped volume texture)
            MipMap = 0x400000,   // Optional; should be used for a mipmap
            Texture = 0x1000,    // Required
        }

        enum DdsCaps2 : uint
        {
            Cubemap = 0x200,
            CubemapPositiveX = 0x400,
            CubemapNegativeX = 0x800,
            CubemapPositiveY = 0x1000,
            CubemapNegativeY = 0x2000,
            CubemapPositiveZ = 0x4000,
            CubemapNegativeZ = 0x8000,
            Volume = 0x200000,

            CubemapAllFaces = Cubemap | CubemapPositiveX | CubemapNegativeX | CubemapPositiveY | CubemapNegativeY | CubemapPositiveZ | CubemapNegativeZ,
        }

        enum Ddpf : uint
        {
            AlphaPixels = 0x1,
            Alpha = 0x2,
            FourCC = 0x4,
            Rgb = 0x40,
            Yuv = 0x200,
            Luminance = 0x20000,
        }

        static uint MakeFourCC(char c1, char c2, char c3, char c4)
        {
            return ((uint)c1 << 24) | ((uint)c2 << 16) | ((uint)c3 << 8) | (uint)c4;
        }

        static uint MakeFourCC(string cc)
        {
            return ((uint)cc[0] << 24) | ((uint)cc[1] << 16) | ((uint)cc[2] << 8) | (uint)cc[3];
        }

        enum FourCC : uint
        {
            Dxt1 = 0x31545844,
            Dxt2 = 0x32545844,
            Dxt3 = 0x33545844,
            Dxt4 = 0x34545844,
            Dxt5 = 0x35545844,
            Dx10 = 0x30315844,
        }

        struct DdsPixelFormat
        {
            public uint dwSize;
            public Ddpf dwFlags;
            public FourCC dwFourCC;
            public uint dwRgbBitCount;
            public uint dwRBitMask;
            public uint dwGBitMask;
            public uint dwBBitMask;
            public uint dwABitMask;
        }

        struct DdsHeader
        {
            public uint dwSize;
            public Ddsd dwFlags;
            public uint dwHeight;
            public uint dwWidth;
            public uint dwPitchOrLinearSize;
            public uint dwDepth;
            public uint dwMipMapCount;
            public DdsPixelFormat ddspf;
            public DdsCaps dwCaps;
            public DdsCaps2 dwCaps2;
        }

        static SurfaceFormat GetSurfaceFormat(ref DdsPixelFormat pixelFormat, out bool rbSwap)
        {
            rbSwap = false;
            if (pixelFormat.dwFlags.HasFlag(Ddpf.FourCC))
            {
                // It is a compressed format
                switch (pixelFormat.dwFourCC)
                {
                    case FourCC.Dxt1:
                        return SurfaceFormat.Dxt1;
                    case FourCC.Dxt2:
                        throw new ContentLoadException("Unsupported compression format DXT2");
                    case FourCC.Dxt3:
                        return SurfaceFormat.Dxt3;
                    case FourCC.Dxt4:
                        throw new ContentLoadException("Unsupported compression format DXT4");
                    case FourCC.Dxt5:
                        return SurfaceFormat.Dxt5;
                }
            }
            else if (pixelFormat.dwFlags.HasFlag(Ddpf.Rgb))
            {
                // Uncompressed format
                if (pixelFormat.dwFlags.HasFlag(Ddpf.AlphaPixels))
                {
                    // Format contains RGB and A
                    if (pixelFormat.dwRgbBitCount == 16)
                    {
                        if (pixelFormat.dwABitMask == 0xF)
                        {
                            rbSwap = pixelFormat.dwBBitMask == 0xF0;
                            return SurfaceFormat.Bgra4444;
                        }
                        rbSwap = pixelFormat.dwBBitMask == 0x3E;
                        return SurfaceFormat.Bgra5551;
                    }
                    else if (pixelFormat.dwRgbBitCount == 32)
                    {
                        rbSwap = pixelFormat.dwBBitMask == 0xFF00;
                        return SurfaceFormat.Color;
                    }
                    throw new ContentLoadException("Unsupported RGBA pixel format");
                }
                else
                {
                    // Format contains RGB only
                    if (pixelFormat.dwRgbBitCount == 16)
                    {
                        rbSwap = pixelFormat.dwBBitMask == 0x1F;
                        return SurfaceFormat.Bgr565;
                    }
                    throw new ContentLoadException("Unsupported RGB pixel format");
                }
            }
            throw new ContentLoadException("Unsupported pixel format");
        }

        static BitmapContent CreateBitmapContent(SurfaceFormat format, int width, int height)
        {
            switch (format)
            {
                case SurfaceFormat.Color:
                    return new PixelBitmapContent<Color>(width, height);

                case SurfaceFormat.Bgra4444:
                    return new PixelBitmapContent<Bgra4444>(width, height);

                case SurfaceFormat.Bgra5551:
                    return new PixelBitmapContent<Bgra5551>(width, height);

                case SurfaceFormat.Bgr565:
                    return new PixelBitmapContent<Bgr565>(width, height);

                case SurfaceFormat.Dxt1:
                    return new Dxt1BitmapContent(width, height);

                case SurfaceFormat.Dxt3:
                    return new Dxt3BitmapContent(width, height);

                case SurfaceFormat.Dxt5:
                    return new Dxt5BitmapContent(width, height);
            }
            throw new ContentLoadException("Unsupported SurfaceFormat " + format);
        }

        static int GetBitmapSize(SurfaceFormat format, int width, int height)
        {
            // It is recommended that the dwPitchOrLinearSize field is ignored and we calculate it ourselves
            // https://msdn.microsoft.com/en-us/library/bb943991.aspx
            int pitch = 0;
            int rows = 0;
            if (format == SurfaceFormat.Dxt1)
            {
                pitch = MathHelper.Max(1, ((width + 3) / 4)) * 8;
                rows = (height + 3) / 4;
            }
            else if (format == SurfaceFormat.Dxt3 || format == SurfaceFormat.Dxt5)
            {
                pitch = MathHelper.Max(1, ((width + 3) / 4)) * 16;
                rows = (height + 3) / 4;
            }
            else if (format == SurfaceFormat.Color)
            {
                pitch = (width * 32 + 7) / 8;
                rows = height;
            }
            return pitch * rows;
        }

        static internal TextureContent Import(string filename, ContentImporterContext context)
        {
            var identity = new ContentIdentity(filename);
            TextureContent output = null;

            using (var reader = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                // Read signature ("DDS ")
                var valid = reader.ReadByte() == 0x44;
                valid = valid && reader.ReadByte() == 0x44;
                valid = valid && reader.ReadByte() == 0x53;
                valid = valid && reader.ReadByte() == 0x20;
                if (!valid)
                    throw new ContentLoadException("Invalid file signature");

                var header = new DdsHeader();

                // Read DDS_HEADER
                header.dwSize = reader.ReadUInt32();
                if (header.dwSize != 124)
                    throw new ContentLoadException("Invalid DDS_HEADER dwSize value");
                header.dwFlags = (Ddsd)reader.ReadUInt32();
                header.dwHeight = reader.ReadUInt32();
                header.dwWidth = reader.ReadUInt32();
                header.dwPitchOrLinearSize = reader.ReadUInt32();
                header.dwDepth = reader.ReadUInt32();
                header.dwMipMapCount = reader.ReadUInt32();
                // The next 11 DWORDs are reserved and unused
                for (int i = 0; i < 11; ++i)
                    reader.ReadUInt32();
                // Read DDS_PIXELFORMAT
                header.ddspf.dwSize = reader.ReadUInt32();
                if (header.ddspf.dwSize != 32)
                    throw new ContentLoadException("Invalid DDS_PIXELFORMAT dwSize value");
                header.ddspf.dwFlags = (Ddpf)reader.ReadUInt32();
                header.ddspf.dwFourCC = (FourCC)reader.ReadUInt32();
                header.ddspf.dwRgbBitCount = reader.ReadUInt32();
                header.ddspf.dwRBitMask = reader.ReadUInt32();
                header.ddspf.dwGBitMask = reader.ReadUInt32();
                header.ddspf.dwBBitMask = reader.ReadUInt32();
                header.ddspf.dwABitMask = reader.ReadUInt32();
                // Continue reading DDS_HEADER
                header.dwCaps = (DdsCaps)reader.ReadUInt32();
                header.dwCaps2 = (DdsCaps2)reader.ReadUInt32();
                // dwCaps3 unused
                reader.ReadUInt32();
                // dwCaps4 unused
                reader.ReadUInt32();
                // dwReserved2 unused
                reader.ReadUInt32();

                // Check for the existence of the DDS_HEADER_DXT10 struct next
                if (header.ddspf.dwFlags == Ddpf.FourCC && header.ddspf.dwFourCC == FourCC.Dx10)
                {
                    throw new ContentLoadException("Unsupported DDS_HEADER_DXT10 struct found");
                }

                int faceCount = 1;
                int mipMapCount = (int)(header.dwCaps.HasFlag(DdsCaps.MipMap) ? header.dwMipMapCount : 1);
                if (header.dwCaps.HasFlag(DdsCaps.Complex))
                {
                    if (header.dwCaps2.HasFlag(DdsCaps2.Cubemap))
                    {
                        if (!header.dwCaps2.HasFlag(DdsCaps2.CubemapAllFaces))
                            throw new ContentLoadException("Incomplete cubemap in DDS file");
                        faceCount = 6;
                        output = new TextureCubeContent() { Identity = identity };
                    }
                    else
                    {
                        output = new Texture2DContent() { Identity = identity };
                    }
                }
                else
                {
                    output = new Texture2DContent() { Identity = identity };
                }

                bool rbSwap;
                var format = GetSurfaceFormat(ref header.ddspf, out rbSwap);

                for (int f = 0; f < faceCount; ++f)
                {
                    var w = (int)header.dwWidth;
                    var h = (int)header.dwHeight;
                    var mipMaps = new MipmapChain();
                    for (int m = 0; m < mipMapCount; ++m)
                    {
                        var content = CreateBitmapContent(format, w, h);
                        var byteCount = GetBitmapSize(format, w, h);
                        var bytes = reader.ReadBytes(byteCount);
                        content.SetPixelData(bytes);
                        mipMaps.Add(content);
                        w = MathHelper.Max(1, w / 2);
                        h = MathHelper.Max(1, h / 2);
                    }
                    output.Faces[f] = mipMaps;
                }
            }

            return output;
        }
    }
}
