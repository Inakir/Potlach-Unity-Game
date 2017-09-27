using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class Score_Board : MonoBehaviour {

    public AudioClip buttonClick;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void onScoreBoardClick()
    {
        GetComponent<AudioSource>().clip = buttonClick;
        GetComponent<AudioSource>().Play();
        SceneManager.LoadScene("Scenes/ScoreBoardScene");
    }
}
