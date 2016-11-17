﻿using UnityEngine;
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
    public int getSize()
    { return size; }
    public Vector3 getCoords()
    {
        return new Vector3(xpos, ypos, TileMonoBehavior.tileZLayer);
    }
}