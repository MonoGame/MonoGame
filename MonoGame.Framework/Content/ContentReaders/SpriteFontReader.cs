// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All)]
    internal class SpriteFontReader : ContentTypeReader<SpriteFont>
    {
        private const int DistanceFieldMetadataSize = 1 + 4 + 4;

        public SpriteFontReader()
        {
        }

        private static bool HasRemainingBytes(ContentReader input, int bytes)
        {
            var stream = input.BaseStream;
            if (!stream.CanSeek)
                return true;

            return stream.Length - stream.Position >= bytes;
        }

        private static bool TryReadHasDistanceFieldBlock(ContentReader input, out bool hasDistanceField)
        {
            hasDistanceField = false;
            if (!HasRemainingBytes(input, 1))
                return false;

            try
            {
                hasDistanceField = input.ReadBoolean();
                return true;
            }
            catch (EndOfStreamException)
            {
                return false;
            }
        }

        private static void ReadOptionalDistanceFieldMetadata(ContentReader input, SpriteFont font)
        {
            if (!TryReadHasDistanceFieldBlock(input, out var hasDistanceField) || !hasDistanceField)
                return;

            if (!HasRemainingBytes(input, DistanceFieldMetadataSize))
                throw new ContentLoadException("Invalid SpriteFont distance field metadata block.");

            var dfType = input.ReadByte();
            if (dfType != (byte)SpriteFont.DistanceFieldType.SDF)
                throw new ContentLoadException($"Unsupported SpriteFont distance field type '{dfType}'. Rebuild the asset with SDF-only distance field output.");

            var spread = input.ReadSingle();
            var emSize = input.ReadSingle();

            if (font != null)
            {
                font._distanceFieldType = SpriteFont.DistanceFieldType.SDF;
                font._distanceFieldSpread = spread;
                font._emSize = emSize;
            }
        }

        protected internal override SpriteFont Read(ContentReader input, SpriteFont existingInstance)
        {
            if (existingInstance != null)
            {
                // Read the texture into the existing texture instance
                input.ReadObject<Texture2D>(existingInstance.Texture);
                
                // discard the rest of the SpriteFont data as we are only reloading GPU resources for now
                input.ReadObject<List<Rectangle>>();
                input.ReadObject<List<Rectangle>>();
                input.ReadObject<List<char>>();
                input.ReadInt32();
                input.ReadSingle();
                input.ReadObject<List<Vector3>>();
                if (input.ReadBoolean())
                {
                    input.ReadChar();
                }

                ReadOptionalDistanceFieldMetadata(input, null);

                return existingInstance;
            }
            else
            {
                // Create a fresh SpriteFont instance
                Texture2D texture = input.ReadObject<Texture2D>();
                List<Rectangle> glyphs = input.ReadObject<List<Rectangle>>();
                List<Rectangle> cropping = input.ReadObject<List<Rectangle>>();
                List<char> charMap = input.ReadObject<List<char>>();
                int lineSpacing = input.ReadInt32();
                float spacing = input.ReadSingle();
                List<Vector3> kerning = input.ReadObject<List<Vector3>>();
                char? defaultCharacter = null;
                if (input.ReadBoolean())
                {
                    defaultCharacter = new char?(input.ReadChar());
                }
                var font = new SpriteFont(texture, glyphs, cropping, charMap, lineSpacing, spacing, kerning, defaultCharacter);

                ReadOptionalDistanceFieldMetadata(input, font);

                return font;
            }
        }
    }
}
