using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class Grid : NetworkBehaviour {

    public ScoreText scoreText;
    public TimeText timeText;
    public GameObject destoryParticle;
    public AudioClip destroySound;
	public AudioClip bird;
	public AudioClip crab;
	public AudioClip dolphin;
	public AudioClip duck;
	public AudioClip bear;
    public AudioClip hawk;

    public int localSer = 0;
    string highScoreKey = "HighScore";
    public int score = 0;
    public int highscore = 0;
    public int serverVar = 0;
    public int clientVar = 0;

    bool insertOther = false;

    [SyncVar]
    public int clientTilesRemoved=0;

    [SyncVar]
    public int serverTilesRemoved=0;

    //used to tell if the grid is done automatically clearing itself. 
    //once it is done clearing itself, we can start accumulating points and keeping track of how many tiles have been cleared
    private bool everythingSet = false;

    //Kind of Piece (i.e normal piece, special piece, blocking piece, empty piece)
    // Also stores how many different types of pieces exist
    public enum PieceType
	{
		EMPTY,
		NORMAL,
		BUBBLE,
		COUNT,
	};

	//since we cant see a Dictionary in the inspector, we create this struct
	//and have an array of thse instead
	[System.Serializable] //flag that is set to enables us to see this struct in the Inspector
	public struct PiecePrefab
	{
		public PieceType type;
		public GameObject prefab;
	};

	//dimensions of the game board
	public int xDim;
	public int yDim;
    public int level; //how many empty rows there are
    public float fillTime;
    public float scale; //how to scale the pieces (used for when more rows are added and need to be fit to the screen)

	//array of Prefabs viewable in the inspector
	public PiecePrefab[] piecePrefabs;

	//used to set the Background Prefab in the Inspector
	public GameObject backgroundPrefab;

	//Dictionary with Keys of PieceType and Values of type GameObject
	//Dictionaries cant be displayed in the inspector
	private Dictionary<PieceType, GameObject> piecePrefabDict;

	//array to hold all pieces
	private GamePiece[,] pieces;

	//bool used to change directions when searching for matches
	private bool inverse = false;

	//store which piece was pressed and which piece we scrolled over with our mouse
	private GamePiece pressedPiece;
	private GamePiece enteredPiece;

	// Use this for initialization
	void Start ()
    {
        if (!isLocalPlayer)
            return;

            if (isServer)
            {
                print("i am the server");
            }
            score = PlayerPrefs.GetInt(highScoreKey, 0);

            if (!isServer)
            {
                print("i am the client");
            }

            //scoreText = new ScoreText();
            scoreText = GameObject.FindGameObjectWithTag("Player").GetComponent<ScoreText>();
            timeText = GameObject.FindGameObjectWithTag("Player").GetComponent<TimeText>();

            //add all of our different Prefabs into the Dictionary
            piecePrefabDict = new Dictionary<PieceType, GameObject>();

            //check to make sure that there is only one copy of the Prefab in the Dictionary
            for (int i = 0; i < piecePrefabs.Length; i++)
            {
                if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type))
                {
                    piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
                }
            }

            //Fill the gameboard with the Background GameObject
            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    GameObject background = (GameObject)Instantiate(backgroundPrefab, GetWorldPosition(x, y), Quaternion.identity);
                    background.transform.parent = transform; //make the background a child of the grid object
                }
            }

            //Array to  hold all game pieces
            //We will manipulate the contents of this array when matching pieces
            pieces = new GamePiece[xDim, yDim];

            //Fill the gamebard with the empty game Pieces
            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    SpawnNewPiece(x, y, PieceType.EMPTY);
                }
            }

            //remove a piece at the center of the grid and replace it with a bubble piece
            /*Destroy (pieces [4, 4+level].gameObject);
            SpawnNewPiece (4,4+level,PieceType.BUBBLE);*/

            //keep filling the grid until its full
            StartCoroutine(Fill());
        
	}

    [Command]
    void CmdTestCommand()
    {
        //if host, send row to client
        GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Grid>().insertRow();
    }

    [ClientRpc]
    void RpcTestCommand()
    {
        //if client, sends row to host
        GameObject.FindGameObjectsWithTag("Player")[1].GetComponent<Grid>().insertRow();

    }

    // Update is called once per frame
    void Update () {
        if (!isLocalPlayer)
            return;

           /* if (Input.GetKeyDown("h"))
            {
                if(isServer)
                {
                print("hello from local");
                    //RpcinsertRow();
                }
                else
                {
                    print("hello from remote");
                CmdTestCommand();
                //CmdinsertRow();
            }
            }
            */

            if (Input.GetKeyDown("space"))
                //insertRow();
            if (Input.GetKeyDown("space"))
                //insertRow();

            if (Input.GetKeyDown("d"))
                deleteRow(10);
            if (Input.GetKeyDown("r"))
                deleteRow(12);
            if (Input.GetKeyDown("l"))
                deleteRow(level);
    }

	//make filling up the grid take several animation frames
	public IEnumerator Fill()
	{
		bool needsRefill = true;
        timeText.subtractTime();
		while (needsRefill) {
			yield return new WaitForSeconds (fillTime);

			while (FillStep ()) {
				inverse = !inverse;
				yield return new WaitForSeconds (fillTime);
			}

			needsRefill = ClearAllValidMatches ();
		}
        everythingSet = true;
	}

	//function used to fill empty cells
	//it looks to see if the cell below is empty.
	//if it is, move the cell above it down
	public bool FillStep()
	{
		bool movedPiece = false;

		//look for pieces that can move down starting from the 2nd row
		for (int y = yDim-2; y >= level; y--)
		{
			for (int loopX = 0; loopX < xDim; loopX++)
			{
				int x = loopX; //used to traverse from 0 to xDim

				if (inverse) { //if inverse is true, traverse from xDim to 0
					x = xDim - 1 - loopX;
				}

				GamePiece piece = pieces [x, y]; //get the current gamepiece

				//if the current game piece isnt an empty piece or a blocking piece
				if (piece.IsMovable ())
				{
					GamePiece pieceBelow = pieces [x, y + 1];//get the piece below it

					//if the piece below it is empty
					//move the current piece down
					//and create and empty piece in its place
					if (pieceBelow.Type == PieceType.EMPTY) {
						Destroy (pieceBelow.gameObject);
						piece.MovableComponent.Move (x, y + 1, fillTime);
						pieces [x, y + 1] = piece;
						SpawnNewPiece (x, y, PieceType.EMPTY);
						movedPiece = true;
					} else {
						//fill in spots diagonally
						for (int diag = -1; diag <= 1; diag++)
						{
							//if diag ==0 then its checking the piece right below,
							//which we already checked in the above if statement
							if (diag != 0)
							{
								//x-coordinate of our diagonal piece
								//depending on the value of diag, this will be
								//to the right or left of our current piece
								int diagX = x + diag;

								if (inverse)
								{
									diagX = x - diag;
								}

								//check if the x-coordinate of our diagonal piece
								//is on the bounds of our grid
								if (diagX >= 0 && diagX < xDim)
								{
									//if it is get a reference to the diagonal piece
									//which is at y+1 (i.e the row below us)
									GamePiece diagonalPiece = pieces [diagX, y + 1];

									//make sure that the space we are replacing is empty
									if (diagonalPiece.Type == PieceType.EMPTY)
									{
										//check is there is a piece above our digonal piece
										//if there is, then it should just move down to fill the empty space
										//we dont want to move diagoanlly unless there is no pieces above the empty space
										//that can fill it
										bool hasPieceAbove = true;

										//loop through all the pieces above the digonal space
										for (int aboveY = y; aboveY >= level; aboveY--)
										{
											GamePiece pieceAbove = pieces [diagX, aboveY];

											//if we encounter a movable piece, we break
											//since this piece can just move down and fill the empty space
											if (pieceAbove.IsMovable ())
											{
												break;
											}

											//if we encounter an obstacle,
											//this empty space is blocked from above and we wont be able to fill it from above
											else if(!pieceAbove.IsMovable() && pieceAbove.Type != PieceType.EMPTY)
											{
												hasPieceAbove = false;
												break;
											}
										}

										//if the piece cant be filled from above,
										//then we fill it daigonally
										if (!hasPieceAbove)
										{
											Destroy (diagonalPiece.gameObject);
											piece.MovableComponent.Move (diagX, y + 1, fillTime);
											pieces [diagX, y + 1] = piece;
											SpawnNewPiece (x, y, PieceType.EMPTY);
											movedPiece = true;
											break;
										}
									} 
								}
							}
						}
					}
				}
			}
		}

		//special case for when we get to the topmost row.
		//since at this point nothing below it is empty
		//we create a new shape above the screen
		//and if any cell in the top row is empty, we move that new shape down
		for (int x = 0; x < xDim; x++)
		{
			GamePiece pieceBelow = pieces [x, level]; //get current game piece on top row

			//if the cell is empty
			//fill tje cel with a new piece of a random color
			if (pieceBelow.Type == PieceType.EMPTY)
			{
				Destroy (pieceBelow.gameObject);
				GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.NORMAL], GetWorldPosition(x, level-1), Quaternion.identity);
				newPiece.transform.parent = transform;

				pieces [x, level] = newPiece.GetComponent<GamePiece> ();
				pieces [x, level].Init (x, level-1, this, PieceType.NORMAL);
				pieces [x, level].MovableComponent.Move (x, level, fillTime);
				pieces [x, level].ColorComponent.SetColor ((ColorPiece.ColorType)Random.Range (0, pieces [x, level].ColorComponent.NumColors));
				movedPiece = true;
			}
		}

		return movedPiece;
	}

	//function used to change a grid coordinate to a world position 
	public Vector2 GetWorldPosition(int x, int y)
	{
		//get the x position of our grid (i.e the center of our grid)
		//subtract half the width to get the left edge of th grid
		//add the x coordinate
		//do same 3 steps for the y position of our grid
		return new Vector2 ((transform.position.x - xDim / 2.0f + x)*scale,
			(transform.position.y + yDim / 2.0f - y)*scale);
	}

	//used to spawn a new piece when one has been cleared away by matching
	public GamePiece SpawnNewPiece(int x, int y, PieceType type)
	{
		//instantiate the piece
		GameObject newPiece = (GameObject)Instantiate (piecePrefabDict [type], GetWorldPosition (x, y), Quaternion.identity);
		newPiece.transform.parent = transform;

		//initialize the piece
		pieces [x, y] = newPiece.GetComponent<GamePiece> ();
		pieces [x, y].Init (x, y, this, type);

		//return the piece
		return pieces [x, y];
	}

	//checks if 2 pieces are adjacent
	public bool IsAdjacent(GamePiece piece1, GamePiece piece2)
	{
		
		return (piece1.X == piece2.X && (int)Mathf.Abs (piece1.Y - piece2.Y) == 1) //column is the same and they are in adjacent rows
			|| (piece1.Y == piece2.Y && (int)Mathf.Abs (piece1.X - piece2.X) == 1); //row is the same and they are in adjacent columns
	}

	public void SwapPieces(GamePiece piece1, GamePiece piece2)
	{
		//checks if both are movable.
		//make one piece equal the other for when/if they swap later
		if (piece1.IsMovable () && piece2.IsMovable ()) {
			pieces [piece1.X, piece1.Y] = piece2;
			pieces [piece2.X, piece2.Y] = piece1;

			//check if they match
			if (GetMatch (piece1, piece2.X, piece2.Y) != null || GetMatch (piece2, piece1.X, piece1.Y) != null) {

				//retain piecee1 coordinates since they get overwritten
				int piece1X = piece1.X;
				int piece1Y = piece1.Y;

				//swap the pieces
				piece1.MovableComponent.Move (piece2.X, piece2.Y, fillTime);
				piece2.MovableComponent.Move (piece1X, piece1Y, fillTime);

				//if any more matches are made, keep clearing those
				ClearAllValidMatches ();

				// countinously fill the screen until its full
				StartCoroutine (Fill ());
			} 

			//else they didnt match
			//so just make the pieces equal their old values
			else {

                pieces[piece1.X, piece1.Y] = piece1;
                pieces[piece2.X, piece2.Y] = piece2;

                int x1 = piece1.X;
                int y1 = piece1.Y;
                int x2 = piece2.X;
                int y2 = piece2.Y;



                //swap the pieces forward and back
                piece1.MovableComponent.Bounce(true, x1, y1, x2, y2, fillTime);
                piece2.MovableComponent.Bounce(true, x2, y2, x1, y1, fillTime);

               
			}
		}
	}

	//save what piece was pressed 
	public void PressPiece(GamePiece piece)
	{
		pressedPiece = piece;
	}

	//save what piece we scrolled over while holding mouse button
	public void EnterPiece(GamePiece piece)
	{
		enteredPiece = piece;
	}

	//save what piece we released the mouse button over
	public void ReleasePiece()
	{
		//if the two pieces gathered from above two function are adjecent and they match, swap them
		if (IsAdjacent (pressedPiece, enteredPiece)) {
			SwapPieces (pressedPiece, enteredPiece);
		}
	}

	//traverses the grid to check matching pieces
	//adds them to a list to be cleared later
	public List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
	{
		//make sure that the piece is a colored piece and not a blocking piece or empty piece
		if (piece.IsColored ()) {
			ColorPiece.ColorType color = piece.ColorComponent.Color; //get the color of the initial piece
			List<GamePiece> horizontalPieces = new List<GamePiece> (); //hold potential horizontal matches
			List<GamePiece> verticalPieces = new List<GamePiece> (); //hold matential vertical matches
			List<GamePiece> matchingPieces = new List<GamePiece> (); //hold pieces that indeed did match

			// First check horizontal
			horizontalPieces.Add(piece);

			for (int dir = 0; dir <= 1; dir++) {
				for (int xOffset = 1; xOffset < xDim; xOffset++) {
					int x;

					if (dir == 0) { // Left
						x = newX - xOffset;
					} else { // Right
						x = newX + xOffset;
					}

					//if it goes out of bounds of our grid then break
					if (x < 0 || x >= xDim) {
						break;
					}

					if (pieces [x, newY].IsColored () && pieces [x, newY].ColorComponent.Color == color) {
						horizontalPieces.Add (pieces [x, newY]);
					} else {
						break;
					}
				}
			}

			//add matches that were in a stright line to the true matches list
			if (horizontalPieces.Count >= 3) {
				for (int i = 0; i < horizontalPieces.Count; i++) {
					matchingPieces.Add (horizontalPieces [i]);
				}
			}

			// Traverse vertically if we found a match (for L and T shapes)
			if (horizontalPieces.Count >= 3) {
				for (int i = 0; i < horizontalPieces.Count; i++) {
					for (int dir = 0; dir <= 1; dir++) {
						for (int yOffset = 1; yOffset < yDim; yOffset++) {
							int y;

							if (dir == 0) { // Up
								y = newY - yOffset;
							} else { // Down
								y = newY + yOffset;
							}

							if (y < 0 || y >= yDim) {
								break;
							}

							if (pieces [horizontalPieces [i].X, y].IsColored () && pieces [horizontalPieces [i].X, y].ColorComponent.Color == color) {
								verticalPieces.Add (pieces [horizontalPieces [i].X, y]);
							} else {
								break; //if we break it is because we only found 2 matches and not 3 or more
							}
						}
					}

					//if there are less than 3 pieces in the array
					//then there are no matches
					//clear the array so that we can search from a different direction
					if (verticalPieces.Count < 2) {
						verticalPieces.Clear ();
					} else {
						for (int j = 0; j < verticalPieces.Count; j++) {
							matchingPieces.Add (verticalPieces [j]);
						}

						break;
					}
				}
			}

			if (matchingPieces.Count >= 3) {
				return matchingPieces;
			}

			// Didn't find anything going horizontally first,
			// so now check vertically
			horizontalPieces.Clear();
			verticalPieces.Clear();
			verticalPieces.Add(piece);

			for (int dir = 0; dir <= 1; dir++) {
				for (int yOffset = 1; yOffset < yDim; yOffset++) {
					int y;

					if (dir == 0) { // Up
						y = newY - yOffset;
					} else { // Down
						y = newY + yOffset;
					}

					//if we go out of bound then break
					if (y < 0 || y >= yDim) {
						break;
					}

					//add the piece the list of potential vertical matches
					if (pieces [newX, y].IsColored () && pieces [newX, y].ColorComponent.Color == color) {
						verticalPieces.Add (pieces [newX, y]);
					} else {
						break; //if we break it is because we only found 2 matches and not 3 or more
					}
				}
			}

			if (verticalPieces.Count >= 3) {
				for (int i = 0; i < verticalPieces.Count; i++) {
					matchingPieces.Add (verticalPieces [i]);
				}
			}

			// Traverse horizontally if we found a match (for L and T shapes)
			if (verticalPieces.Count >= 3) {
				for (int i = 0; i < verticalPieces.Count; i++) {
					for (int dir = 0; dir <= 1; dir++) {
						for (int xOffset = 1; xOffset < xDim; xOffset++) {
							int x;

							if (dir == 0) { // Left
								x = newX - xOffset;
							} else { // Right
								x = newX + xOffset;
							}

							if (x < 0 || x >= xDim) {
								break;
							}

							if (pieces [x, verticalPieces[i].Y].IsColored () && pieces [x, verticalPieces[i].Y].ColorComponent.Color == color) {
								horizontalPieces.Add (pieces [x, verticalPieces[i].Y]);
							} else {
								break;
							}
						}
					}

					if (horizontalPieces.Count < 2) {
						horizontalPieces.Clear ();
					} else {
						for (int j = 0; j < horizontalPieces.Count; j++) {
							matchingPieces.Add (horizontalPieces [j]);
						}

						break;
					}
				}
			}

			if (matchingPieces.Count >= 3) {
				return matchingPieces;
			}
		}

		return null;
	}

	//continue clearing the gid if more matches are made after clearing a piece
	public bool ClearAllValidMatches()
	{
		bool needsRefill = false;

		//go through all the pieces on the grid
		for (int y = level; y < yDim; y++) {
			for (int x = 0; x < xDim; x++) {
				//if the piece is clearable (i.e is not a block piece or empty piece, check if it has a match
				if (pieces [x, y].IsClearable ()) {
					List<GamePiece> match = GetMatch (pieces [x, y], x, y);

					if (match != null) {
						for (int i = 0; i < match.Count; i++) {
							if (ClearPiece (match [i].X, match [i].Y)) {
								needsRefill = true;
							}
						}
					}
				}
			}
		}

		return needsRefill;
	}

    //void incremenetTilesRemoved()
    //{
    //        serverTilesRemoved += 1;
    //        scoreText.addScore();
    //        print("This is server.");
    //        print("clientTilesRemoved=" + clientTilesRemoved);
    //        print("serverTilesRemoved=" + serverTilesRemoved);
    //}

    //clears a piece and spawns an empty piece in its place
    public bool ClearPiece(int x, int y)
	{
		//check if the piece is clearable and hasnt been alreay marked for clearing
		if (pieces [x, y].IsClearable () && !pieces [x, y].ClearableComponent.IsBeingCleared) {
			pieces [x, y].ClearableComponent.Clear ();

            Instantiate(destoryParticle, pieces[x, y].transform.position, pieces[x, y].transform.rotation);
         
			AudioClip temp = destroySound;
			switch (pieces [x, y].GetComponent<ColorPiece> ().Color) {
			case ColorPiece.ColorType.YELLOW:
				temp = bird;
				break;
			case ColorPiece.ColorType.PURPLE:
				temp = bear;
				break;
			case ColorPiece.ColorType.RED:
				temp = duck;
				break;
			case ColorPiece.ColorType.BLUE:
				temp = dolphin;
				break;
			case ColorPiece.ColorType.GREEN:
				temp = hawk;
				break;
			case ColorPiece.ColorType.PINK:
				temp = crab;
				break;
			default:
				temp = destroySound;
				break;
			}
            //scoreText.addScore();
            GetComponent<AudioSource>().clip = temp;
            GetComponent<AudioSource>().Play();
            SpawnNewPiece (x, y, PieceType.EMPTY);

			ClearObstacles (x, y);
            if (everythingSet)
            {
                score += 10;
                scoreText.addScore();
                if (!isServer)
                {
                    clientVar++;
                    print("client: " +clientVar);
                    if(clientVar == 9)
                    {
                        clientVar = 0;
						CmdTestCommand();
                        this.deleteRow(12); 
                    }
                }

                if(isServer)
                {
                    serverVar++;
                    print(serverVar);
                    if(serverVar == 9)
                    {
                        serverVar = 0;
						RpcTestCommand();
                        this.deleteRow(12);
                    }
                }
                //if (isServer)
                //    incremenetTilesRemoved();

                //if (isClient)
                //    CmdIncremementTiles();
            }
            
            return true;
		}

		return false;
	}

    //takes in x and y of a piece thaat was just cleared
    //then will search for obstacles  around the piece
    public void ClearObstacles(int x, int y) {
        //first look at pieces to the left and right of the cleared piece
        for (int adjacentX = x - 1; adjacentX <= x + 1; adjacentX++) {
            //make sure we dont check the cleared piece
            //also make sure we are still in bounds
            if (adjacentX != x && adjacentX >= 0 && adjacentX < xDim) {
                //if the piece is a bubble and its clearable, remove the bubble and fill it with empty piece
                if (pieces[adjacentX, y].Type == PieceType.BUBBLE && pieces[adjacentX, y].IsClearable()) {
                    pieces[adjacentX, y].ClearableComponent.Clear();
                    SpawnNewPiece(adjacentX, y, PieceType.EMPTY); }
            }
        }

        for (int adjacentY = y - 1; adjacentY <= x + 1; adjacentY++) {
            //make sure we dont check the cleared piece
            //also make sure we are still in bounds
            if (adjacentY != x && adjacentY >= 0 && adjacentY < yDim) {
                //if the piece is a bubble and its clearable, remove the bubble and fill it with empty piece
                if (pieces[x, adjacentY].Type == PieceType.BUBBLE && pieces[x, adjacentY].IsClearable()) {
                    pieces[x, adjacentY].ClearableComponent.Clear();
                    SpawnNewPiece(x, adjacentY, PieceType.EMPTY); }
            }
        }
    }

    public bool gridFull()
    {
        for (int i = 0; i < xDim; i++)
        {
            GamePiece piece = pieces[i, 0]; //get the current gamepiece
            if (piece.Type != PieceType.EMPTY)
            {
                return true;
            }
        }
        return false;
    }

    

    //function used delete row
    public void deleteRow(int row)
    {
        GamePiece piece, pieceAbove;
        int x, y;

        for (x = 0; x < xDim; x++)       //deletes a whole row of elements
        {
            piece = pieces[x, row];
            Destroy(piece.gameObject);
            SpawnNewPiece(x, row, PieceType.EMPTY);
        }

        //look for pieces that can move down starting from the deleted row to the "level" row
        for (y = row; y >= level; y--)
        {
            for (x = 0; x < xDim; x++)
            {
                piece = pieces[x, y]; //get the current gamepiece
                pieceAbove = pieces[x, y - 1];//get the piece above it

                //if the current game piece is an empty piece and the piece above can be moved
                if (piece.Type == PieceType.EMPTY && pieceAbove.IsMovable())
                {
                    //if the piece above isn't empty and piece above can be moved
                    //move the above piece down and create and empty piece in its place
                    if (piece.Type == PieceType.EMPTY && pieceAbove.IsMovable())
                    {
                        Destroy(piece.gameObject);
                        pieceAbove.MovableComponent.Move(x, y, fillTime);
                        pieces[x, y] = pieceAbove;
                        SpawnNewPiece(x, y - 1, PieceType.EMPTY);
                    }
                }
            }
        }

        level++; //moves the level down (they're rows so incrementing it means going down)
    }

    //function used insert row below
    public void insertRow()
    {
        bool movedPiece = false;
        GamePiece piece, pieceAbove;
        int x, y;

        if (gridFull())
        { //end game 
        }

            //look for pieces that can move up starting from the "level" row
            for (y = level; y < yDim; y++)
            {
                for (x = 0; x < xDim; x++)
                {
                    piece = pieces[x, y]; //get the current gamepiece

                    //if the current game piece isnt an empty piece or a blocking piece
                    if (piece.IsMovable())
                    {
                        pieceAbove = pieces[x, y - 1];//get the piece above it

                        //if the piece above it is empty
                        //move the current piece up
                        //and create and empty piece in its place
                        if (pieceAbove.Type == PieceType.EMPTY)
                        {
                            Destroy(pieceAbove.gameObject);
                            piece.MovableComponent.Move(x, y - 1, fillTime);
                            pieces[x, y - 1] = piece;
                            SpawnNewPiece(x, y, PieceType.EMPTY);
                            movedPiece = true;
                        }
                    }
                }
            }

            //special case for when we get to the bottom row, we fill it up
            for (x = 0; x < xDim; x++)
            {
                piece = pieces[x, yDim - 1]; //get current game piece on bottom row

                //if the cell is empty
                //fill the cel with a new piece of a random color
                if (piece.Type == PieceType.EMPTY)
                {
                    Destroy(piece.gameObject);
                    GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.NORMAL], GetWorldPosition(x, yDim - 1), Quaternion.identity);
                    newPiece.transform.parent = transform;

                    pieces[x, yDim - 1] = newPiece.GetComponent<GamePiece>();
                    pieces[x, yDim - 1].Init(x, yDim - 1, this, PieceType.NORMAL);
                    pieces[x, yDim - 1].MovableComponent.Move(x, yDim - 1, fillTime);
                    pieces[x, yDim - 1].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x, yDim - 1].ColorComponent.NumColors));
                    movedPiece = true;
                }
            }
        level--;
        }

    //function used insert row below
    [Command]
    public void CmdinsertRow()
    {
        bool movedPiece = false;
        GamePiece piece, pieceAbove;
        int x, y;

        if (gridFull())
        { //end game 
        }

        //look for pieces that can move up starting from the "level" row
        for (y = level; y < yDim; y++)
        {
            for (x = 0; x < xDim; x++)
            {
                piece = pieces[x, y]; //get the current gamepiece

                //if the current game piece isnt an empty piece or a blocking piece
                if (piece.IsMovable())
                {
                    pieceAbove = pieces[x, y - 1];//get the piece above it

                    //if the piece above it is empty
                    //move the current piece up
                    //and create and empty piece in its place
                    if (pieceAbove.Type == PieceType.EMPTY)
                    {
                        Destroy(pieceAbove.gameObject);
                        piece.MovableComponent.Move(x, y - 1, fillTime);
                        pieces[x, y - 1] = piece;
                        SpawnNewPiece(x, y, PieceType.EMPTY);
                        movedPiece = true;
                    }
                }
            }
        }

        //special case for when we get to the bottom row, we fill it up
        for (x = 0; x < xDim; x++)
        {
            piece = pieces[x, yDim - 1]; //get current game piece on bottom row

            //if the cell is empty
            //fill the cel with a new piece of a random color
            if (piece.Type == PieceType.EMPTY)
            {
                Destroy(piece.gameObject);
                GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.NORMAL], GetWorldPosition(x, yDim - 1), Quaternion.identity);
                newPiece.transform.parent = transform;

                pieces[x, yDim - 1] = newPiece.GetComponent<GamePiece>();
                pieces[x, yDim - 1].Init(x, yDim - 1, this, PieceType.NORMAL);
                pieces[x, yDim - 1].MovableComponent.Move(x, yDim - 1, fillTime);
                pieces[x, yDim - 1].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x, yDim - 1].ColorComponent.NumColors));
                movedPiece = true;
            }
        }
        level--;
    }

    void OnDisable()
    {
        if (score > highscore)
        {
            PlayerPrefs.SetInt(highScoreKey, score);
            PlayerPrefs.Save();
        }
    }

}
