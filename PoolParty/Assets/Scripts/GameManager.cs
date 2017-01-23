﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class GameManager : MonoBehaviour 
{
	public Vector3 SpawnLocation;
	public List<GameObject> Players;
	public GameObject[] Characters;
	public float angle = 0.0f;
	public float[] angles;
	public int[] ID;
	public int connectedPlayers;
	public Text uiText;
	public bool gameStarted = false;
	public bool restartGame = false;
	private bool restartTap = false;
	private int destroyedPlayers = 0;
	private float timer = 3.0f;
	#if !DISABLE_AIRCONSOLE 


	public float getAngle(int ID){
		return angles [ID];
	}

	void Awake () {
		AirConsole.instance.onMessage += OnMessage;
		AirConsole.instance.onConnect += OnConnect;
		AirConsole.instance.onDisconnect += OnDisconnect;

	}

	void Start()
	{
//		uiText.text = AirConsole.instance.GetControllerDeviceIds ().Count + "PLAYERS CONNECTED";
	}
	/// <summary>
	/// We start the game if 2 players are connected and the game is not already running (activePlayers == null).
	/// 
	/// NOTE: We store the controller device_ids of the active players. We do not hardcode player device_ids 1 and 2,
	///       because the two controllers that are connected can have other device_ids e.g. 3 and 7.
	///       For more information read: http://developers.airconsole.com/#/guides/device_ids_and_states
	/// 
	/// </summary>
	/// <param name="device_id">The device_id that connected</param>

	void OnConnect (int device_id) {
		if (AirConsole.instance.GetActivePlayerDeviceIds.Count == 0) 
		{
			// Set default spawn
			SpawnLocation.Set(0, 0, 0);

			//create new player
			GameObject newPlayer = Instantiate(Characters[connectedPlayers], SpawnLocation, Quaternion.identity) as GameObject;
			newPlayer.GetComponent<KnobMovement> ().SetID (connectedPlayers);
			ID [connectedPlayers] = device_id;
			Players.Add(newPlayer);
			connectedPlayers++;

			if (AirConsole.instance.GetControllerDeviceIds ().Count >1) 
			{
				
				StartGame ();
			} 
			else 
			{
				//uiText.text =  AirConsole.instance.GetControllerDeviceIds ().Count  + " PLAYERS CONNECTED";

			}
		}

	}

	/// <summary>
	/// If the game is running and one of the active players leaves, we reset the game.
	/// </summary>
	/// <param name="device_id">The device_id that has left.</param>
	void OnDisconnect (int device_id) {
		int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber (device_id);
		if (active_player != -1) 
		{
			//Restart();		
		}
	}

	/// <summary>
	/// Player input
	/// </summary>
	/// <param name="from">From.</param>
	/// <param name="data">Data.</param>
	void OnMessage (int device_id, JToken data) 
	{
		if (restartGame) 
		{
			Debug.Log ("Restart Tap");
			restartTap = true;
		}
		else
		{
			int active_player = AirConsole.instance.ConvertDeviceIdToPlayerNumber (device_id);
			if (active_player != -1) {
				
			}
			
			angle = (float)data ["move"];
			
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

	void StartGame () 
	{
		//uiText.text = "";
		//AirConsole.instance.SetActivePlayers (connectedPlayers);
		//ResetGame();
		gameStarted = true;
	}

	void ResetGame() 
	{
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
		uiText.text = "";
		restartGame = false;
	}

	void FixedUpdate () 
	{
		if (gameStarted) 
		{
			// Check if there is a winner yet
			if (destroyedPlayers == (connectedPlayers - 1)) 
			{
				foreach (GameObject player in Players) 
				{ // Stop all players movement
					KnobMovement temp = player.GetComponent<KnobMovement> ();
					temp.rigBody.drag = 100;
				} 


				// Only one player left, win condition
				if (timer > 0.0f) 
				{
					timer -= Time.deltaTime;
					uiText.text = (int)timer + " seconds to restart";
				} 
				else 
				{
					restartGame = true;

					uiText.text = "Tap to Restart!";
					if (restartTap) 
					{
						destroyedPlayers = 0;
						ResetGame ();
						restartTap = false;
						timer = 3.0f;
					}	
				}
			}
		}
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

	void Restart()
	{
		AirConsole.instance.SetActivePlayers (0);
		connectedPlayers = 0;
		foreach (GameObject player in Players) 
		{
			Destroy (player);
		}
		Players.Clear ();
	}
	#endif
}