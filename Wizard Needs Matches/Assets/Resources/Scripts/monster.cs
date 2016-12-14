using UnityEngine;
using System.Collections;
/**
 * Holds all the monster moving decisions
 **/
public class monster : EntityController
{
	
	private float clock = 0;
	private static readonly float initialInputTime = .5f; ///<the total that inputTime is a fraction of>
	public static float inputTime = 1; ///<tracks how much time each monster takes to perform its turn>
	public static int numMonsters = 0; ///<tracks number of instances of this class in existence, reduces input time so that really large amounts of monsters wont eat up wait time as bad>
	enum EntVecVal { distX, distY, distG, entDir, prevX, prevY, upDatePos };/// Distance to player in X, Y, and Grid, and direction
	/**
	 * Compass layout to help decide where player is (16 directions)
	 **/
	enum Bearing {
		//Straight up to before straight right
		N, NNE, NE, ENE,
		//Straight right to before straight down
		E, ESE, SE, SSE,
		//Straight down to before straight left
		S, SSW, SW, WSW,
		//Straight left to before straight up
		W, WNW, NW, NNW
	};

	private void UpdateInputTime() /// configures UpdateInputTime depending on the number of active monsters in the dungeon
	{
		if (numMonsters >= 1) {
			inputTime = (initialInputTime / numMonsters) + (numMonsters - 1) * .05f;
		}
	}
	
	/**
	 * relative dist and dir array, and old location, and update flag
	 **/
	int [] entVec = new int[7];
	/** How far a monster can react to a player (grid distance)
	 * Overide as needed, default is radius 6 to allow wandering
	 **/
	public int monstSenseDist = 6;
	
	// to know where player is
	GameObject player;
	
	// Use this for initialization
	protected override void Start () {
		base.Start ();
		// FIND player by type
		player = GameObject.FindGameObjectWithTag("Player");
//		Debug.Log("Find " + this);

		numMonsters += 1;
		UpdateInputTime ();
	}
	
	public override void StartTurn()
	{
		base.StartTurn ();
		clock = 0;
	}
	
	/// Every update get the distance to the player. If within monster sense, move intelligently.
	/// If not, wander or get forced into stillness
	void Update ()
	{
		if ((clock >= inputTime) && canAct)
		{
			/// Compares player direction and distance to self
			getGridDistAndDirecTo(player, entVec);					
			/// Compares if player is within range of monster sense
			if (canMonstSensePC(monstSenseDist, entVec))
			{
				// Monster moves anti-player
//				Debug.Log ("Update alert monster behavior");
				monsterMoves(monstSenseDist, entVec, true);
			}
			else
			{
				// Monster Wanders/Doesn't move
				//Debug.Log ("Monster does not care");
				monsterMoves(monstSenseDist, entVec, false);
			}

			if (puppetEntity.GetRemainingSpeed () == 0) {
				EndTurn ();
			}
		}
		else if (canAct) {
			clock += Time.deltaTime;
		}
	}
	/// <summary>
	/// Raises the destroy event and deletes the dead monster
	/// </summary>
	void OnDestroy()
	{
        base.OnDestroy();
		numMonsters --;
	}
	
	/** Entity finds the distance of target from itself by subtracting
	   target's x and y by own x and y respectively, then sums the
	   differences together
	**/
	protected void getGridDistAndDirecTo(GameObject target, int[] pos)
	{
		// PREP *****************************************************
		// Get own X and Y
		int selfX = Mathf.RoundToInt(transform.position.x);
		int selfY = Mathf.RoundToInt(transform.position.y);
		
		// Get target's X and Y
		int targX = Mathf.RoundToInt(target.transform.position.x);
		int targY = Mathf.RoundToInt(target.transform.position.y);
		
		// DISTANCE *************************************************
		// Calculate the differences
		int distX = Mathf.Abs(targX - selfX);
		int distY = Mathf.Abs(targY - selfY);
		
		// Store the difference
		pos[(int)EntVecVal.distX] = distX;
		pos[(int)EntVecVal.distY] = distY;
		/*Debug.Log("distX: " + targX + " - " + selfX + " = " + pos[(int)EntVecVal.distX]);
		 *Debug.Log("distY: " + targY + " - " + selfY + " = " + pos[(int)EntVecVal.distY]);
		 */


		
		// Sum the absolute valued differences, gives the number of steps to take
		pos[(int)EntVecVal.distG] = Mathf.Abs(distX) + Mathf.Abs(distY);
		/*Debug.Log("distG (values absolute valued before summing):" +
		 *        pos[(int)EntVecVal.distX] + " + " +
		 *        pos[(int)EntVecVal.distY] + " = " +
		 *        pos[(int)EntVecVal.distG]);
		 */
		// DIRECTION ***********************************************************
		/**
		 * horizontal: targX < selfX, targX = selfX, targX > selfX
		 * 
		 * vertical:	targY > selfY
		 * 				targY = selfY
		 * 				targY < selfY
		 * 
		 * diagonals: distX < distY,
		 *					distX == distY,
		 * 							distX > dist
		**/
		
		// Quadrant 1 (North and East)
		if (targX > selfX)		// East of self				
		{						
			if (targY > selfY)	// North of self				
			{					
				if( distX > distY) // More horizontal than vertical
				{
					pos[(int)EntVecVal.entDir] = (int)Bearing.ENE;
					//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
					//          "Should be " + (int)Bearing.ENE);				
				}
				else if (distX < distY) // More vertical than horizontal
				{
					pos[(int)EntVecVal.entDir] = (int)Bearing.NNE;
					//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
					//          "Should be " + (int)Bearing.NNE);				
				}
				else // Diagonal
				{
					pos[(int)EntVecVal.entDir] = (int)Bearing.NE;
					//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
					//          "Should be " + (int)Bearing.NE);				
				}
			}
			// Quadrant 4 (East and South)
			else if (targY < selfY)	// South of self				
			{
				if( distX > distY) // More horizontal than vertical
				{
					pos[(int)EntVecVal.entDir] = (int)Bearing.ESE;
					//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
					//          "Should be " + (int)Bearing.ESE);				
				}
				else if (distX < distY) // More vertical than horizontal
				{
					pos[(int)EntVecVal.entDir] = (int)Bearing.SSE;
					//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
					//          "Should be " + (int)Bearing.SSE);				
				}
				else // Diagonal
				{
					pos[(int)EntVecVal.entDir] = (int)Bearing.SE;
					//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
					//          "Should be " + (int)Bearing.SE);				
				}
			}
			else
			{
				pos[(int)EntVecVal.entDir] = (int)Bearing.E;
				//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
				//          "Should be " + (int)Bearing.E);			
			}						
		}
		// Quadrant 2 (North and West)
		else if (targX < selfX)		// West of self				
		{						
			if (targY > selfY)	// North of self				
			{					
				if( distX > distY) // More horizontal than vertical
				{
					pos[(int)EntVecVal.entDir] = (int)Bearing.WNW;
					//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
					//          "Should be " + (int)Bearing.WNW);				
				}
				else if (distX < distY) // More vertical than horizontal
				{
					pos[(int)EntVecVal.entDir] = (int)Bearing.NNW;
					//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
					//          "Should be " + (int)Bearing.NNW);				
				}
				else // Diagonal
				{
					pos[(int)EntVecVal.entDir] = (int)Bearing.NW;
					//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
					//          "Should be " + (int)Bearing.NW);				
				}// Quadrant 3 (East and South, , axis non-inclusive)
			}
			else if (targY < selfY)	// South of self				
			{
				if( distX > distY) // More horizontal than vertical
				{
					pos[(int)EntVecVal.entDir] = (int)Bearing.WSW;
					//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
					//          "Should be " + (int)Bearing.WSW);				
				}
				else if (distX < distY) // More vertical than horizontal
				{
					pos[(int)EntVecVal.entDir] = (int)Bearing.SSW;
					//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
					//          "Should be " + (int)Bearing.SSW);				
				}
				else // Diagonal
				{
					pos[(int)EntVecVal.entDir] = (int)Bearing.SW;
					//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
					//          "Should be " + (int)Bearing.SW);				
				}
			}
			else
			{
				pos[(int)EntVecVal.entDir] = (int)Bearing.W;
				//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
				//          "Should be " + (int)Bearing.W);			
			}
		}						
		// Target is on axis						
		else
		{						
			if (targY > selfY)	// North of self				
			{					
				pos[(int)EntVecVal.entDir] = (int)Bearing.N;				
				//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
				//          "Should be " + (int)Bearing.N);				
			}					
			else if (targY < selfY) // South of self					
			{					
				pos[(int)EntVecVal.entDir] = (int)Bearing.S;				
				//Debug.Log("Target is " + pos[(int)EntVecVal.entDir] +				
				//          "Should be " + (int)Bearing.S);				
			}				
		}
	}

