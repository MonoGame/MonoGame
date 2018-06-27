// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;


namespace MonoGame.Tests.Graphics
{
    [TestFixture]
    class ProfileReachTest
    {
        const GraphicsProfile TestedProfile = GraphicsProfile.Reach;
        const int PowerOfTwoSize = 32;
        const int MaxTexture2DSize = 2048;
        const int MaxTextureCubeSize = 512;
        const int MaxPrimitives = 65535;
        const int MaxRenderTargets = 1;

        private TestGameBase _game;
        private GraphicsDeviceManager _gdm;
        private GraphicsDevice _gd;


        [SetUp]
        public virtual void SetUp()
        {
            _game = new TestGameBase();
            _gdm = new GraphicsDeviceManager(_game);
            _gdm.GraphicsProfile = GraphicsProfile.Reach;
            _game.InitializeOnly();
            _gd = _gdm.GraphicsDevice;
        }

        [TearDown]
        public virtual void TearDown()
        {
            _game.Dispose();
            _game = null;
            _gdm = null;
            _gd = null;
        }
        
        /// <summary>
        /// Ensure we have the correct profile
        /// </summary>
        private void CheckProfile()
        {
            Assert.AreEqual(_gd.GraphicsProfile, TestedProfile);
        }


        [TestCase(MaxTexture2DSize,   MaxTexture2DSize,   false, SurfaceFormat.Color)]
        [TestCase(MaxTexture2DSize,   MaxTexture2DSize-1, false, SurfaceFormat.Color)]
        [TestCase(MaxTexture2DSize-1, MaxTexture2DSize  , false, SurfaceFormat.Color)]
        [TestCase(MaxTexture2DSize-1, MaxTexture2DSize-1, false, SurfaceFormat.Color)]
        [TestCase(PowerOfTwoSize,   PowerOfTwoSize,   true,  SurfaceFormat.Color)]
        [TestCase(PowerOfTwoSize,   PowerOfTwoSize,   false, SurfaceFormat.Dxt1 )]
        [TestCase(PowerOfTwoSize,   PowerOfTwoSize,   false, SurfaceFormat.Dxt3 )]     
        [TestCase(PowerOfTwoSize,   PowerOfTwoSize,   false, SurfaceFormat.Dxt5 )]
        public void Texture2DSize(int width, int height, bool mipMap, SurfaceFormat surfaceFormat = SurfaceFormat.Color)
        {
            CheckProfile();

            Texture2D tx = new Texture2D(_gd, width, height, mipMap, surfaceFormat);
            tx.Dispose();
        }

        
        [TestCase(MaxTexture2DSize,   MaxTexture2DSize+1, false, SurfaceFormat.Color)]
        [TestCase(MaxTexture2DSize+1, MaxTexture2DSize,   false, SurfaceFormat.Color)]
        [TestCase(MaxTexture2DSize+1, MaxTexture2DSize+1, false, SurfaceFormat.Color)]
        [TestCase(PowerOfTwoSize,   PowerOfTwoSize-1, true,  SurfaceFormat.Color)]
        [TestCase(PowerOfTwoSize-1, PowerOfTwoSize  , true,  SurfaceFormat.Color)]
        [TestCase(PowerOfTwoSize-1, PowerOfTwoSize-1, true,  SurfaceFormat.Color)]
        [TestCase(PowerOfTwoSize,   PowerOfTwoSize-4, false, SurfaceFormat.Dxt1)]
        [TestCase(PowerOfTwoSize-4, PowerOfTwoSize  , false, SurfaceFormat.Dxt1)]
        [TestCase(PowerOfTwoSize-4, PowerOfTwoSize-4, false, SurfaceFormat.Dxt1)]
        [TestCase(PowerOfTwoSize,   PowerOfTwoSize-4, false, SurfaceFormat.Dxt3)]
        [TestCase(PowerOfTwoSize-4, PowerOfTwoSize  , false, SurfaceFormat.Dxt3)]
        [TestCase(PowerOfTwoSize-4, PowerOfTwoSize-4, false, SurfaceFormat.Dxt3)] 
        [TestCase(PowerOfTwoSize,   PowerOfTwoSize-4, false, SurfaceFormat.Dxt5)]
        [TestCase(PowerOfTwoSize-4, PowerOfTwoSize  , false, SurfaceFormat.Dxt5)]
        [TestCase(PowerOfTwoSize-4, PowerOfTwoSize-4, false, SurfaceFormat.Dxt5)]
        public void Texture2DSize_ThrowsNotSupportedException(int width, int height, bool mipMap, SurfaceFormat surfaceFormat = SurfaceFormat.Color)
        {
            Assert.Throws<NotSupportedException>( ()=> Texture2DSize(width, height, mipMap, surfaceFormat) );
        }
        
