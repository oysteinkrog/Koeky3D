using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Koeky3D.Textures;
using Koeky3D.BufferHandling;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Koeky3D.Shaders;
using Koeky3D.Properties;
using System.Runtime.InteropServices;
using Koeky3D.Utilities;
using GLFrameWork.Shapes;
using OpenTK.Graphics;
using Koeky3D.Shapes;

namespace Koeky3D.Particles
{
    public class ParticleEmitter
    {
        private VertexArray[] vertexArrays;
        private VertexBuffer[] vertexBuffers;
        private int currentVB = 0;
        private int currentTFB = 1;

        private static ParticleUpdateTechnique updateTechnique;
        private static ParticleRenderTechnique renderTechnique;

        public Vector3 position;
        public Vector3 gravity = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 wind = new Vector3(0.0f, 0.0f, 0.0f);
        public float maxLifeTime = 1.0f;
        public float startScale = 1.0f;
        public float deltaScale = 0.0f;
        public Vector3 minVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 maxVelocity = new Vector3(0.0f, 100.0f, 0.0f);

        private float totalTime = 0.0f;
        private float spawnPerSecond;
        private int maxParticles;
        private Texture2D texture;
        private int textureCount;

        private static Texture1D randomTexture;
        private static Random rand = new Random();
        private static FrameBuffer drawBuffer;
        private static DefaultTechnique defaultTechnique;
        private static int drawBufferWidth = 800, drawBufferHeight = 600;

        public ParticleEmitter(int maxParticles, Texture2D texture, int textureCount, Vector3 position, float spawnPerSecond)
        {
            this.maxParticles = maxParticles;
            this.texture = texture;
            this.textureCount = textureCount;
            this.position = position;
            this.spawnPerSecond = spawnPerSecond;
        }

        public void Initialise()
        {
            if (updateTechnique == null)
            {
                // Create the techniques
                updateTechnique = new ParticleUpdateTechnique();
                renderTechnique = new ParticleRenderTechnique();

                if (!updateTechnique.Initialise())
                {
                    Console.WriteLine("Update technique not initialised! " + updateTechnique.ErrorMessage);
                    Console.ReadLine();
                }
                if (!renderTechnique.Initialise())
                {
                    Console.WriteLine("Render technique not initialised! " + renderTechnique.ErrorMessage);
                    Console.ReadLine();
                }

                // Create the random texture
                int samples = 1024;
                Vector3[] randomSamples = new Vector3[samples];
                for (int i = 0; i < samples; i++)
                {
                    randomSamples[i].X = (float)rand.NextDouble();
                    randomSamples[i].Y = (float)rand.NextDouble();
                    randomSamples[i].Z = (float)rand.NextDouble();
                }

                randomTexture = new Texture1D(PixelInternalFormat.Rgb, PixelType.Float);
                randomTexture.BindTexture(TextureUnit.Texture0);
                randomTexture.SetParameter(TextureParameterName.TextureWrapS, (float)All.Repeat);
                randomTexture.SetParameter(TextureParameterName.TextureMinFilter, (float)All.Linear);
                randomTexture.SetParameter(TextureParameterName.TextureMagFilter, (float)All.Linear);
                unsafe
                {
                    fixed (void* ptr = randomSamples)
                    {
                        IntPtr scan0 = (IntPtr)ptr;
                        randomTexture.SetData(scan0, PixelFormat.Rgb, samples);
                    }
                }

                drawBuffer = new FrameBuffer(drawBufferWidth, drawBufferHeight, 1, true);
                defaultTechnique = new DefaultTechnique();
                if (!defaultTechnique.Initialise())
                    Console.WriteLine("ERROR! " + defaultTechnique.ErrorMessage);
            }

            // Create the start data
            ParticleVertex[] particles = new ParticleVertex[maxParticles];
            for (int i = 0; i < maxParticles; i++)
            {
                // Create the positions, velocities and max life times
                Vector3 velocity = new Vector3((float)rand.NextDouble() * (minVelocity.X + maxVelocity.X) - minVelocity.X,
                                               (float)rand.NextDouble() * (minVelocity.Y + maxVelocity.Y) - minVelocity.Y,
                                               (float)rand.NextDouble() * (minVelocity.Z + maxVelocity.Z) - minVelocity.Z);
                Vector4 textureAlphas = Vector4.Zero;
                switch (rand.Next(textureCount))
                {
                    case (0):
                        textureAlphas = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
                        break;
                    case (1):
                        textureAlphas = new Vector4(0.0f, 1.0f, 0.0f, 0.0f);
                        break;
                    case (2):
                        textureAlphas = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
                        break;
                    case (3):
                        textureAlphas = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                        break;
                }

                particles[i].position = position;
                particles[i].velocity = velocity;
                particles[i].lifeTime = -i / spawnPerSecond;
                particles[i].textureAlphas = textureAlphas;
                particles[i].scale = this.startScale;
            }

            // Upload the start data
            int structSize = Marshal.SizeOf(typeof(ParticleVertex));
            BufferElement[] elements = new BufferElement[5]
            {
                new BufferElement((int)BufferAttribute.Vertex, VertexAttribPointerType.Float, 3, structSize, 0),
                new BufferElement((int)BufferAttribute.ParticleVelocity, VertexAttribPointerType.Float, 3, structSize, sizeof(float) * 3),
                new BufferElement((int)BufferAttribute.ParticleTextureAlpha, VertexAttribPointerType.Float, 4, structSize, sizeof(float) * 6),
                new BufferElement((int)BufferAttribute.ParticleScale, VertexAttribPointerType.Float, 1, structSize, sizeof(float) * 10),
                new BufferElement((int)BufferAttribute.ParticleLifeTime, VertexAttribPointerType.Float, 1, structSize, sizeof(float) * 11),
            };

            // Create buffers
            this.vertexBuffers = new VertexBuffer[2];
            this.vertexArrays = new VertexArray[2];
            for (int i = 0; i < 2; i++)
            {
                // Create the vertex buffer and allocate space for the particles
                VertexBuffer vertexBuffer = new VertexBuffer(BufferUsageHint.DynamicDraw);
                vertexBuffer.SetBufferLayout(elements);
                vertexBuffer.SetData<ParticleVertex>(particles);

                // Create the vertex array and link the vertex buffer
                VertexArray vertexArray = new VertexArray(vertexBuffer);

                this.vertexBuffers[i] = vertexBuffer;
                this.vertexArrays[i] = vertexArray;
            }
        }

