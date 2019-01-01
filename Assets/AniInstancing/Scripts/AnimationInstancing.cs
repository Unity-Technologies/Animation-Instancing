/*
THIS FILE IS PART OF Animation Instancing PROJECT
AnimationInstancing.cs - The core part of the Animation Instancing library

©2017 Jin Xiaoyu. All Rights Reserved.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace AnimationInstancing
{
    [AddComponentMenu("AnimationInstancing")]
    public class AnimationInstancing : MonoBehaviour
    {
        private Animator animator = null;
        [NonSerialized]
        public Transform worldTransform;
        //public GameObject prefab { get; set; }
		public GameObject prototype;
        public BoundingSphere boundingSpere;
        public bool visible { get; set; }
        public AnimationInstancing parentInstance { get; set; }
        public float playSpeed = 1.0f;
        public UnityEngine.Rendering.ShadowCastingMode shadowCastingMode;
        public bool receiveShadow;
        [NonSerialized]
        public int layer;
        float speedParameter = 1.0f, cacheParameter = 1.0f;
        WrapMode wrapMode;
        public WrapMode Mode
        {
             get {return wrapMode;}
             set {wrapMode = value;}
        }
        public bool IsLoop() { return Mode == WrapMode.Loop; }
        public bool IsPause() { return speedParameter == 0.0f; }

        public bool applyRootMotion = false;
        [Range(1, 4)]
        public int bonePerVertex = 4;
        [NonSerialized]
        public float curFrame;
        [NonSerialized]
        public float preAniFrame;
        [NonSerialized]
        public int aniIndex = -1;
        [NonSerialized]
        public int preAniIndex = -1;
        [NonSerialized]
        public int aniTextureIndex = -1;
        int preAniTextureIndex = -1;
        float transitionDuration = 0.0f;
        bool isInTransition = false;
        float transitionTimer = 0.0f;
        [NonSerialized]
        public float transitionProgress = 0.0f;
        private int eventIndex = -1;

        public List<AnimationInfo> aniInfo;
        private ComparerHash comparer;
        private AnimationInfo searchInfo;
        private AnimationEvent aniEvent = null;
        public class LodInfo
        {
            public int lodLevel;
            public SkinnedMeshRenderer[] skinnedMeshRenderer;
            public MeshRenderer[] meshRenderer;
            public MeshFilter[] meshFilter;
            public AnimationInstancingMgr.VertexCache[] vertexCacheList;
            public AnimationInstancingMgr.MaterialBlock[] materialBlockList;
        }
        [NonSerialized]
        public LodInfo[] lodInfo;
        private float lodCalculateFrequency = 0.5f;
        private float lodFrequencyCount = 0.0f;
        [NonSerialized]
        public int lodLevel;
        private Transform[] allTransforms;
        private bool isMeshRender = false;
        [NonSerialized]
        private List<AnimationInstancing> listAttachment;

        void Start()
        {
            if (!AnimationInstancingMgr.Instance.UseInstancing)
            {
                gameObject.SetActive(false);
                return;
            }

            worldTransform = GetComponent<Transform>();
            animator = GetComponent<Animator>();
            boundingSpere = new BoundingSphere(new Vector3(0, 0, 0), 1.0f);
            listAttachment = new List<AnimationInstancing>();
            layer = gameObject.layer;

            switch (QualitySettings.blendWeights)
            {
                case BlendWeights.TwoBones:
                    bonePerVertex = bonePerVertex > 2?2: bonePerVertex;
                    break;
                case BlendWeights.OneBone:
                    bonePerVertex = 1;
                    break;
            }

            UnityEngine.Profiling.Profiler.BeginSample("Calculate lod");
            LODGroup lod = GetComponent<LODGroup>();
            if (lod != null)
            {
                lodInfo = new LodInfo[lod.lodCount];
                LOD[] lods = lod.GetLODs();
                for (int i = 0; i != lods.Length; ++i)
                {
                    if (lods[i].renderers == null)
                    {
                        continue;
                    }

                    LodInfo info = new LodInfo();
                    info.lodLevel = i;
                    info.vertexCacheList = new AnimationInstancingMgr.VertexCache[lods[i].renderers.Length];
                    info.materialBlockList = new AnimationInstancingMgr.MaterialBlock[info.vertexCacheList.Length];
                    List<SkinnedMeshRenderer> listSkinnedMeshRenderer = new List<SkinnedMeshRenderer>();
                    List<MeshRenderer> listMeshRenderer = new List<MeshRenderer>();
                    foreach (var render in lods[i].renderers)
                    {
                        if (render is SkinnedMeshRenderer)
                            listSkinnedMeshRenderer.Add((SkinnedMeshRenderer)render);
                        if (render is MeshRenderer)
                            listMeshRenderer.Add((MeshRenderer)render);
                    }
                    info.skinnedMeshRenderer = listSkinnedMeshRenderer.ToArray();
                    info.meshRenderer = listMeshRenderer.ToArray();
                    //todo, to make sure whether the MeshRenderer can be in the LOD.
                    info.meshFilter = null;
                    for (int j = 0; j != lods[i].renderers.Length; ++j)
                    {
                        lods[i].renderers[j].enabled = false;
                    }
                    lodInfo[i] = info;
                }
            }
            else
            {
                lodInfo = new LodInfo[1];
                LodInfo info = new LodInfo();
                info.lodLevel = 0;
                info.skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
                info.meshRenderer = GetComponentsInChildren<MeshRenderer>();
                info.meshFilter = GetComponentsInChildren<MeshFilter>();
                info.vertexCacheList = new AnimationInstancingMgr.VertexCache[info.skinnedMeshRenderer.Length + info.meshRenderer.Length];
                info.materialBlockList = new AnimationInstancingMgr.MaterialBlock[info.vertexCacheList.Length];
                lodInfo[0] = info;

                for (int j = 0; j != info.meshRenderer.Length; ++j)
                {
                    info.meshRenderer[j].enabled = false;
                }
                for (int j = 0; j != info.skinnedMeshRenderer.Length; ++j)
                {
                    info.skinnedMeshRenderer[j].enabled = false;
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();

            if (AnimationInstancingMgr.Instance.UseInstancing
                && animator != null)
            {
                animator.enabled = false;
            }
            visible = true;
            CalcBoundingSphere();
            AnimationInstancingMgr.Instance.AddBoundingSphere(this);
            AnimationInstancingMgr.Instance.AddInstance(gameObject);
        }


        private void OnDestroy()
        {
            if (!AnimationInstancingMgr.IsDestroy())
            {
                AnimationInstancingMgr.Instance.RemoveInstance(this);
            }
            if (parentInstance != null)
            {
                parentInstance.Deattach(this);
                parentInstance = null;
            }
        }

        private void OnEnable()
        {
            playSpeed = 1.0f;
            visible = true;
            if (listAttachment != null)
            {
                for (int i = 0; i != listAttachment.Count; ++i)
                {
                    listAttachment[i].gameObject.SetActive(true);
                }
            }
        }

        private void OnDisable()
        {
            playSpeed = 0.0f;
            visible = false;
            if (listAttachment != null)
            {
                for (int i = 0; i != listAttachment.Count; ++i)
                {
                    listAttachment[i].gameObject.SetActive(false);
                }
            }
        }


        public bool InitializeAnimation()
        {
			if(prototype == null) 
			{
				Debug.LogError("The prototype is NULL. Please select the prototype first.");
			}
			Debug.Assert(prototype != null);
			GameObject thisPrefab = prototype;
            isMeshRender = false;
            if (lodInfo[0].skinnedMeshRenderer.Length == 0)
            {
                // This is only a MeshRenderer, it has no animations.
                isMeshRender = true;
				AnimationInstancingMgr.Instance.AddMeshVertex(prototype.name,
                    lodInfo,
                    null,
                    null,
                    bonePerVertex);
                return true;
            }

			AnimationManager.InstanceAnimationInfo info = AnimationManager.Instance.FindAnimationInfo(prototype, this);
            if (info != null)
            {
                aniInfo = info.listAniInfo;
                Prepare(aniInfo, info.extraBoneInfo);
            }
            searchInfo = new AnimationInfo();
            comparer = new ComparerHash();
            return true;
        }

        public void Prepare(List<AnimationInfo> infoList, ExtraBoneInfo extraBoneInfo)
        {
            aniInfo = infoList;
            //extraBoneInfo = extraBoneInfo;
            List<Matrix4x4> bindPose = new List<Matrix4x4>(150);
            // to optimize, MergeBone don't need to call every time
            Transform[] bones = RuntimeHelper.MergeBone(lodInfo[0].skinnedMeshRenderer, bindPose);
            allTransforms = bones;

            if (extraBoneInfo != null)
            {
                List<Transform> list = new List<Transform>();
                list.AddRange(bones);                
                Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
                for (int i = 0; i != extraBoneInfo.extraBone.Length; ++i)
                {
                    for (int j = 0; j != transforms.Length; ++j)
                    {
                        if (extraBoneInfo.extraBone[i] == transforms[j].name)
                        {
                            list.Add(transforms[j]);
                        }
                    }
                    bindPose.Add(extraBoneInfo.extraBindPose[i]);
                }
                allTransforms = list.ToArray();
            }
            

			AnimationInstancingMgr.Instance.AddMeshVertex(prototype.name,
                lodInfo,
                allTransforms,
                bindPose,
                bonePerVertex);

            foreach (var lod in lodInfo)
            {
                foreach (var cache in lod.vertexCacheList)
                {
                    cache.shadowcastingMode = shadowCastingMode;
                    cache.receiveShadow = receiveShadow;
                    cache.layer = layer;
                }
            }

            Destroy(GetComponent<Animator>());
            //Destroy(GetComponentInChildren<SkinnedMeshRenderer>());

            PlayAnimation(0);
        }

        private void CalcBoundingSphere()
        {
            UnityEngine.Profiling.Profiler.BeginSample("CalcBoundingSphere()");
            Bounds bound = new Bounds(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            LodInfo info = lodInfo[0];
            for (int i = 0; i != info.meshRenderer.Length; ++i)
            {
                MeshRenderer meshRenderer = info.meshRenderer[i];
                bound.Encapsulate(meshRenderer.bounds);
            }
            for (int i = 0; i != info.skinnedMeshRenderer.Length; ++i)
            {
                SkinnedMeshRenderer skinnedMeshRenderer = info.skinnedMeshRenderer[i];
                bound.Encapsulate(skinnedMeshRenderer.bounds);
            }
            float radius = bound.size.x > bound.size.y ? bound.size.x : bound.size.y;
            radius = radius > bound.size.z ? radius : bound.size.z;
            boundingSpere.radius = radius;
            UnityEngine.Profiling.Profiler.EndSample();
        }


        public void PlayAnimation(string name)
        {
            int hash = name.GetHashCode();
            int index = FindAnimationInfo(hash);
            PlayAnimation(index);
        }

        public void PlayAnimation(int animationIndex)
        {
            if (aniInfo == null)
            {
                return;
            }
            if (animationIndex == aniIndex && !IsPause())
            {
                return;
            }

            transitionDuration = 0.0f;
            transitionProgress = 1.0f;
            isInTransition = false;
            Debug.Assert(animationIndex < aniInfo.Count);
            if (0 <= animationIndex && animationIndex < aniInfo.Count)
            {
                preAniIndex = aniIndex;
                aniIndex = animationIndex;
                preAniFrame = (float)(int)(curFrame + 0.5f);
                curFrame = 0.0f;
                eventIndex = -1;
                preAniTextureIndex = aniTextureIndex;
                aniTextureIndex = aniInfo[aniIndex].textureIndex;
                wrapMode = aniInfo[aniIndex].wrapMode;
                speedParameter = 1.0f;
            }
            else
            {
                Debug.LogWarning("The requested animation index is out of the count.");
                return;
            }
            RefreshAttachmentAnimation(aniTextureIndex);
        }

        public void CrossFade(string animationName, float duration)
        {
            int hash = animationName.GetHashCode();
            int index = FindAnimationInfo(hash);
            CrossFade(index, duration);
        }

        public void CrossFade(int animationIndex, float duration)
        {
            PlayAnimation(animationIndex);
            if (duration > 0.0f)
            {
                isInTransition = true;
                transitionTimer = 0.0f;
                transitionProgress = 0.0f;
            }
            else
            {
                transitionProgress = 1.0f;
            }
            transitionDuration = duration;
        }

        public void Pause()
        {
            cacheParameter = speedParameter;
            speedParameter = 0.0f;
        }

        public void Resume()
        {
            speedParameter = cacheParameter;
        }

        public void Stop()
        {
            aniIndex = -1;
            preAniIndex = -1;
            eventIndex = -1;
            curFrame = 0.0f;
        }

        public bool IsPlaying()
        {
            return aniIndex >= 0 || isMeshRender;
        }

        public bool IsReady()
        {
            return aniInfo != null;
        }

        public AnimationInfo GetCurrentAnimationInfo()
        {
            if (aniInfo != null && 0 <= aniIndex && aniIndex < aniInfo.Count)
            {
                return aniInfo[aniIndex];
            }
            return null;
        }

        public AnimationInfo GetPreAnimationInfo()
        {
            if (aniInfo != null && 0 <= preAniIndex && preAniIndex < aniInfo.Count)
            {
                return aniInfo[preAniIndex];
            }
            return null;
        }

        public void UpdateAnimation()
        {
            if (aniInfo == null || IsPause())
                return;

            if (isInTransition)
            {
                transitionTimer += Time.deltaTime;
                float weight = transitionTimer / transitionDuration;
                transitionProgress = Mathf.Min(weight, 1.0f);
                if (transitionProgress >= 1.0f)
                {
                    isInTransition = false;
                    preAniIndex = -1;
                    preAniFrame = -1;
                }
            }
            float speed = playSpeed * speedParameter;
            curFrame += speed * Time.deltaTime * aniInfo[aniIndex].fps;
            int totalFrame = aniInfo[aniIndex].totalFrame;
            switch (wrapMode)
            {
                case WrapMode.Loop:
                {
                    if (curFrame < 0f)
                        curFrame += (totalFrame - 1);
                    else if (curFrame > totalFrame - 1)
                        curFrame -= (totalFrame - 1);
                    break;
                }
                case WrapMode.PingPong:
                {
                    if (curFrame < 0f)
                    {
                        speedParameter = Mathf.Abs(speedParameter);
                        curFrame = Mathf.Abs(curFrame);
                    }
                    else if (curFrame > totalFrame - 1)
                    {
                        speedParameter = -Mathf.Abs(speedParameter);
                        curFrame = 2 * (totalFrame - 1) - curFrame;
                    }
                    break;
                }
                case WrapMode.Default:
                case WrapMode.Once:
                {
                    if (curFrame < 0f || curFrame > totalFrame - 1.0f)
                    {
                        Pause();
                    }
                    break;
                }
            }

            curFrame = Mathf.Clamp(curFrame, 0f, totalFrame - 1);
            for (int i = 0; i != listAttachment.Count; ++i)
            {
                AnimationInstancing attachment = listAttachment[i];
                attachment.transform.position = transform.position;
                attachment.transform.rotation = transform.rotation;
            }
            UpdateAnimationEvent();
        }

        public void UpdateLod(Vector3 cameraPosition)
        {
            lodFrequencyCount += Time.deltaTime;
            if (lodFrequencyCount > lodCalculateFrequency)
            {
                float sqrLength = (cameraPosition - worldTransform.position).sqrMagnitude;
                if (sqrLength < 50.0f)
                    lodLevel = 0;
                else if (sqrLength < 500.0f)
                    lodLevel = 1;
                else
                    lodLevel = 2;
                lodFrequencyCount = 0.0f;
                lodLevel = Mathf.Clamp(lodLevel, 0, lodInfo.Length - 1);
            }
        }

        private void UpdateAnimationEvent()
        {
            AnimationInfo info = GetCurrentAnimationInfo();
            if (info == null)
                return;
            if (info.eventList.Count == 0)
                return;

            if (aniEvent == null)
            {
                float time = curFrame / info.fps;
                for (int i = eventIndex >= 0? eventIndex: 0; i < info.eventList.Count; ++i)
                {
                    if (info.eventList[i].time > time)
                    {
                        aniEvent = info.eventList[i];
                        eventIndex = i;
                        break;
                    }
                }
            }

            if (aniEvent != null)
            {
                float time = curFrame / info.fps;
                if (aniEvent.time <= time)
                {
                    gameObject.SendMessage(aniEvent.function, aniEvent);
                    aniEvent = null;
                }
            }
        }

        private int FindAnimationInfo(int hash)
        {
            if (aniInfo == null)
                return -1;
            searchInfo.animationNameHash = hash;
            return aniInfo.BinarySearch(searchInfo, comparer);
        }

        public void Attach(string boneName, AnimationInstancing attachment)
        {
            int index = -1;
            int hashBone = boneName.GetHashCode();
            for (int i = 0; i != allTransforms.Length; ++i)
            {
                if (allTransforms[i].name.GetHashCode() == hashBone)
                {
                    index = i;
                    break;
                }
            }
            Debug.Assert(index >= 0);
            if (index < 0)
            {
                Debug.LogError("Can't find the bone.");
                return;
            }
            if (attachment.lodInfo[0].meshRenderer.Length == 0 && attachment.lodInfo[0].skinnedMeshRenderer.Length == 0)
            {
                Debug.LogError("The attachment doesn't have a Renderer");
                return;
            }

            attachment.parentInstance = this;
            AnimationInstancingMgr.VertexCache parentCache = AnimationInstancingMgr.Instance.FindVertexCache(lodInfo[0].skinnedMeshRenderer[0].name.GetHashCode());
            listAttachment.Add(attachment);
            
            int nameCode = boneName.GetHashCode();
            nameCode += attachment.lodInfo[0].meshRenderer.Length > 0? attachment.lodInfo[0].meshRenderer[0].name.GetHashCode(): 0;
            if (attachment.lodInfo[0].meshRenderer.Length == 0)
            {
                //todo, to support the attachment that has skinnedMeshRenderer;
                int skinnedMeshRenderCount = attachment.lodInfo[0].skinnedMeshRenderer.Length;
                nameCode += skinnedMeshRenderCount > 0? attachment.lodInfo[0].skinnedMeshRenderer[0].name.GetHashCode(): 0;
            }
            AnimationInstancingMgr.VertexCache cache = AnimationInstancingMgr.Instance.FindVertexCache(nameCode);
            // if we can reuse the VertexCache, we don't need to create one
            if (cache != null)
            {
                cache.boneTextureIndex = parentCache.boneTextureIndex;
                return;
            }

			AnimationInstancingMgr.Instance.AddMeshVertex(attachment.prototype.name,
                        attachment.lodInfo,
                        null,
                        null,
                        attachment.bonePerVertex,
                        boneName);

            for (int i = 0; i != attachment.lodInfo.Length; ++i)
            {
                LodInfo info = attachment.lodInfo[i];
                for (int j = 0; j != info.meshRenderer.Length; ++j)
                {
                    cache = info.vertexCacheList[info.skinnedMeshRenderer.Length + j];
                    Debug.Assert(cache != null);
                    if (cache == null)
                    {
                        Debug.LogError("Can't find the VertexCache.");
                        continue;
                    }
                    Debug.Assert(cache.boneTextureIndex < 0 || cache.boneIndex[0].x != index);

                    AnimationInstancingMgr.Instance.BindAttachment(parentCache, cache, info.meshFilter[j].sharedMesh, index);
                    AnimationInstancingMgr.Instance.SetupAdditionalData(cache);
                    cache.boneTextureIndex = parentCache.boneTextureIndex;
                }
            }
        }


        public void Deattach(AnimationInstancing attachment)
        {
            attachment.visible = false;
            attachment.parentInstance = null;
            RefreshAttachmentAnimation(-1);
            listAttachment.Remove(attachment);
        }

        public int GetAnimationCount()
        {
            return aniInfo != null? aniInfo.Count: 0;
        }

        private void RefreshAttachmentAnimation(int index)
        {
            for (int k = 0; k != listAttachment.Count; ++k)
            {
                AnimationInstancing attachment = listAttachment[k];
                //attachment.aniIndex = aniIndex;
                for (int i = 0; i != attachment.lodInfo.Length; ++i)
                {
                    LodInfo info = attachment.lodInfo[i];
                    for (int j = 0; j != info.meshRenderer.Length; ++j)
                    {
                        //MeshRenderer render = info.meshRenderer[j];
                        AnimationInstancingMgr.VertexCache cache = info.vertexCacheList[info.skinnedMeshRenderer.Length + j];
                        cache.boneTextureIndex = index;
                    }
                }
            }
        }
    }
}