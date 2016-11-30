using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonManager : MonoBehaviour {

    //public GameObject[] tileset; //holds Prefabs used to tile room
    public static LinkedList<EntityController> turnOrder; //holds all EntityControllers that can act
    //when a controller finishes acting, it tells DungeonManager that its turn is finished, and DungeonManager grabs next actor in Queue and calls its StartTurn method
    //private static int typicalListCapacity = 20;
	
	public static List<TileMonoBehavior> oddTiles; //Contains tiles which have been affected by spells
    private static int levelNumber = 0; //the initial index to set current level to
    private static bool gameStarted = false;
	public int startDelay = 2; //time in seconds before Turn Order goes into effect
    public static DungeonManager theManager; //allows any entity to quickly and easily access turn order, level changing, etc.

    public bool feedback_game_started = false;

    public static bool IsGameStarted()
    {
        return gameStarted;
    }

    // Use this for initialization
	void Start () {
        theManager = this;
        if(turnOrder == null)
            turnOrder = new LinkedList<EntityController>();
		if(oddTiles == null)
			oddTiles = new List<TileMonoBehavior>();
        levelNumber = Application.loadedLevel; //track what the current level of the game is
    }

    //depending on the provided index, goes to the level named with that index IE: Level_One,Level_Two,Level_Three, etc.
    //defaults to loading Level_One if out of bounds
    public static void GoToLevel(int index)
    {
        Debug.Log("Changing Levels");
        switch (index)
        {
            case 0: //current level is TestLevel
                {
                    Application.LoadLevel("proceduralTesting");
                    return;
                }
            case 1:
                {
                    Application.LoadLevel("proceduralTesting");
                    return;
                }
            case 2:
                {
                    Application.LoadLevel("proceduralTesting");
                    return;
                }
            default:
                {
                    Application.LoadLevel("proceduralTesting");
                    return;
                }
        }
    }

    //precondition: menu level uses index 0 or default case in GoToLevel()
    //calls GoToLevel with given Level number
    public void GoToNextLevel()
    {
        GoToLevel(levelNumber);
    }

    //removes static reference to this object to prevent garbage collection
    //cleanup everything about current level to wait for next level to start
    public void OnDestroy()
    {
        theManager = null;
        gameStarted = false;
        
        //empty out the static turn order to prevent non-existent entities still acting, and to aid in garbage-collecting
        /*Debug.Log("Emptying turn order");
        int x = turnOrder.Count;
        for (int i = 0; i < x; i++)
        {
            turnOrder.RemoveFirst();
        }*/
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
    //used to make an EntityController begin following TurnOrder conventions
    //non-player entities will be added to the back of turn order, players will be added to front
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

    //precondition: if EntityController is at front of list, it is EntityController's turn currently
	//removes given EntityController from turn order if able, taking it out of this class's scope
    //also ends EntityController's turn if it is currently their turn
	public static void RemoveFromTurnOrder(EntityController controller)
	{
        
        if (turnOrder.Count == 0)
            return;
        Debug.Log("Removing a Controller from Turn Order");
        if (turnOrder.First.Value == controller) //if it is current Entity's turn, then end their turn after removing them from turn order
        {
            turnOrder.Remove(controller);
            Debug.Log("Remaining entities: " + turnOrder.Count);
            EndTurn();
            return;
        }
	    turnOrder.Remove (controller);
        Debug.Log("Remaining entities: " + turnOrder.Count);
	}

    //precondition: turnOrder is a Queue of EntityControllers, no other objects exist in Queue
    //ends current controller's turn and starts next controller's turn
    public static void EndTurn()
    {
        if (turnOrder == null || turnOrder.Count == 0) //can't end turn for empty turn order (possible case when closing up everything for level transition)
            return;
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
        ProceduralComponentConnector.DeAllocate();
		Debug.Log ("Starting Turn");
		if(turnOrder.Count > 0)
		{
			//Debug.Log("Turn order starts with " + turnOrder.First.Value);
			turnOrder.First.Value.StartTurn(); //after entity starts its turn, it will eventually tell Dungeon Manager to end turn
            gameStarted = true;
		}
		else
			Debug.LogError("Turn Order is empty, can't start turns");
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Submit") && !gameStarted)
		{
			StartTurn();
		}
        feedback_game_started = gameStarted;
	}
}
