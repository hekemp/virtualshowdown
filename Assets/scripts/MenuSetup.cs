﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuSetup : MonoBehaviour
{

	public enum MenuType
	{
		Preferences,
		PreDrillMode,
		PreShowdownMode,
	}

	public MenuType CurrentMenuType;
    public KinectAudioParser AudioParser;
    public  List<MenuSetupSection> Sections;
	private int _activeSection = 0;

    public AudioClip joyconSkipAudioClip;

	// Use this for initialization
	void Start () {
		foreach (var section in Sections)
		{
            if(section) {
                section.OnSelectionMade.AddListener(GoToNextSection);
            }
			
		}
		SetSections();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void SetSections()
	{
		foreach (var section in Sections)
		{
            // TODO: Add if null check
            if (section != null)
            {
                section.gameObject.SetActive(false);
            }
		}
        Debug.Log(_activeSection);
		Sections[_activeSection].gameObject.SetActive(true);
		Sections[_activeSection].OnQuestionShown();
		Sections[_activeSection].CurrentMenuType = CurrentMenuType;
	}

	public void GoToNextSection()
	{
		_activeSection += 1;

        Debug.Log(_activeSection);
        if (_activeSection >= Sections.Count)
        {
			Debug.Log (" final section");
			if (MenuType.Preferences == CurrentMenuType) {
				SceneManager.LoadScene ("Main_Menu");
			} 
			if (MenuType.PreDrillMode == CurrentMenuType) {
				this.gameObject.SetActive(false);
				ShowdownDrillManager.Instance.ConfirmOptions ();
			} 
			if (MenuType.PreShowdownMode == CurrentMenuType) {
				this.gameObject.SetActive(false);
				ShowdownManager.Instance.ConfirmOptions ();
			} 
            

			//_activeSection = 0;
			return;
			
        }
        // If the next section is for the controller rumble but we don't have a controller attatched, we can skip the rumble setting
        if ((Sections[_activeSection].Section == MenuSetupSectionType.ControllerRumble) && (!JoyconController.CheckJoyconAvail()))
		{
			StartCoroutine (handleNoController());
			return;
		}
		SetSections();
	}

	public IEnumerator handleNoController(){
		
		AudioManager.Instance.PlayNarrationImmediate(joyconSkipAudioClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

		_activeSection += 1;
		PreferenceManager.Instance.ControllerRumble = false;
		//this.gameObject.SetActive (false);
		//yield return new WaitForSeconds (joyconSkipAudioClip.length);
		if (MenuType.Preferences == CurrentMenuType) {
			//this.gameObject.SetActive(false);
			yield return new WaitForSeconds (joyconSkipAudioClip.length);
			SceneManager.LoadScene ("Main_Menu");
		} 
	}

    public void OnRepeatSelected()
    {
        Sections[_activeSection].OnQuestionShown();
    }
}
