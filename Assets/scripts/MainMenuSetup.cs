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

    public AudioClip welcomeSFXClip;
    public AudioClip welcomeClip;

    public AudioClip readShowdownOptionClip;
    public AudioClip readDrillOptionClip;
    public AudioClip readPreferencesOptionClip;
    public AudioClip readExplainShowdownOptionClip;
    public AudioClip readExplainDrillOptionClip;
    public AudioClip readRepeatOptionClip;

    public AudioClip explainShowdownClip;
    public AudioClip explainDrillClip;

	// Use this for initialization
	void Start () {
		
		ShowdownDrillBtn.onClick.AddListener(StartDrillMode);
		ShowdownBtn.onClick.AddListener(StartShowdown);
		PreferencesBtn.onClick.AddListener(StartPreferencesMenu);

        StartCoroutine(playIntroductionNarration());
	}

    public void playOptionsAgain()
    {
        StartCoroutine(readAllOptions());
    }

    IEnumerator readAllOptions()
    {
        AudioManager.Instance.PlayNarration(readShowdownOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        AudioManager.Instance.PlayNarration(readDrillOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        AudioManager.Instance.PlayNarration(readPreferencesOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        AudioManager.Instance.PlayNarration(readExplainShowdownOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        AudioManager.Instance.PlayNarration(readExplainDrillOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        AudioManager.Instance.PlayNarration(readRepeatOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        yield return null;
    }

    IEnumerator playIntroductionNarration()
    {
        AudioManager.Instance.PlaySfx(welcomeSFXClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        AudioManager.Instance.PlayNarrationImmediate(welcomeClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        yield return new WaitForSeconds(Mathf.Max(welcomeSFXClip.length, welcomeClip.length));

        StartCoroutine(readAllOptions());

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

    public void explainShowdown()
    {

        AudioManager.Instance.PlayNarrationImmediate(explainShowdownClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

    }

    public void explainDrill()
    {
        AudioManager.Instance.PlayNarrationImmediate(explainDrillClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

    }

}
