using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Board : MonoBehaviour {


public List<Gem> gems = new List<Gem>();
public int GridWidth;
public int GridHeight;
public GameObject gemPrefab;
public Gem lastGem;	
// Use this for initialization
	
void Start () {

for(int y=0; y<GridHeight ; y++){
for(int x=0; x<GridWidth; x++){
GameObject g = Instantiate(gemPrefab,new Vector3(x,y,0),Quaternion.identity)as GameObject;
g.transform.parent = gameObject.transform;
gems.Add(g.GetComponent<Gem>());
}
}
gameObject.transform.position = new Vector3(- 3.5f, -3.5f,0);
	
	}

	
	// Update is called once per frame

	void Update () {

	
	}

	public void SwapGems(Gem currentGem ){
		if( lastGem == null ){
			lastGem  = currentGem;}
		else if ( lastGem  == currentGem ){
			lastGem = null;}
			else{
				if( lastGem.IsNeighborWith(currentGem )){

				}
				else{
					lastGem.ToggleSelector();
					lastGem  = currentGem;
				}
			}

}

}
