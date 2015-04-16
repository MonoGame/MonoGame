using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    /// <summary>
    /// Tests for enum compatibility with XNA.
    /// </summary>
    class EnumConformingTest
    {
        #region MonoGame.Framework

        [Test]
        public void ContainmentTypeEnum()
        {
            Assert.AreEqual(0, (int)ContainmentType.Disjoint);
            Assert.AreEqual(1, (int)ContainmentType.Contains);
            Assert.AreEqual(2, (int)ContainmentType.Intersects);
        }

        [Test]
        public void CurveContinuityEnum()
        {
            Assert.AreEqual(0, (int)CurveContinuity.Smooth);
            Assert.AreEqual(1, (int)CurveContinuity.Step);
        }

        [Test]
        public void CurveLoopTypeEnum()
        {
            Assert.AreEqual(0, (int)CurveLoopType.Constant);
            Assert.AreEqual(1, (int)CurveLoopType.Cycle);
            Assert.AreEqual(2, (int)CurveLoopType.CycleOffset);
            Assert.AreEqual(3, (int)CurveLoopType.Oscillate);
            Assert.AreEqual(4, (int)CurveLoopType.Linear);
        }

        [Test]
        public void CurveTangentEnum()
        {
            Assert.AreEqual(0, (int)CurveTangent.Flat);
            Assert.AreEqual(1, (int)CurveTangent.Linear);
            Assert.AreEqual(2, (int)CurveTangent.Smooth);
        }

        [Test]
        public void DisplayOrientationEnum()
        {
            Assert.AreEqual(0, (int)DisplayOrientation.Default);
            Assert.AreEqual(1, (int)DisplayOrientation.LandscapeLeft);
            Assert.AreEqual(2, (int)DisplayOrientation.LandscapeRight);
            Assert.AreEqual(4, (int)DisplayOrientation.Portrait);
        }

        [Test]
        public void PlaneIntersectionTypeEnum()
        {
            Assert.AreEqual(0, (int)PlaneIntersectionType.Front);
            Assert.AreEqual(1, (int)PlaneIntersectionType.Back);
            Assert.AreEqual(2, (int)PlaneIntersectionType.Intersecting);
        }

        [Test]
        public void PlayerIndexEnum()
        {
            Assert.AreEqual(0, (int)PlayerIndex.One);
            Assert.AreEqual(1, (int)PlayerIndex.Two);
            Assert.AreEqual(2, (int)PlayerIndex.Three);
            Assert.AreEqual(3, (int)PlayerIndex.Four);
        }

        #endregion

        #region MonoGame.Framework.Graphics

        [Test]
        public void BlendEnum()
        {
            Assert.AreEqual(0, (int)Blend.One);
            Assert.AreEqual(1, (int)Blend.Zero);
            Assert.AreEqual(2, (int)Blend.SourceColor);
            Assert.AreEqual(3, (int)Blend.InverseSourceColor);
            Assert.AreEqual(4, (int)Blend.SourceAlpha);
            Assert.AreEqual(5, (int)Blend.InverseSourceAlpha);
            Assert.AreEqual(6, (int)Blend.DestinationColor);
            Assert.AreEqual(7, (int)Blend.InverseDestinationColor);
            Assert.AreEqual(8, (int)Blend.DestinationAlpha);
            Assert.AreEqual(9, (int)Blend.InverseDestinationAlpha);
            Assert.AreEqual(10, (int)Blend.BlendFactor);
            Assert.AreEqual(11, (int)Blend.InverseBlendFactor);
            Assert.AreEqual(12, (int)Blend.SourceAlphaSaturation);
        }

        [Test]
        public void BlendFunctionEnum()
        {
            Assert.AreEqual(0, (int)BlendFunction.Add);
            Assert.AreEqual(1, (int)BlendFunction.Subtract);
            Assert.AreEqual(2, (int)BlendFunction.ReverseSubtract);
            Assert.AreEqual(3, (int)BlendFunction.Min);
            Assert.AreEqual(4, (int)BlendFunction.Max);
        }

        [Test]
        public void BufferUsageEnum()
        {
            Assert.AreEqual(0, (int)BufferUsage.None);
            Assert.AreEqual(1, (int)BufferUsage.WriteOnly);
        }

        [Test]
        public void ClearOptionsEnum()
        {
            Assert.AreEqual(1, (int)ClearOptions.Target);
            Assert.AreEqual(2, (int)ClearOptions.DepthBuffer);
            Assert.AreEqual(4, (int)ClearOptions.Stencil);
        }

        [Test]
        public void ColorWriteChannelsEnum()
        {
            Assert.AreEqual(0, (int)ColorWriteChannels.None);
            Assert.AreEqual(1, (int)ColorWriteChannels.Red);
            Assert.AreEqual(2, (int)ColorWriteChannels.Green);
            Assert.AreEqual(4, (int)ColorWriteChannels.Blue);
            Assert.AreEqual(8, (int)ColorWriteChannels.Alpha);
            Assert.AreEqual(15, (int)ColorWriteChannels.All);
        }

        [Test]
        public void CompareFunctionEnum()
        {
            Assert.AreEqual(0, (int)CompareFunction.Always);
            Assert.AreEqual(1, (int)CompareFunction.Never);
            Assert.AreEqual(2, (int)CompareFunction.Less);
            Assert.AreEqual(3, (int)CompareFunction.LessEqual);
            Assert.AreEqual(4, (int)CompareFunction.Equal);
            Assert.AreEqual(5, (int)CompareFunction.GreaterEqual);
            Assert.AreEqual(6, (int)CompareFunction.Greater);
            Assert.AreEqual(7, (int)CompareFunction.NotEqual);
        }

        [Test]
        public void CubeMapFaceEnum()
        {
            Assert.AreEqual(0, (int)CubeMapFace.PositiveX);
            Assert.AreEqual(1, (int)CubeMapFace.NegativeX);
            Assert.AreEqual(2, (int)CubeMapFace.PositiveY);
            Assert.AreEqual(3, (int)CubeMapFace.NegativeY);
            Assert.AreEqual(4, (int)CubeMapFace.PositiveZ);
            Assert.AreEqual(5, (int)CubeMapFace.NegativeZ);
        }

        [Test]
        public void CullModeEnum()
        {
            Assert.AreEqual(0, (int)CullMode.None);
            Assert.AreEqual(1, (int)CullMode.CullClockwiseFace);
            Assert.AreEqual(2, (int)CullMode.CullCounterClockwiseFace);
        }

        [Test]
        public void DepthFormatEnum()
        {
            Assert.AreEqual(0, (int)DepthFormat.None);
            Assert.AreEqual(1, (int)DepthFormat.Depth16);
            Assert.AreEqual(2, (int)DepthFormat.Depth24);
            Assert.AreEqual(3, (int)DepthFormat.Depth24Stencil8);
        }

        [Test]
        public void EffectParameterClassEnum()
        {
            Assert.AreEqual(0, (int)EffectParameterClass.Scalar);
            Assert.AreEqual(1, (int)EffectParameterClass.Vector);
            Assert.AreEqual(2, (int)EffectParameterClass.Matrix);
            Assert.AreEqual(3, (int)EffectParameterClass.Object);
            Assert.AreEqual(4, (int)EffectParameterClass.Struct);
        }

        [Test]
        public void EffectParameterTypeEnum()
        {
            Assert.AreEqual(0, (int) EffectParameterType.Void);
		    Assert.AreEqual(1, (int)EffectParameterType.Bool);
		    Assert.AreEqual(2, (int)EffectParameterType.Int32);
		    Assert.AreEqual(3, (int)EffectParameterType.Single);
		    Assert.AreEqual(4, (int)EffectParameterType.String);
		    Assert.AreEqual(5, (int)EffectParameterType.Texture);
		    Assert.AreEqual(6, (int)EffectParameterType.Texture1D);
		    Assert.AreEqual(7, (int)EffectParameterType.Texture2D);
		    Assert.AreEqual(8, (int)EffectParameterType.Texture3D);
            Assert.AreEqual(9, (int)EffectParameterType.TextureCube);
        }

        [Test]
        public void FillModeEnum()
        {
            Assert.AreEqual(0, (int)FillMode.Solid);
            Assert.AreEqual(1, (int)FillMode.WireFrame);         
        }

        [Test]
        public void GraphicsDeviceStatusEnum()
        {
            Assert.AreEqual(0, (int)GraphicsDeviceStatus.Normal);
            Assert.AreEqual(1, (int)GraphicsDeviceStatus.Lost);
            Assert.AreEqual(2, (int)GraphicsDeviceStatus.NotReset);
        }

        [Test]
        public void GraphicsProfileEnum()
        {
            Assert.AreEqual(0, (int)GraphicsProfile.Reach);
            Assert.AreEqual(1, (int)GraphicsProfile.HiDef);
        }

        [Test]
        public void IndexElementSizeEnum()
        {
            Assert.AreEqual(0, (int)IndexElementSize.SixteenBits);
            Assert.AreEqual(1, (int)IndexElementSize.ThirtyTwoBits);
        }

        [Test]
        public void PresentIntervalEnum()
        {
            Assert.AreEqual(0, (int)PresentInterval.Default);
            Assert.AreEqual(1, (int)PresentInterval.One);
            Assert.AreEqual(2, (int)PresentInterval.Two);
            Assert.AreEqual(3, (int)PresentInterval.Immediate);
        }

        [Test]
        public void PrimitiveTypeEnum()
        {
            Assert.AreEqual(0, (int)PrimitiveType.TriangleList);
            Assert.AreEqual(1, (int)PrimitiveType.TriangleStrip);
            Assert.AreEqual(2, (int)PrimitiveType.LineList);
            Assert.AreEqual(3, (int)PrimitiveType.LineStrip);
        }

        [Test]
        public void RenderTargetUsageEnum()
        {
            Assert.AreEqual(0, (int)RenderTargetUsage.DiscardContents);
            Assert.AreEqual(1, (int)RenderTargetUsage.PreserveContents);
            Assert.AreEqual(2, (int)RenderTargetUsage.PlatformContents);
        }

        [Test]
        public void SetDataOptionsEnum()
        {
            Assert.AreEqual(0, (int)SetDataOptions.None);
            Assert.AreEqual(1, (int)SetDataOptions.Discard);
            Assert.AreEqual(2, (int)SetDataOptions.NoOverwrite);
        }

        [Test]
        public void SpriteSortModeEnum()
        {
            Assert.AreEqual(0, (int)SpriteSortMode.Deferred);
            Assert.AreEqual(1, (int)SpriteSortMode.Immediate);
            Assert.AreEqual(2, (int)SpriteSortMode.Texture);
            Assert.AreEqual(3, (int)SpriteSortMode.BackToFront);
            Assert.AreEqual(4, (int)SpriteSortMode.FrontToBack);
        }

        [Test]
        public void StencilOperationEnum()
        {
            Assert.AreEqual(0, (int)StencilOperation.Keep);
            Assert.AreEqual(1, (int)StencilOperation.Zero);
            Assert.AreEqual(2, (int)StencilOperation.Replace);
            Assert.AreEqual(3, (int)StencilOperation.Increment);
            Assert.AreEqual(4, (int)StencilOperation.Decrement);
            Assert.AreEqual(5, (int)StencilOperation.IncrementSaturation);
            Assert.AreEqual(6, (int)StencilOperation.DecrementSaturation);
            Assert.AreEqual(7, (int)StencilOperation.Invert);
        }

        [Test]
        public void SurfaceFormateEnum()
        {
            // note : there is only XNA formats, extensions are not included

            Assert.AreEqual(0, (int)SurfaceFormat.Color);
            Assert.AreEqual(1, (int)SurfaceFormat.Bgr565);
            Assert.AreEqual(2, (int)SurfaceFormat.Bgra5551);
            Assert.AreEqual(3, (int)SurfaceFormat.Bgra4444);
            Assert.AreEqual(4, (int)SurfaceFormat.Dxt1);
            Assert.AreEqual(5, (int)SurfaceFormat.Dxt3);
            Assert.AreEqual(6, (int)SurfaceFormat.Dxt5);
            Assert.AreEqual(7, (int)SurfaceFormat.NormalizedByte2);
            Assert.AreEqual(8, (int)SurfaceFormat.NormalizedByte4);
            Assert.AreEqual(9, (int)SurfaceFormat.Rgba1010102);
            Assert.AreEqual(10, (int)SurfaceFormat.Rg32);
            Assert.AreEqual(11, (int)SurfaceFormat.Rgba64);
            Assert.AreEqual(12, (int)SurfaceFormat.Alpha8);
            Assert.AreEqual(13, (int)SurfaceFormat.Single);
            Assert.AreEqual(14, (int)SurfaceFormat.Vector2);
            Assert.AreEqual(15, (int)SurfaceFormat.Vector4);
            Assert.AreEqual(16, (int)SurfaceFormat.HalfSingle);
            Assert.AreEqual(17, (int)SurfaceFormat.HalfVector2);
            Assert.AreEqual(18, (int)SurfaceFormat.HalfVector4);
            Assert.AreEqual(19, (int)SurfaceFormat.HdrBlendable);
        }

        [Test]
        public void TextureAddressModeEnum()
        {
            Assert.AreEqual(0, (int)TextureAddressMode.Wrap);
            Assert.AreEqual(1, (int)TextureAddressMode.Clamp);
            Assert.AreEqual(2, (int)TextureAddressMode.Mirror);
        }

        [Test]
        public void TextureFilterEnum()
        {
            Assert.AreEqual(0, (int)TextureFilter.Linear);
            Assert.AreEqual(1, (int)TextureFilter.Point);
            Assert.AreEqual(2, (int)TextureFilter.Anisotropic);
            Assert.AreEqual(3, (int)TextureFilter.LinearMipPoint);
            Assert.AreEqual(4, (int)TextureFilter.PointMipLinear);
            Assert.AreEqual(5, (int)TextureFilter.MinLinearMagPointMipLinear);
            Assert.AreEqual(6, (int)TextureFilter.MinLinearMagPointMipPoint);
            Assert.AreEqual(7, (int)TextureFilter.MinPointMagLinearMipLinear);
            Assert.AreEqual(8, (int)TextureFilter.MinPointMagLinearMipPoint);
        }

        [Test]
        public void VertexElementFormatEnum()
        {
            Assert.AreEqual(0, (int)VertexElementFormat.Single);
            Assert.AreEqual(1, (int)VertexElementFormat.Vector2);
            Assert.AreEqual(2, (int)VertexElementFormat.Vector3);
            Assert.AreEqual(3, (int)VertexElementFormat.Vector4);
            Assert.AreEqual(4, (int)VertexElementFormat.Color);
            Assert.AreEqual(5, (int)VertexElementFormat.Byte4);
            Assert.AreEqual(6, (int)VertexElementFormat.Short2);
            Assert.AreEqual(7, (int)VertexElementFormat.Short4);
            Assert.AreEqual(8, (int)VertexElementFormat.NormalizedShort2);
            Assert.AreEqual(9, (int)VertexElementFormat.NormalizedShort4);
            Assert.AreEqual(10, (int)VertexElementFormat.HalfVector2);
            Assert.AreEqual(11, (int)VertexElementFormat.HalfVector4);
        }

        [Test]
        public void VertexElementUsageEnum()
        {
            Assert.AreEqual(0, (int)VertexElementUsage.Position);
            Assert.AreEqual(1, (int)VertexElementUsage.Color);
            Assert.AreEqual(2, (int)VertexElementUsage.TextureCoordinate);
            Assert.AreEqual(3, (int)VertexElementUsage.Normal);
            Assert.AreEqual(4, (int)VertexElementUsage.Binormal);
            Assert.AreEqual(5, (int)VertexElementUsage.Tangent);
            Assert.AreEqual(6, (int)VertexElementUsage.BlendIndices);
            Assert.AreEqual(7, (int)VertexElementUsage.BlendWeight);
            Assert.AreEqual(8, (int)VertexElementUsage.Depth);
            Assert.AreEqual(9, (int)VertexElementUsage.Fog);
            Assert.AreEqual(10, (int)VertexElementUsage.PointSize);
            Assert.AreEqual(11, (int)VertexElementUsage.Sample);
            Assert.AreEqual(12, (int)VertexElementUsage.TessellateFactor);
        }

        #endregion
    }
}
