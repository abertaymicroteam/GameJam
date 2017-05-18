using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spins object around axis
public class Spin : MonoBehaviour {

    private float speed = -0.75f;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		transform.Rotate (Vector3.forward * speed);
	}
}
