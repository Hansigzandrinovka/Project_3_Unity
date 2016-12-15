using UnityEngine;
using System.Collections;

//http://answers.unity3d.com/questions/37799/timed-intervals.html
public class spellTest : MonoBehaviour
{
	protected Entity puppetEntity;
	protected int health;

	void Start()
	{
		print ("Please wait while tests run. Do not attempt to play game.");
		puppetEntity = this.GetComponent<Entity> ();
		health = puppetEntity.currentHealth;
		// Calls TestCondition after 5 second
		// and repeats every 3 seconds.
		InvokeRepeating ("TestCondition", 5, 3); 
	}

	// Because of InvokeRepeating, this is called every 3 seconds.
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
	void EndTesting()
	{
		if(puppetEntity.currentHealth != health-2)
		{
			print("Character was not damaged by tile. Test failed.");
			CancelInvoke("TestCondition");
			return;
		}
		CancelInvoke ("TestCondition");
		print ("Testing is finished. You passed.");
	}
}

