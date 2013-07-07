using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using Koeky3D.Properties;

namespace Koeky3D.Shaders
{
    /// <summary>
    /// Provides easy creation and caching of shaders.
    /// </summary>
    [System.Obsolete("Use the Technique class instead", false)]
    public static class ShaderConstructor
    {
        private static Dictionary<String, GLShader> constructedShaders = new Dictionary<String, GLShader>();
        private static GLShader defaultShader;       

        /// <summary>
        /// Constructs a shader. If the shader already excists the cached version will be returned.
        /// If the shader failed to load a default shader will be returned.
        /// </summary>
        /// <param name="vertexShaderPath"></param>
        /// <param name="fragmentShaderPath"></param>
        /// <returns></returns>
        public static GLShader ConstructShader(String vertexShaderPath, String fragmentShaderPath)
        {
            return ConstructShader(vertexShaderPath, fragmentShaderPath, "");
        }
        /// <summary>
        /// Constructs a shader. If the shader already excists the cached version will be returned.
        /// If the shader failed to load a default shader will be returned.
        /// </summary>
        /// <param name="vertexShaderPath"></param>
        /// <param name="fragmentShaderPath"></param>
        /// <param name="geometryShaderPath"></param>
        /// <returns></returns>
        public static GLShader ConstructShader(String vertexShaderPath, String fragmentShaderPath, String geometryShaderPath)
        {
            String key = vertexShaderPath.Trim() + fragmentShaderPath.Trim() + geometryShaderPath.Trim();

            GLShader shader;
            if (constructedShaders.TryGetValue(key, out shader))
                return shader;

            shader = new GLShader();
            if(!String.IsNullOrEmpty(vertexShaderPath))
                shader.AddShaderFile(vertexShaderPath, ShaderType.VertexShader);
            if(!String.IsNullOrEmpty(fragmentShaderPath))
                shader.AddShaderFile(fragmentShaderPath, ShaderType.FragmentShader);
            if(!String.IsNullOrEmpty(geometryShaderPath))
                shader.AddShaderFile(geometryShaderPath, ShaderType.GeometryShader);

            if (shader.HasErrors)
            {
                ErrorShader.HasErrors = true;
                ErrorShader.ErrorMessage = shader.ErrorMessage;
                return ErrorShader;
            }
            constructedShaders.Add(key, shader);

            return shader;
        }

        /// <summary>
        /// Gets an error shader. The error shader displays every mesh with a bright red color.
        /// Alternatively you could use this shader as a default shader for testing purposes.
        /// </summary>
        public static GLShader ErrorShader
        {
            get
            {
                if (defaultShader == null)
                {
                    defaultShader = new GLShader();
                    defaultShader.AddShaderSource(@Resources.ErrorVertexShader, ShaderType.VertexShader);
                    defaultShader.AddShaderSource(@Resources.ErrorFragmentShader, ShaderType.FragmentShader);
                }
                return defaultShader;
            }
        }
    }
}
