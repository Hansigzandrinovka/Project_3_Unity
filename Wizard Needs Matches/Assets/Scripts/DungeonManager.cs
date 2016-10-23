using UnityEngine;
using System.Collections;

public class DungeonManager : MonoBehaviour {

    //public GameObject[] tileset; //holds Prefabs used to tile room
    public static Queue turnOrder; //holds all controllers that can act
    //when a controller finishes acting, it tells DungeonManager that its turn is finished, and DungeonManager grabs next actor in Queue and calls its StartTurn method

    // Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
