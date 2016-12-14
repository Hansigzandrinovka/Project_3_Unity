using UnityEngine;
using System.Collections;

public class musicPlayer : MonoBehaviour {

	public AudioSource BGM;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(gameObject);	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void ChangeMusic(AudioClip music)
	{
		BGM.Stop();
		BGM.clip = music;
		BGM.Play ();
	}
}
