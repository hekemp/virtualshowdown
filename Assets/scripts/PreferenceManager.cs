using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Handedness
{
	Left,
	Right,
}

public enum NarratorVoice
{
	MaleVoice,
}

public enum NarratorTalkativeness
{
	Silent,
	Verbose,
}

public class PreferenceManager : MonoBehaviour
{
	public static PreferenceManager Instance;

	/// <summary>
	/// If the player is left- or right-handed
	/// </summary>
	public Handedness PlayerHandedness;
	
	/// <summary>
	/// If a Switch controller is connected, this activates rumble
	/// </summary>
	public bool ControllerRumble;
	
	/// <summary>
	/// Which style of narrator to play
	/// </summary>
	public NarratorVoice NarratorVoice;

	/// <summary>
	/// How talkative the narrator is
	/// </summary>
	public NarratorTalkativeness NarratorTalkativeness;

	/// <summary>
	/// Whether or not UI text should be read aloud
	/// </summary>
	public bool TooltipNarration;

	// Use this for initialization
	void Start () {
		// We only ever want 1 copy of this game object!
		if (Instance != null)
		{
			Destroy(this);
			return;
		}
		
		// We want preferences to persist throughout the menus
		DontDestroyOnLoad(this);
		
		// TODO: Load preferences from file

		Instance = this;
	}
}
