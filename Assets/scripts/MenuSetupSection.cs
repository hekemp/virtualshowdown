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
					lefthandBtn.onClick.AddListener(() =>
						{
							PreferenceManager.Instance.PlayerHandedness = Handedness.Left;
							Finish();
						});
					var righthandBtn = Buttons[1].GetComponent<Button>();
					righthandBtn.onClick.AddListener(() =>
						{
							PreferenceManager.Instance.PlayerHandedness = Handedness.Right;
							Finish();
						});
					break;
				case MenuSetupSectionType.ControllerRumble:
					var rumbleButton = Buttons[0].GetComponent<Button>();
					rumbleButton.onClick.AddListener(() =>
					{
						PreferenceManager.Instance.ControllerRumble = true;
						Finish();
					});
					var noRumbleButton = Buttons[1].GetComponent<Button>();
					noRumbleButton.onClick.AddListener(() =>
					{
						PreferenceManager.Instance.ControllerRumble = false;
						Finish();
					});
					break;
				case MenuSetupSectionType.NarratorVoice:
					var maleButton = Buttons[0].GetComponent<Button>();
					maleButton.onClick.AddListener(() =>
					{
						PreferenceManager.Instance.NarratorVoice = NarratorVoice.MaleVoice;
						Finish();
					});
					var femaleButton = Buttons[1].GetComponent<Button>();
					femaleButton.onClick.AddListener(() =>
					{
						// TODO: Female voice
						PreferenceManager.Instance.NarratorVoice = NarratorVoice.MaleVoice;
						Finish();
					});
					break;
				case MenuSetupSectionType.NarratorTalkativeness:
					var silentButton = Buttons[0].GetComponent<Button>();
					silentButton.onClick.AddListener(() =>
					{
						PreferenceManager.Instance.NarratorTalkativeness = NarratorTalkativeness.Silent;
						Finish();
					});
					var verboseButton = Buttons[1].GetComponent<Button>();
					verboseButton.onClick.AddListener(() =>
					{
						PreferenceManager.Instance.NarratorTalkativeness = NarratorTalkativeness.Verbose;
						Finish();
					});
					break;
				case MenuSetupSectionType.TooltipNarration:
					var tooltipOnButton = Buttons[0].GetComponent<Button>();
					tooltipOnButton.onClick.AddListener(() =>
					{
						PreferenceManager.Instance.TooltipNarration = true;
						Finish();
					});
					var tooltipOffButton = Buttons[1].GetComponent<Button>();
					tooltipOffButton.onClick.AddListener(() =>
					{
						PreferenceManager.Instance.TooltipNarration = false;
						Finish();
					});
					break;
		}
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
