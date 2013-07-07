using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Koeky3D.Utilities
{
    /// <summary>
    /// Static class containing mathematic helper functions.
    /// </summary>
    public static class GLConversion
    {
        private const double SIMD_2_PI = 6.283185307179586232;
        private const double SIMD_PI = SIMD_2_PI * 0.5;
        private const double SIMD_HALF_PI = SIMD_2_PI * 0.25;

        /// <summary>
        /// Creates a rotation matrix from the given angles.
        /// </summary>
        /// <param name="xAngle">The x angle in radians</param>
        /// <param name="yAngle">The y angle in radians</param>
        /// <param name="zAngle">The z angle in radians</param>
        /// <returns></returns>
        public static Matrix4 CreateRotationMatrix(float xAngle, float yAngle, float zAngle)
        {
            return Matrix4.CreateRotationZ(zAngle) * Matrix4.CreateRotationY(yAngle) * Matrix4.CreateRotationX(xAngle);
        }
        /// <summary>
        /// Creates a rotation matrix from the given euler angle.
        /// </summary>
        /// <param name="angles">The angles in degrees</param>
        /// <returns></returns>
        public static Matrix4 CreateRotationMatrix(Vector3 angles)
        {
            angles /= (float)(180.0f / Math.PI);
            return CreateRotationMatrix(angles.X, angles.Y, angles.Z);
        }
        /// <summary>
        /// Constructs a rotation matrix from the given quaternion
        /// </summary>
        /// <param name="quaternion"></param>
        /// <returns></returns>
        public static Matrix4 CreateRotationMatrix(Quaternion quaternion)
        {
            Matrix4 result;
            CreateRotationMatrix(ref quaternion, out result);
            return result;
        }
        /// <summary>
        /// Creates a rotation matrix from the given quaternion
        /// </summary>
        /// <param name="quaternion">The quaternion to use to rotate</param>
        /// <param name="result">The output matrix</param>
        public static void CreateRotationMatrix(ref Quaternion quaternion, out Matrix4 result)
        {
            result = new Matrix4();

            float x = quaternion.X;
            float y = quaternion.Y;
            float z = quaternion.Z;
            float w = quaternion.W;

            float x2X = 2.0f * x;
            float y2X = 2.0f * y;
            float z2X = 2.0f * z;

            result.M11 = 1.0f - y2X * y - z2X * z;
            result.M12 = x2X * y + 2.0f * w * z;
            result.M13 = x2X * z - 2.0f * w * y;

            result.M21 = x2X * y - 2.0f * w * z;
            result.M22 = 1.0f - x2X * x - z2X * z;
            result.M23 = y2X * z + 2.0f * w * x;

            result.M31 = x2X * z + 2.0f * w * y;
            result.M32 = y2X * z - 2.0f * w * x;
            result.M33 = 1.0f - x2X * x - 2.0f * y * y;

            result.M44 = 1.0f;
        }

        /// <summary>
        /// Creates a transform from the given position and rotation
        /// </summary>
        /// <param name="position">The position</param>
        /// <param name="rotation">The rotation in radians</param>
        /// <returns></returns>
        public static Matrix4 CreateTransformMatrix(Vector3 position, Vector3 rotation)
        {
            return CreateTransformMatrix(position, CreateRotationMatrix(rotation));
        }
        /// <summary>
        /// Creates a transform from the given position and quaternion
        /// </summary>
        /// <param name="position">The position</param>
        /// <param name="quaternion">The quaternion describing the rotation</param>
        /// <returns></returns>
        public static Matrix4 CreateTransformMatrix(Vector3 position, Quaternion quaternion)
        {
            return CreateTransformMatrix(position, CreateRotationMatrix(quaternion));
        }
        /// <summary>
        /// Creates a transform from the given position and rotation matrix
        /// </summary>
        /// <param name="position">The position</param>
        /// <param name="rotation">The matrix describing the rotation</param>
        /// <returns></returns>
        public static Matrix4 CreateTransformMatrix(Vector3 position, Matrix4 rotation)
        {
            return rotation * Matrix4.CreateTranslation(position);
        }
        /// <summary>
        /// Creates a transform from the given position and rotation matrix
        /// </summary>
        /// <param name="position">The position to translate with</param>
        /// <param name="rotation">The rotation matrix to rotate with</param>
        /// <param name="transform">The output matrix</param>
        public static void CreateTransformMatrix(ref Vector3 position, ref Matrix4 rotation, out Matrix4 transform)
        {
            Matrix4 translation = Matrix4.CreateTranslation(position);
            Matrix4.Mult(ref rotation, ref translation, out transform);
        }

        /// <summary>
        /// Converts the angle in radians to a quaternion
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static Quaternion CreateQuaternion(Vector3 radians)
        {
            return CreateQuaternion(radians.X, radians.Y, radians.Z);
        }
        /// <summary>
        /// Converts the angle in radians to a quaternion
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Quaternion CreateQuaternion(float x, float y, float z)
        {
            float angle;
            float sr, sp, sy, cr, cp, cy;

            Quaternion quat = new Quaternion();

            // FIXME: rescale the inputs to 1/2 angle
            angle = z * 0.5f;
            sy = (float)Math.Sin(angle);
            cy = (float)Math.Cos(angle);
            angle = y * 0.5f;
            sp = (float)Math.Sin(angle);
            cp = (float)Math.Cos(angle);
            angle = x * 0.5f;
            sr = (float)Math.Sin(angle);
            cr = (float)Math.Cos(angle);

            quat.X = sr * cp * cy - cr * sp * sy; // X
            quat.Y = cr * sp * cy + sr * cp * sy; // Y
            quat.Z = cr * cp * sy - sr * sp * cy; // Z
            quat.W = cr * cp * cy + sr * sp * sy; // W

            return quat;
        }

        /// <summary>
        /// Creates a scale matrix
        /// </summary>
        /// <param name="x">The x scaling</param>
        /// <param name="y">The y scaling</param>
        /// <param name="z">The z scaling</param>
        /// <returns></returns>
        public static Matrix4 CreateScaleMatrix(float x, float y, float z)
        {
            return new Matrix4(x, 0.0f, 0.0f, 0.0f,
                               0.0f, y, 0.0f, 0.0f,
                               0.0f, 0.0f, z, 0.0f,
                               0.0f, 0.0f, 0.0f, 1.0f);
        }

        /// <summary>
        /// Extracts the position from the given transform
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Vector3 ExtractPosition(Matrix4 transform)
        {
            return new Vector3(transform.M41, transform.M42, transform.M43);
        }

        /// <summary>
        /// Multiplies the given matrix with the given value
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Matrix4 MultWithFloat(ref Matrix4 matrix, float value)
        {
            Matrix4 result;
            MultMatrix(ref matrix, value, out result);
            return result;
        }
        /// <summary>
        /// Multiplies the given matrix with given float value
        /// </summary>
        /// <param name="left">The matrix</param>
        /// <param name="value">The value to multiply with</param>
        /// <param name="result">The matrix to write the result to</param>
        public static void MultMatrix(ref Matrix4 left, float value, out Matrix4 result)
        {
            result = new Matrix4(
                left.Row0 * value,
                left.Row1 * value,
                left.Row2 * value,
                left.Row3 * value);
        }
        /// <summary>
        /// Adds the two given matrices together
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix4 AddMatrix(Matrix4 left, Matrix4 right)
        {
            Matrix4 result;
            AddMatrix(ref left, ref right, out result);
            return result;
        }
        /// <summary>
        /// Adds the two given matrices together
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="result"></param>
        public static void AddMatrix(ref Matrix4 left, ref Matrix4 right, out Matrix4 result)
        {
            result = new Matrix4(
                left.Row0 + right.Row0,
                left.Row1 + right.Row1,
                left.Row2 + right.Row2,
                left.Row3 + right.Row3);
        }
    }
}
