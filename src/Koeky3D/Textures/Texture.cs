using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Koeky3D.Textures
{
    /// <summary>
    /// Base class for all textures used in Koeky3D
    /// </summary>
    public abstract class Texture
    {
        /// <summary>
        /// Binds the texture to the specified texture unit
        /// </summary>
        /// <param name="textureUnit"></param>
        public abstract void BindTexture(TextureUnit textureUnit);
        /// <summary>
        /// Unbinds the texture from the last specified texture unit
        /// </summary>
        public abstract void UnbindTexture();

        /// <summary>
        /// Destroys the texture
        /// </summary>
        public abstract void DestroyTexture();

        /// <summary>
        /// Sets a parameter.
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The parameter value</param>
        public abstract void SetParameter(TextureParameterName name, float value);
        /// <summary>
        /// Sets a parameter.
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The parameter value</param>
        public abstract void SetParameter(TextureParameterName name, int value);

        /// <summary>
        /// The id of this texture
        /// </summary>
        public abstract int TextureId { get; }
        /// <summary>
        /// The target of this texture. For example: Texture2D or Texture3D
        /// </summary>
        public abstract TextureTarget TextureTarget { get; }
        /// <summary>
        /// The internal format of the pixels
        /// </summary>
        public abstract PixelInternalFormat PixelInternalFormat { get; }
        /// <summary>
        /// The type of pixel stored in this texture. For example: float or unsigned int
        /// </summary>
        public abstract PixelType PixelType { get; }
    }
}
