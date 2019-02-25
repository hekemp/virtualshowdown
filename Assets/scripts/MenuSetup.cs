using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSetup : MonoBehaviour
{
    public KinectAudioParser AudioParser;
    public  List<MenuSetupSection> Sections;
	private int _activeSection = 0;

	// Use this for initialization
	void Start () {
		foreach (var section in Sections)
		{
			section.OnSelectionMade.AddListener(GoToNextSection);
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
			section.gameObject.SetActive(false);
		}
		Sections[_activeSection].gameObject.SetActive(true);
		Sections[_activeSection].OnQuestionShown();
	}

	void GoToNextSection()
	{
		_activeSection += 1;
		if (_activeSection == Sections.Count)
		{
			// TODO: Exit setup
			_activeSection = 0;
		}
		SetSections();
	}
}
