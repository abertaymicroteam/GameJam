﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class GameManager : MonoBehaviour 
{
	// Game State
	public enum STATE { MENU, GAME, RESTART, READY };
	public STATE GameState;

	// Scripts
	public MenuScript menu;
	private AudioManager audioMan;

	// Objects for Shrinking
	private GameObject border;
	private GameObject boundaries;
	private Camera cam;
	private GameObject scores;

	// Players
	public Vector3 SpawnLocation;
	public List<GameObject> Players;
	public GameObject[] Characters;
	public List<int> TakenCharacters;
	private int[] charNums = new int[8] {0, 1, 2, 3, 4, 5, 6, 7};
	private float angle = 0.0f;
	public float[] angles;
	public int[] ID;
	public int connectedPlayers = 0;
	private int prevConnectedPlayers = 0;
	private int destroyedPlayers = 0;

	// Misc
	private bool restartTap = false;
	private bool playWin = false;
	private bool playDrop = false;
	public bool newTap = false;
	private float timer = 3.0f;
	private bool winIncremented = false;
	public float rotatormes;
	#if !DISABLE_AIRCONSOLE 

	// Play area shrinking
	private float shrinkTimer = 15.0f;
	private float showdownTimer = 5.0f;
	private bool shrunk = false;
	private bool showdown = false;
	private bool borderRed = false;
	private bool borderFlash = false;

	//variables for update message
	public float[] prevAngle;
	private float currentAngle;
	private int msgI = 0;
	private float messagetimer = 0.1f; // Messages limited to 10 per second

	public float getAngle(int ID)
	{
		return angles [ID];
	}

	void Awake () 
	{
		AirConsole.instance.onMessage += OnMessage;
		AirConsole.instance.onConnect += OnConnect;
		AirConsole.instance.onDisconnect += OnDisconnect;
	}

	void Start()
	{
		// Set initial game state
		GameState = STATE.MENU;

		// Get Menu
		menu =  GameObject.FindGameObjectWithTag("Menu").GetComponent<MenuScript>();

		// Get Audio Manager
		audioMan = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

		// Get objects to shrink
		border = GameObject.FindGameObjectWithTag("Border");
		boundaries = GameObject.FindGameObjectWithTag ("Boundaries");
		cam = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera>();
		scores = GameObject.FindGameObjectWithTag ("Scores");

		// Randomise character order
		ShuffleArray<int>(charNums);
	}

	/// <summary>
	/// 
	/// 
	/// </summary>
	/// <param name="device_id">The device_id that connected</param>
	void OnConnect (int device_id) 
	{
		if (connectedPlayers < 8) 
		{
			if (GameState == STATE.MENU || GameState == STATE.READY)
			{
				// Set default spawn
				SpawnLocation.Set (0, 0, 0);

				// Assign character
				//int character = RandomCharacter();
				int character = charNums [connectedPlayers];
				TakenCharacters [connectedPlayers] = character;

				// Create player
				GameObject newPlayer = Instantiate (Characters [character], SpawnLocation, Quaternion.identity) as GameObject;
				newPlayer.GetComponent<KnobMovement> ().characterNumber = character;
				newPlayer.GetComponent<KnobMovement> ().SetID (connectedPlayers);
				ID [connectedPlayers] = device_id;
				Players.Add (newPlayer);

				// Add Menu Graphic
				menu.ShowConnectGraphic (character, connectedPlayers);

				// Increment connected players
				connectedPlayers++;

				// Send connection message to controller
				JObject connectionMessage = new JObject ();
				connectionMessage.Add ("state", (int)GameState);
				connectionMessage.Add ("angle", 0);
				AirConsole.instance.Message (device_id, connectionMessage);
				AirConsole.instance.SetActivePlayers (8);

				// Play connect sound
				audioMan.PlayDrop ();

				// Is the game ready to play? (2 player connected)
				if (AirConsole.instance.GetControllerDeviceIds ().Count > 0) {	
					menu.HideTitle ();
					ReadyToPlay ();
				} else {
					//uiText.text =  AirConsole.instance.GetControllerDeviceIds ().Count  + " PLAYERS CONNECTED";
				}
			} 
			else 
			{
				// Wait for new round to add new player
				StartCoroutine(SpawnPlayerInNewRound(device_id));
			}
		}
	}

	/// <summary>
	/// If the game is running and one of the active players leaves, we remove their character and continue playing.
	/// </summary>
	/// <param name="device_id">The device_id that has left.</param>
	void OnDisconnect (int device_id) 
	{
		int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber (device_id);
		if (active_player != -1) 
		{
			Debug.Log ("Player " + active_player + " disconnected. Removing!");

			// Update counters for current player states
			connectedPlayers --;
			if (Players [active_player].GetComponent<KnobMovement> ().destroyMe && destroyedPlayers > 0) 
			{
				destroyedPlayers--;
			}

			// Allow character to be retaken
			int toRemove = -1;
			foreach(int i in TakenCharacters)
			{
				if (i == Players[active_player].GetComponent<KnobMovement>().characterNumber)
				{
					toRemove = i;
				}
			}
			if (toRemove != -1)
				TakenCharacters.Remove (toRemove);

			// Remove player connect graphic and score from menu
			menu.RemoveConnectGraphic(active_player);
			menu.RemoveScore (Players [active_player].GetComponent<KnobMovement> ().characterNumber);

			// Destroy game object and remove from list
			Destroy(Players [active_player]);
			Players.Remove (Players [active_player]);
			AirConsole.instance.SetActivePlayers (8);
			//Players.RemoveAll (item => item == null);

			// If there are no more players connected, go back to menu and wait for connection.
			if (connectedPlayers == 0) 
			{
				menu.ShowMenu ();
				GameState = STATE.MENU;
				ShuffleArray<int> (charNums);
			}
		}
	}

	/// <summary>
	/// Player input
	/// </summary>
	/// <param name="from">From.</param>
	/// <param name="data">Data.</param>
	void OnMessage (int device_id, JToken data) 
	{
		if (GameState == STATE.RESTART) 
		{
			int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber (device_id);
			if (active_player != -1) 
			{
				Debug.Log ("Restart Tap");
				restartTap = true;
				GameState = STATE.GAME;
			}
		} 
		else if (GameState == STATE.READY) 
		{
			//StartGame ();

			// Change character
			int playerNumber = AirConsole.instance.ConvertDeviceIdToPlayerNumber(device_id);
			if (playerNumber != -1)
			{
				// Get current character information
				GameObject currentPlayer = Players [playerNumber];
				int currCharacter = currentPlayer.GetComponent<KnobMovement> ().characterNumber;
				int newCharacter = -1;
				if (currCharacter < 7)
				{
					newCharacter = currCharacter + 1;
				}
				else
				{
					newCharacter = 0;
				}

				// Create new player
				GameObject newPlayer = Instantiate (Characters [newCharacter], SpawnLocation, Quaternion.identity) as GameObject;
				newPlayer.GetComponent<KnobMovement> ().characterNumber = newCharacter;
				newPlayer.GetComponent<KnobMovement> ().SetID (playerNumber);
				//ID [playerNumber] = device_id;
				Players[playerNumber] = newPlayer;

				// Destroy old player object
				Destroy(currentPlayer);	

				// Update Menu Graphic
				menu.UpdateConnectGraphic(playerNumber, newCharacter);

				// Play sound
				audioMan.PlayDrop();
			}
		}
		else
		{			
			//if a player is active 
			int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber (device_id);
			if (active_player != -1) 
			{
				//get the angle information from the players tap
				angle = (float)data ["move"];

				//calculate were to apply force to the player
				if (angle > 0) 
				{
					angle = 180 - angle;
				} 
				else if (angle < 0) 
				{
					angle -= 180;
					if (angle < 0) 
					{
						angle += 360;
					}
					angle = 360 - angle;
				}

				//store information on where to spawn splash
				int it = 0;
				foreach(int i in ID)
				{
					if(device_id == i)
					{
						angles [it] = angle;
					}
					it ++;
				}
			}
		}
	}

	void StartGame () 
	{
		// Check if single player
		if (connectedPlayers == 1)
		{
			// turn on walls
			GameObject walls = GameObject.FindGameObjectWithTag("Wall");
			walls.SetActive (true);
		}
		else
		{
			// turn off walls
			GameObject walls = GameObject.FindGameObjectWithTag("Wall");
			walls.SetActive(false);
		}

		AirConsole.instance.SetActivePlayers (8);
		GameState = STATE.GAME;
		MessageAll ();
		menu.displayCountdown = true;
		menu.HideMenu ();
		menu.ShowScores ();

		// Play game start audio
		audioMan.PlayGameStart();
	}

	void ResetGame() 
	{
		// Play splash
		audioMan.PlayGameStart();

		// Restart game objects
		foreach (GameObject player in Players) 
		{
			KnobMovement temp = player.GetComponent<KnobMovement> ();
			temp.RestartMe ();
			temp.rigBody.drag = 0.5f;
		}

		int restartedPlayers = 0;
		while (restartedPlayers < connectedPlayers) 
		{
			foreach (GameObject player in Players) 
			{
				// Check if all players have restarted 
				if (player.GetComponent<SpriteRenderer> ().enabled) 
				{
					restartedPlayers++;
				}
			}
		}

		// Reset shrink timer
		shrinkTimer = 15.0f;

		// Set new game state
		GameState = STATE.GAME;
	}

	void FixedUpdate () 
	{
		if (GameState == STATE.READY && connectedPlayers != prevConnectedPlayers) 
		{
			MessageAll ();
			prevConnectedPlayers = connectedPlayers;
		}
		if (GameState == STATE.GAME) 
		{
			// Check if there is a winner yet
			if (destroyedPlayers == (connectedPlayers - 1) && destroyedPlayers > 0) {
				// Get player ID
				int winningPlayer = -1;
				foreach (GameObject player in Players) {
					KnobMovement script = player.GetComponent<KnobMovement> ();
					if (script.destroyMe == false) 
					{
						winningPlayer = script.playerID; // Get ID
						PersonScript pScript = player.GetComponentInChildren<PersonScript>();
						if (!winIncremented)
						{
							pScript.numWins++; // Increment wins for winning player
							winIncremented = true;
						}
					}
				}

				// Get Player nickname with ID
				string name = "No Nickname";
				if (winningPlayer != -1) 
				{
					int device = AirConsole.instance.ConvertPlayerNumberToDeviceId (winningPlayer);
					name = AirConsole.instance.GetNickname (device);
				}
				// Display winner graphic and play sound
				menu.ShowWinner (name);
				if (!playWin) {
					audioMan.PlayWin ();
					playWin = true;
				}

				//Stop player movement
				foreach (GameObject player in Players) { 
					KnobMovement temp = player.GetComponent<KnobMovement> ();
					temp.rigBody.drag = 100;
				} 

				// If pool is shrunk, return to original
				if (shrunk) 
				{
					shrunk = false;
					showdown = false;
					borderFlash = false;
					int time = 2; // Seconds
					Vector3 borderDest = new Vector3 (0.9262f, 0.93f, 0.9279742f);
					Vector3 boundaryDest = new Vector3 (1.0f, 1.0f, 1.0f);
					Vector3 scoresDest = new Vector3 (0.36841f, 0.36841f, 0.36841f);
					int camSize = 5;

					StartCoroutine (Resize (time, border, borderDest));
					StartCoroutine (Resize (time, boundaries, boundaryDest));
					StartCoroutine (Resize (time, scores, scoresDest));
					StartCoroutine (ResizeCamera (time, cam, camSize));
				}
				if (borderRed) {
					borderRed = false;
					Color blue = new Color (56.0f / 255.0f, 58.0f / 255.0f, 1.0f, 194.0f / 255.0f);
					StartCoroutine (FadeColour (2.0f, border.GetComponent<SpriteRenderer> (), blue));
				}


				//Count down restart timer
				if (timer > 0.0f) 
				{
					timer -= Time.deltaTime;
					//uiText.text = (int)timer + " seconds to restart";
				} 
				//When timer has reached zero restart the game when a player taps the screen
				else 
				{
					GameState = STATE.RESTART;

					menu.ShowTapToRestart ();
					if (!playDrop) 
					{
						audioMan.PlayDrop ();
						playDrop = true;
					}

					if (restartTap) 
					{
						// Reset bools
						playWin = false;
						playDrop = false;
						winIncremented = false;
						restartTap = false;

						// Update Menu
						menu.HideWinner ();
						menu.displayRestart = true;

						// Reset
						destroyedPlayers = 0;
						ResetGame ();
						timer = 3.0f;
					}	
				}
			} 
			else if (destroyedPlayers == connectedPlayers && connectedPlayers > 0) 
			{
				// Players have been crafty and all died, so restart
				// If pool is shrunk, return to original
				if (shrunk) 
				{
					shrunk = false;
					showdown = false;
					borderFlash = false;
					float time = 1.5f; // Seconds
					Vector3 borderDest = new Vector3 (0.9262f, 0.93f, 0.9279742f);
					Vector3 boundaryDest = new Vector3 (1.0f, 1.0f, 1.0f);
					Vector3 scoresDest = new Vector3 (0.36841f, 0.36841f, 0.36841f);
					int camSize = 5;

					StartCoroutine (Resize (time, border, borderDest));
					StartCoroutine (Resize (time, boundaries, boundaryDest));
					StartCoroutine (Resize (time, scores, scoresDest));
					StartCoroutine (ResizeCamera (time, cam, camSize));
				}
				if (borderRed) {
					borderRed = false;
					Color blue = new Color (56.0f / 255.0f, 58.0f / 255.0f, 1.0f, 194.0f / 255.0f);
					StartCoroutine (FadeColour (2.0f, border.GetComponent<SpriteRenderer> (), blue));
				}


				//Count down restart timer
				if (timer > 0.0f) {
					timer -= Time.deltaTime;
					//uiText.text = (int)timer + " seconds to restart";
				} 
				//When timer has reached zero restart the game when a player taps the screen
				else {
					GameState = STATE.RESTART;

					menu.ShowTapToRestart ();
					if (!playDrop) {
						audioMan.PlayDrop ();
						playDrop = true;
					}

					if (restartTap) {
						// Reset bools
						playWin = false;
						playDrop = false;
						winIncremented = false;
						restartTap = false;

						// Update Menu
						menu.HideWinner ();
						menu.displayRestart = true;

						// Reset
						destroyedPlayers = 0;
						ResetGame ();
						timer = 3.0f;
					}	
				}
			}
			else // No winner yet
			{
				// Determine when to shrink the pool size
				if (shrinkTimer < 2 && !borderRed && connectedPlayers > 1) 
				{
					borderRed = true;
					Color red = new Color (1.0f, 0.0f, 0.0f, 194.0f / 255.0f);
					StartCoroutine (FadeColour (2.0f, border.GetComponent<SpriteRenderer> (), red));
				}
				if (shrinkTimer > 0 && !shrunk) 
				{
					shrinkTimer -= Time.deltaTime;
				} 
				else 
				{
					if (!shrunk && connectedPlayers > 1) 
					{ 
						// Shrink the pool
						float time = 1.5f; // Seconds
						Vector3 borderDest = new Vector3 (0.7416216f, 0.7446644f, 0.7430422f);
						Vector3 boundaryDest = new Vector3 (0.80553f, 0.80553f, 0.80553f);
						Vector3 scoresDest = new Vector3 (0.29f, 0.29f, 0.29f);
						int camSize = 4;

						audioMan.PlayStretch ();
						StartCoroutine (Resize (time, border, borderDest));
						StartCoroutine (Resize (time, boundaries, boundaryDest));
						StartCoroutine (ResizeCamera (time, cam, camSize));
						StartCoroutine (Resize (time, scores, scoresDest));
						shrunk = true;
						shrinkTimer = 15.0f;
					}
				}

				// If pool shrunk and only two players left, shrink again!
				if (shrunk && (connectedPlayers - destroyedPlayers == 2) && showdownTimer > 0.0f && !showdown) 
				{
					showdownTimer -= Time.deltaTime;
				}
				if (!borderFlash && showdownTimer < 2.0f) 
				{
					borderFlash = true;
					StartCoroutine(FlashBorder(0.5f, border.GetComponent<SpriteRenderer>()));
				}
				if (!showdown && showdownTimer < 0) 
				{
					// Shrink
					float time = 3.0f; // Seconds
					Vector3 borderDest = new Vector3 (0.5572687f, 0.5595551f, 0.5583362f);
					Vector3 boundaryDest = new Vector3 (0.6485946f, 0.6485946f, 0.6485946f);
					Vector3 scoresDest = new Vector3 (0.22f, 0.22f, 0.22f);
					int camSize = 3;

					audioMan.PlayShowdown ();
					StartCoroutine (Resize (time, border, borderDest));
					StartCoroutine (Resize (time, boundaries, boundaryDest));
					StartCoroutine (ResizeCamera (time, cam, camSize));
					StartCoroutine (Resize (time, scores, scoresDest));
					showdown = true;
					showdownTimer = 5.0f;
				}
			}

			// Updating controllers with character rotations
			messagetimer -= Time.deltaTime;
			if (messagetimer < 0) 
			{
				// Reset iterator 
				msgI = 0;		

				//iterate through all players calculate angles and send to respective controllers 
				foreach (GameObject i in Players) 
				{
					//calculate the current angle of the player to 2 decimal places 
					currentAngle = (Mathf.Round ((i.transform.rotation.eulerAngles.z * 100)) / 100);

					//send message to the player
					UpdateMessage (currentAngle, charNums[msgI], msgI);

					//increase iterator
					msgI++;

				}
				messagetimer = 0.1f;
			}			
		}
	}


	// Sends a message to the players controller with the current rotation of their character 
	void UpdateMessage(float angle, int character, int iterator){

		JObject msg = new JObject ();			//message 

		//if current angle is different from the last angle sent send a new message to the controller with an updated rotation 
		//output information sent to console
		if ((int)angle != (int)prevAngle[iterator]) 
		{
			msg.Add ("angle", (int)(-angle));						//add rotation to msg
			msg.Add ("charNo", character + 1);							//add char number to msg
			msg.Add ("state", (int)GameState);
			AirConsole.instance.Message (ID [iterator], msg);		//send message
			debugMessage(msg, ID[iterator]);						//debug
			prevAngle[iterator] = angle;							//update previous angle
			msg.ClearItems();										//clear item
		}

	}

	//Debug message for what information has been sent to controller
	void debugMessage(JObject msg, int ID){
//		Debug.Log ("Message: " + msg + " sent to device: " + ID );
	}

	void OnDestroy () 
	{
		// unregister airconsole events on scene change
		if (AirConsole.instance != null) 
		{
			AirConsole.instance.onMessage -= OnMessage;
		}
	}

	public void KillMe()
	{
		destroyedPlayers++;
	}

	private void Restart()
	{
		AirConsole.instance.SetActivePlayers (8);
		connectedPlayers = 0;
		foreach (GameObject player in Players) 
		{
			Destroy (player);
		}
		Players.Clear ();
	}

	private void ReadyToPlay()
	{
		GameState = STATE.READY;
	}

	private int RandomCharacter()
	{ // Returns a random index into the character list that is not already taken
		
		int returnIndex = 0;
		bool match = true;

		while (match == true) 
		{
			returnIndex = Random.Range (0, 7);

			int matches = 0;
			if (TakenCharacters.Count > 0) 
			{
				foreach (int i in TakenCharacters) 
				{
					if (returnIndex == i) 
					{
						matches++;
					}
				}
			}
			if ((matches >= 1))
			{
				match = false;
			}
		}

		return returnIndex;
	}

	private IEnumerator Resize(float time, GameObject obj, Vector3 desiredScale)
	{
		Vector3 currentScale = obj.transform.localScale;

		float currTime = 0.0f;

		while (currTime <= time)
		{
			obj.transform.localScale = Vector3.Lerp (currentScale, desiredScale, currTime / time);
			currTime += Time.deltaTime;
			yield return null;
		}
	}

	private IEnumerator ResizeCamera(float time, Camera cam, float desiredSize)
	{
		float currentSize = cam.orthographicSize;

		float currTime = 0.0f;

		while (currTime <= time)
		{
			cam.orthographicSize = Mathf.Lerp (currentSize, desiredSize, currTime / time);
			currTime += Time.deltaTime;
			yield return null;
		}
	}

	private IEnumerator FadeColour(float time, SpriteRenderer rend, Color desiredColour)
	{
		Color currentColour = rend.color;

		float currTime = 0.0f;

		while (currTime <= time)
		{
			rend.color = Color.Lerp (currentColour, desiredColour, currTime / time);
			currTime += Time.deltaTime;
			yield return null;
		}
	}

	private IEnumerator FlashBorder(float speed, SpriteRenderer rend)
	{
		float timer = speed;
		bool fadeUp = false;

		Color faded = new Color (rend.color.r, rend.color.g, rend.color.b, 0.3f);
		Color normal = new Color (rend.color.r, rend.color.g, rend.color.b, rend.color.a);
		
		while(borderFlash)
		{
			if (!fadeUp) 
			{
				timer -= Time.deltaTime;
				rend.color = Color.Lerp (faded, normal, timer / speed);
				if (timer < 0) 
				{
					fadeUp = true;
				}
			}
			else 
			{
				timer += Time.deltaTime;
				rend.color = Color.Lerp (faded, normal, timer / speed);
				if (timer > speed) 
				{
					fadeUp = false;
				}
			}
			yield return null;
		}
	}

	private IEnumerator SpawnPlayerInNewRound(int device_id)
	{
		while (GameState != STATE.RESTART) 
		{
			// Wait
			yield return null;
		}

		if (GameState == STATE.RESTART)
		{
			// Set default spawn
			SpawnLocation.Set (0, 0, 0);

			// Assign character
			//int character = RandomCharacter();
			int character = charNums [connectedPlayers];
			TakenCharacters [connectedPlayers] = character;

			// Create player
			GameObject newPlayer = Instantiate (Characters [character], SpawnLocation, Quaternion.identity) as GameObject;
			newPlayer.GetComponent<KnobMovement> ().characterNumber = character;
			newPlayer.GetComponent<KnobMovement> ().SetID (connectedPlayers);
			ID [connectedPlayers] = device_id;
			Players.Add (newPlayer);

			// Add Menu Graphic
			menu.AddConnectGraphic (character, connectedPlayers);
			menu.AddScore (character, connectedPlayers);

			// Increment connected and destroyed players
			connectedPlayers++;
			destroyedPlayers++;

			// Send connection message to controller
			JObject connectionMessage = new JObject ();
			connectionMessage.Add ("state", (int)GameState);
			connectionMessage.Add ("angle", 0);
			AirConsole.instance.Message (device_id, connectionMessage);
			AirConsole.instance.SetActivePlayers (8);

			// Play connect sound
			audioMan.PlayDrop ();

			Debug.Log ("New Player Added");
			yield return null;
		}
	}

	private void MessageAll()
	{
		int iteratorl = 0;		

		JObject statemsg = new JObject ();	

		//iterate through all players calculate angles and send to respective controllers 
		foreach (GameObject i in Players) 
		{
			//add state
			statemsg.Add("state", (int)GameState);
			statemsg.Add ("charNo", charNums[iteratorl] + 1);
			//send message to the player
			AirConsole.instance.Message (ID [iteratorl], statemsg);		//send message
			debugMessage(statemsg, ID[iteratorl]);					//debug						
			statemsg.ClearItems();	

			//increase iterator
			iteratorl++;
		}

	}

	public static void ShuffleArray<T>(T[] arr) 
	{
		for (int i = arr.Length - 1; i > 0; i--) 
		{
			int r = Random.Range(0, i + 1);
			T tmp = arr[i];
			arr[i] = arr[r];
			arr[r] = tmp;
		}
	}

	#endif
}
