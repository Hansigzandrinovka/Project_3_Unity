using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TestPlayerController : EntityController {
    
    public Slider playerHealthSlider; //the Slider that will store Player's current health
	public Slider playerEnergySlider; //the Slider that will store Player's current Energy
    private float clock = 0; //used to prevent input flooding by slowing user input processing
    public float inputTime = 2; //how many seconds must pass before input can be processed again
	public int energy = 150; //current energy that player can allocate to actions
	public int maxEnergy = 200; //maximum energy that player can store

	public override bool GoesFirst() //Players have higher priority when placed into Turn Order
	{
		return true;
	}
	
    // Use this for initialization
    //overrides virtual Start() in EntityController
    protected override void Start () {
		base.Start ();
        if (inputTime <= 0)
        {
            Debug.LogError("Player Controller Input Time initialized to bad value " + inputTime + ", now setting to 2");
            inputTime = 2;
        }
        if(puppetEntity != null)
        {
			if(playerHealthSlider != null)
			{
				playerHealthSlider.value = puppetEntity.SetUIHealthChangeListener(UpdateDisplayHealth);
				playerHealthSlider.maxValue = puppetEntity.maxHealth;
			}
			if(playerEnergySlider != null)
			{
				playerEnergySlider.value = energy;
				playerEnergySlider.maxValue = maxEnergy;
			}
        }
	}

    void UpdateDisplayHealth(int newHealth)
    {
        if (playerHealthSlider != null)
            playerHealthSlider.value = newHealth;
    }

	//decreases energy to at or above 0
	//assumes amount is positive
	void DecreaseEnergy(int amount)
	{
		energy -= amount;
		if (energy < 0)
			energy = 0;
		if (playerEnergySlider != null)
			playerEnergySlider.value = energy;
	}
	//increases energy to maximum
	//assumes amount is positive
	//if overflow is true, increases energy beyond maximum
	public void IncreaseEnergy(int amount, bool overflow)
	{
		energy += amount;
		if (!overflow && energy >= maxEnergy)
			energy = maxEnergy;
	}
	
	// Update is called once per frame
	//if it is Controller's turn, waits for player input to control Entity
	void Update () {
        if (clock >= inputTime && canAct) //only evaluates player activity every time increment, but only if player can act
        {
            float xAxis = Input.GetAxis("Horizontal");
            float yAxis = Input.GetAxis("Vertical");
            bool turnLeftButton = Input.GetButton("RotateLeft");
            bool turnRightButton = Input.GetButton("RotateRight");

            if (xAxis > 0.5) //user wants to go Right
            {
                //Debug.Log("Player tries to move right");
                puppetEntity.Move(Entity.MoveDirection.right);
                clock = 0;
            }
            else if (xAxis < -0.5) //player tries to move left
            {
                puppetEntity.Move(Entity.MoveDirection.left);
                clock = 0;
            }
                
            else if (yAxis > 0.5)
            {
                clock = 0;
                puppetEntity.Move(Entity.MoveDirection.up);
            } 
            else if (yAxis < -0.5)
            {
                puppetEntity.Move(Entity.MoveDirection.down);
                clock = 0;
            }
            else if(turnLeftButton) //user rotating left to face a new direction
            {
                Debug.Log("Turning left");
                puppetEntity.Rotate(true);
                clock = 0;
            }
            else if(turnRightButton) //user rotating right to face a new direction
            {
//                Debug.Log("Turning right");
                puppetEntity.Rotate(false);
                clock = 0;
            }

			if(puppetEntity.GetRemainingSpeed() == 0) //if Entity can't act anymore
			{
				//Debug.Log("Player Entity finished Turn");
				EndTurn();
			}
        }
        else if(canAct)
            clock += Time.deltaTime;
	}
    override protected void OnDestroy()
    {
        base.OnDestroy();
    }
}
