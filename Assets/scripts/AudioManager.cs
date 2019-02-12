using System.Collections;
using System.Collections.Generic;
using Helper;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	public AudioSource[] SfxAudioSources;
	public AudioSource NarratorAudioSource;

	private readonly Queue<AudioClip> _narratorQueue = new Queue<AudioClip>();
	private readonly Queue<AudioClip> _sfxQueue = new Queue<AudioClip>();

	// Use this for initialization
	void Start () {
		// We only want one audio manager at a time
		if (Instance != null)
		{
			Destroy(this);
			return;
		}

		foreach (var src in SfxAudioSources)
		{
			src.loop = false;
		}
		NarratorAudioSource.loop = false;
		
		Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		// If we have sfx that need playing and an audio source that's not busy, play it!
		foreach (var src in SfxAudioSources)
		{
			if (!src.isPlaying && _sfxQueue.Count > 0)
			{
				src.clip = _sfxQueue.Dequeue();
				src.Play();
			}
		}

		// If we have narration that needs playing and the audio source isn't busy, play it!
		if (!NarratorAudioSource.isPlaying && _narratorQueue.Count > 0)
		{
			NarratorAudioSource.clip = _narratorQueue.Dequeue();
			NarratorAudioSource.Play();
		}
	}

	public void PlaySfx(AudioClip clip)
	{
		_sfxQueue.Enqueue(clip);
	}

	public void StopAllSfx()
	{
		_sfxQueue.Clear();
		foreach (var src in SfxAudioSources)
		{
			src.Stop();
		}
	}
	
	/// <summary>
	/// Queues a narration audio clip
	/// </summary>
	/// <param name="clip">The clip to enqueue</param>
	public void PlayNarration(AudioClip clip)
	{
		_narratorQueue.Enqueue(clip);
	}

	/// <summary>
	/// Stops the current narration, and immediately plays a new audio clip. Should only be used
	/// for very important bits of narration, such as game over.
	/// </summary>
	/// <param name="clip">The clip to play</param>
	public void PlayNarrationImmediate(AudioClip clip)
	{
		_narratorQueue.Clear();
		NarratorAudioSource.Stop();
		NarratorAudioSource.clip = clip;
		NarratorAudioSource.Play();
	}

	public void StopAllNarration()
	{
		_narratorQueue.Clear();
		NarratorAudioSource.Stop();
	}
}
