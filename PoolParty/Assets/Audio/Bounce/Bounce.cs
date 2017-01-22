using UnityEngine;
using System.Collections;

public class Bounce : MonoBehaviour {

	public GameObject[] bounceSound;
	private AudioClip bounceEffect;

	void Start ()
	{
		bounceEffect = bounceSound[Random.Range(0,3)].GetComponent<AudioSource>().clip;
		gameObject.GetComponent<AudioSource>().clip = bounceEffect;

		gameObject.GetComponent<AudioSource>().Play();
	}
	
	void Update ()
	{
		Destroy (this.gameObject, 1f);	
	}
}
