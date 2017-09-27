using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class Quit_to_Menu : MonoBehaviour {

    public AudioClip buttonClick;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void onQuitClick()
    {
        print("Quit clicked");
        GetComponent<AudioSource>().clip = buttonClick;
        GetComponent<AudioSource>().Play();
        SceneManager.LoadScene("Scenes/MainMenu");
    }
}
