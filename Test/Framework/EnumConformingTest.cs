using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    /// <summary>
    /// Tests for enum compatibility with XNA(here is only XNA enum members, extensions are not included).
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

        #region MonoGame.Framework.Input

        [Test]
        public void ButtonsEnum()
        {
            Assert.AreEqual(1, (int)Buttons.DPadUp);
            Assert.AreEqual(2, (int)Buttons.DPadDown);
            Assert.AreEqual(4, (int)Buttons.DPadLeft);
            Assert.AreEqual(8, (int)Buttons.DPadRight);
            Assert.AreEqual(16, (int)Buttons.Start);
            Assert.AreEqual(32, (int)Buttons.Back);
            Assert.AreEqual(64, (int)Buttons.LeftStick);
            Assert.AreEqual(128, (int)Buttons.RightStick);
            Assert.AreEqual(256, (int)Buttons.LeftShoulder);
            Assert.AreEqual(512, (int)Buttons.RightShoulder);
            Assert.AreEqual(2048, (int)Buttons.BigButton);
            Assert.AreEqual(4096, (int)Buttons.A);
            Assert.AreEqual(8192, (int)Buttons.B);
            Assert.AreEqual(16384, (int)Buttons.X);
            Assert.AreEqual(32768, (int)Buttons.Y);
            Assert.AreEqual(2097152, (int)Buttons.LeftThumbstickLeft);
            Assert.AreEqual(4194304, (int)Buttons.RightTrigger);
            Assert.AreEqual(8388608, (int)Buttons.LeftTrigger);
            Assert.AreEqual(16777216, (int)Buttons.RightThumbstickUp);
            Assert.AreEqual(33554432, (int)Buttons.RightThumbstickDown);
            Assert.AreEqual(67108864, (int)Buttons.RightThumbstickRight);
            Assert.AreEqual(134217728, (int)Buttons.RightThumbstickLeft);
            Assert.AreEqual(268435456, (int)Buttons.LeftThumbstickUp);
            Assert.AreEqual(536870912, (int)Buttons.LeftThumbstickDown);
            Assert.AreEqual(1073741824, (int)Buttons.LeftThumbstickRight);
        }

        [Test]
        public void ButtonStateEnum()
        {
            Assert.AreEqual(0, (int)ButtonState.Released);
            Assert.AreEqual(1, (int)ButtonState.Pressed);
        }

        [Test]
        public void GamePadTypeEnum()
        {
            Assert.AreEqual(0, (int)GamePadType.Unknown);
            Assert.AreEqual(1, (int)GamePadType.GamePad);
            Assert.AreEqual(2, (int)GamePadType.Wheel);
            Assert.AreEqual(3, (int)GamePadType.ArcadeStick);
            Assert.AreEqual(4, (int)GamePadType.FlightStick);
            Assert.AreEqual(5, (int)GamePadType.DancePad);
            Assert.AreEqual(6, (int)GamePadType.Guitar);
            Assert.AreEqual(7, (int)GamePadType.AlternateGuitar);
            Assert.AreEqual(8, (int)GamePadType.DrumKit);
            Assert.AreEqual(768, (int)GamePadType.BigButtonPad);
        }

        [Test]
        public void KeysEnum()
        {
            Assert.AreEqual(0, (int)Keys.None);
            Assert.AreEqual(8, (int)Keys.Back);
            Assert.AreEqual(9, (int)Keys.Tab);
            Assert.AreEqual(13, (int)Keys.Enter);
            Assert.AreEqual(19, (int)Keys.Pause);
            Assert.AreEqual(20, (int)Keys.CapsLock);
            Assert.AreEqual(21, (int)Keys.Kana);
            Assert.AreEqual(25, (int)Keys.Kanji);
            Assert.AreEqual(27, (int)Keys.Escape);
            Assert.AreEqual(28, (int)Keys.ImeConvert);
            Assert.AreEqual(29, (int)Keys.ImeNoConvert);
            Assert.AreEqual(32, (int)Keys.Space);
            Assert.AreEqual(33, (int)Keys.PageUp);
            Assert.AreEqual(34, (int)Keys.PageDown);
            Assert.AreEqual(35, (int)Keys.End);
            Assert.AreEqual(36, (int)Keys.Home);
            Assert.AreEqual(37, (int)Keys.Left);
            Assert.AreEqual(38, (int)Keys.Up);
            Assert.AreEqual(39, (int)Keys.Right);
            Assert.AreEqual(40, (int)Keys.Down);
            Assert.AreEqual(41, (int)Keys.Select);
            Assert.AreEqual(42, (int)Keys.Print);
            Assert.AreEqual(43, (int)Keys.Execute);
            Assert.AreEqual(44, (int)Keys.PrintScreen);
            Assert.AreEqual(45, (int)Keys.Insert);
            Assert.AreEqual(46, (int)Keys.Delete);
            Assert.AreEqual(47, (int)Keys.Help);
            Assert.AreEqual(48, (int)Keys.D0);
            Assert.AreEqual(49, (int)Keys.D1);
            Assert.AreEqual(50, (int)Keys.D2);
            Assert.AreEqual(51, (int)Keys.D3);
            Assert.AreEqual(52, (int)Keys.D4);
            Assert.AreEqual(53, (int)Keys.D5);
            Assert.AreEqual(54, (int)Keys.D6);
            Assert.AreEqual(55, (int)Keys.D7);
            Assert.AreEqual(56, (int)Keys.D8);
            Assert.AreEqual(57, (int)Keys.D9);
            Assert.AreEqual(65, (int)Keys.A);
            Assert.AreEqual(66, (int)Keys.B);
            Assert.AreEqual(67, (int)Keys.C);
            Assert.AreEqual(68, (int)Keys.D);
            Assert.AreEqual(69, (int)Keys.E);
            Assert.AreEqual(70, (int)Keys.F);
            Assert.AreEqual(71, (int)Keys.G);
            Assert.AreEqual(72, (int)Keys.H);
            Assert.AreEqual(73, (int)Keys.I);
            Assert.AreEqual(74, (int)Keys.J);
            Assert.AreEqual(75, (int)Keys.K);
            Assert.AreEqual(76, (int)Keys.L);
            Assert.AreEqual(77, (int)Keys.M);
            Assert.AreEqual(78, (int)Keys.N);
            Assert.AreEqual(79, (int)Keys.O);
            Assert.AreEqual(80, (int)Keys.P);
            Assert.AreEqual(81, (int)Keys.Q);
            Assert.AreEqual(82, (int)Keys.R);
            Assert.AreEqual(83, (int)Keys.S);
            Assert.AreEqual(84, (int)Keys.T);
            Assert.AreEqual(85, (int)Keys.U);
            Assert.AreEqual(86, (int)Keys.V);
            Assert.AreEqual(87, (int)Keys.W);
            Assert.AreEqual(88, (int)Keys.X);
            Assert.AreEqual(89, (int)Keys.Y);
            Assert.AreEqual(90, (int)Keys.Z);
            Assert.AreEqual(91, (int)Keys.LeftWindows);
            Assert.AreEqual(92, (int)Keys.RightWindows);
            Assert.AreEqual(93, (int)Keys.Apps);
            Assert.AreEqual(95, (int)Keys.Sleep);
            Assert.AreEqual(96, (int)Keys.NumPad0);
            Assert.AreEqual(97, (int)Keys.NumPad1);
            Assert.AreEqual(98, (int)Keys.NumPad2);
            Assert.AreEqual(99, (int)Keys.NumPad3);
            Assert.AreEqual(100, (int)Keys.NumPad4);
            Assert.AreEqual(101, (int)Keys.NumPad5);
            Assert.AreEqual(102, (int)Keys.NumPad6);
            Assert.AreEqual(103, (int)Keys.NumPad7);
            Assert.AreEqual(104, (int)Keys.NumPad8);
            Assert.AreEqual(105, (int)Keys.NumPad9);
            Assert.AreEqual(106, (int)Keys.Multiply);
            Assert.AreEqual(107, (int)Keys.Add);
            Assert.AreEqual(108, (int)Keys.Separator);
            Assert.AreEqual(109, (int)Keys.Subtract);
            Assert.AreEqual(110, (int)Keys.Decimal);
            Assert.AreEqual(111, (int)Keys.Divide);
            Assert.AreEqual(112, (int)Keys.F1);
            Assert.AreEqual(113, (int)Keys.F2);
            Assert.AreEqual(114, (int)Keys.F3);
            Assert.AreEqual(115, (int)Keys.F4);
            Assert.AreEqual(116, (int)Keys.F5);
            Assert.AreEqual(117, (int)Keys.F6);
            Assert.AreEqual(118, (int)Keys.F7);
            Assert.AreEqual(119, (int)Keys.F8);
            Assert.AreEqual(120, (int)Keys.F9);
            Assert.AreEqual(121, (int)Keys.F10);
            Assert.AreEqual(122, (int)Keys.F11);
            Assert.AreEqual(123, (int)Keys.F12);
            Assert.AreEqual(124, (int)Keys.F13);
            Assert.AreEqual(125, (int)Keys.F14);
            Assert.AreEqual(126, (int)Keys.F15);
            Assert.AreEqual(127, (int)Keys.F16);
            Assert.AreEqual(128, (int)Keys.F17);
            Assert.AreEqual(129, (int)Keys.F18);
            Assert.AreEqual(130, (int)Keys.F19);
            Assert.AreEqual(131, (int)Keys.F20);
            Assert.AreEqual(132, (int)Keys.F21);
            Assert.AreEqual(133, (int)Keys.F22);
            Assert.AreEqual(134, (int)Keys.F23);
            Assert.AreEqual(135, (int)Keys.F24);
            Assert.AreEqual(144, (int)Keys.NumLock);
            Assert.AreEqual(145, (int)Keys.Scroll);
            Assert.AreEqual(160, (int)Keys.LeftShift);
            Assert.AreEqual(161, (int)Keys.RightShift);
            Assert.AreEqual(162, (int)Keys.LeftControl);
            Assert.AreEqual(163, (int)Keys.RightControl);
            Assert.AreEqual(164, (int)Keys.LeftAlt);
            Assert.AreEqual(165, (int)Keys.RightAlt);
            Assert.AreEqual(166, (int)Keys.BrowserBack);
            Assert.AreEqual(167, (int)Keys.BrowserForward);
            Assert.AreEqual(168, (int)Keys.BrowserRefresh);
            Assert.AreEqual(169, (int)Keys.BrowserStop);
            Assert.AreEqual(170, (int)Keys.BrowserSearch);
            Assert.AreEqual(171, (int)Keys.BrowserFavorites);
            Assert.AreEqual(172, (int)Keys.BrowserHome);
            Assert.AreEqual(173, (int)Keys.VolumeMute);
            Assert.AreEqual(174, (int)Keys.VolumeDown);
            Assert.AreEqual(175, (int)Keys.VolumeUp);
            Assert.AreEqual(176, (int)Keys.MediaNextTrack);
            Assert.AreEqual(177, (int)Keys.MediaPreviousTrack);
            Assert.AreEqual(178, (int)Keys.MediaStop);
            Assert.AreEqual(179, (int)Keys.MediaPlayPause);
            Assert.AreEqual(180, (int)Keys.LaunchMail);
            Assert.AreEqual(181, (int)Keys.SelectMedia);
            Assert.AreEqual(182, (int)Keys.LaunchApplication1);
            Assert.AreEqual(183, (int)Keys.LaunchApplication2);
            Assert.AreEqual(186, (int)Keys.OemSemicolon);
            Assert.AreEqual(187, (int)Keys.OemPlus);
            Assert.AreEqual(188, (int)Keys.OemComma);
            Assert.AreEqual(189, (int)Keys.OemMinus);
            Assert.AreEqual(190, (int)Keys.OemPeriod);
            Assert.AreEqual(191, (int)Keys.OemQuestion);
            Assert.AreEqual(192, (int)Keys.OemTilde);
            Assert.AreEqual(202, (int)Keys.ChatPadGreen);
            Assert.AreEqual(203, (int)Keys.ChatPadOrange);
            Assert.AreEqual(219, (int)Keys.OemOpenBrackets);
            Assert.AreEqual(220, (int)Keys.OemPipe);
            Assert.AreEqual(221, (int)Keys.OemCloseBrackets);
            Assert.AreEqual(222, (int)Keys.OemQuotes);
            Assert.AreEqual(223, (int)Keys.Oem8);
            Assert.AreEqual(226, (int)Keys.OemBackslash);
            Assert.AreEqual(229, (int)Keys.ProcessKey);
            Assert.AreEqual(242, (int)Keys.OemCopy);
            Assert.AreEqual(243, (int)Keys.OemAuto);
            Assert.AreEqual(244, (int)Keys.OemEnlW);
            Assert.AreEqual(246, (int)Keys.Attn);
            Assert.AreEqual(247, (int)Keys.Crsel);
            Assert.AreEqual(248, (int)Keys.Exsel);
            Assert.AreEqual(249, (int)Keys.EraseEof);
            Assert.AreEqual(250, (int)Keys.Play);
            Assert.AreEqual(251, (int)Keys.Zoom);
            Assert.AreEqual(253, (int)Keys.Pa1);
            Assert.AreEqual(254, (int)Keys.OemClear);
        }

        #endregion
    }
}
