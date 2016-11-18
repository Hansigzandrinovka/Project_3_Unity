using UnityEngine;
using System.Collections;

public class TestMonsterController : EntityController {

    public Entity player;
	private TileMonoBehavior playerTile;
	//int currentLogicState = 0; //0 is move right, 1 is down, 2 is left, 3 is up
    private float clock = 0; //used to prevent input flooding by slowing user input processing
    public float inputTime = 2; //how many seconds must pass before input can be processed again

    // Use this for initialization
    protected override void Start () {
		base.Start();
	}

    public override void StartTurn()
    {
        base.StartTurn();
	    playerTile = player.occupyingTile;
        clock = 0;
    }
	
	//Takes integer and returns absolute value
	private float absVal(float x)
	{
		if(x < 0)
			return(x*-1);
		else
			return(x);
	}
	
    // Update is called once per frame
    void Update () {
        if(clock >= inputTime && canAct)
        {
		float xDir = playerTile.transform.position.x - puppetEntity.transform.position.x;
		float yDir = playerTile.transform.position.y - puppetEntity.transform.position.y;
            if(xDir > 0 && absVal(yDir) <= absVal(xDir))
	    {
		    puppetEntity.Move(Entity.MoveDirection.right);
	    }
		else if(xDir < 0 && absVal(yDir) <= absVal(xDir))
		{
			puppetEntity.Move(Entity.MoveDirection.left);
		}
            
            if (puppetEntity.GetRemainingSpeed() == 0) //if Entity can't act anymore
            {
                //Debug.Log("Player Entity finished Turn");
                EndTurn();
            }
		
		if(yDir > 0)
		{
			puppetEntity.Move(Entity.MoveDirection.up);
		}
		else if(yDir < 0)
		{
			puppetEntity.Move(Entity.MoveDirection.down);
		}
        }
        else if (canAct)
            clock += Time.deltaTime;
    }
}