        [TestCase(MaxTextureCubeSize  )]
        [TestCase(MaxTextureCubeSize/2)]
        public void TextureCubeSize(int size)
        {
            CheckProfile();
            
            TextureCube tx = new TextureCube(_gd, size, false, SurfaceFormat.Color);
            tx.Dispose();
        }
        
        [TestCase(MaxTextureCubeSize*2)]        
        [TestCase(MaxTextureCubeSize+1)] // nonPowerOfTwo or maxSize
        [TestCase(MaxTextureCubeSize+4)] // nonPowerOfTwo or maxSize
        [TestCase(PowerOfTwoSize-4    )]
        public void TextureCubeSize_ThrowsNotSupportedException(int size)
        {
            Assert.Throws<NotSupportedException>( ()=> TextureCubeSize(size) );
        }
        
        [TestCase(16, 16, 16)]
        public void Texture3DSize_ThrowsNotSupportedException(int width, int height, int depth)
        {
            CheckProfile();
                        
            Assert.Throws<NotSupportedException>(()=>
            { 
                Texture3D tx = new Texture3D(_gd, width, height, depth, false, SurfaceFormat.Color);            
                tx.Dispose();
            });
        }
        
        [TestCase(SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Bgr565)]
        [TestCase(SurfaceFormat.Bgra5551)]
        [TestCase(SurfaceFormat.Bgra4444)]
        [TestCase(SurfaceFormat.Dxt1)]
        [TestCase(SurfaceFormat.Dxt3)]
        [TestCase(SurfaceFormat.Dxt5)]
        [TestCase(SurfaceFormat.NormalizedByte2)]
        [TestCase(SurfaceFormat.NormalizedByte4)]
        public void Texture2DSurface(SurfaceFormat surfaceFormat)
        {
            CheckProfile();

            Texture2D tx = new Texture2D(_gd, 16, 16, false, surfaceFormat);
            tx.Dispose();
        }

        [TestCase(SurfaceFormat.Rgba1010102)]
        [TestCase(SurfaceFormat.Rg32)]
        [TestCase(SurfaceFormat.Rgba64)]
        [TestCase(SurfaceFormat.Alpha8)]
        [TestCase(SurfaceFormat.Single)]
        [TestCase(SurfaceFormat.Vector2)]
        [TestCase(SurfaceFormat.Vector4)]
        [TestCase(SurfaceFormat.HalfSingle)]
        [TestCase(SurfaceFormat.HalfVector2)]
        [TestCase(SurfaceFormat.HalfVector4)]
        [TestCase(SurfaceFormat.HdrBlendable)]
        public void Texture2DSurface_ThrowsNotSupportedException(SurfaceFormat surfaceFormat)
        {
            Assert.Throws<NotSupportedException>(() => Texture2DSurface(surfaceFormat));
        }

        [TestCase(SurfaceFormat.Color)]
        public void Texture3DSurface_ThrowsNotSupportedException(SurfaceFormat surfaceFormat)
        {
            CheckProfile();
                        
            Assert.Throws<NotSupportedException>(()=>
            {
                Texture3D tx = new Texture3D(_gd, 16, 16, 16, false, surfaceFormat);
                tx.Dispose();                
            });
        }

        [TestCase(SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Bgr565)]
        [TestCase(SurfaceFormat.Bgra5551)]
        [TestCase(SurfaceFormat.Bgra4444)]
        [TestCase(SurfaceFormat.Dxt1)]
        [TestCase(SurfaceFormat.Dxt3)]
        [TestCase(SurfaceFormat.Dxt5)]
        public void TextureCubeSurface(SurfaceFormat surfaceFormat)
        {
            CheckProfile();

            TextureCube tx = new TextureCube(_gd, 16, false, surfaceFormat);
            tx.Dispose();
        }

