using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberSpeech : MonoBehaviour {

    public AudioClip[] numbers0Through19Clips; // 0-19
    public AudioClip tiedClip; // tied
    public AudioClip youreUpClip; // youup1
    public AudioClip opponentUpClip; // oppup1
    public AudioClip scoreIsClip; // scoreis
    public AudioClip toClip; // to
    public AudioClip yourServeClip; // yourserve
    public AudioClip opponentServeClip; // oppserve
    public AudioClip readyGoClip; //readygo
    public AudioClip nextBallClip; // nextball
    public AudioClip congratsClip; // congrats
    public AudioClip lostClip; // lost
    public AudioClip thanksClip; // thanks
    public AudioClip welcomeClip; // welcome
    public AudioClip welcomeMusicClip; // welcomemus
    public AudioClip[] multiplesOf10From20To90Clips; // x0's
    public AudioClip youHaveClip; // youhave
    public AudioClip pointsClip; // points
    public AudioClip byClip; // by
    public AudioClip footClip; // foot
    public AudioClip penClip; // pen
    public AudioClip inchesClip; // inches
    public AudioClip pinchClip; // pinches
    public AudioClip armClip; // arm

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


    public void PlayNumbersAudio(int number)
    {
        // We have audio clips for up to 19 because these are unique numbers
        if (number <= 19)
        {
            AudioManager.Instance.PlayNarration(numbers0Through19Clips[number], AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        }
        // We have to do a bit of fancy manipulation now
        else
        {
            int firstDigit = Mathf.FloorToInt(number / 10);
            AudioManager.Instance.PlayNarration(multiplesOf10From20To90Clips[firstDigit - 2], AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            // We don't say something extra if it's not a multiple of ten, so let's see if it was before saying something
            int secondDigit = Mathf.FloorToInt(number % 10);
            if (secondDigit != 0)
            {
                AudioManager.Instance.PlayNarration(numbers0Through19Clips[secondDigit], AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
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
    public void PlayExpPointsAudio(int points)
    {
        if (points > 99)
        {
            return;
        }

        AudioManager.Instance.PlayNarration(youHaveClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        PlayNumbersAudio(points);
        AudioManager.Instance.PlayNarration(pointsClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
    }

    /// <summary>
    /// Reads numbers, for Exp mode correction hints in an equal distrobution between an analogy
    /// and an exact measurement.
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public void PlayFancyNumberAudio(int num)
    {
        AudioManager.Instance.PlayNarration(byClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        int inchNum = (int)(num / 2.54);
        if (inchNum >= 11 && inchNum <= 13)
        {
            AudioManager.Instance.PlayNarration(footClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        }
        else if (inchNum >= 18)
        {
            AudioManager.Instance.PlayNarration(armClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        }
        else if (inchNum > 4 && inchNum < 8)
        {
            AudioManager.Instance.PlayNarration(penClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        }
        else if (inchNum < 2)
        {
            AudioManager.Instance.PlayNarration(pinchClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        }

        PlayNumbersAudio(inchNum);

        AudioManager.Instance.PlayNarration(inchesClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

    }
}
