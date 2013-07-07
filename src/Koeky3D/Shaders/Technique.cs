using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.IO;
using Koeky3D.BufferHandling;
using OpenTK;
using Koeky3D.Utilities;

namespace Koeky3D.Shaders
{
    /// <summary>
    /// Provides generalized acces to creating, linking and validating shaders.
    /// Also provides an easy way to set shader attributes.
    /// </summary>
    public abstract class Technique
    {
        #region variables
        /// <summary>
        /// The shader used by this technique
        /// </summary>
        public GLShader shader;

        private String errorMessage = "";

        private int projectionLocation, worldLocation, viewLocation, camPosLocation;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs an empty Technique.
        /// </summary>
        public Technique()
        {
        }
        /// <summary>
        /// Constructs a technique using the given shader.
        /// Calling initialise is not needed.
        /// </summary>
        /// <param name="shader"></param>
        public Technique(GLShader shader)
        {
            this.shader = shader;
        }
        #endregion

        #region Technique methods
        /// <summary>
        /// Binds this technique. This method will activate the used shader.
        /// Do not call this method until after Initialise has been called.
        /// </summary>
        public void Bind()
        {
            this.shader.BindShader();
        }
        /// <summary>
        /// Unbinds this technique.
        /// </summary>
        public void Unbind()
        {
            this.shader.UnbindShader();
        }
        /// <summary>
        /// Initialises this technique
        /// </summary>
        /// <returns>True if the technique succesfully initialised</returns>
        public virtual bool Initialise()
        {
            if (!this.Validate())
                return false;

            return true;
        }
        /// <summary>
        /// Finalizes the creation of this technique. Link errors (if any) are detected at this stage.
        /// This method also retrieves the uniform location of the variables "projection", "world", "view" and "camPos".
        /// For further error checking you can call Validate after this method.
        /// </summary>
        /// <returns>True if the technique linked succesfully</returns>
        protected bool Finalize()
        {
            if (!this.shader.LinkShader())
            {
                this.errorMessage = this.shader.ErrorMessage;
                return false;
            }

            this.projectionLocation = GetUniformLocation(GLManager.SHADER_PROJECTION);
            this.worldLocation = GetUniformLocation(GLManager.SHADER_WORLD);
            this.viewLocation = GetUniformLocation(GLManager.SHADER_VIEW);
            this.camPosLocation = GetUniformLocation(GLManager.SHADER_CAMPOS);

            return true;
        }
        /// <summary>
        /// Validates this technique 
        /// </summary>
        /// <returns>True if this technique validated succesfully</returns>
        protected bool Validate()
        {
            if (!this.shader.Validate())
            {
                this.errorMessage = this.shader.ErrorMessage;
                return false;
            }

            return true;
        }
        /// <summary>
        /// Enables this technique
        /// </summary>
        public abstract void Enable();

        /// <summary>
        /// Sets the view uniform to the given value
        /// </summary>
        /// <param name="view"></param>
        public void SetView(Matrix4 view)
        {
            SetView(ref view);
        }
        /// <summary>
        /// Sets the view uniform to the given value
        /// </summary>
        /// <param name="view"></param>
        public void SetView(ref Matrix4 view)
        {
            if(this.viewLocation != -1)
                GL.UniformMatrix4(this.viewLocation, false, ref view);
        }

        /// <summary>
        /// Sets the projection uniform to the given value
        /// </summary>
        /// <param name="projection"></param>
        public void SetProjection(Matrix4 projection)
        {
            SetProjection(ref projection);
        }
        /// <summary>
        /// Sets the projection uniform to the given value
        /// </summary>
        /// <param name="projection"></param>
        public void SetProjection(ref Matrix4 projection)
        {
            if(this.projectionLocation != -1)
                GL.UniformMatrix4(this.projectionLocation, false, ref projection);
        }

        /// <summary>
        /// Sets the world uniform to the given value
        /// </summary>
        /// <param name="world"></param>
        public void SetWorld(Matrix4 world)
        {
            SetWorld(ref world);
        }
        /// <summary>
        /// Sets the world uniform to the given value
        /// </summary>
        /// <param name="world"></param>
        public void SetWorld(ref Matrix4 world)
        {
            if (this.worldLocation != -1)
                GL.UniformMatrix4(this.worldLocation, false, ref world);
        }

        /// <summary>
        /// Sets the camPos uniform to the given value
        /// </summary>
        /// <param name="camPos"></param>
        public void SetCamPos(Vector3 camPos)
        {
            SetCamPos(ref camPos);
        }
        /// <summary>
        /// Sets the camPos uniform to the given value
        /// </summary>
        /// <param name="camPos"></param>
        public void SetCamPos(ref Vector3 camPos)
        {
            this.shader.SetVariable(this.camPosLocation, ref camPos);
        }

