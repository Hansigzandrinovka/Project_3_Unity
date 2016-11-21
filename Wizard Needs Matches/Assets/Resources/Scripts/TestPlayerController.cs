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

    //constants for spending Player Energy
    private static readonly int EnergyCost_Move = 30; //cost in energy to move in any direction
    private static readonly int EnergyCost_Attack = 60; //cost in energy to make an attack against an adjacent enemy
    private static int EnergyCost_Cast_Spell_1 = 150; //cost in energy to cast the spell bound to 1 key
    private static int EnergyCost_Cast_Spell_2 = 200;
    private static int EnergyCost_Cast_Spell_3 = 250;
    private static int EnergyCost_Cast_Spell_4 = 280;

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

    public override void StartTurn()
    {
        base.StartTurn();
        clock = 0;
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
        if(Board.theMatchingBoard != null)
        {
            Board.theMatchingBoard.SetScoreIncreaseListener(this.UpdateDisplayEnergy_Increment);
        }
        else
        {
            Debug.LogError("Alert! " + this + " was unable to sync with static Board instance to configure Score change listening!");
        }
	}

    void UpdateDisplayHealth(int newHealth)
    {
        if (playerHealthSlider != null)
            playerHealthSlider.value = newHealth;
    }

    //used as a Listener to Board
    //increases Energy of Controller towards maximum
    //and updates display with new energy
    void UpdateDisplayEnergy_Increment(int amountToChange)
    {
        IncreaseEnergy(amountToChange, false);
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
	
	// Update is called once per frame
	//if it is Controller's turn, waits for player input to control Entity
	void Update () {
        if (clock >= inputTime && canAct) //only evaluates player activity every time increment, but only if player can act
        {
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
