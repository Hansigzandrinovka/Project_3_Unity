using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class InEditor_SnapToGrid : MonoBehaviour {
    //The sole purpose of this Script is to run during Edit mode and make its owning object snap to Unity's grid when the object is first placed

	// When this object is first placed in the editor, it snaps to Unity's X-Y grid
	void OnRenderObject () {
        transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y),transform.position.z);
	}
}
