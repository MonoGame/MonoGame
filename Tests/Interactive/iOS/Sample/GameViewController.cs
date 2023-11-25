using System;
using System.Runtime.InteropServices;
using System.Threading;

using CoreAnimation;
using CoreGraphics;
using Foundation;
using OpenTK;
using Metal;
using UIKit;

namespace Sample
{
    public partial class GameViewController : UIViewController
    {
        struct Uniforms
        {
            public Matrix4 ModelviewProjectionMatrix;
            public Matrix4 NormalMatrix;
        }

        // The max number of command buffers in flight
        const int max_inflight_buffers = 3;

        // Max API memory buffer size
        const int max_bytes_per_frame = 1024 * 1024;

        float[] cubeVertexData = {
            // Data layout for each line below is:
            // positionX, positionY, positionZ,     normalX, normalY, normalZ,
            0.5f, -0.5f, 0.5f,   0.0f, -1.0f,  0.0f,
            -0.5f, -0.5f, 0.5f,   0.0f, -1.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,   0.0f, -1.0f,  0.0f,
            0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
            0.5f, -0.5f, 0.5f,   0.0f, -1.0f,  0.0f,
            -0.5f, -0.5f, -0.5f,   0.0f, -1.0f,  0.0f,

            0.5f, 0.5f, 0.5f,    1.0f, 0.0f,  0.0f,
            0.5f, -0.5f, 0.5f,   1.0f,  0.0f,  0.0f,
            0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
            0.5f, 0.5f, -0.5f,   1.0f, 0.0f,  0.0f,
            0.5f, 0.5f, 0.5f,    1.0f, 0.0f,  0.0f,
            0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,

            -0.5f, 0.5f, 0.5f,    0.0f, 1.0f,  0.0f,
            0.5f, 0.5f, 0.5f,    0.0f, 1.0f,  0.0f,
            0.5f, 0.5f, -0.5f,   0.0f, 1.0f,  0.0f,
            -0.5f, 0.5f, -0.5f,   0.0f, 1.0f,  0.0f,
            -0.5f, 0.5f, 0.5f,    0.0f, 1.0f,  0.0f,
            0.5f, 0.5f, -0.5f,   0.0f, 1.0f,  0.0f,

            -0.5f, -0.5f, 0.5f,  -1.0f,  0.0f, 0.0f,
            -0.5f, 0.5f, 0.5f,   -1.0f, 0.0f,  0.0f,
            -0.5f, 0.5f, -0.5f,  -1.0f, 0.0f,  0.0f,
            -0.5f, -0.5f, -0.5f,  -1.0f,  0.0f,  0.0f,
            -0.5f, -0.5f, 0.5f,  -1.0f,  0.0f, 0.0f,
            -0.5f, 0.5f, -0.5f,  -1.0f, 0.0f,  0.0f,

            0.5f, 0.5f,  0.5f,  0.0f, 0.0f,  1.0f,
            -0.5f, 0.5f,  0.5f,  0.0f, 0.0f,  1.0f,
            -0.5f, -0.5f, 0.5f,   0.0f,  0.0f, 1.0f,
            -0.5f, -0.5f, 0.5f,   0.0f,  0.0f, 1.0f,
            0.5f, -0.5f, 0.5f,   0.0f,  0.0f,  1.0f,
            0.5f, 0.5f,  0.5f,  0.0f, 0.0f,  1.0f,

            0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
            -0.5f, -0.5f, -0.5f,   0.0f,  0.0f, -1.0f,
            -0.5f, 0.5f, -0.5f,  0.0f, 0.0f, -1.0f,
            0.5f, 0.5f, -0.5f,  0.0f, 0.0f, -1.0f,
            0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
            -0.5f, 0.5f, -0.5f,  0.0f, 0.0f, -1.0f
        };

        // Layer
        CAMetalLayer metalLayer;
        bool layerSizeDidUpdate;
        MTLRenderPassDescriptor renderPassDescriptor;

        // Controller
        CADisplayLink timer;
        Semaphore inflightSemaphore;
        IMTLBuffer dynamicConstantBuffer;
        byte constantDataBufferIndex;

