using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using MilkModelLoader.MilkshapeData;

namespace MilkModelLoader
{
    public class MilkshapeModel
    {
        /// <summary>
        /// The vertex positions
        /// </summary>
        public Vector3[] vertices;
        /// <summary>
        /// The texture coordinates
        /// </summary>
        public Vector2[] texCoords;
        /// <summary>
        /// The normals
        /// </summary>
        public Vector3[] normals;
        /// <summary>
        /// The bone indices
        /// or: which bones every vertex is attached to
        /// </summary>
        public Vector4[] boneIndices;
        /// <summary>
        /// The bone weights
        /// or: how much every vertex is influenced by the attached bones
        /// </summary>
        public Vector4[] boneWeights;
        /// <summary>
        /// The triangle meshes (or groups) this model contains
        /// </summary>
        public MilkshapeMesh[] meshes;
        /// <summary>
        /// The bones of this model.
        /// </summary>
        public MilkshapeBone[] bones;
    }
}
