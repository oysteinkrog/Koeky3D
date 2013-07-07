using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Koeky3D.BufferHandling;
using OpenTK.Graphics;
using Koeky3D;
using Koeky3D.Textures;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Windows.Forms;
using Koeky3D.Shaders;
using GLFrameWork.Shapes;
using Koeky3D.Utilities;

namespace FramebufferExample
{
    class ExampleWindow : GameWindow
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
        private Matrix4 mainCubeTransform;
        private Matrix4 secCubeTransform;
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

        /// <summary>
        /// The frame buffer to draw the secondary cube to
        /// </summary>
        private FrameBuffer frameBuffer;

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

        public ExampleWindow()
            : base(1024, 800)
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
            this.renderOptions = new RenderOptions(base.Width, base.Height, base.WindowState, base.VSync);
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

            // Create the frame buffer. This is a default frame buffer with one color texture and a depth component
            // The frame buffer has a low resolution, this is why it look so pixalated on the screen
            this.frameBuffer = new FrameBuffer(200, 200, 1, true);

            // Create the render technique and initialise it
            this.technique = new RenderTechnique();
            if (!this.technique.Initialise())
                MessageBox.Show(this.technique.ErrorMessage);

            // Create the cube transform
            this.mainCubeTransform = Matrix4.Identity;
            this.secCubeTransform = Matrix4.Identity;
            // Create the camera transform
            this.cameraTransform = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(35.0f)) * Matrix4.CreateTranslation(0.0f, 0.0f, -10.0f);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // Rotate the cube
            this.mainCubeTransform *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(2.0f));
            this.secCubeTransform *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-2.0f));

            // If enter is pressed: turn wireframe mode on
            this.glManager.PolygonMode = base.Keyboard[Key.Enter] ? PolygonMode.Line : PolygonMode.Fill;

            // Set the title to show the fps
            base.Title = ((int)base.RenderFrequency).ToString();
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // By calling PushRenderState we save the OpenGL settings exposed by the GLManager class
            this.glManager.PushRenderState();

            // Clear the screen using a red color
            this.glManager.ClearColor = Color4.Red;
            this.glManager.ClearScreen(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Bind the current render technique
            this.glManager.BindTechnique(this.technique);
            this.technique.EnableLighting = true;

            // Render the cube. This cube will be seen once we call SwapBuffers
            RenderCube(this.renderOptions.Projection, this.mainCubeTransform);

            // Bind the frame buffer. frame buffers are placed on a stack. 
            // If I call PushFrameBuffer three times I also have to call PopFrameBuffer three times
            // before I draw on the default OpenGL frame buffer again.
            this.glManager.PushFrameBuffer(this.frameBuffer);

            // Clear the screen using a blue color
            this.glManager.ClearColor = Color4.Blue;
            this.glManager.ClearScreen(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Render the cube. We are now drawing on our own frame buffer
            RenderCube(Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), 1.0f, 0.1f, 1000.0f), this.secCubeTransform);

            // Restore the previous frame buffer
            this.glManager.PopFrameBuffer();

            // Lastly we render the color texture of the frame buffer to the main screen
            this.glManager.BindTexture(this.frameBuffer.PrimaryTexture, TextureUnit.Texture0);
            // Use a orthographic projection
            this.glManager.Projection = this.renderOptions.Ortho;
            // Set the view to the identity.
            this.glManager.View = Matrix4.Identity;
            // Lighting is enabled, we wouldn't want to light a 2d quad.
            this.technique.EnableLighting = false;

            // We can use the ShapeDrawer class to easily draw default shapes. 
            // Not all shapes are implemented yet tough.
            ShapeDrawer.Begin(this.glManager);

            // We draw a quad at the top left corner with the framebuffer's texture
            ShapeDrawer.DrawQuad(this.glManager, new Vector2(10, 50), new Vector2(400, 400));

            ShapeDrawer.End(this.glManager);

            // Restore the settings saved by calling PushRenderState at the start of this method
            this.glManager.PopRenderState();

            // Display the image to the user
            base.SwapBuffers();
        }

        private void RenderCube(Matrix4 projection, Matrix4 world)
        {
            // Set the camera transform, cube transform and projection
            this.glManager.View = this.cameraTransform;
            this.glManager.World = world;
            this.glManager.Projection = projection;

            // Bind the required objects.
            this.glManager.BindTexture(this.texture, TextureUnit.Texture0);
            this.glManager.BindVertexArray(this.vertexArray);

            // Draw the data
            this.glManager.DrawElements(BeginMode.Quads, 0, 6 * 4);
        }
    }
}
