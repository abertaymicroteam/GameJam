using UnityEngine;
using System.Collections;

// This class handles the effects of the Juggernaut ability and destroys itself after a set time
public class JugScript : MonoBehaviour
{
    // Object references
	private PlayerScript myPlayerScript;
    private GameManager gMan;
    private AudioManager audioMan;

    // Attributes
    float timer = 7.5f;
    float popTimer = 1.0f;
    bool popTimerActive = false;
    float defaultDrag;
    Vector3 defaultScale;

	// Use this for initialization
	void Start ()
    {
        // Get parent script
        myPlayerScript = GetComponentInParent<PlayerScript>();

        // get game manager
        gMan = GameObject.FindObjectOfType<GameManager>();

        // Get audio manager
        audioMan = GameObject.FindObjectOfType<AudioManager>();

        // Apply initial effects
        defaultDrag = myPlayerScript.rigBody.drag;
        defaultScale = myPlayerScript.gameObject.transform.localScale;
        myPlayerScript.rigBody.drag = 15.0f;
        //myPlayerScript.gameObject.transform.localScale *= 1.25f;

        // Play activate audio
        audioMan.PlayJugActivate();
        popTimerActive = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Wait to play pop sound effect
        if (popTimerActive && popTimer > 0.0f)
        {
            popTimer -= Time.deltaTime;
        }

        if (popTimer <= 0.0f && popTimerActive)
        {
            popTimerActive = false;
            audioMan.PlayDrop();
        }

        // Count to ability lifetime
	    if (timer > 0.0f)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            // Destroy effect
            myPlayerScript.rigBody.drag = defaultDrag;
            myPlayerScript.gameObject.transform.localScale = defaultScale;
            audioMan.PlayDeath(); // Pop sound works well here
            Destroy(gameObject);
        }

        if (myPlayerScript.destroyMe || gMan.GameState != GameManager.STATE.GAME)
        {
            timer = 0.0f;
        }
	}
}
