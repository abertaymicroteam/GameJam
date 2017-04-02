using UnityEngine;
using System.Collections;

// Auto-destroys particle system once complete
public class AutoDestroy : MonoBehaviour {

	// Component References
	private ParticleSystem pSystem;

	// Use this for initialization
	void Start () 
	{
		pSystem = GetComponentInChildren<ParticleSystem> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (pSystem)
		{
			if(!pSystem.IsAlive())
			{
				Destroy (gameObject);
			}
		}
	}
}