        // Renderer
        IMTLDevice device;
        IMTLCommandQueue commandQueue;
        IMTLLibrary defaultLibrary;
        IMTLRenderPipelineState pipelineState;
        IMTLBuffer vertexBuffer;
        IMTLDepthStencilState depthState;
        IMTLTexture depthTex;

        // Uniforms
        Matrix4 projectionMatrix;
        Matrix4 viewMatrix;
        Uniforms uniformBuffer;
        float rotation;

        protected GameViewController (IntPtr handle) : base (handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            constantDataBufferIndex = 0;
            inflightSemaphore = new Semaphore (max_inflight_buffers, max_inflight_buffers);

            SetupMetal ();
            LoadAssets ();

            timer = CADisplayLink.Create (Gameloop);
            timer.FrameInterval = 1;
            timer.AddToRunLoop (NSRunLoop.Main, NSRunLoop.NSDefaultRunLoopMode);
        }

        public override void DidReceiveMemoryWarning ()
        {
            base.DidReceiveMemoryWarning ();
            // Dispose of any resources that can be recreated.
        }

        void SetupMetal ()
        {
            // Find a usable device
            device = MTLDevice.SystemDefault;

            // Create a new command queue
            commandQueue = device.CreateCommandQueue ();

            // Load all the shader files with a metal file extension in the project
            NSError error;

            defaultLibrary = device.CreateLibrary ("default.metallib", out error);

            // Setup metal layer and add as sub layer to view
            metalLayer = new CAMetalLayer ();
            metalLayer.Device = device;
            metalLayer.PixelFormat = MTLPixelFormat.BGRA8Unorm;

            // Change this to NO if the compute encoder is used as the last pass on the drawable texture
            metalLayer.FramebufferOnly = true;

            // Add metal layer to the views layer hierarchy
            metalLayer.Frame = View.Layer.Frame;
            View.Layer.AddSublayer (metalLayer);

            View.Opaque = true;
            View.BackgroundColor = null;
            View.ContentScaleFactor = UIScreen.MainScreen.Scale;
        }

        void LoadAssets ()
        {
            // Allocate one region of memory for the uniform buffer
            dynamicConstantBuffer = device.CreateBuffer (max_bytes_per_frame, 0);
            dynamicConstantBuffer.Label = "UniformBuffer";

            // Load the fragment program into the library
            IMTLFunction fragmentProgram = defaultLibrary.CreateFunction ("lighting_fragment");

            // Load the vertex program into the library
            IMTLFunction vertexProgram = defaultLibrary.CreateFunction ("lighting_vertex");

            // Setup the vertex buffers
            vertexBuffer = device.CreateBuffer<float> (cubeVertexData, (MTLResourceOptions)0);
            vertexBuffer.Label = "Vertices";

            // Create a reusable pipeline state
            var pipelineStateDescriptor = new MTLRenderPipelineDescriptor {
                Label = "MyPipeline",
                SampleCount = 1,
                VertexFunction = vertexProgram,
                FragmentFunction = fragmentProgram,
                DepthAttachmentPixelFormat = MTLPixelFormat.Depth32Float
            };

            pipelineStateDescriptor.ColorAttachments[0].PixelFormat = MTLPixelFormat.BGRA8Unorm;

            NSError error;

            pipelineState = device.CreateRenderPipelineState (pipelineStateDescriptor, out error);

            if (pipelineState == null)
                Console.WriteLine ("Failed to created pipeline state, error " + error);

            var depthStateDesc = new MTLDepthStencilDescriptor {
                DepthCompareFunction = MTLCompareFunction.Less,
                DepthWriteEnabled = true
            };
        
            depthState = device.CreateDepthStencilState (depthStateDesc);
        }

