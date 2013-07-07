using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GLFramework;
using GLFrameWork.Shapes;
using GLFramework.Models;
using GLFramework.Lighting;
using GLFramework.BufferHandling;
using GLFramework.ShaderUtilities;
using GLFramework.Shapes;
using GLFramework.Textures;
using GLFramework.Utilities;

namespace GLFramework
{
    /// <summary>
    /// Provides basic render functionality using OpenGL
    /// </summary>
    public class GLRenderer
    {
        #region constants
        private const int COLORBUFFER_COUNT = 4;

        private const int COLORBUFFER_AMBIENT = 0;
        private const int COLORBUFFER_DIFFUSE = 1;
        private const int COLORBUFFER_TEXTURE = 2;
        private const int COLORBUFFER_POSITION = 3;
        private const int DEPTHBUFFER = 4;
        #endregion

        #region variables
        private RenderOptions renderOptions;

        private ShaderProgram gBufferStaticShader;
        private ShaderProgram gBufferSkinningShader;
        private ShaderProgram pointLightShader;
        private ShaderProgram spotLightShader;
        private ShaderProgram defaultShader;
        private ShaderProgram colorShader;
        private ShaderProgram testDepthShader;
        private ShaderProgram staticShadowShader;
        private ShaderProgram skinnedShadowShader;

        private FrameBuffer gBuffer;
        private FrameBuffer shadowBuffer;

        private int shadowResX = 800;
        private int shadowResY = 600;
        #endregion

