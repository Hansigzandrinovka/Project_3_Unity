using UnityEngine;
using System.Collections;

public class dungeon_room {
    //a container for a dungeon room's origin point, size, and connected dungeon rooms
    dungeon_room previousRoom;

    private int size = 1;
    private float xpos, ypos = 0;
    private dungeon_room leftRoom = null;
    private dungeon_room upRoom = null;
    private dungeon_room rightRoom = null;
    private dungeon_room downRoom = null;
    //@params: x -> x location of this room in Unity, y -> y location of this room in Unity, connectedRoom -> the room that connects to this one for navigation, connectionDirection -> direction this room is in relation to connected room
    public dungeon_room(float x, float y, int dimensions/*, dungeon_room connectedRoom, Entity.MoveDirection connectionDirection*/)
    {
        size = dimensions;
        xpos = x;
        ypos = y;
        //Not sure if we want rooms connected to each other
        /*switch(connectionDirection)
        {
            case Entity.MoveDirection.up:
                {
                    downRoom = connectedRoom;
                    connectedRoom.upRoom = this;
                    break;
                }
            case Entity.MoveDirection.down:
                {
                    upRoom = connectedRoom;
                    connectedRoom.downRoom = this;
                    break;
                }
            case Entity.MoveDirection.left:
                {
                    rightRoom = connectedRoom;
                    connectedRoom.leftRoom = this;
                    break;
                }
            case Entity.MoveDirection.right:
                {
                    leftRoom = connectedRoom;
                    connectedRoom.rightRoom = this;
                    break;
                }
        }*/
    }
    //severs this dungeon room's connections to/from it when it is deleted
    ~dungeon_room()
    {
        //removes other rooms' connections to this room so that it/they can leave scope successfully
        if(leftRoom != null)
        leftRoom.rightRoom = null;
        leftRoom = null;
        if(rightRoom != null)
        rightRoom.leftRoom = null;
        rightRoom = null;
        if(upRoom != null)
        upRoom.downRoom = null;
        upRoom = null;
        if(downRoom != null)
        downRoom.upRoom = null;
        downRoom = null;
        //Debug.Log("Destroying a dungeon room");
    }

    //returns size
    public int getSize()
    { return size; }

    //returns a new Vector3 representing center position (upper right of center) tile of the room that the room was built around
    public Vector3 getCoords()
    {
        return new Vector3(xpos, ypos, TileMonoBehavior.tileZLayer);
    }

    //uses Unity's built in RNG to return a new Vector3 with x and y coordinates corresponding to the location of a random tile in the room
    public Vector3 getRandomTileInRoom()
    {
        int leftAmount = -1 * Mathf.FloorToInt(size / 2);
        int rightAmount = Mathf.CeilToInt(size / 2) - 1;
        float tileXPos = Random.Range(rightAmount, leftAmount) + xpos;
        Debug.Log("random x offset " + tileXPos + " from " + xpos);
        float tileYPos = Random.Range(rightAmount, leftAmount) + ypos;
        Debug.Log("random y offset " + tileYPos + " from " + ypos);
        return new Vector3(tileXPos, tileYPos, 0);
    }
}
