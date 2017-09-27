using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeText : MonoBehaviour {

    public Text timeText;
    private int time = 120;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
       // timeText.text = "time: " + time;
	}

    public void subtractTime()
    {
        time--;
    }
}
