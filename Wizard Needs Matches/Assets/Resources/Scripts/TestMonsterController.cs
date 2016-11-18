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

    // Update is called once per frame
    void Update () {
        if(clock >= inputTime && canAct)
        {
            if(playerTile.transform.position.x > puppetEntity.transform.position.x)
	    {
		    puppetEntity.Move(Entity.MoveDirection.right);
	    }
		else if(playerTile.transform.position.x < puppetEntity.transform.position.x)
		{
			puppetEntity.Move(Entity.MoveDirection.left);
		}
            
            if (puppetEntity.GetRemainingSpeed() == 0) //if Entity can't act anymore
            {
                //Debug.Log("Player Entity finished Turn");
                EndTurn();
            }
		
		if(playerTile.transform.position.y > puppetEntity.transform.position.y)
		{
			puppetEntity.Move(Entity.MoveDirection.up);
		}
		else if(playerTile.transform.position.y < puppetEntity.transform.position.y)
		{
			puppetEntity.Move(Entity.MoveDirection.down);
		}
        }
        else if (canAct)
            clock += Time.deltaTime;
    }
}
