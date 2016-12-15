using UnityEngine;
using System.Collections;

//http://answers.unity3d.com/questions/37799/timed-intervals.html
public class spellTest : MonoBehaviour
{
	protected Entity puppetEntity;
	protected int health;
    protected int testingMode = -1; 
    //-2 is a failed test, -1 is before testing Terrain status, 0 is during/after testing Terrain status (placing new tile), 2 is testing Monster placement in void, 3 is placing Monster on existing tile, 4 is placing Monster on newly created tile
    protected float delayClock = 0;
    protected int lastTurnOrderCount = 0; //used to track if additional acting entities have been added to scene since last test

    public GameObject floorTilePrefab;
    public GameObject stdMonsterPrefab;

	void Start()
	{
		print ("Please wait while tests run. Do not attempt to play game.");
		puppetEntity = this.GetComponent<Entity> ();
		health = puppetEntity.currentHealth;
		// Calls TestCondition after 5 second
		// and repeats every 3 seconds.
		InvokeRepeating ("TestCondition", 5, 3); 
	}

    /// <summary>
    /// tests adding additional tiles to the gameboard, then using them
    /// </summary>
    void TerrainTesting()
    {
        if(floorTilePrefab == null)
        {
            print("Unable to test terrain because no Tile prefab to use");
            testingMode = -2;
            //perform additional tests?
            return;
        }
        if(stdMonsterPrefab == null)
        {
            print("Unable to test terrain because no Monster to place on it");
            testingMode = -2;
            return;
        }
        switch(testingMode)
        {
            //attempt to place new tile in floor
            //then check if colliding with tile
            //check if tile connections work both ways
            case -1: //place a new tile 5 left of player
                {
                    print("Beginning Terrain testing");
                    testingMode = 0; //prevent repeating of this case
                    print("Attempting to place new tile 5 to the left of player");
                    GameObject newTile = (GameObject)Instantiate(floorTilePrefab, new Vector2(transform.position.x - 5, transform.position.y),transform.rotation);
                    return;
                }
            //attempt to place monster on empty space (left of placed tile), wait briefly, then check monster count, expect 0
            case 0:
                {
                    testingMode = 1;
                    print("Attempting to place a monster in invalid tile-less position, expect it to be destroyed");
                    lastTurnOrderCount = DungeonManager.turnOrder.Count;
                    GameObject newMonster = (GameObject)Instantiate(stdMonsterPrefab, new Vector2(transform.position.x - 6, transform.position.y), transform.rotation);
                    break;
                }
            //attempt to place monster on a tile, wait briefly, then check monster count, expect 1
            case 1:
                {
                    testingMode = 2;
                    print("Attempting to place a monster on pre-existing tile, expect it to exist");
                    lastTurnOrderCount = DungeonManager.turnOrder.Count;
                    GameObject newMonster = (GameObject)Instantiate(stdMonsterPrefab, new Vector2(transform.position.x - 4, transform.position.y), transform.rotation);
                    break;
                }
            case 2:
                {
                    testingMode = 3;
                    print("Attempting to place a monster on newly-created tile, expect it to exist");
                    lastTurnOrderCount = DungeonManager.turnOrder.Count;
                    GameObject newMonster = (GameObject)Instantiate(stdMonsterPrefab, new Vector2(transform.position.x - 5, transform.position.y), transform.rotation);
                    break;
                }
            default:
                {
                    if(testingMode > 2)
                    {
                        testingMode++;
                    }
                    return;
                }
        }
        
        
        
        
        
        //attempt to place monster on new tile, wait briefly, then check monster count, expect 1
    }

