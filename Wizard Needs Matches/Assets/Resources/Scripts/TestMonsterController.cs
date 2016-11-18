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
	
	//Takes float and returns absolute value
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
            if(absVal(xDir) >= 5 && absVal(yDir) >= 5) //Monster can't see player - sit or move randomly
	    {
		    puppetEntity.speed = 0;
	    }
		else if(absVal(xDir) >= absVal(yDir)) //Player is further away horizontally
		{
			if(!MoveHorizontal(xDir,yDir))
			{
				if(!MoveVertical(xDir,yDir))
				{
					MoveSomewhere();
				}
			}
		}
		else
		{
			if(!MoveVertical(xDir,yDir))
			{
				if(!MoveHorizontal(xDir,yDir))
				{
					MoveSomewhere();
				}
			}
		}
            if (puppetEntity.GetRemainingSpeed() == 0) //if Entity can't act anymore
            {
                EndTurn();
            }
        }
        else if (canAct)
            clock += Time.deltaTime;
    }
	//Player is further away in horizontal direction
	//Return true if monster moved left or right; false otherwise
	private bool MoveHorizontal(float xDir, float yDir)
	{
		if(xDir > 0) //Player is right of monster
		{
			return(puppetEntity.Move(Entity.MoveDirection.right));
		}
		else if(xDir < 0) //Player is left of monster
		{
			return(puppetEntity.Move(Entity.MoveDirection.left));
		}
		return(false);
	}
	//Player is further away in vertical direction
	//Return true if monster moved up or down; false otherwise
	private bool MoveVertical(float xDir, float yDir)
	{
		if(yDir > 0) //Player is in front of monster
		{
			return(puppetEntity.Move(Entity.MoveDirection.up));
		}
		else if(yDir < 0) //Player is behind monster
		{
			return(puppetEntity.Move(Entity.MoveDirection.down));
		}
		return(false);
	}
	//Monster can't move toward player - move somewhere
	//Return true if moved; false otherwise
	private bool MoveSomewhere()
	{
		if(!puppetEntity.Move(Entity.MoveDirection.down))
		{
			if(!puppetEntity.Move(Entity.MoveDirection.up))
			{
				if(!puppetEntity.Move(Entity.MoveDirection.right))
				{
					if(!puppetEntity.Move(Entity.MoveDirection.left))
					{
						puppetEntity.speed = 0;
						return(false);
					}
				}
			}
		}
		return(true);
	}
}
