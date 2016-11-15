using UnityEngine;
using System.Collections;

public class TileMonoBehavior : MonoBehaviour {

    public Entity occupyingEntity;
    //public GameObject tileImage;
    int rowPosition;
    int colPosition;
    public static int tileZLayer = 0; //used to make all tiles appear on the same layer
    //[SerializeField] //SerializeField allows us to make private variables show up in Inspector for testing and editing.
    private TileMonoBehavior below;
    //[SerializeField]
    private TileMonoBehavior above;
    //[SerializeField]
    private TileMonoBehavior left;
    //[SerializeField]
    private TileMonoBehavior right;
    // Use this for GameObject initialization

    //Layer Mask values are integer equivalent of binary arrays, so (1000)v2 is (8)v10, represents true for layer 8, false for all other layers
    public readonly static int tileLayerMask = 1 << 8; //this Layer Mask evaluates on Tiles Layer
    public readonly static int entitiesLayerMask = 1 << 9; //this Layer Mask evaluates on Entities Layer

    public enum TileType { empty, wall, stairs };
    public TileType tileType = TileType.empty;

    public TileType getTileType()
    { return tileType; }

    //returns the Tile at location relative to this tile, or null if tile could not be found
    //DOES NOT physically move anything in a direction
    public TileMonoBehavior StepInDirection(int colAmount, int rowAmount)
    {
        int x = colAmount;
        int y = rowAmount;
        TileMonoBehavior givenTile = this;
        while (x > 0 && right != null)
        {
            givenTile = right;
            x++;
        }
        while (y > 0 && above != null)
        {
            givenTile = above;
            y++;
        }
        while (x < 0 && left != null)
        {
            givenTile = left;
            x--;
        }
        while (y < 0 && below != null)
        {
            givenTile = below;
            y--;
        }
        return givenTile;
    }

    public TileMonoBehavior getLeft()
    { return left; }
    public TileMonoBehavior getRight()
    { return right; }
    public TileMonoBehavior getAbove()
    { return above; }
    public TileMonoBehavior getBelow()
    { return below; }

    //returns true if an Entity is capable of walking in this tile
    //subject to change if entities have movement types, but that is an easy fix
    public bool IsWalkable()
    {
        switch(tileType)
        {
            case (TileType.empty):
                return true;
            case (TileType.wall):
                return false;
            case (TileType.stairs):
                return true;
            default:
                return false;
        }
    }

    //if an Entity is in this tile, it is occupied, return true
    //else return false
    public bool IsOccupied()
    {
        return occupyingEntity != null;
    }

    

    void Start () {
        //Debug.Log("Started");
        transform.position.Set(transform.position.x, transform.position.y, tileZLayer);
        //get all tiles L,R,U,D relative to this one, and point them together (first acting tile connects both ways, so no need to connect back later, and makes later tiles start faster)
        if(right == null) //if tile hasn't been already defined (because we don't want to define the same tile's same value twice)
            //look for a tile right of this
        {
            Vector2 topRightLoc = new Vector2(transform.position.x + 1.25f, transform.position.y + 0.25f);
            Vector2 botLeftLoc = new Vector2(transform.position.x + 0.75f, transform.position.y - 0.25f);
            Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc,tileLayerMask);
            if (collider != null)
            {
                TileMonoBehavior newRight = collider.gameObject.GetComponent<TileMonoBehavior>();
                if (newRight != null)
                {
                    //Debug.Log("Tile at " + transform.position.x + "," + transform.position.y + " found a right Tile");
                    right = newRight;
                    right.left = this;
                }
            }
        }
        if(left == null) //look for a tile left of this
        {
            Vector2 topRightLoc = new Vector2(transform.position.x - 0.75f, transform.position.y + 0.25f);
            Vector2 botLeftLoc = new Vector2(transform.position.x - 1.25f, transform.position.y - 0.25f);
            Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, tileLayerMask);
            if (collider != null)
            {
                TileMonoBehavior newTile = collider.gameObject.GetComponent<TileMonoBehavior>();
                if (newTile != null)
                {
                    left = newTile;
                    left.right = this;
                }
            }
        }
        if (above == null) //look for a tile above this
        {
            Vector2 topRightLoc = new Vector2(transform.position.x + 0.25f, transform.position.y + 1.25f);
            Vector2 botLeftLoc = new Vector2(transform.position.x - 0.25f, transform.position.y + 0.75f);
            Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, tileLayerMask);
            if (collider != null)
            {
                TileMonoBehavior newTile = collider.gameObject.GetComponent<TileMonoBehavior>();
                if (newTile != null)
                {
                    above = newTile;
                    above.below = this;
                }
            }
        }
        if (below == null) //look for a tile below this
        {
            Vector2 topRightLoc = new Vector2(transform.position.x + 0.25f, transform.position.y - 0.75f);
            Vector2 botLeftLoc = new Vector2(transform.position.x - 0.25f, transform.position.y - 1.25f);
            Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, tileLayerMask);
            if (collider != null)
            {
                TileMonoBehavior newTile = collider.gameObject.GetComponent<TileMonoBehavior>();
                if (newTile != null)
                {
                    below = newTile;
                    below.above = this;
                }
            }
        }
        if(occupyingEntity == null) //if there is no entity already defined, see if there is one standing on top of this tile
        {
            Vector2 topRightLoc = new Vector2(transform.position.x + 0.25f, transform.position.y + 0.25f);
            Vector2 botLeftLoc = new Vector2(transform.position.x - 0.25f, transform.position.y - 0.25f);
            Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, entitiesLayerMask);
            if (collider != null)
            {
                Entity foundEntity = collider.gameObject.GetComponent<Entity>();
                if (foundEntity != null)
                {
                    occupyingEntity = foundEntity;
                    foundEntity.goToTile(this);
                }
            }
        }
    }

    //used after Tile has already been created, connects given Entity to this Tile both ways so that Entity can walk on tile
    public bool ConnectToEntity(Entity connectedEntity)
    {
        if (occupyingEntity == null) //if entity can't exist on tile because another already exists
        {
            occupyingEntity = connectedEntity;
            connectedEntity.goToTile(this);
            return true;
        }
        return false;
    }
    
    //cleans up connections this Tile has to other Tiles
    void DeleteTile()
    {
        below = null;
        above = null;
        left = null;
        right = null;
    }

    //initializes this Tile relative to the tile below and left of it
    //both tiles will be "pointing" (not using pointers) to each other
    public void InitTile(TileMonoBehavior belowTile, TileMonoBehavior leftTile)
    {
        this.below = belowTile;
        this.left = leftTile;
        if (belowTile != null)
            belowTile.above = this;
        if (leftTile != null)
            leftTile.right = this;
    }

    

    // Update is called once per frame
    void Update () {
	
	}
}
