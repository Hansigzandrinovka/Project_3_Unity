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
	
    //on start, create all the gems for user to match with, then move this object to a convenient location
    void Start () {

        for(int y=0; y<GridHeight ; y++){
            for(int x=0; x<GridWidth; x++){
                GameObject g = Instantiate(gemPrefab,new Vector3(x,y,0),Quaternion.identity)as GameObject;
                g.transform.parent = gameObject.transform;
                gems.Add(g.GetComponent<Gem>());
            }
        }
        //positions board at approximately center
        gameObject.transform.position = new Vector3(- 3.5f, -3.5f,0);
    }

	
	    // Update is called once per frame
    //every update, either match current gems, swap gems, or moniter board state
	void Update () {


		if (isMatched) {

			for (int i=0; i<gems.Count; i++) {
				if (gems [i].isMatched) {
					gems [i].CreateGem ();
					gems [i].transform.position = new Vector3 (
						gems [i].transform.position.x,
						gems [i].transform.position.y + 6,
						gems [i].transform.position.z);
				}
			}
			isMatched = false;
		} else if (isSwapping) {
			MoveGem (gem1, gem1End, gem1Start);
			MoveNegGem (gem2, gem2End, gem2Start);
			if (Vector3.Distance (gem1.transform.position, gem1End) < 0.1f || Vector3.Distance (gem2.transform.position, gem2End) < 0.1f) {
				gem1.transform.position = gem1End;
				gem2.transform.position = gem2End;
				gem1.ToggleSelector ();
				gem2.ToggleSelector ();
				lastGem = null;

				

				isSwapping = false;
				TogglePhysics (false);
				CheckMatch ();

			}
		} else if(!DetermineBoardState() ){ //if board is stable unmoving, check for additional matches
			for (int i=0; i<gems.Count; i++){
				CheckForNearbyMatches( gems[i] );

			}
		}
        //make board clear if no viable matches
	
	}

    //checks if board contains matches by trying all gems for matching potential
    public bool DoesBoardContainMatches()
    {
        TogglePhysics(false);
        for(int i = 0; i < gems.Count; i++)
        {
            for(int j = 0; j < gems.Count; j++)
            {
                if(gems[i].IsNeighborWith(gems[j])) //try to swap neighboring gems, see if match
                {
                    Gem g = gems[i];
                    Gem f = gems[j];
                    Vector3 tempPos = g.transform.position;
                    g.transform.position = f.transform.position;
                    f.transform.position = tempPos;
                    List<Gem> testNeighbors = new List<Gem>(g.Neighbors);
                    //TODO: finish this from video #19
                }
            }
        }
    }

    //checks if board is in a stable state (we can check for matches)
    //returns true if board is NOT in a stable state, else false
	public bool DetermineBoardState()
	{
        //TODO: relate "8" to cols + transform.position.y
		for (int i=0; i < gems.Count; i++) { //if there are gems above upper bounds of board
			if(gems[i].transform.localPosition.y > 8)
				return true;
			else if( Mathf.Abs(gems[i].GetComponent<Rigidbody>().velocity.y) > .1f) //if there is a gem that is still moving, board is not stable
				return true;
		}
		return false;
	}

    //attempts to find matches for Gem 1 and Gem 2 (called after a swap finishes)
	public void CheckMatch(){
		List<Gem> gem1List = new List<Gem>();
		List<Gem> gem2List = new List<Gem>();
		ConstructMatchList( gem1.color , gem1 , gem1.XCoord, gem1.YCoord , ref gem1List );
		FixMatchList( gem1 , gem1List);
		ConstructMatchList( gem2.color , gem2 , gem2.XCoord, gem2.YCoord , ref gem2List );
		FixMatchList( gem2 , gem2List);

		//print("Gem1"+ gem1List.Count);

		}

    //checks if there are matches involving given gem
	public void CheckForNearbyMatches( Gem g)
	{
		List<Gem> gemList = new List<Gem> ();
		ConstructMatchList (g.color, g , g.XCoord , g.YCoord , ref gemList);
		FixMatchList ( g , gemList);
	}

    //builds a list of gems that can match with given gem at given position
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

    //converts single list of matching gems to two lists: rows and columns,
    //checks if there are enough gems in either list to constitute a match
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

    //Linearly interpolates (moves) a gem from "fromPos" to "toPos",
    //rotates gem in 3d around central axis point 0,0,.1
	public void MoveGem(Gem gemToMove, Vector3 toPos, Vector3 fromPos){
		Vector3 center = (fromPos + toPos)* 0.5f;
		center -= new Vector3(0,0,0.1f);
		Vector3 riseRelCenter = fromPos - center;
		Vector3  setRelCenter = toPos - center;
		float fracComplete = (Time.time - startTime)/swapRate;
		gemToMove.transform.position = Vector3.Slerp(riseRelCenter , setRelCenter , fracComplete );
		gemToMove.transform.position += center;
	}

    //linearly interpolates (moves) gem from "fromPos" to "toPos",
    //rotates gem in 3d around central axis point 0,0,-.1
	public void MoveNegGem(Gem gemToMove, Vector3 toPos, Vector3 fromPos){
		Vector3 center = (fromPos + toPos)* 0.5f;
		center -= new Vector3(0,0,-0.1f);
		Vector3 riseRelCenter = fromPos - center;
		Vector3  setRelCenter = toPos - center;
		float fracComplete = (Time.time - startTime)/swapRate;
		gemToMove.transform.position = Vector3.Slerp(riseRelCenter , setRelCenter , fracComplete );
		gemToMove.transform.position += center;
    }

    //enables (true) or disables (false) physics system for all gems,
    //used to prevent gems falling while swapping two gems
	public void TogglePhysics( bool isOn){
		for( int i=0 ; i < gems.Count ; i++){
			gems[i].GetComponent<Rigidbody>().isKinematic = isOn;
			}
		}
	
    //handles the logic of selecting/deselecting gems for swapping,
    //sets first gem if undefined, else unsets first gem or sets second gem
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
