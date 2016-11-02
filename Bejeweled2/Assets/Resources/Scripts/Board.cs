using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Board : MonoBehaviour {


public List<Gem> gems = new List<Gem>();
public int GridWidth;
public int GridHeight;
public GameObject gemPrefab;
public Gem lastGem;
public Vector3 gem1Start, gem1End, gem2Start, gem2End;
public bool isSwapping = false;
public Gem gem1,gem2;
public float startTime;
public float swapRate = 2 ;
public int AmountToMatch = 3;
public bool isMatched = false;

	
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

		if (isSwapping){
			MoveGem(gem1  , gem1End, gem1Start);
			MoveNegGem(gem2  , gem2End, gem2Start);
			if( Vector3.Distance(gem1.transform.position,gem1End) < 0.1f || Vector3.Distance(gem2.transform.position,gem2End) < 0.1f ){
				gem1.transform.position = gem1End;
				gem2.transform.position = gem2End;
				gem1.ToggleSelector();
				gem2.ToggleSelector();
				lastGem = null;

				

				isSwapping = false;
				TogglePhysics( false );
				CheckMatch();

				}
			}
	
	}


	public void CheckMatch(){
		List<Gem> gem1List = new List<Gem>();
		List<Gem> gem2List = new List<Gem>();
		ConstructMatchList( gem1.color , gem1 , gem1.XCoord, gem1.YCoord , ref gem1List );
		FixMatchList( gem1 , gem1List);
		ConstructMatchList( gem2.color , gem2 , gem2.XCoord, gem2.YCoord , ref gem2List );
		FixMatchList( gem2 , gem2List);

		//print("Gem1"+ gem1List.Count);

		}

	public void ConstructMatchList( string color, Gem gem, int XCoord, int YCoord, ref List<Gem> MatchList ){
		
		if( gem == null){
			return;
			}
		else if( gem.color != color){
				return;}
			else if( MatchList.Contains(gem)){
					return;
					}
				else{
					MatchList.Add(gem);
					if(XCoord == gem.XCoord || YCoord == gem.YCoord){
						foreach( Gem g in gem.Neighbors){
							//ConstructMatchList( color, g, XCoord, YCoord, ref MatchList );
							ConstructMatchList( color, g, XCoord, YCoord, ref  MatchList );
							}
						}
					}
}


	public void FixMatchList(Gem gem , List<Gem> ListToFix){

		List<Gem> rows = new List<Gem>();
		List<Gem> Collumns = new List<Gem>();
		
		for( int i =0 ; i< ListToFix.Count ; i++){
			if( gem.XCoord == ListToFix[i].XCoord){
				rows.Add(ListToFix[i]);
				}
			if( gem.YCoord == ListToFix[i].YCoord){
				Collumns.Add(ListToFix[i]);
				}
			}

			if( rows.Count >= AmountToMatch){
				isMatched = true;
				for( int i=0 ; i < rows.Count ; i++){
					rows[i].isMatched  = true;
					}
				
				}

			if( Collumns.Count >= AmountToMatch){
				isMatched = true;
				for( int i=0 ; i < Collumns.Count ; i++){
					Collumns[i].isMatched  = true;
					}
				
				}

		}


	public void MoveGem(Gem gemToMove, Vector3 toPos, Vector3 fromPos){
		Vector3 center = (fromPos + toPos)* 0.5f;
		center -= new Vector3(0,0,0.1f);
		Vector3 riseRelCenter = fromPos - center;
		Vector3  setRelCenter = toPos - center;
		float fracComplete = (Time.time - startTime)/swapRate;
		gemToMove.transform.position = Vector3.Slerp(riseRelCenter , setRelCenter , fracComplete );
		gemToMove.transform.position += center;
	}

	public void MoveNegGem(Gem gemToMove, Vector3 toPos, Vector3 fromPos){
		Vector3 center = (fromPos + toPos)* 0.5f;
		center -= new Vector3(0,0,-0.1f);
		Vector3 riseRelCenter = fromPos - center;
		Vector3  setRelCenter = toPos - center;
		float fracComplete = (Time.time - startTime)/swapRate;
		gemToMove.transform.position = Vector3.Slerp(riseRelCenter , setRelCenter , fracComplete );
		gemToMove.transform.position += center;
}

	public void TogglePhysics( bool isOn){
		for( int i=0 ; i < gems.Count ; i++){
			gems[i].GetComponent<Rigidbody>().isKinematic = isOn;
			}
		}
	
	public void SwapGems(Gem currentGem ){
		if( lastGem == null ){
			lastGem  = currentGem;}
		else if ( lastGem  == currentGem ){
			lastGem = null;}
			else{
				if( lastGem.IsNeighborWith(currentGem )){
					gem1Start = lastGem.transform.position;
					gem1End = currentGem.transform.position;

					gem2Start = currentGem.transform.position;
					gem2End = lastGem.transform.position;

					startTime = Time.time;
					TogglePhysics( true);
					gem1 = lastGem;
					gem2 = currentGem; 
					isSwapping =true;

				}
				else{
					lastGem.ToggleSelector();
					lastGem  = currentGem;
				}
			}

}

}
