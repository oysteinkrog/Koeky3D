using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using Koeky3D.BufferHandling;
using Koeky3D.Textures;
using Koeky3D.Models;
using Koeky3D.Shaders;
using OpenTK.Graphics;
using Koeky3D.Utilities;

namespace Koeky3D
{
    /// <summary>
    /// Provides basic functionality for communicating with OpenGL
    /// </summary>
    public class GLManager
    {
        #region constants
        public const String SHADER_WORLD = "world";
        public const String SHADER_VIEW = "view";
        public const String SHADER_PROJECTION = "projection";

        public const String SHADER_DIFFUSECOLOR = "diffuseColor";
        public const String SHADER_SPECULARCOLOR = "specularColor";

        public const String SHADER_NORMALMAP = "normalMap";
        public const String SHADER_SPECULARMAP = "specularMap";
        public const String SHADER_DIFFUSEMAP = "diffuseMap";

        public const String SHADER_HASNORMALMAP = "hasNormalMap";
        public const String SHADER_HASSPECULARMAP = "hasSpecularMap";
        public const String SHADER_HASDIFFUSEMAP = "hasDiffuseMap";

        public const String SHADER_HASSKELETON = "hasSkeleton";
        public const String SHADER_INVJOINTS = "invJoints";
        public const String SHADER_JOINTS = "joints";

        public const String SHADER_CAMPOS = "camPos";
        #endregion

        #region variables
        private Stack<RenderState> oldStates = new Stack<RenderState>();
        private RenderState state = new RenderState();

        private Matrix4 view, projection, world;
        private Vector3 camPos;

        private GLShader shader;
        private Technique technique;
        private VertexArray vertexArray;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        private int frameBufferIndex = -1;
        private FrameBuffer[] frameBuffers = new FrameBuffer[32];

        private RenderOptions renderOptions;
        #endregion

        #region constructors
        public GLManager(RenderOptions renderOptions)
        {
            this.renderOptions = renderOptions;
            this.InitialiseState();
        }
        public GLManager(RenderState state, RenderOptions renderOptions)
        {
            this.state = state;
            this.renderOptions = renderOptions;
            this.InitialiseState();
        }
        #endregion

        #region GLManager methods
        /// <summary>
        /// Clears the screen using the specified ClearBufferMask.
        /// If a FrameBuffer is bound, the FrameBuffer will be cleared.
        /// </summary>
        /// <param name="clear"></param>
        public void ClearScreen(ClearBufferMask clear)
        {
            if (this.frameBufferIndex == -1)
                GL.Clear(clear);
            else
            {
                this.frameBuffers[this.frameBufferIndex].ClearBuffers(clear);
            }
        }

        /// <summary>
        /// Binds the specified FrameBuffer and saves the previous.
        /// </summary>
        /// <param name="frameBuffer"></param>
        public void PushFrameBuffer(FrameBuffer frameBuffer)
        {
            // If another frame buffer is bound, unbind it
            if (this.frameBufferIndex >= 0)
                this.frameBuffers[this.frameBufferIndex].UnbindBuffer();

            GL.Viewport(0, 0, frameBuffer.Width, frameBuffer.Height);

            // bind the new buffer and add it to the bound array
            frameBuffer.BindBuffer();
            this.frameBuffers[++this.frameBufferIndex] = frameBuffer;
        }
        /// <summary>
        /// Unbinds the current FrameBuffer and binds the previous FrameBuffer
        /// </summary>
        public void PopFrameBuffer()
        {
            // Retrieve current frame buffer and unbind it
            FrameBuffer buffer = this.frameBuffers[this.frameBufferIndex--];
            buffer.UnbindBuffer();

            if (this.frameBufferIndex >= 0)
            {
                // Another frame buffer stil exists. Bind it
                FrameBuffer newFrameBuffer = this.frameBuffers[this.frameBufferIndex];
                newFrameBuffer.BindBuffer();
                GL.Viewport(0, 0, newFrameBuffer.Width, newFrameBuffer.Height);
            }
            else
            {
                // revert to the default OpenGL frame buffer
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                this.renderOptions.SetViewPort();
            }
        }

        /// <summary>
        /// Saves the current render state.
        /// </summary>
        public void PushRenderState()
        {
            this.oldStates.Push(new RenderState(this.state));
        }
        /// <summary>
        /// Retrieves the previous render state.
        /// </summary>
        public void PopRenderState()
        {
            this.state = this.oldStates.Pop();
            this.InitialiseState();
        }

