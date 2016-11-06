using UnityEngine;
using System.Collections;

public class dungeon_room {
    //a container for a dungeon room's origin point, size, and connected dungeon rooms
    dungeon_room previousRoom;

    private int size = 1;
    private float xpos, ypos = 0;
    public dungeon_room(float x, float y, int dimensions)
    {
        size = dimensions;
        xpos = x;
        ypos = y;
    }
    public int getSize()
    { return size; }
    public Vector3 getCoords()
    {
        return new Vector3(xpos, ypos, TileMonoBehavior.tileZLayer);
    }
}
