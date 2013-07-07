using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MilkModelLoader.MilkshapeData;
using OpenTK;
using System.IO;
using OpenTK.Graphics;

namespace MilkModelLoader
{
    /// <summary>
    /// An enum containing error messages for loading the Milkshape model
    /// </summary>
    public enum MilkshapeLoadResult
    {
        ModelLoaded = 0,
        FileNotFound = 1,
        InvalidHeader = 2,
        ErrorReadingData = 3
    }

    /// <summary>
    /// The MilkshapeLoader class can load a milkshape file from disk.
    /// Internally it turns the milkshape format in a format which uses index buffers.
    /// </summary>
    public class MilkshapeLoader
    {
        private const String MS3D_HEADER_STRING = "MS3D000000";
        private const int MS3D_HEADER_VERSION = 4;

        /// <summary>
        /// Loads a milkshape model from disk.
        /// </summary>
        /// <param name="path">The location of the milkshape model</param>
        /// <returns>The loaded milkshape model</returns>
        public MilkshapeLoadResult LoadModel(String path, out MilkshapeModel model)
        {
            model = null;

            // Does the file we need to load exists?
            if (!File.Exists(path))
                return MilkshapeLoadResult.FileNotFound;
            // I don't bother checking extensions. The validity of the milkshape
            // model is determined by checking the header anyway.

            // The general loading strategy is this:
            // - Load all raw data
            // - Filter redundant vertices and create index buffers for every mesh
            // - Store new data in a MilkshapeModel object and return this object

            // Filtering redundant vertices and creating index buffers is done like this:
            // - Store every milkshape vertex in an object
            // - If the object already exists (in a List of vertices) return the index
            // - If not: add to the current vertex to the List and return the new index

            // Define raw data storage variables
            MilkshapeVertex[] milkVertices;
            MilkshapeTriangle[] milkTriangles;
            MilkshapeGroup[] milkGroups;
            MilkshapeMaterial[] milkMaterials;
            MilkshapeBone[] milkBones;

            try
            {
                // Load the model in memory
                byte[] data = File.ReadAllBytes(path);
                int index = 0;

                // We first have to verify the header
                // if the header is invalid we return an invalid header error
                String id = Encoding.ASCII.GetString(data, 0, 10);
                int version = BitConverter.ToInt32(data, 10);
                index += 14;

                if (id != MS3D_HEADER_STRING || version != MS3D_HEADER_VERSION)
                    return MilkshapeLoadResult.InvalidHeader;

                // The header is correct. We can now read the vertices
                LoadMilkVertices(data, ref index, out milkVertices);

                // Load the triangles
                LoadMilkTriangles(data, ref index, milkVertices, out milkTriangles);

                // Load the groups
                LoadMilkGroups(data, ref index, milkTriangles, out milkGroups);

                // Load the materials
                LoadMilkMaterials(data, ref index, path, milkGroups, out milkMaterials);

                // Read the animation data
                LoadMilkBones(data, ref index, out milkBones);

                // Read subversion data if available
                if (data.Length != index)
                    LoadSubversionData(data, ref index, milkVertices);
            }
            catch(Exception ex)
            {
                return MilkshapeLoadResult.ErrorReadingData;
            }

            // The milkshape model is now loaded, now to filter the redundant vertices
            List<TotalVertex> uniqueVertices = new List<TotalVertex>();
            MilkshapeMesh[] meshes = new MilkshapeMesh[milkGroups.Length];

            // For every group we create one mesh
            for (int i = 0; i < meshes.Length; i++)
            {
                MilkshapeGroup group = milkGroups[i];

                // Create the index container
                List<short> indices = new List<short>(1024);
                // Iterate every triangle part of this triangle mesh
                foreach (MilkshapeTriangle triangle in group.triangles)
                {
                    // Construct three total vertices
                    TotalVertex[] totalVertices = constructVertices(triangle);

                    for (int j = 0; j < 3; j++)
                    {
                        // Does this total vertex already exist?
                        TotalVertex found = FindVertex(totalVertices[j], uniqueVertices);
                        if (found.index == -1)
                        {
                            // It does not yet exist, add it to the list of unique vertices
                            uniqueVertices.Add(found);
                            found.index = (short)(uniqueVertices.Count - 1);
                        }
                        // Add the index of the vertex to the index buffer
                        indices.Add(found.index);
                    }
                }

                // Create the mesh
                MilkshapeMesh mesh = new MilkshapeMesh();
                mesh.name = group.name;
                mesh.material = group.material;
                mesh.indices = indices.ToArray();

                // store the mesh
                meshes[i] = mesh;
            }

            // The index buffers are now created, now to create the other arrays
            Vector3[] vertices = new Vector3[uniqueVertices.Count];
            Vector2[] texCoords = new Vector2[uniqueVertices.Count];
            Vector3[] normals = new Vector3[uniqueVertices.Count];
            Vector4[] boneIndices = new Vector4[uniqueVertices.Count];
            Vector4[] boneWeights = new Vector4[uniqueVertices.Count];
            for (int i = 0; i < uniqueVertices.Count; i++)
            {
                TotalVertex vertex = uniqueVertices[i];
                vertices[i] = vertex.vertex;
                texCoords[i] = vertex.texCoord;
                normals[i] = vertex.normal;
                boneIndices[i] = vertex.boneIndices;
                boneWeights[i] = vertex.boneWeights;
            }
            model = new MilkshapeModel();
            model.vertices = vertices;
            model.texCoords = texCoords;
            model.normals = normals;
            model.boneIndices = boneIndices;
            model.boneWeights = boneWeights;
            model.meshes = meshes;
            model.bones = milkBones;

            // Model loading succes!
            return MilkshapeLoadResult.ModelLoaded;
        }

