﻿using UnityEngine;
using System.Collections;

public class KnobMovement : MonoBehaviour {

	private Rigidbody rigBody;
	private GameObject dude;
	public Camera camera;
	public GameObject splash;

	private float force;

	// Use this for initialization
	void Start () 
	{
		rigBody = gameObject.GetComponent<Rigidbody> ();
		dude = GameObject.FindGameObjectWithTag ("Player");

		force = 3.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetMouseButtonDown(0)) 
		{
			Vector3 mousePos = Input.mousePosition;
			Vector3 playerPos = camera.WorldToScreenPoint(dude.transform.position);

			Vector3 distance = playerPos - mousePos;

<<<<<<< HEAD
			if ((distance.magnitude < 150.0f) && (distance.magnitude > 30.0f))
=======
			Vector3 splashPos = camera.ScreenToWorldPoint(mousePos);
			splashPos.z = 0;

			Instantiate(splash, splashPos, Quaternion.identity);

			if (distance.magnitude > 0) 
>>>>>>> 3dbb3d3bdd3f7d50d3f132043e175d8222135469
			{
				float ratio = (150.0f - (distance.magnitude - 30.0f)) / 120.0f;

				distance.Normalize();

				rigBody.AddForce (distance * (force * ratio), ForceMode.Impulse);
			}
		}
	}
}
