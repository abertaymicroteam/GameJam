using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

	public GameObject[] characterSound;
	private AudioClip characterEffect;

	void Start ()
	{
		characterEffect = characterSound[Random.Range(0,14)].GetComponent<AudioSource>().clip;
		gameObject.GetComponent<AudioSource>().clip = characterEffect;

		gameObject.GetComponent<AudioSource>().Play();
	}
	
	void Update ()
	{
		Destroy (this.gameObject, 2f);	
	}
}
