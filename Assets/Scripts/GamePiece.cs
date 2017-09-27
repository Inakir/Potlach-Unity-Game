using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GamePiece : NetworkBehaviour  
{
	//coordinates of the game piece on the board
	private int x;
	private int y;

	//gets X coordinate of piece
	//if the piece if movable, allow other functions to edit its x coordinate
	public int X
	{
		get { return x; }
		set {
			if (IsMovable ()) {
				x = value;
			}
		}
	}

	//gets Y coordinate of piece
	//if the piece if movable, allow other functions to edit its y coordinate
	public int Y
	{
		get { return y; }
		set {
			if (IsMovable ()) {
				y = value;
			}
		}
	}

	//gets type of piece
	private Grid.PieceType type;

	public Grid.PieceType Type
	{
		get { return type; }
	}

	//used in case we need to get info about the grid or the pieces in it
	private Grid grid;

	public Grid GridRef
	{
		get { return grid; }
	}

	//used to check if a piece is movable
	private MovablePiece movableComponent;

	public MovablePiece MovableComponent
	{
		get { return movableComponent; }
	}

	//used to check if a piece is colored
	private ColorPiece colorComponent;

	public ColorPiece ColorComponent
	{
		get { return colorComponent; }
	}

	//used to check if piece is clearable
	private ClearablePiece clearableComponent;

	public ClearablePiece ClearableComponent {
		get { return clearableComponent; }
	}

	void Awake()
	{
		//get references to components
		//if the piece doesnt have one, it sets it to NULL (i.e piece is not movable, colored, or clearable)
		movableComponent = GetComponent<MovablePiece> ();
		colorComponent = GetComponent<ColorPiece> ();
		clearableComponent = GetComponent<ClearablePiece> ();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//use this to initialize the game piece with its info
	//this function is called after we instantiate the game piece's GameObject
	public void Init(int _x, int _y, Grid _grid, Grid.PieceType _type)
	{
		x = _x;
		y = _y;
		grid = _grid;
		type = _type;
	}

	void OnMouseEnter()
	{
		grid.EnterPiece (this);
	}

	void OnMouseDown()
	{
		grid.PressPiece (this);
	}

	void OnMouseUp()
	{
		grid.ReleasePiece ();
	}

	public bool IsMovable()
	{
		//if the componenet exists, it returns true (i.e the piece is movable)
		//else it returns false (i.e the piece is not movable)
		return movableComponent != null;
	}

	public bool IsColored()
	{
		//if the componenet exists, it returns true (i.e the piece is colored)
		//else it returns false (i.e the piece is not colored)
		return colorComponent != null;
	}

	public bool IsClearable()
	{
		//if the componenet exists, it returns true (i.e the piece is clearable)
		//else it returns false (i.e the piece is not clearable)
		return clearableComponent != null;
	}
}
