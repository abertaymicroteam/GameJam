using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour 
{
	// Attributes
	private Camera myCamera;
	bool moving = false;

	// Use this for initialization
	void Start () 
	{
		myCamera = GetComponent<Camera> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void MoveCamera(Vector3 position, float timeToTake)
	{
		// Moves camera towards given position at given speed
		StartCoroutine (Move (position, timeToTake));
	}

	private IEnumerator Move(Vector3 target, float time)
	{
		Vector3 currentPosition = transform.position;
		moving = true;

		float currTime = 0.0f;

		while (currTime <= time)
		{
			transform.position = Vector3.Lerp(currentPosition, target, currTime / time);
			currTime += Time.deltaTime;
			yield return null;
		}

		// Account for any small error once lerp is complete
		transform.position = target;

		// No longer moving
		moving = false;

		// Hide loading screen once off camera    (TODO: Do this elsewhere by checking when this function finishes somehow, so that this function can be used generically to move the camera rather than just at the start)
		MenuScript menu =  GameObject.FindGameObjectWithTag("Menu").GetComponent<MenuScript>();
		menu.HideTitle ();
	}
}
