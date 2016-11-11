using UnityEngine;
using System.Collections.Generic;

//EquipmentOwner holds all of the equipment that given entity can hold and has equipped
//also stores most up-to-date stat changes affecting entity at the moment
public class EquipmentOwner : MonoBehaviour {

    public ChestArmor chestSlot; //ChestArmor equipped in chest slot
    public List<Equipment> carriedEquipment; //the equipment that entity has, but has not equipped
    private static readonly int inventory_Size = 10; //number of items that entity can carry at any given time

    private int defense = 0; //measures total defense from all gear

    public void ChangeDefense(int amount)
    {
        defense += amount;
    }

	// Use this for initialization
	void Start () {
        carriedEquipment = new List<Equipment>(inventory_Size);
        Entity foundEntity = gameObject.GetComponent<Entity>();
        if (foundEntity != null)
            foundEntity.SetTileEnterListener(this.DetectGearPickup);
	}

    //handles checking if gear is on the ground for entity to pick up, if there is, it is added to their inventory
    void DetectGearPickup(TileMonoBehavior tileWithGear)
    {
        if(tileWithGear.IsEquipmentOnTile() && !InventoryIsFull())
        {
            GiveItem(tileWithGear.GrabEquipmentFromTile());
        }
    }

    bool InventoryIsFull()
    {
        return (carriedEquipment.Count >= inventory_Size);
    }
	
    //attempts to store item in entity's inventory
    //returns false if item was not added to inventory, else true
    bool GiveItem(Equipment item)
    {
        if(carriedEquipment.Count >= inventory_Size || item == null)
        {
            return false;
        }
        carriedEquipment.Add(item);
        return true;
    }
    bool EquipItem(ChestArmor chestArmor)
    {
        if (chestArmor.CanEquip(this))
        {
            if (chestSlot != null)
            {
                chestSlot.UnEquip(this);
                carriedEquipment.Add(chestSlot);
            }
            chestSlot = chestArmor;
            chestSlot.Equip(this);
            return true;
        }
        else
            return false;
    }

    //empties out equipment list (Gear should be copied/saved before moving to next level!)
    void OnDestroy()
    {
        int x = carriedEquipment.Count;
        for(int i = 0; i < x; i++)
        {
            carriedEquipment.RemoveAt(0);
        }
    }
}
