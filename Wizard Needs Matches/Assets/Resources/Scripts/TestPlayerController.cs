using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TestPlayerController : EntityController {
    
    public Slider playerHealthSlider; //the Slider that will provide feedback on Player's current health
	public Slider playerEnergySlider; //the Slider that will provide feedback on Player's current Energy
    public Slider playerTimeSlider; //the Slider that will provide feedback on Player's remaining time to take a turn
    private float clock = 0; //used to prevent input flooding by slowing user input processing
    public float inputTime = 2; //how many seconds must pass before input can be processed again
    public float remainingTime = Player_Turn_WaitTime_Base;
	public int energy = 150; //current energy that player can allocate to actions
	public int maxEnergy = 200; //maximum energy that player can store
    private float maxRemainingTime = Player_Turn_WaitTime_Base;

    //constants for spending Player Energy
    private static readonly int EnergyCost_Move = 30; //cost in energy to move in any direction
    private static readonly int EnergyCost_Attack = 60; //cost in energy to make an attack against an adjacent enemy
    private static int EnergyCost_Cast_Spell_1 = 150; //cost in energy to cast the spell bound to 1 key
    private static int EnergyCost_Cast_Spell_2 = 200;
    private static int EnergyCost_Cast_Spell_3 = 250;
    private static int EnergyCost_Cast_Spell_4 = 280;

    private static readonly float Player_Turn_WaitTime_Base = 20f; //the base time the game will wait for the player to take their turn before making player forfeit their turn
    private static readonly float Player_Turn_WaitTime_Speed_Increment = 10; //the amount of additional time player gets to plan because they have higher speed (IE speed 2 gives 30 = 20 + 10*1 seconds)

	public override bool GoesFirst() //Players have higher priority when placed into Turn Order
	{
		return true;
	}

    //returns true if Controller was capable of spending energy (which they did spend)
    //else returns false, and energy is unchanged
    public bool TrySpendEnergy(int spentEnergy)
    {
        if (energy > spentEnergy)
        {
            energy -= spentEnergy;
            playerEnergySlider.value = energy;
            return true;
        }
        else
            return false;
    }

    //overrides EntityController.StartTurn() to initialize the clock for player input and the player's remaining time
    public override void StartTurn()
    {
        base.StartTurn();
        clock = 0;
        remainingTime = maxRemainingTime;
    }

    //assumes provided objects are not null
    //used for post-Start() GUI connections (or reconnections?)
    public void ConfigureGUIConnections(Slider healthSlider,Slider energySlider,Slider timeSlider)
    {
        playerHealthSlider = healthSlider;
        playerEnergySlider = energySlider;
        playerTimeSlider = timeSlider;
        if(puppetEntity != null)
        {
            playerHealthSlider.value = puppetEntity.SetUIHealthChangeListener(UpdateDisplayHealth);
            playerHealthSlider.maxValue = puppetEntity.maxHealth;
            playerEnergySlider.value = energy;
            playerEnergySlider.maxValue = maxEnergy;
            playerTimeSlider.value = remainingTime;
            playerTimeSlider.maxValue = maxRemainingTime;
        }
    }

    // Use this for initialization
    //overrides virtual Start() in EntityController
    protected override void Start () {
		base.Start ();
        ProceduralComponentConnector.AllocateGUICameraListener(this);
        ProceduralComponentConnector.AllocatePlayerGUILink(this);
        if (inputTime <= 0)
        {
            Debug.LogError("Player Controller Input Time initialized to bad value " + inputTime + ", now setting to 2");
            inputTime = 2;
        }
        if(puppetEntity != null)
        {
            maxRemainingTime = Player_Turn_WaitTime_Base + ((puppetEntity.speed - 1) * Player_Turn_WaitTime_Speed_Increment);
            remainingTime = maxRemainingTime;
            bool needToPrepProcedural = false; //if we don't have a health,energy,time slider, notify component connector
			if(playerHealthSlider != null)
			{
				playerHealthSlider.value = puppetEntity.SetUIHealthChangeListener(UpdateDisplayHealth);
				playerHealthSlider.maxValue = puppetEntity.maxHealth;
			}
            else
            {
                Debug.LogError("Alert! Player Health Slider not configured!");
            }
			if(playerEnergySlider != null)
			{
				playerEnergySlider.value = energy;
				playerEnergySlider.maxValue = maxEnergy;
			}
            else
            {
                Debug.LogError("Alert! Player Energy Slider not configured!");
            }
            if(playerTimeSlider != null)
            {
                playerTimeSlider.value = remainingTime;
                playerTimeSlider.maxValue = maxRemainingTime;
            }
            else
            {
                Debug.LogError("Alert! Player Time Slider not configured!");
            }
        }
        else
        {
            Debug.LogError("Alert! TestPlayerController is missing a puppetEntity to control!");
        }
        if(Board.theMatchingBoard != null)
        {
            Board.theMatchingBoard.SetScoreIncreaseListener(this.ReceiveMatchPoints);
        }
        else
        {
            Debug.LogError("Alert! " + this + " was unable to sync with static Board instance to configure Score change listening!");
        }
	}

    //called whenever attached Entity's health changes
    //used to update Player's health display and handle player dying
    void UpdateDisplayHealth(int newHealth)
    {
        if (playerHealthSlider != null)
            playerHealthSlider.value = newHealth;

        if(newHealth <= 0)
        {
            Debug.Log("Player has died :(");
            DungeonManager.GoToLevel(0); //returns player to the main menu
        }
    }

    //used as a Listener to Board
    //increases Energy of Controller towards maximum,
    //increases remaining time,
    //and updates display with new energy
    public void ReceiveMatchPoints(int amountToChange)
    {
        IncreaseEnergy(amountToChange, false);
        if(canAct) //if it is this player's turn, so time is ticking down, so give them more time
            IncreaseRemainingTime(Mathf.Ceil(amountToChange / Board.averageScore),false);
    }
	//increases energy
    //assumes amount is positive
	//if overflow is true, increases energy beyond maximum, else energy cannot be greater than maximum
	public void IncreaseEnergy(int amount, bool overflow)
	{
		energy += amount;
		if (!overflow && energy >= maxEnergy)
			energy = maxEnergy;
        if (playerEnergySlider != null)
            playerEnergySlider.value = energy;
    }

    //reduces remaining time for Controller if player can act,
    //and updates the time display accordingly
    //there is a possibility of external influences decreasing remaining time, IE ailments that make time move faster
    public void DecreaseRemainingTime(float amount)
    {
        remainingTime -= amount;
        if (playerTimeSlider != null)
            playerTimeSlider.value = Mathf.Ceil(remainingTime);
    }
    public void IncreaseRemainingTime(float amount, bool overflow)
    {
        remainingTime += amount;
        if ((remainingTime > maxRemainingTime) && !overflow) //if player is not allowed to have beyond-max energy:
            remainingTime = maxRemainingTime;
        if (playerTimeSlider != null)
            playerTimeSlider.value = Mathf.Ceil(remainingTime);
    }
	
	// Update is called once per frame
	//if it is Controller's turn, waits for player input to control Entity
	void Update () {
        if (clock >= inputTime && canAct) //only evaluates player activity every time increment, but only if player can act
        {
            DecreaseRemainingTime(Time.deltaTime); //player now has less time to act

            float xAxis = Input.GetAxis("Horizontal");
            float yAxis = Input.GetAxis("Vertical");
            bool turnLeftButton = Input.GetButton("RotateLeft");
            bool turnRightButton = Input.GetButton("RotateRight");
            bool castRegSpell = Input.GetButton("Fire1");
		    bool castFireSpell = Input.GetButton("FireSp");
		    bool castIceSpell = Input.GetButton("IceSp");
		    bool castLightningSpell = Input.GetButton("LightSp");

            if (xAxis > 0.5) //user wants to go Right
            {

                //if player will attack in direction,
                if(puppetEntity.WillAttack(Entity.MoveDirection.right))
                {
                    // then Try to spend energy on attacking in direction
                    if (TrySpendEnergy(EnergyCost_Attack))
                        puppetEntity.Move(Entity.MoveDirection.right);
                    else //otw, player can't attack in direction, so notify player somehow
                    {
                    }
                } //entity won't attack in direction, try moving
                else if(TrySpendEnergy(EnergyCost_Move)) //try to spend energy on moving
                {
                    //player has enough energy
                    //try to move right. On fail, give player their energy back
                    if(!puppetEntity.Move(Entity.MoveDirection.right))
                    {
                        IncreaseEnergy(EnergyCost_Move, false);
                    }
                    //on success, player may be standing on stairs
                    else if(puppetEntity.occupyingTile.tileType == TileMonoBehavior.TileType.stairs)
                    {
                        PromptForStairs();
                    }
                }
                clock = 0;
            }
            else if (xAxis < -0.5) //player tries to move left
            {
                //if player will attack in direction,
                if (puppetEntity.WillAttack(Entity.MoveDirection.left))
                {
                    // then Try to spend energy on attacking in direction
                    if (TrySpendEnergy(EnergyCost_Attack))
                        puppetEntity.Move(Entity.MoveDirection.left);
                    else //otw, player doesn't have enough energy, so notify player somehow
                    {
                    }
                } //entity won't attack in direction, try moving
                else if (TrySpendEnergy(EnergyCost_Move)) //try to spend energy on moving
                {
                    //player has enough energy
                    //try to move right. On fail, give player their energy back
                    if (!puppetEntity.Move(Entity.MoveDirection.left))
                    {
                        IncreaseEnergy(EnergyCost_Move, false);
                    }
                    //on success, player may be standing on stairs
                    else if (puppetEntity.occupyingTile.tileType == TileMonoBehavior.TileType.stairs)
                    {
                        PromptForStairs();
                    }
                }

                clock = 0;
            }
                
            else if (yAxis > 0.5)
            {
                clock = 0;

                //if player will attack in direction,
                if (puppetEntity.WillAttack(Entity.MoveDirection.up))
                {
                    // then Try to spend energy on attacking in direction
                    if (TrySpendEnergy(EnergyCost_Attack))
                        puppetEntity.Move(Entity.MoveDirection.up);
                    else //otw, player doesn't have enough energy, so notify player somehow
                    {
                    }
                } //entity won't attack in direction, try moving
                else if (TrySpendEnergy(EnergyCost_Move)) //try to spend energy on moving
                {
                    //player has enough energy
                    //try to move right. On fail, give player their energy back
                    if (!puppetEntity.Move(Entity.MoveDirection.up))
                    {
                        IncreaseEnergy(EnergyCost_Move, false);
                    }
                    //on success, player may be standing on stairs
                    else if (puppetEntity.occupyingTile.tileType == TileMonoBehavior.TileType.stairs)
                    {
                        PromptForStairs();
                    }
                }

                //puppetEntity.Move(Entity.MoveDirection.up);
            } 
            else if (yAxis < -0.5)
            {
                //puppetEntity.Move(Entity.MoveDirection.down);

                //if player will attack in direction,
                if (puppetEntity.WillAttack(Entity.MoveDirection.down))
                {
                    // then Try to spend energy on attacking in direction
                    if (TrySpendEnergy(EnergyCost_Attack))
                        puppetEntity.Move(Entity.MoveDirection.down);
                    else //otw, player doesn't have enough energy, so notify player somehow
                    {
                    }
                } //entity won't attack in direction, try moving
                else if (TrySpendEnergy(EnergyCost_Move)) //try to spend energy on moving
                {
                    //player has enough energy
                    //try to move right. On fail, give player their energy back
                    if (!puppetEntity.Move(Entity.MoveDirection.down))
                    {
                        IncreaseEnergy(EnergyCost_Move, false);
                    }
                    //on success, player may be standing on stairs
                    else if (puppetEntity.occupyingTile.tileType == TileMonoBehavior.TileType.stairs)
                    {
                        PromptForStairs();
                    }
                }

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
            else if(castRegSpell)
            {
                if(TrySpendEnergy(EnergyCost_Cast_Spell_1))
                    puppetEntity.CastSpell(0);
                clock = 0;
            }
		    else if(castFireSpell)
		    {
                if (TrySpendEnergy(EnergyCost_Cast_Spell_2))
                    puppetEntity.CastSpell(1);
			    clock = 0;
		    }
		    else if(castIceSpell)
		    {
                if (TrySpendEnergy(EnergyCost_Cast_Spell_3))
                    puppetEntity.CastSpell(2);
			    clock = 0;
		    }
		    else if(castLightningSpell)
		    {
                if (TrySpendEnergy(EnergyCost_Cast_Spell_4))
                    puppetEntity.CastSpell(3);
			    clock = 0;
		    }

			if(puppetEntity.GetRemainingSpeed() == 0 || remainingTime <= 0) //if Entity can't act anymore, or player ran out of time -> forfeits turn
			{
				//Debug.Log("Player Entity finished Turn");
				EndTurn();
			}
        }
        else if(canAct) //player can act, but is off-cycle
        {
            DecreaseRemainingTime(Time.deltaTime);
            clock += Time.deltaTime;
            if (remainingTime <= 0) //not time to act, but player ran out of time to act, forfeits turn
                EndTurn();
        }
	}
    override protected void OnDestroy()
    {
        base.OnDestroy();
        //possibly play a death animation for player?
        
    }

    void PromptForStairs()
    {
        DungeonManager.theManager.GoToNextLevel();
    }
}