        void SetupRenderPassDescriptorForTexture (IMTLTexture texture)
        {
            if (renderPassDescriptor == null)
                renderPassDescriptor = MTLRenderPassDescriptor.CreateRenderPassDescriptor ();

            renderPassDescriptor.ColorAttachments[0].Texture = texture;
            renderPassDescriptor.ColorAttachments[0].LoadAction = MTLLoadAction.Clear;
            renderPassDescriptor.ColorAttachments[0].ClearColor = new MTLClearColor (0.65f, 0.65f, 0.65f, 1.0f);
            renderPassDescriptor.ColorAttachments[0].StoreAction = MTLStoreAction.Store;

            if (depthTex == null || (depthTex.Width != texture.Width || depthTex.Height != texture.Height)) {
                //  If we need a depth texture and don't have one, or if the depth texture we have is the wrong size
                //  Then allocate one of the proper size
                MTLTextureDescriptor desc = MTLTextureDescriptor.CreateTexture2DDescriptor (MTLPixelFormat.Depth32Float, texture.Width, texture.Height, false);
                depthTex = device.CreateTexture (desc);
                depthTex.Label = "Depth";

                renderPassDescriptor.DepthAttachment.Texture = depthTex;
                renderPassDescriptor.DepthAttachment.LoadAction = MTLLoadAction.Clear;
                renderPassDescriptor.DepthAttachment.ClearDepth = 1.0f;
                renderPassDescriptor.DepthAttachment.StoreAction = MTLStoreAction.DontCare;
            }
        }

        void Render ()
        {
            inflightSemaphore.WaitOne ();

            Update ();

            // Create a new command buffer for each renderpass to the current drawable
            IMTLCommandBuffer commandBuffer = commandQueue.CommandBuffer ();
            commandBuffer.Label = "MyCommand";

            // Obtain a drawable texture for this render pass and set up the renderpass descriptor for the command encoder to render into
            ICAMetalDrawable drawable = GetCurrentDrawable ();

            SetupRenderPassDescriptorForTexture (drawable.Texture);

            // Create a render command encoder so we can render into something
            IMTLRenderCommandEncoder renderEncoder = commandBuffer.CreateRenderCommandEncoder (renderPassDescriptor);
            renderEncoder.Label = "MyRenderEncoder";
            renderEncoder.SetDepthStencilState (depthState);

            // Set context state
            renderEncoder.PushDebugGroup ("DrawCube");
            renderEncoder.SetRenderPipelineState (pipelineState);
            renderEncoder.SetVertexBuffer (vertexBuffer, 0, 0);
            renderEncoder.SetVertexBuffer (dynamicConstantBuffer, (nuint) (Marshal.SizeOf (typeof (Uniforms)) * constantDataBufferIndex), 1);

            // Tell the render context we want to draw our primitives
            renderEncoder.DrawPrimitives (MTLPrimitiveType.Triangle, 0, 36, 1);
            renderEncoder.PopDebugGroup ();

            // We're done encoding commands
            renderEncoder.EndEncoding ();

            // Call the view's completion handler which is required by the view since it will signal its semaphore and set up the next buffer
            commandBuffer.AddCompletedHandler (buffer => {
                drawable.Dispose ();
                inflightSemaphore.Release ();
            });
                
            // Schedule a present once the framebuffer is complete
            commandBuffer.PresentDrawable (drawable);

            // Finalize rendering here & push the command buffer to the GPU
            commandBuffer.Commit ();

            // The renderview assumes it can now increment the buffer index and that the previous index won't be touched until we cycle back around to the same index
            constantDataBufferIndex = (byte) ((constantDataBufferIndex + 1) % max_inflight_buffers);
        }

        void Reshape ()
        {
            // When reshape is called, update the view and projection matricies since this means the view orientation or size changed
            var aspect = (float)(View.Bounds.Size.Width / View.Bounds.Size.Height);
            projectionMatrix = CreateMatrixFromPerspective (65.0f * ((float) Math.PI / 180.0f), aspect, 0.1f, 100.0f);

            viewMatrix = Matrix4.Identity;
        }

