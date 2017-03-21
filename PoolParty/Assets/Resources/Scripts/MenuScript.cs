using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuScript : MonoBehaviour 
{

	// Object References
	private SpriteRenderer menuRenderer;
	private AudioManager audioMan;
	private GameManager gMan;
	private GameObject canvas;

	// Graphics
	public List<SpriteRenderer> countdownNums = new List<SpriteRenderer> ();
	float countdownTimer = 3.0f;
	int currCountdown = 0;
	private int prevCountdown = 0;
	public SpriteRenderer Title;
	public SpriteRenderer winner;
	public SpriteRenderer restart;
	public SpriteRenderer helper;
	public SpriteRenderer tapToRestart;
	public SpriteRenderer logo;
	public SpriteRenderer tapToPlay;
	private Text winnerName;
	float restartTimer = 1.0f;
	public List<GameObject> CharacterGraphics;
	public List<GameObject> CharacterScores;
	private List<GameObject> ActiveGraphics = new List<GameObject> ();
	public List<GameObject> ActiveScores = new List<GameObject> ();
	private List<Vector2> GraphicPositions = new List<Vector2> {new Vector2(-5.44f, 1.379f), new Vector2(-1.77f, 1.369f), new Vector2(1.84f, 1.37f), new Vector2(5.45f, 1.37f),
																new Vector2(-5.42f, -2.83f), new Vector2(-1.77f, -2.84f), new Vector2(1.84f, -2.83f), new Vector2(5.48f, -2.83f)};
	private List<Vector2> ScorePositions = new List<Vector2> {new Vector2(-41.5f, -23.0f), new Vector2(-29.57f, -23.0f), new Vector2(-17.14f, -23.0f), new Vector2(-4.71f, -23.0f),
															  new Vector2(7.72f, -23.0f), new Vector2(20.15f, -23.0f), new Vector2(32.58f, -23.0f), new Vector2(45.0f, -23.0f)};

	// Display flags
	public bool displayCountdown = false;
	public bool displayRestart = false;
	private bool scoresMoved = false;

	// Use this for initialization
	void Start () 
	{
		Title.enabled = true;
		menuRenderer = gameObject.GetComponent<SpriteRenderer> ();

		winnerName = GameObject.Find ("WinnerName").GetComponent<Text>();

		audioMan = GameObject.FindGameObjectWithTag ("Audio").GetComponent<AudioManager> ();
		gMan = GameObject.FindObjectOfType<GameManager> ();
		canvas = GameObject.FindGameObjectWithTag ("Scores");
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (displayCountdown) 
		{ // Display countdown from 3 to 1
			Countdown ();
			helper.enabled = true;
		}
		else 
		{
			helper.enabled = false;
		}
		if (displayRestart) 
		{ // Display countdown from 3 to 1
			ShowRestart ();
		}
		
	}

	public void HideTitle(){
		Title.enabled = false;
	}

	public void ShowMenu()
	{
		menuRenderer.enabled = true;
		logo.enabled = true;
		tapToPlay.enabled = true;
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
		logo.enabled = false;
		tapToPlay.enabled = false;
		foreach (GameObject graphic in ActiveGraphics) 
		{
			SpriteRenderer rend = graphic.GetComponent<SpriteRenderer> ();
			rend.enabled = false;
		}
		SpriteRenderer pNumsRenderer = GameObject.Find("Player Numbers").GetComponent<SpriteRenderer> ();
		pNumsRenderer.enabled = false;
	}

	public void ShowConnectGraphic(int character, int playerID)
	{ 
		// Displays graphic of connected player's character

		GameObject newGraphic = Instantiate (CharacterGraphics [character], GraphicPositions [playerID], Quaternion.identity) as GameObject;
		ActiveGraphics.Add (newGraphic);
	}

	public void UpdateConnectGraphics(int playerToRemove)
	{
		// Destroy and remove disconnected player's graphic
		Destroy(ActiveGraphics[playerToRemove]);
		ActiveGraphics.Remove (ActiveGraphics [playerToRemove]);
	}

	public void ShowScores()
	{
		foreach (GameObject player in gMan.Players) 
		{
			KnobMovement temp = player.GetComponent<KnobMovement> ();
			int character = temp.characterNumber;
			int playerID = temp.playerID;
			GameObject newScore = Instantiate (CharacterScores [character], new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
			newScore.transform.SetParent (canvas.transform);
			newScore.transform.localPosition = ScorePositions [playerID];
			if (character == 0) {
				// Move phil right a bit..
				newScore.transform.localPosition += new Vector3(1.0f, 0.0f, 0.0f);
			}
			newScore.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			ActiveScores.Add (newScore);
		}
	}

	private void Countdown()
	{
		if (countdownTimer > 2) 
		{
			currCountdown = 0;
			if (currCountdown != prevCountdown)
				audioMan.PlayDrop ();
		} 
		else if ((countdownTimer <= 2) && (countdownTimer > 1)) 
		{
			currCountdown = 1;
			if (currCountdown != prevCountdown)
				audioMan.PlayDrop ();
		} 
		else if ((countdownTimer <= 1) && (countdownTimer > 0)) 
		{
			currCountdown = 2;
			if (currCountdown != prevCountdown)
				audioMan.PlayDrop ();
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
			prevCountdown = currCountdown;
		} 
		else 
		{
			displayCountdown = false;
			countdownTimer = 3.0f;
			// Play bell sound
			audioMan.PlayBell();
		}
	}

	public void ShowWinner(string name)
	{
		winnerName.text = name;
		winnerName.enabled = true;
		winner.enabled = true;

		if(!scoresMoved)
		{
			// Update all scores
			foreach (GameObject score in ActiveScores) 
			{
				ScoreScript script = score.GetComponent<ScoreScript> ();
				script.UpdateScore ();
			}

			// Sort scores in descending order
			ActiveScores.Sort(delegate(GameObject x, GameObject y) 
			{
					return (x.GetComponent<ScoreScript>().wins).CompareTo(y.GetComponent<ScoreScript>().wins);
			});
			ActiveScores.Reverse ();
			
			
			// Move each score based on its position in the sorted list
			GameObject score1;
			ScoreScript script1;
			for (int i = 0; i < ActiveScores.Count; i++)
			{
				score1 = ActiveScores [i];
				script1 = score1.GetComponent<ScoreScript> ();
				script1.Move (ScorePositions [i].x);
			}

			scoresMoved = true;
		}
	}

	public void ShowRestart()
	{
		restart.enabled = true;
		tapToRestart.enabled = false;

		if (restartTimer < 0) 
		{
			restart.enabled = false;
			displayRestart = false;
			restartTimer = 1.0f;
		}

		restartTimer -= Time.deltaTime;
	}

	public void ShowTapToRestart()
	{
		tapToRestart.enabled = true;
	}

	public void HideWinner()
	{
		winnerName.enabled = false;
		winner.enabled = false;
		scoresMoved = false;
	}

	public void HideRestart()
	{
		restart.enabled = false;
	}
}
