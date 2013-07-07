using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Koeky3D;
using Koeky3D.Shaders;
using System.Windows.Forms;
using Koeky3D.BufferHandling;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace MyFirstTriangle
{
    class ExampleWindow : GameWindow
    {
        private GLManager glManager;
        private RenderOptions renderOptions;
        private DefaultTechnique technique;

        private VertexArray vertexArray;

        protected override void OnLoad(EventArgs e)
        {
            // Create the render options class. This makes it easy to extract a view frustum or projection matrix.
            this.renderOptions = new RenderOptions(base.Width, base.Height, base.WindowState, base.VSync);

            // Create the GLManager class. This makes state changes easier and cleaner.
            this.glManager = new GLManager(this.renderOptions);

            // Set the background color to red
            this.glManager.ClearColor = Color4.Red;

            // Create the render technique, we use the DefaultTechnique class right now.
            // However more often than not you will find that it is easier to implement this class yourself.
            this.technique = new DefaultTechnique();
            if (!this.technique.Initialise())
                MessageBox.Show(this.technique.ErrorMessage);

            // Create a vertex buffer which will contain the vertex position
            Vector3[] vertices = new Vector3[3]
            {
                new Vector3(-1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f)
            };
            VertexBuffer vertexBuffer = new VertexBuffer(BufferUsageHint.StaticDraw, (int)BufferAttribute.Vertex, vertices);

            // Create a vertex array
            this.vertexArray = new VertexArray(vertexBuffer);
        }

        protected override void OnResize(EventArgs e)
        {
            // Notify the RenderOptions class of the change in resolution
            this.renderOptions.Resolution = base.Size;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // clear the screen using the GLManager
            this.glManager.ClearScreen(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Set the projection, view and world matrix
            this.glManager.Projection = this.renderOptions.Projection;
            this.glManager.World = Matrix4.Identity;
            this.glManager.View = Matrix4.CreateTranslation(0.0f, 0.0f, -5.0f);

            // Bind the vertex array and render technique
            this.glManager.BindVertexArray(this.vertexArray);
            this.glManager.BindTechnique(this.technique);

            // Set some render settings in the DefaulTechnique
            this.technique.UseTexture = false;
            this.technique.DrawColor = Color4.Blue;

            // Draw the triangle, again using the GLManager class
            this.glManager.DrawElements(BeginMode.Triangles, 0, 3);

            // display the image to the user
            base.SwapBuffers();
        }
    }
}
