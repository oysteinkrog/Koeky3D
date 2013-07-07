using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Koeky3D.Shapes;
using Koeky3D.Models;
using Koeky3D.BufferHandling;
using Koeky3D.Shaders;
using OpenTK.Graphics.OpenGL;
using Koeky3D.Textures;
using OpenTK;
using GLFrameWork.Shapes;
using Koeky3D.Utilities;

namespace Koeky3D.Pipeline.DefaultStages
{
    /// <summary>
    /// Provides a basic render stage for rendering GLMesh objects.
    /// This render stage currently only supports opaque objects.
    /// Performance is optimized by sorting objects based on shaders, materials, distance and vertex/index buffers.
    /// </summary>
    public class GLMeshStage : IRenderStage
    {
        private IDataPipeline dataPipeline;
        private ModelTechnique technique;

        /// <summary>
        /// Constructs a new GLMeshStage.
        /// </summary>
        /// <param name="dataPipeline">The IDataPipeline used to retrieve the 3d models</param>
        /// <param name="modelTechnique">The model technique with which to render the models.</param>
        public GLMeshStage(IDataPipeline dataPipeline, ModelTechnique modelTechnique)
        {
            this.dataPipeline = dataPipeline;
            this.technique = modelTechnique;
            this.technique.Initialise();
        }

        public void DoStage(GLRenderPipeline pipeline, ViewFrustum frustum, GLManager glManager, RenderOptions renderOptions)
        {
            glManager.View = frustum.transform;
            glManager.Projection = renderOptions.Projection;

            // retrieve the visible meshes
            List<GLMesh> meshes = new List<GLMesh>();
            this.dataPipeline.GetMeshes(frustum, meshes);

            // sort the meshes based on material
            meshes.Sort();

            // render the meshes
            VertexArray currentVertexArray = null;
            IndexBuffer currentIndexBuffer = null;
            Texture2D currentDiffuseMap = null;
            Texture2D currentNormalMap = null;
            Texture2D currentSpecularMap = null;

            glManager.BindTechnique(this.technique);

            foreach (GLMesh mesh in meshes)
            {
                Material meshMaterial = mesh.material;

                if (meshMaterial.diffuseMap != currentDiffuseMap)
                {
                    currentDiffuseMap = meshMaterial.diffuseMap;
                    if (meshMaterial.diffuseMap != null)
                    {
                        // bind new diffuse map
                        glManager.BindTexture(currentDiffuseMap, TextureUnit.Texture0);
                    }
                }
                if (meshMaterial.normalMap != currentNormalMap)
                {
                    currentNormalMap = meshMaterial.normalMap;

                    if (meshMaterial.normalMap != null)
                    {
                        // bind new normal map
                        glManager.BindTexture(currentNormalMap, TextureUnit.Texture1);
                    }
                }
                if (meshMaterial.specularMap != currentSpecularMap)
                {
                    currentSpecularMap = meshMaterial.specularMap;

                    if (currentSpecularMap != null)
                    {
                        // bind specular map
                        glManager.BindTexture(currentSpecularMap, TextureUnit.Texture2);
                    }
                }
                if (mesh.vertexArray != currentVertexArray)
                {
                    // bind new vertex array
                    currentVertexArray = mesh.vertexArray;

                    glManager.BindVertexArray(currentVertexArray);
                }
                if (mesh.indexBuffer != currentIndexBuffer)
                {
                    // bind new index buffer
                    currentIndexBuffer = mesh.indexBuffer;

                    glManager.BindIndexBuffer(currentIndexBuffer);
                }

                // Set some technique variables
                if (!meshMaterial.hasSpecularMap)
                    technique.SpecularColor = meshMaterial.specularColor;
                if (!meshMaterial.hasDiffuseMap)
                    technique.DiffuseColor = meshMaterial.diffuseColor;

                technique.HasDiffuseMap = meshMaterial.hasDiffuseMap;
                technique.HasSpecularMap = meshMaterial.hasSpecularMap;
                technique.HasNormalMap = meshMaterial.hasNormalMap;

                if (mesh.Skeleton != null)
                {
                    // upload skeleton variables
                    technique.HasSkeleton = true;
                    technique.Joints = mesh.Skeleton.finalJointMatrices;
                    technique.InvJoints = mesh.Skeleton.invJointMatrices;
                }
                else
                    technique.HasSkeleton = false;

                // set transform
                technique.SetWorld(ref mesh.transform);

                // draw elements
                glManager.DrawElementsIndexed(BeginMode.Triangles, mesh.IndexCount, mesh.IndexOffset);
            }
        }
    }
}
