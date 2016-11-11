using UnityEngine;
using System.Collections;

public class Armor : Equipment {

    private int armor = 1; //decrease to damage taken
    private int atkPwr = 0; //increase to melee damage

    //elemental damage bonuses
    private int firePwr = 0;
    private int waterPwr = 0;
    private int earthPwr = 0;
    private int airPwr = 0;

    //elemental damage resistances (like armor against spells)
    private int fireRes = 1;
    private int waterRes = 1;
    private int earthRes = 1;
    private int airRes = 1;

    //applies changes for equipping armor (more armor, less fire res, more water res)
    public override void Equip(EquipmentOwner bonusTarget)
    {
        bonusTarget.ChangeDefense(armor);
    }

    //removes relevant changes when user doffs armor (less armor, more fire res, less water res)
    public override void UnEquip(EquipmentOwner bonusTarget)
    {
        bonusTarget.ChangeDefense(-armor);
    }
}