	/// Because of InvokeRepeating, this is called every 3 seconds.
    /// Performs testing of conditions based upon player's current rotation
	void TestCondition()
	{
		print ("There are " + DungeonManager.turnOrder.Count + " entities on the board.");
		if (puppetEntity.facing == Entity.MoveDirection.up) {
			TestIceSpellAndRightTurn ();
		} else if (puppetEntity.facing == Entity.MoveDirection.right && DungeonManager.turnOrder.Count == 4) {
			if (puppetEntity.transform.position.x == 4) {
				TestSliding ();
			} else {
				print ("Character casts second ice spell to destroy monster");
				puppetEntity.CastSpell (2);
			}
		} else if (puppetEntity.facing == Entity.MoveDirection.right && DungeonManager.turnOrder.Count == 3) {
			if(puppetEntity.transform.position.x == 4)
			{
				TestFireSpellAndOddTiles ();
			}
			else
			{
				TestLightningSpell ();
			}
		} else if (puppetEntity.facing == Entity.MoveDirection.right && DungeonManager.turnOrder.Count == 2) {
			TestBurningTilesAndRegularSpells ();
		} else if (puppetEntity.facing == Entity.MoveDirection.right && DungeonManager.turnOrder.Count == 1) {
			EndTesting ();
		} else {
			print ("Something was wrong. Test aborted.");
			CancelInvoke ("TestCondition");
			return;
		}
	}

	void TestIceSpellAndRightTurn()
	{
		puppetEntity.Rotate (false);
		if (puppetEntity.facing == Entity.MoveDirection.right) {
			print ("Character rotated to the right");
		} else {
			print ("Character did not rotate to the right. Test failed.");
			CancelInvoke ("TestCondition");
			return;
		}
		puppetEntity.CastSpell (2);
		print("Character casts an ice spell. Monster to the right should not be destroyed. Tiles should turn blue");
	}
	void TestSliding()
	{
		if (puppetEntity.occupyingTile.getRight ().GetComponent<SpriteRenderer> ().sharedMaterial == puppetEntity.occupyingTile.defaultMaterial) {
			print ("Tile color was not changed. Test failed.");
			CancelInvoke ("TestCondition");
			return;
		}
		puppetEntity.goToTile (puppetEntity.occupyingTile.getRight ());
		print("Character slid on ice and should have hit monster.");
	}
	void TestLightningSpell()
	{
		puppetEntity.CastSpell (3);
		print("Character casts a lightning spell. Monster to the right should not be destroyed. Tiles should turn yellow");
		puppetEntity.lastDirection = Entity.MoveDirection.left;
		puppetEntity.goToTile (puppetEntity.occupyingTile.getLeft ());
		print("Character should have slid on ice back to original spot.");
	}
	void TestFireSpellAndOddTiles()
	{
		if (DungeonManager.oddTiles.Count != 3) {
			print ("Affected tiles were not added correctly to oddTiles in DungeonManager. Test failed.");
			CancelInvoke ("TestCondition");
			return;
		}
		puppetEntity.CastSpell (1);
		print ("Player casts a fire spell. Monster to the right should be destroyed. All tiles in the line should be red.");
	}
	void TestBurningTilesAndRegularSpells()
	{
		if (puppetEntity.occupyingTile.getRight ().GetComponent<SpriteRenderer> ().sharedMaterial == puppetEntity.occupyingTile.defaultMaterial) {
			print ("Tile color was not changed. Test failed.");
			CancelInvoke ("TestCondition");
			return;
		}
		puppetEntity.lastDirection = Entity.MoveDirection.right;
		puppetEntity.goToTile (puppetEntity.occupyingTile.getRight ());
		print ("Character stepped on burning tiles. Health should have decreased by two.");
		puppetEntity.CastSpell (0);
		print ("Player casts a regular spell. Last monster to the right should be destroyed. Tiles in front of player should go back to gray.");
	}
    //ends spell-casting testing and begins terrain testing
    void EndTesting()
	{
		if(puppetEntity.currentHealth != health-2)
		{
			print("Character was not damaged by tile. Test failed.");
			CancelInvoke("TestCondition");
			return;
		}
		CancelInvoke ("TestCondition");
		print ("Spell-casting Testing is finished. You passed.");
        testingMode = -1;
        TerrainTesting();
	}

