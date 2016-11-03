using UnityEngine;
using System.Collections;

public class DungeonGenerator : MonoBehaviour {
    //uses a RNG to create dungeon tiles through rooms connected by hallways

    public static int testingSeed = 5; //the seed for the RNG system to predictably produce random tiles
    public GameObject floorTilePrefab; //the tile used to generate the floor of a given dungeon
    public int roomCount = 1;
    public int minRoomSize = 3;
    public int maxRoomSize = 5;

    private int count = 0; //used for counting action sequence

	// Use this for initialization
	void Start () {
        if(floorTilePrefab == null)
        {
            Debug.LogError("Unrecoverable Error! Missing floor tile prefab, removing this gameobject");
            Destroy(this.gameObject);
        }
        Random.seed = testingSeed;
        if (maxRoomSize == 0)
        {
            maxRoomSize = 3;
            Debug.LogError("Error! Max room size should not be zero, assuming value three");
        }
        if(minRoomSize == 0)
        {
            minRoomSize = 3;
            Debug.LogError("Error! Min room size should not be zero, assuming value three");
        }
        if (minRoomSize > maxRoomSize)
        {
            minRoomSize = maxRoomSize;
            Debug.LogError("Error! Min room size is less than max room size, assuming they are same value");
        }
	}

    //@precondition: attached object (this) doesn't have a messed up rotation
    bool TryCreateRoom(Vector3 roomOrigin, int roomSize)
    {
        Debug.Log("Attempting to create room");
        for(int i = 0; i < roomSize; i++)
            for(int j = 0; j < roomSize; j++) //for every position in the room, try to place a tile there, if there is a tile, abort
            {
                Vector2 topRightLoc = new Vector2(roomOrigin.x + i + 0.25f, roomOrigin.y + j + 0.25f);
                Vector2 botLeftLoc = new Vector2(roomOrigin.x + i - 0.25f, transform.position.y + j - 0.25f);
                Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask); //check if there is a tile there
                if (collider != null) //if a tile already exists here, this whole room is a bust
                {
                    Debug.Log("Thats a negatory, its blocked");
                    return false;
                }
                    
            }
        Debug.Log("Created room successfully");
        for(int i = 0; i < roomSize; i++) //populate room with Tiles
            for(int j = 0; j < roomSize; j++)
            {
                Instantiate(floorTilePrefab, new Vector3(roomOrigin.x + i, roomOrigin.y + j, roomOrigin.z), transform.rotation);
            }
        return true;
    }
	
	// Update is called once per frame
	void Update () {
	switch(count)
        {
            case 0:
                {
                    TryCreateRoom(new Vector3(transform.position.x, transform.position.y, TileMonoBehavior.tileZLayer), 3);
                    Debug.Log("First room attempted");
                    count++;
                    break;
                }
            case 1:
                {
                    TryCreateRoom(new Vector3(transform.position.x, transform.position.y, TileMonoBehavior.tileZLayer), 3);
                    Debug.Log("Second room attempted, expect failure");
                    count++;
                    break;
                }
        }
	}
}
