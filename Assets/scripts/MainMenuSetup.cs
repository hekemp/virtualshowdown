using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class MainMenuSetup : MonoBehaviour
{
	public KinectAudioParser AudioParser;
	public Button ShowdownDrillBtn;
	public Button ShowdownBtn;
	public Button PreferencesBtn;
	public Button ExplainShowdownBtn;
	public Button ExplainDrillBtn;
	public Button QuitGameBtn;

    public AudioClip welcomeSFXClip;
    public AudioClip welcomeClip;
	public AudioClip gameModeIntroductionClip;
	public AudioClip firstButtonClip;

    /*public AudioClip readShowdownOptionClip;
    public AudioClip readDrillOptionClip;
    public AudioClip readPreferencesOptionClip;
    public AudioClip readExplainShowdownOptionClip;
    public AudioClip readExplainDrillOptionClip;
    public AudioClip readRepeatOptionClip;*/

    public AudioClip explainShowdownClip;
    public AudioClip explainDrillClip;

	public EventSystem es;

	// Use this for initialization
	void Start () {
		
		ShowdownDrillBtn.onClick.AddListener(StartDrillMode);
		ShowdownBtn.onClick.AddListener(StartShowdown);
		PreferencesBtn.onClick.AddListener(StartPreferencesMenu);
		ExplainShowdownBtn.onClick.AddListener (ExplainShowdown);
		ExplainDrillBtn.onClick.AddListener (ExplainDrill);
		QuitGameBtn.onClick.AddListener (QuitGame);

        StartCoroutine(playIntroductionNarration());
	}

    /*public void playOptionsAgain()
    {
        StartCoroutine(readAllOptions());
    }
*/
   /* IEnumerator readAllOptions()
    {
        AudioManager.Instance.PlayNarration(readShowdownOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        AudioManager.Instance.PlayNarration(readDrillOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        AudioManager.Instance.PlayNarration(readPreferencesOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        AudioManager.Instance.PlayNarration(readExplainShowdownOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        AudioManager.Instance.PlayNarration(readExplainDrillOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        AudioManager.Instance.PlayNarration(readRepeatOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        yield return null;
    }*/

    IEnumerator playIntroductionNarration()
    {
		// TODO: investigate this
		// Due to a timing issue, sometimes the button narration can play first. By delaying by a negliable amount we avoid this
		yield return new WaitForSeconds(.0001f);
        AudioManager.Instance.PlaySfx(welcomeSFXClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        AudioManager.Instance.PlayNarrationImmediate(welcomeClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
		yield return new WaitForSeconds (welcomeClip.length+1);
		AudioManager.Instance.PlayNarration(gameModeIntroductionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
		AudioManager.Instance.PlayNarration(firstButtonClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);



        //StartCoroutine(readAllOptions());

	}

    // Update is called once per frame
    void Update () {
		
	}

	public void StartDrillMode()
	{
		Debug.Log("Going to drill mode");
		SceneManager.LoadSceneAsync("ShowdownDrill");
	}

	public void StartShowdown()
	{
		Debug.Log("Going to showdown mode");
		SceneManager.LoadSceneAsync("Showdown");
	}

	public void StartPreferencesMenu()
	{
		Debug.Log("Going to preference mode");
		SceneManager.LoadSceneAsync("Menu_Setup");
	}

    public void ExplainShowdown()
    {

        AudioManager.Instance.PlayNarrationImmediate(explainShowdownClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

    }

    public void ExplainDrill()
    {
        AudioManager.Instance.PlayNarrationImmediate(explainDrillClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

    }

	public void QuitGame(){
        // TODO: quit game here
        Application.Quit();
    }

}
