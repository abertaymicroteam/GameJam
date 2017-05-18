/*using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]

public class PlayMovie : MonoBehaviour {

	public float timer;
	public MovieTexture movie;
	private AudioSource movAudio;

	// Use this for initialization
	void Start () {
		GetComponent<RawImage> ().texture = movie as MovieTexture;
		movAudio = GetComponent<AudioSource>();
		movAudio.clip = movie.audioClip;
		movie.Play ();
		movAudio.Play ();
	}
	
	// Update is called once per frame
	void Update () {
		timer -= Time.deltaTime;
		if (timer < 0) {
			RawImage.Destroy(gameObject);
		}
	}
}*/
