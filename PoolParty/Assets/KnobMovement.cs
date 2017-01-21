using UnityEngine;
using System.Collections;

public class KnobMovement : MonoBehaviour {

	private Rigidbody rigBody;
	private GameObject dude;
	public Camera camera;

	// Use this for initialization
	void Start () 
	{
		rigBody = gameObject.GetComponent<Rigidbody> ();
		dude = GameObject.FindGameObjectWithTag ("Player");
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetMouseButtonDown(0)) 
		{
			Vector3 mousePos = Input.mousePosition;
			Vector3 playerPos = camera.WorldToScreenPoint(dude.transform.position);

			Vector3 distance = playerPos - mousePos;

			if (distance.magnitude > 0) 
			{
				distance.Normalize();

				rigBody.AddForce (distance * 2.0f, ForceMode.Impulse);
			}
		}
	}
}
