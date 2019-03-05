using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameAudioParserManager : MonoBehaviour {

	public KinectAudioParser AudioParser;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void onPauseGame()
	{
		// TODO: Implement pause game
	}

	public void onUnPauseGame()
	{
		// TODO: Implement unpause game
	}

	public void onLeaveGameMode()
	{
		SceneManager.LoadScene("Main_Menu");
	}
}
