using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuSetup : MonoBehaviour
{
	public KinectAudioParser AudioParser;
	public Button ShowdownDrillBtn;
	public Button ShowdownBtn;
	public Button PreferencesBtn;

	// Use this for initialization
	void Start () {
		
		ShowdownDrillBtn.onClick.AddListener(StartDrillMode);
		ShowdownBtn.onClick.AddListener(StartShowdown);
		PreferencesBtn.onClick.AddListener(StartPreferencesMenu);

        // TODO: play welcome music
        // TODO: play welcome audio
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void StartDrillMode()
	{
		Debug.Log("Going to drill mode");
		SceneManager.LoadScene("ShowdownDrill");
	}

	public void StartShowdown()
	{
		Debug.Log("Going to showdown mode");
		SceneManager.LoadScene("Showdown");
	}

	public void StartPreferencesMenu()
	{
		Debug.Log("Going to preference mode");
		SceneManager.LoadScene("Menu_Setup");
	}

}
