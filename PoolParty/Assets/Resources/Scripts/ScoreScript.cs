using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreScript : MonoBehaviour {

	public int myCharacter;
	public int wins;
	private Text myText;
	private GameManager gMan;
	private SpriteRenderer headRenderer;

	// Move attributes
	private int moveDuration = 2;
	private float moveTimer = 0.0f;
	public bool moving = false;

	// Use this for initialization
	void Start () 
	{
		myText = gameObject.GetComponent<Text> ();
		gMan = FindObjectOfType<GameManager> ();
		headRenderer = gameObject.GetComponentInChildren<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void UpdateScore()
	{
		foreach(GameObject player in gMan.Players)
		{
			KnobMovement temp = player.GetComponent<KnobMovement> ();
			if (temp.characterNumber == myCharacter) 
			{
				PersonScript tempPerson = player.GetComponentInChildren<PersonScript> ();
				wins = tempPerson.numWins;
				myText.text = tempPerson.numWins.ToString();
			}
		}
	}

	public void FadeOut()
	{
		myText.color = new Vector4(myText.color.r, myText.color.g, myText.color.b, 0.2f);
		headRenderer.color = new Vector4(headRenderer.color.r, headRenderer.color.g, headRenderer.color.b, 0.2f);
	}

	public void FadeIn()
	{
		myText.color = new Vector4(myText.color.r, myText.color.g, myText.color.b, 1.0f);
		headRenderer.color = new Vector4(headRenderer.color.r, headRenderer.color.g, headRenderer.color.b, 1.0f);
	}

	public void Move(float Xposition)
	{
		StartCoroutine (MovePositions (Xposition));
	}

	private IEnumerator MovePositions(float endPos)
	{
		float startPos = transform.localPosition.x;
		moving = true;

		while (moveTimer <= moveDuration) 
		{
			moveTimer += Time.deltaTime;

			transform.localPosition = new Vector3 (Mathf.Lerp (startPos, endPos, moveTimer / moveDuration), transform.localPosition.y, transform.localPosition.z);

			yield return null;
		}

		moving = false;
		moveTimer = 0.0f;
	}
}
