/*
THIS FILE IS PART OF Animation Instancing PROJECT
AnimationInstancing.cs - The core part of the Animation Instancing library

©2017 Jin Xiaoyu. All Rights Reserved.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationInstancing
{
    public class AnimationEvent
    {
        public string function;
        public int intParameter;
        public float floatParameter;
        public string stringParameter;
        public string objectParameter;
        public float time;
    }
    public class AnimationInfo
    {
        public string animationName;
        public int animationNameHash;
        public int totalFrame;
        public int fps;
        public int animationIndex;
        public int textureIndex;
        public bool rootMotion;
        public Vector3[] velocity;
        public Vector3[] angularVelocity;
        public List<AnimationEvent> eventList; 
    }

    public class ExtraBoneInfo
    {
        public string[] extraBone;
        public Matrix4x4[] extraBindPose;
    }

    public class ComparerHash : IComparer<AnimationInfo>
    {
        public int Compare(AnimationInfo x, AnimationInfo y)
        {
            return x.animationNameHash.CompareTo(y.animationNameHash);
        }
    }
}
