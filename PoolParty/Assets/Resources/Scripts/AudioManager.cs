using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {
	// Holds Audio Clips and functions to play them

	// List holds currently playing audio sources
	private List<AudioSource> Playing = new List<AudioSource>();

	// Clip Prefabs
	public AudioClip gameStart;
	public AudioClip countdownDrop;
	public AudioClip hit;
	public AudioClip boing;
	public AudioClip death;
	public AudioClip bell;
	public AudioClip win;
	public AudioClip stretch;
	public AudioClip showdown;
    public AudioClip bombExplode;
    public AudioClip chargeRotor;
    public AudioClip jugHit;
    public AudioClip jugActivate;
    public AudioClip bubbles;
    public AudioClip beep;

    // Use this for initialization
    void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Check if playing audio sources have finished.
		AudioSource toRemove = new AudioSource();
		foreach(AudioSource src in Playing)
		{
			if (src.isPlaying == false)
			{
				toRemove = src;
			}
		}

		// Remove finished audio source
		if (toRemove != null)
		{
			Playing.Remove(toRemove);
			Destroy (toRemove);
		}
	}

	public void PlayGameStart()
	{
		AudioSource newSrc = gameObject.AddComponent<AudioSource>();
		newSrc.clip = gameStart;
		newSrc.loop = false;
		newSrc.Play ();
		Playing.Add (newSrc);
	}

	public void PlayDrop()
	{
		AudioSource newSrc = gameObject.AddComponent<AudioSource>();
		newSrc.clip = countdownDrop;
		newSrc.loop = false;
		newSrc.Play ();
		Playing.Add (newSrc);
	}

	public void PlayHit()
	{
		AudioSource newSrc = gameObject.AddComponent<AudioSource>();
		AudioSource newSrc1 = gameObject.AddComponent<AudioSource>();
		newSrc.clip = hit;
		newSrc1.clip = boing;
		newSrc.loop = false;
		newSrc1.loop = false;
		newSrc.Play ();
		newSrc1.Play ();
		Playing.Add (newSrc);
		Playing.Add (newSrc1);
	}

	public void PlayDeath()
	{
		AudioSource newSrc = gameObject.AddComponent<AudioSource>();
		newSrc.clip = death;
		newSrc.loop = false;
		newSrc.Play ();
		Playing.Add (newSrc);
	}

	public void PlayBell()
	{
		AudioSource newSrc = gameObject.AddComponent<AudioSource>();
		newSrc.clip = bell;
		newSrc.loop = false;
		newSrc.Play ();
		Playing.Add (newSrc);
	}

	public void PlayWin()
	{
		AudioSource newSrc = gameObject.AddComponent<AudioSource>();
		newSrc.clip = win;
		newSrc.loop = false;
		newSrc.Play ();
		Playing.Add (newSrc);
	}

	public void PlayStretch()
	{
		AudioSource newSrc = gameObject.AddComponent<AudioSource>();
		newSrc.clip = stretch;
		newSrc.loop = false;
		newSrc.Play ();
		Playing.Add (newSrc);
	}

	public void PlayShowdown()
	{
		AudioSource newSrc = gameObject.AddComponent<AudioSource>();
		newSrc.clip = showdown;
		newSrc.loop = false;
		newSrc.Play ();
		Playing.Add (newSrc);
	}

    public void PlayBombExplode()
    {
        AudioSource newSrc = gameObject.AddComponent<AudioSource>();
        newSrc.clip = bombExplode;
        newSrc.loop = false;
        newSrc.Play();
        Playing.Add(newSrc);
    }

    public void PlayCharge()
    {
        AudioSource newSrc = gameObject.AddComponent<AudioSource>();
        newSrc.clip = chargeRotor;
        newSrc.loop = false;
        newSrc.Play();
        Playing.Add(newSrc);
    }

    public void PlayJugBounce()
    {
        AudioSource newSrc = gameObject.AddComponent<AudioSource>();
        newSrc.clip = jugHit;
        newSrc.loop = false;
        newSrc.Play();
        Playing.Add(newSrc);
    }

    public void PlayJugActivate()
    {
        AudioSource newSrc = gameObject.AddComponent<AudioSource>();
        newSrc.clip = jugActivate;
        newSrc.loop = false;
        newSrc.Play();
        Playing.Add(newSrc);
    }

    public void PlayBubbles()
    {
        AudioSource newSrc = gameObject.AddComponent<AudioSource>();
        newSrc.clip = bubbles;
        newSrc.loop = false;
        newSrc.Play();
        Playing.Add(newSrc);
    }

    public void PlayBeep()
    {
        AudioSource newSrc = gameObject.AddComponent<AudioSource>();
        newSrc.clip = beep;
        newSrc.loop = false;
        newSrc.Play();
        Playing.Add(newSrc);
    }
}
