using UnityEngine;
using System.Collections;

public class pan_camera_simple : MonoBehaviour {
    public bool pan_enabled = true;
    public int panSpeed = 3;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if(pan_enabled)
        transform.Translate(new Vector3(Time.deltaTime * panSpeed * Input.GetAxis("Horizontal"), Time.deltaTime * panSpeed * Input.GetAxis("Vertical")));
        pan_enabled = pan_enabled ^ Input.GetButtonDown("Jump");
	}
}
