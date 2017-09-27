using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MovablePiece : NetworkBehaviour {

	//used to get info about the piece being moved
	private GamePiece piece;
	private IEnumerator moveCoroutine;
    private IEnumerator bounceCoroutine;

    void Awake() {
		piece = GetComponent<GamePiece> ();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
       /* if (!isLocalPlayer)
        {
            return;
        }***/

	}

	//allows the piece to be moved over several animation frames
	public void Move(int newX, int newY, float time)
	{
		if (moveCoroutine != null) {
			StopCoroutine (moveCoroutine);
		}

		moveCoroutine = MoveCoroutine (newX, newY, time);
		StartCoroutine (moveCoroutine);
	}

	//slowly move the piece little by little
	public IEnumerator MoveCoroutine(int newX, int newY, float time)
	{
		piece.X = newX;
		piece.Y = newY;

		Vector3 startPos = transform.position;
		Vector3 endPos = piece.GridRef.GetWorldPosition (newX, newY);

		for (float t = 0; t <= 1 * time; t += Time.deltaTime) {
			piece.transform.position = Vector3.Lerp (startPos, endPos, t / time);
			yield return 0; //wait for 1 frame
		}

		piece.transform.position = piece.GridRef.GetWorldPosition (newX, newY);
	}

    //allows the piece to be bounced over several animation frames
    public void Bounce(bool fwd, int oldX, int oldY, int newX, int newY, float time)
    {
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
        }

        bounceCoroutine = BounceCoroutine(fwd, oldX, oldY, newX, newY, time);
        StartCoroutine(bounceCoroutine);
    }

    //slowly bounce the piece little by little
    public IEnumerator BounceCoroutine(bool fwd, int oldX, int oldY, int newX, int newY, float time)
    {
        piece.X = newX;
        piece.Y = newY;
        Vector3 startPos = transform.position;
        Vector3 endPos = piece.GridRef.GetWorldPosition(newX, newY);

        for (float t = 0; t <= 1 * time; t += Time.deltaTime)
        {
            piece.transform.position = Vector3.Lerp(startPos, endPos, t / time);
            yield return 0; //wait for 1 frame
        }
        piece.transform.position = piece.GridRef.GetWorldPosition(newX, newY);


        piece.X = oldX;
        piece.Y = oldY;
        startPos = transform.position;
        endPos = piece.GridRef.GetWorldPosition(oldX, oldY);

        for (float t = 0; t <= 1 * time; t += Time.deltaTime)
        {
            piece.transform.position = Vector3.Lerp(startPos, endPos, t / time);
            yield return 0; //wait for 1 frame
        }
        piece.transform.position = piece.GridRef.GetWorldPosition(oldX, oldY);
    }
}
