using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public enum MenuSetupSectionType
{
	Handedness,
	ControllerRumble,
	NarratorVoice,
	NarratorTalkativeness,
	TooltipNarration,
	KinectCalibration,
}

public class MenuSetupSection : MonoBehaviour
{
	public GameObject[] Buttons;
	public MenuSetupSectionType Section;
	public UnityEvent OnSelectionMade = new UnityEvent();
	public bool bodyAnnounced = false;
    public bool multipleBodiesAnnounced = false;

	// Use this for initialization
	void Start () {
		switch (Section)
		{
				case MenuSetupSectionType.Handedness:
					var lefthandBtn = Buttons[0].GetComponent<Button>();
					lefthandBtn.onClick.AddListener(OnLeftHandSelected);
					var righthandBtn = Buttons[1].GetComponent<Button>();
					righthandBtn.onClick.AddListener(OnRightHandSelected);
                    break;
				case MenuSetupSectionType.ControllerRumble:
					var rumbleButton = Buttons[0].GetComponent<Button>();
					rumbleButton.onClick.AddListener(OnVibrationOnSelected);
					var noRumbleButton = Buttons[1].GetComponent<Button>();
					noRumbleButton.onClick.AddListener(OnVibrationOffSelected);
					break;
				case MenuSetupSectionType.NarratorVoice:
					var maleButton = Buttons[0].GetComponent<Button>();
					maleButton.onClick.AddListener(OnMaleNarratorSelected);
					var femaleButton = Buttons[1].GetComponent<Button>();
					femaleButton.onClick.AddListener(OnFemaleNarratorSelected);
					break;
				case MenuSetupSectionType.KinectCalibration:
					var confirmButton = Buttons[0].GetComponent<Button>();
					confirmButton.onClick.AddListener(onCalibrationConfirmation);
					// TODO: Announce that we are looking for the player
					Debug.Log("Looking for the player now...");
					break;
		}
	}

    public void OnLeftHandSelected()
    {
        if (!gameObject.activeInHierarchy)
            return;
        PreferenceManager.Instance.PlayerHandedness = Handedness.Left;
        Finish();
    }

    public void OnRightHandSelected()
    {
        if (!gameObject.activeInHierarchy)
            return;
        PreferenceManager.Instance.PlayerHandedness = Handedness.Right;
        Finish();
    }

    public void OnVibrationOnSelected()
    {
        if (!gameObject.activeInHierarchy)
            return;
        PreferenceManager.Instance.ControllerRumble = true;
        Finish();
    }

    public void OnVibrationOffSelected()
    {
        if (!gameObject.activeInHierarchy)
            return;
        PreferenceManager.Instance.ControllerRumble = false;
        Finish();
    }

    public void OnMaleNarratorSelected()
    {
        if (!gameObject.activeInHierarchy)
            return;
        PreferenceManager.Instance.NarratorVoice = NarratorVoice.MaleVoice;
        Finish();
    }

    public void OnFemaleNarratorSelected()
    {
        // TODO: Female voice
        if (!gameObject.activeInHierarchy)
            return;
        PreferenceManager.Instance.NarratorVoice = NarratorVoice.MaleVoice;
        Finish();
    }

	public void onCalibrationConfirmation()
	{
		if (!gameObject.activeInHierarchy)
			return;
        if (!BodySourceManager.Instance.bodyFound)
		{
			// TODO: Announce that we haven't seen the player yet
			Debug.Log("Please wait until we can see you.");
			return;
		}
        if(BodySourceManager.Instance.MultipleBodiesDetected)
        {
            // TODO: Announce that we need to see just 1 player
            Debug.Log("Please clear the play area so there is just 1 player.");
            return;
        }
		Finish();
	}

    public void OnDefaultSelected()
    {
        Finish();
    }



    public void OnQuestionShown()
	{
		// TODO: Narrate question
		switch (Section)
		{
			case MenuSetupSectionType.Handedness:
				Debug.Log("Looking for handedness now...");
				break;
			case MenuSetupSectionType.ControllerRumble:
				Debug.Log("Looking for controller rumble now...");
				break;
			case MenuSetupSectionType.NarratorVoice:
				Debug.Log("Looking for narrator voice now...");
				break;
			case MenuSetupSectionType.KinectCalibration:
				Debug.Log("Looking for calibration now...");
				break;
		}
	}

	void Finish()
	{
//		foreach (var btn in Buttons)
//		{
//			var button = btn.GetComponent<Button>();
//			button.onClick.RemoveAllListeners();
//		}
		OnSelectionMade.Invoke();
	}
	
	// Update is called once per frame
	void Update () {
		switch (Section)
		{
			case MenuSetupSectionType.Handedness:
				break;
			case MenuSetupSectionType.ControllerRumble:
				break;
			case MenuSetupSectionType.NarratorVoice:
				break;
			case MenuSetupSectionType.KinectCalibration:
				// Body was already announced and we still see the body. AKA waiting their OK.
				if (bodyAnnounced && BodySourceManager.Instance.bodyFound)
				{
					// Nothing yet?
				}
				// Body was not announced but we now see the body. AKA need to announce we found them.
				else if (!bodyAnnounced && BodySourceManager.Instance.bodyFound)
				{
					// TODO: Announce status
					Debug.Log("We found you!");
					bodyAnnounced = true;
				}
				// Body was announced but now we don't see the body. AKA we need to announce that we lost sight of them
				else if (bodyAnnounced && !BodySourceManager.Instance.bodyFound)
				{
					// TODO: Announce status
					Debug.Log("We lost you!");
					bodyAnnounced = false;
				}
				// Body was not announced but we don't see them yet. AKA we're waiting for them to get into position.
				else
				{
					// Nothing yet? 
				}

                // Multiple bodies was already announced and we still multiple bodies. AKA waiting for them to move.
                if (multipleBodiesAnnounced && BodySourceManager.Instance.MultipleBodiesDetected)
                {
                    // Nothing yet?
                }
                // Multiple bodies was not announced and we see multiple bodies. Aka announce that there's an error here.
                else if (!multipleBodiesAnnounced && BodySourceManager.Instance.MultipleBodiesDetected)
                {
                    // TODO: Announce status
                    Debug.Log("We see two bodies! Please clear the play area if you're not the player");
                    multipleBodiesAnnounced = true;
                }
                // Multiple bodies were seen but now we only see one. AKA we need to announce that we're good to go
                else if (multipleBodiesAnnounced && !BodySourceManager.Instance.MultipleBodiesDetected)
                {
                    // TODO: Announce status
                    Debug.Log("We can only see the player now! We're good to go!");
                    multipleBodiesAnnounced = false;
                }
                // Multiple bodies weren't seen and we don't see multiple bodies. Aka there's nothing for us to do here
                else
                {
                    // Nothing yet? 
                }

                break;
		}
	}
}
