using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuSetup : MonoBehaviour
{
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
	}

	public void GoToNextSection()
	{
		_activeSection += 1;
        if (_activeSection >= Sections.Count)
        {
            SceneManager.LoadScene("Main_Menu");
            _activeSection = 0;
            return;
        }
        // If the next section is for the controller rumble but we don't have a controller attatched, we can skip the rumble setting
        if ((Sections[_activeSection].Section == MenuSetupSectionType.ControllerRumble) && (!JoyconController.CheckJoyconAvail()))
		{
            AudioManager.Instance.PlayNarrationImmediate(joyconSkipAudioClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

            _activeSection += 1;
			PreferenceManager.Instance.ControllerRumble = false;
		}
		SetSections();
	}

    public void OnRepeatSelected()
    {
        Sections[_activeSection].OnQuestionShown();
    }
}
