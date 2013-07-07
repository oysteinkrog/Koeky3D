using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Koeky3D.BufferHandling;
using Koeky3D;
using Koeky3D.Utilities;
using Koeky3D.Shapes;

namespace GLFrameWork.Shapes
{
    /// <summary>
    /// Helper class for drawing some frequently used shapes like: quads, spheres, circles, cones and cylinders.
    /// </summary>
    public static class ShapeDrawer
    {
        #region constants
        private const float SPHERE_HEIGHTINC = 0.1f;
        private const float SPHERE_ANGLEINC = 10.0f;

        private const float CONE_ANGLEINC = 10.0f;
        #endregion

        #region variables
        private static bool initialised = false;
        private static bool beginCalled = false;

        private static VertexArray vertexArray;

        private static int quadBegin, quadCount;
        private static int sphereBegin, sphereCount;
        private static int coneBegin, coneCount;
        #endregion

        #region ShapeDrawer static methods
        private static void Initialise()
        {
            if (initialised)
                throw new Exception("Cannot initialise an already initialised ShaperDrawer");

            List<Vector3> vertexes = new List<Vector3>();
            List<Vector2> texCoords = new List<Vector2>();

            #region Quad creation
            // create a 1*1 quad

            quadBegin = 0;
            quadCount = 6;

            // add vertexes and texcoords
            vertexes.Add(new Vector3(0.0f, 0.0f, 0.0f));
            texCoords.Add(new Vector2(0.0f, 1.0f));
            vertexes.Add(new Vector3(0.0f, 1.0f, 0.0f));
            texCoords.Add(new Vector2(0.0f, 0.0f));
            vertexes.Add(new Vector3(1.0f, 1.0f, 0.0f));
            texCoords.Add(new Vector2(1.0f, 0.0f));

            vertexes.Add(new Vector3(1.0f, 1.0f, 0.0f));
            texCoords.Add(new Vector2(1.0f, 0.0f));
            vertexes.Add(new Vector3(1.0f, 0.0f, 0.0f));
            texCoords.Add(new Vector2(1.0f, 1.0f));
            vertexes.Add(new Vector3(0.0f, 0.0f, 0.0f));
            texCoords.Add(new Vector2(0.0f, 1.0f));
            #endregion

            #region Sphere creation
            // create a grid of points mapped as a sphere

            sphereBegin = vertexes.Count;

            int sphereWidth = (int)(360.0f / SPHERE_ANGLEINC) + 1;
            int sphereHeight = (int)(2.0f / SPHERE_HEIGHTINC) + 1;

            Vector3[,] pointMap = new Vector3[sphereHeight + 1, sphereWidth];

            for (int xIndex = 0; xIndex <= sphereHeight; xIndex++)
            {
                float height = (xIndex - (sphereHeight >> 1)) * SPHERE_HEIGHTINC;
                float radius = (float)Math.Sqrt(1.0f - height * height);

                for (int yIndex = 0; yIndex < sphereWidth; yIndex++)
                {
                    float radians = MathHelper.DegreesToRadians(yIndex * SPHERE_ANGLEINC);
                    float sin = (float)Math.Sin(radians) * radius;
                    float cos = (float)Math.Cos(radians) * radius;

                    pointMap[xIndex, yIndex] = new Vector3(sin, height, cos);
                }
            }

            // TODO: Generate texture coordinates
            sphereCount = 0;
            for (int x = 0; x < sphereHeight; x++)
            {
                for (int y = 0; y < sphereWidth - 1; y++)
                {
                    vertexes.Add(pointMap[x, y]);
                    texCoords.Add(new Vector2(0.0f, 0.0f));
                    vertexes.Add(pointMap[x, y + 1]);
                    texCoords.Add(new Vector2(0.0f, 0.0f));
                    vertexes.Add(pointMap[x + 1, y + 1]);
                    texCoords.Add(new Vector2(0.0f, 0.0f));

                    vertexes.Add(pointMap[x, y]);
                    texCoords.Add(new Vector2(0.0f, 0.0f));
                    vertexes.Add(pointMap[x + 1, y + 1]);
                    texCoords.Add(new Vector2(0.0f, 0.0f));
                    vertexes.Add(pointMap[x + 1, y]);
                    texCoords.Add(new Vector2(0.0f, 0.0f));

                    sphereCount += 6;
                }
            }
            #endregion

            #region Cone creation
            coneBegin = vertexes.Count;

            Vector3 previous, next;
            previous = next = Vector3.Zero;

            int segmentCount = (int)(360.0f / CONE_ANGLEINC);
            for (int i = 0; i < segmentCount; i++)
            {
                // calculate current angle
                float angleCur = MathHelper.DegreesToRadians(i * CONE_ANGLEINC);
                float sinCur = (float)Math.Sin(angleCur);
                float cosCur = (float)Math.Cos(angleCur);

                // calculate next angle
                float angleNext = MathHelper.DegreesToRadians((i + 1) * CONE_ANGLEINC);
                float sinNext = (float)Math.Sin(angleNext);
                float cosNext = (float)Math.Cos(angleNext);

                // calculate binding points
                Vector3 point1 = new Vector3(sinCur, cosCur, 1.0f);
                Vector3 point2 = new Vector3(sinNext, cosNext, 1.0f);

                // add first triangle
                vertexes.Add(Vector3.Zero);
                texCoords.Add(Vector2.Zero);

                vertexes.Add(point1);
                texCoords.Add(Vector2.Zero);

                vertexes.Add(point2);
                texCoords.Add(Vector2.Zero);

                // add second triangle
                vertexes.Add(new Vector3(0.0f, 0.0f, 1.0f));
                texCoords.Add(Vector2.Zero);

                vertexes.Add(point2);
                texCoords.Add(Vector2.Zero);

                vertexes.Add(point1);
                texCoords.Add(Vector2.Zero);
            }

            coneCount = vertexes.Count - coneBegin;
            #endregion

            // store data in a vertex array
            vertexArray = new VertexArray
            (
                new VertexBuffer(BufferUsageHint.StaticDraw, (int)BufferAttribute.Vertex, vertexes.ToArray()),
                new VertexBuffer(BufferUsageHint.StaticDraw, (int)BufferAttribute.TexCoord, texCoords.ToArray())
            );

            initialised = true;
        }

