using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK;
using Koeky3D.Textures;
using GLFrameWork.Shapes;
using Koeky3D.Utilities;

namespace Koeky3D.BufferHandling
{
    public class FrameBuffer
    {
        #region variables
        private int frameBufferId;

        private int width, height;

        private Texture[] colorBuffers;
        private Texture depthTexture;

        private bool binded = false;

        private DrawBuffersEnum[] attachements;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a FrameBuffer
        /// </summary>
        /// <param name="width">The width in pixels of this FrameBuffer</param>
        /// <param name="height">The height in pixels of this FrameBuffer</param>
        /// <param name="colorBufferCount">The amount of color buffers. Color buffers use the default Texture2D format.</param>
        /// <param name="hasDepthBuffer">True if this FrameBuffer has a depth format. The format for the depth buffer is: DepthComponent32 and pixel type float</param>
        public FrameBuffer(int width, int height, int colorBufferCount, bool hasDepthBuffer)
        {
            this.width = width;
            this.height = height;

            // create color buffers
            this.colorBuffers = new Texture2D[colorBufferCount];
            for (int i = 0; i < colorBufferCount; i++)
            {
                this.colorBuffers[i] = new Texture2D();
            }

            if (hasDepthBuffer)
            {
                // create depth texture
                this.depthTexture = new Texture2D(PixelInternalFormat.DepthComponent32, PixelType.Float);
            }

            CreateBuffers();
        }
        /// <summary>
        /// Constructs a FrameBuffer
        /// </summary>
        /// <param name="width">The width in pixels of this FrameBuffer</param>
        /// <param name="height">The height in pixels of this FrameBuffer</param>
        /// <param name="colorBuffers">The color buffers to use. 1D textures are not supported.</param>
        /// <param name="depthTexture">The depth texture to use or null if no depth texture. 1D textures are not supported.</param>
        public FrameBuffer(int width, int height, Texture[] colorBuffers, Texture depthTexture)
        {
            Type colorBufferType = colorBuffers[0].GetType();
            if (colorBufferType == typeof(Texture1D))
                throw new Exception("1D textures are not supported as color buffers");
            if (depthTexture != null)
            {
                Type depthBufferType = depthTexture.GetType();
                if (depthBufferType == typeof(Texture1D))
                    throw new Exception("1D textures are not supported as depth buffer");

                if (depthBufferType != colorBufferType)
                    throw new Exception("The color buffer type and depth buffer type must be equal");
            }

            this.width = width;
            this.height = height;

            this.colorBuffers = colorBuffers;
            this.depthTexture = depthTexture;

            CreateBuffers();
        }
        #endregion

        #region FrameBuffer methods
        /// <summary>
        /// Clears this FrameBuffer. The FrameBuffer must be bound for this method to work.
        /// </summary>
        /// <param name="mask"></param>
        public void ClearBuffers(ClearBufferMask mask)
        {
            if (!binded)
                throw new Exception("Frame buffer must be binded before calling clear");
            GL.Clear(mask);
        }

        /// <summary>
        /// Binds this FrameBuffer
        /// </summary>
        public void BindBuffer()
        {
            if (binded)
                throw new Exception("Cannot bind an already bound frame buffer");

            binded = true;
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.frameBufferId);
            GL.DrawBuffers(this.colorBuffers.Length, this.attachements);
        }
        /// <summary>
        /// Unbinds this FrameBuffer
        /// </summary>
        public void UnbindBuffer()
        {
            if (!binded)
                throw new Exception("Cannot unbind a frame buffer that is not bound");

            binded = false;
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        /// <summary>
        /// Resizes this FrameBuffer. This method will destroy the textures used.
        /// </summary>
        /// <param name="width">The new width in pixels</param>
        /// <param name="height">The new height in pixels</param>
        public void Resize(int width, int height)
        {
            // resize here
            this.DestroyFramebuffer(true);
            this.width = width;
            this.height = height;
            this.CreateBuffers();
        }
        /// <summary>
        /// Unloads the resources used by this FrameBuffer. Do not bind this FrameBuffer after this call!
        /// </summary>
        /// <param name="retainTextures">
        /// False if the textures must also be destroyed.
        /// Not destroying the textures means you must do this yourself at a later time.
        /// </param>
        public void DestroyFramebuffer(bool retainTextures)
        {
            if (!retainTextures)
            {
                // Destroy the textures
                foreach (Texture texture in this.colorBuffers)
                    texture.DestroyTexture();
                if (this.depthTexture != null)
                    this.depthTexture.DestroyTexture();
            }

            GL.DeleteFramebuffers(1, ref this.frameBufferId);
        }

        /// <summary>
        /// Draws this framebuffer to the current screen using the level 0 colorbuffer, this method works best using an orthographic projection
        /// </summary>
        public void Draw(GLManager glManager)
        {
            Draw(0, glManager);
        }
        /// <summary>
        /// Draws this FrameBuffer on the screen, this method works best using an orthographic projection.
        /// </summary>
        /// <param name="level">The level, or texture, to draw</param>
        /// <param name="glManager">A GLManager used to bind buffers and call draw methods</param>
        public void Draw(int level, GLManager glManager)
        {
            Draw(level, glManager, new Vector2(), this.width, this.height);
        }
        /// <summary>
        /// Draws this FrameBuffer on the screen, this method works best using an orthographic projection.
        /// </summary>
        /// <param name="level">The level, or texture, to draw</param>
        /// <param name="glManager">The GLManager to use when drawing.</param>
        /// <param name="position">The draw position</param>
        /// <param name="width">Width of the quad to draw</param>
        /// <param name="height">Height of the quad to draw</param>
        public void Draw(int level, GLManager glManager, Vector2 position, int width, int height)
        {
            ShapeDrawer.Begin(glManager);

            glManager.BindTexture(this.colorBuffers[level], TextureUnit.Texture0);
            ShapeDrawer.DrawQuad(glManager, position, new Vector2(width, height));

            ShapeDrawer.End(glManager);
        }

        /// <summary>
        /// Defines the face of the cubemap texture to draw to.
        /// This method is only required for framebuffers using cubemaps.
        /// This method assumes the framebuffer is bound.
        /// </summary>
        /// <param name="target">The face to draw to</param>
        public void SetActiveCubemapFace(TextureTarget target)
        {
            for(int i = 0; i < this.colorBuffers.Length; i++)
            {
                Texture colorBuffer = this.colorBuffers[i];
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, target, colorBuffer.TextureId, 0);
            }
        }

