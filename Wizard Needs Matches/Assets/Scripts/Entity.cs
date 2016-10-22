using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour { //testtest
    public int health = 10;
    public int speed = 1; //number of actions entity can perform on a given turn
    protected int remainingSpeed = 0; //number of remaining actions entity can take
    public TileMonoBehavior occupyingTile; //the tile this entity stands on, and its entry point into moving around on the board

    //moves this Enity to the specified Tile and begins tracking it
    public void goToTile(TileMonoBehavior newTile)
    {
        occupyingTile = newTile;
        occupyingTile.occupyingEntity = this;
        transform.position.Set(occupyingTile.transform.position.x, occupyingTile.transform.position.y, occupyingTile.transform.position.z);
    }

    // Update is called once per frame
    void Update () {
	
	}
}
