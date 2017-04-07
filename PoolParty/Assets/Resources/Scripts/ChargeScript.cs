using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChargeScript : MonoBehaviour
{ // Class handles the physics and lifetime of the charge ability

    // Object references
    private KnobMovement myPlayerScript;
    private GameManager gMan;
    private AudioManager audioMan;

    // Attributes
    public float timer = 3.0f;
    float forceTimer = 0.1f;
    public PhysicsMaterial2D chargeMaterial;
    private PhysicsMaterial2D defaultMaterial;
    float defaultDrag;
    bool forceApplied = false;
    public float impulse = 25.0f;
    public bool fire = false;
    private bool rotated = false;
    public Vector3 direction = Vector3.zero;
    public float angle = 0.0f;

    // Use this for initialization
    void Start ()
    {
        // Get parent script
        myPlayerScript = GetComponentInParent<KnobMovement>();

        // get game manager
        gMan = GameObject.FindObjectOfType<GameManager>();

        // Get audio manager
        audioMan = GameObject.FindObjectOfType<AudioManager>();

        // Apply initial effects
        defaultDrag = myPlayerScript.rigBody.drag;
        defaultMaterial = myPlayerScript.GetComponent<CircleCollider2D>().sharedMaterial;
        myPlayerScript.GetComponent<CircleCollider2D>().sharedMaterial = chargeMaterial;
        myPlayerScript.rigBody.drag = 0.0f;
        myPlayerScript.chargeAbilityReady = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (fire)
        {
            if (!rotated)
            {

                // rotate particle system and rotor
                transform.rotation = Quaternion.Euler(0, 0, -angle);
                Debug.Log("Rotation " + angle);
                rotated = true;

                // Start rotor animation
                //Animator rotorAnimation = GetComponentInChildren<Animator>(true);
                //rotorAnimation.enabled = true;

                // Start particle systems
                ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();
                foreach(ParticleSystem system in systems)
                {
                    system.Play();
                }

                // Play audio
                audioMan.PlayCharge();
            }

            if (timer > 0.0f)
            {
                timer -= Time.deltaTime;
                //if (!forceApplied)
                {
                   // forceTimer -= Time.deltaTime;
                    myPlayerScript.rigBody.AddForce(direction * impulse, ForceMode2D.Impulse);
                   // forceApplied = true;
                }
            }
            if (timer <= 0.0f)
            {
                myPlayerScript.GetComponent<CircleCollider2D>().sharedMaterial = defaultMaterial;
                myPlayerScript.rigBody.drag = defaultDrag;
                forceApplied = false;
                Destroy(gameObject);
            }
        }

        if (myPlayerScript.destroyMe || gMan.GameState != GameManager.STATE.GAME)
        {
            timer = 0.0f;
        }
    }

    public void SetAngle(float angle_)
    {
        angle = angle_;
    }
}
