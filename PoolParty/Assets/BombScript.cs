using UnityEngine;
using System.Collections;

// <summary>
// When initialised this class handles the animation, collision handling and physics of the bomb ability
// </summary>
public class BombScript : MonoBehaviour 
{	
	// Object references
	private GameManager gMan;

	// Use this for initialization
	void Start () 
	{
		// Get game manager for access to Player List
		gMan =  (GameManager)FindObjectOfType<GameManager>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		// Get direction from centre of bomb to collider point
		Vector3 direction = collider.gameObject.transform.position - transform.position;
		direction.Normalize ();

		// Define force of explosion
		float force = 100;
		float maxDistance = 10; // The distance within which to apply explosive force

		// Send collider flying!
		collider.attachedRigidbody.AddForce(direction * force, ForceMode2D.Impulse);

		// Add impulse to other nearby players based on distance
		foreach (GameObject player in gMan.Players) 
		{
			// Get direction from centre of bomb to player position
			Vector3 newDir = player.transform.position - transform.position;
			float distance = newDir.magnitude;

			// Check distance and that this player is not colliding player
			if (distance < maxDistance && player.GetInstanceID() != collider.gameObject.GetInstanceID())
			{
				// Normalise distance vector
				newDir.Normalize();
				
				// Define magnitude of explosion
				float minMag = 1; float maxMag = 100;

				// Work out force based on distance
				Vector3 Force = Vector3.Lerp(newDir * maxMag, newDir * minMag, distance / maxDistance);

				// Send collider flying!
				player.GetComponent<Rigidbody2D>().AddForce(Force, ForceMode2D.Impulse);
			}
		}

		// Destroy bomb
		Destroy(gameObject);
	}
}
