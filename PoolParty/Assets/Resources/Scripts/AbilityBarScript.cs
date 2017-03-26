using UnityEngine;
using System.Collections;

public class AbilityBarScript : MonoBehaviour
{
    // Object references 
    private KnobMovement playerScript;
    private SpriteRenderer[] renderers;

	// Private attributes
	private bool flashing = false;

	// Public attributes
	public float maxAlpha = 1.0f;

	// Use this for initialization
	void Start ()
    {
        playerScript = GameObject.Find("Mike(Clone)").GetComponent<KnobMovement>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!playerScript)
        {
            playerScript = GameObject.Find("Mike(Clone)").GetComponent<KnobMovement>();
        }

        // Scale sprite based on ability metre
		transform.localScale =  new Vector3 (Mathf.Lerp(0.0f, 42.16f, playerScript.chargeLevel / 100.0f), transform.localScale.y, transform.localScale.z);

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
				Debug.Log ("Flash");
				flash.enabled = true;
				flashing = true;
				StartCoroutine (Flash (0.5f, flash));
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