        [TestCase(SurfaceFormat.NormalizedByte2)]
        [TestCase(SurfaceFormat.NormalizedByte4)]
        [TestCase(SurfaceFormat.Rgba1010102)]
        [TestCase(SurfaceFormat.Rg32)]
        [TestCase(SurfaceFormat.Rgba64)]
        [TestCase(SurfaceFormat.Alpha8)]
        [TestCase(SurfaceFormat.Single)]
        [TestCase(SurfaceFormat.Vector2)]
        [TestCase(SurfaceFormat.Vector4)]
        [TestCase(SurfaceFormat.HalfSingle)]
        [TestCase(SurfaceFormat.HalfVector2)]
        [TestCase(SurfaceFormat.HalfVector4)]
        [TestCase(SurfaceFormat.HdrBlendable)]
        public void TextureCubeSurface_ThrowsNotSupportedException(SurfaceFormat surfaceFormat)
        {
            Assert.Throws<NotSupportedException>( ()=> TextureCubeSurface(surfaceFormat) );
        }

        [TestCase(SurfaceFormat.Color, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Rgba1010102, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Rg32, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Rgba64, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Alpha8, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Single, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Vector2, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Vector4, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.HalfSingle, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.HalfVector2, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.HalfVector4, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.HdrBlendable, SurfaceFormat.Color)]
        public void RenderTarget2DSurface(SurfaceFormat preferredFormat, SurfaceFormat expectedFallbackFormat)
        {
            CheckProfile();

            RenderTarget2D tx = new RenderTarget2D(_gd, 16, 16, false, preferredFormat, DepthFormat.None);
            Assert.AreEqual(tx.Format, expectedFallbackFormat);
            tx.Dispose();
        }

        [TestCase(SurfaceFormat.Color, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Rgba1010102, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Rg32, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Rgba64, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Alpha8, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Single, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Vector2, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Vector4, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.HalfSingle, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.HalfVector2, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.HalfVector4, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.HdrBlendable, SurfaceFormat.Color)]
        public void RenderTargetCubeSurface(SurfaceFormat preferredFormat, SurfaceFormat expectedFallbackFormat)
        {
            CheckProfile();

            RenderTargetCube tx = new RenderTargetCube(_gd, 16, false, preferredFormat, DepthFormat.None);
            Assert.AreEqual(tx.Format, expectedFallbackFormat);
            tx.Dispose();
        }
        