        private void InitialiseState()
        {
            if (this.state.depthEnabled)
                GL.Enable(EnableCap.DepthTest);
            else
                GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(this.state.writeDepth);
            GL.DepthFunc(this.state.depthFunction);

            if (this.state.cullingEnabled)
                GL.Enable(EnableCap.CullFace);
            else
                GL.Disable(EnableCap.CullFace);
            GL.CullFace(this.state.cullingMode);

            if (this.state.blendingEnabled)
                GL.Enable(EnableCap.Blend);
            else
                GL.Disable(EnableCap.Blend);
            GL.BlendFunc(this.state.blendSrc, this.state.blendDest);

            GL.PolygonMode(MaterialFace.FrontAndBack, this.state.polygonMode);

            GL.BlendColor(this.state.blendColor);
            GL.ClearColor(this.state.clearColor);

            if (this.state.discardRasterizer)
                GL.Enable(EnableCap.RasterizerDiscard);
            else
                GL.Disable(EnableCap.RasterizerDiscard);
        }

        /// <summary>
        /// Binds the specified shader. Also updates the view, model and projection matrices.
        /// </summary>
        /// <param name="shader"></param>
        private void BindShader(GLShader shader)
        {
            if (this.shader == shader)
                return;

            if (shader == null && this.shader != null)
                this.shader.UnbindShader();
            this.shader = shader;

            if (this.shader != null)
            {
                this.shader.BindShader();

                this.shader.SetVariable(SHADER_WORLD, ref this.world);
                this.shader.SetVariable(SHADER_VIEW, ref this.view);
                this.shader.SetVariable(SHADER_PROJECTION, ref this.projection);
            }
        }
        /// <summary>
        /// Binds the specified render technique.
        /// </summary>
        /// <param name="technique"></param>
        public void BindTechnique(Technique technique)
        {
            if (this.technique == technique)
                return;

            if (technique == null && this.technique != null)
                this.technique.Unbind();

            this.technique = technique;
            if (this.technique == null)
                return;

            if (this.technique.shader != this.shader)
            {
                this.technique.Bind();
                this.shader = this.technique.shader;

                this.technique.SetView(ref this.view);
                this.technique.SetWorld(ref this.world);
                this.technique.SetProjection(ref this.projection);
                this.technique.SetCamPos(ref this.camPos);
            }

            this.technique.Enable();
        }
        /// <summary>
        /// Binds the specified vertex array.
        /// </summary>
        /// <param name="vertexArray"></param>
        public void BindVertexArray(VertexArray vertexArray)
        {
            if (this.vertexArray == vertexArray)
                return;
            if (this.vertexBuffer != null)
            {
                this.vertexBuffer.UnbindBuffer();
                this.vertexBuffer.UnbindAttributes();
                this.vertexBuffer = null;
            }

            if (this.vertexArray != null)
                this.vertexArray.Unbind();

            this.vertexArray = vertexArray;
            if(this.vertexArray != null)
                this.vertexArray.Bind();
        }
        /// <summary>
        /// Binds the specified vertex buffer.
        /// </summary>
        /// <param name="vertexBuffer"></param>
        public void BindVertexBuffer(VertexBuffer vertexBuffer)
        {
            if (this.vertexBuffer == vertexBuffer)
                return;
            if (this.vertexArray != null)
            {
                this.vertexArray.Unbind();
                this.vertexArray = null;
            }

            if (this.vertexBuffer != null)
            {
                this.vertexBuffer.UnbindBuffer();
                this.vertexBuffer.UnbindAttributes();
            }

            this.vertexBuffer = vertexBuffer;

            if (this.vertexBuffer != null)
            {
                this.vertexBuffer.BindBuffer();
                this.vertexBuffer.BindAttributes();
            }
        }
        /// <summary>
        /// Binds the given index buffer. This index buffer will be used when calling DrawElementsIndexed.
        /// </summary>
        /// <param name="indexBuffer"></param>
        public void BindIndexBuffer(IndexBuffer indexBuffer)
        {
            if (this.indexBuffer == indexBuffer)
                return;

            if (this.indexBuffer != null)
                this.indexBuffer.UnbindBuffer();

            this.indexBuffer = indexBuffer;

            if (this.indexBuffer != null)
                this.indexBuffer.BindBuffer();
        }
        /// <summary>
        /// Binds the specified texture to the specified texture unit
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="textureUnit"></param>
        public void BindTexture(Texture texture, TextureUnit textureUnit)
        {
            texture.BindTexture(textureUnit);
        }

