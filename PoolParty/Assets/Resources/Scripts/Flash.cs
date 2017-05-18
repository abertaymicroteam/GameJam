using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Causes the object's SpriteRenderer to flash between two opacity values
public class Flash : MonoBehaviour {

	public float MinimumOpacity = 0.25f;
	public float MaximumOpacity = 1.0f;
	public float speed = 1.0f;
	private bool flash = true;
	private bool start = false;
	private SpriteRenderer mySprite;

	// Use this for initialization
	void Start () 
	{
		mySprite = GetComponent<SpriteRenderer> ();
	}

	
	// Update is called once per frame
	void Update () 
	{
		if (gameObject.activeSelf && !start) 
		{
			start = true;
			StartCoroutine (FlashRoutine(speed, mySprite));
		}
	}

	private IEnumerator FlashRoutine(float speed, SpriteRenderer rend)
	{
		float timer = speed;
		bool fadeUp = false;

		Color min = new Color (rend.color.r, rend.color.g, rend.color.b, MinimumOpacity);
		Color max = new Color (rend.color.r, rend.color.g, rend.color.b, MaximumOpacity);

		while(flash)
		{
			if (!fadeUp) 
			{
				timer -= Time.deltaTime;
				rend.color = Color.Lerp (min, max, timer / speed);
				if (timer < 0) 
				{
					fadeUp = true;
				}
			}
			else 
			{
				timer += Time.deltaTime;
				rend.color = Color.Lerp (min, max, timer / speed);
				if (timer > speed) 
				{
					fadeUp = false;
				}
			}
			yield return null;
		}
	}
}
