using UnityEngine;
using System.Collections;

public class AbilityBarScript : MonoBehaviour
{
    // Object references 
    private KnobMovement playerScript;
    private SpriteRenderer flash;

	// Use this for initialization
	void Start ()
    {
        playerScript = GameObject.Find("Mike(Clone)").GetComponent<KnobMovement>();
        flash = gameObject.GetComponentInChildren<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!playerScript)
        {
            playerScript = GameObject.Find("Mike(Clone)").GetComponent<KnobMovement>();
        }
        // Scale sprite based on ability metre
        transform.localScale.Set(Mathf.Lerp(0.0f, 0.75f, playerScript.chargeLevel / 100.0f), transform.localScale.y, transform.localScale.z);

        // Turn on flash when at 100
        if (playerScript.chargeLevel == 100)
        {
            flash.color = new Color(flash.color.r, flash.color.g, flash.color.b, 255.0f);
        }
	}
}
