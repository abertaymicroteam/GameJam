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
	
	void Update ()
	{
		Destroy (this.gameObject, 2f);	
	}
}
