using UnityEngine;
using UnityEngine.SceneManagement;

public class Dont_Destory : MonoBehaviour {

    static bool AudioBegin = false;
    //public new AudioSource audio;

    void Start()
    {
        
    }

    void Awake()
    {
        if (!AudioBegin)
        {
            GetComponent<AudioSource>().Play();
            DontDestroyOnLoad(gameObject);
            AudioBegin = true;
        }
    }
    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 4)
        {
            GetComponent<AudioSource>().Stop();
            AudioBegin = false;
        }
    }
}
