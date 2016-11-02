using UnityEngine;

using System.Collections;


using System.Collections.Generic;
public class Gem : MonoBehaviour {

	

	public GameObject sphere;
	public GameObject selector;
	string[] gemMats ={"Red","Blue","Green","Orange","Yellow","Pink","Purple"}; //the different gem colors this gem can exist in
	string color="";
	public List<Gem> Neighbors = new List<Gem>(); //all bordering Gems that are above, below, left, or right of this Gem
	public bool isSelected = false; //whether or not gem is selected for matching


	// Use this for initialization

	void Start () {

		CreateGem(); //on creation, give this gem a type
	
	}

	
	// Update is called once per frame

	void Update () {

	
	}

	public void ToggleSelector(){
		isSelected  = !isSelected ;
		selector.SetActive(isSelected);
}
	public void CreateGem(){ //fetches a random color and displays gem as that color
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
		ToggleSelector();
		GameObject.Find("Board").GetComponent<Board>().SwapGems(this);
		}


}
