using UnityEngine;
using System.Collections;

public class KnobMovement : MonoBehaviour {

	private Rigidbody2D rigBody;
	private GameObject dude;
	public Camera camera;
	public Splash splash;
	private GameManager gMan;

	private float force;
	private float lastAngle;

	// Use this for initialization
	void Start () 
	{
		rigBody = gameObject.GetComponent<Rigidbody2D> ();
		dude = GameObject.FindGameObjectWithTag ("Player");
		gMan = GameObject.FindObjectOfType<GameManager> ();

		force = 3.0f;
		lastAngle = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (gMan.angle != lastAngle) 
		{ // New press

			lastAngle = gMan.angle;

			// Get mouse and player pos in world coords
			Vector2 mousePos = Input.mousePosition;
			Vector2 playerPos = camera.WorldToScreenPoint(dude.transform.position);

			// Get screen centre
		//	Vector2 centre = new Vector2(Screen.width, Screen.height) / 2;

			// Offset mouse pos by centre to put (0, 0) at the centre of screen rather than bottom left
			mousePos -= playerPos; // CHANGE THIS WHEN CONTORLLER

			// Find angle between the click and player
			float angle = Mathf.Atan2(mousePos.x, mousePos.y);

			// Find point on circumfrence to spawn splash
			Vector2 splashPos = CirclePos (playerPos, (1.0f), gMan.angle * Mathf.Deg2Rad);

			// Spawn splash
			Instantiate(splash, splashPos, Quaternion.identity);

			// Calculate direction to push player
			playerPos = dude.transform.position;
			Vector2 direction = playerPos - splashPos;
			direction.Normalize ();

			// Add force
			rigBody.AddForce (direction * force, ForceMode2D.Impulse);
		}
	}

	Vector2 CirclePos(Vector2 centre, float radius, float angle)
	{
		// Return position on circumfrence around player based on input angle
		Vector2 pos;
		Vector3 c = camera.ScreenToWorldPoint (new Vector3(centre.x, centre.y, 0));

		pos.x = c.x + radius * Mathf.Sin(angle);
		pos.y = c.y + radius * Mathf.Cos(angle);

		return pos;
	}
}