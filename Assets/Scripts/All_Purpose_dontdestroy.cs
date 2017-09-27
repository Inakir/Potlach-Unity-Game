using UnityEngine;
using System.Collections;

public class All_Purpose_dontdestroy : MonoBehaviour
{
    
	// Use this for initialization
	void Start () {
	
	}
	
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

	// Update is called once per frame
	void Update () {
	
	}
}
