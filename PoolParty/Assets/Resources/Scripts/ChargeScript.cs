using UnityEngine;
using System.Collections;

public class ChargeScript : MonoBehaviour
{ // Class handles the physics and lifetime of the charge ability

    // Object references
    private KnobMovement myPlayerScript;
    private GameManager gMan;

    // Attributes
    float timer = 2.0f;
    float forceTimer = 0.1f;
    public PhysicsMaterial2D chargeMaterial;
    private PhysicsMaterial2D defaultMaterial;
    float defaultDrag;
    bool forceApplied = false;
    public float impulse = 25.0f;
    public bool fire = false;
    public Vector3 direction = Vector3.zero;

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
            if (timer > 0.0f)
            {
                timer -= Time.deltaTime;
                if (forceTimer > 0.0f)
                {
                    forceTimer -= Time.deltaTime;
                    myPlayerScript.rigBody.AddForce(direction * impulse, ForceMode2D.Impulse);
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
