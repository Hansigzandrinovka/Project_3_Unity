using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ProceduralCallGUIPlayerConnection : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Canvas testCanvas = this.GetComponent<Canvas>();
        if(testCanvas == null)
        {
            Debug.LogError("GUI Player Connector unable to get Canvas for GUI connection");
            return;
        }
        ProceduralComponentConnector.AllocateGUICameraListener(testCanvas);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
