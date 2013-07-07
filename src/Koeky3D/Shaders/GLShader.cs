using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Graphics;
using Koeky3D.BufferHandling;

namespace Koeky3D.Shaders
{
    /// <summary>
    /// Provides a wrapper for an OpenGL shader.
    /// This class wraps the creation of entire shaders (vertex + fragment shaders for example) in one class.
    /// </summary>
    public class GLShader
    {
        #region variables
        /// <summary>
        /// The program id of this shader
        /// </summary>
        public readonly int programId;

        private String errorMessage;
        private bool hasErrors;

        private Dictionary<String, int> cachedShaderKeys = new Dictionary<String, int>();

        private bool linked = false;
        private bool validated = false;
        #endregion

        #region constructors
        /// <summary>
        /// Creates an empty shader.
        /// </summary>
        public GLShader()
        {
            this.programId = GL.CreateProgram();
        }
        /// <summary>
        /// Creates a shader with the source loaded from the specified filepaths
        /// </summary>
        /// <param name="vertexShaderPath">The file path pointing to the source of the vertex shader</param>
        /// <param name="fragmentShaderPath">The file path pointing to the source of the fragment shader</param>
        public GLShader(String vertexShaderPath, String fragmentShaderPath)
        {
            // create shader id
            this.programId = GL.CreateProgram();

            // check if the shader files exist
            if (!File.Exists(vertexShaderPath))
            {
                this.errorMessage = "Vertex shader not found";
                this.hasErrors = true;
                return;
            }
            if (!File.Exists(fragmentShaderPath))
            {
                this.errorMessage = "Fragment shader not found";
                this.hasErrors = true;
                return;
            }

            // read vertex shader source
            StreamReader reader = new StreamReader(vertexShaderPath);
            String vertexShaderSource = reader.ReadToEnd();
            reader.Close();
            // read fragment shader source
            reader = new StreamReader(fragmentShaderPath);
            String fragmentShaderSource = reader.ReadToEnd();
            reader.Close();

            // load the shaders
            if (!AddShaderSource(fragmentShaderSource, ShaderType.FragmentShader))
                return;
            if (!AddShaderSource(vertexShaderSource, ShaderType.VertexShader))
                return;
        }
        #endregion

        #region GLShader methods
        /// <summary>
        /// Adds a shader from a file.
        /// </summary>
        /// <param name="path">The path to the file containing the shader code</param>
        /// <param name="type">The type of the shader</param>
        /// <returns>True if the shader was added succesfully</returns>
        public bool AddShaderFile(String path, ShaderType type)
        {
            // check if the shader files exist
            if (!File.Exists(path))
            {
                this.errorMessage = "File not found";
                this.hasErrors = true;
                return false;
            }

            // read shader source
            StreamReader reader = new StreamReader(path);
            String shaderSource = reader.ReadToEnd();
            reader.Close();

            // add the shader
            return AddShaderSource(shaderSource, type);
        }
        /// <summary>
        /// Adds a shader from source
        /// </summary>
        /// <param name="source">The source of the shader</param>
        /// <param name="type">The type of the shader</param>
        /// <returns>True if the shader was added succesfully</returns>
        public bool AddShaderSource(String source, ShaderType type)
        {
            int shaderId = GL.CreateShader(type);
            GL.ShaderSource(shaderId, @source);
            GL.CompileShader(shaderId);

            // load shader info log
            GL.GetShaderInfoLog(shaderId, out this.errorMessage);

            // check if the compile was succesful.
            int compileResult;
            GL.GetShader(shaderId, ShaderParameter.CompileStatus, out compileResult);
            if (compileResult != 1)
            {
                this.hasErrors = true;
                return false;
            }

            // attack the shader
            GL.AttachShader(this.programId, shaderId);

            return true;
        }

