using UnityEngine;
using System.Collections;

public class TileMonoBehavior : MonoBehaviour {

    public Entity occupyingEntity;
    //public GameObject tileImage;
    int rowPosition;
    int colPosition;
    public static int tileZLayer = 0;
    //[SerializeField] //SerializeField allows us to make private variables show up in Inspector for testing and editing.
    private TileMonoBehavior below;
    //[SerializeField]
    private TileMonoBehavior above;
    //[SerializeField]
    private TileMonoBehavior left;
    //[SerializeField]
    private TileMonoBehavior right;
    // Use this for GameObject initialization


    public TileMonoBehavior getLeft()
    { return left; }
    public TileMonoBehavior getRight()
    { return right; }
    public TileMonoBehavior getAbove()
    { return above; }
    public TileMonoBehavior getBelow()
    { return below; }

    //Layer Mask values are integer equivalent of binary arrays, so (1000)v2 is (8)v10, represents true for layer 8, false for all other layers
    private static int tileLayerMask = 1 << 8; //this Layer Mask evaluates on Tiles Layer
    private static int entitiesLayerMask = 1 << 9; //this Layer Mask evaluates on Entities Layer

    public enum TileType { empty, wall };
    public TileType tileType = TileType.empty;

    void Awake()
    {
        Debug.Log("awakened");
    }
    void Start () {
        Debug.Log("Started");
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
                    Debug.Log("Tile at " + transform.position.x + "," + transform.position.y + " found a right Tile");
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
