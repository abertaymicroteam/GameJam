using UnityEngine;
using System.Collections;

public class KnobMovement : MonoBehaviour {

	public Rigidbody2D rigBody;
	public Splash splash;
	public int playerID;
	private GameManager gMan;

	public float force;
	private float lastAngle;
	public bool destroyMe;
	public Vector3 SpawnLocation;
	private bool match;
	private SpriteRenderer ringRenderer;
	private CircleCollider2D col;

	// Use this for initialization
	void Start () 
	{
		// Get objects and initialise variables
		rigBody = gameObject.GetComponent<Rigidbody2D> ();
		gMan = GameObject.FindObjectOfType<GameManager> ();
		lastAngle = 0.0f;
		ringRenderer = gameObject.GetComponent<SpriteRenderer> ();
		col = gameObject.GetComponent<CircleCollider2D> ();

		// Generate random start position
		SpawnLocation.Set(Random.Range(-6,6),Random.Range(-4,4), 0);

		do 
		{
			foreach (GameObject i in gMan.Players)
			{
				if(SpawnLocation.x <  i.transform.position.x + 2.0f && SpawnLocation.x > i.transform.position.x - 2.0   && SpawnLocation.y < i.transform.position.y + 2.0 && SpawnLocation.y > i.transform.position.y- 2.0){
					SpawnLocation.Set(Random.Range(-6,6),Random.Range(-4,4),0 );
					match = true;
				}
				else
				{
					match = false;
				}
			}
		} 
		while(match);

		transform.position = SpawnLocation;
	}

	public void SetID(int ID)
	{
		playerID = ID;
	}

	// Update is called once per frame
	void FixedUpdate () 
	{
		if ((gMan.getAngle (playerID) != lastAngle) && (gMan.gameStarted == true) && (!destroyMe))
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
		// Hide sprite and disable collisions
		ringRenderer.enabled = false;
		col.enabled = false;
		rigBody.velocity.Set (0, 0);

		// Set flag for game manager
		destroyMe = true;
		gMan.KillMe ();

		//set respawn position
		SpawnLocation.Set(Random.Range(-6,6),Random.Range(-4,4), 0);
		bool match = true;
		do 
		{
			foreach (GameObject i in gMan.Players)
			{
				if(SpawnLocation.x <  i.transform.position.x + 1.5 && SpawnLocation.x > i.transform.position.x - 1.5   && SpawnLocation.y < i.transform.position.y + 1.5 && SpawnLocation.y > i.transform.position.y-1.5){
					SpawnLocation.Set(Random.Range(-5,5),Random.Range(-3,3),0 );
					match = true;
				}
				else
				{
					match = false;
				}
			}
		} 
		while(match);

		rigBody.drag = 100;

		gameObject.transform.position = SpawnLocation;

	}

	public void RestartMe()
	{
		Debug.Log ("Player restart");
		destroyMe = false;
		ringRenderer.enabled = true;
		col.enabled = true;
		rigBody.drag = 0.5f;
	}
}