using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Koeky3D.Utilities;

namespace Koeky3D.Animation
{
    /// <summary>
    /// Contains all keyframes for one bone in one animation.
    /// The keyframes can be sampled using a time. Time values less than 0.0 will be clamped to the first frame.
    /// Time values greater than the total running time of all keyframes will be clamped to the last frame.
    /// </summary>
    public class BoneKeyframes
    {
        #region variables
        /// <summary>
        /// The translation key frames
        /// </summary>
        public readonly KeyframePosition[] posFrames;
        /// <summary>
        /// The rotation key frames
        /// </summary>
        public readonly KeyframeRotation[] rotFrames;

        /// <summary>
        /// The weight of all keyframes on a bone. Default is 1.0
        /// </summary>
        public float weight = 1.0f;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a new BoneKeyframes object
        /// </summary>
        /// <param name="posFrames"></param>
        /// <param name="rotFrames"></param>
        public BoneKeyframes(KeyframePosition[] posFrames, KeyframeRotation[] rotFrames)
        {
            this.posFrames = posFrames;
            this.rotFrames = rotFrames;
        }
        #endregion

        #region BoneKeyframes methods
        /// <summary>
        /// Retrieves the translation and rotation at the given time.
        /// </summary>
        /// <param name="time">The current animation time</param>
        /// <param name="translation">The output location for the translation</param>
        /// <param name="rotation">The output location for the rotation</param>
        public void GetTranslationRotation(float time, out Vector3 translation, out Quaternion rotation)
        {
            // Determine the current key frames i1 and i2
            int i1, i2;
            i1 = i2 = -1;
            int length = this.posFrames.Length - 1;
            for (int i = 0; i < length; i++)
            {
                if (time >= posFrames[i].time && time < posFrames[i + 1].time)
                {
                    i1 = i;
                    i2 = i + 1;
                    break;
                }
            }

            GetPosition(time, i1, i2, out translation);
            GetRotation(time, i1, i2, out rotation);
        }

        /// <summary>
        /// Retrieves the translation at the given time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Vector3 GetPosition(float time)
        {
            // determine the current frame
            int i1, i2;
            i1 = i2 = -1;
            int length = this.posFrames.Length - 1;
            for (int i = 0; i < length; i++)
            {
                if (time >= posFrames[i].time && time < posFrames[i + 1].time)
                {
                    i1 = i;
                    i2 = i + 1;
                    break;
                }
            }

            if (i1 == -1 && i2 == -1)
            {
                if (time < this.posFrames[0].time)
                    return this.posFrames[0].translation;
                else
                    return this.posFrames[this.posFrames.Length - 1].translation;
            }

            // interpolate for current frame
            KeyframePosition pos1 = this.posFrames[i1];
            KeyframePosition pos2 = this.posFrames[i2];

            float t = (time - pos1.time) / (pos2.time - pos1.time);

            return pos1.translation + ((pos2.translation - pos1.translation) * t);
        }
        /// <summary>
        /// Computes the translation at the given time between the given key frames
        /// </summary>
        /// <param name="time">The animation time</param>
        /// <param name="i1">The first key frame index</param>
        /// <param name="i2">The seond key frame index</param>
        /// <param name="translation">The output location for the translation</param>
        public void GetPosition(float time, int i1, int i2, out Vector3 translation)
        {
            if (i1 == -1 && i2 == -1)
            {
                if (time < this.posFrames[0].time)
                    translation = this.posFrames[0].translation;
                else
                    translation = this.posFrames[this.posFrames.Length - 1].translation;
                return;
            }

            // interpolate for current frame
            KeyframePosition pos1 = this.posFrames[i1];
            KeyframePosition pos2 = this.posFrames[i2];

            float t = (time - pos1.time) / (pos2.time - pos1.time);

            translation = pos1.translation + ((pos2.translation - pos1.translation) * t);
        }
        /// <summary>
        /// Retrieves the rotation at the given time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Quaternion GetRotation(float time)
        {
            // determine the current frame
            int i1, i2;
            i1 = i2 = -1;
            for (int i = 0; i < rotFrames.Length - 1; i++)
            {
                if (time >= rotFrames[i].time && time < rotFrames[i + 1].time)
                {
                    i1 = i;
                    i2 = i + 1;
                    break;
                }
            }

            // if no frames found
            if (i1 == -1 && i2 == -1)
            {
                if (time < rotFrames[0].time)
                    return rotFrames[0].quaternion;
                else
                    return rotFrames[rotFrames.Length - 1].quaternion;
            }

            // interpolate for current frame
            KeyframeRotation rot1 = this.rotFrames[i1];
            KeyframeRotation rot2 = this.rotFrames[i2];

            float t = (time - rot1.time) / (rot2.time - rot1.time);

            Quaternion quat = Quaternion.Slerp(rot1.quaternion, rot2.quaternion, t);
            //Quaternion quat = GLConversion.NLerp(rot1.quaternion, rot2.quaternion, t);

            return quat;
        }
        /// <summary>
        /// Computes the rotation at the given time between the given key frames
        /// </summary>
        /// <param name="time">The animation time</param>
        /// <param name="i1">The first key frame index</param>
        /// <param name="i2">The seond key frame index</param>
        /// <param name="rotation">The output location for the rotation</param>
        public void GetRotation(float time, int i1, int i2, out Quaternion rotation)
        {
            // if no frames found
            if (i1 == -1 && i2 == -1)
            {
                if (time < rotFrames[0].time)
                    rotation = rotFrames[0].quaternion;
                else
                    rotation = rotFrames[rotFrames.Length - 1].quaternion;
                return;
            }

            // interpolate for current frame
            KeyframeRotation rot1 = this.rotFrames[i1];
            KeyframeRotation rot2 = this.rotFrames[i2];

            float t = (time - rot1.time) / (rot2.time - rot1.time);

            rotation = Quaternion.Slerp(rot1.quaternion, rot2.quaternion, t);
        }

