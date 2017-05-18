using UnityEngine;
using System.Collections;

public class PersonScript : MonoBehaviour {

	private SpriteRenderer myRenderer;
	private PlayerScript ringScript;
	public int numWins = 0;

	// Use this for initialization
	void Start () 
	{
		myRenderer = gameObject.GetComponent<SpriteRenderer> ();
		ringScript = gameObject.GetComponentInParent<PlayerScript> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Tether sprite renderer to parent
		myRenderer.enabled = !ringScript.destroyMe;
	}
}
