using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour { //testtest
    public int health = 10;
    public int speed = 1; //number of actions entity can perform on a given turn
    protected int remainingSpeed = 1; //number of remaining actions entity can take
    public int delay = 0; //number of turns entity must forfeit after an action before resuming action
    protected int remainingDelay; //number of remaining turns before next action
    public TileMonoBehavior occupyingTile; //the tile this entity stands on, and its entry point into moving around on the board

    public enum MoveDirection { up, down, left, right }; //the direction player wishes to move for the purpose of Move function
    public enum DamageType { poking }; //the types of damage that an entity can take for the purpose of the TakeDamage function

    //tells the DungeonController this entity is done acting
    public void EndTurn()
    { }

    //by default, generic entities do not know any spells
    public virtual bool CastSpell(int spellIndex)
    {
        return false;
    }

    public bool TakeDamage(int amount, DamageType type)
    {
        health -= amount;
        if (health <= 0)
        {
            //TODO: make entity explode into money, candy, fireworks, etc.
            return true;
        }
        return false;
            
    }

    //tries to move this entity in direction, returns false on fail
    //fail can be because out of movement, or because tile is occupied, or because not this entity's turn
    public virtual bool Move(MoveDirection givenDirection)
    {
        if (remainingSpeed == 0 || remainingDelay > 0)
        {
            Debug.Log("Entity speed is depleted");
            return false;
        }
            
        TileMonoBehavior targetTile;
        switch(givenDirection) //finds tile in given direction, tries to move there
        {
            case (MoveDirection.up):
                targetTile = occupyingTile.getAbove();
                break;
            case (MoveDirection.left):
                targetTile = occupyingTile.getLeft();
                break;
            case (MoveDirection.down):
                targetTile = occupyingTile.getBelow();
                break;
            default:
                targetTile = occupyingTile.getRight();
                break;
        }
        if (targetTile == null)
        {
            //Debug.Log("No Tile Found to move to");
            return false;
        }
            
        //if is occupied, return false, if is not walkable return false
        //otherwise return true
        if (!targetTile.IsOccupied() & targetTile.IsWalkable())
        {
            goToTile(targetTile);
            return true;
        }
        else
        {
            //Debug.Log("Was occupied or not walkable");
            return false;
        }
            
    }

    //moves this Enity to the specified Tile and begins tracking it
    //assumes it is already possible for Entity to move to designated Tile (tile is walkable, not occupied
    public void goToTile(TileMonoBehavior newTile)
    {
        if(occupyingTile != null)
            occupyingTile.occupyingEntity = null; //makes old tile not think Entity is still there
        occupyingTile = newTile;
        occupyingTile.occupyingEntity = this;
        transform.position = new Vector3(occupyingTile.transform.position.x, occupyingTile.transform.position.y, transform.position.z);
    }

    // Update is called once per frame
    void Update () {
	
	}
}
