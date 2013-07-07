using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Koeky3D.Textures
{
    /// <summary>
    /// Provides a wrapper for 3d textures in OpenGL.
    /// </summary>
    public class Texture3D : Texture
    {
        #region variables
        private int textureId;
        private PixelInternalFormat pixelInternalFormat;
        private PixelType pixelType;
        private TextureUnit lastTextureUnit;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a new Texture3D object with internal pixel format Rgba8 and pixel type unsigned int.
        /// </summary>
        public Texture3D()
            : this(PixelInternalFormat.Rgba8, PixelType.UnsignedByte)
        {
        }
        /// <summary>
        /// Construcs a new Texture3D object
        /// </summary>
        /// <param name="pixelInternalFormat">The internal pixel format to use</param>
        /// <param name="pixelType">The pixel type to use</param>
        public Texture3D(PixelInternalFormat pixelInternalFormat, PixelType pixelType)
        {
            this.pixelInternalFormat = pixelInternalFormat;
            this.pixelType = pixelType;

            this.textureId = GL.GenTexture();
        }
        /// <summary>
        /// Constructs a new Texture3D object using a collection of bitmaps as texture data.
        /// This method must internally convert the array of bitmaps to something that can be uploaded to the gpu.
        /// For this reason this method may be slow.
        /// </summary>
        /// <param name="data">The texture data. Every bitmap is one slice of the texture 3d. Every bitmap MUST have the same dimensions.</param>
        /// <param name="pixelInternalFormat">The internal pixel format to use</param>
        /// <param name="pixelType">The pixel type to use</param>
        public Texture3D(Bitmap[] data, PixelInternalFormat pixelInternalFormat, PixelType pixelType)
        {
            // Sadly there appears to be no easy way to upload this data. 
            // So we have to convert it first.
            int width = data[0].Width;
            int height = data[0].Height;
            int depth = data.Length;
            byte[, ,] bitmapData = new byte[depth, height, width * 4];

            for (int i = 0; i < depth; i++)
            {
                Bitmap bitmap = data[i];
                if (bitmap.Width != width || bitmap.Height != height)
                    throw new Exception("All bitmaps must have the same dimensions");

                // retrieve a pointer to the data. We use this for fast acces to the bitmap.
                System.Drawing.Imaging.BitmapData bitmapD = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), 
                                                                            System.Drawing.Imaging.ImageLockMode.ReadOnly, 
                                                                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* bytePtr = (byte*)bitmapD.Scan0.ToPointer();

                    for (int y = 0; y < height; y++)
                    {
                        int index = y * bitmapD.Stride; 
                        for (int x = 0; x < width; x++)
                        {
                            bitmapData[i, y, x] = bytePtr[index];
                            bitmapData[i, y, x + 1] = bytePtr[index + 1];
                            bitmapData[i, y, x + 2] = bytePtr[index + 2];
                            bitmapData[i, y, x + 3] = bytePtr[index + 3];

                            index += 4;
                        }
                    }
                }

                bitmap.UnlockBits(bitmapD);
            }

            this.pixelInternalFormat = pixelInternalFormat;
            this.pixelType = pixelType;
            this.textureId = GL.GenTexture();

            bitmapData = new byte[64, 16, 2];
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    bitmapData[x + 0, y, 0] = 255;
                    bitmapData[x + 1, y, 0] = 0;
                    bitmapData[x + 2, y, 0] = 0;
                    bitmapData[x + 3, y, 0] = 255;
                }
            }

            this.SetData<byte>(bitmapData, PixelFormat.Rgba);
        }
        #endregion

        #region Texture methods
        /// <summary>
        /// Binds this texture.
        /// </summary>
        /// <param name="textureUnit">The texture unit to bind to</param>
        public override void BindTexture(TextureUnit textureUnit)
        {
            this.lastTextureUnit = textureUnit;
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture3D, this.textureId);
        }
        /// <summary>
        /// Unbinds this texture from the last bound texture unit
        /// </summary>
        public override void UnbindTexture()
        {
            GL.ActiveTexture(this.lastTextureUnit);
            GL.BindTexture(TextureTarget.Texture3D, 0);
        }

        /// <summary>
        /// Destroys this texture, clearing any resources in use. Do not use this object after this call.
        /// </summary>
        public override void DestroyTexture()
        {
            GL.DeleteTexture(this.textureId);
        }

        /// <summary>
        /// Sets the specified parameter to the specified value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public override void SetParameter(TextureParameterName name, float value)
        {
            GL.TexParameter(TextureTarget.Texture3D, name, value);
        }
        /// <summary>
        /// Sets the specified parameter to the specified value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public override void SetParameter(TextureParameterName name, int value)
        {
            GL.TexParameter(TextureTarget.Texture3D, name, value);
        }
        #endregion

        #region Texture3D methods
        /// <summary>
        /// Sets the texture data of this texture 3d.
        /// </summary>
        /// <typeparam name="T">The type of the data. Must be a struct</typeparam>
        /// <param name="data">The data to upload. The array is defined as following: [depth, width, height]</param>
        /// <param name="format">The format of the data to upload</param>
        public void SetData<T>(T[,,] data, PixelFormat format) where T : struct
        {
            GL.TexImage3D<T>(TextureTarget.Texture3D, 0, this.pixelInternalFormat, data.GetLength(0), data.GetLength(1), data.GetLength(2), 0, format, this.pixelType, data);
        }
        /// <summary>
        /// Sets the texture data of this texture 3d
        /// </summary>
        /// <param name="data">A pointer to the data</param>
        /// <param name="width">The width of the data</param>
        /// <param name="height">The height of the data</param>
        /// <param name="depth">The depth of the data</param>
        /// <param name="format">The pixel format of the data</param>
        public void SetData(IntPtr data, int width, int height, int depth, PixelFormat format)
        {
            GL.TexImage3D(TextureTarget.Texture3D, 0, this.pixelInternalFormat, width, height, depth, 0, format, this.pixelType, data);
        }
        #endregion

        #region getters & setters
        public override int TextureId
        {
            get { return this.textureId; }
        }

        public override TextureTarget TextureTarget
        {
            get { return OpenTK.Graphics.OpenGL.TextureTarget.Texture3D; }
        }

        public override PixelInternalFormat PixelInternalFormat
        {
            get { return this.pixelInternalFormat; }
        }
        public override PixelType PixelType
        {
            get { return this.pixelType; }
        }
        #endregion
    }
}