        /// <summary>
        /// Draws a set of elements from the currently bound vertex array or vertex buffer.
        /// </summary>
        /// <param name="primitive">The primitive to draw</param>
        /// <param name="firstVertex">The index of the first vertex</param>
        /// <param name="vertexCount">The amount of vertices to draw</param>
        public void DrawElements(BeginMode primitive, int firstVertex, int vertexCount)
        {
            if (this.vertexArray == null && this.vertexBuffer == null)
                throw new Exception("No element buffer active");
            if (this.shader == null)
                throw new Exception("No shader active");

            GL.DrawArrays(primitive, firstVertex, vertexCount);
        }
        /// <summary>
        /// Draws the current vertex array or vertex buffer using the current index buffer.
        /// </summary>
        /// <param name="primitive">The primitive to draw</param>
        /// <param name="indexCount">The amount of indices in the index buffer to process</param>
        /// <param name="indexOffset">The first index to process</param>
        public void DrawElementsIndexed(BeginMode primitive, int indexCount, int indexOffset)
        {
            if (this.vertexArray == null && this.vertexBuffer == null)
                throw new Exception("No element buffer active");
            if (this.indexBuffer == null && !this.vertexArray.HasIndexBuffer)
                throw new Exception("No index buffer active");
            if (this.shader == null)
                throw new Exception("No shader active");

            GL.DrawElements(primitive, indexCount, this.indexBuffer == null ? this.vertexArray.IndexBuffer.ElementType : this.indexBuffer.ElementType, indexOffset);
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// Gets of sets the current view transform.
        /// </summary>
        public Matrix4 View
        {
            get
            {
                return this.view;
            }
            set
            {
                this.view = value;
                if (this.technique != null)
                    this.technique.SetView(ref value);
            }
        }
        /// <summary>
        /// Gets or sets the current world transform.
        /// </summary>
        public Matrix4 World
        {
            get
            {
                return this.world;
            }
            set
            {
                this.world = value;
                if (this.technique != null)
                    this.technique.SetWorld(ref value);
            }
        }
        /// <summary>
        /// Gets or sets the current projection transform.
        /// </summary>
        public Matrix4 Projection
        {
            get
            {
                return this.projection;
            }
            set
            {
                this.projection = value;
                if (this.technique != null)
                    this.technique.SetProjection(ref value);
            }
        }
        /// <summary>
        /// Gets or sets the position of the camera in world space.
        /// </summary>
        public Vector3 CamPos
        {
            get
            {
                return this.camPos;
            }
            set
            {
                this.camPos = value;
                if (this.technique != null)
                    this.technique.SetCamPos(ref value);
            }
        }

        /// <summary>
        /// Gets the maximum amount of textures active at any given time.
        /// </summary>
        public int MaxTextures
        {
            get
            {
                int maxTextures;
                GL.GetInteger(GetPName.MaxTextureImageUnits, out maxTextures);
                return maxTextures;
            }
        }
        /// <summary>
        /// Gets the maximum amount of vertex attributes.
        /// </summary>
        public int MaxVertexAttributes
        {
            get
            {
                int maxAttributes;
                GL.GetInteger(GetPName.MaxVertexAttribs, out maxAttributes);
                return maxAttributes;
            }
        }
        /// <summary>
        /// Gets the maximum amount of vertex shader uniforms.
        /// </summary>
        public int MaxVertexShaderUniforms
        {
            get
            {
                int result;
                GL.GetInteger(GetPName.MaxVertexUniformComponents, out result);

                return result;
            }
        }
        /// <summary>
        /// Gets the maximum amount of fragment shader uniforms.
        /// </summary>
        public int MaxFragmentShaderUniforms
        {
            get
            {
                int result;
                GL.GetInteger(GetPName.MaxFragmentUniformComponents, out result);

                return result;
            }
        }
        /// <summary>
        /// Gets the maximum amount of transform feedback buffers when using vertex arrays.
        /// </summary>
        public int MaxTransformFeedbackSeperateAttribs
        {
            get
            {
                int value;
                GL.GetInteger(GetPName.MaxTransformFeedbackSeparateAttribs, out value);
                return value;
            }
        }
        /// <summary>
        /// Gets the major version of the current installed opengl version.
        /// </summary>
        public int OpenGLMajorVersion
        {
            get
            {
                int result;
                GL.GetInteger(GetPName.MajorVersion, out result);
                return result;
            }
        }
        /// <summary>
        /// Gets the minor version of the current installed opengl version.
        /// </summary>
        public int OpenGLMinorVersion
        {
            get
            {
                int result;
                GL.GetInteger(GetPName.MinorVersion, out result);
                return result;
            }
        }

        public GLShader Shader
        {
            get
            {
                return this.shader;
            }
        }
        public VertexArray VertexArrayObject
        {
            get
            {
                return this.vertexArray;
            }
        }

        /// <summary>
        /// True if depth testing is enabled
        /// </summary>
        public bool DepthTestEnabled
        {
            get
            {
                return this.state.depthEnabled;
            }
            set
            {
                if (this.state.depthEnabled == value)
                    return;

                this.state.depthEnabled = value;
                if (value)
                    GL.Enable(EnableCap.DepthTest);
                else
                    GL.Disable(EnableCap.DepthTest);
            }
        }
        /// <summary>
        /// True if depth values are written
        /// </summary>
        public bool WriteDepth
        {
            get
            {
                return this.state.writeDepth;
            }
            set
            {
                this.state.writeDepth = value;
                GL.DepthMask(value);
            }
        }
        /// <summary>
        /// The function to use while depth testing
        /// </summary>
        public DepthFunction DepthTestFunction
        {
            get
            {
                return this.state.depthFunction;
            }
            set
            {
                if (this.state.depthFunction == value)
                    return;

                this.state.depthFunction = value;
                GL.DepthFunc(value);
            }
        }

        /// <summary>
        /// True if cull face is enabled
        /// </summary>
        public bool CullFaceEnabled
        {
            get
            {
                return this.state.cullingEnabled;
            }
            set
            {
                if (this.state.cullingEnabled == value)
                    return;

                this.state.cullingEnabled = value;
                if (value)
                    GL.Enable(EnableCap.CullFace);
                else
                    GL.Disable(EnableCap.CullFace);
            }
        }
        /// <summary>
        /// The cull face mode to use
        /// </summary>
        public CullFaceMode CullFaceMode
        {
            get
            {
                return this.state.cullingMode;
            }
            set
            {
                if (this.state.cullingMode == value)
                    return;

                this.state.cullingMode = value;
                GL.CullFace(value);
            }
        }

        /// <summary>
        /// True if blending is enabled
        /// </summary>
        public bool BlendingEnabled
        {
            get
            {
                return this.state.blendingEnabled;
            }
            set
            {
                if (this.state.blendingEnabled == value)
                    return;

                this.state.blendingEnabled = value;
                if (value)
                    GL.Enable(EnableCap.Blend);
                else
                    GL.Disable(EnableCap.Blend);
            }
        }
        /// <summary>
        /// The blending operation at the source
        /// </summary>
        public BlendingFactorSrc BlendingSource
        {
            get
            {
                return this.state.blendSrc;
            }
            set
            {
                if (this.state.blendSrc == value)
                    return;

                this.state.blendSrc = value;
                GL.BlendFunc(value, this.state.blendDest);
            }
        }
        /// <summary>
        /// The blending operation at the destination
        /// </summary>
        public BlendingFactorDest BlendingDestination
        {
            get
            {
                return this.state.blendDest;
            }
            set
            {
                if (this.state.blendDest == value)
                    return;

                this.state.blendDest = value;
                GL.BlendFunc(this.state.blendSrc, value);
            }
        }
        /// <summary>
        /// Gets or sets the value with which to blend.
        /// </summary>
        public Color4 BlendingColor
        {
            get
            {
                return this.state.blendColor;
            }
            set
            {
                this.state.blendColor = value;
                GL.BlendColor(value);
            }
        }

        /// <summary>
        /// The polygon mode to use
        /// </summary>
        public PolygonMode PolygonMode
        {
            get
            {
                return this.state.polygonMode;
            }
            set
            {
                if (this.state.polygonMode == value)
                    return;

                this.state.polygonMode = value;
                GL.PolygonMode(MaterialFace.FrontAndBack, value);
            }
        }

        /// <summary>
        /// Gets or sets the color with which to clear the openGL backbuffer.
        /// </summary>
        public Color4 ClearColor
        {
            get
            {
                return this.state.clearColor;
            }
            set
            {
                this.state.clearColor = value;
                GL.ClearColor(value);
            }
        }

        /// <summary>
        /// True if rasterizing is disabled
        /// </summary>
        public bool DiscardRasterizer
        {
            get
            {
                return this.state.discardRasterizer;
            }
            set
            {
                this.state.discardRasterizer = value;
                if (value)
                    GL.Enable(EnableCap.RasterizerDiscard);
                else
                    GL.Disable(EnableCap.RasterizerDiscard);
            }
        }
        #endregion
    }
}
