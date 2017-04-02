using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// <summary>
// When initialised this class handles the animation, collision handling and physics of the bomb ability
// </summary>
public class BombScript : MonoBehaviour 
{	
	// Object references
	private GameManager gMan;
    private AudioManager audioMan;

	// Animation prefabs and containers
	public List<GameObject> Animations = new List<GameObject>();
	private GameObject release;
	private GameObject active;
	private GameObject explode;

	// Attributes
	public int owner = 0; // The InstanceID of the player who this ability belongs to
	private bool activated = false; // Is the bomb armed
	private bool spawned = true;
	private float timer = 4.0f;
    private float bubbleTimer = 2.0f;
    private float beepTimer = 0.0f;
    private bool bubbleTimerActive = false;
    private bool beepTimerActive = false;
    Vector3 spawnPos;

	// Use this for initialization
	void Start () 
	{
		// Get game manager for access to Player List
		gMan =  (GameManager)FindObjectOfType<GameManager>();

        // Get audio manager
        audioMan = (AudioManager)FindObjectOfType<AudioManager>();

        // De-parent the object
        transform.parent = null;
        transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
		// Spawn release animation
		release = Instantiate(Animations[0], transform, false) as GameObject;

        // Start counting to bubbles sound
        bubbleTimerActive = true;        
	}
	
	// Update is called once per frame
	void Update () 
	{
        // Wait to play bubble sound effect
        if (bubbleTimerActive && bubbleTimer > 0.0f)
        {
            bubbleTimer -= Time.deltaTime;
        }

        if (bubbleTimer <= 0.0f && bubbleTimerActive)
        {
            bubbleTimerActive = false;
            audioMan.PlayBubbles();
        }

        // Play beep every one second once activated
        if (beepTimerActive && beepTimer > 0.0f)
        {
            beepTimer -= Time.deltaTime;
        }
        if (beepTimerActive && beepTimer <= 0.0f)
        {
            beepTimer = 1.0f;
            audioMan.PlayBeep();
        }

        if (spawned && timer > 0.0f) {
			timer -= Time.deltaTime;
		}

		// Swap to active animation once spawn finished
		if (spawned && timer <= 0.0f) 
		{
			activated = true;
			spawned = false;
            beepTimerActive = true;
			active = Instantiate (Animations [1], transform, false) as GameObject;
			//DestroyObject (release);
		}
	}

	// Sets owner of this ability (Instance ID)
	public void SetOwner(int ID)
	{
		owner = ID;
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
        if (activated)
        {
			// Play explosion animation 
			explode = Instantiate(Animations[2], transform, false) as GameObject;
			explode.transform.parent = null;

            // Play explosion sound
            audioMan.PlayBombExplode();

            // Get direction from centre of bomb to collider point
            Vector3 direction = collider.gameObject.transform.position - transform.position;
            direction.Normalize();

            // Define force of explosion
            float force = 100.0f;
            float maxDistance = 7.5f; // The distance within which to apply explosive force

            // Send player flying!
            collider.attachedRigidbody.AddForce(direction * force, ForceMode2D.Impulse);

            // Add impulse to other nearby players based on distance
            foreach (GameObject player in gMan.Players)
            {
                // Get direction from centre of bomb to player position
                Vector3 newDir = player.transform.position - transform.position;
                float distance = newDir.magnitude;

                // Check distance and that this player is not colliding player
                if (distance < maxDistance && player.GetInstanceID() != collider.gameObject.GetInstanceID() && player.GetInstanceID() != owner)
                {
                    // Normalise distance vector
                    newDir.Normalize();

                    // Define magnitude of explosion
                    float minMag = 1; float maxMag = force;

                    // Work out force based on distance
                    Vector3 Force = Vector3.Lerp(newDir * maxMag, newDir * minMag, distance / maxDistance);

                    // Send player flying!
                    player.GetComponent<Rigidbody2D>().AddForce(Force, ForceMode2D.Impulse);
                }
            }

            // Destroy bomb
            Destroy(gameObject);
        }		
	}

    //private void OnTriggerExit2D(Collider2D collider)
   // {
   //     if (collider.gameObject.GetInstanceID() == owner && !activated)
    //    {
    //        activated = true;
   //     }
   // }
}
