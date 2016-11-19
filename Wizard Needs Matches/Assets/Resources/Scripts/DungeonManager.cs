using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonManager : MonoBehaviour {

    //public GameObject[] tileset; //holds Prefabs used to tile room
    public static LinkedList<EntityController> turnOrder; //holds all EntityControllers that can act
    //when a controller finishes acting, it tells DungeonManager that its turn is finished, and DungeonManager grabs next actor in Queue and calls its StartTurn method
    //private static int typicalListCapacity = 20;
	
	public static List<TileMonoBehavior> oddTiles; //Contains tiles which have been affected by spells
	
    private EntityController someController;
    public int turnOrderSize = -1;
	public int startDelay = 2; //time in seconds before Turn Order goes into effect

    // Use this for initialization
	void Start () {
        if(turnOrder == null)
            turnOrder = new LinkedList<EntityController>();
		if(oddTiles == null)
			oddTiles = new List<TileMonoBehavior>();
	}
	
	//Initializes list of changed tiles and adds tiles to list when called
	public static void AddToTileList(TileMonoBehavior tile)
	{
		Debug.Log("Adding tiles");
		if(oddTiles == null)
		{
			oddTiles = new List<TileMonoBehavior>();
		}
		oddTiles.Add(tile);
	}
	//Reduces each tile's time in the list and removes tiles with no time left
	public static void RemoveTiles()
	{
		foreach(TileMonoBehavior tile in oddTiles)
		{
			tile.reduceTime();
		}
		oddTiles.RemoveAll(getRevertingTiles);
	}
	//Changes tile back to default material if needed and returns reverting tiles back to RemoveAll function
	private static bool getRevertingTiles(TileMonoBehavior tile)
	{
		if(tile.timeToRevert == 0)
		{
			tile.GetComponent<SpriteRenderer>().material = tile.defaultMaterial;
			tile.inList = false;
		}
		return(tile.timeToRevert == 0);
	}

    //initializes Queue and adds a Controller to it
    public static void AddToTurnOrder(EntityController controllerToAdd)
    {
        //TODO: fancy Turn Order Logic
        if (turnOrder == null) //handles cases where other Objects call "Start()" before this script's Object does
            turnOrder = new LinkedList<EntityController>();
			
		if(controllerToAdd.GoesFirst())
			turnOrder.AddFirst(controllerToAdd);
		else
			turnOrder.AddLast(controllerToAdd);
    }

	//removes given Entity from turn order if able, taking it out of this class's scope, and returning whether or not it was successful
	public static bool RemoveFromTurnOrder(EntityController controller)
	{
		return turnOrder.Remove (controller);
	}

    //precondition: turnEnder is a Queue of EntityControllers, no other objects exist in Queue
    //ends current controller's turn and starts next controller's turn
    public static void EndTurn()
    {
	    RemoveTiles();
        EntityController currentTurnHolder = null;
        if(turnOrder != null && turnOrder.First != null)
            currentTurnHolder = turnOrder.First.Value;
		turnOrder.RemoveFirst ();
        if(currentTurnHolder != null)
        {
            turnOrder.AddLast(currentTurnHolder);
        }
        if(turnOrder.Count > 0) //if there is a next Controller that can act
        {
            currentTurnHolder = (EntityController)turnOrder.First.Value; //doesn't remove Controller from Queue, just notifies it
            Debug.Log("Starting turn for " + currentTurnHolder);
			currentTurnHolder.StartTurn();
        }
    }

    //empties all objects in TurnOrder, dereferencing them
    public static void EmptyTurnOrder()
    {
		Debug.Log ("Emptying Turn Order");
        if(turnOrder != null)
        {
            int x = turnOrder.Count;
            for(int i = 0; i < x; i++) //remove every element in Queue
            {
				turnOrder.RemoveFirst();
            }
        }
    }
	/// <summary>
	/// Tries to make first EntityController start its turn
	/// if turnOrder is empty, print error to console
	/// </summary>
	public void StartTurn()
	{
		Debug.Log ("Starting Turn");
		if(turnOrder.Count > 0)
		{
			//Debug.Log("Turn order starts with " + turnOrder.First.Value);
			turnOrder.First.Value.StartTurn(); //after entity starts its turn, it will eventually tell Dungeon Manager to end turn
		}
		else
			Debug.LogError("Turn Order is empty, can't start turns");
	}
	
	// Update is called once per frame
	void Update () {
        if (turnOrder != null)
            turnOrderSize = turnOrder.Count;
        else
            turnOrderSize = -1;

		if(Input.GetButtonDown("Submit"))
		{
			StartTurn();
		}
	}
}
