#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content
{
    public sealed class ContentTypeReaderManager
    {
        ContentReader _reader;
        ContentTypeReader[] contentReaders;		
		
		static string assemblyName;
		
		static ContentTypeReaderManager()
		{
#if WINRT
            assemblyName = typeof(ContentTypeReaderManager).GetTypeInfo().Assembly.FullName;
#else
			assemblyName = Assembly.GetExecutingAssembly().FullName;
#endif
        }

        public ContentTypeReaderManager(ContentReader reader)
        {
            _reader = reader;
        }

        public ContentTypeReader GetTypeReader(Type targetType)
        {
            foreach (ContentTypeReader r in contentReaders)
            {
                if (targetType == r.TargetType) return r;
            }
            return null;
        }

        // Trick to prevent the linker removing the code, but not actually execute the code
        static bool falseflag = false;

		internal ContentTypeReader[] LoadAssetReaders()
        {
#pragma warning disable 0219, 0649
            // Trick to prevent the linker removing the code, but not actually execute the code
            if (falseflag)
            {
                // Dummy variables required for it to work on iDevices ** DO NOT DELETE ** 
                // This forces the classes not to be optimized out when deploying to iDevices
                var hByteReader = new ByteReader();
                var hSByteReader = new SByteReader();
                var hDateTimeReader = new DateTimeReader();
                var hDecimalReader = new DecimalReader();
                var hBoundingSphereReader = new BoundingSphereReader();
                var hBoundingFrustumReader = new BoundingFrustumReader();
                var hRayReader = new RayReader();
                var hCharListReader = new ListReader<Char>();
                var hRectangleListReader = new ListReader<Rectangle>();
                var hRectangleArrayReader = new ArrayReader<Rectangle>();
                var hVector3ListReader = new ListReader<Vector3>();
                var hStringListReader = new ListReader<StringReader>();
				var hIntListReader = new ListReader<Int32>();
                var hSpriteFontReader = new SpriteFontReader();
                var hTexture2DReader = new Texture2DReader();
                var hCharReader = new CharReader();
                var hRectangleReader = new RectangleReader();
                var hStringReader = new StringReader();
                var hVector2Reader = new Vector2Reader();
                var hVector3Reader = new Vector3Reader();
                var hVector4Reader = new Vector4Reader();
                var hCurveReader = new CurveReader();
                var hIndexBufferReader = new IndexBufferReader();
                var hBoundingBoxReader = new BoundingBoxReader();
                var hMatrixReader = new MatrixReader();
                var hBasicEffectReader = new BasicEffectReader();
                var hVertexBufferReader = new VertexBufferReader();
                var hAlphaTestEffectReader = new AlphaTestEffectReader();
                var hEnumSpriteEffectsReader = new EnumReader<Graphics.SpriteEffects>();
                var hArrayFloatReader = new ArrayReader<float>();
                var hArrayVector2Reader = new ArrayReader<Vector2>();
                var hListVector2Reader = new ListReader<Vector2>();
                var hArrayMatrixReader = new ArrayReader<Matrix>();
                var hEnumBlendReader = new EnumReader<Graphics.Blend>();
                var hNullableRectReader = new NullableReader<Rectangle>();
				var hEffectMaterialReader = new EffectMaterialReader();
				var hExternalReferenceReader = new ExternalReferenceReader();
            }
#pragma warning restore 0219, 0649

            int numberOfReaders;
			
            // The first content byte i read tells me the number of content readers in this XNB file
            numberOfReaders = _reader.Read7BitEncodedInt();
            contentReaders = new ContentTypeReader[numberOfReaders];
		
            // For each reader in the file, we read out the length of the string which contains the type of the reader,
            // then we read out the string. Finally we instantiate an instance of that reader using reflection
            for (int i = 0; i < numberOfReaders; i++)
            {
                // This string tells us what reader we need to decode the following data
                // string readerTypeString = reader.ReadString();
				string originalReaderTypeString = _reader.ReadString();

                Func<ContentTypeReader> readerFunc;
                if (typeCreators.TryGetValue(originalReaderTypeString, out readerFunc))
                {
                    contentReaders[i] = readerFunc();
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine(originalReaderTypeString);

    				// Need to resolve namespace differences
    				string readerTypeString = originalReaderTypeString;

    				readerTypeString = PrepareType(readerTypeString);

    				var l_readerType = Type.GetType(readerTypeString);
                    if (l_readerType != null)
                    {
                        try
                        {
                            contentReaders[i] = l_readerType.GetDefaultConstructor().Invoke(null) as ContentTypeReader;
                        }
                        catch (TargetInvocationException ex)
                        {
                            // If you are getting here, the Mono runtime is most likely not able to JIT the type.
                            // In particular, MonoTouch needs help instantiating types that are only defined in strings in Xnb files. 
                            throw new InvalidOperationException(
                                "Failed to get default constructor for ContentTypeReader. To work around, add a creation function to ContentTypeReaderManager.AddTypeCreator() " +
                                "with the following failed type string: " + originalReaderTypeString);
                        }
                    }
                    else
                        throw new ContentLoadException("Could not find matching content reader of type " + originalReaderTypeString + " (" + readerTypeString + ")");
                }

				// I think the next 4 bytes refer to the "Version" of the type reader,
                // although it always seems to be zero
                int typeReaderVersion = _reader.ReadInt32();
            }

            return contentReaders;
        }
		
		/// <summary>
		/// Removes Version, Culture and PublicKeyToken from a type string.
		/// </summary>
		/// <remarks>
		/// Supports multiple generic types (e.g. Dictionary<TKey,TValue>) and nested generic types (e.g. List<List<int>>).
		/// </remarks> 
		/// <param name="type">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string PrepareType(string type)
		{			
			//Needed to support nested types
			int count = type.Split(new[] {"[["}, StringSplitOptions.None).Length - 1;
			
			string preparedType = type;
			
			for(int i=0; i<count; i++)
			{
				preparedType = Regex.Replace(preparedType, @"\[(.+?), Version=.+?\]", "[$1]");
			}
						
			//Handle non generic types
			if(preparedType.Contains("PublicKeyToken"))
				preparedType = Regex.Replace(preparedType, @"(.+?), Version=.+?$", "$1");

			// TODO: For WinRT this is most likely broken!
			preparedType = preparedType.Replace(", Microsoft.Xna.Framework.Graphics", string.Format(", {0}", assemblyName));
			preparedType = preparedType.Replace(", Microsoft.Xna.Framework", string.Format(", {0}", assemblyName));
			
			return preparedType;
		}

        // Static map of type names to creation functions. Required as iOS requires all types at compile time
        private static Dictionary<string, Func<ContentTypeReader>> typeCreators = new Dictionary<string, Func<ContentTypeReader>>();

        /// <summary>
        /// Adds the type creator.
        /// </summary>
        /// <param name='typeString'>
        /// Type string.
        /// </param>
        /// <param name='createFunction'>
        /// Create function.
        /// </param>
        public static void AddTypeCreator(string typeString, Func<ContentTypeReader> createFunction)
        {
            if (!typeCreators.ContainsKey(typeString))
                typeCreators.Add(typeString, createFunction);
        }

        public static void ClearTypeCreators()
        {
            typeCreators.Clear();
        }

    }
}
