using UnityEngine;
using System.Collections;

public class DungeonGenerator : MonoBehaviour {
    //uses a RNG to create dungeon tiles through rooms connected by hallways

    public int testingSeed = 22; //the seed for the RNG system to predictably produce random tiles
    public GameObject stairsTilePrefab; //the tile used to represent the stairs used to reach the next floor
    public GameObject roomFloorTilePrefab; //the tile used to fill generated rooms
    public GameObject hallwayFloorTilePrefab; //the tile used to fill hallways between rooms
    public GameObject tileContainer; //the gameobject that every tile prefab will be a part of

    public GameObject playerObject; //the gameobject representing the player in the scene
    public GameObject dungeonController; //the gameobject holding turn regulation: ie entity turn order

    public int numberOfRooms = 1; //number of rooms that the generator will attempt to generate on this floor
    public int minRoomSize = 3; //minimum room size any given room will be (ie 3x3 rooms minimum)
    public int maxRoomSize = 5; //maximum room size any given room will be (ie 5x5 rooms maximum)
    public int maxDisplacement = 3; //max distance any two rooms can be from each other
    public int minDisplacement = 1; //min distance any two rooms can be from each other
    public bool testingMode = true; //determines if testing seed is used for RNG

    private int roomCount = 0; //used for counting number of rooms that have been created
    private Queue createdRoomsQueue; //rooms that have been created, but could be invalid to create bordering rooms around
    private Stack tappedRoomsStack; //rooms that have been found invalid to create at

