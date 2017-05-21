using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;

// TODO: Make this class less of a clusterfuck ie. paint the forth rail bridge
// TODO: Add more to do's
public class GameManager : MonoBehaviour 
{
	// Game State
	public enum STATE { MENU, GAME, RESTART, READY };
	public STATE GameState;

	// Scripts
	public MenuScript menu;
	private AudioManager audioMan;
	private CameraScript cameraScript;

	// Objects for Shrinking
	private GameObject border;
	private GameObject boundaries;
	private GameObject scores;

	// Players
	public List<GameObject> Players;
	public List<PlayerScript> PlayerScripts;
    public List<GameObject> Abilities;
	public GameObject[] Characters;
	public List<int> TakenCharacters;
    [SerializeField]
	private int[] charNums = new int[8] {0, 1, 2, 3, 4, 5, 6, 7};
	public int connectedPlayers = 0;
	private int prevConnectedPlayers = 0;
	private int destroyedPlayers = 0;
	private int msgI = 0; // Update message iterator
	private float messagetimer = 0.1f; // Update messages limited to 10 per second

	// Misc
	private bool restartTap = false;
	private bool playWin = false;
	private bool playDrop = false;
    public bool winner = false;
	public bool newTap = false;
	private float timer = 3.0f;
	private bool winIncremented = false;
	public float rotatormes;
	private bool adShowing = false;
	private bool recievedPlay = false;
	#if !DISABLE_AIRCONSOLE 

	// Play area shrinking
	public float shrinkTimer = 15.0f;
	public float showdownTimer = 5.0f;
	public bool shrunk = false;
	public bool showdown = false;
	public bool borderRed = false;
	public bool borderFlash = false;

	void Awake () 
	{
        //overwrite airconsole functions;
		AirConsole.instance.onMessage += OnMessage;
		AirConsole.instance.onConnect += OnConnect;
		AirConsole.instance.onDisconnect += OnDisconnect;
        AirConsole.instance.onAdShow += OnAdShow;
        AirConsole.instance.onAdComplete += OnAdComplete;
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
		// TODO: Perhaps bundle all of these elements under one "HUD" object. Means only need one
		// resize routine and makes it quicker to make new UI elements shrink to the right size as they will all 
		// shrink the same amount relative to parent.
		border = GameObject.FindGameObjectWithTag("Border");
		boundaries = GameObject.FindGameObjectWithTag ("Boundaries");
		scores = GameObject.FindGameObjectWithTag ("Scores");

		// Get camera script
		Camera cam = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera>();
		cameraScript = cam.GetComponent<CameraScript>();

		// Randomise character order
		ShuffleArray<int>(charNums);
	}