        /// <summary>
        /// Links this shader
        /// </summary>
        /// <returns>True if linked succesfully</returns>
        public bool LinkShader()
        {
            if (!this.linked)
            {
                GL.LinkProgram(this.programId);

                int linkStatus;
                GL.GetProgram(this.programId, ProgramParameter.LinkStatus, out linkStatus);
                if (linkStatus == 0)
                {
                    // linking failed
                    this.errorMessage = "LINK ERROR: " + GL.GetProgramInfoLog(this.programId);
                    this.hasErrors = true;
                    return false;
                }

                return true;
            }

            this.errorMessage = "Shader is already linked";
            this.hasErrors = true;
            return false;
        }
        /// <summary>
        /// Validates this shader
        /// </summary>
        /// <returns>True if validated succesfully</returns>
        public bool Validate()
        {
            if (!this.validated)
            {
                GL.ValidateProgram(this.programId);
                int validateStatus;
                GL.GetProgram(this.programId, ProgramParameter.ValidateStatus, out validateStatus);
                if (validateStatus == 0)
                {
                    this.errorMessage = "VALIDATE ERROR: " + GL.GetProgramInfoLog(this.programId);
                    this.hasErrors = true;
                    return false;
                }

                return true;
            }

            this.errorMessage = "Shader is already validated";
            this.hasErrors = true;
            return false;
        }

        /// <summary>
        /// Binds the shader, making it active
        /// </summary>
        public void BindShader()
        {
            GL.UseProgram(this.programId);
        }
        /// <summary>
        /// Unbinds this shader
        /// </summary>
        public void UnbindShader()
        {
            GL.UseProgram(0);
        }
        #endregion

