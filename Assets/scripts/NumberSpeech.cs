﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberSpeech : MonoBehaviour {

    public AudioClip[] numbers0Through19Clips; // 0-19
    public AudioClip[] multiplesOf10From20To90Clips; // x0's
    public AudioClip youHaveClip; // youhave
    public AudioClip pointsClip; // points
    public AudioClip byClip; // by
    public AudioClip footClip; // foot
    public AudioClip penClip; // pen
    public AudioClip inchesClip; // inches
    public AudioClip pinchClip; // pinches
    public AudioClip armClip; // arm
	public AudioClip yourFinalScoreWasClip;

    public static NumberSpeech Instance;

    public void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }


    public IEnumerator PlayNumbersAudio(int number)
    {
		Debug.Log (number);
        // We have audio clips for up to 19 because these are unique numbers
        if (number <= 19)
        {
            AudioManager.Instance.PlayNarration(numbers0Through19Clips[number], AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(numbers0Through19Clips[number].length);
        }
        // We have to do a bit of fancy manipulation now
        else
        {
            int firstDigit = Mathf.FloorToInt(number / 10);
            AudioManager.Instance.PlayNarration(multiplesOf10From20To90Clips[firstDigit - 2], AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(multiplesOf10From20To90Clips[firstDigit - 2].length);
            // We don't say something extra if it's not a multiple of ten, so let's see if it was before saying something
            int secondDigit = Mathf.FloorToInt(number % 10);
            if (secondDigit != 0)
            {
                AudioManager.Instance.PlayNarration(numbers0Through19Clips[secondDigit], AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
                yield return new WaitForSeconds(numbers0Through19Clips[secondDigit].length);
            }
        }
    }

    /// <summary>
    /// Plays audio number in a range of 0 - 99.
    /// Ex: "You Have 84 points"
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    /// 
    public IEnumerator PlayExpPointsAudio(int points)
    {
        if (points < 99)
        {
            AudioManager.Instance.PlayNarration(youHaveClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(youHaveClip.length);
			yield return PlayNumbersAudio(points);
            AudioManager.Instance.PlayNarration(pointsClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(pointsClip.length);
        }

        
    }

	public IEnumerator PlayFinalExpPointsAudio(int points){
		if (points < 99)
		{
			AudioManager.Instance.PlayNarration(yourFinalScoreWasClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
			yield return new WaitForSeconds(yourFinalScoreWasClip.length);
			yield return PlayNumbersAudio(points);
			AudioManager.Instance.PlayNarration(pointsClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
			yield return new WaitForSeconds(pointsClip.length);
		}
	}

    /// <summary>
    /// Reads numbers, for Exp mode correction hints in an equal distrobution between an analogy
    /// and an exact measurement.
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public IEnumerator PlayFancyNumberAudio(int num)
    {
        AudioManager.Instance.PlayNarration(byClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        yield return new WaitForSeconds(byClip.length);

        int inchNum = (int)(num / 2.54);
        if (inchNum >= 11 && inchNum <= 13)
        {
            AudioManager.Instance.PlayNarration(footClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(footClip.length);
        }
        else if (inchNum >= 18)
        {
            AudioManager.Instance.PlayNarration(armClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(armClip.length);
        }
        else if (inchNum > 4 && inchNum < 8)
        {
            AudioManager.Instance.PlayNarration(penClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(penClip.length);
        }
        else if (inchNum < 2)
        {
            AudioManager.Instance.PlayNarration(pinchClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(pinchClip.length);
        }

        yield return PlayNumbersAudio(inchNum);

        AudioManager.Instance.PlayNarration(inchesClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        yield return new WaitForSeconds(inchesClip.length);

    }
}
