using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowdownManager : MonoBehaviour {

    private TextMesh scoreText;
    private int playerScore;
    private int opponentScore;

    public AudioClip playerPointClip;
    public AudioClip opponentPointClip;
    public AudioClip playerServeClip;
    public AudioClip opponentServeClip;
    public AudioClip playerWinClip;
    public AudioClip opponentWinClip;
    public AudioClip playerAdvantageClip;
    public AudioClip opponentAdvantageClip;
    public AudioClip tiedAudioClip;

    public AudioClip theScoreIsAudioClip;
    public AudioClip toScoreAudioClip;

    private int serveNumber;
    private bool isPlayerServe
    {
        get
        {
            if (serveNumber % 4 < 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    // TODO: Difficulty getters/setters
    // Use this for initialization
    void Start () {

        // TODO: wait for configuration confirmation
        // TODO: wait for difficulty setter
        setupGame();
        startGame();
		
	}

    private void setupGame()
    {
        playerScore = 0;
        opponentScore = 0;
        serveNumber = 0;

        // TODO: set the paddle's location 
        //PaddleScript.TableEdge = BodySourceManager.Instance.baseKinectPosition.Z;
        //PaddleScript.CenterX = BodySourceManager.Instance.baseKinectPosition.X;

        // TODO: say you're ready/calibrated
        //AddAudioToPlayingList(nowCalibAudio);
    }

    private void startGame()
    {
        // TODO: read the serve
        // TODO: set the ball at the center, i guess 

        //StartCoroutine(ReadInitServe());
        //BallObj.transform.position = new Vector3(0, BallObj.transform.position.y, 0);
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void opponentScores()
    {
        opponentScore += 2;
        PlayOpponentPointSound();
        scoreText.text = "Player: " + playerScore + "\nComputer: " + opponentScore;

        // TODO:  update state
        //StartCoroutine(ReadScore());
        // TODO: reset the ball
        // TODO: trigger the next thing?
    }

    public void playerScores()
    {
        playerScore += 2;
        PlayPlayerPointSound();
        scoreText.text = "Player: " + playerScore + "\nComputer: " + opponentScore;

        // TODO: update state
       // StartCoroutine(ReadScore());
        // TODO: reset the ball
        // TODO: trigger the next thing?
    }

    /// <summary>
    /// play the win sound when a player goal is scored.
    /// </summary>
    private void PlayPlayerPointSound()
    {
        AudioManager.Instance.PlaySfx(playerPointClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
    }

    /// <summary>
    /// play the lose sound when an opponent goal is scored.
    /// </summary>
    private void PlayOpponentPointSound()
    {
        AudioManager.Instance.PlaySfx(opponentPointClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
    }

    // <summary>
    /// Reads the serve of the game
    /// </summary>
    private IEnumerator ReadServe()
    {
        AudioClip clipToPlay = isPlayerServe ? playerServeClip : opponentServeClip;

        // Wait .7 seconds less if we've already served once.
        float timeToWait = serveNumber == 0 ? clipToPlay.length : clipToPlay.length - 0.7f;

        AudioManager.Instance.PlayNarration(clipToPlay, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        yield return new WaitForSeconds(timeToWait);
    }

    private void serve()
    {
        serveNumber++; 

        // TODO: do serve logic?
    }

    private IEnumerator playerWins()
    {
        // TODO: set game state to gameover;
        AudioManager.Instance.PlayNarration(playerWinClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        yield return new WaitForSeconds(playerWinClip.length);
    }

    private IEnumerator opponentWins()
    {
        // TODO: set game state to gameover;
        AudioManager.Instance.PlayNarration(opponentWinClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        yield return new WaitForSeconds(opponentWinClip.length);
    }

    private bool isItGameOver()
    {
        // If a player's score is above 11 before the other player's score is at/above 10, then they win.
        // If both players are at/above 10 points, then a player must lead by 2 points to win.
        if ((playerScore >= 11 && opponentScore < 10) || ((playerScore >= 10 && opponentScore >= 10) && (playerScore > opponentScore + 1)))
        {
            StartCoroutine(playerWins());
            return true;
        }
        else if ((opponentScore >= 11 && playerScore < 10) || ((opponentScore >= 10 && playerScore >= 10) && (opponentScore > playerScore + 1)))
        {
            StartCoroutine(opponentWins());
            return true;
        }

        return false;
    }

    public IEnumerator ReadScore()
    {
        // TODO: reset ball position? this needs to be moved

        // Read both players' scores like normal if we haven't hit a game over and both of their scores are less than or equal to 12
        if (playerScore <= 12 && opponentScore <= 12)
        {
            AudioManager.Instance.PlayNarration(theScoreIsAudioClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(theScoreIsAudioClip.length);

            NumberSpeech.Instance.PlayNumbersAudio(playerScore);

            AudioManager.Instance.PlayNarration(toScoreAudioClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(toScoreAudioClip.length);

            NumberSpeech.Instance.PlayNumbersAudio(opponentScore);
        }
        // Both players are greater than or equal to 10 points, so we'll only need to keep track of who has advantage now rather than numbers
        else if (playerScore >= 10 && opponentScore >= 10)
        {
            // Tied
            if (playerScore == opponentScore)
            {
                AudioManager.Instance.PlayNarration(tiedAudioClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
                yield return new WaitForSeconds(tiedAudioClip.length);
            }
            // Player Advantage
            else if (playerScore > opponentScore)
            {
                AudioManager.Instance.PlayNarration(playerAdvantageClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
                yield return new WaitForSeconds(playerAdvantageClip.length);
            }
            // Opponent Advantage
            else if (opponentScore > playerScore)
            {
                AudioManager.Instance.PlayNarration(opponentAdvantageClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
                yield return new WaitForSeconds(opponentAdvantageClip.length);
            }
        }

        // If the game over method returns true, it triggers the appropriate game over procedures.
        // So we only need to worry about/handle the cases where it is false
        if (!isItGameOver())
        {
            StartCoroutine(ReadServe());
        }

    }

    public void ResetBall()
    {
        // TODO: stop the ball, and reset the ball

        if(isPlayerServe)
        {
            // TODO: move ball to new Vector3(0, 5, -100f);
        } else
        {
            // TODO: move ball to new Vector3(0, 5, 100f);
        }
    }
    


   


}
