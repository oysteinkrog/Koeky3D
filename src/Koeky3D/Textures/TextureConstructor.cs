using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Koeky3D.Textures
{
    /// <summary>
    /// Provides easy loading of 2d texture from disk.
    /// </summary>
    public static class TextureConstructor
    {
        private static Texture2D defaultTexture2D;
        private static Dictionary<String, Texture2D> constructedTextures2D = new Dictionary<String, Texture2D>();

        /// <summary>
        /// Constructs a texture from the image at the given path.
        /// If the texture is already loaded the cached version will be returned.
        /// If the texture could not be loaded the default texture will be returned.
        /// </summary>
        /// <param name="path">The path to the image to load</param>
        /// <returns></returns>
        public static Texture2D ConstructTexture2D(String path)
        {
            Texture2D texture;
            if (constructedTextures2D.TryGetValue(path, out texture))
                return texture;

            try
            {
                Bitmap bitmap = new Bitmap(path);

                texture = new Texture2D(bitmap, true);

                constructedTextures2D.Add(path, texture);

                bitmap.Dispose();

                return texture;
            }
            catch
            {
                // failed to load texture, return default texture
                return DefaultTexture2D;
            }
        }

        /// <summary>
        /// The default texture to use when a texture could not be loaded.
        /// This is a 1x1 purple texture.
        /// </summary>
        public static Texture2D DefaultTexture2D
        {
            get
            {
                if (defaultTexture2D == null)
                {
                    Bitmap bitmap = new Bitmap(1, 1);
                    bitmap.SetPixel(0, 0, Color.Purple);

                    defaultTexture2D = new Texture2D(bitmap, true);

                    bitmap.Dispose();
                }

                return defaultTexture2D;
            }
        }
    }
}