        #region constructors
        public GLRenderer(RenderOptions renderOptions)
        {
            this.renderOptions = renderOptions;

            this.renderOptions.OnResolutionChange += ResolutionChanged;

            // create the gBuffer shader
            this.gBufferStaticShader = new ShaderProgram();
            if (!this.gBufferStaticShader.AddShader("Resources/Shaders/GBuffer_VertexShader.txt", OpenTK.Graphics.OpenGL.ShaderType.VertexShader))
            {
                Console.WriteLine(this.gBufferStaticShader.GetLastLog());
            }
            if (!this.gBufferStaticShader.AddShader("Resources/Shaders/GBuffer_FragmentShader.txt", OpenTK.Graphics.OpenGL.ShaderType.FragmentShader))
            {
                Console.WriteLine(this.gBufferStaticShader.GetLastLog());
            }

            // create a skinning gbuffer shader
            this.gBufferSkinningShader = new ShaderProgram();
            if (!this.gBufferSkinningShader.AddShader("Resources/Shaders/GBuffer_SkinnedVertexShader.txt", OpenTK.Graphics.OpenGL.ShaderType.VertexShader))
            {
                Console.WriteLine(this.gBufferSkinningShader.GetLastLog());
            }
            if (!this.gBufferSkinningShader.AddShader("Resources/Shaders/GBuffer_FragmentShader.txt", OpenTK.Graphics.OpenGL.ShaderType.FragmentShader))
            {
                Console.WriteLine(this.gBufferSkinningShader.GetLastLog());
            }

            // create a default shader
            this.defaultShader = new ShaderProgram();
            if (!this.defaultShader.AddShader("Resources/Shaders/DefaultVertexShader.txt", OpenTK.Graphics.OpenGL.ShaderType.VertexShader))
            {
                Console.WriteLine(this.defaultShader.GetLastLog());
            }
            if (!this.defaultShader.AddShader("Resources/Shaders/DefaultFragmentShader.txt", OpenTK.Graphics.OpenGL.ShaderType.FragmentShader))
            {
                Console.WriteLine(this.defaultShader.GetLastLog());
            }

            // create a point light shader
            this.pointLightShader = new ShaderProgram();
            if (!this.pointLightShader.AddShader("Resources/Shaders/PointLight_VertexShader.txt", OpenTK.Graphics.OpenGL.ShaderType.VertexShader))
            {
                Console.WriteLine(this.pointLightShader.GetLastLog());
            }
            if (!this.pointLightShader.AddShader("Resources/Shaders/PointLight_FragmentShader.txt", OpenTK.Graphics.OpenGL.ShaderType.FragmentShader))
            {
                Console.WriteLine(this.pointLightShader.GetLastLog());
            }

            // create a spot light shader
            this.spotLightShader = new ShaderProgram();
            if (!this.spotLightShader.AddShader("Resources/Shaders/PointLight_VertexShader.txt", OpenTK.Graphics.OpenGL.ShaderType.VertexShader))
            {
                Console.WriteLine(this.spotLightShader.GetLastLog());
            }
            if (!this.spotLightShader.AddShader("Resources/Shaders/SpotLight_FragmentShader.txt", OpenTK.Graphics.OpenGL.ShaderType.FragmentShader))
            {
                Console.WriteLine(this.spotLightShader.GetLastLog());
            }

            // create a color shader
            this.colorShader = new ShaderProgram();
            if (!this.colorShader.AddShader("Resources/Shaders/DefaultVertexShader.txt", OpenTK.Graphics.OpenGL.ShaderType.VertexShader))
            {
                Console.WriteLine(this.colorShader.GetLastLog());
            }
            if (!this.colorShader.AddShader("Resources/Shaders/ColorFragmentShader.txt", OpenTK.Graphics.OpenGL.ShaderType.FragmentShader))
            {
                Console.WriteLine(this.colorShader.GetLastLog());
            }

            // create a test depth render shader
            this.testDepthShader = new ShaderProgram();
            if (!this.testDepthShader.AddShader("Resources/Shaders/DefaultVertexShader.txt", OpenTK.Graphics.OpenGL.ShaderType.VertexShader))
            {
                Console.WriteLine(this.testDepthShader.GetLastLog());
            }
            if (!this.testDepthShader.AddShader("Resources/Shaders/DepthFragmentShader.txt", OpenTK.Graphics.OpenGL.ShaderType.FragmentShader))
            {
                Console.WriteLine(this.testDepthShader.GetLastLog());
            }

            // create the static shadow shader
            this.staticShadowShader = new ShaderProgram();
            if (!this.staticShadowShader.AddShader("Resources/Shaders/Shadow_VertexShader.txt", OpenTK.Graphics.OpenGL.ShaderType.VertexShader))
            {
                Console.WriteLine(this.staticShadowShader.GetLastLog());
            }
            if (!this.staticShadowShader.AddShader("Resources/Shaders/Shadow_FragmentShader.txt", OpenTK.Graphics.OpenGL.ShaderType.FragmentShader))
            {
                Console.WriteLine(this.staticShadowShader.GetLastLog());
            }

            // create the skinned shadow shader
            this.skinnedShadowShader = new ShaderProgram();
            if (!this.skinnedShadowShader.AddShader("Resources/Shaders/Shadow_SkinnedVertexShader.txt", OpenTK.Graphics.OpenGL.ShaderType.VertexShader))
            {
                Console.WriteLine(this.skinnedShadowShader.GetLastLog());
            }
            if (!this.skinnedShadowShader.AddShader("Resources/Shaders/Shadow_FragmentShader.txt", OpenTK.Graphics.OpenGL.ShaderType.FragmentShader))
            {
                Console.WriteLine(this.skinnedShadowShader.GetLastLog());
            }

            // create the framebuffer which holds the gBuffer
            Texture2D[] gBufferTextures = new Texture2D[4]
            {
                new Texture2D(PixelInternalFormat.Rgba8, PixelType.UnsignedByte),
                new Texture2D(PixelInternalFormat.Rgba8, PixelType.UnsignedByte),
                new Texture2D(PixelInternalFormat.Rgba8, PixelType.UnsignedByte),
                new Texture2D(PixelInternalFormat.Rgba16f, PixelType.Float)
            };
            Texture2D gBufferDepthTexture = new Texture2D(PixelInternalFormat.DepthComponent32, PixelType.Float);
            this.gBuffer = new FrameBuffer(this.renderOptions.Width, this.renderOptions.Height, gBufferTextures, gBufferDepthTexture);

            this.shadowBuffer = new FrameBuffer(this.shadowResX, this.shadowResY, 1, true);
        }
        #endregion

        #region event handling
        private void ResolutionChanged(object sender, EventArgs e)
        {
            this.gBuffer.Resize(this.renderOptions.Width, this.renderOptions.Height);
        }
        #endregion