        /// <summary>
        /// Renders the viewing frustum and inward pointing normals.
        /// WARNING: this function should only be used for testing. It is no way efficient.
        /// Begin does not need to be called for this function.
        /// </summary>
        /// <param name="frustum"></param>
        public static void DrawViewFrustum(GLManager glManager, ViewFrustum frustum)
        {
            // Generate a vertex array
            VertexBuffer vertexBuffer = new VertexBuffer(BufferUsageHint.StaticDraw, (int)BufferAttribute.Vertex, frustum.transformedVertices);
            IndexBuffer indices = new IndexBuffer(BufferUsageHint.StaticDraw,
                new int[12 * 2]
                {
                    // bottom
                    0, 1,
                    1, 2,
                    2, 3,
                    3, 0,

                    // top
                    4, 5,
                    5, 6,
                    6, 7,
                    7, 4,

                    // middle
                    0, 4,
                    1, 5,
                    2, 6,
                    3, 7
                }
            );
            VertexArray vertexArray = new VertexArray(indices, vertexBuffer);
            glManager.BindVertexArray(vertexArray);
            glManager.DrawElementsIndexed(BeginMode.Lines, indices.Count, 0);
        }

        /// <summary>
        /// Enabled the drawing of default shapes. Always call this function before calling any of the draw functions.
        /// </summary>
        /// <param name="glManager"></param>
        public static void Begin(GLManager glManager)
        {
            if (beginCalled)
                return;
            if (!initialised)
                Initialise();

            glManager.BindVertexArray(vertexArray);

            beginCalled = true;
        }
        /// <summary>
        /// Ends the drawing of default shapes. Always call this function when finished calling draw functions.
        /// </summary>
        /// <param name="glManager"></param>
        public static void End(GLManager glManager)
        {
            if (!beginCalled)
                return;

            glManager.BindVertexArray(null);

            beginCalled = false;
        }

