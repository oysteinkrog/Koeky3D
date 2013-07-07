using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Koeky3D.Utilities;

namespace Koeky3D.Animation
{
    /// <summary>
    /// Provides the implementation for one bone of a skeleton.
    /// A bone can be animated by a collection of animations.
    /// </summary>
    public class Bone
    {
        #region variables
        /// <summary>
        /// The index of this bone in a skeleton
        /// </summary>
        public readonly int index;
        /// <summary>
        /// The index of the parent bone in a skeleton
        /// </summary>
        public readonly int parentIndex;
        /// <summary>
        /// The name of the bone
        /// </summary>
        public String boneName = "";
        /// <summary>
        /// The local transform of this bone
        /// </summary>
        public Matrix4 localTransform;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a bone
        /// </summary>
        /// <param name="index">The index of this bone in a skeleton</param>
        /// <param name="parentIndex">The index of the parent bone in a skeleton</param>
        /// <param name="localTransform">The local transform of this bone</param>
        public Bone(int index, int parentIndex, Matrix4 localTransform)
        {
            this.index = index;
            this.parentIndex = parentIndex;
            this.localTransform = localTransform;
        }
        #endregion

        #region Bone methods
        /// <summary>
        /// Animates this bone using the given animations. The result is written to the given skeleton.
        /// </summary>
        /// <param name="animations">
        /// The animations to animate this bone with. Every animation has a weight. 
        /// The total weight of all animations together always equals 1.0.
        /// <para>Example: if you have two animations each with weight 1.0 the weight of one animation becomes 0.5.</para>
        /// </param>
        /// <param name="skeleton">The skeleton this bone is part of</param>
        public void ProcessAnimations(List<RunningAnimation> animations, Skeleton skeleton)
        {
            if (animations.Count == 0)
            {
                // no animation, use default transform multiplied with parent transform if available
                if (this.parentIndex == -1)
                    skeleton.finalJointMatrices[this.index] = skeleton.absoluteJointMatrices[this.index];
                else
                    skeleton.finalJointMatrices[this.index] = this.localTransform * skeleton.finalJointMatrices[this.parentIndex];
            }
            else
            {
                Vector3 totalTrans = Vector3.Zero;
                Vector4 totalQuat = Vector4.Zero;
                Quaternion totalRot = Quaternion.Identity;

                Matrix4 totalTransform = new Matrix4();

                // compute the total weight, this is used as a baseline. So if we only gained 0.5 weight it will be upsized to 1.0
                float totalWeight = 0.0f;
                foreach (RunningAnimation runAnim in animations)
                {
                    Animation animation = runAnim.animation;
                    BoneKeyframes frames = animation.framesPerBone[this.index];

                    // if no keyframes: skip
                    if (frames.TotalFrames == 0)
                        continue;

                    totalWeight += frames.weight * runAnim.animWeight;
                }

                foreach (RunningAnimation runAnim in animations)
                {
                    Animation animation = runAnim.animation;
                    BoneKeyframes frames = animation.framesPerBone[this.index];

                    float animWeight = (frames.weight * runAnim.animWeight) / totalWeight;

                    // if no keyframes: skip
                    if (frames.TotalFrames == 0 || animWeight <= 0.0f)
                        continue;

                    // Retrieve the translation and rotation
                    Vector3 translation;
                    Quaternion rotation;

                    frames.GetTranslationRotation(runAnim.time, out translation, out rotation);

                    // Add it to the current total transform
                    Matrix4 rotMatrix, transform;
                    GLConversion.CreateRotationMatrix(ref rotation, out rotMatrix);
                    GLConversion.CreateTransformMatrix(ref translation, ref rotMatrix, out transform);
                    GLConversion.MultMatrix(ref transform, animWeight, out transform);
                    GLConversion.AddMatrix(ref totalTransform, ref transform, out totalTransform);
                }

                // multiply the animation transform with the location transform
                Matrix4.Mult(ref totalTransform, ref this.localTransform, out totalTransform);

                if (this.parentIndex == -1)
                    skeleton.finalJointMatrices[this.index] = totalTransform;
                else
                {
                    Matrix4.Mult(ref totalTransform, ref skeleton.finalJointMatrices[this.parentIndex], out skeleton.finalJointMatrices[this.index]);
                }
            }
        }
        #endregion

        #region object methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.index + ". " + this.boneName + " -> " + this.parentIndex;
        }
        #endregion
    }
}
