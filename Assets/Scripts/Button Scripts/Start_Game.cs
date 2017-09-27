using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Start_Game : MonoBehaviour {

    public AudioClip buttonClick;

    public bool gameStart = false;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void TaskOnClick()
    {
        gameStart = true;
        GetComponent<AudioSource>().clip = buttonClick;
        GetComponent<AudioSource>().Play();
        SceneManager.LoadScene("Scenes/IndianScene");
    }



}