        #region GLRenderer methods
        public void DrawWorld(IGraphicsWorld graphicsWorld, GLManager glManager, ViewFrustum viewFrustum)
        {
            this.DrawWorld(graphicsWorld, glManager, viewFrustum, false, false, false);
        }
        public void DrawWorld(IGraphicsWorld graphicsWorld, GLManager glManager, ViewFrustum viewFrustum, bool debugDrawLights, bool drawDefShadeDebug, bool drawWireFrame)
        {
            // save renderstate
            glManager.PushRenderState();

            if (drawWireFrame)
                glManager.PolygonMode = PolygonMode.Line;

            // bind gBuffer
            glManager.PushFrameBuffer(this.gBuffer);

            // clear screen
            glManager.ClearScreen(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // set projection and view
            glManager.Projection = this.renderOptions.Projection;
            glManager.View = viewFrustum.transform;

            // draw every static model
            glManager.BindShader(this.gBufferStaticShader);
            List<IStaticTransform> statics = graphicsWorld.GetStaticTransforms(viewFrustum);
            foreach (IStaticTransform staticTransform in statics)
            {
                //staticTransform.Model.Draw(staticTransform.ModelTransform, glManager);
            }

            // draw every skinned model
            glManager.BindShader(this.gBufferSkinningShader);
            List<ISkinnedTransform> skinned = graphicsWorld.GetSkinnedTransform(viewFrustum);
            foreach (ISkinnedTransform skinnedTransform in skinned)
            {
                //skinnedTransform.Model.Draw(skinnedTransform.ModelTransform, glManager, skinnedTransform.AnimatedSkeleton);
            }

            // unbind gBuffer
            glManager.PopFrameBuffer();

            if (drawWireFrame)
                glManager.PolygonMode = PolygonMode.Fill;

            // set lighting parameters
            glManager.CullFaceMode = CullFaceMode.Front;
            glManager.DepthTestEnabled = false;
            glManager.BlendingEnabled = true;
            glManager.BlendingSource = BlendingFactorSrc.One;
            glManager.BlendingDestination = BlendingFactorDest.One;

            // draw point lights
            DrawPointLights(graphicsWorld, glManager, viewFrustum, debugDrawLights);
            // draw spot lights
            DrawSpotLights(graphicsWorld, glManager, viewFrustum, debugDrawLights);

            // reset lighting paramaters
            glManager.DepthTestEnabled = true;
            glManager.CullFaceMode = CullFaceMode.Back;
            glManager.BlendingEnabled = false;

            if (drawDefShadeDebug)
            {
                // debug options: draw gBuffer result
                glManager.BindShader(this.defaultShader);
                glManager.Projection = this.renderOptions.Ortho;
                glManager.View = Matrix4.Identity;

                // draw the gBuffer textures
                this.gBuffer.Draw(COLORBUFFER_AMBIENT, glManager, new Vector2(100, 100), 300, 300);
                this.gBuffer.Draw(COLORBUFFER_DIFFUSE, glManager, new Vector2(100, 420), 300, 300);
                this.gBuffer.Draw(COLORBUFFER_TEXTURE, glManager, new Vector2(420, 100), 300, 300);
                this.gBuffer.Draw(COLORBUFFER_POSITION, glManager, new Vector2(420, 420), 300, 300);

                glManager.BindShader(this.testDepthShader);
                this.testDepthShader.SetVariable("zNear", this.renderOptions.ZNear);
                this.testDepthShader.SetVariable("zFar", this.renderOptions.ZFar);
                ShapeDrawer.Begin(glManager);

                glManager.BindTexture(this.gBuffer.DepthTexture, TextureUnit.Texture0);
                ShapeDrawer.DrawQuad(glManager, new Vector2(800, 100), new Vector2(600, 600));

                ShapeDrawer.End(glManager);
            }

            // restore renderstate
            glManager.PopRenderState();
        }

        private void DrawPointLights(IGraphicsWorld graphicsWorld, GLManager glManager, ViewFrustum viewFrustum, bool drawDebug)
        {
            // draw the scene with deferred shading
            glManager.BindShader(this.pointLightShader);
            this.pointLightShader.SetVariable("viewPort", new Vector2(this.renderOptions.Width, this.renderOptions.Height));

            // bind textures
            glManager.BindTexture(gBuffer[COLORBUFFER_AMBIENT], TextureUnit.Texture0 + COLORBUFFER_AMBIENT);
            this.pointLightShader.SetVariable("ambientTexture", COLORBUFFER_AMBIENT);

            glManager.BindTexture(gBuffer[COLORBUFFER_DIFFUSE], TextureUnit.Texture0 + COLORBUFFER_DIFFUSE);
            this.pointLightShader.SetVariable("diffuseTexture", COLORBUFFER_DIFFUSE);

            glManager.BindTexture(gBuffer[COLORBUFFER_POSITION], TextureUnit.Texture0 + COLORBUFFER_POSITION);
            this.pointLightShader.SetVariable("positionTexture", COLORBUFFER_POSITION);

            glManager.BindTexture(gBuffer[COLORBUFFER_TEXTURE], TextureUnit.Texture0 + COLORBUFFER_TEXTURE);
            this.pointLightShader.SetVariable("textureTexture", COLORBUFFER_TEXTURE);

            // draw a sphere for every point light
            List<PointLight> pointLights = graphicsWorld.GetPointLights(viewFrustum);
            ShapeDrawer.Begin(glManager);
            foreach (PointLight pointLight in pointLights)
            {
                pointLight.SetLightParameters(glManager);
                ShapeDrawer.DrawSphere(glManager, pointLight.Position, pointLight.Radius);
            }
            ShapeDrawer.End(glManager);

            if (drawDebug)
            {
                // debug option: draw lights
                glManager.BindShader(this.colorShader);
                glManager.PolygonMode = PolygonMode.Line;
                glManager.CullFaceEnabled = false;

                ShapeDrawer.Begin(glManager);
                foreach (PointLight pointLight in pointLights)
                {
                    this.colorShader.SetVariable("color", pointLight.Diffuse);
                    ShapeDrawer.DrawSphere(glManager, pointLight.Position, pointLight.Radius);
                }
                ShapeDrawer.End(glManager);

                glManager.CullFaceEnabled = true;
                glManager.PolygonMode = PolygonMode.Fill;
            }
        }
        private void DrawSpotLights(IGraphicsWorld graphicsWorld, GLManager glManager, ViewFrustum viewFrustum, bool drawDebug)
        {
            return;
            glManager.BindShader(this.spotLightShader);
            this.spotLightShader.SetVariable("viewPort", new Vector2(this.renderOptions.Width, this.renderOptions.Height));

            // bind textures
            glManager.BindTexture(gBuffer[COLORBUFFER_AMBIENT], TextureUnit.Texture0 + COLORBUFFER_AMBIENT);
            this.spotLightShader.SetVariable("ambientTexture", COLORBUFFER_AMBIENT);

            glManager.BindTexture(gBuffer[COLORBUFFER_DIFFUSE], TextureUnit.Texture0 + COLORBUFFER_DIFFUSE);
            this.spotLightShader.SetVariable("diffuseTexture", COLORBUFFER_DIFFUSE);

            glManager.BindTexture(gBuffer[COLORBUFFER_POSITION], TextureUnit.Texture0 + COLORBUFFER_POSITION);
            this.spotLightShader.SetVariable("positionTexture", COLORBUFFER_POSITION);

            glManager.BindTexture(gBuffer[COLORBUFFER_TEXTURE], TextureUnit.Texture0 + COLORBUFFER_TEXTURE);
            this.spotLightShader.SetVariable("textureTexture", COLORBUFFER_TEXTURE);

            // draw a sphere for every point light
            List<SpotLight> spotLights = graphicsWorld.GetSpotLights(viewFrustum);
            
            foreach (SpotLight spotLight in spotLights)
            {
                // first render the scene from the spot lights POV to the shadow buffer
                glManager.PushFrameBuffer(this.shadowBuffer);

                glManager.ClearScreen(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                ViewFrustum lightFrustum = new ViewFrustum(spotLight.HalfAngle * 2, this.shadowResX / (float)this.shadowResY, 0.1f, 100.0f);
                Vector3 lightRot = new Vector3(spotLight.Orientation.X, spotLight.Orientation.Y, spotLight.Orientation.Z);
                Matrix4 lightRotMat = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(lightRot.Z)) *
                                      Matrix4.CreateRotationY(MathHelper.DegreesToRadians(lightRot.Y)) *
                                      Matrix4.CreateRotationX(MathHelper.DegreesToRadians(lightRot.X));
                lightFrustum.SetTransform(Matrix4.CreateTranslation(-spotLight.Position) * lightRotMat);

                Matrix4 spotLightProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(spotLight.HalfAngle * 2), 
                                                                                   this.shadowResX / (float)this.shadowResY, 0.1f, 
                                                                                   100.0f);

                glManager.Projection = spotLightProjection;
                glManager.View = lightFrustum.transform;
                glManager.DepthTestEnabled = true;

                FillShadowMap(graphicsWorld, glManager, lightFrustum);

                glManager.DepthTestEnabled = false;

                glManager.PopFrameBuffer();

                glManager.Projection = this.renderOptions.Projection;
                glManager.View = viewFrustum.transform;

                glManager.BindShader(this.colorShader);
                glManager.PolygonMode = PolygonMode.Line;
                glManager.CullFaceEnabled = false;

                ShapeDrawer.Begin(glManager);
                this.colorShader.SetVariable("color", spotLight.Diffuse);

                ShapeDrawer.DrawCone(glManager, lightRotMat * Matrix4.CreateTranslation(spotLight.Position));
                ShapeDrawer.End(glManager);

                glManager.CullFaceEnabled = true;
                glManager.PolygonMode = PolygonMode.Fill;

                glManager.BindShader(this.testDepthShader);
                testDepthShader.SetVariable("zNear", 0.1f);
                testDepthShader.SetVariable("zFar", 100.0f);
                glManager.Projection = this.renderOptions.Ortho;
                glManager.View = Matrix4.Identity;
                glManager.CullFaceMode = CullFaceMode.Back;
                glManager.BlendingEnabled = false;

                glManager.BindTexture(this.shadowBuffer.DepthTexture, TextureUnit.Texture0);

                ShapeDrawer.Begin(glManager);
                ShapeDrawer.DrawQuad(glManager, new Vector2(300, 300), new Vector2(300, 300));
                ShapeDrawer.End(glManager);

                glManager.CullFaceMode = CullFaceMode.Front;
                glManager.BlendingEnabled = true;

                // bind spot light shader
                glManager.BindShader(this.spotLightShader);
                glManager.BindTexture(this.shadowBuffer.DepthTexture, TextureUnit.Texture0 + DEPTHBUFFER);
                spotLightShader.SetVariable("shadowMap", DEPTHBUFFER);

                // compute texture matrix
                Matrix4 biasMatrix = new Matrix4(0.5f, 0.0f, 0.0f, 0.5f,
                                                 0.0f, 0.5f, 0.0f, 0.5f,
                                                 0.0f, 0.0f, 0.5f, 0.5f,
                                                 0.0f, 0.0f, 0.0f, 1.0f);
                Matrix4 inverseView = viewFrustum.transform;
                inverseView.Invert();
                Matrix4 texMatrix = biasMatrix * spotLightProjection * lightFrustum.transform * inverseView;

                this.spotLightShader.SetVariable("textureMatrix", texMatrix);

                // reset matrices
                glManager.Projection = this.renderOptions.Projection;
                glManager.View = viewFrustum.transform;

                ShapeDrawer.Begin(glManager);
                spotLight.SetLightParameters(glManager);
                Matrix4 scaleMatrix = GLConversion.CreateScaleMatrix(spotLight.HalfAngle, spotLight.HalfAngle, spotLight.SpotExtend * 2);
                ShapeDrawer.DrawCone(glManager, scaleMatrix * lightRotMat * Matrix4.CreateTranslation(spotLight.Position));
                ShapeDrawer.End(glManager);
            }

            //if (drawDebug)
            //{
                // debug option: draw lights
                //this.glManager.BindShader(this.colorShader);
                //this.glManager.PolygonMode = PolygonMode.Line;
                //this.glManager.CullFaceEnabled = false;

                //ShapeDrawer.Begin(this.glManager);
                //foreach (SpotLight spotLight in spotLights)
                //{
                //    this.colorShader.SetVariable("color", spotLight.Diffuse);

                //    Matrix4 orientation = (Matrix4.CreateRotationX(MathHelper.DegreesToRadians(spotLight.Orientation.X)) *
                //                           Matrix4.CreateRotationY(MathHelper.DegreesToRadians(spotLight.Orientation.Y)));
                //    ShapeDrawer.DrawCone(glManager, spotLight.Position, orientation, new Vector3(spotLight.HalfAngle, spotLight.HalfAngle, spotLight.SpotExtend * 2));
                //}
                //ShapeDrawer.End(this.glManager);

                //this.glManager.CullFaceEnabled = true;
                //this.glManager.PolygonMode = PolygonMode.Fill;
            //}
        }

        private void FillShadowMap(IGraphicsWorld graphicsWorld, GLManager glManager, ViewFrustum frustum)
        {
            // draw every static model
            glManager.BindShader(this.staticShadowShader);
            List<IStaticTransform> statics = graphicsWorld.GetStaticTransforms(frustum);
            foreach (IStaticTransform staticTransform in statics)
            {
                //staticTransform.Model.Draw(staticTransform.ModelTransform, glManager);
            }

            // draw every skinned model
            glManager.BindShader(this.skinnedShadowShader);
            List<ISkinnedTransform> skinned = graphicsWorld.GetSkinnedTransform(frustum);
            foreach (ISkinnedTransform skinnedTransform in skinned)
            {
                //skinnedTransform.Model.Draw(skinnedTransform.ModelTransform, glManager, skinnedTransform.AnimatedSkeleton);
            }
        }
        #endregion
    }
}
