using UnityEngine;
using System.Collections;

public class EntityController : MonoBehaviour {
    //is capable of ending its turn and performing actions on its Entity
    //to be subclassed by Player or AI Controllers

    protected Entity puppetEntity;
    protected bool canAct = false;
	//public int initiative = 1;
	
	//tells DungeonManager if this Controller has priority when being placed into Turn Order (IE is player)
	public virtual bool GoesFirst()
	{
		return false;
	}

    //called when 
    protected virtual void EndTurn()
    {
        if(canAct == true) //only allow currently acting Controllers to end the turn
        {
            //do cleanup stuff
            canAct = false;
            puppetEntity.OnDeplete();

            //notify DungeonManager this Controller is done
            DungeonManager.EndTurn();
        }
    }
	
	public int getInitiative()
	{
		return this.initiative;
	}
	
    protected virtual void StartTurn()
    {
        canAct = true;
        if(!puppetEntity.OnRefresh()) //try to awaken the Entity. If it can't be wakened, end current turn
            EndTurn();
    }

    protected virtual void Start()
    {
        //add this Entity to the DungeonManager's Queue
        DungeonManager.AddToTurnOrder(this);
    }
}