        /// <summary>
        /// Creates the shaders from files stored on disk.
        /// </summary>
        /// <param name="vertexShaderPath">The path to the vertex shader. Empty means it will be ignored.</param>
        /// <param name="fragmentShaderPath">The path to the fragment shader. Empty means it will be ignored.</param>
        /// <param name="geometryShaderPath">The path to the geometry shader. Empty means it will be ignored.</param>
        /// <returns>True if the shaders are succesfully created</returns>
        protected bool CreateShaderFromFile(String vertexShaderPath, String fragmentShaderPath, String geometryShaderPath)
        {
            this.shader = new GLShader();

            if (!String.IsNullOrEmpty(vertexShaderPath))
            {
                this.shader.AddShaderFile(vertexShaderPath, ShaderType.VertexShader);
                if (this.shader.HasErrors)
                {
                    this.errorMessage = this.shader.ErrorMessage;
                    return false;
                }
            }
            if (!String.IsNullOrEmpty(fragmentShaderPath))
            {
                this.shader.AddShaderFile(fragmentShaderPath, ShaderType.FragmentShader);
                if (this.shader.HasErrors)
                {
                    this.errorMessage = this.shader.ErrorMessage;
                    return false;
                }
            }
            if (!String.IsNullOrEmpty(geometryShaderPath))
            {
                this.shader.AddShaderFile(geometryShaderPath, ShaderType.GeometryShader);
                if (this.shader.HasErrors)
                {
                    this.errorMessage = this.shader.ErrorMessage;
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Creates the shaders
        /// </summary>
        /// <param name="vertexShaderSource">The code of the vertex shader. Empty means it will be ignored.</param>
        /// <param name="fragmentShaderSource">The code of the fragment shader. Empty means it will be ignored.</param>
        /// <param name="geometryShaderSource">The code of the geometry shader. Empty means it will be ignored.</param>
        /// <returns>True if the shaders are succesfully created</returns>
        protected bool CreateShaderFromSource(String vertexShaderSource, String fragmentShaderSource, String geometryShaderSource)
        {
            this.shader = new GLShader();

            if (!String.IsNullOrEmpty(vertexShaderSource))
            {
                this.shader.AddShaderSource(vertexShaderSource, ShaderType.VertexShader);
                if (this.shader.HasErrors)
                {
                    this.errorMessage = this.shader.ErrorMessage;
                    return false;
                }
            }
            if (!String.IsNullOrEmpty(fragmentShaderSource))
            {
                this.shader.AddShaderSource(fragmentShaderSource, ShaderType.FragmentShader);
                if (this.shader.HasErrors)
                {
                    this.errorMessage = this.shader.ErrorMessage;
                    return false;
                }
            }
            if (!String.IsNullOrEmpty(geometryShaderSource))
            {
                this.shader.AddShaderSource(geometryShaderSource, ShaderType.GeometryShader);
                if (this.shader.HasErrors)
                {
                    this.errorMessage = this.shader.ErrorMessage;
                    return false;
                }
            }

            return true;
        }
        private bool AddShaderFromSource(String source, ShaderType shaderType)
        {
            if (!this.shader.AddShaderSource(source, shaderType))
            {
                this.errorMessage = this.shader.ErrorMessage;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Sets a shader attribute.
        /// </summary>
        /// <param name="attribute">The attribute</param>
        /// <param name="attributeName">The attribute name in the shader</param>
        protected void SetShaderAttribute(int attribute, String attributeName)
        {
            GL.BindAttribLocation(this.shader.programId, attribute, attributeName);
        }
        /// <summary>
        /// Retrieves a location of an uniform in the current shader
        /// </summary>
        /// <param name="uniform">The name of the uniform</param>
        /// <returns>The location of the uniform</returns>
        protected int GetUniformLocation(String uniform)
        {
            return this.shader.GetShaderKey(uniform);
        }
        /// <summary>
        /// Specifies the transform feedback varyings, These are used to write to when using transform feedback.
        /// </summary>
        /// <param name="varyings">The names of the output variables</param>
        /// <param name="feedbackMode"></param>
        protected void SetTransformFeedbackVaryings(String[] varyings, TransformFeedbackMode feedbackMode)
        {
            GL.TransformFeedbackVaryings(this.shader.programId, varyings.Length, varyings, feedbackMode);
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// The current error message, if any.
        /// </summary>
        public String ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }
        }
        #endregion
    }
}
