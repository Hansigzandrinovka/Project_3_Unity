using UnityEngine;
using System.Collections;

public class DungeonGenerator : MonoBehaviour {
    //uses a RNG to create dungeon tiles through rooms connected by hallways

    public static int testingSeed = 5; //the seed for the RNG system to predictably produce random tiles
    public GameObject floorTilePrefab; //the tile used to generate the floor of a given dungeon
    public int numberOfRooms = 1;
    public int minRoomSize = 3;
    public int maxRoomSize = 5;
    public int maxDisplacement = 3; //max distance any two rooms can be from each other

    private int count = 0; //used for counting action sequence
    private Stack createdRooms;

	// Use this for initialization
	void Start () {
        createdRooms = new Stack(numberOfRooms);
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
    dungeon_room TryCreateRoom(Vector3 roomOrigin, int roomSize)
    {
        Debug.Log("Attempting to create room");
        for(int i = 0; i < roomSize; i++)
            for(int j = 0; j < roomSize; j++) //for every position in the room, try to place a tile there, if there is a tile, abort
            {
                Vector2 topRightLoc = new Vector2(roomOrigin.x + i + 0.25f, roomOrigin.y + j + 0.25f);
                Vector2 botLeftLoc = new Vector2(roomOrigin.x + i - 0.25f, roomOrigin.y + j - 0.25f);
                Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask); //check if there is a tile there
                if (collider != null) //if a tile already exists here, this whole room is a bust
                {
                    Debug.Log("Error! Found gameobject at " + collider.transform.position.ToString());
                    Debug.Log("Failed at coordinates " + (roomOrigin.x + i) + "," + (roomOrigin.y + j));
                    return null;
                }
                    
            }
        Debug.Log("Created room successfully");
        for(int i = 0; i < roomSize; i++) //populate room with Tiles
            for(int j = 0; j < roomSize; j++)
            {
                Instantiate(floorTilePrefab, new Vector3(roomOrigin.x + i, roomOrigin.y + j, roomOrigin.z), transform.rotation);
            }
        return new dungeon_room(roomOrigin.x, roomOrigin.y, roomSize) ;
    }
	
	// Update is called once per frame
	void Update ()
    {
        //TryCreateRoom(new Vector3(0, 0, 0), 3);
        //TryCreateRoom(new Vector3(0, -4, 0), 3);
        //TryCreateRoom(new Vector3(0, 50, 0), 3);
        //TryCreateRoom(new Vector3(-3, 0, 0), 3);
        //on each update, pick a random room from list, try to generate a new room relative to that room
        //if no room in list (means we have exhausted all rooms to create around, or we have not created first room), give up on creating room
        //
        if(count < numberOfRooms) //create numberOfRooms rooms
        {
            if(count == 0) //creating very first room
            {
                int newSize = Random.Range(minRoomSize, maxRoomSize + 1); //size of new room
                dungeon_room newRoom = TryCreateRoom(new Vector3(transform.position.x - 10, transform.position.y, TileMonoBehavior.tileZLayer), newSize);
                if (newRoom != null) //if room successfully created
                {
                    Debug.Log("First room created");
                    createdRooms.Push(newRoom);
                    count++;
                }
                else
                {
                    Debug.Log("Unable to create first room, so floor must be predefined.");
                    count = numberOfRooms; //forces Generator to stop generating
                }
            }
            else if(createdRooms.Peek() != null) //not first element, and there is a next element to grab
            {
                
                dungeon_room oldRoom = (dungeon_room)createdRooms.Peek(); //use old room's position to find new room
                dungeon_room newRoom = null; //the room we hope to create
                int roomSize = Random.Range(minRoomSize, maxRoomSize + 1);
                Debug.Log("new room size picked to be " + roomSize);
                
                float distance = Random.Range(1, maxDisplacement + 1);
                Debug.Log("Distance away " + distance);

                Vector3 newRoomLocation = new Vector3();
                Vector3 oldRoomLocation = oldRoom.getCoords();
                int direction = Random.Range(1, 5); //direction can be 1 up, 2 left, 3 down, 4 right, 0 not yet defined
                Debug.Log("Picked direction " + direction);
                int originalDirection = direction;

                do //try to create room at given direction, if it fails, keep trying with new direction until 
                {
                    
                    switch (direction)
                    {
                        case 1: //up
                            {
                                newRoomLocation = new Vector3(oldRoomLocation.x, oldRoomLocation.y + distance + oldRoom.getSize(), 0);
                                break;
                            }
                        case 2: //left
                            {
                                newRoomLocation = new Vector3(oldRoomLocation.x - distance - oldRoom.getSize(), oldRoomLocation.y, 0);
                                break;
                            }
                        case 3: //down
                            {
                                newRoomLocation = new Vector3(oldRoomLocation.x, oldRoomLocation.y - distance - roomSize, 0);
                                break;
                            }
                        case 4: //right
                            {
                                newRoomLocation = new Vector3(oldRoomLocation.x, 0 + distance + roomSize);
                                break;
                            }
                    }
                    //default case, it tries to initialize room at 0,0,0
                    Debug.Log("Creating next room at " + newRoomLocation.x + ',' + newRoomLocation.y + ',' + newRoomLocation.z + " with size " + roomSize);
                    newRoom = TryCreateRoom(newRoomLocation,roomSize); //try to create the room at specified location
                    if (newRoom != null) //if room creation succeeded, store the room in list for later use
                    {
                        count++;
                        Debug.Log("New room created");
                        createdRooms.Push(newRoom);
                    }
                    else if (direction == 4)
                    {
                        //else keep trying new directions until all exhausted
                        direction = 1;
                        Debug.Log("Creation right failed, trying up");
                    }
                    else
                    {
                        direction++;
                        Debug.Log("Creation failed, might try next direction");
                    }
                        
                }
                while (newRoom != null && direction != originalDirection);
                if(newRoom == null) //we were unable to create a room with any direction, giving up
                {
                    Debug.Log("Failed to create room, finished with room generation");
                    count = numberOfRooms;
                }
                else
                {
                    Debug.Log("Successfully created room");
                }

            }
        }
        else //we are finished creating rooms
        {
            Debug.Log("Finished creating dungeon");
            Destroy(this.gameObject);
        }
	}
}
