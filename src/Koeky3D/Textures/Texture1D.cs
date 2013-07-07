using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Koeky3D.Textures
{
    /// <summary>
    /// Provides a wrapper for 1d textures in OpenGL
    /// </summary>
    public class Texture1D : Texture
    {
        #region variables
        private readonly int textureId;

        private PixelInternalFormat internalFormat;
        private PixelType pixelType;

        private int width;

        private TextureUnit lastActiveUnit;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a new empty 1d texture
        /// </summary>
        /// <param name="internalFormat">The pixel format to use</param>
        /// <param name="pixelType">The pixel type to use</param>
        public Texture1D(PixelInternalFormat internalFormat, PixelType pixelType)
        {
            this.textureId = GL.GenTexture();

            this.internalFormat = internalFormat;
            this.pixelType = pixelType;
        }
        #endregion

        #region Texture1D methods
        /// <summary>
        /// Binds this texture to the given texture unit
        /// </summary>
        /// <param name="unit"></param>
        public override void BindTexture(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture1D, this.textureId);

            this.lastActiveUnit = unit;
        }

        /// <summary>
        /// Unbinds this texture from the last bound texture unit
        /// </summary>
        public override void UnbindTexture()
        {
            GL.ActiveTexture(this.lastActiveUnit);
            GL.BindTexture(TextureTarget.Texture1D, 0);
        }

        /// <summary>
        /// Destroys this texture, freeing any resources used.
        /// This object can no longer be used after this call.
        /// </summary>
        public override void DestroyTexture()
        {
            GL.DeleteTexture(this.textureId);
        }

        /// <summary>
        /// Sets a parameter. This function assumes BindTexture has been called first.
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The parameter value</param>
        public override void SetParameter(TextureParameterName name, float value)
        {
            GL.TexParameter(TextureTarget.Texture1D, name, value);
        }
        /// <summary>
        /// Sets a parameter. This function assumes BindTexture has been called first
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The parameter value</param>
        public override void SetParameter(TextureParameterName name, int value)
        {
            GL.TexParameter(TextureTarget.Texture1D, name, value);
        }

        /// <summary>
        /// Sets the texture data
        /// </summary>
        /// <param name="scan0">A pointer to the data</param>
        /// <param name="pixelFormat">The format in which the data is delivered</param>
        /// <param name="width">The width of the texture</param>
        public void SetData(IntPtr scan0, PixelFormat pixelFormat, int width)
        {
            this.width = width;
            GL.TexImage1D(TextureTarget.Texture1D, 0, this.internalFormat, width, 0, pixelFormat, this.pixelType, scan0);
        }

        /// <summary>
        /// Sets the data of this texture
        /// </summary>
        /// <typeparam name="T">The type of the data. Must be of type struct.</typeparam>
        /// <param name="values">The data of the texture</param>
        /// <param name="pixelFormat">The pixel format of the given data</param>
        public void SetData<T>(T[] values, PixelFormat pixelFormat) where T : struct
        {
            this.width = values.Length;
            pixelFormat = PixelFormat.Red;
            GL.TexImage1D<T>(TextureTarget.Texture1D, 0, this.internalFormat, values.Length, 0, pixelFormat, pixelType, values);
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// The texture id of this texture
        /// </summary>
        public override int TextureId
        {
            get
            {
                return this.textureId;
            }
        }

        /// <summary>
        /// The 1d texture target
        /// </summary>
        public override TextureTarget TextureTarget
        {
            get
            {
                return TextureTarget.Texture1D;
            }
        }

        /// <summary>
        /// The pixel format used by this 1d texture
        /// </summary>
        public override PixelInternalFormat PixelInternalFormat
        {
            get
            {
                return this.internalFormat;
            }
        }

        /// <summary>
        /// The pixel type used by this 1d texture
        /// </summary>
        public override PixelType PixelType
        {
            get
            {
                return this.pixelType;
            }
        }

        /// <summary>
        /// The width of this 1D texture
        /// </summary>
        public int Width
        {
            get
            {
                return this.width;
            }
        }
        #endregion
    }
}
