﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuScript : MonoBehaviour {

	// Object References
	private SpriteRenderer menuRenderer;

	// Graphics
	public enum CHARACTER {PHIL, JAMIE, WALLACE, BERTHA};
	CHARACTER character;
	public List<GameObject> CharacterGraphics;
	private List<GameObject> ActiveGraphics = new List<GameObject> ();
	private List<Vector2> GraphicPositions = new List<Vector2> {new Vector2(-6.56f, 2.37f), new Vector2(-2.41f, 2.37f), new Vector2(1.98f, 2.37f), new Vector2(6.24f, 2.37f),
																new Vector2(-6.71f, -3.07f), new Vector2(-2.41f, -3.07f), new Vector2(1.98f, -3.07f), new Vector2(6.24f, -3.07f)};

	// Use this for initialization
	void Start () 
	{
		menuRenderer = gameObject.GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void ShowMenu()
	{
		menuRenderer.enabled = true;
		foreach (GameObject graphic in ActiveGraphics) 
		{
			SpriteRenderer rend = graphic.GetComponent<SpriteRenderer> ();
			rend.enabled = true;
		}
	}

	public void HideMenu()
	{
		menuRenderer.enabled = false;
		foreach (GameObject graphic in ActiveGraphics) 
		{
			SpriteRenderer rend = graphic.GetComponent<SpriteRenderer> ();
			rend.enabled = false;
		}
	}

	public void ShowConnectGraphic(CHARACTER character, int playerID)
	{ // Displays graphic of connected player's character

		GameObject newGraphic = Instantiate (CharacterGraphics [(int)character], GraphicPositions [playerID], Quaternion.identity) as GameObject;
		ActiveGraphics.Add (newGraphic);
	}
}
