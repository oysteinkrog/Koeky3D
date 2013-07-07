using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Koeky3D;
using MilkModelLoader;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using Koeky3D.BufferHandling;
using Koeky3D.Textures;
using MilkModelLoader.MilkshapeData;
using SimpleModelExample;
using Koeky3D.Models;
using Koeky3D.Animation;
using Koeky3D.Utilities;

namespace AnimationExample
{
    class ExampleWindow : GameWindow
    {
        private GLManager glManager;
        private RenderOptions renderOptions;
        private Model model;
        private ModelTechnique modelTechnique;

        private Skeleton skeleton;
        private Animation animation;

        protected override void OnLoad(EventArgs e)
        {
            // Create the render options class and the gl manager
            this.renderOptions = new RenderOptions(base.Width, base.Height, base.WindowState, base.VSync);
            this.glManager = new GLManager(this.renderOptions);

            // Adjust the far plane to properly show the model
            this.renderOptions.ZFar = 1000.0f;

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

            // We must now convert the milkshape model to something we can use with OpenGL.
            // Unlike the previous example (SimpleModelExample) we must now also upload the bones per vertex.
            // To store the opengl data I have reused the Model class from the SimpleModelExample project.
            // I have not included the boneweights, to keep things simple.

            // Create the vertex buffer
            VertexBuffer verticesBuffer = new VertexBuffer(BufferUsageHint.StaticDraw, (int)BufferAttribute.Vertex, milkModel.vertices);
            VertexBuffer texCoordBuffer = new VertexBuffer(BufferUsageHint.StaticDraw, (int)BufferAttribute.TexCoord, milkModel.texCoords);
            VertexBuffer normalBuffer = new VertexBuffer(BufferUsageHint.StaticDraw, (int)BufferAttribute.Normal, milkModel.normals);

            // The interesting part: here we create a buffer to keep track which vertex is linked to which bone.
            VertexBuffer boneBuffer = new VertexBuffer(BufferUsageHint.StaticDraw, (int)BufferAttribute.BoneIndex, milkModel.boneIndices);

            // Create the vertex array
            VertexArray vertexArray = new VertexArray(verticesBuffer, texCoordBuffer, normalBuffer, boneBuffer);

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
            this.model = new Model(Matrix4.Identity, vertexArray, indexBuffers, textures);

            // Create the render technique. I use the default provided class ModelTechnique, it allows for skeletal animations.
            this.modelTechnique = new ModelTechnique("Data/Shaders/vertexShaderSkinned.txt", "Data/Shaders/fragmentShader.txt", "", true);
            if (!this.modelTechnique.Initialise())
            {
                // Shader failed to compile, show a message and quit.
                MessageBox.Show(this.modelTechnique.ErrorMessage);
                Environment.Exit(0);
            }

            // Lastly we need to create the skeleton of the dwarf and an animation to play
            // The milkshape model stores all bones in a MilkshapeBone class. We need to convert that to the Bone class which the Koeky 3D Framework can use.
            Bone[] bones = new Bone[milkModel.bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                MilkshapeBone milkBone = milkModel.bones[i];

                int index = milkBone.index;
                int parentIndex = milkBone.parentBone == null ? -1 : milkBone.parentBone.index;
                Matrix4 localMatrix = GLConversion.CreateTransformMatrix(milkBone.initPosition, milkBone.initRotation);

                bones[i] = new Bone(index, parentIndex, localMatrix);
            }
            // With all bones available we can create the Skeleton of the dwarf
            this.skeleton = new Skeleton(bones);

            // Skeletons and animations are seperate in the Koeky 3D framework. So now we need to create the animation as well.
            // For this we need to create an array with BoneKeyFrame classes. A BoneKeyFrame object contains all keyframes for one bone.
            BoneKeyframes[] animationKeyFrames = new BoneKeyframes[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                MilkshapeBone milkBone = milkModel.bones[i];

                // Create all key frames which translate a bone
                KeyframePosition[] translationKeyFrames = new KeyframePosition[milkBone.translations.Length];
                for (int j = 0; j < translationKeyFrames.Length; j++)
                {
                    MilkshapeMovement translation = milkBone.translations[j];

                    translationKeyFrames[j] = new KeyframePosition(translation.time, translation.movement.X, translation.movement.Y, translation.movement.Z);
                }

                // Create all key frames which rotate a bone
                KeyframeRotation[] rotationKeyFrames = new KeyframeRotation[milkBone.rotations.Length];
                for (int j = 0; j < rotationKeyFrames.Length; j++)
                {
                    MilkshapeMovement rotation = milkBone.rotations[j];

                    rotationKeyFrames[j] = new KeyframeRotation(rotation.time, rotation.movement.X, 
                                                                               rotation.movement.Y, 
                                                                               rotation.movement.Z);
                }

                // Store the translation and rotation key frames in a BoneKeyFrames object
                animationKeyFrames[i] = new BoneKeyframes(translationKeyFrames, rotationKeyFrames);
                animationKeyFrames[i].weight = 1.0f;
            }
            // Finally create the animation
            this.animation = new Animation(animationKeyFrames);

            // We now got a skeleton and an animation to animate it with.
            // Now all we need to do is start the animation
            this.skeleton.StartAnimation(this.animation, AnimationType.ForwardLoop, 2.0f);
        }
        protected override void OnResize(EventArgs e)
        {
            this.renderOptions.Resolution = base.Size;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            this.model.world *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(1.0f));
            this.skeleton.Update((float)e.Time);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            this.glManager.ClearScreen(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            // Bind the model technique
            this.glManager.BindTechnique(this.modelTechnique);

            // Uploads the joints (or bones) transform to the shader.
            // This is an array with 4x4 matrices containing one transform per bone.
            this.modelTechnique.Joints = this.skeleton.finalJointMatrices;

            // Uploads the inverse of every absolute transform. Before multiplying a vertex by it's bone transform we must first center it around zero again.
            // We do this by multiplying with the inverse of the default transform of the bone.
            // We could also do this transformation while loading the model, this may speed things up since we would not need to upload them to the shader anymore.
            this.modelTechnique.InvJoints = this.skeleton.invJointMatrices;

            this.glManager.Projection = this.renderOptions.Projection;
            this.glManager.View = Matrix4.CreateTranslation(0.0f, -30.0f, -150.0f);

            // Render the model
            this.model.Draw(this.glManager);

            base.SwapBuffers();
        }
    }
}
