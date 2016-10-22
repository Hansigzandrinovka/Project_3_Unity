using UnityEngine;
using System.Collections;

public class TestPlayerController : MonoBehaviour {
    private Entity puppetEntity;
    public float debug_input_value = 0;
    private float clock = 0; //used to prevent input flooding by slowing user input processing
    public float inputTime = 2; //how many seconds must pass before input can be processed again

	// Use this for initialization
	void Start () {
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
            
	}
	
	// Update is called once per frame
	void Update () {
        debug_input_value = Input.GetAxis("Horizontal");
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
