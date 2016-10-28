using UnityEngine;
using System.Collections;

public class DungeonManager : MonoBehaviour {

    //public GameObject[] tileset; //holds Prefabs used to tile room
    public static LinkedList<EntityController> turnOrder; //holds all EntityControllers that can act
    //when a controller finishes acting, it tells DungeonManager that its turn is finished, and DungeonManager grabs next actor in Queue and calls its StartTurn method
    //private static int typicalListCapacity = 20;
    private EntityController someController;
    public int turnOrderSize = -1;

    // Use this for initialization
	void Start () {
        if(turnOrder == null)
            turnOrder = new LinkedList<EntityController>();
        //turnOrder.Enqueue(someController);
	}

    //initializes Queue and adds a Controller to it
    public static void AddToTurnOrder(EntityController controllerToAdd)
    {
        //TODO: fancy Turn Order Logic
        if (turnOrder == null) //handles cases where other Objects call "Start()" before this script's Object does
            turnOrder = new LinkedList();
			
		if(controllerToAdd.GoesFirst())
			turnOrder.AddFirst(controllerToAdd);
		else
			turnOrder.AddLast(controllerToAdd);
    }

    //precondition: turnEnder is a Queue of EntityControllers, no other objects exist in Queue
    //ends current controller's turn and starts next controller's turn
    public static void EndTurn()
    {
        EntityController currentTurnHolder = (EntityController)turnOrder.Dequeue();
        if(currentTurnHolder != null)
        {
            turnOrder.Enqueue(currentTurnHolder);
        }
        if(turnOrder.Count > 0) //if there is a next Controller that can act
        {
            currentTurnHolder = (EntityController)turnOrder.Peek(); //doesn't remove Controller from Queue, just notifies it
            //currentTurnHolder.Act();
        }
    }

    //empties all objects in TurnOrder, dereferencing them
    public static void EmptyTurnOrder()
    {
        if(turnOrder != null)
        {
            int x = turnOrder.Count;
            for(int i = 0; i < x; i++) //remove every element in Queue
            {
                turnOrder.Dequeue();
            }
        }
        
    }
	
	// Update is called once per frame
	void Update () {
        if (turnOrder != null)
            queueSize = turnOrder.Count;
        else
            queueSize = -1;
	}
}