    void Update()
    {
        if(testingMode >= 0) //testing using the clock, but only run terrain testing after initial testing finished
        {
            delayClock += Time.deltaTime;
            if(delayClock >= 1) //1 second has passed
            {
                delayClock = 0;
                switch (testingMode)
                {
                    case 0: //should find new tile 5 left of player, connected to right of it
                        {
                            Vector2 topRightLoc = new Vector2(transform.position.x - 4.75f, transform.position.y + 0.25f);
                            Vector2 botLeftLoc = new Vector2(transform.position.x - 5.25f, transform.position.y - 0.25f);
                            Collider2D collider = Physics2D.OverlapArea(topRightLoc, botLeftLoc, TileMonoBehavior.tileLayerMask);
                            if (collider != null)
                            {
                                print("Found tile at location, success! Checking tile's connections");
                                TileMonoBehavior tileScript = collider.gameObject.GetComponent<TileMonoBehavior>();
                                if(tileScript == null)
                                {
                                    print("ALERT! GameObject we found is not a Tile, aborting tests");
                                    testingMode = -2;
                                    return;
                                }
                                bool testsFailed = false;
                                if (tileScript.getLeft() != null)
                                {
                                    print("ALERT! Found left tile that shouldn't be there");
                                    testsFailed = true;
                                }
                                    
                                if (tileScript.getRight() == null)
                                {
                                    print("ALERT! Found no right tile, but there should be one there...");
                                    testsFailed = true;
                                }
                                    
                                if (tileScript.getAbove() != null)
                                {
                                    print("ALERT! Found above tile that shouldn't be there");
                                    testsFailed = true;
                                }
                                    
                                if (tileScript.getBelow() != null)
                                {
                                    print("ALERT! Found below tile that shouldn't be there");
                                    testsFailed = true;
                                }
                                if(testsFailed)
                                {
                                    print("Tests failed! Aborting");
                                    testingMode = -2;
                                    return;
                                }
                                TerrainTesting(); //now testingMode is 0
                            }
                            break;
                        }
                    case 1: //see if monster still exists in game
                        {
                            if(DungeonManager.turnOrder.Count != lastTurnOrderCount)
                            {
                                print("ALERT! Expected invalid created monster to be deleted, but turn order does not reflect this! Aborting!");
                                print("Turn Order was actually " + DungeonManager.turnOrder.Count);
                                testingMode = -2;
                                return;
                            }
                            print("Entity count remained the same as expected");
                            TerrainTesting();
                            break;
                        }
                    case 2: //see if monster still exists in game
                        {
                            if (DungeonManager.turnOrder.Count != (lastTurnOrderCount + 1))
                            {
                                print("ALERT! Expected created monster to not be deleted, but turn order does not reflect this! Aborting!");
                                print("Turn Order was actually " + DungeonManager.turnOrder.Count);
                                testingMode = -2;
                                return;
                            }
                            print("Entity count went up by 1 as expected");
                            TerrainTesting();
                            break;
                        }
                    case 3: //see if monster still exists in game
                        {
                            if (DungeonManager.turnOrder.Count != (lastTurnOrderCount + 1))
                            {
                                print("ALERT! Expected created monster to not be deleted, but turn order does not reflect this! Aborting!");
                                print("Turn Order was actually " + DungeonManager.turnOrder.Count);
                                testingMode = -2;
                                return;
                            }
                            print("Entity count went up by 1 as expected");
                            TerrainTesting();
                            break;
                        }
                    case 4:
                        {
                            print("finished with Terrain testing!");
                            print("Testing finished!");
                            testingMode = -1; //prevent looping through Update
                            return;
                        }
                    default://no defined testing case to do... "We're done here" (Cave Johnson)
                        {
                            if (testingMode < 5)
                                testingMode++;
                            else
                                DungeonManager.GoToLevel(0);
                            return;
                        }
                }
            }
        }
    }
}

