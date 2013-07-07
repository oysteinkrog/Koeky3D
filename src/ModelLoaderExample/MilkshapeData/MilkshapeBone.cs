using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace MilkModelLoader.MilkshapeData
{
    public class MilkshapeBone
    {
        /// <summary>
        /// The index of the bone in the bone hierarchy.
        /// </summary>
        public int index;
        /// <summary>
        /// The name of this bone
        /// </summary>
        public String name;
        /// <summary>
        /// The parent bone or null if no parent is available
        /// </summary>
        public MilkshapeBone parentBone;
        /// <summary>
        /// The initial position of this bone relative to its parent
        /// </summary>
        public Vector3 initPosition;
        /// <summary>
        /// The initial rotation of this bone relative to its parent
        /// </summary>
        public Vector3 initRotation;

        /// <summary>
        /// All the key frame rotations of this bone
        /// </summary>
        public MilkshapeMovement[] rotations;
        /// <summary>
        /// All the key frame translations of this bone
        /// </summary>
        public MilkshapeMovement[] translations;
    }

    public class MilkshapeMovement
    {
        public float time;
        public Vector3 movement;
    }
}
