using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowdownManager : MonoBehaviour {

    public TextMesh scoreText;
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
    public AudioClip readyGoAudioClip;

    public PaddleScript playerPaddle;

    public GameObject BallPrefab;
    private GameObject ball;
    private BallScript ballScript;

    public enum ShowdownGameState
    {
        Unstarted, DifficultySet, Setup, SettingBall, BallSet, BallInPlay, GoalScored, GameOver
    }
    public static ShowdownGameState currentGameState;

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

        currentGameState = ShowdownGameState.Unstarted;
        
        currentGameState = ShowdownGameState.DifficultySet;
        // TODO: wait for difficulty setter

        currentGameState = ShowdownGameState.Setup;
        // TODO: wait for configuration confirmation

        setupGame();
        startGame();
		
	}

    private void createBall(Vector3 origin)
    {
        ball = Instantiate(BallPrefab, origin, new Quaternion());
        ballScript = ball.GetComponent<BallScript>();
        //ballScript.onBallCollisionEvent = new UnityEvent();
        //ballScript.onBallCollisionEvent.AddListener(setCollisionSnapshot);
    }

    private void setupGame()
    {
        playerScore = 0;
        opponentScore = 0;
        serveNumber = 0;
        createBall(new Vector3(0, 5, 0));

        GoalScript[] goalScriptList = GameObject.FindObjectsOfType<GoalScript>();
        foreach (GoalScript goal in goalScriptList)
        {
            goal.isInDrillMode = false;
        }

        if (BodySourceManager.Instance)
        {
            PaddleScript.TableEdge = BodySourceManager.Instance.baseKinectPosition.Z;
            PaddleScript.CenterX = BodySourceManager.Instance.baseKinectPosition.X;
        }

        // TODO: say you're ready/calibrated
        //AddAudioToPlayingList(nowCalibAudio);
        Debug.Log("Ready to go!");

        currentGameState = ShowdownGameState.SettingBall;
    }

    private void Update()
    {
        checkBall();
    }

    private void startGame()
    {
        // TODO: set up stuff for starting the game

        StartCoroutine(serveBall());
        
    }

    private IEnumerator serveBall()
    {
        ResetBall();

        ballScript.GetComponent<Rigidbody>().isKinematic = true;

        yield return StartCoroutine(ReadServe());

        currentGameState = ShowdownGameState.SettingBall;

        ResetBallForServe();

    }

    public void opponentScores()
    {
        Debug.Log(currentGameState);
        if (currentGameState == ShowdownGameState.BallInPlay)
        {
            opponentScore += 2;
            PlayOpponentPointSound();
            handleGoal();
        }
    }

    public void playerScores()
    {
        Debug.Log(currentGameState);
        if(currentGameState == ShowdownGameState.BallInPlay)
        {
            playerScore += 2;
            PlayPlayerPointSound();
            handleGoal();
        }
    }

    private void handleGoal()
    {
        scoreText.text = "Player: " + playerScore + "\nComputer: " + opponentScore;

        currentGameState = ShowdownGameState.GoalScored;

        // TODO: reset the ball

        serveNumber++;

        StartCoroutine(ReadScore());
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

    private IEnumerator playerWins()
    {
        currentGameState = ShowdownGameState.GameOver;
        AudioManager.Instance.PlayNarration(playerWinClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        yield return new WaitForSeconds(playerWinClip.length);
    }

    private IEnumerator opponentWins()
    {
        currentGameState = ShowdownGameState.GameOver;
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

        // Read both players' scores like normal if we haven't hit a game over and both of their scores are less than or equal to 12
        if (playerScore <= 12 && opponentScore <= 12)
        {
            AudioManager.Instance.PlayNarration(theScoreIsAudioClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(theScoreIsAudioClip.length);

            yield return NumberSpeech.Instance.PlayNumbersAudio(playerScore);

            AudioManager.Instance.PlayNarration(toScoreAudioClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(toScoreAudioClip.length);

            yield return NumberSpeech.Instance.PlayNumbersAudio(opponentScore);
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
            StartCoroutine(serveBall());
        }

    }

    public void ResetBallForServe()
    {
        if(isPlayerServe)
        {
            ballScript.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.GetComponent<Rigidbody>().position = new Vector3(0, 5, -100f);
        } else
        {
            ballScript.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.GetComponent<Rigidbody>().position = new Vector3(0, 5, 100f);
        }
    }

    public void ResetBall()
    {
        ballScript.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().position = new Vector3(0, transform.position.y, 0);
    }

    private void checkBall()
    {

        if (currentGameState == ShowdownGameState.SettingBall)
        {
            if (isPlayerServe)
            {
                var paddlePos = playerPaddle.transform.position;
                if (JoyconController.ButtonPressed)
                {
                    // Is our paddle within the bounds of the stage>
                    if (paddlePos.x < 50 && paddlePos.x > -50 && paddlePos.z > -130)
                    {
                        currentGameState = ShowdownGameState.BallSet;
                        StartCoroutine(PauseForBallSetAudio(new Vector3(paddlePos.x, ball.transform.position.y, paddlePos.z + 10)));
                    }
                }
                else //Still thinking where to place ball
                {
                    ballScript.GetComponent<Rigidbody>().MovePosition(new Vector3(paddlePos.x, ball.transform.position.y, paddlePos.z + 10));
                }
            } else
            {
                currentGameState = ShowdownGameState.BallSet;
                StartCoroutine(PauseForBallSetAudio(new Vector3(0, transform.position.y, 100)));
            }
            
        }
        else if (currentGameState == ShowdownGameState.GoalScored)
        {
            ballScript.StopBallSound();
        }
        //Unstarted, 
        //DifficultySet,
        //Setup,
        //SettingBall, 
        
        //BallSet,
        //BallInPlay,
        //GoalScored,
        //GameOver
    }

    private IEnumerator PauseForBallSetAudio(Vector3 ballPos)
    {
        // Set our ball's velocity to 0 and start it at the specified ball position
        
        ballScript.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ballScript.GetComponent<Rigidbody>().MovePosition(ballPos);

        AudioManager.Instance.PlayNarration(readyGoAudioClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        yield return new WaitForSeconds(readyGoAudioClip.length - 0.5f); //Give 1 second to move the bat way bc of jitters

        currentGameState = ShowdownGameState.BallInPlay;

        ballScript.GetComponent<Rigidbody>().isKinematic = false;
        ballScript.StartBallSound();
    }


    // TODO: method for handling ball speed points

    /*
     * RYAN'S CODE
     * 
    

    private void BallSpeedPoints()
    {
        if (rb.velocity.magnitude < 8 && GameUtils.playState == GameUtils.GamePlayState.InPlay)
        {
            if (timerStarted)
            {
                if (Time.time > oldTime + 2)
                {
                    GameUtils.ResetBall(transform.gameObject);
                    timerStarted = false;
                    GameUtils.playState = GameUtils.GamePlayState.SettingBall;
                    rb.isKinematic = true;
                    if (transform.position.z > 0)
                    {
                        GoalScript.PlayerScore++;
                        GoalScript.PlayWinSound();
                    }
                    else
                    {
                        GoalScript.OpponentScore++;
                        GoalScript.PlayLoseSound();
                    }
                    StartCoroutine(GoalScript.ReadScore());
                }
            }
            else
            {
                timerStarted = true;
                oldTime = Time.time;
            }
        }
        else
        {
        // TODO: if that timer coroutine is in and not null stop it
            timerStarted = false;
        }
    }


    }*/

    }
