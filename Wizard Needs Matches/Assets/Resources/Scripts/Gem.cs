using UnityEngine;

using System.Collections;


using System.Collections.Generic;
public class Gem : MonoBehaviour {


	public GameObject sphere;
	public GameObject selector;
	string[] gemMats ={"Red","Blue","Green","Orange","Yellow","Pink","Purple"};
	public string color="";
	public List<Gem> Neighbors = new List<Gem>(); ///contains gems to the left, up, right, and below this gem
	public bool isSelected = false;
	public bool isMatched = false; ///true when gem has been found as part of a matching set, then false after gem has been matched and then removed
    public int matchSize = 0; ///the size of the match chain this gem is a part of, awards extra points depending on the size of the chain, ie 3 gem lines award fewer points than 4 gem lines, then 5 gem lines
    //public bool isFalling = false;
    private Rigidbody velocityReader;

    public static readonly float stopSpeed = .1f; ///the speed at which object moves to be approximately unmoving

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
        /*if(velocityReader != null) //enforces that gems can only fall down
        {
            if (velocityReader.velocity.y > stopSpeed)
                velocityReader.velocity = new Vector3(velocityReader.velocity.x, 0, velocityReader.velocity.z);
        }*/
	}

	public void ToggleSelector(){
		isSelected  = !isSelected ;
		selector.SetActive(isSelected);
}
    
    ///gives the Gem its appearance and color identity
	public void CreateGem(){
        matchSize = 0;
		color = gemMats[Random.Range(0,gemMats.Length)];
		Material m = Resources.Load("Materials/"+color)as Material;
		sphere.GetComponent<Renderer>().material = m;
        isMatched = false;
}

	public bool IsNeighborWith(Gem g){
		if( Neighbors.Contains(g)){
			return true;
		}
		return false;
}
	public void AddNeighbor(Gem g){
        //Neighbors.Add(g);
		if(!Neighbors.Contains(g))
			Neighbors.Add(g);
		}
	public void RemoveNeighbor(Gem g){
		Neighbors.Remove(g);
		}
	void OnMouseDown(){
        if (!DungeonManager.IsGameStarted()) //player can't make matches until the game starts
            return;
		if( !Board.theMatchingBoard.isSwapping)
        {
			ToggleSelector();
			Board.theMatchingBoard.SwapGems(this);
			}
		}


}
