using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Koeky3D.Textures
{
    /// <summary>
    /// Provides a wrapper for cubemap textures in OpenGL
    /// </summary>
    public class TextureCube : Texture
    {
        #region variables
        private readonly int textureId;

        private int width, height;

        private PixelInternalFormat pixelInternalFormat;
        private PixelType pixelType;

        private TextureUnit lastActiveUnit;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a cube map with an internal pixel format of rgba8 and pixeltype unsigned byte
        /// </summary>
        public TextureCube()
            : this(PixelInternalFormat.Rgba8, PixelType.UnsignedByte)
        {
        }
        /// <summary>
        /// Constructs a cube map with the specified internal pixel format and pixel type
        /// </summary>
        /// <param name="pixelInternalFormat"></param>
        /// <param name="pixelType"></param>
        public TextureCube(PixelInternalFormat pixelInternalFormat, PixelType pixelType)
        {
            this.pixelInternalFormat = pixelInternalFormat;
            this.pixelType = pixelType;

            this.textureId = GL.GenTexture();
        }
        /// <summary>
        /// Constructs a cube map from the given bitmaps and with the specified internal pixel format and pixel type
        /// </summary>
        /// <param name="cubeFaces">The bitmaps to use for the cube map. This array must be filled with exactly 6 bitmaps. The resolution of every bitmap must be the same.</param>
        /// <param name="pixelInternalFormat"></param>
        /// <param name="pixelType"></param>
        public TextureCube(Bitmap[] cubeFaces, PixelInternalFormat pixelInternalFormat, PixelType pixelType)
        {
            // Check if there are exactly 6 bitmaps available
            if (cubeFaces.Length != 6)
                throw new Exception("Constructing a cubemap requires 6 bitmaps");

            // Upload every bitmap
            this.width = cubeFaces[0].Width;
            this.height = cubeFaces[0].Height;
            for (int i = 0; i < 6; i++)
            {
                Bitmap bitmap = cubeFaces[i];

                // Check if the resolution is correct, every face of the cubemap must have exactly the same resolution
                if (bitmap.Width != this.width || bitmap.Height != this.height)
                    throw new Exception("Every given bitmap must have the same resolution");

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
                SetData(TextureTarget.TextureCubeMapPositiveX + i, uploadFormat, data.Scan0, this.width, this.height);
                //SetParameter(TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                //SetParameter(TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.BindTexture(TextureTarget.TextureCubeMap, 0);

                bitmap.UnlockBits(data);
            }
        }
        #endregion

        #region Texture methods
        /// <summary>
        /// Binds this cube map to texture unit 0
        /// </summary>
        public void BindTexture()
        {
            BindTexture(TextureUnit.Texture0);
        }
        /// <summary>
        /// Binds this cube map to the specified texture unit
        /// </summary>
        /// <param name="textureUnit"></param>
        public override void BindTexture(TextureUnit textureUnit)
        {
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.TextureCubeMap, this.textureId);

            this.lastActiveUnit = textureUnit;
        }

        /// <summary>
        /// Unbinds the texture from the last bound texture unit
        /// </summary>
        public override void UnbindTexture()
        {
            GL.ActiveTexture(this.lastActiveUnit);
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }

        /// <summary>
        /// Destroys the texture, freeing any used resources.
        /// You cannot use this object after this call.
        /// </summary>
        public override void DestroyTexture()
        {
            GL.DeleteTexture(this.textureId);
        }

        /// <summary>
        /// Sets a parameter of this cubemap.
        /// For this method to have effect you must first call BindTexture
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public override void SetParameter(TextureParameterName name, int value)
        {
            GL.TexParameter(TextureTarget.TextureCubeMap, name, value);
        }
        /// <summary>
        /// Sets a parameter of this cubemap.
        /// For this method to have effect you must first call BindTexture
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public override void SetParameter(TextureParameterName name, float value)
        {
            GL.TexParameter(TextureTarget.TextureCubeMap, name, value);
        }
        #endregion

        #region TextureCube methods
        /// <summary>
        /// Sets the data for the given cubemap face
        /// </summary>
        /// <param name="cubeFace">The face to place the texture data on.</param>
        /// <param name="format">The pixel format of the given data</param>
        /// <param name="pixels">The pixel data</param>
        /// <param name="width">The width of the data</param>
        /// <param name="height">The height of the data</param>
        public void SetData(TextureTarget cubeFace, PixelFormat format, IntPtr pixels, int width, int height)
        {
            this.width = width;
            this.height = height;
            GL.TexImage2D(cubeFace, 0, this.pixelInternalFormat, this.width, this.height, 0, format, this.pixelType, pixels);
        }
        /// <summary>
        /// Sets the data for the given cubemap face
        /// </summary>
        /// <typeparam name="T">The type of the data</typeparam>
        /// <param name="cubeFace">The face to place the texture data on</param>
        /// <param name="format">The format of the given pixel data</param>
        /// <param name="values">The pixel data</param>
        public void SetData<T>(TextureTarget cubeFace, PixelFormat format, T[,] values) where T : struct
        {
            this.width = values.GetLength(0);
            this.height = values.GetLength(1);
            GL.TexImage2D<T>(cubeFace, 0, this.pixelInternalFormat, this.width, this.height, 0, format, this.pixelType, values);
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// The width of a cubemap face
        /// </summary>
        public int Width
        {
            get
            {
                return this.width;
            }
        }
        /// <summary>
        /// The height of a cubemap face
        /// </summary>
        public int Height
        {
            get
            {
                return this.height;
            }
        }

        /// <summary>
        /// The texture id of this cubemap
        /// </summary>
        public override int TextureId
        {
            get
            {
                return this.textureId;
            }
        }
        /// <summary>
        /// The texturetarget
        /// </summary>
        public override TextureTarget TextureTarget
        {
            get
            {
                return TextureTarget.TextureCubeMap;
            }
        }

        /// <summary>
        /// The internal pixel format of this cubemap
        /// </summary>
        public override PixelInternalFormat PixelInternalFormat
        {
            get
            {
                return this.pixelInternalFormat;
            }
        }
        /// <summary>
        /// The type of the pixels
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
