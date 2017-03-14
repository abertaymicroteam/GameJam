using UnityEngine;
using System.Collections;

public class RotLock : MonoBehaviour {


	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
		// Lock position relative to parent every frame
		transform.position.Set(transform.parent.position.x - 1.62f, transform.parent.position.y - 0.5f, transform.parent.position.z);
	}
}
