using UnityEngine;
using System.Collections;

public class KnobMovement : MonoBehaviour {

	private Rigidbody2D rigBody;
	private GameObject dude;
	public Camera camera;
	public Splash splash;

	private float force;

	// Use this for initialization
	void Start () 
	{
		rigBody = gameObject.GetComponent<Rigidbody2D> ();
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

			if ((distance.magnitude < 150.0f) && (distance.magnitude > 30.0f))
			{
				Vector3 splashPos = camera.ScreenToWorldPoint(mousePos);
				splashPos.z = 0;

				Instantiate(splash, splashPos, Quaternion.identity);

				if (distance.magnitude > 0) 
				{
					float ratio = (150.0f - (distance.magnitude - 30.0f)) / 120.0f;

					distance.Normalize();

					rigBody.AddForce (distance * (force * ratio), ForceMode2D.Impulse);
				}
			}
		}
	}
}