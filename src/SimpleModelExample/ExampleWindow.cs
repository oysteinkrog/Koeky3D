using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using MilkModelLoader;
using System.Windows.Forms;
using Koeky3D.BufferHandling;
using OpenTK.Graphics.OpenGL;
using Koeky3D.Textures;
using MilkModelLoader.MilkshapeData;
using Koeky3D;
using Koeky3D.Shaders;

namespace SimpleModelExample
{
    class ExampleWindow : GameWindow
    {
        private Model model;
        private GLManager glManager;
        private RenderOptions renderOptions;
        private DefaultTechnique technique;

        protected override void OnResize(EventArgs e)
        {
            this.renderOptions.Resolution = base.Size;
        }

        protected override void OnLoad(EventArgs e)
        {
            this.renderOptions = new RenderOptions(base.Width, base.Height, base.WindowState, base.VSync, 45.0f, 1.0f, 1000.0f);
            this.glManager = new GLManager(this.renderOptions);

            // Load the milkshape model
            MilkshapeLoader loader = new MilkshapeLoader();
            MilkshapeModel milkModel;
            MilkshapeLoadResult result = loader.LoadModel("Data/Models/dwarf.ms3d", out milkModel);
            if (result != MilkshapeLoadResult.ModelLoaded)
            {
                // Model failed to load, show a message and quit
                MessageBox.Show("Error: " + result.ToString());
                Environment.Exit(0);
            }

            // We now need to convert the milkshape model data to Koeky3D data
            // I have created the Model class in this project to contain all the data
            Matrix4 modelTransform = Matrix4.Identity;
            
            // Create the vertex buffer
            VertexBuffer verticesBuffer = new VertexBuffer(BufferUsageHint.StaticDraw, (int)BufferAttribute.Vertex, milkModel.vertices);
            VertexBuffer texCoordBuffer = new VertexBuffer(BufferUsageHint.StaticDraw, (int)BufferAttribute.TexCoord, milkModel.texCoords);
            VertexBuffer normalBuffer = new VertexBuffer(BufferUsageHint.StaticDraw, (int)BufferAttribute.Normal, milkModel.normals);
            // Create the vertex array
            VertexArray vertexArray = new VertexArray(verticesBuffer, texCoordBuffer, normalBuffer);

            // Create the index buffers, for every triangle mesh one
            IndexBuffer[] indexBuffers = new IndexBuffer[milkModel.meshes.Length];
            Texture2D[] textures = new Texture2D[milkModel.meshes.Length];

            for (int i = 0; i < milkModel.meshes.Length; i++)
            {
                MilkshapeMesh mesh = milkModel.meshes[i];
                indexBuffers[i] = new IndexBuffer(BufferUsageHint.StaticDraw, mesh.indices);
                textures[i] = TextureConstructor.ConstructTexture2D(mesh.material.texturePath);
            }
            // Create the model
            this.model = new Model(modelTransform, vertexArray, indexBuffers, textures);

            // Create the render technique
            this.technique = new DefaultTechnique();
            if (!this.technique.Initialise())
                MessageBox.Show("Failed to init technique: " + this.technique.ErrorMessage);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // Rotate the model
            this.model.world *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(1.0f));
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // Clear screen
            this.glManager.ClearScreen(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            // Bind technique
            this.glManager.BindTechnique(this.technique);
            this.technique.UseTexture = true;
            this.glManager.Projection = this.renderOptions.Projection;
            this.glManager.View = Matrix4.CreateTranslation(0.0f, -30.0f, -100.0f);

            // Draw the model
            this.model.Draw(this.glManager);

            base.SwapBuffers();
        }
    }
}