        private void CreateBuffers()
        {
            int bufferType = this.colorBuffers[0].GetType() == typeof(Texture2D) ? 0 : 1;

            GL.GenFramebuffers(1, out this.frameBufferId);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.frameBufferId);

            this.attachements = new DrawBuffersEnum[this.colorBuffers.Length];
            for (int i = 0; i < this.colorBuffers.Length; i++)
            {
                Texture colorBuffer = this.colorBuffers[i];

                colorBuffer.BindTexture(TextureUnit.Texture0);
                colorBuffer.SetParameter(TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                colorBuffer.SetParameter(TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                colorBuffer.SetParameter(TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                colorBuffer.SetParameter(TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                // Reserve space for the data of the cubemap or texture2d. 
                switch (bufferType)
                {
                    case(0):
                        // The color buffer is a texture 2d
                        (colorBuffer as Texture2D).SetData(IntPtr.Zero, PixelFormat.Rgba, this.width, this.height);
                        break;
                    case(1):
                        // The color buffer is a cube map. Iterate for every face and reserve the data
                        TextureCube cubeColorBuffer = (TextureCube)colorBuffer;
                        for(int j = 0; j < 6; j++)
                        {
                            cubeColorBuffer.SetData(TextureTarget.TextureCubeMapPositiveX + j, PixelFormat.Rgba, IntPtr.Zero, this.width, this.height);
                        }
                        break;
                }

                this.attachements[i] = DrawBuffersEnum.ColorAttachment0 + i;
                if (bufferType == 0)
                {
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, colorBuffer.TextureTarget, colorBuffer.TextureId, 0);
                }
                else
                {
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, TextureTarget.TextureCubeMapPositiveX, colorBuffer.TextureId, 0);
                }
            }

            if (this.depthTexture != null)
            {
                this.depthTexture.BindTexture(TextureUnit.Texture0);
                this.depthTexture.SetParameter(TextureParameterName.TextureMinFilter, (int)All.Nearest);
                this.depthTexture.SetParameter(TextureParameterName.TextureMagFilter, (int)All.Nearest);
                this.depthTexture.SetParameter(TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                this.depthTexture.SetParameter(TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                switch (bufferType)
                {
                    case (0):
                        // The color buffer is a texture 2d
                        (this.depthTexture as Texture2D).SetData(IntPtr.Zero, PixelFormat.DepthComponent, this.width, this.height);
                        break;
                    case (1):
                        // The color buffer is a cube map. Iterate for every face and set the data
                        TextureCube cubeDepthBuffer = (TextureCube)this.depthTexture;
                        for (int j = 0; j < 6; j++)
                        {
                            cubeDepthBuffer.SetData(TextureTarget.TextureCubeMapPositiveX + j, PixelFormat.DepthComponent, IntPtr.Zero, this.width, this.height);
                        }
                        break;
                }

                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, this.depthTexture.TextureTarget, this.depthTexture.TextureId, 0);
            }

            FramebufferErrorCode error = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (error != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Failed to create framebuffer: " + error.ToString());

            // unbind all bound frame buffers and textures
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// Retrieves the primary Texture2D object (level 0)
        /// </summary>
        public Texture PrimaryTexture
        {
            get
            {
                return this.colorBuffers[0];
            }
        }

        /// <summary>
        /// Gets the array with color buffers for this frame buffer
        /// </summary>
        public Texture[] ColorBuffers
        {
            get
            {
                return this.colorBuffers;
            }
        }

        /// <summary>
        /// The depth buffer or null if no depth buffer
        /// </summary>
        public Texture DepthTexture
        {
            get
            {
                return this.depthTexture;
            }
        }

        public int Width
        {
            get
            {
                return this.width;
            }
        }
        public int Height
        {
            get
            {
                return this.height;
            }
        }
        #endregion
    }
}