	/// Called when a new player connects
	void OnConnect (int device_id) 
	{
		if (connectedPlayers < 8) 
		{
			if (GameState == STATE.MENU || GameState == STATE.READY)
			{
				// Assign character
				//int character = RandomCharacter();
				int character = charNums [connectedPlayers];			

				// Create player
				GameObject newPlayer = Instantiate (Characters [character], new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
                PlayerScript newScript = newPlayer.GetComponent<PlayerScript>();
				newScript.characterNumber = character;
				newScript.hoveredChar = character;
				newScript.SetID (connectedPlayers);
				newScript.deviceID = device_id;
                newScript.ability = Abilities[0];
				Players.Add (newPlayer);
				PlayerScripts.Add (newScript);

				// Add Menu Graphic
				menu.ShowConnectGraphic (character, connectedPlayers);
				menu.UpdateAbilityGraphic (PlayerScripts[connectedPlayers].hoveredPow, connectedPlayers);
				// Increment connected players
				connectedPlayers++;

                StartCoroutine(ConnectMessage(device_id, connectedPlayers));
				AirConsole.instance.SetActivePlayers (8);
				Debug.Log ("Player " + GetPlayerNumberWithDeviceId(device_id) + " connected");

				// Play connect sound
				audioMan.PlayDrop ();

				// Is the game ready to play? (2 player connected)
				if (AirConsole.instance.GetControllerDeviceIds ().Count > 1) 
				{	
					cameraScript.MoveCamera (new Vector3 (0.0f, 0.0f, -10.0f), 0.75f);
					ReadyToPlay ();
                    MessageAll();
				} 
				else 
				{
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

	// Updates player IDs when someone disconnects
    void updateIds(int disconnectedID)
    {
        int iterator = 0;
        foreach(PlayerScript player in PlayerScripts)
        {
            if(iterator > disconnectedID)
            {
                player.SetID(player.playerID - 1);
            }
            iterator++;
        }
    }

	/// If the game is running and one of the active players leaves, we remove their character and continue playing.
	void OnDisconnect (int device_id) 
	{
		int active_player = GetPlayerNumberWithDeviceId(device_id);
		if (active_player != -1) 
		{
			Debug.Log ("Player " + active_player + " disconnected. Removing!");

			// Update counters for current player states
			connectedPlayers --;
			if (Players [active_player].GetComponent<PlayerScript> ().destroyMe && destroyedPlayers > 0) 
			{
				destroyedPlayers--;
			}

			// Allow character to be retaken
			int toRemove = -1;
			foreach(int i in TakenCharacters)
			{
				if (i == Players[active_player].GetComponent<PlayerScript>().characterNumber)
				{
					toRemove = i;
				}
			}
            if (toRemove != -1)
            {
                TakenCharacters.Remove(toRemove);
				TakenCharacters.Add (-1); // Add empty slot to the end
            }

			// Remove player connect graphic and score from menu
			menu.RemoveConnectGraphic(active_player);
			menu.RemoveScore (Players [active_player].GetComponent<PlayerScript> ().characterNumber);

            // Update remaining player IDs
            updateIds(active_player);

            // Destroy game object and remove from list
            Destroy(Players [active_player]);
			Players.Remove (Players [active_player]);
			PlayerScripts.Remove (PlayerScripts [active_player]);
            AirConsole.instance.SetActivePlayers (8);

            // If there is only one player connected, go back to menu and wait for connection.
			// TODO: Remaining player's controller should return to character select state once this happens, and players can reconnect and game can restart
            if (connectedPlayers == 1) 
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
    void OnMessage(int device_id, JToken data)
	{
		Debug.Log ("Player number is " + GetPlayerNumberWithDeviceId(device_id));

        if (GameState == STATE.RESTART)
        {
			int active_player = GetPlayerNumberWithDeviceId(device_id);
            if (active_player != -1 && !adShowing)
            {
                restartTap = true;
                GameState = STATE.GAME;
            }
        }
        else if (GameState == STATE.MENU || GameState == STATE.READY)
        {

            if (data["title"] != null)
            {
                if (data["title"].ToString() == "recievedChar")
                {
					PlayerScripts[GetPlayerNumberWithDeviceId(device_id)].recievedMessage = true;
                }

                if (data["title"].ToString() == "recievedPlay")
                {
                    recievedPlay = true;
                }
            }

            if (data["button"] != null)
            {

                //find out who the message is from
				int id = -1;
				foreach (PlayerScript player in PlayerScripts)
                {
					if (device_id == player.deviceID)
                    {
						id = player.playerID;
						break;
                    }
                }

                //controller character select
				// TODO: Available/Taken should update in real time on other player's controllers
				// 		 eg. im hovering over phil when available, someone else takes phil, should change to taken on my phone
				if(id != -1)
				{
                	if (PlayerScripts[id].controllerState == PlayerScript.ControllerState.Char)
                	{
                	    if (PlayerScripts[id].locked == false)
                	    {
                	        if (data["button"].ToString() == "left")
                	        {
                	            PlayerScripts[id].hoveredChar--;

                	            // Get current character information
                	            GameObject currentPlayer = Players[id];
                	            PlayerScript currentPlayerScript = currentPlayer.GetComponent<PlayerScript>();
                	            int currCharacter = currentPlayerScript.characterNumber;
                	            int newCharacter = -1;
                	            if (currCharacter > 0)
                	            {
                	                newCharacter = currCharacter - 1;
                	            }
                	            else
                	            {
                	                newCharacter = 7;
                	            }
					
                	            JObject availableMsg = new JObject();
								if (TakenCharacters.Contains(PlayerScripts[id].hoveredChar))
                	            {
                	                availableMsg.Add("available", 1);
                	            }
                	            else
                	            {
                	                availableMsg.Add("available", 2);
                	            }
								AirConsole.instance.Message(PlayerScripts[id].deviceID, availableMsg);
                	            availableMsg.ClearItems();
					
               	                // reate new player
								GameObject newPlayer = Instantiate(Characters[newCharacter], new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
                	            PlayerScript newPlayerScript = newPlayer.GetComponent<PlayerScript>();
                	            newPlayerScript.characterNumber = newCharacter;
                	            newPlayerScript.SetID(id);
                	            newPlayerScript.ability = currentPlayerScript.ability;
								newPlayerScript.hoveredChar = currentPlayerScript.hoveredChar;
								newPlayerScript.hoveredPow = currentPlayerScript.hoveredPow;
								newPlayerScript.deviceID = device_id;
								newPlayerScript.recievedMessage = currentPlayerScript.recievedMessage;
					
                	            Players[id] = newPlayer;
								PlayerScripts [id] = newPlayerScript;
					
               	                // Destroy old player object
                	            Destroy(currentPlayer);
					
               	                // Update Menu Graphic
                	            menu.UpdateConnectGraphic(id, newCharacter);
					
					
              	                // Play sound
                	            audioMan.PlayDrop();					
                	        }
                	        if (data["button"].ToString() == "right")
                	        {
								PlayerScripts[id].hoveredChar++;

                	            // Get current character information
                	            GameObject currentPlayer = Players[id];
                	            PlayerScript currentPlayerScript = currentPlayer.GetComponent<PlayerScript>();
                	            int currCharacter = currentPlayerScript.characterNumber;
                	            int newCharacter = -1;
                	            if (currCharacter < 7)
                	            {
                	                newCharacter = currCharacter + 1;
                	            }
                	            else
                	            {
                	                newCharacter = 0;
                	            }
					
                	            JObject availableMsg = new JObject();
								if (TakenCharacters.Contains(PlayerScripts[id].hoveredChar))
                	            {
                	                availableMsg.Add("available", 1);
                	            }
                	            else
                	            {
                	                availableMsg.Add("available", 2);
                	            }
								AirConsole.instance.Message(PlayerScripts[id].deviceID, availableMsg);
                	            availableMsg.ClearItems();
					
                	            // Create new player
								GameObject newPlayer = Instantiate(Characters[newCharacter], new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
                	            PlayerScript newPlayerScript = newPlayer.GetComponent<PlayerScript>();
                	            newPlayerScript.characterNumber = newCharacter;
                	            newPlayerScript.SetID(id);
                	            newPlayerScript.ability = currentPlayerScript.ability;
								newPlayerScript.hoveredChar = currentPlayerScript.hoveredChar;
								newPlayerScript.hoveredPow = currentPlayerScript.hoveredPow;
								newPlayerScript.deviceID = device_id;
								newPlayerScript.recievedMessage = currentPlayerScript.recievedMessage;
					
                	            Players[id] = newPlayer;
								PlayerScripts [id] = newPlayerScript;
					
                	            // Destroy old player object
                	            Destroy(currentPlayer);
					
                	            // Update Menu Graphic
                	            menu.UpdateConnectGraphic(id, newCharacter);
										
                	            // Play sound
                	            audioMan.PlayDrop();
                	        					
                	        }
                	        if (data["button"].ToString() == "select")
                	        {
								if (!TakenCharacters.Contains(PlayerScripts[id].hoveredChar))
                	            {
									PlayerScripts[id].locked = true;
									TakenCharacters[id] = (PlayerScripts[id].hoveredChar);
									PlayerScripts[id].controllerState = PlayerScript.ControllerState.Power;
                	                JObject newmsg = new JObject();
                	                newmsg.Add("char", 1);
									AirConsole.instance.Message(PlayerScripts[id].deviceID, newmsg);
									charNums[id] = PlayerScripts[id].hoveredChar;
                	            }
                	        }
                	    }
                	}
                	// powerup selection
					else if (PlayerScripts[id].controllerState == PlayerScript.ControllerState.Power)
                	{
					
                	    if (data["button"].ToString() == "back")
                	    {
							PlayerScripts[id].locked = false;
							TakenCharacters.Remove(PlayerScripts[id].hoveredChar);
							PlayerScripts[id].controllerState = PlayerScript.ControllerState.Char;
                	    }
					
                	    if (data["button"].ToString() == "right")
                	    {
							PlayerScripts[id].hoveredPow++;
							menu.UpdateAbilityGraphic(PlayerScripts[id].hoveredPow, id);
							Players[id].GetComponent<PlayerScript>().ability = Abilities[PlayerScripts[id].hoveredPow];
                	    }
					
                	    if (data["button"].ToString() == "left")
                	    {
							PlayerScripts[id].hoveredPow--;
							menu.UpdateAbilityGraphic(PlayerScripts[id].hoveredPow, id);
							Players[id].GetComponent<PlayerScript>().ability = Abilities[PlayerScripts[id].hoveredPow];
                	    }
					
                	    if (data["button"].ToString() == "select")
                	    {
							PlayerScripts[id].controllerState = PlayerScript.ControllerState.Ready;
                	        JObject newmsg = new JObject();
                	        newmsg.Add("pow", 1);
							AirConsole.instance.Message(PlayerScripts[id].deviceID, newmsg);
							PlayerScripts[id].ready = true;
                	    }
					
                	}
                	//readyscreen
					else if (PlayerScripts[id].controllerState == PlayerScript.ControllerState.Ready)
                	{
                	    if (data["button"].ToString() == "back")
                	    {
                	        //menu.UpdateAbilityGraphic (hoveredPow [0], it);
							PlayerScripts[id].controllerState = PlayerScript.ControllerState.Power;
							PlayerScripts[id].ready = false;
                	    }
					
						// TODO: If play button is shown, then a player backs out from ready screen, play button should be removed.
                	    if (data["button"].ToString() == "play")
                	    {
                	        if (connectedPlayers != prevConnectedPlayers)
                	        {
                	            MessageAll();
                	            prevConnectedPlayers = connectedPlayers;
                	        }
							StartGame ();
                	    }
                	}
				}
            }            
        }
        else if (GameState == STATE.READY)
        {
         //   StartGame();
        }
        else
        {
			// Message is a player input, send to appropriate player class for handling.
			int playerNumber = GetPlayerNumberWithDeviceId(device_id);
			if (playerNumber != -1) 
			{
				PlayerScripts [playerNumber].GameMessage (data);
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


		JObject gameState = new JObject ();

		gameState.Add ("sceneInfo", "game");

		AirConsole.instance.SetCustomDeviceState (gameState as JToken);


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
			PlayerScript temp = player.GetComponent<PlayerScript> ();
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

        if (GameState == STATE.MENU && connectedPlayers > 1)
        {

            bool play = true;
            if (AirConsole.instance.IsAirConsoleUnityPluginReady())
            { 
                for (int i = 0; i < connectedPlayers; i++)
                {
                    if (PlayerScripts[i].ready == false)
                    {
                        play = false;
                    }
                }
                if (play)
                {
                    // Sends message to display play button until it is accepted
                    StartCoroutine(SendPlayButton());
                    GameState = STATE.READY;
                }
            }
        }

		//if (GameState == STATE.READY && connectedPlayers != prevConnectedPlayers) 
		//{
		//	MessageAll ();
		//	prevConnectedPlayers = connectedPlayers;
		//}
		if (GameState == STATE.GAME) 
		{
            
            // Check if there is a winner yet
            if (destroyedPlayers == (connectedPlayers - 1))
            {
                // Set winner bool
                winner = true;

				// Get player ID
				int winningPlayer = -1;
				foreach (GameObject player in Players)
                {
					PlayerScript script = player.GetComponent<PlayerScript> ();
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
					PlayerScript temp = player.GetComponent<PlayerScript> ();
					temp.rigBody.drag = 100;
				} 

				// If pool is shrunk, return to original
				if (shrunk) 
				{
                    // Reset shrink values
                    shrunk = false;
                    showdown = false;
                    borderFlash = false;
                    showdownTimer = 5.0f;
                    shrinkTimer = 15.0f;

                    // Zoom out
                    int time = 2; // Seconds
					Vector3 borderDest = new Vector3 (0.9262f, 0.93f, 0.9279742f);
					Vector3 boundaryDest = new Vector3 (1.0f, 1.0f, 1.0f);
					Vector3 scoresDest = new Vector3 (0.36841f, 0.36841f, 0.36841f);
					int camSize = 5;

					StartCoroutine (ResizeUp (time, border, borderDest));
					StartCoroutine (ResizeUp (time, boundaries, boundaryDest));
					StartCoroutine (ResizeUp (time, scores, scoresDest));
					StartCoroutine (cameraScript.ResizeCameraUp (time, camSize));
				}
				if (borderRed)
                {
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

                    //show ad if eonugh time has pssed
                    AirConsole.instance.ShowAd();

					if (restartTap) 
					{
						// Reset bools
						playWin = false;
						playDrop = false;
						winIncremented = false;
						restartTap = false;
                        winner = false;

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
                    // Reset shrink values
					shrunk = false;
					showdown = false;
					borderFlash = false;
                    showdownTimer = 5.0f;
                    shrinkTimer = 15.0f;
                    
                    // Zoom out
					float time = 1.5f; // Seconds
					Vector3 borderDest = new Vector3 (0.9262f, 0.93f, 0.9279742f);
					Vector3 boundaryDest = new Vector3 (1.0f, 1.0f, 1.0f);
					Vector3 scoresDest = new Vector3 (0.36841f, 0.36841f, 0.36841f);
					int camSize = 5;

					StartCoroutine (ResizeUp (time, border, borderDest));
					StartCoroutine (ResizeUp (time, boundaries, boundaryDest));
					StartCoroutine (ResizeUp (time, scores, scoresDest));
					StartCoroutine (cameraScript.ResizeCameraUp (time, camSize));
				}
				if (borderRed)
                {
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

                    //if enough time has passed play ad
                    AirConsole.instance.ShowAd();

					if (restartTap) {
						// Reset bools
						playWin = false;
						playDrop = false;
						winIncremented = false;
						restartTap = false;
                        winner = false;

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
						StartCoroutine (ResizeDown (time, border, borderDest));
						StartCoroutine (ResizeDown (time, boundaries, boundaryDest));
						StartCoroutine (cameraScript.ResizeCameraDown (time, camSize));
						StartCoroutine (ResizeDown (time, scores, scoresDest));
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
					StartCoroutine (ResizeDown (time, border, borderDest));
					StartCoroutine (ResizeDown (time, boundaries, boundaryDest));
					StartCoroutine (cameraScript.ResizeCameraDown (time, camSize));
					StartCoroutine (ResizeDown (time, scores, scoresDest));
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
					float currentAngle = 0;

                    //calculate the current angle of the player to 2 decimal places 
                    if (i.transform.rotation.eulerAngles.z  != 0) 
					{ 
                   		currentAngle = (Mathf.Round((i.transform.rotation.eulerAngles.z * 100)) / 100);
                    }

					//send message to the player
					UpdateMessage (currentAngle, msgI, i.GetComponent<PlayerScript>().chargeLevel);

					//increase iterator
					msgI++;
				}
				messagetimer = 0.1f;
			}

    
        }
	}


	// Sends a message to the players controller with the current rotation of their character, game state and charge level
	void UpdateMessage(float angle, int iterator, int powpow)
	{
		JObject msg = new JObject ();			//message 

		msg.Add ("angle", (int)(-angle));						//add rotation to msg
        msg.Add("powUp", powpow);
		msg.Add ("state", (int)GameState);
		AirConsole.instance.Message (PlayerScripts [iterator].deviceID, msg);		//send message
		debugMessage(msg, PlayerScripts[iterator].deviceID);						//debug
		msg.ClearItems();										//clear item
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

	public void KillMe(int playerNumber)
	{
        JObject deadzo = new JObject();
        deadzo.Add("deadzo", 1);
        AirConsole.instance.Message(AirConsole.instance.ConvertPlayerNumberToDeviceId(playerNumber), deadzo);
        
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
		//GameState = STATE.READY;
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

	private IEnumerator ResizeDown(float time, GameObject obj, Vector3 desiredScale)
	{
		Vector3 currentScale = obj.transform.localScale;

		float currTime = 0.0f;

		while (currTime <= time && !winner)
		{
			obj.transform.localScale = Vector3.Lerp (currentScale, desiredScale, currTime / time);
			currTime += Time.deltaTime;
			yield return null;
		}
	}

    private IEnumerator ResizeUp(float time, GameObject obj, Vector3 desiredScale)
    {
        Vector3 currentScale = obj.transform.localScale;

        float currTime = 0.0f;

        while (currTime <= time)
        {
            obj.transform.localScale = Vector3.Lerp(currentScale, desiredScale, currTime / time);
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

    private IEnumerator ConnectMessage(int device_id, int ID)
    {
        float timer = 0;
        
		while (!PlayerScripts[ID - 1].recievedMessage)
        {
            timer += Time.deltaTime;
            if (timer >= 0.1)
            {
                // Send connection message to controller
                JObject connectionMessage = new JObject();
                connectionMessage.Add("state", 0);
                connectionMessage.Add("angle", 0);
                if (AirConsole.instance.GetControllerDeviceIds().Count > 1)
                {
                    connectionMessage.Add("charNo", PlayerScripts[ID - 1].hoveredChar + 1);
                }
                AirConsole.instance.Message(device_id, connectionMessage);
                timer = 0;
            }
            yield return null;
        }

		Debug.Log("Stopped sending character");    
    }

    private IEnumerator SendPlayButton()
    {
        float timer = 0;

        while (!recievedPlay)
        {
            timer += Time.deltaTime;
            if (timer >= 0.1)
            {
                JObject newmsg = new JObject();
                newmsg.Add("No", 1);
				if (PlayerScripts[0].deviceID != 0 && PlayerScripts[1].deviceID != 0) // once two players are connected and ready
                {
					AirConsole.instance.Message(PlayerScripts[0].deviceID, newmsg);
                }
                timer = 0;
            }
            yield return null;
        }
    }

	private IEnumerator SpawnPlayerInNewRound(int device_id)
	{
        bool roundEnded = false;

		while (GameState != STATE.RESTART && !roundEnded) 
		{
			// Wait
			yield return null;
		}

		if (GameState == STATE.RESTART)
		{
            roundEnded = true;
		}

        if (roundEnded)
        {
            while(GameState != STATE.GAME)
            {
                // Wait
                yield return null;
            }

            // Assign character
            //int character = RandomCharacter();
            int character = charNums[connectedPlayers];
            while (TakenCharacters.Contains(character))
            {
                character = Random.Range(0,7);
            }
			TakenCharacters[connectedPlayers] = (character);

            // Create player
			GameObject newPlayer = Instantiate(Characters[character], new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
            PlayerScript newPlayerScript = newPlayer.GetComponent<PlayerScript>();
            newPlayerScript.characterNumber = character;
            newPlayerScript.SetID(connectedPlayers);
            newPlayerScript.ability = Abilities[Random.Range(0, 2)];
			newPlayerScript.hoveredChar = character;
			newPlayerScript.deviceID = device_id;
            Players.Add(newPlayer);
			PlayerScripts.Add (newPlayerScript);

            // Add Menu Graphic
            menu.AddConnectGraphic(character, connectedPlayers);
            menu.AddScore(character, connectedPlayers);
            menu.HideAbilitySprite(newPlayerScript.playerID);

            // Increment connected and destroyed players
            connectedPlayers++;
            destroyedPlayers++;

            // Send connection message to controller
            JObject connectionMessage = new JObject();
            connectionMessage.Add("state", (int)GameState);
            connectionMessage.Add("angle", 0);
            connectionMessage.Add("lateConnect", character + 1);
            AirConsole.instance.Message(device_id, connectionMessage);
            AirConsole.instance.SetActivePlayers(8);            

            // Play connect sound
            audioMan.PlayDrop();

            Debug.Log("New Player Added");
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
			AirConsole.instance.Message (PlayerScripts [iteratorl].deviceID, statemsg);		//send message
			debugMessage(statemsg, PlayerScripts[iteratorl].deviceID);					//debug						
			statemsg.ClearItems();	
			//increase iterator
			iteratorl++;
		}

	}

   

    void OnAdShow()
    {
        //mute game audio prevent game restart
        AudioListener.volume = 0;
        adShowing = true;
    }

    void OnAdComplete(bool adWasShown)
    {
        if (adWasShown)
        {
            AudioListener.volume = 1;
            adShowing = false;
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

	private int GetPlayerNumberWithDeviceId(int deviceID)
	{
		int playerNumber = -1;
		foreach (PlayerScript player in PlayerScripts)
		{
			if (player.deviceID == deviceID) 
			{
				playerNumber = player.playerID;
				break;
			}
		}
		return playerNumber;
	}

	#endif
}
