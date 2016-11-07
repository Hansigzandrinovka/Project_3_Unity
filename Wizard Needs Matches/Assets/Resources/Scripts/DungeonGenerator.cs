using UnityEngine;
using System.Collections;

public class DungeonGenerator : MonoBehaviour {
    //uses a RNG to create dungeon tiles through rooms connected by hallways

    public static int testingSeed = 5; //the seed for the RNG system to predictably produce random tiles
    public GameObject floorTilePrefab; //the tile used to generate the floor of a given dungeon
    public GameObject tileContainer; //the gameobject that every tile prefab will be a part of
    public int numberOfRooms = 1;
    public int minRoomSize = 3;
    public int maxRoomSize = 5;
    public int maxDisplacement = 3; //max distance any two rooms can be from each other
    public bool testingMode = true; //determines if testing seed is used for RNG

    private int count = 0; //used for counting action sequence
    private Queue createdRoomsQueue; //rooms that have been created, but could be invalid to create bordering rooms around
    private Stack tappedRoomsStack; //rooms that have been found invalid to create at

	// Use this for initialization
	void Start () {
        if (maxRoomSize == 0)
        {
            maxRoomSize = 3;
            Debug.LogError("Error! Max room size should not be zero, assuming value three");
        }
        if (minRoomSize == 0)
        {
            minRoomSize = 3;
            Debug.LogError("Error! Min room size should not be zero, assuming value three");
        }
        if (minRoomSize > maxRoomSize)
        {
            minRoomSize = maxRoomSize;
            Debug.LogError("Error! Min room size is less than max room size, assuming they are same value");
        }
        if (tileContainer == null)
            Debug.Log("Missing TileContainer object, Tiles will be cluttering normal hierarchy");
        if(floorTilePrefab == null)
        {
            Debug.LogError("Unrecoverable Error! Missing floor tile prefab, removing this gameobject");
            Destroy(this.gameObject);
            return;
        }
        //hold off on data structure creation until after all validations occur (we don't want to waste processor time on structures that won't be used)
        createdRoomsQueue = new Queue(numberOfRooms); //we won't have more than the given number of rooms to create in our Queue
        tappedRoomsStack = new Stack(numberOfRooms);
        if(testingMode)
            Random.seed = testingSeed; //configure random seed for testing purposes
	}


    //@precondition: attached object (this) doesn't have a messed up rotation
    //@params:  roomOrigin -> location in 3d space for room origin (bottom left corner), 
    //          roomSize -> number of tiles to extend right and up from origin, 
    //          pathLength -> number of tiles to create path leading up
    //          direction -> direction that old tile is relative to new tile
    //          connectedRoom -> the room whose "direction" edge may be this room
    //Checks for space occupation in area defined roomSize x roomSize right and up from specified origin position,
    //then if un-occupied, creates tiles there
    //returns a dungeon room representing the location and dimensions of the room, or null if Queue is empty
    dungeon_room TryCreateRoom(Vector3 roomOrigin, int roomSize/*,int pathLength, Entity.MoveDirection direction, dungeon_room connectedRoom*/)
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
                    Debug.Log("Found gameobject at " + collider.transform.position.ToString());
                    Debug.Log("Failed at coordinates " + (roomOrigin.x + i) + "," + (roomOrigin.y + j));
                    return null;
                }
                    
            }
        Debug.Log("Created room successfully");
        for(int i = 0; i < roomSize; i++) //populate room with Tiles
            for(int j = 0; j < roomSize; j++)
            {
                GameObject tile = (GameObject)Instantiate(floorTilePrefab, new Vector3(roomOrigin.x + i, roomOrigin.y + j, roomOrigin.z), transform.rotation);
                if (tileContainer != null)
                    tile.transform.SetParent(tileContainer.transform);
            }
        //build path from room
        return new dungeon_room(roomOrigin.x, roomOrigin.y, roomSize) ;
    }

    //uses Random.Range to cycle the Queue (Enqueue the Dequeued value), then returns dungeon room at front of queue
    public dungeon_room GetRandomRoomFromQueue()
    {
        int cycleCount = Random.Range(0,createdRoomsQueue.Count - 1); //gets a random room from the Queue
        if (createdRoomsQueue.Peek() != null) //if Queue contains elements
        {
            for (int i = 0; i < cycleCount; i++)
                createdRoomsQueue.Enqueue(createdRoomsQueue.Dequeue());
            return (dungeon_room)createdRoomsQueue.Peek();
        }
        else
            return null;
    }
	
    //TODO: implement leaving and returning to main menu
    //handles leaving current scene and returning to main menu, along with any potential cleanup needed
    void GracefullyExit()
    {
        Destroy(this.gameObject);
        return;
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
                dungeon_room newRoom = TryCreateRoom(new Vector3(transform.position.x, transform.position.y, TileMonoBehavior.tileZLayer), newSize);
                if (newRoom != null) //if room successfully created
                {
                    Debug.Log("First room created");
                    createdRoomsQueue.Enqueue(newRoom);
                    count++;
                }
                else
                {
                    Debug.Log("Unable to create first room, so floor must be predefined.");
                    count = numberOfRooms; //forces Generator to stop generating
                    GracefullyExit();
                    return;
                }
            }
            else if(createdRoomsQueue.Peek() != null) //not first element, and there is a next element to grab
            {
                dungeon_room oldRoom = GetRandomRoomFromQueue(); //picks an old room to generate new room relative to
                if(oldRoom == null)
                {
                    Debug.LogError("Unhandled Error! Room queue is Empty! Aborting");
                    count = numberOfRooms;
                    return;
                }
                Debug.Log("Using room at " + oldRoom.getCoords().ToString());
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

                do //try to create room at given direction, if it fails, keep trying with new direction until there are no more directions to pick
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
                                newRoomLocation = new Vector3(oldRoomLocation.x + distance + roomSize, oldRoomLocation.y);
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
                        createdRoomsQueue.Enqueue(newRoom);
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
                        Debug.Log("Creation failed, might try next direction" + direction);
                    }
                        
                }
                while (newRoom != null && direction != originalDirection);
                if(newRoom == null) //we were unable to create a room with any direction, so current old room is invalid, remove it from queue
                {
                    Debug.Log("Failed to create room, finished with room generation");
                    
                    count = numberOfRooms;
                }
                else //successfully created room, prepare for creating next room
                {
                    Debug.Log("Successfully created room");
                    while(tappedRoomsStack.Count != 0) //move all previously invalidated dungeon rooms back into queue again
                    {
                        createdRoomsQueue.Enqueue(tappedRoomsStack.Pop());
                    }
                }
            }
            //reaching here, this is not first room, so there exists at least one room, but we can't make any more rooms, so stop and cleanup
            else //createdRooms.Peek() == 0, so no more rooms to pick from. We can't build dungeon, so give up.
            {
                Debug.Log("Unable to create any more rooms, so we'll have to deal with fewer rooms.");
                count = numberOfRooms; //forces Generator to stop generating
            }
        }
        else //we are finished creating rooms
        {
            Debug.Log("Finished creating dungeon");
            Destroy(this.gameObject);
        }
	}

    void OnDestroy()
    {
        while (createdRoomsQueue.Count != 0)
            createdRoomsQueue.Dequeue();
        while (tappedRoomsStack.Count != 0)
            tappedRoomsStack.Pop();
    }
}