        #region Shader variable editing
        /// <summary>
        /// Retrieves the location of an uniform
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <returns>The location of the uniform</returns>
        public int GetShaderKey(String name)
        {
            int key;
            if (cachedShaderKeys.TryGetValue(name, out key))
                return key;

            key = GL.GetUniformLocation(this.programId, name);
            this.cachedShaderKeys.Add(name, key);

            return key;
        }

        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="value">The new value of the uniform</param>
        public void SetVariable(String name, float value)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform1(location, value);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="value">The new value of the uniform</param>
        public void SetVariable(String name, int value)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform1(location, value);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="value">The new value of the uniform</param>
        public void SetVariable(String name, uint value)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform1(location, value);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="value">The new value of the uniform</param>
        public void SetVariable(String name, bool value)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform1(location, value ? 1 : 0);
        }

        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="vector">The new value of the uniform</param>
        public void SetVariable(String name, Vector3 vector)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform3(location, ref vector);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="vector">The new value of the uniform</param>
        public void SetVariable(String name, ref Vector3 vector)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform3(location, vector);
        }

        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="vector">The new value of the uniform</param>
        public void SetVariable(String name, Vector4 vector)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform4(location, ref vector);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="vector">The new value of the uniform</param>
        public void SetVariable(String name, ref Vector4 vector)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform4(location, ref vector);
        }

        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="color">The new value of the uniform</param>
        public void SetVariable(String name, Color4 color)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform4(location, color);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="color">The new value of the uniform</param>
        public void SetVariable(String name, ref Color4 color)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform4(location, color);
        }

        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="matrix">The new value of the uniform</param>
        public void SetVariable(String name, Matrix4 matrix)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.UniformMatrix4(location, false, ref matrix);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="matrix">The new value of the uniform</param>
        public void SetVariable(String name, ref Matrix4 matrix)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.UniformMatrix4(location, false, ref matrix);
        }

        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="vector">The new value of the uniform</param>
        public void SetVariable(String name, Vector2 vector)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform2(location, ref vector);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="vector">The new value of the uniform</param>
        public void SetVariable(String name, ref Vector2 vector)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform2(location, ref vector);
        }

        /// <summary>
        /// Sets an array of uniforms.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="count"></param>
        /// <param name="value">The new values of the uniform</param>
        public void SetVariable(String name, int count, float[] value)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform4(location, count, value);
        }
        /// <summary>
        /// Sets an array of uniforms.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="count"></param>
        /// <param name="value">The new values of the uniform</param>
        public void SetVariable(String name, int count, int[] value)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform4(location, count, value);
        }
        /// <summary>
        /// Sets an array of uniforms.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="count"></param>
        /// <param name="value">The new values of the uniform</param>
        public void SetVariable(String name, int count, uint[] value)
        {
            int location = GetShaderKey(name);
            if (location != -1)
                GL.Uniform4(location, count, value);
        }

        /// <summary>
        /// Sets an array of uniforms.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="count"></param>
        /// <param name="value">The new values of the uniform</param>
        public void SetVariable(String name, int count, Matrix4[] value)
        {
            int location = GetShaderKey(name);
            if (location != -1)
            {
                unsafe
                {
                    // retreive a pointer to the start of the array
                    // because it contains only Matrix4 structs we can pass this pointer directly to OpenGL
                    fixed (void* ptr = value)
                    {
                        float* floatPtr = (float*)ptr;
                        GL.UniformMatrix4(location, count, true, floatPtr);
                    }
                }
            }
        }

        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="value">The new value of the uniform</param>
        public void SetVariable(int location, float value)
        {
            GL.Uniform1(location, value);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="value">The new value of the uniform</param>
        public void SetVariable(int location, int value)
        {
            GL.Uniform1(location, value);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="value">The new value of the uniform</param>
        public void SetVariable(int location, uint value)
        {
            GL.Uniform1(location, value);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="value">The new value of the uniform</param>
        public void SetVariable(int location, bool value)
        {
            GL.Uniform1(location, value ? 1 : 0);
        }

        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="vector">The new value of the uniform</param>
        public void SetVariable(int location, Vector3 vector)
        {
            GL.Uniform3(location, ref vector);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="vector">The new value of the uniform</param>
        public void SetVariable(int location, ref Vector3 vector)
        {
            GL.Uniform3(location, vector);
        }

        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="vector">The new value of the uniform</param>
        public void SetVariable(int location, Vector4 vector)
        {
            GL.Uniform4(location, ref vector);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="vector">The new value of the uniform</param>
        public void SetVariable(int location, ref Vector4 vector)
        {
            GL.Uniform4(location, ref vector);
        }

        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="color">The new value of the uniform</param>
        public void SetVariable(int location, Color4 color)
        {
            GL.Uniform4(location, color);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="color">The new value of the uniform</param>
        public void SetVariable(int location, ref Color4 color)
        {
            GL.Uniform4(location, color);
        }

        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="matrix">The new value of the uniform</param>
        public void SetVariable(int location, Matrix4 matrix)
        {
            GL.UniformMatrix4(location, false, ref matrix);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="matrix">The new value of the uniform</param>
        public void SetVariable(int location, ref Matrix4 matrix)
        {
            GL.UniformMatrix4(location, false, ref matrix);
        }

        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="vector">The new value of the uniform</param>
        public void SetVariable(int location, Vector2 vector)
        {
            GL.Uniform2(location, ref vector);
        }
        /// <summary>
        /// Sets an uniform.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="vector">The new value of the uniform</param>
        public void SetVariable(int location, ref Vector2 vector)
        {
            GL.Uniform2(location, ref vector);
        }

        /// <summary>
        /// Sets an array of uniforms.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="count"></param>
        /// <param name="value">The new values of the uniform</param>
        public void SetVariable(int location, int count, float[] value)
        {
            GL.Uniform4(location, count, value);
        }
        /// <summary>
        /// Sets an array of uniforms.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="count"></param>
        /// <param name="value">The new values of the uniform</param>
        public void SetVariable(int location, int count, int[] value)
        {
            GL.Uniform4(location, count, value);
        }
        /// <summary>
        /// Sets an array of uniforms.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="count"></param>
        /// <param name="value">The new values of the uniform</param>
        public void SetVariable(int location, int count, uint[] value)
        {
            GL.Uniform4(location, count, value);
        }

        /// <summary>
        /// Sets an array of uniforms.
        /// </summary>
        /// <param name="location">The name of the uniform</param>
        /// <param name="count"></param>
        /// <param name="value">The new values of the uniform</param>
        public void SetVariable(int location, int count, Matrix4[] value)
        {
            unsafe
            {
                // retreive a pointer to the start of the array
                // because it contains only Matrix4 structs we can pass this pointer directly to OpenGL
                fixed (void* ptr = value)
                {
                    float* floatPtr = (float*)ptr;
                    GL.UniformMatrix4(location, count, true, floatPtr);
                }
            }
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// True if this shader has errors
        /// </summary>
        public bool HasErrors
        {
            get
            {
                return this.hasErrors;
            }
            set
            {
                this.hasErrors = value;
            }
        }
        /// <summary>
        /// The error message if there are errors
        /// </summary>
        public String ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }
            set
            {
                this.errorMessage = value;
            }
        }
        #endregion
    }
}
