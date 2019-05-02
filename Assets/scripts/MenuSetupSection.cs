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

	private Coroutine repeatCoroutine;
	private Coroutine reselectCorutine;

	public EventSystem es;

    public AudioClip narrationForQuestion;

	public AudioClip repeatedNarrationForQuestionClip;

	public MenuSetup.MenuType CurrentMenuType;

	public AudioClip firstOptionNarration;

	public AudioClip welcomeAudioClip;


    // Extra Array for Kinect Feedback Clips
    public AudioClip[] extraNarrationForQuestion;
	// For Kinect: 
    // 0: Player Found
    // 1: Player Lost
    // 2: Multiple Bodies Seen
    // 3: Ready Said while Multiple Bodies Seen
    // 4: Ready Said before Player Found
	// 5: troubles locating the player

	// For handedness confirmation
	// 0: currently left
	// 1: currently right
	// 2: keep the same
	// 3: are now left
	// 4: are now right

	// For opponent difficulty: 
	// 0: now easy
	// 1: now medium
	// 2: now hard

	// For Handedness:
	// 0: left
	// 1: right

	// For Vibration
	// 0: on
	// 1: off

    private AudioClip playerFoundClip;
    private AudioClip playerLostClip;
    private AudioClip multipleBodiesSeenClip;
    private AudioClip readySaidWhileMultipleBodiesSeen;
    private AudioClip readySaidWhilePlayerLost;
	private AudioClip troublesLocatingPlayerClip;

	private AudioClip youAreCurrentlyLeftyClip;
	private AudioClip youAreCurrentlyRightyClip;
	private AudioClip keepSettingsTheSameClip;
	private AudioClip areNowLeftyClip;
	private AudioClip areNowRightyClip;

	private AudioClip nowEasyClip;
	private AudioClip nowMediumClip;
	private AudioClip nowHardClip;

	private AudioClip vibrationsEnabledClip;
	private AudioClip vibrationsDisabledClip;

	public Text subtitle;
    

	// Use this for initialization
	void Start () {
		
		switch (Section)
		{
			case MenuSetupSectionType.Handedness:
				var lefthandBtn = Buttons[0].GetComponent<Button>();
				lefthandBtn.onClick.AddListener(OnLeftHandSelected);
				var righthandBtn = Buttons[1].GetComponent<Button>();
				righthandBtn.onClick.AddListener(OnRightHandSelected);
				areNowLeftyClip = extraNarrationForQuestion[0];
				areNowRightyClip = extraNarrationForQuestion[1];
                break;
			case MenuSetupSectionType.ControllerRumble:
				var rumbleButton = Buttons[0].GetComponent<Button>();
				rumbleButton.onClick.AddListener(OnVibrationOnSelected);
				var noRumbleButton = Buttons[1].GetComponent<Button>();
				noRumbleButton.onClick.AddListener(OnVibrationOffSelected);
				vibrationsEnabledClip = extraNarrationForQuestion[0];
				vibrationsDisabledClip = extraNarrationForQuestion[1];
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
				troublesLocatingPlayerClip = extraNarrationForQuestion[5];
				break;
		case MenuSetupSectionType.HandednessConfirmation:
				var confirmCorrectButton = Buttons [0].GetComponent<Button> ();
				confirmCorrectButton.onClick.AddListener (OnHandednessCorrectSelect);
				var denyButton = Buttons [1].GetComponent<Button> ();
				denyButton.onClick.AddListener (OnHandednessIncorrectSelect);
				youAreCurrentlyLeftyClip = extraNarrationForQuestion [0];
				youAreCurrentlyRightyClip = extraNarrationForQuestion [1];
				keepSettingsTheSameClip = extraNarrationForQuestion [2];
				areNowLeftyClip = extraNarrationForQuestion [3];
				areNowRightyClip = extraNarrationForQuestion [4];
				subtitle.text = "";
				Debug.Log (youAreCurrentlyRightyClip);
				if (PreferenceManager.Instance.PlayerHandedness == Handedness.Left) {
					subtitle.text = "Currently, you're set to be left-handed. Is this correct?";
					narrationForQuestion = youAreCurrentlyLeftyClip;
					repeatedNarrationForQuestionClip = youAreCurrentlyLeftyClip;
				} else {
					subtitle.text = "Currently, you're set to be right-handed. Is this correct?";
					narrationForQuestion = youAreCurrentlyRightyClip;
					repeatedNarrationForQuestionClip = youAreCurrentlyRightyClip;
				}

				break;
			case MenuSetupSectionType.OpponentDifficulty:
				var easyButton = Buttons[0].GetComponent<Button>();
				easyButton.onClick.AddListener(OnEasyDifficultySelect);
				var mediumButton = Buttons[1].GetComponent<Button>();
				mediumButton.onClick.AddListener(OnMediumDifficultySelect);
				var hardButton = Buttons[2].GetComponent<Button>();
				hardButton.onClick.AddListener(OnHardDifficultySelect);

				nowEasyClip = extraNarrationForQuestion[0];
				nowMediumClip = extraNarrationForQuestion[1];
				nowHardClip = extraNarrationForQuestion[2];
				break;
		}

	}

    public void OnLeftHandSelected()
    {
        if (!gameObject.activeInHierarchy)
            return;
        PreferenceManager.Instance.PlayerHandedness = Handedness.Left;

		AudioManager.Instance.PlayNarrationImmediate(areNowLeftyClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

		StartCoroutine (FinishAfterSeconds (areNowLeftyClip.length));
    }

    public void OnRightHandSelected()
    {
        if (!gameObject.activeInHierarchy)
            return;
        PreferenceManager.Instance.PlayerHandedness = Handedness.Right;
		AudioManager.Instance.PlayNarrationImmediate(areNowRightyClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

		StartCoroutine (FinishAfterSeconds (areNowRightyClip.length));
    }

    public void OnVibrationOnSelected()
    {
        if (!gameObject.activeInHierarchy)
            return;
        PreferenceManager.Instance.ControllerRumble = true;
		AudioManager.Instance.PlayNarrationImmediate(vibrationsEnabledClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

		StartCoroutine (FinishAfterSeconds (vibrationsEnabledClip.length));
    }

    public void OnVibrationOffSelected()
    {
        if (!gameObject.activeInHierarchy)
            return;
        PreferenceManager.Instance.ControllerRumble = false;
		AudioManager.Instance.PlayNarrationImmediate(vibrationsDisabledClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

		StartCoroutine (FinishAfterSeconds (vibrationsDisabledClip.length));
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
        /*if (!BodySourceManager.Instance.bodyFound)
		{
            AudioManager.Instance.PlayNarrationImmediate(readySaidWhilePlayerLost, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
			if (repeatCoroutine != null) {
				StopCoroutine (repeatCoroutine);
			}
			repeatCoroutine = StartCoroutine(checkTenSecondTimeLimit());
            return;
		}
        if(BodySourceManager.Instance.MultipleBodiesDetected)
        {
            AudioManager.Instance.PlayNarrationImmediate(readySaidWhileMultipleBodiesSeen, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
			if (repeatCoroutine != null) {
				StopCoroutine (repeatCoroutine);
			}
			repeatCoroutine = StartCoroutine(checkTenSecondTimeLimit());
			return;
        }*/

		Finish ();
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

		AudioManager.Instance.PlayNarrationImmediate(keepSettingsTheSameClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

		StartCoroutine (FinishAfterSeconds (keepSettingsTheSameClip.length));
	}

	public void OnHandednessIncorrectSelect(){
		if (MenuSetup.MenuType.PreDrillMode == CurrentMenuType) {
			ShowdownDrillManager.Instance.handleHandednessPrompt (false);
		}
		if (MenuSetup.MenuType.PreShowdownMode == CurrentMenuType) {
			ShowdownManager.Instance.handleHandednessPrompt (false);
		}

		AudioClip clipToPlay;

		if (PreferenceManager.Instance.PlayerHandedness == Handedness.Left) {
			clipToPlay = areNowLeftyClip;
		} else {
			clipToPlay = areNowRightyClip;
		}

		AudioManager.Instance.PlayNarrationImmediate(clipToPlay, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

		StartCoroutine (FinishAfterSeconds (clipToPlay.length));

	}

	IEnumerator FinishAfterSeconds(float s) {
		yield return new WaitForSeconds (s);
		Finish ();
	}



    public void OnQuestionShown()
	{
		es.SetSelectedGameObject(Buttons[0]);

		// TODO: investigate this
		// Due to a race condition of this being activiated before start, you have to set the audio here
		if (Section == MenuSetupSectionType.HandednessConfirmation) {
			youAreCurrentlyLeftyClip = extraNarrationForQuestion [0];
			youAreCurrentlyRightyClip = extraNarrationForQuestion [1];
			if (PreferenceManager.Instance.PlayerHandedness == Handedness.Left) {
				subtitle.text = "Currently, you're set to be left-handed. Is this correct?";
				narrationForQuestion = youAreCurrentlyLeftyClip;
				repeatedNarrationForQuestionClip = youAreCurrentlyLeftyClip;
			} else {
				subtitle.text = "Currently, you're set to be right-handed. Is this correct?";
				narrationForQuestion = youAreCurrentlyRightyClip;
				repeatedNarrationForQuestionClip = youAreCurrentlyRightyClip;
			}
		}

		Debug.Log (youAreCurrentlyRightyClip);
		Debug.Log (Section);
		if (welcomeAudioClip != null) {
			AudioManager.Instance.PlayNarrationImmediate (welcomeAudioClip, AudioManager.Instance.locationSettings [AudioManager.AudioLocation.Default]);

			Debug.Log (narrationForQuestion);
			if (narrationForQuestion != null) {
				
				AudioManager.Instance.PlayNarration (narrationForQuestion, AudioManager.Instance.locationSettings [AudioManager.AudioLocation.Default]);
			}
		} 
		else {
			if (narrationForQuestion != null) {
				AudioManager.Instance.PlayNarrationImmediate (narrationForQuestion, AudioManager.Instance.locationSettings [AudioManager.AudioLocation.Default]);
			}
		}

		if (firstOptionNarration != null) {
			AudioManager.Instance.PlayNarration(firstOptionNarration, AudioManager.Instance.locationSettings [AudioManager.AudioLocation.Default]);
		}
		if (repeatedNarrationForQuestionClip == null) {
			repeatedNarrationForQuestionClip = narrationForQuestion;
		}
		repeatCoroutine = StartCoroutine(checkTenSecondTimeLimit());

		//StartCoroutine(SelectAfterSeconds(narrationForQuestion.length));
	}

	private IEnumerator checkTenSecondTimeLimit()
	{
		if (repeatedNarrationForQuestionClip != null) {
			yield return new WaitForSeconds (repeatedNarrationForQuestionClip.length + 10);
			AudioManager.Instance.PlayNarration (repeatedNarrationForQuestionClip, AudioManager.Instance.locationSettings [AudioManager.AudioLocation.Default]);

			repeatCoroutine = StartCoroutine (checkTenSecondTimeLimit ());
		}
		yield return null;
	}

	/*private IEnumerator SelectAfterSeconds(float s){
		yield return new WaitForSeconds (s);
		if (Buttons [1] != null) {
			es.SetSelectedGameObject (Buttons [1]);
		}
		es.SetSelectedGameObject(Buttons[0]);
	}*/


	void Finish()
	{
		if (repeatCoroutine != null) {
			StopCoroutine(repeatCoroutine);
			repeatCoroutine = null;
		}
		foreach (var btn in Buttons)
		{
			var button = btn.GetComponent<Button>();
			button.onClick.RemoveAllListeners();
		}

		// stop the repeat coroutine

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
					repeatedNarrationForQuestionClip = playerFoundClip;
				}
				// Body was not announced but we now see the body. AKA need to announce we found them.
				else if (!bodyAnnounced && BodySourceManager.Instance.bodyFound)
				{
                    AudioManager.Instance.PlayNarrationImmediate(playerFoundClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

					if (repeatCoroutine != null) {
						StopCoroutine (repeatCoroutine);
					}
					repeatCoroutine = StartCoroutine(checkTenSecondTimeLimit());
					bodyAnnounced = true;
					repeatedNarrationForQuestionClip = playerFoundClip;
				}
				// Body was announced but now we don't see the body. AKA we need to announce that we lost sight of them
				else if (bodyAnnounced && !BodySourceManager.Instance.bodyFound)
				{

                    AudioManager.Instance.PlayNarrationImmediate(playerLostClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

					if (repeatCoroutine != null) {
						StopCoroutine (repeatCoroutine);
					}
					repeatCoroutine = StartCoroutine(checkTenSecondTimeLimit());
                    bodyAnnounced = false;
					repeatedNarrationForQuestionClip = troublesLocatingPlayerClip;
				}
				// Body was not announced but we don't see them yet. AKA we're waiting for them to get into position.
				else
				{
					// Nothing yet? 
					repeatedNarrationForQuestionClip = troublesLocatingPlayerClip;
				}

                // Multiple bodies was already announced and we still multiple bodies. AKA waiting for them to move.
                if (multipleBodiesAnnounced && BodySourceManager.Instance.MultipleBodiesDetected)
                {
                    // Nothing yet?
					repeatedNarrationForQuestionClip = multipleBodiesSeenClip;
                }
                // Multiple bodies was not announced and we see multiple bodies. Aka announce that there's an error here.
                else if (!multipleBodiesAnnounced && BodySourceManager.Instance.MultipleBodiesDetected)
                {
                    AudioManager.Instance.PlayNarrationImmediate(multipleBodiesSeenClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

					if (repeatCoroutine != null) {
						StopCoroutine (repeatCoroutine);
					}
					repeatCoroutine = StartCoroutine(checkTenSecondTimeLimit());
                    multipleBodiesAnnounced = true;
					repeatedNarrationForQuestionClip = multipleBodiesSeenClip;
                }
                // Multiple bodies were seen but now we only see one. AKA we need to announce that we're good to go
                else if (multipleBodiesAnnounced && !BodySourceManager.Instance.MultipleBodiesDetected)
                {
                    AudioManager.Instance.PlayNarrationImmediate(playerFoundClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

					if (repeatCoroutine != null) {
						StopCoroutine (repeatCoroutine);
					}
					repeatCoroutine = StartCoroutine(checkTenSecondTimeLimit());
                    multipleBodiesAnnounced = false;
					repeatedNarrationForQuestionClip = playerFoundClip;
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
