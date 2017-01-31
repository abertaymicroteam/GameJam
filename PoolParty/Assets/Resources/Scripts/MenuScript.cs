using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuScript : MonoBehaviour {

	// Object References
	private SpriteRenderer menuRenderer;

	// Graphics
	public List<SpriteRenderer> countdownNums = new List<SpriteRenderer> ();
	float countdownTimer = 3.0f;
	int currCountdown = 0;
	public SpriteRenderer winner;
	public SpriteRenderer restart;
	float restartTimer = 1.0f;
	public List<GameObject> CharacterGraphics;
	private List<GameObject> ActiveGraphics = new List<GameObject> ();
	private List<Vector2> GraphicPositions = new List<Vector2> {new Vector2(-6.57f, 2.6f), new Vector2(-2.19f, 2.6f), new Vector2(2.16f, 2.6f), new Vector2(6.49f, 2.6f),
																new Vector2(-6.57f, -2.56f), new Vector2(-2.19f, -2.56f), new Vector2(2.16f, -2.56f), new Vector2(6.49f, -2.56f)};

	// Display flags
	public bool displayCountdown = false;
	public bool displayRestart = false;

	// Use this for initialization
	void Start () 
	{
		menuRenderer = gameObject.GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (displayCountdown) 
		{ // Display countdown from 3 to 1
			Countdown ();
		}
		if (displayRestart) 
		{ // Display countdown from 3 to 1
			ShowRestart ();
		}
	}

	public void ShowMenu()
	{
		menuRenderer.enabled = true;
		foreach (GameObject graphic in ActiveGraphics) 
		{
			SpriteRenderer rend = graphic.GetComponent<SpriteRenderer> ();
			rend.enabled = true;
		}
		SpriteRenderer pNumsRenderer = GameObject.Find("Player Numbers").GetComponent<SpriteRenderer> ();
		pNumsRenderer.enabled = true;
	}

	public void HideMenu()
	{ // Hide character menu assets
		
		menuRenderer.enabled = false;
		foreach (GameObject graphic in ActiveGraphics) 
		{
			SpriteRenderer rend = graphic.GetComponent<SpriteRenderer> ();
			rend.enabled = false;
		}
		SpriteRenderer pNumsRenderer = GameObject.Find("Player Numbers").GetComponent<SpriteRenderer> ();
		pNumsRenderer.enabled = false;
	}

	public void ShowConnectGraphic(int character, int playerID)
	{ // Displays graphic of connected player's character

		GameObject newGraphic = Instantiate (CharacterGraphics [character], GraphicPositions [playerID], Quaternion.identity) as GameObject;
		ActiveGraphics.Add (newGraphic);
	}

	private void Countdown()
	{
		if (countdownTimer > 2) 
		{
			currCountdown = 0;
		} 
		else if ((countdownTimer <= 2) && (countdownTimer > 1)) 
		{
			currCountdown = 1;
		} 
		else if ((countdownTimer <= 1) && (countdownTimer > 0)) 
		{
			currCountdown = 2;
		} 
		else 
		{
			currCountdown = 3;
		}

		countdownTimer -= Time.deltaTime;

		// Disable all renderers then display correct one
		foreach (SpriteRenderer rend in countdownNums) 
		{
			rend.enabled = false;
		}
		if (currCountdown < 3) 
		{
			countdownNums [currCountdown].enabled = true;
		} 
		else 
		{
			displayCountdown = false;
			countdownTimer = 3.0f;
		}
	}

	public void ShowWinner()
	{
		winner.enabled = true;
	}

	public void ShowRestart()
	{
		restart.enabled = true;

		if (restartTimer < 0) 
		{
			restart.enabled = false;
			displayRestart = false;
			restartTimer = 1.0f;
		}

		restartTimer -= Time.deltaTime;
	}

	public void HideWinner()
	{
		winner.enabled = false;
	}

	public void HideRestart()
	{
		restart.enabled = false;
	}
}
