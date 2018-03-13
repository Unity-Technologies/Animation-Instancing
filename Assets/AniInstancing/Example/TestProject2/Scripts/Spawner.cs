using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner: MonoBehaviour {

    public GameObject prefabA;
    public GameObject prefabB;
    static int count = 0;
    static float lastTime = 0;
    public int showCount = 0;

    List<GameObject> objList;
	void OnGUI()
	{
		GUILayout.Label(string.Format("Spawns up to {0} characters, current {1}", showCount, count));

        if (GUI.Button(new Rect(10, 100, 100, 40), "Decrease"))
        {
            showCount -= 50;
        }
        if (GUI.Button(new Rect(130, 100, 100, 40), "Increase"))
        {
            showCount += 50;
        }

        string text = AnimationInstancing.AnimationInstancingMgr.Instance.UseInstancing ? "EnableInstancing" : "DisableInstancing";
        if (GUI.Button(new Rect(10, 150, 140, 40), text))
        {
            AnimationInstancing.AnimationInstancingMgr.Instance.UseInstancing = !AnimationInstancing.AnimationInstancingMgr.Instance.UseInstancing;
            Clear();
        }
    }

    void Start()
    {
        lastTime = Time.time;
        objList = new List<GameObject>();
        LoadAB();
		AnimationInstancing.AnimationInstancingMgr.Instance.UseInstancing = true;
    }

    void LoadAB()
    {
		StartCoroutine(AnimationInstancing.AnimationManager.Instance.LoadAnimationAssetBundle(Application.streamingAssetsPath + "/AssetBundle/animationtexture"));
    }


    void Clear()
    {
        foreach (var obj in objList)
        {
            Destroy(obj);
        }
        AnimationInstancing.AnimationInstancingMgr.Instance.Clear();

        objList.Clear();
        count = 0;
    }

    void Update()
    {
        if(count < showCount)
        {
            bool alt = Input.GetButton("Fire1");

            if (Time.time - lastTime > 0.1f)
            {
                if (AnimationInstancing.AnimationInstancingMgr.Instance.UseInstancing)
                {
                    if (prefabA != null)
                    {
                        GameObject obj = AnimationInstancing.AnimationInstancingMgr.Instance.CreateInstance(prefabA);
                        obj.transform.position = new Vector3(0, 0, 0);
                        objList.Add(obj);
                        //obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        //obj.GetComponent<AnimationInstancing.AnimationInstancing>().PlayAnimation(Random.Range(0, 2));
                    }
                }
                else
                {
                    GameObject obj = null;
                    if (prefabA != null && !alt)
                        obj = Instantiate(prefabB, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
                    if (prefabB != null && alt)
                        obj = Instantiate(prefabA, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
                    obj.SetActive(true);
                    objList.Add(obj);
                }
                
                lastTime = Time.time;
                count++;
                //showCount = count;
            }
        }
    }
}