        /// <summary>
        /// True if the index is the last translation frame
        /// </summary>
        /// <param name="posFrame"></param>
        /// <returns></returns>
        public bool IsLastPosFrame(int posFrame)
        {
            return posFrame >= posFrames.Length - 1;
        }
        /// <summary>
        /// True if the index is the last rotation frame
        /// </summary>
        /// <param name="rotFrame"></param>
        /// <returns></returns>
        public bool IsLastRotFrame(int rotFrame)
        {
            return rotFrame >= rotFrames.Length - 1;
        }
        #endregion

        #region getters & setters
        /// <summary>
        /// Gets the total translation frames
        /// </summary>
        public int TotalPositionFrames
        {
            get
            {
                return this.posFrames.Length;
            }
        }
        /// <summary>
        /// Gets the total rotation frames
        /// </summary>
        public int TotalRotationFrames
        {
            get
            {
                return this.rotFrames.Length;
            }
        }
        /// <summary>
        /// Gets the total amount of frames.
        /// </summary>
        public int TotalFrames
        {
            get
            {
                return Math.Max(TotalPositionFrames, TotalRotationFrames);
            }
        }

        /// <summary>
        /// Gets the total amount of time needed for all translation frames
        /// </summary>
        public float TotalPositionTime
        {
            get
            {
                if (this.posFrames.Length == 0)
                    return 0.0f;

                return this.posFrames[this.posFrames.Length - 1].time;
            }
        }
        /// <summary>
        /// Gets the total amount of time needed for all rotation frames
        /// </summary>
        public float TotalRotationTime
        {
            get
            {
                if (this.rotFrames.Length == 0)
                    return 0.0f;

                return this.rotFrames[this.rotFrames.Length - 1].time;
            }
        }
        /// <summary>
        /// Gets the total amount of time needed for all frames
        /// </summary>
        public float TotalTime
        {
            get
            {
                return Math.Max(TotalPositionTime, TotalRotationTime);
            }
        }
        #endregion
    }
}
