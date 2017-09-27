using UnityEngine;
using System.Collections;

public class Dont_Destory_Scores : MonoBehaviour {

    public int score = 0;
    public int finalScore = 0;

	// Use this for initialization
	void Start () {
	
	}

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
	
	// Update is called once per frame
	void Update ()
    {
        score = GameObject.FindGameObjectWithTag("Player").GetComponent<ScoreText>().score;
        finalScore = score;
        print(finalScore);
	}
}
