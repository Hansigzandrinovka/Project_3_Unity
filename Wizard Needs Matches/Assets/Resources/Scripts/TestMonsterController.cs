using UnityEngine;
using System.Collections;

public class TestMonsterController : EntityController {

    int currentLogicState = 0; //0 is move right, 1 is down, 2 is left, 3 is up
    private float clock = 0; //used to prevent input flooding by slowing user input processing
    public float inputTime = 2; //how many seconds must pass before input can be processed again

    // Use this for initialization
    protected override void Start () {
		base.Start();
	}


	
	// Update is called once per frame
	void Update () {
        if(clock >= inputTime && canAct)
        {
            switch(currentLogicState)
            {
                case (0): //move right
                    {
                        puppetEntity.Move(Entity.MoveDirection.right);
                        currentLogicState = 1;
                        break;
                    }
                case (1): //move down
                    {
                        puppetEntity.Move(Entity.MoveDirection.down);
                        currentLogicState = 2;
                        break;
                    }
                case (2): //move left
                    {
                        puppetEntity.Move(Entity.MoveDirection.left);
                        currentLogicState = 3;
                        break;
                    }
                case (3): //move right
                    {
                        puppetEntity.Move(Entity.MoveDirection.up);
                        currentLogicState = 0;
                        break;
                    }
            }
            
            if (puppetEntity.GetRemainingSpeed() == 0) //if Entity can't act anymore
            {
                //Debug.Log("Player Entity finished Turn");
                EndTurn();
            }
        }
        else if (canAct)
            clock += Time.deltaTime;
    }
}
