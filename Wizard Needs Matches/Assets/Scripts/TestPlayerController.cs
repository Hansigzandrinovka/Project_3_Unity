using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TestPlayerController : EntityController {
    
    public Slider playerHealthSlider; //the Slider that will store Player's current health
    private float clock = 0; //used to prevent input flooding by slowing user input processing
    public float inputTime = 2; //how many seconds must pass before input can be processed again

    // Use this for initialization
    //overrides virtual Start() in EntityController
    protected override void Start () {
        if (inputTime <= 0)
        {
            Debug.Log("Player Controller Input Time initialized to bad value " + inputTime);
            inputTime = 2;
        }
            
        puppetEntity = GetComponent<Entity>();
        if (puppetEntity == null) //prevents this script from running if no puppet entity is found
        {
            enabled = false;
            Debug.Log("Error! Player Controller could not find Entity!");
        }
        else if(playerHealthSlider != null)
        {
            playerHealthSlider.value = puppetEntity.SetUIHealthChangeListener(UpdateDisplayHealth);
            playerHealthSlider.maxValue = puppetEntity.maxHealth;
        }
        base.Start();
	}

    void UpdateDisplayHealth(int newHealth)
    {
        if (playerHealthSlider != null)
            playerHealthSlider.value = newHealth;
    }
	
	// Update is called once per frame
	void Update () {
        if (clock >= inputTime)
        {
            float xAxis = Input.GetAxis("Horizontal");
            float yAxis = Input.GetAxis("Vertical");

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
                
            
        }
        else
            clock += Time.deltaTime;
        
	}
}
