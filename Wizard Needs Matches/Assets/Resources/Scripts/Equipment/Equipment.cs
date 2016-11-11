using UnityEngine;
using System.Collections;

//Equipment is anything that could be equipped (weapons, armor, consumables, etc. -> anything that would go in an inventory)
public abstract class Equipment : MonoBehaviour {

    //here would go logic to determine if gear was equippable by user, eg user too low level
    //by default, Equipment is unequippable (you can't equip consumables, for example)
    public virtual bool CanEquip(EquipmentOwner equipper)
    {
        return true;
    }

    //handle changing statistics for the owner when equipped/unequipped, ie gain +2 armor on equip, lose -2 armor on unequip
    public abstract void Equip(EquipmentOwner bonusTarget);
    public abstract void UnEquip(EquipmentOwner bonusTarget);
}
