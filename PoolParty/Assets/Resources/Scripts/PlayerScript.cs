﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class PlayerScript : MonoBehaviour {

	// Script references 
	public Splash splash;
	private GameManager gMan;
	private AudioManager audioMan;

	// Physics
	public Rigidbody2D rigBody;
	public float force;
	private const int MAX_VELOCITY = 50;
	// Limiting velocity
	private float startDrag;
	private float dragTimer = 1.0f;
	private bool drag = false;

	// Controller
	public enum ControllerState { Char, Power, Ready, Game };
	public ControllerState controllerState = ControllerState.Char;
	public int hoveredChar = 0;
	public int hoveredPow = 0;
	public bool locked = false;
	public bool ready = false;
	public bool recievedMessage = false;

    // Personal Attributes
	public Vector3 SpawnLocation;
	private SpriteRenderer ringRenderer;
	public int playerID;
	public int deviceID;
	public int characterNumber = 0;
	private float angle = 0;
	private float tapTimer = 0;
	public bool destroyMe;
	private bool match;

    // Ability
	public GameObject ability;
    public const int CHARGE_FULL = 100;
    public const int CHARGE_EMPTY = 1;
    public int chargeLevel = CHARGE_EMPTY;
    public int chargePerSec = 1;
    public float chargeTimer = 1;
    public int bumpBoost = 2;
    public int killBoost = 10;
    public bool abilityAvailable = false;
    public bool chargeAbilityReady = false;
    public bool abilityInUse = false;

	// Collisions
	private CircleCollider2D col;
	private List<Collider2D> currentColliders = new List<Collider2D>();
	private bool colliding;
    private Collision2D lastCollision;

    // Braking
	public GameObject brakeParticles;
    private float holdtimer = 0.0f;
	private float brakeCooldownTimer = 0.0f;
    private bool applyDrag = false;
	private bool runHoldtimer = false;
	private bool runCoolDownTimer = false;

	// Don't play audio for same collision within 0.5 seconds
	private float audioTimer = 0.5f;
	private int lastCollided = 0; 

	// Use this for initialization
	void Start () 
	{
		// Get objects and initialise variables
		rigBody = gameObject.GetComponent<Rigidbody2D> ();
		startDrag = rigBody.drag;
		gMan = GameObject.FindObjectOfType<GameManager> ();
		audioMan = GameObject.FindGameObjectWithTag ("Audio").GetComponent<AudioManager> ();
		ringRenderer = gameObject.GetComponent<SpriteRenderer> ();
		col = gameObject.GetComponent<CircleCollider2D> ();

		// Generate random start position
		SpawnLocation.Set(Random.Range(-6,6),Random.Range(-4,4), 0);
		do 
		{ // Check if it overlaps another player
			foreach (GameObject i in gMan.Players)
			{
				if(SpawnLocation.x <  i.transform.position.x + 3.0f && SpawnLocation.x > i.transform.position.x - 3.0   && SpawnLocation.y < i.transform.position.y + 3.0 && SpawnLocation.y > i.transform.position.y- 3.0)
				{
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
		// Increment audio timer
		tapTimer += Time.deltaTime;
		if (audioTimer > 0) {
			audioTimer -= Time.deltaTime;
		}

        // Increment charge timer
        if (!gMan.winner && !destroyMe)
        {
            if (chargeTimer > 0.0f && !abilityAvailable)
            {
                chargeTimer -= Time.deltaTime;
            }
            else
            {
				if (gMan.GameState == GameManager.STATE.GAME)
				{
					// Only charge when game is active
					chargeLevel += chargePerSec;
				}
                chargeTimer = 1.0f;
            }
        }

        if (chargeLevel > CHARGE_FULL)
        {
            abilityAvailable = true;
            chargeLevel = CHARGE_FULL;
        }

		// Limit velocity
		if (rigBody.velocity.magnitude > MAX_VELOCITY) 
		{
			rigBody.drag = 20;
			drag = true;
		}
		if (drag) {
			if (dragTimer > 0) {
				dragTimer -= Time.deltaTime;
			} else {
				rigBody.drag = startDrag;
				drag = false;
				dragTimer = 1.0f;
			}
		}

        CheckBrakes();
    }

	// Called by game manager when this player's device sends an input message
	public void GameMessage(JToken data)
	{
		// Check if message is power up or movement
		// TODO: Investigate problems with power up button. Doesn't always turn on/off at right times
		if (data["button"] != null)
		{
			if (data["button"].ToString() == "pow")
			{
				// Activate ability if player is alive				
				if (!destroyMe)
				{ 
					UseAbility();
				}
			}
		}
		if (data["move"] != null)
		{
			if (data["move"] != null)
			{				
				//get the angle information from the players tap
				angle = (float)data["move"];

				//calculate ere to apply force to the player
				if (angle > 0)
				{
					angle = 180 - angle;
				}
				else if (angle < 0)
				{
					angle -= 180;
					if (angle < 0)
					{
						angle += 360;
					}
					angle = 360 - angle;
				}
			}

			if ((gMan.GameState == GameManager.STATE.GAME) && (!destroyMe) && (tapTimer > 0.125f) && (!gMan.menu.displayCountdown))
			{ // New touch press

				// Get mouse and player pos in world coords
				Vector2 playerPos = transform.position;

				// Find point on circumfrence to spawn splash
				Vector2 splashPos = CirclePos (playerPos, (1.0f), angle * Mathf.Deg2Rad);

				// Spawn splash
				Instantiate(splash, splashPos, Quaternion.identity);

				// Calculate direction to push player
				Vector2 direction = playerPos - splashPos;
				direction.Normalize ();

				if (chargeAbilityReady)
				{
					// Do charge boost instead
					GetComponentInChildren<ChargeScript>().direction = direction;
					GetComponentInChildren<ChargeScript>().angle = angle;
					GetComponentInChildren<ChargeScript>().fire = true;
					chargeAbilityReady = false;

					// Reset timer
					tapTimer = 0;
				}
				else
				{
					// Add force
					rigBody.AddForce(direction * force, ForceMode2D.Impulse);

					// Reset timer
					tapTimer = 0;
				}
			}
		}
        if (data["input"] != null)
        {
            if (data["input"].ToString() == "hold_start")
            {
                Debug.Log("hold start");
                runHoldtimer = true;
            }
            if (data["input"].ToString() == "hold_end")
            {
                Debug.Log("hold end");
                runHoldtimer = false;
                holdtimer = 0.0f;
                applyDrag = false;
                rigBody.drag = 0.5f;
            }
        }
    }

    public void UseAbility()
    {
        // Spawn ability when ready
        if (abilityAvailable && gMan.GameState == GameManager.STATE.GAME)
        {
            GameObject newAbility = Instantiate(ability, gameObject.transform, false) as GameObject;
            abilityInUse = true;
            if (newAbility.GetComponent<BombScript>() != null)
            {
                abilityInUse = false; // Don't disable braking for bomb ability
                newAbility.GetComponent<BombScript>().SetOwner(gameObject.GetInstanceID());
            }
            if (newAbility.GetComponent<ChargeScript>() != null)
            {
                newAbility.GetComponent<ChargeScript>().SetAngle(angle);
            }
            abilityAvailable = false;
            chargeLevel = CHARGE_EMPTY;
       
        }
      
    }

    private void CheckBrakes()
    {
        if (runHoldtimer)
        {
            holdtimer += Time.deltaTime;
        }
		if (runCoolDownTimer) 
		{
			brakeCooldownTimer += Time.deltaTime;
			if (brakeCooldownTimer > 3.0f) 
			{
				runCoolDownTimer = false;
				brakeCooldownTimer = 0.0f;
			}
		}
		if(holdtimer > 0.1f && !runCoolDownTimer && !applyDrag && !abilityInUse)
        {
			Instantiate (brakeParticles, transform.position, Quaternion.identity);
			rigBody.drag = 6.0f;
            applyDrag = true;
        }
        if(holdtimer > 0.6f)
        {
            applyDrag = false;
            runHoldtimer = false;
			runCoolDownTimer = true;
            holdtimer = 0.0f;
            rigBody.drag = 0.5f;
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
		if(other.tag == "Score")
		{ // If overlapping with a score sprite, fade out the sprite
			ScoreScript scoreScript = other.GetComponent<ScoreScript> ();
			scoreScript.FadeOut ();
			currentColliders.Add(other);
			if (!destroyMe)
			colliding = true;
		}
		else if (other.tag != "PowerUp")
		{
			// Dead. Hide sprite and disable collisions
			ringRenderer.enabled = false;
			col.enabled = false;
			rigBody.velocity.Set (0, 0);

			// Check if currently fading out a score
			if (colliding) 
			{
				foreach (Collider2D coll in currentColliders) 
				{
                    // Fade score back in
                    if (coll != null)
                    {
                        ScoreScript temp = coll.GetComponent<ScoreScript>();
                        temp.FadeIn();
                    }
				}
				colliding = false;
			}

            // Add bonus to player that killed you
            if (lastCollision != null)
				lastCollision.gameObject.GetComponent<PlayerScript>().chargeLevel += killBoost;
			
			// Play death sound
			audioMan.PlayDeath();			

			// Set flag for game manager
			destroyMe = true;
			gMan.KillMe (characterNumber);
			
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

	}

	void OnTriggerExit2D(Collider2D other)
	{
		if(other.tag == "Score")
		{
			ScoreScript scoreScript = other.GetComponent<ScoreScript> ();
			scoreScript.FadeIn ();
			currentColliders.Remove (other);
			if (currentColliders.Count == 0)
			colliding = false;
		}
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.gameObject.tag == "Player")
		{
			// Play hit audio on only one collider (whichever instance ID is lower)
			if (gameObject.GetInstanceID() < collision.gameObject.GetInstanceID()) 
			{
				if (audioTimer > 0 && collision.gameObject.GetInstanceID() == lastCollided)
				{
					// Do not play audio
				}
				else 
				{
					audioMan.PlayHit ();
					lastCollided = collision.gameObject.GetInstanceID ();
					audioTimer = 0.5f;
				}
			}

            // Boost charge timer for being involved in the fight!
            chargeLevel += bumpBoost;

            // Turn off charge once hit another player
            if (GetComponentInChildren<ChargeScript>() != null)
            {
                if (GetComponentInChildren<ChargeScript>().fire)
                {
                    GetComponentInChildren<ChargeScript>().timer = 0.0f;
                }
            }

            // Store last collision for kill reward
            lastCollision = collision;

            // Check for current power up
            if(GetComponentInChildren<JugScript>() != null)
            {
                // Bounce attacker off
                Vector3 direction = collision.transform.position - transform.position;
                direction.Normalize();
                collision.rigidbody.AddForce(direction * 100.0f, ForceMode2D.Impulse);
                audioMan.PlayJugBounce();
            }
		}
	}

	public void RestartMe()
	{
		destroyMe = false;
		ringRenderer.enabled = true;
		col.enabled = true;
		rigBody.drag = 0.5f;
	}
}