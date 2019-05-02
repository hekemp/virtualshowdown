using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using UnityEngine.EventSystems;

public enum MenuSetupSectionType
{
	Handedness,
	ControllerRumble,
	NarratorVoice,
	NarratorTalkativeness,
	TooltipNarration,
	KinectCalibration,
	HandednessConfirmation,
	OpponentDifficulty,
}

public class MenuSetupSection : MonoBehaviour
{
	public GameObject[] Buttons;
	public MenuSetupSectionType Section;
	public UnityEvent OnSelectionMade = new UnityEvent();
	public bool bodyAnnounced = false;
    public bool multipleBodiesAnnounced = false;

	public EventSystem es;

    public AudioClip narrationForQuestion;

	public MenuSetup.MenuType CurrentMenuType;


    // Extra Array for Kinect Feedback Clips
    public AudioClip[] extraNarrationForQuestion;
    // 0: Player Found
    // 1: Player Lost
    // 2: Multiple Bodies Seen
    // 3: Ready Said while Multiple Bodies Seen
    // 4: Ready Said before Player Found

    private AudioClip playerFoundClip;
    private AudioClip playerLostClip;
    private AudioClip multipleBodiesSeenClip;
    private AudioClip readySaidWhileMultipleBodiesSeen;
    private AudioClip readySaidWhilePlayerLost;
    

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
                playerFoundClip = extraNarrationForQuestion[0];
                playerLostClip = extraNarrationForQuestion[1];
                multipleBodiesSeenClip = extraNarrationForQuestion[2];
                readySaidWhileMultipleBodiesSeen = extraNarrationForQuestion[3];
                readySaidWhilePlayerLost = extraNarrationForQuestion[4];

                Debug.Log("Looking for the player now...");
				break;
			case MenuSetupSectionType.HandednessConfirmation:
				var confirmCorrectButton = Buttons[0].GetComponent<Button>();
				confirmCorrectButton.onClick.AddListener(OnHandednessCorrectSelect);
				var denyButton = Buttons[1].GetComponent<Button>();
				denyButton.onClick.AddListener(OnHandednessIncorrectSelect);
				break;
			case MenuSetupSectionType.OpponentDifficulty:
				var easyButton = Buttons[0].GetComponent<Button>();
				easyButton.onClick.AddListener(OnEasyDifficultySelect);
				var mediumButton = Buttons[1].GetComponent<Button>();
				mediumButton.onClick.AddListener(OnMediumDifficultySelect);
				var hardButton = Buttons[0].GetComponent<Button>();
				hardButton.onClick.AddListener(OnHardDifficultySelect);
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

		// TODO: put this back, but hide it when testing menus w/o kinect plugged in
        if (!BodySourceManager.Instance.bodyFound)
		{
            AudioManager.Instance.PlayNarrationImmediate(readySaidWhilePlayerLost, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

            return;
		}
        if(BodySourceManager.Instance.MultipleBodiesDetected)
        {
            AudioManager.Instance.PlayNarrationImmediate(readySaidWhileMultipleBodiesSeen, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            return;
        }

		Finish();
	}

    public void OnDefaultSelected()
    {
        Finish();
    }

	public void OnEasyDifficultySelect(){
		if (MenuSetup.MenuType.PreShowdownMode == CurrentMenuType) {
			ShowdownManager.Instance.handleDifficultyPrompt (0);
		}

		Finish();	
	}

	public void OnMediumDifficultySelect(){
		if (MenuSetup.MenuType.PreShowdownMode == CurrentMenuType) {
			ShowdownManager.Instance.handleDifficultyPrompt (1);
		}

		Finish();	
	}

	public void OnHardDifficultySelect(){
		if (MenuSetup.MenuType.PreShowdownMode == CurrentMenuType) {
			ShowdownManager.Instance.handleDifficultyPrompt (2);
		}

		Finish();	
	}

	public void OnHandednessCorrectSelect(){
		if (MenuSetup.MenuType.PreDrillMode == CurrentMenuType) {
			ShowdownDrillManager.Instance.handleHandednessPrompt (true);
		}
		if (MenuSetup.MenuType.PreShowdownMode == CurrentMenuType) {
			ShowdownManager.Instance.handleHandednessPrompt (true);
		}

		Finish();
	}

	public void OnHandednessIncorrectSelect(){
		if (MenuSetup.MenuType.PreDrillMode == CurrentMenuType) {
			ShowdownDrillManager.Instance.handleHandednessPrompt (false);
		}
		if (MenuSetup.MenuType.PreShowdownMode == CurrentMenuType) {
			ShowdownManager.Instance.handleHandednessPrompt (false);
		}
		Finish();
	}



    public void OnQuestionShown()
	{
		es.SetSelectedGameObject(Buttons[0]);

        switch (Section)
		{
			case MenuSetupSectionType.Handedness:
                AudioManager.Instance.PlayNarrationImmediate(narrationForQuestion, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
				break;
			case MenuSetupSectionType.ControllerRumble:
                AudioManager.Instance.PlayNarrationImmediate(narrationForQuestion, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
				break;
			case MenuSetupSectionType.NarratorVoice:
                // Queue it up in this case so the missing joycon for controller rumble can play
                AudioManager.Instance.PlayNarrationImmediate(narrationForQuestion, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
				break;
			case MenuSetupSectionType.KinectCalibration:
                // Queue it up in this case so the missing joycon for controller rumble can play
                AudioManager.Instance.PlayNarrationImmediate(narrationForQuestion, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
				break;
			// TODO: add audio for new sections
		}

	}


	void Finish()
	{
		foreach (var btn in Buttons)
		{
			var button = btn.GetComponent<Button>();
			button.onClick.RemoveAllListeners();
		}

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
                    AudioManager.Instance.PlayNarrationImmediate(playerFoundClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

					bodyAnnounced = true;
				}
				// Body was announced but now we don't see the body. AKA we need to announce that we lost sight of them
				else if (bodyAnnounced && !BodySourceManager.Instance.bodyFound)
				{

                    AudioManager.Instance.PlayNarrationImmediate(playerLostClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

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
                    AudioManager.Instance.PlayNarrationImmediate(multipleBodiesSeenClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

                    multipleBodiesAnnounced = true;
                }
                // Multiple bodies were seen but now we only see one. AKA we need to announce that we're good to go
                else if (multipleBodiesAnnounced && !BodySourceManager.Instance.MultipleBodiesDetected)
                {
                    AudioManager.Instance.PlayNarrationImmediate(playerFoundClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

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
