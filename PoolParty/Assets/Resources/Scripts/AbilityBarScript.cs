using UnityEngine;
using System.Collections;

public class AbilityBarScript : MonoBehaviour
{
    // Object references 
	private PlayerScript playerScript;
    private GameManager gMan;
    private SpriteRenderer[] renderers;

	// Private attributes
	private bool flashing = false;

	// Public attributes
	public float maxAlpha = 1.0f;
    public int myCharacter = 0;

	// Use this for initialization
	void Start ()
    {
        // Get game manager script and player script for this character
        gMan = GameObject.FindObjectOfType<GameManager>();
        foreach (GameObject player in gMan.Players)
        {
            PlayerScript script = player.GetComponent<PlayerScript>();
            if (script.characterNumber == myCharacter)
            {
                playerScript = script;
            }
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Scale sprite based on ability metre
		transform.localScale =  new Vector3 (Mathf.Lerp(3.0f, 42.16f, playerScript.chargeLevel / 100.0f), transform.localScale.y, transform.localScale.z);

        // Turn on flash when at 100
		if (playerScript.chargeLevel >= 100 && flashing == false)
        {
			renderers = gameObject.GetComponentsInChildren<SpriteRenderer> ();
			SpriteRenderer flash = GetComponent<SpriteRenderer>();
			foreach (SpriteRenderer rend in renderers) 
			{
				if (rend.transform.parent != null) 
				{ // Get child component, not my component
					flash = rend;
				}
			}
			if (flash != null) 
			{
				flash.enabled = true;
				flashing = true;
				StartCoroutine (Flash (0.5f, flash));
			}
        }

        // Turn off flash once ability used
        if (playerScript.chargeLevel < 100 && flashing == true)
        {
            renderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
            SpriteRenderer flash = GetComponent<SpriteRenderer>();
            foreach (SpriteRenderer rend in renderers)
            {
                if (rend.transform.parent != null)
                { // Get child component, not my component
                    flash = rend;
                }
            }
            if (flash != null)
            {
                flash.enabled = false;
                flashing = false;
            }
        }
    }

	private IEnumerator Flash(float speed, SpriteRenderer rend)
	{
		float timer = speed;
		bool fadeUp = true;

		Color faded = new Color (rend.color.r, rend.color.g, rend.color.b, 0.3f);

		while(flashing)
		{
			// Inside loop since maxAlpha will change 
			Color normal = new Color (rend.color.r, rend.color.g, rend.color.b, maxAlpha);

			if (!fadeUp) 
			{
				timer -= Time.deltaTime;
				rend.color = Color.Lerp (faded, normal, timer / speed);
				if (timer < 0) 
				{
					fadeUp = true;
				}
			}
			else 
			{
				timer += Time.deltaTime;
				rend.color = Color.Lerp (faded, normal, timer / speed);
				if (timer > speed) 
				{
					fadeUp = false;
				}
			}
			yield return null;
		}
	}
}
