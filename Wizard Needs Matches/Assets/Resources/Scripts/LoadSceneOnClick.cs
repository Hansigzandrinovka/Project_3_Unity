using UnityEngine;
using System.Collections;

public class LoadSceneOnClick : MonoBehaviour {

	public void LoadByIndex(int sceneIndex)
	{
		Application.LoadLevel (sceneIndex);
	}
}
