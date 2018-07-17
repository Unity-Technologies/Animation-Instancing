using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationInstancing
{
    public class TestInstancing : MonoBehaviour
    {

        public int InstancingCount = 500;
        public int OriginalCount = 1000;
        public GameObject m_commonObj = null;
        public GameObject[] m_instancingList;
        public GameObject m_testAttachment;
        private ArrayList m_objs;
        private bool m_useInstancing = false;

        private void OnEnable()
        {
            LoadAB();
        }

        void LoadAB()
        {
			StartCoroutine(AnimationManager.GetInstance().LoadAnimationAssetBundle(Application.streamingAssetsPath + "/AssetBundle/animationtexture"));
        }

        // Use this for initialization
        void Start()
        {
            m_objs = new ArrayList();
            CreateObjInstancing();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnGUI()
        {
            Rect rect = new Rect(20, 10, 200, 100);
            GUILayout.BeginArea(rect);
            if (GUILayout.Toggle(m_useInstancing, "Use Animations Instancing"))
            {
                if (!m_useInstancing)
                {
                    m_useInstancing = true;
                    Clear();
                    CreateObjInstancing();
                }

            }
            else
            {
                if (m_useInstancing)
                {
                    m_useInstancing = false;
                    Clear();
                    CreateObjNoInstancing();
                }
            }

            if (GUILayout.Button("RemoveInstancingRandom"))
            {
                int randomIndex = Random.Range(0, m_objs.Count);
                GameObject obj = m_objs[randomIndex] as GameObject;
                m_objs.Remove(obj);
                Destroy(obj);
                obj = null;
            }
            GUILayout.EndArea();
        }

        void Clear()
        {
            AnimationInstancingMgr.GetInstance().Clear();
            foreach (var obj in m_objs)
            {
                GameObject gameObj = obj as GameObject;
                Destroy(gameObj);
            }
        }

        void CreateObjNoInstancing()
        {
            Vector3 pos = new Vector3();
            Quaternion q = new Quaternion();
            int width = (int)UnityEngine.Mathf.Sqrt((int)InstancingCount);
            for (int i = 0; i != OriginalCount; ++i)
            {
                GameObject obj = Instantiate(m_commonObj, pos, q);
                pos.x += 1.5f;
                if (pos.x > width * 1.5f)
                {
                    pos.x = 0.0f;
                    pos.z += 1.5f;
                }
                m_objs.Add(obj);
            }
        }

        void CreateObjInstancing()
        {
            //LoadAB();
            Vector3 pos = new Vector3();
            int width = (int)UnityEngine.Mathf.Sqrt((int)InstancingCount);
            for (int i = 0; i != InstancingCount; ++i)
            {
                GameObject prefab = m_instancingList[Random.Range(0, m_instancingList.Length)];
                GameObject obj = AnimationInstancingMgr.GetInstance().CreateInstance(prefab);
                GameObject attachment = null;
                if (m_testAttachment != null)
                {
                    attachment = AnimationInstancingMgr.GetInstance().CreateInstance(m_testAttachment);
                }
                
                Transform trans = obj.GetComponent<Transform>();
                trans.SetPositionAndRotation(pos, Quaternion.identity);
                //trans.Rotate(new Vector3(0, 45, 0));
                pos.x += 1.5f;
                if (pos.x > width * 1.5f)
                {
                    pos.x = 0.0f;
                    pos.z += 1.5f;
                }
                AnimationInstancing script = obj.GetComponent<AnimationInstancing>();
                //script.PlayAnimation(Random.Range(0, script.m_aniInfo.Count));
                AnimationInstancing attachmentScript = null;
                if (attachment)
                {
                    attachmentScript = GetComponent<AnimationInstancing>();
                }
                StartCoroutine(RandomPlayAnimation(script, attachmentScript));
                //StartCoroutine(RandomPlayAnimation(script));
                m_objs.Add(obj);
            }
        }

        int test = 0;
        WaitForSeconds wait2Play;
        IEnumerator RandomPlayAnimation(AnimationInstancing script, AnimationInstancing attachment = null)
        {
            if (wait2Play == null)
            {
                wait2Play = new WaitForSeconds(2.0f);
            }
            yield return wait2Play;

            script.PlayAnimation(Random.Range(0, script.GetAnimationCount()));
            //script.PlayAnimation(5);

            if (attachment != null)
            {
                //string name = Random.Range(0, 100)>50 ? "RightHand" : "LeftHand";
                string name = "Prop";
                script.Attach(name, attachment);
                //script.Attach("Neck", attachment);
            }

            if (attachment != null)
            {
                //StartCoroutine(TestDetach(script, attachment));
            }
        }

        IEnumerator TestDetach(AnimationInstancing script, AnimationInstancing attachment)
        {
            yield return wait2Play;

            script.Deattach(attachment);
        }
    }
}