	/**
	 * Monster checks if PC is within monster sense range
	 **/
	protected bool canMonstSensePC(int monstSenseDist, int[] pos)
	{
		// default is monster is out of range, or behind a wall
		bool monstSensedPC = false;

		// Distance is within monster sense range
		if (monstSenseDist >= pos[(int)EntVecVal.distG])
		{
			//Debug.Log("Monster sensed player");
			monstSensedPC = true;
		}
		else
		{
			Debug.Log ("Monster did not sense player");
		}
		return monstSensedPC;
	}

	/// Checks if monster is going back to the same square as last turn
	protected bool isMonsterRevisiting( int[] pos, int moveX, int moveY)
	{
		bool monsterRevisting = false;
		// Get future location
		int futureMoveX = Mathf.RoundToInt(transform.position.x) + moveX;
		int futureMoveY = Mathf.RoundToInt(transform.position.y) + moveY;
		Debug.Log("MoveX = " + moveX + " 1 if E, -1 if W\n" +
		          "MoveY = " + moveY + " 1 if N, -1 if S");
		Debug.Log ("futureX is " + futureMoveX + " futureY is " + futureMoveY);

		// Get past location
		int prevX = pos[(int)EntVecVal.prevX];
		int prevY = pos[(int)EntVecVal.prevY];
		Debug.Log ("prevX is " + prevX + " prevY is " + prevY);

		// if future equals past, last tile is revisited
		if ((prevX == futureMoveX) && (prevY == futureMoveY))
		{
			Debug.Log("Monster trying to revist last square");
			monsterRevisting = true;
		}
		return monsterRevisting;
	}

	/**
	 * Updates monster's last tile
	 **/ 
	protected void updatePrevTile(int[] pos, int movedX, int movedY)
	{
		// adds the inverse of the direction moved to current position
		// if didn't move on that axis, -1 * zero keeps same axis
		pos[(int)EntVecVal.prevX] =
			Mathf.RoundToInt(transform.position.x + (-1 * movedX));
		pos[(int)EntVecVal.prevY] =
			Mathf.RoundToInt(transform.position.y + (-1 * movedY));
	}

