using UnityEngine;
using System.Collections;

public class spellController : MonoBehaviour {
	
	public int damageCaused = 10;
	public TileMonoBehavior occupyingTile; //the tile this entity stands on, and its entry point into moving around on the board
	public enum spellType
	{
		regular,
		fire,
		ice,
		lightning
	};
	public spellType type = spellType.regular;
	public Material defaultMaterial;
	
	void Start()
	{
		defaultMaterial = GetComponent<SpriteRenderer>().material;
	}
	
	// Update is called once per frame
	//Change color of spell based on type
	//Gets the tile beneath the spell and changes tile material based on spell type
	//Spell is destroyed after hitting enemy or going off the board
	void Update () {
		if(type == spellType.regular){
			GetComponent<SpriteRenderer>().color = Color.white;
		} else if (type == spellType.fire){
			GetComponent<SpriteRenderer>().color = Color.red;
			damageCaused = 5;
		} else if (type == spellType.ice){
			GetComponent<SpriteRenderer>().color = Color.blue;
			damageCaused = 5;
		} else{
			GetComponent<SpriteRenderer>().color = Color.yellow;
			damageCaused = 5;
		}
		
		Vector2 topRightLoc = new Vector2(transform.position.x + 0.25f, transform.position.y + 0.25f);
		Vector2 botLeftLoc = new Vector2(transform.position.x - 0.25f, transform.position.y - 0.25f);
		Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask); //test if a floor tile is under this tile, if so then grab it and check what occupies it
		if(collider != null)
		{
            //Debug.Log("Testing floor collision");
			TileMonoBehavior tileBeneathUs = collider.gameObject.GetComponent<TileMonoBehavior>();
			if(tileBeneathUs != null)
			{
				if(!tileBeneathUs.IsWalkable())
				{
					Destroy(this.gameObject);
				}
				else if(tileBeneathUs.IsOccupied())
				{
                    Debug.Log(tileBeneathUs.occupyingEntity + " takes " + damageCaused + " poking damage from the spell");
					tileBeneathUs.occupyingEntity.TakeDamage(damageCaused,Entity.DamageType.poking);
					Destroy(this.gameObject);
				}
				else
				{
					this.occupyingTile = tileBeneathUs;
					if(type == spellType.regular)
					{
						tileBeneathUs.GetComponent<SpriteRenderer>().material = defaultMaterial;
						tileBeneathUs.timeToRevert = 0;
					}
					else if(type == spellType.fire)
					{
						tileBeneathUs.GetComponent<SpriteRenderer>().material = Resources.Load("Materials/Red") as Material;
						if(!tileBeneathUs.inList){
							tileBeneathUs.timeToRevert = DungeonManager.turnOrder.Count*5;
							DungeonManager.AddToTileList(tileBeneathUs);
							tileBeneathUs.inList = true;
						}
					}
					else if(type == spellType.ice)
					{
						tileBeneathUs.GetComponent<SpriteRenderer>().material = Resources.Load("Materials/Blue") as Material;
						if(!tileBeneathUs.inList)
						{
							tileBeneathUs.timeToRevert = DungeonManager.turnOrder.Count*5;
							DungeonManager.AddToTileList(tileBeneathUs);
							tileBeneathUs.inList = true;
						}
					}
					else
					{
						tileBeneathUs.GetComponent<SpriteRenderer>().material = Resources.Load("Materials/Yellow") as Material;
						if(!tileBeneathUs.inList)
						{
							tileBeneathUs.timeToRevert = DungeonManager.turnOrder.Count*5;
							DungeonManager.AddToTileList(tileBeneathUs);
							tileBeneathUs.inList = true;
						}					
					}
				}
			}
			else
			{
                //Debug.Log("Destroying spell by lack of Tile");
				Destroy(this.gameObject);
			}
		}
        else
        {
            //Debug.Log("Destroying spell by lack of Collider");
            Destroy(this.gameObject);
        }
	}
}
