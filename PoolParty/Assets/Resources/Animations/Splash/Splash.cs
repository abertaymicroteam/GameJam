using UnityEngine;
using System.Collections;

public class Splash : MonoBehaviour {

	public GameObject[] waterSound;
	private AudioClip soundEffect;

	void Start ()
	{
		/*soundEffect = waterSound[Random.Range(0,3)].GetComponent<AudioSource>().clip;
		gameObject.GetComponent<AudioSource>().clip = soundEffect;

		gameObject.GetComponent<AudioSource>().Play();*/
	}
	
<<<<<<< HEAD:PoolParty/Assets/Resources/Animations/Splash/Splash.cs
	// Update is called once per frame
	void Update () 
	{		
		Destroy (this.gameObject, 1f);	
=======
	void Update ()
	{
		Destroy (this.gameObject, 2f);	
>>>>>>> f05b9d4602cbcb95469de67bff5a0c8d8a18d87f:PoolParty/Assets/Animations/Splash/Splash.cs
	}
}