        private TotalVertex FindVertex(TotalVertex vertex, List<TotalVertex> vertices)
        {
            // Check if an equal vertex already exists
            foreach (TotalVertex other in vertices)
            {
                if (other.Equals(vertex))
                    return other;
            }

            // return self
            return vertex;
        }
        private TotalVertex[] constructVertices(MilkshapeTriangle triangle)
        {
            TotalVertex vertex1 = new TotalVertex();
            vertex1.vertex = triangle.vertex1.vertex;
            vertex1.texCoord = triangle.texCoord1;
            vertex1.normal = triangle.normal1;
            vertex1.boneIndices = triangle.vertex1.boneIndices;
            vertex1.boneWeights = triangle.vertex1.boneWeights;

            TotalVertex vertex2 = new TotalVertex();
            vertex2.vertex = triangle.vertex2.vertex;
            vertex2.texCoord = triangle.texCoord2;
            vertex2.normal = triangle.normal2;
            vertex2.boneIndices = triangle.vertex2.boneIndices;
            vertex2.boneWeights = triangle.vertex2.boneWeights;

            TotalVertex vertex3 = new TotalVertex();
            vertex3.vertex = triangle.vertex3.vertex;
            vertex3.texCoord = triangle.texCoord3;
            vertex3.normal = triangle.normal3;
            vertex3.boneIndices = triangle.vertex3.boneIndices;
            vertex3.boneWeights = triangle.vertex3.boneWeights;

            return new TotalVertex[3] { vertex1, vertex2, vertex3 };
        }

