using UnityEngine;

using System.Collections;


using System.Collections.Generic;
public class Gem : MonoBehaviour {

	

	public GameObject sphere;
	public GameObject selector;
	string[] gemMats ={"Red","Blue","Green","Orange","Yellow","Pink","Purple"};
	public string color="";
	public List<Gem> Neighbors = new List<Gem>();
	public bool isSelected = false;
	public bool isMatched = false;
    private Rigidbody attachedRigidbody;
    private static readonly float maxUpwardSpeed = 2f;

    //returns the approximate X location of the Gem as an int
	public int XCoord{
		get{
			return Mathf.RoundToInt(transform.localPosition.x);
			}
		}

    //returns the approximate Y location of the Gem as an int
	public int YCoord{
		get{
			return Mathf.RoundToInt(transform.localPosition.y);
			}
		}



	// Use this for initialization
    //gives this Gem a random color
	void Start () {

		CreateGem();
        attachedRigidbody = GetComponent<Rigidbody>();
        if(attachedRigidbody == null)
        {
            Debug.LogError("Alert! No Rigidbody attached to Gem at " + transform.position.x + "," + transform.position.y);
        }
	
	}

	
	// Update is called once per frame

	void Update () {
        ReduceUpwardBouncyness();
	}

    //tracks if this gem is selected or not,
    //turns on/off the Particle effects indicating this Gem is selected
	public void ToggleSelector(){
		isSelected  = !isSelected ;
		selector.SetActive(isSelected);
}

    //prevents the gem from bouncing upward with a high velocity when gems swap
    //or when hitting a gem from above
    void ReduceUpwardBouncyness()
    {
        if(attachedRigidbody != null && attachedRigidbody.velocity.y > maxUpwardSpeed)
        {
            //Debug.Log("Enforcing max Y Velocity on Gem");
            attachedRigidbody.velocity = new Vector3(attachedRigidbody.velocity.x,0f,attachedRigidbody.velocity.z);
        }

    }

    //sets current gem's color and appearance to a new color/appearance using Unity's RNG
    //if unable to load resource, Logs an Error and destroys Object
	public void CreateGem(){
		color = gemMats[Random.Range(0,gemMats.Length)];
		Material m = Resources.Load("Materials/"+color)as Material;
        if(m == null)
        {
            Debug.LogError("Unable to fetch material at Materials/" + color);
            Destroy(this.gameObject);
            return;
        }
		sphere.GetComponent<Renderer>().material = m;
}

    //returns true if given Gem is one of this Gem's neighbors
	public bool IsNeighborWith(Gem g){
		if( Neighbors.Contains(g)){
			return true;
		}
		return false;
}
    //adds Gem to this gem's neighbors if isn't already in neighbors
	public void AddNeighbor(Gem g){
		if(!Neighbors.Contains(g))
			Neighbors.Add(g);
		}

    //attempts to remove given Gem from this gem's neighbors
	public void RemoveNeighbor(Gem g){
		Neighbors.Remove(g);
		}

    //called by Unity if this object is clicked with Mouse during runtime
    //toggles particle effects if in swapping state, else attempts to swap gems
	void OnMouseDown(){
		
		if( !GameObject.Find("Board").GetComponent<Board>().isSwapping){
			ToggleSelector();
			GameObject.Find("Board").GetComponent<Board>().SwapGems(this);
			}
		}


}
