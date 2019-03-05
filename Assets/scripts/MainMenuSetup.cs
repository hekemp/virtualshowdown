using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuSetup : MonoBehaviour
{
	public KinectAudioParser AudioParser;
	public Button DrillModeBtn;
	public Button ShowdownPracticeBtn;
	public Button PreferencesBtn;

	// Use this for initialization
	void Start () {
		
		DrillModeBtn.onClick.AddListener(StartDrillMode);
		ShowdownPracticeBtn.onClick.AddListener(StartShowdownPractice);
		PreferencesBtn.onClick.AddListener(StartPreferencesMenu);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void StartDrillMode()
	{
		Debug.Log("Going to drill mode");
		SceneManager.LoadScene("DrillMode");
	}

	public void StartShowdownPractice()
	{
		Debug.Log("Going to showdown mode");
		SceneManager.LoadScene("ShowdownPractice");
	}

	public void StartPreferencesMenu()
	{
		Debug.Log("Going to preference mode");
		SceneManager.LoadScene("MenuSetup");
	}

}
