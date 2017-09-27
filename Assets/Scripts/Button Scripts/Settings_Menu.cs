using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Settings_Menu : MonoBehaviour {

    public AudioClip buttonClick;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void onSettingsClick()
    {
        GetComponent<AudioSource>().clip = buttonClick;
        GetComponent<AudioSource>().Play();
        SceneManager.LoadScene("Scenes/SettingsScene");
    }
}