        private void LoadMilkVertices(byte[] data, ref int index, out MilkshapeVertex[] vertices)
        {
            // Retrieve the vertex count
            short vertexCount = BitConverter.ToInt16(data, index);
            index += 2;

            vertices = new MilkshapeVertex[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                // skip flags
                index++;

                // read x, y and z component
                float x = BitConverter.ToSingle(data, index);
                index += 4;
                float y = BitConverter.ToSingle(data, index);
                index += 4;
                float z = BitConverter.ToSingle(data, index);
                index += 4;

                // read the first bone id (we can load more at the end of the file)
                byte boneId = data[index++];

                // skip reference count
                index++;

                MilkshapeVertex vertex = new MilkshapeVertex();
                vertex.boneIndices = new Vector4(boneId, -1.0f, -1.0f, -1.0f);
                vertex.boneWeights = Vector4.Zero;
                vertex.vertex = new Vector3(x, y, z);

                // store the vertex
                vertices[i] = vertex;
            }
        }   
        private void LoadMilkTriangles(byte[] data, ref int index, MilkshapeVertex[] vertices, out MilkshapeTriangle[] triangles)
        {
            // Retrieve the triangle count
            short triangleCount = BitConverter.ToInt16(data, index);
            index += 2;

            triangles = new MilkshapeTriangle[triangleCount];
            for (int i = 0; i < triangleCount; i++)
            {
                // skip the flag
                index += 2;

                // read the vertex indices
                short vertexIndex1 = BitConverter.ToInt16(data, index);
                index += 2;
                short vertexIndex2 = BitConverter.ToInt16(data, index);
                index += 2;
                short vertexIndex3 = BitConverter.ToInt16(data, index);
                index += 2;

                // read the normals
                Vector3 normal1 = new Vector3(BitConverter.ToSingle(data, index),
                                              BitConverter.ToSingle(data, index + 4),
                                              BitConverter.ToSingle(data, index + 8));
                index += 12;
                Vector3 normal2 = new Vector3(BitConverter.ToSingle(data, index),
                                              BitConverter.ToSingle(data, index + 4),
                                              BitConverter.ToSingle(data, index + 8));
                index += 12;
                Vector3 normal3 = new Vector3(BitConverter.ToSingle(data, index),
                                              BitConverter.ToSingle(data, index + 4),
                                              BitConverter.ToSingle(data, index + 8));
                index += 12;

                // Read the texture coordinates
                float s1 = BitConverter.ToSingle(data, index);
                index += 4;
                float s2 = BitConverter.ToSingle(data, index);
                index += 4;
                float s3 = BitConverter.ToSingle(data, index);
                index += 4;

                float t1 = BitConverter.ToSingle(data, index);
                index += 4;
                float t2 = BitConverter.ToSingle(data, index);
                index += 4;
                float t3 = BitConverter.ToSingle(data, index);
                index += 4;

                Vector2 texCoord1 = new Vector2(s1, t1);
                Vector2 texCoord2 = new Vector2(s2, t2);
                Vector2 texCoord3 = new Vector2(s3, t3);

                // skip the smoothing group and group index
                index += 2;

                // create the triangle
                MilkshapeTriangle triangle = new MilkshapeTriangle();
                triangle.normal1 = normal1;
                triangle.normal2 = normal2;
                triangle.normal3 = normal3;

                triangle.texCoord1 = texCoord1;
                triangle.texCoord2 = texCoord2;
                triangle.texCoord3 = texCoord3;

                triangle.vertex1 = vertices[vertexIndex1];
                triangle.vertex2 = vertices[vertexIndex2];
                triangle.vertex3 = vertices[vertexIndex3];

                // store the triangle
                triangles[i] = triangle;
            }
        }
        private void LoadMilkGroups(byte[] data, ref int index, MilkshapeTriangle[] triangles, out MilkshapeGroup[] groups)
        {
            // Retrieve the group count
            short groupCount = BitConverter.ToInt16(data, index);
            index += 2;

            groups = new MilkshapeGroup[groupCount];
            for (int i = 0; i < groupCount; i++)
            {
                // skip the flags
                index++;

                // read the group name
                String name = ReadNullTerminatedString(data, index, 32);
                index += 32;

                // retrieve triangle count
                short triangleCount = BitConverter.ToInt16(data, index);
                index += 2;

                // Get triangles linked to this group
                MilkshapeTriangle[] groupTriangles = new MilkshapeTriangle[triangleCount];
                for (int j = 0; j < triangleCount; j++)
                {
                    groupTriangles[j] = triangles[BitConverter.ToInt16(data, index)];
                    index += 2;
                }

                // get material index
                byte materialIndex = data[index++];

                // Create the group
                MilkshapeGroup group = new MilkshapeGroup();
                group.materialIndex = materialIndex;
                group.name = name;
                group.triangles = groupTriangles;

                // store the group
                groups[i] = group;
            }
        }
        private void LoadMilkMaterials(byte[] data, ref int index, String filePath, MilkshapeGroup[] groups, out MilkshapeMaterial[] materials)
        {
            // Retrieve material count
            short materialCount = BitConverter.ToInt16(data, index);
            index += 2;

            FileInfo info = new FileInfo(filePath);

            // Read the materials
            materials = new MilkshapeMaterial[materialCount];
            for (int i = 0; i < materialCount; i++)
            {
                String name = ReadNullTerminatedString(data, index, 32);
                index += 32;

                // Retrieve ambient, diffuse, specular and emissive variables
                Color4 ambient = new Color4(BitConverter.ToSingle(data, index + 0),
                                            BitConverter.ToSingle(data, index + 4),
                                            BitConverter.ToSingle(data, index + 8),
                                            BitConverter.ToSingle(data, index + 12));
                index += 16;
                Color4 diffuse = new Color4(BitConverter.ToSingle(data, index + 0),
                                            BitConverter.ToSingle(data, index + 4),
                                            BitConverter.ToSingle(data, index + 8),
                                            BitConverter.ToSingle(data, index + 12));
                index += 16;
                Color4 specular = new Color4(BitConverter.ToSingle(data, index + 0),
                                            BitConverter.ToSingle(data, index + 4),
                                            BitConverter.ToSingle(data, index + 8),
                                            BitConverter.ToSingle(data, index + 12));
                index += 16;
                Color4 emissive = new Color4(BitConverter.ToSingle(data, index + 0),
                                            BitConverter.ToSingle(data, index + 4),
                                            BitConverter.ToSingle(data, index + 8),
                                            BitConverter.ToSingle(data, index + 12));
                index += 16;

                // retrieve shininess and transparancy
                float shininess = BitConverter.ToSingle(data, index);
                index += 4;
                float transparancy = BitConverter.ToSingle(data, index);
                index += 4;

                // skip mode
                index++;

                // Read texture and alphamap path
                // This is a bit tricky. The read path are relative to the file.
                // However we need them to be absolute, otherwise we would only read them correctly 
                // if the model was in the same folder as the program.
                String relTexturePath = ReadNullTerminatedString(data, index, 128);
                index += 128;
                String relAlphaMapPath = ReadNullTerminatedString(data, index, 128);
                index += 128;

                // Make the paths absolute by adding the file path
                String absTexturePath = Path.Combine(info.Directory.ToString(), relTexturePath);
                String absAlphaMapPath = Path.Combine(info.Directory.ToString(), relAlphaMapPath);

                // Create the material
                MilkshapeMaterial material = new MilkshapeMaterial();
                material.ambient = ambient;
                material.diffuse = diffuse;
                material.specular = specular;
                material.emissive = emissive;

                material.shininess = shininess;
                material.transparancy = transparancy;

                material.name = name;

                material.texturePath = absTexturePath;
                
                // store the material
                materials[i] = material;
            }

            // Link every MilkshapeGroup to the correct MilkshapeMaterial
            foreach (MilkshapeGroup group in groups)
                group.material = materials[group.materialIndex];
        }
        private void LoadMilkBones(byte[] data, ref int index, out MilkshapeBone[] bones)
        {
            // skip animation data
            index += 12;

            // Retrieve bone count
            short boneCount = BitConverter.ToInt16(data, index);
            index += 2;

            bones = new MilkshapeBone[boneCount];
            Dictionary<String, MilkshapeBone> namedBones = new Dictionary<String, MilkshapeBone>();
            for (int i = 0; i < boneCount; i++)
            {
                // skip flags
                index++;

                // Read name
                String name = ReadNullTerminatedString(data, index, 32);
                index += 32;
                String parentName = ReadNullTerminatedString(data, index, 32);
                index += 32;

                // get initial position and rotation
                Vector3 initRotation = new Vector3(BitConverter.ToSingle(data, index + 0),
                                                   BitConverter.ToSingle(data, index + 4),
                                                   BitConverter.ToSingle(data, index + 8));
                index += 12;
                Vector3 initPosition = new Vector3(BitConverter.ToSingle(data, index + 0),
                                                   BitConverter.ToSingle(data, index + 4),
                                                   BitConverter.ToSingle(data, index + 8));
                index += 12;

                // Read key frame count
                short rotationCount = BitConverter.ToInt16(data, index);
                index += 2;
                short translationCount = BitConverter.ToInt16(data, index);
                index += 2;

                // Read key frames
                MilkshapeMovement[] rotations = new MilkshapeMovement[rotationCount];
                MilkshapeMovement[] translations = new MilkshapeMovement[translationCount];

                // read rotations
                for (int j = 0; j < rotationCount; j++)
                {
                    // Read the rotation data
                    float time = BitConverter.ToSingle(data, index);
                    index += 4;
                    Vector3 rotation = new Vector3(BitConverter.ToSingle(data, index + 0),
                                                   BitConverter.ToSingle(data, index + 4),
                                                   BitConverter.ToSingle(data, index + 8));
                    index += 12;

                    // Create the rotation key frame
                    MilkshapeMovement move = new MilkshapeMovement();
                    move.time = time;
                    move.movement = rotation;

                    // store the key frame
                    rotations[j] = move;
                }
                // read translations
                for (int j = 0; j < translationCount; j++)
                {
                    // Read the rotation data
                    float time = BitConverter.ToSingle(data, index);
                    index += 4;
                    Vector3 translation = new Vector3(BitConverter.ToSingle(data, index + 0),
                                                   BitConverter.ToSingle(data, index + 4),
                                                   BitConverter.ToSingle(data, index + 8));
                    index += 12;

                    // Create the rotation key frame
                    MilkshapeMovement move = new MilkshapeMovement();
                    move.time = time;
                    move.movement = translation;

                    // store the key frame
                    translations[j] = move;
                }

                // Create the bone
                MilkshapeBone bone = new MilkshapeBone();
                bone.index = i;
                bone.initPosition = initPosition;
                bone.initRotation = initRotation;
                bone.name = name;
                bone.parentBone = parentName.Length != 0 ? namedBones[parentName] : null;
                bone.rotations = rotations;
                bone.translations = translations;

                // store the bone
                bones[i] = bone;
                namedBones.Add(name, bone);
            }
        }
        private void LoadSubversionData(byte[] data, ref int index, MilkshapeVertex[] vertices)
        {
            // skip comment subversion
            index += 4;

            // Skip group comments
            SkipCommentSection(data, ref index);
            // Skip material comments
            SkipCommentSection(data, ref index);
            // Skip joint comments
            SkipCommentSection(data, ref index);
            // Skip model comment
            SkipCommentSection(data, ref index);

            // get vertex subversion
            int subVersion = BitConverter.ToInt32(data, index);
            index += 4;
            float weightDiv = subVersion == 1 ? 255 : 100;

            foreach(MilkshapeVertex vertex in vertices)
            {
                vertex.boneIndices.Y = data[index];
                index++;
                vertex.boneIndices.Z = data[index];
                index++;
                vertex.boneIndices.W = data[index];
                index++;

                if (subVersion == 1)
                {
                    vertex.boneWeights.X = (float)data[index] / 100.0f;
                    index++;
                    vertex.boneWeights.Y = (float)data[index] / 100.0f;
                    index++;
                    vertex.boneWeights.Z = (float)data[index] / 100.0f;
                    index++;
                }
                else
                {
                    vertex.boneWeights.X = (float)data[index] / 255.0f;
                    index++;
                    vertex.boneWeights.Y = (float)data[index] / 255.0f;
                    index++;
                    vertex.boneWeights.Z = (float)data[index] / 255.0f;
                    index++;
                }
                vertex.boneWeights.W = 1.0f - vertex.boneWeights.X - vertex.boneWeights.Y - vertex.boneWeights.Z;
            }
        }

        private void SkipCommentSection(byte[] data, ref int index)
        {
            int commentCount = BitConverter.ToInt32(data, index);
            index += 4;
            for (int i = 0; i < commentCount; i++)
            {
                SkipComment(data, ref index);
            }
        }
        private void SkipComment(byte[] data, ref int index)
        {
            // skip index
            index += 4;

            // read comment length
            int commentLength = BitConverter.ToInt32(data, index);
            index += 4 + commentLength;
        }

        private String ReadNullTerminatedString(byte[] data, int index, int length)
        {
            String result = "";
            int endIndex = index + length;

            while (data[index] != '\0' && index != endIndex)
            {
                result += (char)data[index++];
            }

            return result;
        }
    }
}
