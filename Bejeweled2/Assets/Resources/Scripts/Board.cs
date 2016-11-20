﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Board : MonoBehaviour {


public List<Gem> gems = new List<Gem>(); //holds all active Gems on the board
public int GridWidth;
public int GridHeight;
public GameObject gemPrefab; //the prefab used to create Gems on the board
public Gem lastGem;
public Vector3 gem1Start, gem1End, gem2Start, gem2End;
public bool isSwapping = false;
public Gem gem1,gem2;
public float startTime;
public float swapRate = 2 ;
public int AmountToMatch = 3; //number of gems in a row that are needed to match, ie 3 in a row
public bool isMatched = false;
    public int currentScore = 0; //tracks the score until we move score tracking to EntityControllers

	
// Use this for initialization
	
    void Start () {

    for(int y=0; y<GridHeight ; y++){
        for(int x=0; x<GridWidth; x++){
            GameObject g = Instantiate(gemPrefab,new Vector3(x,y,0),Quaternion.identity)as GameObject;
            g.transform.parent = gameObject.transform;
            gems.Add(g.GetComponent<Gem>());
        }
    }
    gameObject.transform.position = new Vector3(- 3.5f, -3.5f,0); //places board in center of screen
	
}

    //handles generation of score/energy numbers based on the number of gems matched in this iteration, ie matching 3 gems gives 
    void ScoreGems(int comboCount, int matchSize)
    {
        if (comboCount < 1)
        {
            Debug.LogError("Alert! Called ScoreGems with comboCount < 1: " + comboCount);
            return;
        }
        if(matchSize < AmountToMatch)
        {
            Debug.LogError("Alert! Called ScoreGems with matchSize < AmountToMatch: " + matchSize + "," + AmountToMatch);
            return;
        }
            
        int comboPoints = comboCount * 5;
        Debug.Log("Awarded Combo Points: " + comboPoints);
        currentScore += comboPoints;
        int matchPoints = 0;
        switch(matchSize)
        {
            case 1:
                {
                    matchPoints = 0;
                    break;
                }
            case 2:
                {
                    matchPoints = 0;
                    break;
                }
            case 3:
                {
                    matchPoints = 10; //30 total
                    break;
                }
            case 4:
                {
                    matchPoints = 12; //48 total
                    break;
                }
            case 5:
                {
                    matchPoints = 14; //70 total
                    break;
                }
            case 6:
                {
                    matchPoints = 15; //90 total
                    break;
                }
            default:
                {
                    matchPoints = 9 + matchSize; //expect for size=7, points = 16, increase by 1 every case after
                    break;
                }
        }
        Debug.Log("Awarded match points: " + matchPoints);
        currentScore += matchPoints;
    }

	
	// Update is called once per frame

	void Update () {


		if (isMatched) { //time to remove matched gems
            int comboCount = 0; //number of gems matched in the same update, awards more points the more gems matched
			for (int i=0; i<gems.Count; i++) { //push matched gems to the top of the screen so they can fall back down
				if (gems [i].isMatched) {
                    comboCount++;
                    ScoreGems(comboCount,gems[i].matchSize);
					gems [i].CreateGem ();
                    //ToDo: make sure there isn't already a gem there
					gems [i].transform.position = new Vector3 (
						gems [i].transform.position.x,
						gems [i].transform.position.y + GridHeight, //prevents weird stuff happening when we match the gems at the bottom two rows (originally was + 6)
						gems [i].transform.position.z);
				}
			}
			isMatched = false;
		} else if (isSwapping) { //time to swap gems
			MoveGem (gem1, gem1End, gem1Start);
			MoveNegGem (gem2, gem2End, gem2Start);
			if (Vector3.Distance (gem1.transform.position, gem1End) < 0.1f || Vector3.Distance (gem2.transform.position, gem2End) < 0.1f) { //if gems are close enough to their final positions to snap, snap them and acknowledge swap occurred
				gem1.transform.position = gem1End;
				gem2.transform.position = gem2End;
				gem1.ToggleSelector ();
				gem2.ToggleSelector ();
				lastGem = null;

				

				isSwapping = false;
				TogglePhysics (false);
				CheckMatch ();

			}
		} else if( DetermineBoardState() ){ //not matching or swapping, so check board state
			for (int i=0; i<gems.Count; i++){
				CheckForNearbyMatches( gems[i] );

			}
		}
	
	}

    //returns true if board is not in a stable state because: either gems are above the board, or gems are falling right now
	public bool DetermineBoardState()
	{
		for (int i=0; i < gems.Count; i++) {
			if(gems[i].transform.localPosition.y > GridHeight)
				return true;
			else if( Mathf.Abs(gems[i].GetComponent<Rigidbody>().velocity.y) > .1f)
				return true;
		}
		return false;
	}

    //tests match for two given gems
    //creates two lists, determines if lists contain valid matches in them, makes each list test all gems in them for matches
    public void CheckMatch(){
		List<Gem> gem1List = new List<Gem>();
		List<Gem> gem2List = new List<Gem>();
		ConstructMatchList( gem1.color , gem1 , gem1.XCoord, gem1.YCoord , ref gem1List ); //recursive function
		FixMatchList( gem1 , gem1List);
		ConstructMatchList( gem2.color , gem2 , gem2.XCoord, gem2.YCoord , ref gem2List );
		FixMatchList( gem2 , gem2List);

		//print("Gem1"+ gem1List.Count);

		} 
    //fetches all matching gems around given gem, then checks them for row/column alignment
	public void CheckForNearbyMatches( Gem g)
	{
		List<Gem> gemList = new List<Gem> ();
		ConstructMatchList (g.color, g , g.XCoord , g.YCoord , ref gemList);
		FixMatchList ( g , gemList);
	}

    //see if current gem in same row/column of given gem to build row/col list of matching gems
	public void ConstructMatchList( string color, Gem gem, int XCoord, int YCoord, ref List<Gem> MatchList ){
		
		if( gem == null){ //if somehow we're looking at nonexistent gem
			return;
			}
		else if( gem.color != color){ //only match same-color gems
				return;}
			else if( MatchList.Contains(gem)){ //if we have already evaluated gem?
					return;
					}
				else{ //same kind of gem not in list, so add to list and check
					MatchList.Add(gem);
					if(XCoord == gem.XCoord || YCoord == gem.YCoord){
						foreach( Gem g in gem.Neighbors){ //check each gem in this gem's neighbors for matching, will naturally exclude incorrectly colored and already checked gems
							//ConstructMatchList( color, g, XCoord, YCoord, ref MatchList );
							ConstructMatchList( color, g, XCoord, YCoord, ref  MatchList );
							}
						}
					}
}

    //makes Match Lists remove gems that are not in the row/column of the majority of the list
	public void FixMatchList(Gem gem , List<Gem> ListToFix){

		List<Gem> rows = new List<Gem>();
		List<Gem> Collumns = new List<Gem>();
		
        //iterate through given List, move Gem to rows list or cols list depending on its coordinates
		for( int i =0 ; i< ListToFix.Count ; i++){
			if( gem.XCoord == ListToFix[i].XCoord){ //it is in same row as gem
				rows.Add(ListToFix[i]);
				}
			if( gem.YCoord == ListToFix[i].YCoord){ //it is in same column as gem
				Collumns.Add(ListToFix[i]);
				}
			}

			if( rows.Count >= AmountToMatch){ //check if we have enough gems to make a match
				isMatched = true;
				for( int i=0 ; i < rows.Count ; i++){ //notify each of the gems in row that they match (should be removed)
					rows[i].isMatched  = true;
                rows[i].matchSize += rows.Count;
					}
				
				}

			if( Collumns.Count >= AmountToMatch){
				isMatched = true;
				for( int i=0 ; i < Collumns.Count ; i++){
					Collumns[i].isMatched  = true;
                Collumns[i].matchSize += Collumns.Count;
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
