using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class ScoreText : NetworkBehaviour {

    public Text scoreText;
    public int score = 0;
    public int highscore = 0;

	// Use this for initialization
	void Start ()
    {
        
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        //this.scoreText.text = "Score: " + score;
	}

    public void addScore()
    {
        score += 10;
        scoreText.text = "Score: " + score.ToString();

    }

    
}
