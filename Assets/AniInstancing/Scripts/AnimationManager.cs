
/*
THIS FILE IS PART OF Animation Instancing PROJECT
AnimationInstancing.cs - The core part of the Animation Instancing library

©2017 Jin Xiaoyu. All Rights Reserved.
*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AnimationInstancing
{
    public class AnimationManager : Singleton<AnimationManager>
    {
        // A request to create animation info, because we use async method
        struct CreateAnimationRequest 
        {
            public GameObject prefab;
            public AnimationInstancing instance;
        }
        // A container to storage all animations info within game object
        public class InstanceAnimationInfo 
        {
            public List<AnimationInfo> listAniInfo;
            public ExtraBoneInfo extraBoneInfo;
        }
        private List<CreateAnimationRequest> m_requestList;
        private Dictionary<GameObject, InstanceAnimationInfo> m_animationInfo;

        private AssetBundle m_mainBundle;
        bool m_useBundle = false;

        public static AnimationManager GetInstance()
        {
            return Singleton<AnimationManager>.Instance;
        }

        private void Awake()
        {
            m_animationInfo = new Dictionary<GameObject, InstanceAnimationInfo>();
            m_requestList = new List<CreateAnimationRequest>();
        }

        private void Update()
        {
            if (m_mainBundle == null || m_requestList.Count == 0)
                return;

            for (int i = 0; i != m_requestList.Count; ++i)
            {
                CreateAnimationRequest request = m_requestList[i];
                StartCoroutine(LoadAnimationInfoFromAssetBundle(request));
            }
            m_requestList.Clear();
        }

        public InstanceAnimationInfo FindAnimationInfo(GameObject prefab, AnimationInstancing instance)
        {
            Debug.Assert(prefab != null);
            InstanceAnimationInfo info = null;
            if (m_animationInfo.TryGetValue(prefab, out info))
            {
                return info;
            }

#if UNITY_IPHONE || UNITY_ANDROID
            Debug.Assert(m_useBundle);
			if (m_mainBundle == null)
            	Debug.LogError("You should call LoadAnimationAssetBundle first.");
#endif
            if (m_useBundle)
            {
                CreateAnimationRequest request = new CreateAnimationRequest();
                request.prefab = prefab;
                request.instance = instance;
                if (m_mainBundle != null)
                {
                    StartCoroutine(LoadAnimationInfoFromAssetBundle(request));
                }
                else
                {
                    m_requestList.Add(request);
                }
                return null;
            }
            else
                return CreateAnimationInfoFromFile(prefab);
        }

        public IEnumerator LoadAnimationAssetBundle(string path)
        {
            m_useBundle = true;
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            yield return request;

			if (request.assetBundle != null)
            {
                Debug.LogFormat("Load the AB {0} successed.", path);
                m_mainBundle = request.assetBundle;
            }
        }

        public void UnloadAnimationAssetBundle()
        {
            if (m_mainBundle != null)
            {
                m_mainBundle.Unload(false);
            }
        }

        private IEnumerator LoadAnimationInfoFromAssetBundle(CreateAnimationRequest request)
        {
            Debug.Assert(m_mainBundle);
            AssetBundleRequest abRequest = m_mainBundle.LoadAssetAsync(request.prefab.name);
            yield return abRequest;

            bool find = false;
            InstanceAnimationInfo info = null;
            if (m_animationInfo.TryGetValue(request.prefab, out info))
            {
                find = true;
                request.instance.Prepare(info.listAniInfo, info.extraBoneInfo);
            }

			if (abRequest != null && !find)
            {
                TextAsset asset = abRequest.asset as TextAsset;
                BinaryReader reader = new BinaryReader(new MemoryStream(asset.bytes));
                info = new InstanceAnimationInfo();
                info.listAniInfo = ReadAnimationInfo(reader);
                info.extraBoneInfo = ReadExtraBoneInfo(reader);
                AnimationInstancingMgr.Instance.ImportAnimationTexture(request.prefab.name, reader);
                request.instance.Prepare(info.listAniInfo, info.extraBoneInfo);
                m_animationInfo.Add(request.prefab, info);
            }
        }

        private InstanceAnimationInfo CreateAnimationInfoFromFile(GameObject prefab)
        {
            Debug.Assert(prefab != null);
            string path;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            path = Application.dataPath + "/AnimationTexture/";
#elif UNITY_STANDALONE_OSX
		    path = Application.dataPath + "/Resources/Data/StreamingAssets/AnimationTexture/";
#endif

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            //Debug.Log("This is the data path:" + path);
            FileStream file = File.Open(path + prefab.name + ".bytes", FileMode.Open);
            Debug.Assert(file.CanRead);
            InstanceAnimationInfo info = new InstanceAnimationInfo();
            BinaryReader reader = new BinaryReader(file);
            info.listAniInfo = ReadAnimationInfo(reader);
            info.extraBoneInfo = ReadExtraBoneInfo(reader);
            m_animationInfo.Add(prefab, info);
            AnimationInstancingMgr.Instance.ImportAnimationTexture(prefab.name, reader);
            file.Close();
#elif UNITY_IPHONE
		    path = "file://" + Application.dataPath +"/Raw/AnimationTexture/";
		    WWW w = new WWW(path + prefab.name + ".bytes");
		    //wwwLoad(path + "/boneTexture/" + prefab.name + ".aniData", SetupAniInfo);
		    while (!w.isDone){}
		    //if (w.error)
		    Debug.Log("This is the data path:" + path);
		    Debug.Log(w.error);
            BinaryReader reader = new BinaryReader(new MemoryStream(w.bytes));	
            InstanceAnimationInfo info = new InstanceAnimationInfo();
            info.listAniInfo = ReadAnimationInfo(reader);
            info.extraBoneInfo = ReadExtraBoneInfo(reader);
            m_animationInfo.Add(prefab, info);
            AnimationInstancingMgr.Instance.ImportAnimationTexture(prefab.name, reader);
#elif UNITY_ANDROID
            path = "jar:file://" + Application.dataPath + "!/assets/";
            WWW w = new WWW(path + "AnimationTexture/" + prefab.name + ".bytes");
            //wwwLoad(path + "/boneTexture/" + thisPrefab.name + ".aniData", SetupAniInfo);
            while (!w.isDone) { }
//             BinaryReader reader = new BinaryReader(new MemoryStream(w.bytes));
//             List<AnimationInfo> listInfo = ReadAnimationInfo(reader);
//             m_animationInfo.Add(prefab, listInfo);
            BinaryReader reader = new BinaryReader(new MemoryStream(w.bytes));	
            InstanceAnimationInfo info = new InstanceAnimationInfo();
            info.listAniInfo = ReadAnimationInfo(reader);
            info.extraBoneInfo = ReadExtraBoneInfo(reader);
            m_animationInfo.Add(prefab, info);
            AnimationInstancingMgr.Instance.ImportAnimationTexture(prefab.name, reader);
#endif
            return info;
        }

        private List<AnimationInfo> ReadAnimationInfo(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            List<AnimationInfo>  listInfo = new List<AnimationInfo>();
            for (int i = 0; i != count; ++i)
            {
                AnimationInfo info = new AnimationInfo();
                //info.animationNameHash = reader.ReadInt32();
                info.animationName = reader.ReadString();
                info.animationNameHash = info.animationName.GetHashCode();
                info.animationIndex = reader.ReadInt32();
                info.textureIndex = reader.ReadInt32();
                info.totalFrame = reader.ReadInt32();
                info.fps = reader.ReadInt32();
                info.rootMotion = reader.ReadBoolean();
                if (info.rootMotion)
                {
                    info.velocity = new Vector3[info.totalFrame];
                    info.angularVelocity = new Vector3[info.totalFrame];
                    for (int j = 0; j != info.totalFrame; ++j)
                    {
                        info.velocity[j].x = reader.ReadSingle();
                        info.velocity[j].y = reader.ReadSingle();
                        info.velocity[j].z = reader.ReadSingle();

                        info.angularVelocity[j].x = reader.ReadSingle();
                        info.angularVelocity[j].y = reader.ReadSingle();
                        info.angularVelocity[j].z = reader.ReadSingle();
                    }
                }
                int evtCount = reader.ReadInt32();
                info.eventList = new List<AnimationEvent>();
                for (int j = 0; j != evtCount; ++j)
                {
                    AnimationEvent evt = new AnimationEvent();
                    evt.function = reader.ReadString();
                    evt.floatParameter = reader.ReadSingle();
                    evt.intParameter = reader.ReadInt32();
                    evt.stringParameter = reader.ReadString();
                    evt.time = reader.ReadSingle();
                    evt.objectParameter = reader.ReadString();
                    info.eventList.Add(evt);
                }
                listInfo.Add(info);
            }
            listInfo.Sort(new ComparerHash());
            return listInfo;
        }

        private ExtraBoneInfo ReadExtraBoneInfo(BinaryReader reader)
        {
            ExtraBoneInfo info = null;
            if (reader.ReadBoolean())
            {
                info = new ExtraBoneInfo();
                int count = reader.ReadInt32();
                info.extraBone = new string[count];
                info.extraBindPose = new Matrix4x4[count];
                for (int i = 0; i != info.extraBone.Length; ++i)
                {
                    info.extraBone[i] = reader.ReadString();
                }
                for (int i = 0; i != info.extraBindPose.Length; ++i)
                {
                    for (int j = 0; j != 16; ++j)
                    {
                        info.extraBindPose[i][j] = reader.ReadSingle();
                    }
                }
            }
            return info;
        }
    }
}