using UnityEngine;
using System.Collections;

public class spellController : MonoBehaviour {
	
	public int damageCaused = 10;
	public enum spellType
	{
		regular,
		fire,
		ice,
		lightning
	};
	public spellType type = spellType.regular;
	public Material defaultMaterial;
	public TileMonoBehavior occupyingTile; //the tile this entity stands on, and its entry point into moving around on the board
	
	void Start()
	{
		defaultMaterial = GetComponent<SpriteRenderer>().material;
	}
	// Update is called once per frame
	void Update () {
		if(type == spellType.fire)
		{
			GetComponent<SpriteRenderer>().color = Color.red;
		}
		else if(type == spellType.ice)
		{
			GetComponent<SpriteRenderer>().color = Color.blue;
		}
		else if(type == spellType.lightning)
		{
			GetComponent<SpriteRenderer>().color = Color.yellow;
		}
		else
		{
			GetComponent<SpriteRenderer>().color = Color.white;
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
					tileBeneathUs.occupyingEntity.TakeDamage(damageCaused,Entity.DamageType.poking);
					Destroy(this.gameObject);
				}
				else
				{
					this.occupyingTile = tileBeneathUs;
					if(type == spellType.regular)
					{
						tileBeneathUs.GetComponent<SpriteRenderer>().material = defaultMaterial;
					}
					else if(type == spellType.fire)
					{
						tileBeneathUs.GetComponent<SpriteRenderer>().material = Resources.Load("Materials/Red") as Material;
					}
					else if(type == spellType.ice)
					{
						tileBeneathUs.GetComponent<SpriteRenderer>().material = Resources.Load("Materials/Blue") as Material;
					}
					else
					{
						tileBeneathUs.GetComponent<SpriteRenderer>().material = Resources.Load("Materials/Yellow") as Material;
					}
					//TODO: material changes back to default after a few turns
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
        Debug.Log("after collider check");
	}
}
