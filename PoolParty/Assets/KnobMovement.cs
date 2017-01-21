using UnityEngine;
using System.Collections;

public class KnobMovement : MonoBehaviour {

	private Rigidbody2D rigBody;
	private GameObject dude;
	public Camera camera;
	public Splash splash;
	public int playerID;
	private GameManager gMan;

	private float force;
	private float lastAngle;

	// Use this for initialization
	void Start () 
	{
		rigBody = gameObject.GetComponent<Rigidbody2D> ();
		dude = GameObject.FindGameObjectWithTag ("Player");
		gMan = GameObject.FindObjectOfType<GameManager> ();
		force = 6.0f;
		lastAngle = 0.0f;
	}

	public void SetID(int ID)
	{
		playerID = ID;
	}

	// Update is called once per frame
	void Update () 
	{
		if (gMan.getAngle (playerID) != lastAngle) 
		{ // New touch press

			lastAngle = gMan.getAngle (playerID);

			// Get mouse and player pos in world coords
			Vector2 playerPos = transform.position;

			// Find point on circumfrence to spawn splash
			Vector2 splashPos = CirclePos (playerPos, (1.0f), gMan.getAngle(playerID)* Mathf.Deg2Rad);

			// Spawn splash
			Instantiate(splash, splashPos, Quaternion.identity);

			// Calculate direction to push player
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
		Vector3 c = new Vector3(centre.x, centre.y, 0);

		pos.x = c.x + radius * Mathf.Sin(angle);
		pos.y = c.y + radius * Mathf.Cos(angle);

		return pos;
	}

	void OnTriggerEnter2D(Collider2D other) 
	{
		Destroy  (gameObject);		
	}
}