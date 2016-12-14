using UnityEngine;
using System.Collections;

/// Music manager
public class musicPlayer : MonoBehaviour {

	public AudioSource BGM;
	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(gameObject);	///Keep the music playing
	}
	
	/*
	// Update is called once per frame
	void Update () {
	
	}
	public void ChangeMusic(AudioClip music)
	{
		BGM.Stop();
		BGM.clip = music;
		BGM.Play ();
	}
	*/
}
