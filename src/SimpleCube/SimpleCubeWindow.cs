using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Koeky3D;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using Koeky3D.BufferHandling;
using Koeky3D.Shaders;
using Koeky3D.Textures;
using System.Windows.Forms;
using OpenTK.Input;

namespace SimpleCube
{
    /// <summary>
    /// A GameWindow which displays a simple rotating cube with a texture
    /// </summary>
    class SimpleCubeWindow : GameWindow
    {
        /// <summary>
        /// The glManager takes care of opengl state changes. It is also used to bind opengl object
        /// </summary>
        private GLManager glManager;
        /// <summary>
        /// Contains some render options
        /// </summary>
        private RenderOptions renderOptions;

        /// <summary>
        /// The world transform of the cube
        /// </summary>
        private Matrix4 cubeTransform;
        /// <summary>
        /// The world transform of the camera
        /// </summary>
        private Matrix4 cameraTransform;

        /// <summary>
        /// The vertex array object which contains the settings for the cube
        /// </summary>
        private VertexArray vertexArray;
        /// <summary>
        /// The texture to use with the cube
        /// </summary>
        private Texture2D texture;
        /// <summary>
        /// The render technique. Contains the shader which renders the cube.
        /// </summary>
        private RenderTechnique technique;

        #region cube data
        private Vector3[] vertices = new Vector3[6 * 4]
        {
            // bottom
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3( 1.0f, -1.0f, -1.0f),
            new Vector3( 1.0f, -1.0f,  1.0f),
            new Vector3(-1.0f, -1.0f,  1.0f),

            // top
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, 1.0f,  1.0f),
            new Vector3( 1.0f, 1.0f,  1.0f),
            new Vector3( 1.0f, 1.0f, -1.0f),

            // right
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),

            // left
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),

            // up
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),

            // down
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
        };
        public Vector2[] texCoords = new Vector2[6 * 4]
        {
            // bottom
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            // top
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            // right
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            // left
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            // up
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            // down
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),
        };
        public Vector3[] normals = new Vector3[6 * 4]
        {
            // bottom
            new Vector3(0.0f, -1.0f, 0.0f),
            new Vector3(0.0f, -1.0f, 0.0f),
            new Vector3(0.0f, -1.0f, 0.0f),
            new Vector3(0.0f, -1.0f, 0.0f),

            // top
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f),

            // right
            new Vector3(1.0f, 0.0f, 0.0f),
            new Vector3(1.0f, 0.0f, 0.0f),
            new Vector3(1.0f, 0.0f, 0.0f),
            new Vector3(1.0f, 0.0f, 0.0f),

            // left
            new Vector3(-1.0f, 0.0f, 0.0f),
            new Vector3(-1.0f, 0.0f, 0.0f),
            new Vector3(-1.0f, 0.0f, 0.0f),
            new Vector3(-1.0f, 0.0f, 0.0f),

            // up
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),

            // down
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(0.0f, 0.0f, -1.0f),
        };
        #endregion

        public SimpleCubeWindow()
            : base(800, 600)
        {
        }

        protected override void OnResize(EventArgs e)
        {
            // Update the render options resolution
            this.renderOptions.Resolution = base.Size;
        }

        protected override void OnLoad(EventArgs e)
        {
            // Initialise the GLFramework managers
            this.renderOptions = new RenderOptions(800, 600, base.WindowState, base.VSync);
            this.glManager = new GLManager(this.renderOptions);

            // Set the background color
            this.glManager.ClearColor = Color4.LightBlue;

            // Create three vertex buffers. One for each data type (vertex, texture coordinate and normal)
            VertexBuffer verticesBuffer = new VertexBuffer(BufferUsageHint.StaticDraw, (int)BufferAttribute.Vertex, this.vertices);
            VertexBuffer texCoordBuffer = new VertexBuffer(BufferUsageHint.StaticDraw, (int)BufferAttribute.TexCoord, this.texCoords);
            VertexBuffer normalBuffer = new VertexBuffer(BufferUsageHint.StaticDraw, (int)BufferAttribute.Normal, this.normals);

            // Create the vertex array which encapsulates the state changes needed to enable the vertex buffers
            this.vertexArray = new VertexArray(verticesBuffer, texCoordBuffer, normalBuffer);

            // Load the texture. We can also create the texture by calling the constructor of Texture2D
            // but by using the TextureConstructor the texture will automatically be cached.
            this.texture = TextureConstructor.ConstructTexture2D("Data/Textures/crate.jpg");

            // Create the render technique and initialise it
            this.technique = new RenderTechnique();
            if (!this.technique.Initialise())
                MessageBox.Show(this.technique.ErrorMessage);

            // Create the cube transform
            this.cubeTransform = Matrix4.Identity;
            // Create the camera transform
            this.cameraTransform = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(35.0f)) * Matrix4.CreateTranslation(0.0f, 0.0f, -10.0f);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // Rotate the cube
            this.cubeTransform *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(2.0f));

            // If enter is pressed: turn wireframe mode on
            this.glManager.PolygonMode = base.Keyboard[Key.Enter] ? PolygonMode.Line : PolygonMode.Fill;

            // Set the title to show the fps
            base.Title = ((int)base.RenderFrequency).ToString();
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            this.glManager.ClearScreen(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Set the camera transform, cube transform and projection
            this.glManager.View = this.cameraTransform;
            this.glManager.World = this.cubeTransform;
            this.glManager.Projection = this.renderOptions.Projection;

            // Bind the required objects.
            this.glManager.BindTechnique(this.technique);
            this.technique.EnableLighting = true;
            this.glManager.BindTexture(this.texture, TextureUnit.Texture0);
            this.glManager.BindVertexArray(this.vertexArray);

            // Draw the data
            this.glManager.DrawElements(BeginMode.Quads, 0, 6 * 4);

            // Display the image to the user
            base.SwapBuffers();
        }
    }
}
