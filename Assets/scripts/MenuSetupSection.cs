using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum MenuSetupSectionType
{
	Handedness,
	ControllerRumble,
	NarratorVoice,
	NarratorTalkativeness,
	TooltipNarration,
}

public class MenuSetupSection : MonoBehaviour
{
	public GameObject[] Buttons;
	public MenuSetupSectionType Section;
	public UnityEvent OnSelectionMade = new UnityEvent();

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

    public void OnDefaultSelected()
    {
        Finish();
    }

    public void OnRepeatSelected()
    {
        OnQuestionShown();
    }

    public void OnQuestionShown()
	{
		// TODO: Narrate question
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
		
	}
}
