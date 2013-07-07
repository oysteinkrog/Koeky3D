using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Koeky3D.Textures
{
    /// <summary>
    /// Provides a wrapper for 2d textures in OpenGL
    /// </summary>
    public class Texture2D : Texture
    {
        #region variables
        /// <summary>
        /// The id of this texture as used by OpenGL
        /// </summary>
        public readonly int textureId;

        private PixelInternalFormat pixelInternalFormat;
        private PixelType pixelType;

        private int width, height;

        private TextureUnit lastActiveUnit;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs an empty Texture2D.
        /// </summary>
        public Texture2D()
            : this(PixelInternalFormat.Rgba8, PixelType.UnsignedByte)
        {
        }
        /// <summary>
        /// Constructs an empty Texture2D with the specified internal pixel format and pixel type.
        /// </summary>
        /// <param name="pixelInternalFormat">The internal pixel format to use</param>
        /// <param name="pixelType">The pixel type to use</param>
        public Texture2D(PixelInternalFormat pixelInternalFormat, PixelType pixelType)
        {
            this.textureId = GL.GenTexture();
            this.pixelInternalFormat = pixelInternalFormat;
            this.pixelType = pixelType;
        }
        /// <summary>
        /// Constructs a texture from the given bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap used to create this Texture2D</param>
        /// <param name="genMipMaps">True if mipmaps should be generated</param>
        public Texture2D(Bitmap bitmap, bool genMipMaps)
        {
            this.textureId = GL.GenTexture();
            this.width = bitmap.Width;
            this.height = bitmap.Height;

            PixelFormat uploadFormat;
            if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                this.pixelInternalFormat = PixelInternalFormat.Rgba8;
                this.pixelType = PixelType.UnsignedByte;
                uploadFormat = PixelFormat.Bgra;
            }
            else if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                this.pixelInternalFormat = PixelInternalFormat.Rgb;
                this.pixelType = PixelType.UnsignedByte;
                uploadFormat = PixelFormat.Bgr;
            }
            else
            {
                throw new Exception("Unsupported pixel format");
            }

            Rectangle area = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(area, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            BindTexture();
            SetData(data.Scan0, uploadFormat, bitmap.Width, bitmap.Height);
            SetParameter(TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            SetParameter(TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            if (genMipMaps)
                GenerateMipmaps();
            GL.BindTexture(TextureTarget.Texture2D, 0);

            bitmap.UnlockBits(data);
        }
        #endregion

        #region Texture2D methods
        /// <summary>
        /// Binds this texture to TextureUnit0.
        /// </summary>
        public void BindTexture()
        {
            BindTexture(TextureUnit.Texture0);
        }
        /// <summary>
        /// Binds this texture to the specified texture unit.
        /// </summary>
        /// <param name="textureUnit"></param>
        public override void BindTexture(TextureUnit textureUnit)
        {
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, this.textureId);

            this.lastActiveUnit = textureUnit;
        }
        /// <summary>
        /// Unbinds the texture from the last bound texture unit.
        /// </summary>
        public override void UnbindTexture()
        {
            GL.ActiveTexture(this.lastActiveUnit);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /// <summary>
        /// Destroys this texture. It is no longer usable after this call.
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
            GL.TexParameter(TextureTarget.Texture2D, name, value);
        }
        /// <summary>
        /// Sets a parameter. This function assumes BindTexture has been called first
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The parameter value</param>
        public override void SetParameter(TextureParameterName name, int value)
        {
            GL.TexParameter(TextureTarget.Texture2D, name, value);
        }

        /// <summary>
        /// Uploads data to this texture. This method assumes BindTexture has been called first
        /// </summary>
        /// <param name="data">The data. Pass IntPtr.Zero to specify no data</param>
        /// <param name="pixelFormat">The pixel format of the given data</param>
        /// <param name="width">The width of the texture</param>
        /// <param name="height">The height of the texture</param>
        public void SetData(IntPtr data, PixelFormat pixelFormat, int width, int height)
        {
            this.width = width;
            this.height = height;

            GL.TexImage2D(TextureTarget.Texture2D, 0, this.pixelInternalFormat, width, height, 0, pixelFormat, this.pixelType, data);
        }
        /// <summary>
        /// Uploads data to this texture. This method assumes BindTexture has been called first.
        /// </summary>
        /// <typeparam name="T">The type of the data to upload. This value must be a struct type.</typeparam>
        /// <param name="values">The data to upload in a 2 dimensional array</param>
        /// <param name="pixelFormat">The pixel format of the given data.</param>
        public void SetData<T>(T[,] values, PixelFormat pixelFormat) where T : struct
        {
            this.width = values.GetLength(0);
            this.height = values.GetLength(1);

            GL.TexImage2D<T>(TextureTarget.Texture2D, 0, this.pixelInternalFormat, values.GetLength(0), values.GetLength(1), 0, pixelFormat, this.pixelType, values);
        }

        /// <summary>
        /// Generates mipmaps for this texture. This method assumes BindTexture() has been called.
        /// This method can be called multiple times. For example: after a FrameBuffer has changed its contents.
        /// </summary>
        public void GenerateMipmaps()
        {
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        /// <summary>
        /// Deletes this texture. This object is no longer valid after this call.
        /// </summary>
        public void UnloadResources()
        {
            GL.DeleteTexture(this.textureId);
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// The width of this texture
        /// </summary>
        public int Width
        {
            get
            {
                return this.width;
            }
        }
        /// <summary>
        /// The height of this texture
        /// </summary>
        public int Height
        {
            get
            {
                return this.height;
            }
        }

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
        /// The 2d texture target
        /// </summary>
        public override TextureTarget TextureTarget
        {
            get
            {
                return TextureTarget.Texture2D;
            }
        }

        /// <summary>
        /// The pixel format used by this texture 2d
        /// </summary>
        public override PixelInternalFormat PixelInternalFormat
        {
            get
            {
                return this.pixelInternalFormat;
            }
        }

        /// <summary>
        /// The pixel type used by this texture 2d
        /// </summary>
        public override PixelType PixelType
        {
            get
            {
                return this.pixelType;
            }
        }
        #endregion

        
    }
}
