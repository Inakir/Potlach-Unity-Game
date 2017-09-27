using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColorPiece : MonoBehaviour {

	//piece colorType
	public enum ColorType
	{
		YELLOW,
		PURPLE,
		RED,
		BLUE,
		GREEN,
		PINK,
		ANY,
		COUNT
	};

	//used to set the color of the piece and sprite that goes with the piece in the Inspector
	[System.Serializable]//flag that is set to enables us to see this struct in the Inspector
	public struct ColorSprite
	{
		public ColorType color;
		public Sprite sprite;
	};

	//array to hold all the sprites with their respective color in the Inspector
	public ColorSprite[] colorSprites;

	//the actual value of the color variable of the piece
	private ColorType color;

	//gets and sets the color of the piece
	public ColorType Color
	{
		get { return color; }
		set { SetColor (value); }
	}

	//gets the nuber of possible colors
	//returns the length of the colorSprites array that we set in the Inspector
	public int NumColors
	{
		get { return colorSprites.Length; }
	}

	//used to store the Render Component of our sprite
	// so that we can change the sprite dynamically later
	//in the setColor function
	private SpriteRenderer sprite;

	//dictionary which holds the color/sprite pairs
	private Dictionary<ColorType, Sprite> colorSpriteDict;

	void Awake()
	{
		//find child game object called piece
		sprite = transform.Find ("piece").GetComponent<SpriteRenderer> ();

		//create the dictionary that will hold our color/sprite pairs
		colorSpriteDict = new Dictionary<ColorType, Sprite> ();

		//check to make sure that their arent things of the same color
		for (int i = 0; i < colorSprites.Length; i++) {
			if (!colorSpriteDict.ContainsKey (colorSprites [i].color)) {
				colorSpriteDict.Add (colorSprites [i].color, colorSprites [i].sprite);
			}
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//sets the color of the piece
	//and changes the sprite accordingly
	public void SetColor(ColorType newColor)
	{
		color = newColor;

		if (colorSpriteDict.ContainsKey (newColor)) {
			sprite.sprite = colorSpriteDict [newColor];
		}
	}
}