        void Update ()
        {
            var baseModel = Matrix4.Mult (CreateMatrixFromTranslation (0.0f, 0.0f, 5.0f), CreateMatrixFromRotation (rotation, 0.0f, 1.0f, 0.0f));
            var baseMv = Matrix4.Mult (viewMatrix, baseModel);
            var modelViewMatrix = Matrix4.Mult (baseMv, CreateMatrixFromRotation (rotation, 1.0f, 1.0f, 1.0f));

            uniformBuffer.NormalMatrix = Matrix4.Invert (Matrix4.Transpose (modelViewMatrix));
            uniformBuffer.ModelviewProjectionMatrix = Matrix4.Transpose (Matrix4.Mult (projectionMatrix, modelViewMatrix));

            // Copy uniformBuffer's content into dynamicConstantBuffer.Contents
            int rawsize = Marshal.SizeOf (typeof (Uniforms));
            var rawdata = new byte[rawsize];
            IntPtr ptr = Marshal.AllocHGlobal (rawsize);
            Marshal.StructureToPtr (uniformBuffer, ptr, false);
            Marshal.Copy (ptr, rawdata, 0, rawsize);
            Marshal.FreeHGlobal (ptr);

            Marshal.Copy (rawdata, 0, dynamicConstantBuffer.Contents + rawsize * constantDataBufferIndex, rawsize);

            rotation += 0.01f;
        }

        // The main game loop called by the CADisplayLine timer
        public void Gameloop ()
        {
            if (layerSizeDidUpdate) {
                CGSize drawableSize = View.Bounds.Size;
                drawableSize.Width *= View.ContentScaleFactor;
                drawableSize.Height *= View.ContentScaleFactor;
                metalLayer.DrawableSize = drawableSize;

                Reshape ();
                layerSizeDidUpdate = false;
            }

            Render ();
        }

        // Called whenever view changes orientation or layout is changed
        public override void ViewDidLayoutSubviews ()
        {
            base.ViewDidLayoutSubviews ();
            layerSizeDidUpdate = true;
            metalLayer.Frame = View.Layer.Frame;
        }

        public override bool ShouldAutorotate ()
        {
            return true;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
        {
            return UIInterfaceOrientationMask.AllButUpsideDown;
        }

        ICAMetalDrawable GetCurrentDrawable ()
        {
            ICAMetalDrawable currentDrawable = null;

            while (currentDrawable == null) {
                currentDrawable = metalLayer.NextDrawable ();
                if (currentDrawable == null)
                    Console.WriteLine ("CurrentDrawable is null");
            }

            return currentDrawable;
        }

        static Matrix4 CreateMatrixFromPerspective (float fovY, float aspect, float nearZ, float farZ)
        {
            float yscale = 1.0f / (float) Math.Tan (fovY * 0.5f);
            float xscale = yscale / aspect;
            float q = farZ / (farZ - nearZ);

            var m = new Matrix4 {
                Row0 = new Vector4 (xscale, 0.0f, 0.0f, 0.0f),
                Row1 = new Vector4 (0.0f, yscale, 0.0f, 0.0f),
                Row2 = new Vector4 (0.0f, 0.0f, q, q * -nearZ),
                Row3 = new Vector4 (0.0f, 0.0f, 1.0f, 0.0f)
            };

            return m;
        }

        static Matrix4 CreateMatrixFromTranslation (float x, float y, float z)
        {
            var m = Matrix4.Identity;
            m.Row0.W = x;
            m.Row1.W = y;
            m.Row2.W = z;
            m.Row3.W = 1.0f;
            return m;
        }

        static Matrix4 CreateMatrixFromRotation (float radians, float x, float y, float z)
        {
            Vector3 v = Vector3.Normalize (new Vector3 (x, y, z));
            var cos = (float)Math.Cos (radians);
            var sin = (float)Math.Sin (radians);
            float cosp = 1.0f - cos;

            var m = new Matrix4 {
                Row0 = new Vector4 (cos + cosp * v.X * v.X, cosp * v.X * v.Y - v.Z * sin, cosp * v.X * v.Z + v.Y * sin, 0.0f),
                Row1 = new Vector4 (cosp * v.X * v.Y + v.Z * sin, cos + cosp * v.Y * v.Y, cosp * v.Y * v.Z - v.X * sin, 0.0f),
                Row2 = new Vector4 (cosp * v.X * v.Z - v.Y * sin, cosp * v.Y * v.Z + v.X * sin, cos + cosp * v.Z * v.Z, 0.0f),
                Row3 = new Vector4 (0.0f, 0.0f, 0.0f, 1.0f)
            };

            return m;
        }
    }
}