        public void Update(float timeStep, GLManager glManager)
        {
            this.totalTime += timeStep;

            glManager.PushRenderState();

            glManager.DiscardRasterizer = true;

            VertexBuffer currentVertexBuffer = this.vertexBuffers[this.currentVB];
            VertexArray currentVertexArray = this.vertexArrays[this.currentVB];

            // Bind update technique and set variables
            glManager.BindTechnique(updateTechnique);
            updateTechnique.EmitterPosition = this.position;
            updateTechnique.Gravity = this.gravity;
            updateTechnique.Wind = this.wind;
            updateTechnique.DeltaTime = timeStep;
            updateTechnique.Time = this.totalTime;
            updateTechnique.MaxLifeTime = this.maxLifeTime;
            updateTechnique.ScaleStart = this.startScale;
            updateTechnique.ScaleDelta = this.deltaScale;
            updateTechnique.MinVelocity = this.minVelocity;
            updateTechnique.MaxVelocity = this.maxVelocity;

            randomTexture.BindTexture(TextureUnit.Texture0);

            currentVertexArray.Bind();

            GL.BindBufferBase(BufferTarget.TransformFeedbackBuffer, 0, this.vertexBuffers[this.currentTFB].BufferId);

            GL.BeginTransformFeedback(BeginFeedbackMode.Points);

            GL.DrawArrays(BeginMode.Points, 0, this.maxParticles);

            GL.EndTransformFeedback();

            currentVertexArray.Unbind();

            glManager.PopRenderState();

            this.currentVB = this.currentTFB;
            this.currentTFB = (this.currentTFB + 1) % 2;
        }

        public void Draw(ViewFrustum view, GLManager glManager, RenderOptions options)
        {
            glManager.PushRenderState();
            glManager.BlendingEnabled = true;
            glManager.BlendingSource = BlendingFactorSrc.One;
            glManager.BlendingDestination = BlendingFactorDest.One;
            glManager.WriteDepth = false;
            //glManager.DepthTestEnabled = false;
    
            glManager.BindTechnique(renderTechnique);
            glManager.View = view.transform;
            //glManager.Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(options.FOV),
            //                                                             drawBufferWidth / (float)drawBufferHeight,
            //                                                             options.ZNear,
            //                                                             options.ZFar);
            //GL.Viewport(0, 0, drawBufferWidth, drawBufferHeight);

            renderTechnique.TextureCount = this.textureCount;
            renderTechnique.MaxLifeTime = this.maxLifeTime;

            //glManager.PushFrameBuffer(drawBuffer);
            //glManager.ClearColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
            //glManager.ClearScreen(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            glManager.BindTexture(this.texture, TextureUnit.Texture0);

            glManager.BindVertexArray(this.vertexArrays[this.currentVB]);

            glManager.DrawElements(BeginMode.Points, 0, this.maxParticles);

            glManager.BindVertexArray(null);

            //glManager.PopFrameBuffer();
            //GL.Viewport(0, 0, options.Width, options.Height);

            // Draw the frame buffer screen to the final screen
            //glManager.BindTechnique(defaultTechnique);
            //defaultTechnique.UseTexture = true;
            //glManager.WriteDepth = false;
            //glManager.BlendingEnabled = true;
            //glManager.BlendingSource = BlendingFactorSrc.SrcAlpha;
            //glManager.BlendingDestination = BlendingFactorDest.OneMinusSrcAlpha;
            //ShapeDrawer.Begin(glManager);

            //glManager.Projection = options.Ortho;
            //glManager.View = Matrix4.Identity;

            //glManager.BindTexture(drawBuffer[0], TextureUnit.Texture0);

            //ShapeDrawer.DrawQuad(glManager, Vector2.Zero, new Vector2(options.Width, options.Height));

            //ShapeDrawer.End(glManager);

            glManager.PopRenderState();
        }

        public void ClearResources()
        {
            // TODO
        }
    }
}