        [TestCase("DrawPrimitives", 0, MaxPrimitives)]
        [TestCase("DrawPrimitives", 3, MaxPrimitives)]
        [TestCase("DrawIndexedPrimitives", 0, MaxPrimitives)]
        [TestCase("DrawUserPrimitives", 0, MaxPrimitives)]
        [TestCase("DrawUserPrimitives", 3, MaxPrimitives)]
        [TestCase("DrawUserIndexedPrimitives", 0, MaxPrimitives)]
        public void MaximumPrimitivesPerDrawCall(string method, int vertexStart, int primitiveCount)
        {
            CheckProfile();

            int verticesCount = vertexStart + 3*primitiveCount;
            var effect = new BasicEffect(_gd);
            effect.CurrentTechnique.Passes[0].Apply();
            
            switch(method)
            {
                case "DrawPrimitives":
                    var vb = new VertexBuffer(_gd, VertexPositionColor.VertexDeclaration, verticesCount, BufferUsage.None);
                    _gd.SetVertexBuffer(vb);
                    _gd.DrawPrimitives(PrimitiveType.TriangleList, vertexStart, primitiveCount);
                    vb.Dispose();
                    break;
                case "DrawIndexedPrimitives":
                    var vb2 = new VertexBuffer(_gd, VertexPositionColor.VertexDeclaration, verticesCount, BufferUsage.None);
                    var ib2 = new IndexBuffer(_gd, IndexElementSize.SixteenBits, verticesCount, BufferUsage.None);
                    _gd.SetVertexBuffer(vb2);
                    _gd.Indices = ib2;
                    _gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, vertexStart, verticesCount, 0, primitiveCount);
                    vb2.Dispose();
                    ib2.Dispose();
                    break;
                case "DrawUserPrimitives":
                    var vertices = new VertexPositionColor[verticesCount];
                    _gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, vertexStart, primitiveCount);
                    break;
                case "DrawUserIndexedPrimitives":
                    var vertices2 = new VertexPositionColor[verticesCount];
                    var indices16bit = new short[verticesCount];
                    _gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices2, 0, vertices2.Length, indices16bit, 0, primitiveCount);
                    break;
                case "DrawUserIndexedPrimitives_32bit":
                    var vertices3 = new VertexPositionColor[verticesCount];
                    var indices32bit = new int[verticesCount];
                    _gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices3, 0, vertices3.Length, indices32bit, 0, primitiveCount);
                    break;
                default:
                    throw new ArgumentException("method");
            }
            
            effect.Dispose();
        }

        [TestCase("DrawPrimitives", 0, MaxPrimitives + 1)]
        [TestCase("DrawIndexedPrimitives", 0, MaxPrimitives + 1)]
        [TestCase("DrawUserPrimitives", 0, MaxPrimitives + 1)]
        [TestCase("DrawUserIndexedPrimitives", 0, MaxPrimitives + 1)]
        public void MaximumPrimitivesPerDrawCall_ThrowsNotSupportedException(string method, int vertexStart, int primitiveCount)
        {
            Assert.Throws<NotSupportedException>( ()=> MaximumPrimitivesPerDrawCall(method, vertexStart, primitiveCount) );
        }

        [TestCase(IndexElementSize.SixteenBits)]
        public void IndexBufferElementSize(IndexElementSize elementSize)
        {
            CheckProfile();

            IndexBuffer ib = new IndexBuffer(_gd, elementSize, 16, BufferUsage.None);
            ib.Dispose();
        }

        [TestCase(IndexElementSize.ThirtyTwoBits)]
        public void IndexBufferElementSize_ThrowsNotSupportedException(IndexElementSize elementSize)
        {
            Assert.Throws<NotSupportedException>( ()=> IndexBufferElementSize(elementSize) );
        }

        [TestCase(IndexElementSize.SixteenBits)]
        public void IndicesElementSize(IndexElementSize elementSize)
        {
            CheckProfile();

            int verticesCount = 3 * 16;
            var effect = new BasicEffect(_gd);
            effect.CurrentTechnique.Passes[0].Apply();
            var vertices = new VertexPositionColor[verticesCount];

            switch (elementSize)
            {
                case IndexElementSize.SixteenBits:
                    var indices16bit = new short[3 * 16];
                    _gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices16bit, 0, 16);
                    break;
                case IndexElementSize.ThirtyTwoBits:
                    var indices32bit = new int[3 * 16];
                    _gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices32bit, 0, 16);
                    break;
                default:
                    throw new ArgumentException("method");
            }
            
            effect.Dispose();
        }
        
        [TestCase(IndexElementSize.ThirtyTwoBits)]
        public void IndicesElementSize_ThrowsNotSupportedException(IndexElementSize elementSize)
        {            
            Assert.Throws<NotSupportedException>( ()=> IndicesElementSize(elementSize) );
        }

        [TestCase()]
        public void OcclusionQuery_ThrowsNotSupportedException()
        {
            CheckProfile();

            Assert.Throws<NotSupportedException>(()=>
            { 
                var oc = new OcclusionQuery(_gd);
                oc.Dispose();
            });
        }

        [TestCase(MaxRenderTargets)]
        public void MultipleRenderTargets(int count)
        {
            CheckProfile();
            
            var rtBinding = new RenderTargetBinding[count];
            for(int i=0;i<count;i++)
                rtBinding[i] = new RenderTargetBinding(new RenderTarget2D(_gd, PowerOfTwoSize, PowerOfTwoSize));

            _gd.SetRenderTargets(rtBinding);

            _gd.SetRenderTarget(null);
            for(int i=0;i<count;i++)
                rtBinding[i].RenderTarget.Dispose();
        }

        [TestCase(MaxRenderTargets + 1)]
        public void MultipleRenderTargets_ThrowsNotSupportedException(int count)
        {
            Assert.Throws<NotSupportedException>( ()=> MultipleRenderTargets(count) );
        }
    }
}
