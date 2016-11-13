using UnityEngine;
using System.Collections;

public class DungeonGenerator : MonoBehaviour {
    //uses a RNG to create dungeon tiles through rooms connected by hallways

    public int testingSeed = 22; //the seed for the RNG system to predictably produce random tiles
    public GameObject roomFloorTilePrefab; //the tile used to fill generated rooms
    public GameObject hallwayFloorTilePrefab; //the tile used to fill hallways between rooms
    public GameObject tileContainer; //the gameobject that every tile prefab will be a part of
    public int numberOfRooms = 1;
    public int minRoomSize = 3;
    public int maxRoomSize = 5;
    public int maxDisplacement = 3; //max distance any two rooms can be from each other
    public int minDisplacement = 1; //min distance any two rooms can be from each other
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
        if(roomFloorTilePrefab == null)
        {
            Debug.LogError("Unrecoverable Error! Missing room floor tile prefab, removing this gameobject");
            Destroy(this.gameObject);
            return;
        }
        if (hallwayFloorTilePrefab == null)
        {
            Debug.LogError("Unrecoverable Error! Missing hallway floor tile prefab, removing this gameobject");
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
    //          distanceAway -> distance from old tile that new tile is away, must be POSITIVE, used to populate connecting tiles
    //          direction -> direction that new tile is relative to old tile
    //Checks for space occupation in area defined roomSize x roomSize right and up from specified origin position,
    //then if un-occupied, creates tiles there, afterwards creates hallway in opposite direction
    //returns a dungeon room representing the location and dimensions of the room, or null if Queue is empty
    dungeon_room TryCreateRoom(Vector3 roomOrigin, int roomSize,float distanceAway, Entity.MoveDirection direction)
    {
        int halfSizeOffset = roomSize / 2;
        Debug.Log("Attempting to create room");
        for(int i = 0; i < roomSize; i++) //(x coord)
            for(int j = 0; j < roomSize; j++) //for every position in the room, try to place a tile there, if there is a tile, abort, (y coord)
            {
                Vector2 topRightLoc = new Vector2(roomOrigin.x + i + 0.25f - halfSizeOffset, roomOrigin.y + j - halfSizeOffset + 0.25f);
                Vector2 botLeftLoc = new Vector2(roomOrigin.x + i - 0.25f - halfSizeOffset, roomOrigin.y + j - halfSizeOffset - 0.25f);
                Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask); //check if there is a tile there
                if (collider != null) //if a tile already exists here, this whole room is a bust
                {
                    Debug.Log("Found gameobject at " + collider.transform.position.ToString());
                    Debug.Log("Failed at coordinates " + (roomOrigin.x + i) + "," + (roomOrigin.y + j));
                    return null;
                }
            }
        //Debug.Log("Created room successfully");
        for(int i = 0; i < roomSize; i++) //populate room with Tiles
            for(int j = 0; j < roomSize; j++)
            {
                GameObject tile = (GameObject)Instantiate(roomFloorTilePrefab, new Vector3(roomOrigin.x + i - halfSizeOffset, roomOrigin.y + j - halfSizeOffset, roomOrigin.z), transform.rotation);
                if (tileContainer != null)
                    tile.transform.SetParent(tileContainer.transform);
            }
        //now create the pathway leading away from this room, we don't care what is in the way, only that we fill in spaces that are unoccupied
        //switch(direction), for i = 0; i < distanceAway, i++), try to create tile at position relative to center in direction
        switch(direction)
        {
            case Entity.MoveDirection.up://new room is above old room, so work down from new room
                {
                    for (int i = 0; i <= distanceAway; i++)
                    {
                        Debug.Log("from above, formula is: room origin.y " + roomOrigin.x + ", i " + i + ", halfSizeOffset " + halfSizeOffset);
                        Vector2 topRightLoc = new Vector2(roomOrigin.x + 0.25f, roomOrigin.y - halfSizeOffset - i + 0.25f);
                        Vector2 botLeftLoc = new Vector2(roomOrigin.x - 0.25f, roomOrigin.y - halfSizeOffset - i - 0.25f);
                        Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask); //check if there is a tile there
                        if (collider == null) //if no tile there, place a tile there
                        {

                            GameObject tile = (GameObject)Instantiate(roomFloorTilePrefab, new Vector3(roomOrigin.x, roomOrigin.y - i - halfSizeOffset, roomOrigin.z), transform.rotation);
                            Debug.Log("Connecting hallway: " + roomOrigin.x + ',' + (roomOrigin.y - i - halfSizeOffset) + " on iteration " + i);
                            if (tileContainer != null)
                                tile.transform.SetParent(tileContainer.transform);
                        }
                    }
                    break;
                }
            case Entity.MoveDirection.left: //new room is left of old room, so work right
                {
                    int otherHalfSize = roomSize - halfSizeOffset;
                    for (int i = 0; i <= distanceAway; i++)
                    {
                        Debug.Log("from left, formula is: room origin.x " + roomOrigin.x + ", i " + i + ", otherHalfSize " + otherHalfSize);
                        Vector2 topRightLoc = new Vector2(roomOrigin.x + otherHalfSize + i + 0.25f, roomOrigin.y + 0.25f);
                        Vector2 botLeftLoc = new Vector2(roomOrigin.x + otherHalfSize + i - 0.25f, roomOrigin.y - 0.25f);
                        Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask); //check if there is a tile there
                        if (collider == null) //if no tile there, place a tile there
                        {

                            GameObject tile = (GameObject)Instantiate(roomFloorTilePrefab, new Vector3(roomOrigin.x + i + otherHalfSize, roomOrigin.y, roomOrigin.z), transform.rotation);
                            Debug.Log("Connecting hallway: " + (roomOrigin.x + i + otherHalfSize) + ',' + roomOrigin.y + " on iteration " + i);
                            if (tileContainer != null)
                                tile.transform.SetParent(tileContainer.transform);
                        }
                    }
                    break;
                }
            case Entity.MoveDirection.down: //new room is beneath old room, so work up
                {
                    int otherHalfSize = roomSize - halfSizeOffset;
                    for (int i = 0; i <= distanceAway; i++)
                    {
                        Debug.Log("from down, formula is: room origin.y " + roomOrigin.y + ", i " + i + ", otherHalfSize " + otherHalfSize);
                        Vector2 topRightLoc = new Vector2(roomOrigin.x + 0.25f, roomOrigin.y + otherHalfSize + i + 0.25f);
                        Vector2 botLeftLoc = new Vector2(roomOrigin.x - 0.25f, roomOrigin.y + otherHalfSize + i - 0.25f);
                        Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask); //check if there is a tile there
                        if (collider == null) //if no tile there, place a tile there
                        {

                            GameObject tile = (GameObject)Instantiate(roomFloorTilePrefab, new Vector3(roomOrigin.x, roomOrigin.y + i + otherHalfSize, roomOrigin.z), transform.rotation);
                            Debug.Log("Connecting hallway: " + roomOrigin.x + ',' + (roomOrigin.y + i + otherHalfSize) + " on iteration " + i);
                            if (tileContainer != null)
                                tile.transform.SetParent(tileContainer.transform);
                        }
                    }
                    break;
                }
            case Entity.MoveDirection.right: //new room is right of old room, so work left
                {
                    for (int i = 0; i <= distanceAway; i++)
                    {
                        Debug.Log("from right, formula is: room origin.x " + roomOrigin.x + ", i " + i + ", halfSizeOffset " + halfSizeOffset);
                        Vector2 topRightLoc = new Vector2(roomOrigin.x - i - halfSizeOffset + 0.25f, roomOrigin.y + 0.25f);
                        Vector2 botLeftLoc = new Vector2(roomOrigin.x - i - halfSizeOffset - 0.25f, roomOrigin.y - 0.25f);
                        Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask); //check if there is a tile there
                        if (collider == null) //if no tile there, place a tile there
                        {
                            GameObject tile = (GameObject)Instantiate(roomFloorTilePrefab, new Vector3(roomOrigin.x - i - halfSizeOffset, roomOrigin.y, roomOrigin.z), transform.rotation);
                            Debug.Log("Connecting hallway: " + (roomOrigin.x - i - halfSizeOffset) + ',' + roomOrigin.y + " on iteration " + i);
                            if (tileContainer != null)
                                tile.transform.SetParent(tileContainer.transform);
                        }
                    }
                    break;
                }
        }
        //build path from room
        return new dungeon_room(roomOrigin.x, roomOrigin.y, roomSize) ;
    }

    //Can return null!
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
                dungeon_room newRoom = TryCreateRoom(new Vector3(transform.position.x, transform.position.y, TileMonoBehavior.tileZLayer), newSize,0,Entity.MoveDirection.up);
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
                Debug.Log("Using room at " + oldRoom.getCoords().ToString() + " of size " + oldRoom.getSize());
                dungeon_room newRoom = null; //the room we hope to create
                int roomSize = Random.Range(minRoomSize, maxRoomSize + 1); //the size we want the new room to be
                Debug.Log("new room size picked to be " + roomSize);
                
                float distance = Random.Range(minDisplacement, maxDisplacement + 1);
                Debug.Log("Distance away " + distance);

                Vector3 newRoomLocation = new Vector3();
                Vector3 oldRoomLocation = oldRoom.getCoords();
                Entity.MoveDirection direction = (Entity.MoveDirection)Random.Range(0, 4); //direction can be 0 up, 2 left, 1 down, 3 right
                Debug.Log("Picked direction " + direction);
                Entity.MoveDirection originalDirection = direction;

                do //try to create room at given direction, if it fails, keep trying with new direction until there are no more directions to pick
                {
                    switch (direction)
                    {
                        case Entity.MoveDirection.up: //up
                            {
                                float relativePosition = Mathf.Ceil(oldRoom.getSize() / 2f) + distance + Mathf.Floor(roomSize / 2f);
                                newRoomLocation = new Vector3(oldRoomLocation.x, oldRoomLocation.y + relativePosition, 0);
                                break;
                            }
                        case Entity.MoveDirection.left: //left
                            {
                                float relativePosition = Mathf.Floor(oldRoom.getSize() / 2f) + distance + Mathf.Ceil(roomSize / 2f);
                                newRoomLocation = new Vector3(oldRoomLocation.x - relativePosition, oldRoomLocation.y, 0);
                                break;
                            }
                        case Entity.MoveDirection.down: //down
                            {
                                float relativePosition = Mathf.Floor(oldRoom.getSize() / 2f) + distance + Mathf.Ceil(roomSize / 2f);
                                Debug.Log("DEBUG LOG: " + relativePosition);
                                Debug.Log("DEBUG LOG 2: " + Mathf.Ceil(roomSize / 2f));
                                newRoomLocation = new Vector3(oldRoomLocation.x, oldRoomLocation.y - relativePosition, 0);
                                break;
                            }
                        case Entity.MoveDirection.right: //right
                            {
                                float relativePosition = Mathf.Ceil(oldRoom.getSize() / 2f) + distance + Mathf.Floor(roomSize / 2f);
                                //if size % 2 = 0, even, then nudge 1 back
                                newRoomLocation = new Vector3(oldRoomLocation.x + relativePosition, oldRoomLocation.y);
                                break;
                            }
                    }
                    //default case, it tries to initialize room at 0,0,0
                    Debug.Log("Creating next room at " + newRoomLocation.x + ',' + newRoomLocation.y + ',' + newRoomLocation.z + " with size " + roomSize);
                    newRoom = TryCreateRoom(newRoomLocation,roomSize,distance,direction); //try to create the room at specified location
                    if (newRoom != null) //if room creation succeeded, store the room in list for later use
                    {
                        count++;
                        Debug.Log("New room created");
                        createdRoomsQueue.Enqueue(newRoom);
                    }
                    else if (direction == Entity.MoveDirection.right)
                    {
                        //else keep trying new directions until all exhausted
                        direction = Entity.MoveDirection.up;
                        Debug.Log("Creation right failed, trying up");
                    }
                    else
                    {
                        direction++;
                        Debug.Log("Creation failed, might try next direction " + direction);
                    }
                }
                while (newRoom == null && direction != originalDirection); //stop iterating when we successfully create a room, or when we reach original direction
                
                if(newRoom != null) //successfully created room, prepare for creating next room
                {
                    //Debug.Log("Successfully created room");
                    while(tappedRoomsStack.Count != 0) //move all previously invalidated dungeon rooms back into queue again
                    {
                        createdRoomsQueue.Enqueue(tappedRoomsStack.Pop());
                    }
                }
                else //if we were unable to create a room with any direction, so current old room is invalid, remove it from queue
                {
                    Debug.Log("Failed to create room, finished with room generation");
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
