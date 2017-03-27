using UnityEngine;
using System.Collections;

// This class handles the effects of the Juggernaut ability and destroys itself after a set time
public class JugScript : MonoBehaviour
{
    // Object references
    private KnobMovement myPlayerScript;
    private GameManager gMan;

    // Attributes
    float timer = 7.5f;
    float defaultDrag;
    Vector3 defaultScale;

	// Use this for initialization
	void Start ()
    {
        // Get parent script
        myPlayerScript = GetComponentInParent<KnobMovement>();

        // get game manager
        gMan = GameObject.FindObjectOfType<GameManager>();

        // Apply initial effects
        defaultDrag = myPlayerScript.rigBody.drag;
        defaultScale = myPlayerScript.gameObject.transform.localScale;
        myPlayerScript.rigBody.drag = 15.0f;
        myPlayerScript.gameObject.transform.localScale *= 1.25f;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (timer > 0.0f)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            // Destroy effect
            myPlayerScript.rigBody.drag = defaultDrag;
            myPlayerScript.gameObject.transform.localScale = defaultScale;
            Destroy(gameObject);
        }

        if (myPlayerScript.destroyMe || gMan.GameState != GameManager.STATE.GAME)
        {
            timer = 0.0f;
        }
	}
}
