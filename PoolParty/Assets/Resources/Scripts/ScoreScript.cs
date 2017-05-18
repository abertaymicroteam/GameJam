using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreScript : MonoBehaviour {

	public int myCharacter;
	public int wins;
	private Text myText;
	private GameManager gMan;
	private SpriteRenderer[] renderers;
	private AbilityBarScript abilityScript;

	// Move attributes
	private int moveDuration = 2;
	private float moveTimer = 0.0f;
	public bool moving = false;

	// Use this for initialization
	void Start () 
	{
		myText = gameObject.GetComponent<Text> ();
		gMan = FindObjectOfType<GameManager> ();
		renderers = gameObject.GetComponentsInChildren<SpriteRenderer> ();
		abilityScript = gameObject.GetComponentInChildren<AbilityBarScript> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void UpdateScore()
	{
		foreach(GameObject player in gMan.Players)
		{
			PlayerScript temp = player.GetComponent<PlayerScript> ();
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
		foreach(SpriteRenderer renderer in renderers)
		{			
			renderer.color = new Vector4 (renderer.color.r, renderer.color.g, renderer.color.b, 0.2f);
		}
		abilityScript.maxAlpha = 0.2f;
	}

	public void FadeIn()
	{
		myText.color = new Vector4(myText.color.r, myText.color.g, myText.color.b, 1.0f);
		foreach(SpriteRenderer renderer in renderers)
		{			
			renderer.color = new Vector4 (renderer.color.r, renderer.color.g, renderer.color.b, 1.0f);
		}
		abilityScript.maxAlpha = 1.0f;
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
