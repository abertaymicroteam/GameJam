using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChargeScript : MonoBehaviour
{ // Class handles the physics and lifetime of the charge ability

    // Object references
    private KnobMovement myPlayerScript;
    private GameManager gMan;

    // Attributes
    float timer = 4.0f;
    float forceTimer = 0.1f;
    public PhysicsMaterial2D chargeMaterial;
    private PhysicsMaterial2D defaultMaterial;
    float defaultDrag;
    bool forceApplied = false;
    public float impulse = 10.0f;
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
                transform.rotation = new Quaternion(transform.localRotation.x, transform.localRotation.y, -angle * Mathf.Deg2Rad, 1.0f);
                Debug.Log("Rotation " + angle);
                rotated = true;

                // Start particle systems
                ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();
                foreach(ParticleSystem system in systems)
                {
                    system.Play();
                }
            }

            if (timer > 0.0f)
            {
                timer -= Time.deltaTime;
               // if (forceTimer > 0.0f)
                {
                   // forceTimer -= Time.deltaTime;
                    myPlayerScript.rigBody.AddForce(direction * impulse, ForceMode2D.Force);
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
}
