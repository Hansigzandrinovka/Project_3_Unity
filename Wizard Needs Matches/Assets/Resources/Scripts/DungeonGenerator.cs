using UnityEngine;
using System.Collections;

public class DungeonGenerator : MonoBehaviour {
    //uses a RNG to create dungeon tiles through rooms connected by hallways

    public static int testingSeed = 5; //the seed for the RNG system to predictably produce random tiles
    public GameObject floorTilePrefab; //the tile used to generate the floor of a given dungeon

	// Use this for initialization
	void Start () {
        Random.seed = testingSeed;
        for(int i = 0; i < 10; i++)
            for(int j = 0; j < 12; j++)
            {
                Instantiate(floorTilePrefab, new Vector3(i, j), floorTilePrefab.transform.rotation);
            }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
