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
    private Queue<AudioSettings> _sfxSettingsQueue = new Queue<AudioSettings>();
    private Queue<AudioSettings> _narratorSettingsQueue = new Queue<AudioSettings>();

    public class AudioSettings
    {
        public bool mute;
        public bool bypassEffects;
        public bool bypassListenerEffect;
        public bool bypassReverbZone;
        public bool playOnAwake;
        public bool loop;
        public int priority;
        public float volume;
        public float pitch;
        public float stereoPan;
        public float spatialBlend;
        public float reverbZone;
        public Vector3 location;

        public AudioSettings(bool mute, bool bypassEffects, bool bypassListenerEffect, bool bypassReverbZone, bool playOnAwake, bool loop, int priority, float volume, float pitch, float stereoPan, float spatialBlend, float reverbZone, Vector3 location)
        {
            this.mute = mute;
            this.bypassEffects = bypassEffects;
            this.bypassListenerEffect = bypassListenerEffect;
            this.bypassReverbZone = bypassReverbZone;
            this.playOnAwake = playOnAwake;
            this.loop = loop;
            this.priority = priority;
            this.volume = volume;
            this.pitch = pitch;
            this.stereoPan = stereoPan;
            this.spatialBlend = spatialBlend;
            this.reverbZone = reverbZone;
            this.location = location;
        }
    }

    public enum AudioLocation
    {
        End1,
        End2,
        End3,
        End4,
        End5,
        Start1,
        Start2,
        Start3,
        Guideline,
        Default,
    }

    public GameObject End1;
    public GameObject End2;
    public GameObject End3;
    public GameObject End4;
    public GameObject End5;
    public GameObject Start1;
    public GameObject Start2;
    public GameObject Start3;
    public GameObject Guideline;

    public Dictionary<AudioLocation, AudioSettings> locationSettings = new Dictionary<AudioLocation, AudioSettings>();

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

        AudioSettings start1Settings = new AudioSettings(false, false, false, false, false, false, 128, 1.0f, 1, -1, 1, 1, Start1.transform.position);
        AudioSettings start2Settings = new AudioSettings(false, false, false, false, false, false, 128, 1.0f, 1, 0, 1, 1, Start2.transform.position);
        AudioSettings start3Settings = new AudioSettings(false, false, false, false, false, false, 128, 1.0f, 1, 1, 1, 1, Start3.transform.position);

        AudioSettings end1Settings = new AudioSettings(false, false, false, false, false, false, 128, 1.0f, 1, -1, 1, 1, End1.transform.position);
        AudioSettings end2Settings = new AudioSettings(false, false, false, false, false, false, 128, 1.0f, 1, -.5f, 1, 1, End2.transform.position);
        AudioSettings end3Settings = new AudioSettings(false, false, false, false, false, false, 128, 1.0f, 1, 0, 1, 1, End3.transform.position);
        AudioSettings end4Settings = new AudioSettings(false, false, false, false, false, false, 128, 1.0f, 1, .5f, 1, 1, End4.transform.position);
        AudioSettings end5Settings = new AudioSettings(false, false, false, false, false, false, 128, 1.0f, 1, 1, 1, 1, End5.transform.position);

        AudioSettings guidelinesSettings = new AudioSettings(false, false, false, false, false, false, 128, 1, 1, 0, 1, 1, Guideline.transform.position);

        AudioSettings defaultSettings = new AudioSettings(false, false, false, false, false, false, 128, 1.0f, 1, 0, 0, 1, Vector3.zero);

        locationSettings.Add(AudioLocation.Start1, start1Settings);
        locationSettings.Add(AudioLocation.Start2, start2Settings);
        locationSettings.Add(AudioLocation.Start3, start3Settings);
        locationSettings.Add(AudioLocation.End1, end1Settings);
        locationSettings.Add(AudioLocation.End2, end2Settings);
        locationSettings.Add(AudioLocation.End3, end3Settings);
        locationSettings.Add(AudioLocation.End4, end4Settings);
        locationSettings.Add(AudioLocation.End5, end5Settings);
        locationSettings.Add(AudioLocation.Guideline, guidelinesSettings);
        locationSettings.Add(AudioLocation.Default, defaultSettings);
    }
	
	// Update is called once per frame
	void Update () {
		// If we have sfx that need playing and an audio source that's not busy, play it!
		foreach (var src in SfxAudioSources)
		{
			if (!src.isPlaying && _sfxQueue.Count > 0)
			{
				src.clip = _sfxQueue.Dequeue();
                AudioSettings clipSettings = _sfxSettingsQueue.Dequeue();
                setSettingsForSource(src, clipSettings);
                src.Play();
			}
		}

		// If we have narration that needs playing and the audio source isn't busy, play it!
		if (!NarratorAudioSource.isPlaying && _narratorQueue.Count > 0)
		{
			NarratorAudioSource.clip = _narratorQueue.Dequeue();
            AudioSettings clipSettings = _narratorSettingsQueue.Dequeue();
            setSettingsForSource(NarratorAudioSource, clipSettings);
            NarratorAudioSource.Play();
		}
	}

	public void PlaySfx(AudioClip clip, AudioSettings clipSettings)
	{
		_sfxQueue.Enqueue(clip);
        _sfxSettingsQueue.Enqueue(clipSettings);
	}

	public void StopAllSfx()
	{
		_sfxQueue.Clear();
        _sfxSettingsQueue.Clear();
		foreach (var src in SfxAudioSources)
		{
			src.Stop();
		}
	}
	
	/// <summary>
	/// Queues a narration audio clip
	/// </summary>
	/// <param name="clip">The clip to enqueue</param>
	public void PlayNarration(AudioClip clip, AudioSettings clipSettings)
	{
        Debug.Log("Speaking " + clip.name);
		_narratorQueue.Enqueue(clip);
        _narratorSettingsQueue.Enqueue(clipSettings);
	}

	/// <summary>
	/// Stops the current narration, and immediately plays a new audio clip. Should only be used
	/// for very important bits of narration, such as game over.
	/// </summary>
	/// <param name="clip">The clip to play</param>
	public void PlayNarrationImmediate(AudioClip clip, AudioSettings clipSettings)
	{
		_narratorQueue.Clear();
        _narratorSettingsQueue.Clear();
		NarratorAudioSource.Stop();
		NarratorAudioSource.clip = clip;
        setSettingsForSource(NarratorAudioSource, clipSettings);
		NarratorAudioSource.Play();
	}

	public void StopAllNarration()
	{
        _narratorSettingsQueue.Clear();
        _narratorQueue.Clear();
		NarratorAudioSource.Stop();
	}

    public void setSettingsForSource(AudioSource source, AudioSettings clipSettings)
    {
        source.mute = clipSettings.mute;
        source.bypassEffects = clipSettings.bypassEffects;
        source.bypassListenerEffects = clipSettings.bypassListenerEffect;
        source.bypassReverbZones = clipSettings.bypassReverbZone;
        source.playOnAwake = clipSettings.playOnAwake;
        source.loop = clipSettings.loop;
        source.priority = clipSettings.priority;
        source.volume = clipSettings.volume;
        source.pitch = clipSettings.pitch;
        source.panStereo = clipSettings.stereoPan;
        source.spatialBlend = clipSettings.spatialBlend;
        source.reverbZoneMix = clipSettings.reverbZone;
        source.transform.position = clipSettings.location;
    }
}
