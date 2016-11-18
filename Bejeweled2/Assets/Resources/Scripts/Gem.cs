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
    public bool isFalling = false;
    private Rigidbody velocityReader;

    private static readonly float stopSpeed = .1f; //the speed at which object moves to be approximately unmoving

	public int XCoord{
		get{
			return Mathf.RoundToInt(transform.localPosition.x);
			}
		}

	public int YCoord{
		get{
			return Mathf.RoundToInt(transform.localPosition.y);
			}
		}



	// Use this for initialization

	void Start () {

		CreateGem();
        velocityReader = GetComponent<Rigidbody>();
        if (velocityReader == null)
            Debug.LogError("Alert! Could not find Rigidbody on Gem!");
	}

	
	// Update is called once per frame

	void Update () {
        if(velocityReader != null)
        {
            if (velocityReader.velocity.magnitude > stopSpeed)
                isFalling = true;
            else
                isFalling = false;
        }
	}

	public void ToggleSelector(){
		isSelected  = !isSelected ;
		selector.SetActive(isSelected);
}
	public void CreateGem(){
		color = gemMats[Random.Range(0,gemMats.Length)];
		Material m = Resources.Load("Materials/"+color)as Material;
		sphere.GetComponent<Renderer>().material = m;
}

	public bool IsNeighborWith(Gem g){
		if( Neighbors.Contains(g)){
			return true;
		}
		return false;
}
	public void AddNeighbor(Gem g){
		if(!Neighbors.Contains(g))
			Neighbors.Add(g);
		}
	public void RemoveNeighbor(Gem g){
		Neighbors.Remove(g);
		}
	void OnMouseDown(){
		
		if( !GameObject.Find("Board").GetComponent<Board>().isSwapping){
			ToggleSelector();
			GameObject.Find("Board").GetComponent<Board>().SwapGems(this);
			}
		}


}