	// Use this for initialization
    //validates that all provided inputs are acceptable,
    //Logs errors to console and attempts to fix bad input errors
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
        if (stairsTilePrefab == null)
        {
            Debug.LogError("Unrecoverable Error! Missing stairs tile prefab, removing this gameobject");
            Destroy(this.gameObject);
            return;
        }
        if (playerObject == null)
        {
            Debug.LogError("Unrecoverable Error! Missing player character prefab, removing this gameobject");
            Destroy(this.gameObject);
            return;
        }
        if (dungeonController == null)
        {
            Debug.LogError("Unrecoverable Error! Missing dungeon controller prefab, removing this gameobject");
            Destroy(this.gameObject);
            return;
        }
        //hold off on data structure creation until after all validations occur (we don't want to waste processor time on structures that won't be used)
        createdRoomsQueue = new Queue(numberOfRooms); //we won't have more than the given number of rooms to create in our Queue
        tappedRoomsStack = new Stack(numberOfRooms);
        if(testingMode)
            Random.seed = testingSeed; //configure random seed for testing purposes
	}


    //@precondition: attached object (this) has default rotation (Vector3: 0,0,0)
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
        for(int i = 0; i < roomSize; i++) //(x coord)
            for(int j = 0; j < roomSize; j++) //for every position in the room, try to place a tile there, if there is a tile, abort, (y coord)
            {
                Vector2 topRightLoc = new Vector2(roomOrigin.x + i + 0.25f - halfSizeOffset, roomOrigin.y + j - halfSizeOffset + 0.25f);
                Vector2 botLeftLoc = new Vector2(roomOrigin.x + i - 0.25f - halfSizeOffset, roomOrigin.y + j - halfSizeOffset - 0.25f);
                Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask); //check if there is a tile there
                if (collider != null) //if a tile already exists here, this whole room is a bust
                {
                    return null;
                }
            }
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
                        Vector2 topRightLoc = new Vector2(roomOrigin.x + 0.25f, roomOrigin.y - halfSizeOffset - i + 0.25f);
                        Vector2 botLeftLoc = new Vector2(roomOrigin.x - 0.25f, roomOrigin.y - halfSizeOffset - i - 0.25f);
                        Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask); //check if there is a tile there
                        if (collider == null) //if no tile there, place a tile there
                        {

                            GameObject tile = (GameObject)Instantiate(roomFloorTilePrefab, new Vector3(roomOrigin.x, roomOrigin.y - i - halfSizeOffset, roomOrigin.z), transform.rotation);
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
                        Vector2 topRightLoc = new Vector2(roomOrigin.x + otherHalfSize + i + 0.25f, roomOrigin.y + 0.25f);
                        Vector2 botLeftLoc = new Vector2(roomOrigin.x + otherHalfSize + i - 0.25f, roomOrigin.y - 0.25f);
                        Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask); //check if there is a tile there
                        if (collider == null) //if no tile there, place a tile there
                        {

                            GameObject tile = (GameObject)Instantiate(roomFloorTilePrefab, new Vector3(roomOrigin.x + i + otherHalfSize, roomOrigin.y, roomOrigin.z), transform.rotation);
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
                        Vector2 topRightLoc = new Vector2(roomOrigin.x + 0.25f, roomOrigin.y + otherHalfSize + i + 0.25f);
                        Vector2 botLeftLoc = new Vector2(roomOrigin.x - 0.25f, roomOrigin.y + otherHalfSize + i - 0.25f);
                        Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask); //check if there is a tile there
                        if (collider == null) //if no tile there, place a tile there
                        {

                            GameObject tile = (GameObject)Instantiate(roomFloorTilePrefab, new Vector3(roomOrigin.x, roomOrigin.y + i + otherHalfSize, roomOrigin.z), transform.rotation);
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
                        Vector2 topRightLoc = new Vector2(roomOrigin.x - i - halfSizeOffset + 0.25f, roomOrigin.y + 0.25f);
                        Vector2 botLeftLoc = new Vector2(roomOrigin.x - i - halfSizeOffset - 0.25f, roomOrigin.y - 0.25f);
                        Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask); //check if there is a tile there
                        if (collider == null) //if no tile there, place a tile there
                        {
                            GameObject tile = (GameObject)Instantiate(roomFloorTilePrefab, new Vector3(roomOrigin.x - i - halfSizeOffset, roomOrigin.y, roomOrigin.z), transform.rotation);
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
        //on each update, pick a random room from list, try to generate a new room relative to that room
        //if no room in list (means we have exhausted all rooms to create around, or we have not created first room), give up on creating room
        //after creating rooms, populate them with a player, a set of stairs, and any relevant entities
        if(roomCount < numberOfRooms) //create numberOfRooms rooms
        {
            if(roomCount == 0) //creating very first room
            {
                int newSize = Random.Range(minRoomSize, maxRoomSize + 1); //size of new room
                dungeon_room newRoom = TryCreateRoom(new Vector3(transform.position.x, transform.position.y, TileMonoBehavior.tileZLayer), newSize,0,Entity.MoveDirection.up);
                if (newRoom != null) //if room successfully created
                {
                    createdRoomsQueue.Enqueue(newRoom);
                    roomCount++;
                }
                else
                {
                    roomCount = numberOfRooms; //forces Generator to stop generating
                    GracefullyExit();
                    return;
                }
            }
            else if(createdRoomsQueue.Peek() != null) //not first element, and there is a next element to grab
            {
                dungeon_room oldRoom = GetRandomRoomFromQueue(); //picks an old room to generate new room relative to
                if(oldRoom == null)
                {
                    roomCount = numberOfRooms;
                    return;
                }
                dungeon_room newRoom = null; //the room we hope to create
                int roomSize = Random.Range(minRoomSize, maxRoomSize + 1); //the size we want the new room to be
                
                float distance = Random.Range(minDisplacement, maxDisplacement + 1);

                Vector3 newRoomLocation = new Vector3();
                Vector3 oldRoomLocation = oldRoom.getCoords();
                Entity.MoveDirection direction = (Entity.MoveDirection)Random.Range(0, 4); //direction can be 0 up, 2 left, 1 down, 3 right
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
                    newRoom = TryCreateRoom(newRoomLocation,roomSize,distance,direction); //try to create the room at specified location
                    if (newRoom != null) //if room creation succeeded, store the room in list for later use
                    {
                        roomCount++;
                        createdRoomsQueue.Enqueue(newRoom);
                    }
                    else if (direction == Entity.MoveDirection.right)
                    {
                        //else keep trying new directions until all exhausted
                        direction = Entity.MoveDirection.up;
                    }
                    else
                    {
                        direction++;
                    }
                }
                while (newRoom == null && direction != originalDirection); //stop iterating when we successfully create a room, or when we reach original direction
                
                if(newRoom != null) //successfully created room, prepare for creating next room
                {
                    while(tappedRoomsStack.Count != 0) //move all previously invalidated dungeon rooms back into queue again
                    {
                        createdRoomsQueue.Enqueue(tappedRoomsStack.Pop());
                    }
                }
            }
            //reaching here, this is not first room, so there exists at least one room, but we can't make any more rooms, so stop and cleanup
            else //createdRooms.Peek() == 0, so no more rooms to pick from. We can't build dungeon, so give up.
            {
                roomCount = numberOfRooms; //forces Generator to stop generating
            }
        }
        else //we are finished creating rooms
        {
            PopulateDungeon();
            Destroy(this.gameObject);
        }
	}

    //populates random dungeon rooms, including placing player character
    void PopulateDungeon()
    {
        //create dungeon controller that all entities will reference

        Instantiate(dungeonController, new Vector3(), transform.rotation); //places dungeon controller first to take advantage of automatic turn order tracking

        //Instantiate(playerObject, GetRandomRoomFromQueue().getCoords(), transform.rotation); //places player character in a random room

        dungeon_room targetRoom = GetRandomRoomFromQueue(); //pick a random room to place the stairs in
        if (createdRoomsQueue.Count >= 2) //if we can have stairs in separate room from Player (if we grab top room, there is another room after for player to go into)
        {
            Debug.Log("Preventing Stairs appearing in player's room");
            tappedRoomsStack.Push(createdRoomsQueue.Dequeue()); //prevent the stairs from appearing in the same room as the player (we already grabbed the stairs room, now we're just keeping it separate from random rooms list)
        }
        //clear the tile that the stairs occupy
        Vector3 stairsPos = targetRoom.getRandomTileInRoom(); //grab a random position in the room, TODO: remove that tile, and place the stairs tile in its place
        Vector2 topRightLoc = new Vector2(stairsPos.x + 0.25f, stairsPos.y + 0.25f);
        Vector2 botLeftLoc = new Vector2(stairsPos.x - 0.25f, stairsPos.y - 0.25f);



        Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask);//build a collider at stairs position to see if there is already a tile there
        if (collider != null)
        {
            Destroy(collider.gameObject);
        }
        //place stairs into the dungeon
        Instantiate(stairsTilePrefab, stairsPos, transform.rotation);
        targetRoom = GetRandomRoomFromQueue();

        //playe player into the dungeon
        Vector3 playerPos = targetRoom.getRandomTileInRoom();
        while (playerPos.x == stairsPos.x && playerPos.y == stairsPos.y) //if player directly overlaps stairs, move player someplace else
            playerPos = targetRoom.getRandomTileInRoom();
        Instantiate(playerObject, playerPos, transform.rotation);

        if (tappedRoomsStack.Count > 0) //shift all of the removed elements back onto the queue, preparation for placing in rest of the dungeon
        {
            createdRoomsQueue.Enqueue(tappedRoomsStack.Pop());
        }
        //TODO: insert spectacular monster/item spawning algorithm here!
    }

    void OnDestroy()
    {
        if(createdRoomsQueue != null)
        while (createdRoomsQueue.Count != 0)
            createdRoomsQueue.Dequeue();
        if(tappedRoomsStack != null)
        while (tappedRoomsStack.Count != 0)
            tappedRoomsStack.Pop();
    }
}