	/** Monster moves randomly (PC not sensed) or
	 	takes intelligent action (PC sensed)
	 **/
	protected void monsterMoves(int monstSenseDist, int[] pos,
	                            bool monsterSensedPC)
	{
		// range for which monsters do not act
		int noMoveRange =  (int)(1.5 * monstSenseDist);

		// Monster randomly walks
		if (monsterSensedPC == false)
		{
			// Don't have monsters that are far away think 
			if (pos[(int)EntVecVal.distG] >= noMoveRange)
			{
				Debug.Log("Monster Logic short-circuited as dist = " +
				          pos[(int)EntVecVal.distG] + " which is >= " +
				          noMoveRange + " noMoveRange");
				EndTurn();
			}
			else
			{
				// randomly pick a direction, or stand still
				int direction = (int)Random.Range(0,5);
				Debug.Log("Monster wanders with value of " + direction);
				switch(direction)
				{
				case (0): //move up
				{
					Debug.Log("Monster moves up");
					if (puppetEntity.Move(Entity.MoveDirection.up))
					{} // No action required				
					else
					{
						EndTurn();
					}
					break;
				}
				case (1): //move right
				{
					Debug.Log("Monster moves right");
					if (puppetEntity.Move(Entity.MoveDirection.right))
					{} // No action required				
					else
					{
						EndTurn();
					}
					break;
				}
				case (2): //move down
				{
					Debug.Log("Monster moves down");
					if (puppetEntity.Move(Entity.MoveDirection.down))
					{} // No action required				
					else
					{
						EndTurn();
					}
					break;
				}
				case (3): //move left
				{
					Debug.Log("Monster moves left");
					if (puppetEntity.Move(Entity.MoveDirection.left))
					{} // No action required				
					else
					{
						EndTurn();
					}
					break;
				}
				case (4): // stays stationary
				{
					EndTurn();
					break;
				}
				// Doubled up stationary cases to make monsters rest more
				case (5): // stays stationary
				{
					EndTurn();
					break;
				}
				}
			}
		}
		// Monster takes intelligent action
		else if (monsterSensedPC == true)
		{
			// Get the vertical/horizontal distance
			int distX = pos[(int)EntVecVal.distX];
			int distY = pos[(int)EntVecVal.distY];
			
			// used for forecasting and leaving bread crumbs in deciding where
			// to move
			int north = 1;
			int east = 1;
			int south = -1;
			int west = -1;
			int zero = 0;
			
			// See if player is more vertical or horizontal
			if (distX > distY) // horizitonal
			{
				// East (move right) vs West (move left)
				switch (entVec[(int)EntVecVal.entDir])
				{
					// EAST Weighted (move east/to the right)
				case ((int)Bearing.ENE):
				{
					// Try to move to new tile East, then North, then South
					// If no new tile, move west
					if ((!(isMonsterRevisiting(pos, east, zero)))  &&
					    (puppetEntity.Move(Entity.MoveDirection.right)))
					{
						Debug.Log ("PC is: ENE at " +
						           entVec[(int)EntVecVal.distG] + " moving E");
						updatePrevTile(pos, east, zero);
					}
					else if ((!(isMonsterRevisiting(pos, zero, north)))  &&
					         (puppetEntity.Move(Entity.MoveDirection.up)))
					{
						Debug.Log ("PC is: ENE at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving N, 2nd choice");
						updatePrevTile(pos, zero, north);
					}
					else if ((!(isMonsterRevisiting(pos, zero, south))) &&
					         (puppetEntity.Move(Entity.MoveDirection.down)))
					{
						Debug.Log ("PC is: ENE at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving S, 3rd choice");
						updatePrevTile(pos, zero, south);
					}
					else //Retread and hope for better path
					{
						if(puppetEntity.Move(Entity.MoveDirection.left))
						{
							Debug.Log ("PC is: ENE at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving W, 4th choice");
							updatePrevTile(pos, west, zero);
						}
						else // Reset AI
						{
							Debug.Log("AI Reset: ENE");
							EndTurn ();
							updatePrevTile(pos, zero, zero);
						}
					}
					break;
				}
				case ((int)Bearing.E):
				{
					// randomizes vertical (0) or horizontal (1) movement chosen
					int direction = Mathf.RoundToInt(Random.value);
					
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY] +
					           "\nDirection = " + direction);
					// Try to move to new tile East, then North or South,
					// If no new tile, move west
					if ((!(isMonsterRevisiting(pos, east, zero)))  &&
					    (puppetEntity.Move(Entity.MoveDirection.right)))
					{
						Debug.Log ("PC is: E at " +
						           entVec[(int)EntVecVal.distG] + " moving E");
						updatePrevTile(pos, east, zero);
					}
					// Checks if north or south is being revisited
					else if ((!(isMonsterRevisiting(pos, zero, north))) ||
					         (!(isMonsterRevisiting(pos, zero, south))))
					{
						// both not revisited? Randomly choose direction to move
						if ((!(isMonsterRevisiting(pos, zero, north))) &&
						    (!(isMonsterRevisiting(pos, zero, south))))
						{
							if ((direction == north) &&
							    (puppetEntity.Move(Entity.MoveDirection.up)))
							{
								Debug.Log ("PC is: E at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving N, random 2nd choice");
								updatePrevTile(pos, zero, north);
							}
							// zero to save another random number generation
							else if ((direction == zero) &&
							         (puppetEntity.Move(Entity.MoveDirection.down)))
							{
								Debug.Log ("PC is: E at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving S, random 2nd choice");
								updatePrevTile(pos, zero, south);
							}
							else
							{
								Debug.Log(" Monster couldn't move where wanted");
								EndTurn();
							}
						}
						// Either North or South failed Revisiting Check
						// The one that did not fail is the move to make
						else if ((!(isMonsterRevisiting(pos, zero, north)))
						         && (puppetEntity.Move(Entity.MoveDirection.up)))
						{
							Debug.Log ("PC is: E at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving N, 2nd choice");
							updatePrevTile(pos, zero, north);
						}
						else if ((!(isMonsterRevisiting(pos, zero, south))) &&
						         (puppetEntity.Move(Entity.MoveDirection.down)))
						{
							Debug.Log ("PC is: E at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving S, 2nd choice");
							updatePrevTile(pos, zero, south);
						}
						else //Retread and hope for better path
						{
							if(puppetEntity.Move(Entity.MoveDirection.left))
							{
								Debug.Log ("PC is: E at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving W, 3rd choice");
								updatePrevTile(pos, west, zero);
							}
							else // Reset AI
							{
								Debug.Log("AI Reset: E");
								EndTurn ();
								updatePrevTile(pos, zero, zero);
							}
						}
					}
					break;
				}
				case ((int)Bearing.ESE):
				{
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY]);
					// Try to move to new tile East, then South, then North,
					// If no new tile, move west
					if ((!(isMonsterRevisiting(pos, east, zero)))  &&
					    (puppetEntity.Move(Entity.MoveDirection.right)))
					{
						Debug.Log ("PC is: ESE at " +
						           entVec[(int)EntVecVal.distG] + " moving E");
						updatePrevTile(pos, east, zero);
					}					
					else if ((!(isMonsterRevisiting(pos, zero, south))) &&
					         (puppetEntity.Move(Entity.MoveDirection.down)))
					{
						Debug.Log ("PC is: ESE at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving S, 2nd choice");
						updatePrevTile(pos, zero, south);
					}
					else if ((!(isMonsterRevisiting(pos, zero, north)))  &&
					         (puppetEntity.Move(Entity.MoveDirection.up)))
					{
						Debug.Log ("PC is: ESE at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving N, 3nd choice");
						updatePrevTile(pos, zero, north);
					}
					else //Retread and hope for better path
					{
						if(puppetEntity.Move(Entity.MoveDirection.left))
						{
							Debug.Log ("PC is: ESE at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving W, 4th choice");
							updatePrevTile(pos, west, zero);
						}
						else // Reset AI
						{
							Debug.Log("AI Reset: ESE");
							EndTurn ();
							updatePrevTile(pos, zero, zero);
						}
					}
					break;
				}				
					// WEST Weighted (move west/to the left
				case ((int)Bearing.WSW):
				{
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY]);
					// Try to move to new tile West, then South, then North,
					// If no new tile, move East
					if ((!(isMonsterRevisiting(pos, west, zero)))  &&
					    (puppetEntity.Move(Entity.MoveDirection.left)))
					{
						Debug.Log ("PC is: WSW at " +
						           entVec[(int)EntVecVal.distG] + " moving W");
						updatePrevTile(pos, west, zero);
					}					
					else if ((!(isMonsterRevisiting(pos, zero, south))) &&
					         (puppetEntity.Move(Entity.MoveDirection.down)))
					{
						Debug.Log ("PC is: WSW at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving S, 2nd choice");
						updatePrevTile(pos, zero, south);
					}
					else if ((!(isMonsterRevisiting(pos, zero, north)))  &&
					         (puppetEntity.Move(Entity.MoveDirection.up)))
					{
						Debug.Log ("PC is: WSW at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving N, 3nd choice");
						updatePrevTile(pos, zero, north);
					}
					else //Retread and hope for better path
					{
						if(puppetEntity.Move(Entity.MoveDirection.right))
						{
							puppetEntity.Move(Entity.MoveDirection.right);					
							Debug.Log ("PC is: WSW at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving E, 4th choice");
							updatePrevTile(pos, east, zero);
						}
						else // Reset AI
						{
							Debug.Log("AI Reset: WSW");
							EndTurn ();
							updatePrevTile(pos, zero, zero);
						}
					}
					break;
				}
				case ((int)Bearing.W):
				{
					// randomizes vertical (0) or horizontal (1) movement chosen
					int direction = Mathf.RoundToInt(Random.value);
					
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY] +
					           "\nDirection = " + direction);
					// Try to move to new tile West, then North or South,
					// If no new tile, move East
					if ((!(isMonsterRevisiting(pos, west, zero)))  &&
					    (puppetEntity.Move(Entity.MoveDirection.left)))
					{
						Debug.Log ("PC is: W at " +
						           entVec[(int)EntVecVal.distG] + " moving W");
						updatePrevTile(pos, west, zero);
					}
					// Checks if north or south is being revisited
					else if ((!(isMonsterRevisiting(pos, zero, north))) ||
					         (!(isMonsterRevisiting(pos, zero, south))))
					{
						// both not revisited? Randomly choose direction to move
						if ((!(isMonsterRevisiting(pos, zero, north))) &&
						    (!(isMonsterRevisiting(pos, zero, south))))
						{
							if ((direction == north) &&
							    (puppetEntity.Move(Entity.MoveDirection.up)))
							{
								Debug.Log ("PC is: W at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving N, random 2nd choice");
								updatePrevTile(pos, zero, north);
							}
							// zero to save another random number generation
							else if ((direction == zero) &&
							         (puppetEntity.Move(Entity.MoveDirection.down)))
							{
								Debug.Log ("PC is: W at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving S, random 2nd choice");
								updatePrevTile(pos, zero, south);
							}
							else
							{
								Debug.Log(" Monster couldn't move where wanted");
								EndTurn();
							}
						}
						// Either North or South failed Revisiting Check
						// The one that did not fail is the move to make
						else if ((!(isMonsterRevisiting(pos, zero, north)))
						         && (puppetEntity.Move(Entity.MoveDirection.up)))
						{
							Debug.Log ("PC is: W at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving N, 2nd choice");
							updatePrevTile(pos, zero, north);
						}
						else if ((!(isMonsterRevisiting(pos, zero, south))) &&
						         (puppetEntity.Move(Entity.MoveDirection.down)))
						{
							Debug.Log ("PC is: W at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving S, 2nd choice");
							updatePrevTile(pos, zero, south);
						}
						else //Retread and hope for better path
						{
							if(puppetEntity.Move(Entity.MoveDirection.right))
							{
								Debug.Log ("PC is: W at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving E, 3rd choice");
								updatePrevTile(pos, east, zero);
							}
							else // Reset AI
							{
								Debug.Log("AI Reset: W");
								EndTurn ();
								updatePrevTile(pos, zero, zero);
							}
						}
					}
					break;
				}
				case ((int)Bearing.WNW):
				{
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY]);
					// Try to move to new tile West, then North, then South,
					// If no new tile, move East
					if ((!(isMonsterRevisiting(pos, west, zero)))  &&
					    (puppetEntity.Move(Entity.MoveDirection.left)))
					{
						Debug.Log ("PC is: WNW at " +
						           entVec[(int)EntVecVal.distG] + " moving W");
						updatePrevTile(pos, west, zero);
					}					
					else if ((!(isMonsterRevisiting(pos, zero, north)))  &&
					         (puppetEntity.Move(Entity.MoveDirection.up)))
					{
						Debug.Log ("PC is: WNW at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving N, 2nd choice");
						updatePrevTile(pos, zero, north);
					}
					else if ((!(isMonsterRevisiting(pos, zero, south))) &&
					         (puppetEntity.Move(Entity.MoveDirection.down)))
					{
						Debug.Log ("PC is: WNW at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving S, 3nd choice");
						updatePrevTile(pos, zero, south);
					}
					else //Retread and hope for better path
					{
						if(puppetEntity.Move(Entity.MoveDirection.right))
						{
							Debug.Log ("PC is: WNW at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving E, 4th choice");
							updatePrevTile(pos, east, zero);
						}
						else // Reset AI
						{
							Debug.Log("AI Reset: WNW");
							EndTurn ();
							updatePrevTile(pos, zero, zero);
						}
					}
					break;
				}
				}
			}
			else if (distY > distX) // vertical
			{
				// North (move up) vs South (move down)
				switch (entVec[(int)EntVecVal.entDir])
				{
					// North Weighted (move North/to the up)
				case ((int)Bearing.NNW):
				{
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY]);
					// Try to move to new tile North, then West, then South,
					// If no new tile, move East
					if ((!(isMonsterRevisiting(pos, zero, north)))  &&
					    (puppetEntity.Move(Entity.MoveDirection.up)))
					{
						Debug.Log ("PC is: NNW at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving N");
						updatePrevTile(pos, zero, north);
					}					
					else if ((!(isMonsterRevisiting(pos, west, zero)))  &&
					         (puppetEntity.Move(Entity.MoveDirection.left)))
					{
						Debug.Log ("PC is: NNW at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving W, 2nd choice");
						updatePrevTile(pos, west, zero);
					}
					else if ((!(isMonsterRevisiting(pos, zero, south))) &&
					         (puppetEntity.Move(Entity.MoveDirection.down)))
					{
						Debug.Log ("PC is: NNW at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving S, 3nd choice");
						updatePrevTile(pos, zero, south);
					}
					else //Retread and hope for better path
					{
						if(puppetEntity.Move(Entity.MoveDirection.right))
						{
							Debug.Log ("PC is: NNW at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving E, 4th choice");
							updatePrevTile(pos, east, zero);
						}
						else // Reset AI
						{
							Debug.Log("AI Reset: NNW");
							EndTurn ();
							updatePrevTile(pos, zero, zero);
						}
					}
					break;
				}
				case ((int)Bearing.N):
				{
					// randomizes vertical (0) or horizontal (1) movement chosen
					int direction = Mathf.RoundToInt(Random.value);
					
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY] +
					           "\nDirection = " + direction);
					// Try to move to new tile North, then East or West,
					// If no new tile, move South
					if ((!(isMonsterRevisiting(pos, zero, north)))  &&
					    (puppetEntity.Move(Entity.MoveDirection.up)))
					{
						Debug.Log ("PC is: N at " +
						           entVec[(int)EntVecVal.distG] + " moving N");
						updatePrevTile(pos, zero, north);
					}
					// Checks if east or west is being revisited
					else if ((!(isMonsterRevisiting(pos, east, zero))) ||
					         (!(isMonsterRevisiting(pos, west, zero))))
					{
						// both not revisited? Randomly choose direction to move
						if ((!(isMonsterRevisiting(pos, east, zero))) &&
						    (!(isMonsterRevisiting(pos, west, zero))))
						{
							if ((direction == east) &&
							    (puppetEntity.Move(Entity.MoveDirection.right)))
							{
								Debug.Log ("PC is: N at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving E, random 2nd choice");
								updatePrevTile(pos, east, zero);
							}
							// zero to save another random number generation
							else if ((direction == zero) &&
							         (puppetEntity.Move(Entity.MoveDirection.left)))
							{
								Debug.Log ("PC is: N at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving W, random 2nd choice");
								updatePrevTile(pos, west, zero);
							}
							else
							{
								Debug.Log(" Monster couldn't move where wanted");
								EndTurn();
							}
						}
						// Either East or West failed Revisiting Check
						// The one that did not fail is the move to make
						else if ((!(isMonsterRevisiting(pos, east, zero)))
						         && (puppetEntity.Move(Entity.MoveDirection.right)))
						{
							Debug.Log ("PC is: N at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving E, 2nd choice");
							updatePrevTile(pos, east, zero);
						}
						else if ((!(isMonsterRevisiting(pos, west, zero))) &&
						         (puppetEntity.Move(Entity.MoveDirection.left)))
						{
							Debug.Log ("PC is: N at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving W, 2nd choice");
							updatePrevTile(pos, west, zero);
						}
						else //Retread and hope for better path
						{
							if(puppetEntity.Move(Entity.MoveDirection.down))
							{
								Debug.Log ("PC is: N at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving S, 3rd choice");
								updatePrevTile(pos, zero, south);
							}
							else // Reset AI
							{
								Debug.Log("AI Reset: N");
								EndTurn ();
								updatePrevTile(pos, zero, zero);
							}
						}
					}
					break;
				}
				case ((int)Bearing.NNE):
				{
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY]);
					// Try to move to new tile North, then East, then South,
					// If no new tile, move West
					if ((!(isMonsterRevisiting(pos, zero, north)))  &&
					    (puppetEntity.Move(Entity.MoveDirection.up)))
					{
						Debug.Log ("PC is: NNE at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving N");
						updatePrevTile(pos, zero, north);
					}					
					else if ((!(isMonsterRevisiting(pos, east, zero)))  &&
					         (puppetEntity.Move(Entity.MoveDirection.right)))
					{
						Debug.Log ("PC is: NNE at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving E, 2nd choice");
						updatePrevTile(pos, east, zero);
					}
					else if ((!(isMonsterRevisiting(pos, zero, south))) &&
					         (puppetEntity.Move(Entity.MoveDirection.down)))
					{
						Debug.Log ("PC is: NNE at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving S, 3nd choice");
						updatePrevTile(pos, zero, south);
					}
					else //Retread and hope for better path
					{
						if(puppetEntity.Move(Entity.MoveDirection.left))
						{
							Debug.Log ("PC is: NNE at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving W, 4th choice");
							updatePrevTile(pos, west, zero);
						}
						else // Reset AI
						{
							Debug.Log("AI Reset: NNE");
							EndTurn ();
							updatePrevTile(pos, zero, zero);
						}
					}
					break;
				}				
					// South Weighted (move South/to the down
				case ((int)Bearing.SSW):
				{
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY]);
					// Try to move to new tile South, then West, then North,
					// If no new tile, move East
					if ((!(isMonsterRevisiting(pos, zero, south))) &&
					    (puppetEntity.Move(Entity.MoveDirection.down)))
					{
						Debug.Log ("PC is: SSW at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving S");
						updatePrevTile(pos, zero, south);
					}					
					else if ((!(isMonsterRevisiting(pos, west, zero)))  &&
					         (puppetEntity.Move(Entity.MoveDirection.left)))
					{
						Debug.Log ("PC is: SSW at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving W, 2nd choice");
						updatePrevTile(pos, west, zero);
					}
					else if ((!(isMonsterRevisiting(pos, zero, north)))  &&
					         (puppetEntity.Move(Entity.MoveDirection.up)))
					{
						Debug.Log ("PC is: SSW at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving N, 3nd choice");
						updatePrevTile(pos, zero, north);
					}
					else //Retread and hope for better path
					{
						if(puppetEntity.Move(Entity.MoveDirection.right))
						{
							Debug.Log ("PC is: SSW at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving E, 4th choice");
							updatePrevTile(pos, east, zero);
						}
						else // Reset AI
						{
							Debug.Log("AI Reset: SSW");
							EndTurn ();
							updatePrevTile(pos, zero, zero);
						}
					}
					break;
				}
				case ((int)Bearing.S):
				{
					// randomizes vertical (0) or horizontal (1) movement chosen
					int direction = Mathf.RoundToInt(Random.value);
					
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY] +
					           "\nDirection = " + direction);
					// Try to move to new tile South, then East or West,
					// If no new tile, move North
					if ((!(isMonsterRevisiting(pos, zero, south)))  &&
					    (puppetEntity.Move(Entity.MoveDirection.down)))
					{
						Debug.Log ("PC is: S at " +
						           entVec[(int)EntVecVal.distG] + " moving S");
						updatePrevTile(pos, zero, south);
					}
					// Checks if east or west is being revisited
					else if ((!(isMonsterRevisiting(pos, east, zero))) ||
					         (!(isMonsterRevisiting(pos, west, zero))))
					{
						// both not revisited? Randomly choose direction to move
						if ((!(isMonsterRevisiting(pos, east, zero))) &&
						    (!(isMonsterRevisiting(pos, west, zero))))
						{
							if ((direction == east) &&
							    (puppetEntity.Move(Entity.MoveDirection.right)))
							{
								Debug.Log ("PC is: S at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving E, random 2nd choice");
								updatePrevTile(pos, east, zero);
							}
							// zero to save another random number generation
							else if ((direction == zero) &&
							         (puppetEntity.Move(Entity.MoveDirection.left)))
							{
								Debug.Log ("PC is: S at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving W, random 2nd choice");
								updatePrevTile(pos, west, zero);
							}
							else
							{
								Debug.Log(" Monster couldn't move where wanted");
								EndTurn();
							}
						}
						// Either East or West failed Revisiting Check
						// The one that did not fail is the move to make
						else if ((!(isMonsterRevisiting(pos, east, zero)))
						         && (puppetEntity.Move(Entity.MoveDirection.right)))
						{
							Debug.Log ("PC is: S at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving E, 2nd choice");
							updatePrevTile(pos, east, zero);
						}
						else if ((!(isMonsterRevisiting(pos, west, zero))) &&
						         (puppetEntity.Move(Entity.MoveDirection.left)))
						{
							Debug.Log ("PC is: S at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving W, 2nd choice");
							updatePrevTile(pos, west, zero);
						}
						else //Retread and hope for better path
						{
							if(puppetEntity.Move(Entity.MoveDirection.up))
							{
								Debug.Log ("PC is: S at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving N, 3rd choice");
								updatePrevTile(pos, zero, north);
							}
							else // Reset AI
							{
								Debug.Log("AI Reset: S");
								EndTurn ();
								updatePrevTile(pos, zero, zero);
							}
						}
					}
					break;
				}
				case ((int)Bearing.SSE):
				{
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY]);
					// Try to move to new tile South, then East, then North,
					// If no new tile, move West
					if ((!(isMonsterRevisiting(pos, zero, south))) &&
					    (puppetEntity.Move(Entity.MoveDirection.down)))
					{
						Debug.Log ("PC is: SSE at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving S");
						updatePrevTile(pos, zero, south);
					}					
					else if ((!(isMonsterRevisiting(pos, east, zero)))  &&
					         (puppetEntity.Move(Entity.MoveDirection.right)))
					{
						Debug.Log ("PC is: SSE at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving E, 2nd choice");
						updatePrevTile(pos, east, zero);
					}
					else if ((!(isMonsterRevisiting(pos, zero, north)))  &&
					         (puppetEntity.Move(Entity.MoveDirection.up)))
					{
						Debug.Log ("PC is: SSE at " +
						           entVec[(int)EntVecVal.distG] +
						           " moving N, 3nd choice");
						updatePrevTile(pos, zero, north);
					}
					else //Retread and hope for better path
					{
						if(puppetEntity.Move(Entity.MoveDirection.left))
						{
							Debug.Log ("PC is: SSE at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving W, 4th choice");
							updatePrevTile(pos, west, zero);
						}
						else // Reset AI
						{
							Debug.Log("AI Reset: SSE");
							EndTurn ();
							updatePrevTile(pos, zero, zero);
						}
					}
					break;
				}
				}
			}
			else // same vert as horizontal (the diagonals)
			{
				// randomizes vertical (0) or horizontal (1) movement chosen
				int direction = Mathf.RoundToInt(Random.value);
				
				// Which diagonal
				switch (entVec[(int)EntVecVal.entDir])
				{
				case ((int)Bearing.NE):
				{
					// randomizes vertical (0) or horizontal (1) movement chosen
					direction = Mathf.RoundToInt(Random.value);
					
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY] +
					           "\nDirection = " + direction);
					// Try to move to new tile East or North,
					// If no new tile, move West or South
					if ((!(isMonsterRevisiting(pos, east, zero))) ||
					    (!(isMonsterRevisiting(pos, zero, north))))
					{
						// both not revisited? Randomly choose direction to move
						if ((!(isMonsterRevisiting(pos, east, zero))) &&
						    (!(isMonsterRevisiting(pos, zero, north))))
						{
							if ((direction == east) &&
							    (puppetEntity.Move(Entity.MoveDirection.right)))
							{
								Debug.Log ("PC is: NE at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving E, random 1st choice");
								updatePrevTile(pos, east, zero);
							}
							// zero to save another random number generation
							else if ((direction == zero) &&
							         (puppetEntity.Move(Entity.MoveDirection.up)))
							{
								Debug.Log ("PC is: NE at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving N, random 1st choice");
								updatePrevTile(pos, zero, north);
							}
							else // Have to retreat
							{
								Debug.Log("In NE else clause with dir " + direction);
								// Randomly choose direction to move
								if (direction == zero)
								{
									if (puppetEntity.Move(Entity.MoveDirection.left))
									{
										Debug.Log ("PC is: NE at " +
									           entVec[(int)EntVecVal.distG] +
									           " moving W, random 2nd choice");
										updatePrevTile(pos, west, zero);
									}
									else if (puppetEntity.Move(Entity.MoveDirection.down))
									{
										Debug.Log ("PC is: NE at " +
										           entVec[(int)EntVecVal.distG] +
										           " moving S, random 3rd choice");
										updatePrevTile(pos, zero, south);
									}
									else // Reset AI
									{
										Debug.Log("AI Reset: NE, inner else");
										EndTurn ();
										updatePrevTile(pos, zero, zero);
									}
								}
							}						
						}
						// Either East or North failed Revisiting Check
						// The one that did not fail is the move to make
						else if ((!(isMonsterRevisiting(pos, east, zero)))
						         && (puppetEntity.Move(Entity.MoveDirection.right)))
						{
							Debug.Log ("PC is: NE at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving E, 2nd choice");
							updatePrevTile(pos, east, zero);
						}
						else if ((!(isMonsterRevisiting(pos, zero, north))) &&
						         (puppetEntity.Move(Entity.MoveDirection.up)))
						{
							Debug.Log ("PC is: NE at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving N, 2nd choice");
							updatePrevTile(pos, zero, north);
						}
						else // Reset AI
						{
							Debug.Log("AI Reset: NE, middle else");
							EndTurn ();
							updatePrevTile(pos, zero, zero);
						}
					}
					else // Reset AI
					{
						Debug.Log("AI Reset: NE, outer else");
						EndTurn ();
						updatePrevTile(pos, zero, zero);
					}
					break;
				}
				case ((int)Bearing.SE):
				{
					// randomizes vertical (0) or horizontal (1) movement chosen
					direction = Mathf.RoundToInt(Random.value);
					
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY] +
					           "\nDirection = " + direction);
					// Try to move to new tile East or North,
					// If no new tile, move West or South
					if ((!(isMonsterRevisiting(pos, east, zero))) ||
					    (!(isMonsterRevisiting(pos, zero, north))))
					{
						// both not revisited? Randomly choose direction to move
						if ((!(isMonsterRevisiting(pos, east, zero))) &&
						    (!(isMonsterRevisiting(pos, zero, north))))
						{
							if ((direction == east) &&
							    (puppetEntity.Move(Entity.MoveDirection.right)))
							{
								Debug.Log ("PC is: SE at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving E, random 1st choice");
								updatePrevTile(pos, east, zero);
							}
							// zero to save another random number generation
							else if ((direction == zero) &&
							         (puppetEntity.Move(Entity.MoveDirection.up)))
							{
								Debug.Log ("PC is: SE at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving N, random 1st choice");
								updatePrevTile(pos, zero, north);
							}
							else // Have to retreat
							{
								Debug.Log("In SE else clause with dir " + direction);
								// Randomly choose direction to move
								if (direction == zero)
								{
									if (puppetEntity.Move(Entity.MoveDirection.left))
									{
										Debug.Log ("PC is: SE at " +
										           entVec[(int)EntVecVal.distG] +
										           " moving W, random 2nd choice");
										updatePrevTile(pos, west, zero);
									}
									else if (puppetEntity.Move(Entity.MoveDirection.down))
									{
										Debug.Log ("PC is: SE at " +
										           entVec[(int)EntVecVal.distG] +
										           " moving S, random 3rd choice");
										updatePrevTile(pos, zero, south);
									}
									else // Reset AI
									{
										Debug.Log ("AI Reset: SE, inner else");
									}
								}
							}						
						}
						// Either East or North failed Revisiting Check
						// The one that did not fail is the move to make
						else if ((!(isMonsterRevisiting(pos, east, zero)))
						         && (puppetEntity.Move(Entity.MoveDirection.right)))
						{
							Debug.Log ("PC is: SE at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving E, 2nd choice");
							updatePrevTile(pos, east, zero);
						}
						else if ((!(isMonsterRevisiting(pos, zero, north))) &&
						         (puppetEntity.Move(Entity.MoveDirection.up)))
						{
							Debug.Log ("PC is: SE at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving N, 2nd choice");
							updatePrevTile(pos, zero, north);
						}
						else // Reset AI
						{
							Debug.Log("AI Reset: SE, middle else");
							EndTurn ();
							updatePrevTile(pos, zero, zero);
						}
					}
					else // Reset AI
					{
						Debug.Log("AI Reset: SE, outer else");
						EndTurn ();
						updatePrevTile(pos, zero, zero);
					}
					break;
				}
				case ((int)Bearing.SW):
				{
					// randomizes vertical (0) or horizontal (1) movement chosen
					direction = Mathf.RoundToInt(Random.value);
					
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY] +
					           "\nDirection = " + direction);
					// Try to move to new tile West or South,
					// If no new tile, move west or North
					if ((!(isMonsterRevisiting(pos, west, zero))) ||
					    (!(isMonsterRevisiting(pos, zero, south))))
					{
						// both not revisited? Randomly choose direction to move
						if ((!(isMonsterRevisiting(pos, west, zero))) &&
						    (!(isMonsterRevisiting(pos, zero, south))))
						{
							if ((direction == (Mathf.Abs(west))) &&
							    (puppetEntity.Move(Entity.MoveDirection.left)))
							{
								Debug.Log ("PC is: SW at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving W, random 1st choice");
								updatePrevTile(pos, west, zero);
							}
							// zero to save another random number generation
							else if ((direction == zero) &&
							         (puppetEntity.Move(Entity.MoveDirection.down)))
							{
								Debug.Log ("PC is: SW at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving S, random 1st choice");
								updatePrevTile(pos, zero, south);
							}
							else // Have to retreat
							{
								Debug.Log("In SW else clause with dir " + direction);
								// Randomly choose direction to move
								if (direction == zero)
								{
									if (puppetEntity.Move(Entity.MoveDirection.left))
									{
										Debug.Log ("PC is: SW at " +
										           entVec[(int)EntVecVal.distG] +
										           " moving W, random 2nd choice");
										updatePrevTile(pos, west, zero);
									}
									else if (puppetEntity.Move(Entity.MoveDirection.down))
									{
										Debug.Log ("PC is: SW at " +
										           entVec[(int)EntVecVal.distG] +
										           " moving S, random 3rd choice");
										updatePrevTile(pos, zero, south);
									}
									else // Reset AI
									{
										Debug.Log ("AI Reset: SW, inner else");
										EndTurn();
										updatePrevTile(pos, zero, zero);
									}
								}
							}						
						}
						// Either west or south failed Revisiting Check
						// The one that did not fail is the move to make
						else if ((!(isMonsterRevisiting(pos, west, zero)))
						         && (puppetEntity.Move(Entity.MoveDirection.left)))
						{
							Debug.Log ("PC is: SW at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving E, 2nd choice");
							updatePrevTile(pos, west, zero);
						}
						else if ((!(isMonsterRevisiting(pos, zero, south))) &&
						         (puppetEntity.Move(Entity.MoveDirection.down)))
						{
							Debug.Log ("PC is: SW at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving N, 2nd choice");
							updatePrevTile(pos, zero, south);
						}
						else // Reset AI
						{
							Debug.Log("AI Reset: SW, middle else");
							EndTurn ();
							updatePrevTile(pos, zero, zero);
						}
					}
					else // Reset AI
					{
						Debug.Log("AI Reset: SW, outer else");
						EndTurn ();
						updatePrevTile(pos, zero, zero);
					}
					break;
				}
				case ((int)Bearing.NW):
				{
					// randomizes vertical (0) or horizontal (1) movement chosen
					direction = Mathf.RoundToInt(Random.value);
					
					Debug.Log ("New prevX = " + pos[(int)EntVecVal.prevX] +
					           "\nNew prevY = " + pos[(int)EntVecVal.prevY] +
					           "\nDirection = " + direction);
					// Try to move to new tile West or North,
					// If no new tile, move west or South
					if ((!(isMonsterRevisiting(pos, west, zero))) ||
					    (!(isMonsterRevisiting(pos, zero, north))))
					{
						// both not revisited? Randomly choose direction to move
						if ((!(isMonsterRevisiting(pos, west, zero))) &&
						    (!(isMonsterRevisiting(pos, zero, north))))
						{
							if ((direction == (Mathf.Abs(west))) &&
							    (puppetEntity.Move(Entity.MoveDirection.left)))
							{
								Debug.Log ("PC is: NW at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving W, random 1st choice");
								updatePrevTile(pos, west, zero);
							}
							// zero to save another random number generation
							else if ((direction == zero) &&
							         (puppetEntity.Move(Entity.MoveDirection.up)))
							{
								Debug.Log ("PC is: NW at " +
								           entVec[(int)EntVecVal.distG] +
								           " moving N, random 1st choice");
								updatePrevTile(pos, zero, north);
							}
							else // Have to retreat
							{
								Debug.Log("In NW else clause with dir " + direction);
								// Randomly choose direction to move
								if (direction == zero)
								{
									if (puppetEntity.Move(Entity.MoveDirection.left))
									{
										Debug.Log ("PC is: NW at " +
										           entVec[(int)EntVecVal.distG] +
										           " moving W, random 2nd choice");
										updatePrevTile(pos, west, zero);
									}
									else if (puppetEntity.Move(Entity.MoveDirection.down))
									{
										Debug.Log ("PC is: NW at " +
										           entVec[(int)EntVecVal.distG] +
										           " moving S, random 3rd choice");
										updatePrevTile(pos, zero, south);
									}
								}
								else // Reset AI
								{
									Debug.Log ("AI Reset: NW, inner else");
									EndTurn();
									updatePrevTile(pos, zero, zero);
								}
							}						
						}
						// Either west or North failed Revisiting Check
						// The one that did not fail is the move to make
						else if ((!(isMonsterRevisiting(pos, west, zero)))
						         && (puppetEntity.Move(Entity.MoveDirection.left)))
						{
							Debug.Log ("PC is: NW at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving E, 2nd choice");
							updatePrevTile(pos, west, zero);
						}
						else if ((!(isMonsterRevisiting(pos, zero, north))) &&
						         (puppetEntity.Move(Entity.MoveDirection.up)))
						{
							Debug.Log ("PC is: NW at " +
							           entVec[(int)EntVecVal.distG] +
							           " moving N, 2nd choice");
							updatePrevTile(pos, zero, north);
						}
						else // Reset AI
						{
							// Bug fix, otherwise AI gets stuck (out of state?)
							Debug.Log("AI Reset: NW, middle else");
							EndTurn ();
							updatePrevTile(pos, zero, zero);
						}
					}
					else // Reset AI
					{
						Debug.Log("AI Reset: NW, outer else");
						EndTurn ();
						updatePrevTile(pos, zero, zero);
					}
					break;
				}
				}
			}
		}
	}
}