using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour { //testtest

    public delegate void HealthChangeListener(int newHealth);
    //delegates define Method Type variables, ie the "return void, take int x" type
    //if a variable is used of the Delegate's type, you can store a method in the variable to call some class's method through this class
    //in this case, I can store a delegate variable as a listener for when this Entity's health changes (as part of TakeDamage()), and whatever method is in the delegate will be called
    private HealthChangeListener uiHealthChangeListener; //the listener associated with the UI, that will notify UI displays for health to update properly
	private MoveDirection lastDirection;

    public delegate void TurnReadyListener(); //the listener associated with Turn Order and deciding what to do, Controllers will 
    public int maxHealth = 10;
    public int currentHealth = 7;
    public int speed = 1; //number of actions entity can perform on a given turn
    protected int remainingSpeed = 1; //number of remaining actions entity can take
    public int delay = 0; //number of turns entity must forfeit after an action before resuming action
    protected int remainingDelay; //number of remaining turns before next action
    public TileMonoBehavior occupyingTile; //the tile this entity stands on, and its entry point into moving around on the board
    public MoveDirection facing = MoveDirection.up; //direction Entity is facing
    public GameObject spriteForFacing;
    public int damageAmount = 1;
    public DamageType attackType = DamageType.poking;
    public GameObject Spell; //the prefab associated with casting a particular spell
    public float castSpeed = 1; //speed with which a projectile travels through the dungeon

    public enum MoveDirection { up = 0, down = 1, left = 2, right = 3 }; //the direction player wishes to move for the purpose of Move function
    public enum DamageType { poking, burn }; //the types of damage that an entity can take for the purpose of the TakeDamage function

    //On Start, Entity attempts to bind itself to a Tile occupying its space
    public virtual void Start()
    {
        if(occupyingTile == null)
        {
            Vector2 topRightLoc = new Vector2(transform.position.x + 0.25f, transform.position.y + 0.25f);
            Vector2 botLeftLoc = new Vector2(transform.position.x - 0.25f, transform.position.y - 0.25f);
            Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask);
            if (collider != null)
            {
                TileMonoBehavior belowTile = collider.gameObject.GetComponent<TileMonoBehavior>();
                if (belowTile != null)
                {
                    Debug.Log("Entity syncing with Tile");
                    if (!belowTile.ConnectToEntity(this)) //if could not connect entity to a tile, we should garbage collect entity because it can't interract with game at all
                    {
                        Destroy(this.gameObject);
                    }
                }
            }
        }
    }

	public int GetRemainingSpeed()
	{
		return remainingSpeed;
	}

    public int SetUIHealthChangeListener(HealthChangeListener x)
    {
        uiHealthChangeListener = x;
        return currentHealth;
    }

    //refresh updates anything that happens at the beginning of the turn for an Entity:
    //ie remaining speed = speed, remainingDelay--, switch sprite to "Awake" state, etc.
    //returns false if Entity cannot act this turn, ie remainingDelay >= 1, Entity is Frozen or otherwise incapable of action
    public bool OnRefresh()
    {
        remainingDelay--;
        if (remainingDelay >= 1)
            return false;
        remainingSpeed = speed;
        return true;
    }

    //updates anything that happens at the end of an Entity's turn: (including when they are unable to act)
    //ie remaining speed = 0, switch sprite to "Asleep" state, deal "Standing in Fire" damage, decrease remaining time on "Shield" buff
    public void OnDeplete()
    {
        remainingSpeed = 0;
    }

    //by default, generic entities do not know any spells
    public virtual bool CastSpell(int spellIndex)
    {
        GameObject clone;
        switch (facing)
        {
            case (MoveDirection.up):
                clone = (GameObject)Instantiate(Spell, transform.position + transform.up, transform.rotation);
                if(spellIndex == 0)
		{
			clone.GetComponent<spellController>().type = spellController.spellType.regular;
		}
		else if(spellIndex == 1)
		{
			clone.GetComponent<spellController>().type = spellController.spellType.fire;
		}		
		else if(spellIndex == 2)
		{
			clone.GetComponent<spellController>().type = spellController.spellType.ice;
		}
		else if(spellIndex == 3)
		{
			clone.GetComponent<spellController>().type = spellController.spellType.lightning;
		}
		clone.GetComponent<Rigidbody2D>().velocity = transform.up * castSpeed;
                break;
            case (MoveDirection.right):
                clone = (GameObject)Instantiate(Spell, transform.position + transform.right, transform.rotation);
			if(spellIndex == 0)
			{
				clone.GetComponent<spellController>().type = spellController.spellType.regular;
			}
			else if(spellIndex == 1)
			{
				clone.GetComponent<spellController>().type = spellController.spellType.fire;
			}
			else if(spellIndex == 2)
			{
				clone.GetComponent<spellController>().type = spellController.spellType.ice;
			}
			else if(spellIndex == 3)
			{
				clone.GetComponent<spellController>().type = spellController.spellType.lightning;
			}
                clone.GetComponent<Rigidbody2D>().velocity = transform.right * castSpeed;
                break;
            case (MoveDirection.left):
                clone = (GameObject)Instantiate(Spell, transform.position - transform.right, transform.rotation);
			if(spellIndex == 0)
			{
				clone.GetComponent<spellController>().type = spellController.spellType.regular;
			}
			else if(spellIndex == 1)
			{
				clone.GetComponent<spellController>().type = spellController.spellType.fire;
			}
			else if(spellIndex == 2)
			{
				clone.GetComponent<spellController>().type = spellController.spellType.ice;
			}
			else if(spellIndex == 3)
			{
				clone.GetComponent<spellController>().type = spellController.spellType.lightning;
			}
                clone.GetComponent<Rigidbody2D>().velocity = transform.right * -1 * castSpeed;
                break;
            default:
                clone = (GameObject)Instantiate(Spell, transform.position - transform.up, transform.rotation);
			if(spellIndex == 0)
			{
				clone.GetComponent<spellController>().type = spellController.spellType.regular;
			}
			else if(spellIndex == 1)
			{
				clone.GetComponent<spellController>().type = spellController.spellType.fire;
			}
			else if(spellIndex == 2)
			{
				clone.GetComponent<spellController>().type = spellController.spellType.ice;
			}
			else if(spellIndex == 3)
			{
				clone.GetComponent<spellController>().type = spellController.spellType.lightning;
			}
                clone.GetComponent<Rigidbody2D>().velocity = transform.up * -1 * castSpeed;
                break;
        }
        remainingSpeed--; //casting a spell takes an action
        return false;
    }

    public bool TakeDamage(int amount, DamageType type)
    {
        Debug.Log(this + " takes " + amount + " " + type + " damage");
        currentHealth -= amount;
        if (uiHealthChangeListener != null)
            uiHealthChangeListener(currentHealth);
        //uiHealthChangeListener?.Invoke(health); //if Listener defined, call it, passing in health
        if (currentHealth <= 0)
        {
            Debug.Log(this + " dies");
            //TODO: make entity explode into money, candy, fireworks, etc.
			Die();
            return true;
        }
        return false;
            
    }

    //tries to move this entity in direction, returns false on fail
    //fail can be because out of movement, or because tile is occupied, or because not this entity's turn
    public virtual bool Move(MoveDirection givenDirection)
    {
	    lastDirection = givenDirection;
        if (remainingSpeed == 0 || remainingDelay > 0)
        {
            Debug.Log("Entity speed is depleted, can't move");
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
            return false;
        }
        if (targetTile.IsWalkable())
        {
            if(!targetTile.IsOccupied())
            {
                goToTile(targetTile);
            }
            else
            {
                targetTile.occupyingEntity.TakeDamage(damageAmount,attackType);
            }
            remainingSpeed--;
            return true;
        }
        else
        {
            return false;
        }  
    }

    //attempts to rotate Entity to face a new direction specified as input
    //returns true if command was successful, false if unable to (ie not enough move)
    public virtual bool Rotate(MoveDirection direction)
    {
        if(remainingSpeed== 0 || remainingDelay > 0)
        {
            Debug.Log("Entity speed is depleted, can't rotate");
            return false;
        }
        else if(spriteForFacing == null)
        {
            Debug.LogError("Missing sprite, can't rotate nonexistent sprite");
            return false;
        }
        else
        {
            switch(direction)
            {
                case (MoveDirection.up):
                    {
                        spriteForFacing.transform.eulerAngles = new Vector3(0, 0, 0);
                        break;
                    }
                case (MoveDirection.left):
                    {
                        spriteForFacing.transform.eulerAngles = new Vector3(0, 0, 90);
                        break;
                    }
                case (MoveDirection.down):
                    {
                        spriteForFacing.transform.eulerAngles = new Vector3(0, 0, 180);
                        break;
                    }
                default:
                    {
                        Debug.Log("Facing Right");
                        spriteForFacing.transform.eulerAngles = new Vector3(0, 0, -90);
                        break;
                    }
            }
            facing = direction;
            //remainingSpeed--; //should turning consume your turn?
            return true;
        }
    }
    public virtual bool Rotate(bool turnLeft)
    {
        switch(facing)
        {
            case (MoveDirection.up):
                {
                    if (turnLeft)
                        return Rotate(MoveDirection.left);
                    return Rotate(MoveDirection.right);
                }
            case (MoveDirection.left):
                {
                    if (turnLeft)
                        return Rotate(MoveDirection.down);
                    return Rotate(MoveDirection.up);
                }
            case (MoveDirection.down):
                if (turnLeft)
                    return Rotate(MoveDirection.right);
                return Rotate(MoveDirection.left);
            default:
                if (turnLeft)
                    return Rotate(MoveDirection.up);
                return Rotate(MoveDirection.down);
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
	    if(occupyingTile.GetComponent<SpriteRenderer>().sharedMaterial == Resources.Load("Materials/Red"))
	    {
		    this.TakeDamage(2,DamageType.burn);
	    }
	    else if(occupyingTile.GetComponent<SpriteRenderer>().sharedMaterial == Resources.Load("Materials/Yellow"))
	    {
		    if(this.speed > 1)
		    {
			    this.speed--;
		    }
	    }
	    else if(occupyingTile.GetComponent<SpriteRenderer>().sharedMaterial == Resources.Load("Materials/Blue"))
	    {
		    while(occupyingTile.GetComponent<SpriteRenderer>().sharedMaterial == Resources.Load("Materials/Blue"))
		    {
			    TileMonoBehavior targetTile;
			    switch(lastDirection)
			    {
				case(MoveDirection.up):
						targetTile = occupyingTile.getAbove();
					    	break;
				case(MoveDirection.down):
					    targetTile = occupyingTile.getBelow();
					    break;
				case(MoveDirection.left):
					    targetTile = occupyingTile.getLeft();
					    break;
				    default:
					    targetTile = occupyingTile.getRight();
					    break;
			    }
			    if(targetTile == null)
			    {
				    break;
			    }
			    if(targetTile.IsWalkable())
			    {
				    if(!targetTile.IsOccupied())
				    {
					    goToTile(targetTile);
				    }
				    else
				    {
					    targetTile.occupyingEntity.TakeDamage(damageAmount,attackType);
				    }
			    }
			    else
			    {
				    break;
			    }
		    }
		    Debug.Log("Finished sliding");
	    }						  
    }
	
	public void Die()
	{
		Destroy(this.gameObject);
	}
	
    // Update is called once per frame
    void Update () {
	
	}
}