        /// <summary>
        /// Draws a 2D quad. Setting an orthographic projection is left to the caller.
        /// </summary>
        /// <param name="glManager"></param>
        /// <param name="position">The position</param>
        /// <param name="size">The size</param>
        public static void DrawQuad(GLManager glManager, Vector2 position, Vector2 size)
        {
            DrawQuad(glManager, GLConversion.CreateScaleMatrix(size.X, size.Y, 1.0f) * Matrix4.CreateTranslation(position.X, position.Y, 0.0f));
        }
        /// <summary>
        /// Draws a 2D quad. Setting an orthographic projection is left to the caller.
        /// </summary>
        /// <param name="glManager"></param>
        /// <param name="transform">The transform</param>
        public static void DrawQuad(GLManager glManager, Matrix4 transform)
        {
            if (!beginCalled)
                throw new Exception("Begin not called");

            glManager.World = transform;
            glManager.DrawElements(BeginMode.Triangles, quadBegin, quadCount);
        }

        /// <summary>
        /// Draws a sphere.
        /// </summary>
        /// <param name="glManager"></param>
        /// <param name="position">The sphere position</param>
        /// <param name="radius">The sphere radius</param>
        public static void DrawSphere(GLManager glManager, Vector3 position, float radius)
        {
            DrawSphere(glManager, GLConversion.CreateScaleMatrix(radius, radius, radius) * Matrix4.CreateTranslation(position));
        }
        /// <summary>
        /// Draws a sphere.
        /// </summary>
        /// <param name="glManager"></param>
        /// <param name="transform">The transform</param>
        public static void DrawSphere(GLManager glManager, Matrix4 transform)
        {
            if (!beginCalled)
                throw new Exception("Begin not called");

            glManager.World = transform;
            glManager.DrawElements(BeginMode.Triangles, sphereBegin, sphereCount);
        }

        /// <summary>
        /// Draws a cone
        /// </summary>
        /// <param name="glManager">The GLManager to use</param>
        /// <param name="position">The position of the cone</param>
        /// <param name="orientation">The orientation of the cone in degrees</param>
        /// <param name="scale">The scale of the cone</param>
        public static void DrawCone(GLManager glManager, Vector3 position, Vector3 orientation, Vector3 scale)
        {
            DrawCone(glManager, GLConversion.CreateScaleMatrix(scale.X, scale.Y, scale.Z) * GLConversion.CreateRotationMatrix(orientation) * Matrix4.CreateTranslation(position));
        }
        public static void DrawCone(GLManager glManager, Vector3 position, Matrix4 orientation, Vector3 scale)
        {
            DrawCone(glManager, GLConversion.CreateScaleMatrix(scale.X, scale.Y, scale.Z) * orientation * Matrix4.CreateTranslation(position));
        }
        /// <summary>
        /// Draws a cone
        /// </summary>
        /// <param name="glManager">The GLManager to use</param>
        /// <param name="transform">The transform of the cone</param>
        public static void DrawCone(GLManager glManager, Matrix4 transform)
        {
            if (!beginCalled)
                throw new Exception("Begin not called");

            glManager.World = transform;
            glManager.DrawElements(BeginMode.Triangles, coneBegin, coneCount);
        }

        public static void DrawCircle()
        {
            if (!beginCalled)
                throw new Exception("Begin not called");

            throw new NotImplementedException();
        }
        public static void DrawCylinder()
        {
            if (!beginCalled)
                throw new Exception("Begin not called");

            throw new NotImplementedException();
        }
        #endregion
    }
}
