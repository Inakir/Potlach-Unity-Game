using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Score1 : MonoBehaviour {

    public Text text1;
    public int score;
    string highScoreKey = "HighScore";


    // Use this for initialization
    void Start ()
    {
        score = PlayerPrefs.GetInt(highScoreKey, 0);
	}

    void Awake()
    {
        text1.text = "Name: " + score;
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
