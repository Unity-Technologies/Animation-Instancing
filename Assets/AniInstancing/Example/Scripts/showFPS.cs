using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class showFPS : MonoBehaviour {

    private Text m_text;
	// Use this for initialization
	void Start () {
        m_text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        float fps = 1.0f / UnityEngine.Time.smoothDeltaTime;
        //m_fps = string.Format("FPS: {0}", 1.0f / UnityEngine.Time.smoothDeltaTime);
        //GUI.Label(m_uiPosition, "FPS: " + fps.ToString());
        m_text.text = "FPS: " + fps.ToString();
    }
}
