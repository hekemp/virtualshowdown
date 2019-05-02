using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Timers;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Class that defines the path a ball will go in the Experiment
/// </summary>
public class BallPath
{
    public Vector3 Origin { get; set; }
    public Vector3 Destination { get; set; }
    public BallOriginType BallOriginType { get; set; }
    public BallDestType BallDestType { get; set; }
}
public enum BallOriginType { left, center, right }
public enum BallDestType { farLeft, centerLeft, center, centerRight, farRight }

public class Snapshot { public Vector3 ballPos { get; set; } public Vector3 batPos { get; set; } }

public class ShowdownDrillManager : MonoBehaviour
{
    [Serializable]
    public enum GameState
    {
        Unstarted,
        HandednessSet,
        BallStart,
        BallYourSide,
        BallFinish,
        BallInactive,
        GameOver,
    }

    private enum HintLength { full, shortLen, nonspatial }
    private enum HitRes { miss = 0, tipped = 1, hitNotPastHalf = 2, pastHalfHit = 3, goal = 4 }

    public GameObject BallPrefab;
    public PaddleScript playerPaddle;

    // Flags
    public bool IsTactileDouse;
    public bool IsCorrectionHints;
    public bool IsMidPointAnnounce;
    public bool hasStartedGame;
    public bool hasReached6Minutes;

    #region Audio Clips
    public AudioClip[] positiveReinforcementSuccessClips;
    public AudioClip[] positiveReinforcementMissClips;
    public AudioClip clappingClip;
    public AudioClip levelUpClip;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip nextBallClip;

    public AudioClip clickClip;

    // From originally "End1" --> -1 Stereo
    public AudioClip farLeftClip;
    public AudioClip reachLeftClip;
    public AudioClip endTooFarLeftClip;
    public AudioClip leftClip;

    // From originally "End2" --> -.5 Stereo
    public AudioClip centerLeftClip;

    // From originally "End3" --> 0.0 Stereo
    public AudioClip centerClip;
    public AudioClip middleAudioClip;
    public AudioClip tippedClip;
    public AudioClip backwardsClip;

    // From Originally "End4" --> .5 Stereo
    public AudioClip centerRightClip;

    // From Originally "End5" --> 1 Stereo
    public AudioClip farRightClip;
    public AudioClip reachRightClip;
    public AudioClip tooFarRightClip;
    public AudioClip rightClip;

    // From Originally "Start1" --> -1 Stereo
    public AudioClip startLeftClip;
    //  public AudioClip leftClip;

    // From Originally "Start2" --> 0 Stereo
    public AudioClip startCenterClip;
    // public AudioClip centerClip;

    // From Originally "Start 3" --> 1 Stereo
    public AudioClip startRightClip;
    // public AudioClip rightClip;

    // From originally guideline --> 0 stereo, variable volume
    public AudioClip andIsClip; // .69 v.
    public AudioClip tooForwardClip; // 1 v.
    public AudioClip tooBackClip; // 1 v.
    public AudioClip toClip; // .67 v.
    //public AudioClip leftClip; // .67
    //public AudioClip centerLeftClip; // .67
    //public AudioClip centerRightClip; // .67
    //public AudioClip rightClip; // .67
    public AudioClip moveLeftClip; // 1 v
    public AudioClip moveRightClip; // .67 v

    public AudioClip welcomeToShowdownDrillClip;
    public AudioClip currentlyLeftHandedClip;
    public AudioClip currentlyRightHandedClip;
    public AudioClip handednessOptionsForLeftyClip;
    public AudioClip handednessOptionsForRightyClip;
    public AudioClip nowSetToLefty;
    public AudioClip nowSetToRightyClip;

    public AudioClip quitByMainMenuOptionClip;
    public AudioClip restartGameOptionClip;
    public AudioClip explainDrillOptionClip;
    public AudioClip explainDrillClip;
    public AudioClip repeatOptionClip;
    public AudioClip readyOptionClip;

    public AudioClip yourFinalScoreWas;
    public AudioClip toPlayAgainOptionClip;
    public AudioClip goToMainMenuOptionClip;
    #endregion

    // UI
    public Text ballAndPosText;
    public Text clockText;

    public static GameState CurrentState;

    private List<BallPath> ballPositions = null;
    private GameObject ball;
    private BallScript ballScript;
    private Snapshot collisionSnapshot;
    private Snapshot endSnapshot;
    private IEnumerator<int> shuffledBallPositions;
    /// <summary>
    /// How many balls have been launched
    /// </summary>
    private int currentBall;
    /// <summary>
    /// The level of the player
    /// </summary>
    public int playerLevel;
    /// <summary>
    /// Whether or not we should be announcing details about
    /// the ball trajectory (specific locations)
    /// </summary>
    private bool shouldAnnounceDetailedBallTrajectory;
    /// <summary>
    /// How detailed of hints to be giving
    /// </summary>
    private HintLength hintLength;
    /// <summary>
    /// How many points the user has scored
    /// </summary>
    private int gamePoints;
    /// <summary>
    /// The current path the ball will take
    /// </summary>
    private BallPath currentBallPath;
    /// <summary>
    /// Whether or not this is the ball's first time past the midpoint
    /// </summary>
    private bool isFirstPass;
    private string clockString;
    private int[] prevHits;
    private float maxDistance;

	public static ShowdownDrillManager Instance;

    private Coroutine checkMissCoroutine;

    // CONSTS
    // TODO: Make constants for start 1-3 and end 1-5
    private const int rightStartXPos = 37;
    private const int leftStartXPos = -37;
    private const int centerStartXPos = 0;

    private void Start()
    { 
		Instance = this;
		
        Debug.Log("Starting game");
        hasStartedGame = false;
        CurrentState = GameState.Unstarted;

        StartCoroutine(startNarration());

    }

    IEnumerator startNarration()
    {
        AudioManager.Instance.PlayNarrationImmediate(welcomeToShowdownDrillClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        yield return new WaitForSeconds(welcomeToShowdownDrillClip.length);


    }

    public void handleHandednessPrompt(bool shouldKeepSame)
    {
        if (CurrentState != GameState.Unstarted)
        {
            return;
        }

        if (!shouldKeepSame)
        {
            // they were right handed, so we should set them to be left handed
            if (PreferenceManager.Instance.PlayerHandedness == Handedness.Right)
            {
                //AudioManager.Instance.PlayNarrationImmediate(nowSetToLefty, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

                PreferenceManager.Instance.PlayerHandedness = Handedness.Left;
            }
            else // they were left handed, so we should set them to be right handed
            {
                //AudioManager.Instance.PlayNarrationImmediate(nowSetToRightyClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

                PreferenceManager.Instance.PlayerHandedness = Handedness.Right;
            }
        }

        CurrentState = GameState.HandednessSet;

    }

    public void explainDrill()
    {
        if (CurrentState == GameState.HandednessSet)
        {
            AudioManager.Instance.PlayNarrationImmediate(explainDrillClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        }
    }

    public void sayMenuOption()
    {
        if (CurrentState == GameState.HandednessSet)
        {
            AudioManager.Instance.PlayNarrationImmediate(quitByMainMenuOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            AudioManager.Instance.PlayNarration(restartGameOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            AudioManager.Instance.PlayNarration(explainDrillOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            AudioManager.Instance.PlayNarration(repeatOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            AudioManager.Instance.PlayNarration(readyOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        }

    }


    public void playGameOverNarration()
    {
        if (CurrentState == GameState.GameOver) {
	        AudioManager.Instance.PlayNarration(toPlayAgainOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
	        AudioManager.Instance.PlayNarration(goToMainMenuOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
	        AudioManager.Instance.PlayNarration(repeatOptionClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
	    }
    }

	private IEnumerator checkTenSecondTimeLimit()
	{
		yield return new WaitForSeconds (toPlayAgainOptionClip.length + 10);
		playGameOverNarration();
		// TODO: update with just the gameover audio clip since there isn't these three anymore stuff

		StartCoroutine (checkTenSecondTimeLimit ());
	}

    public void repeatOptions()
    {

        if (CurrentState == GameState.GameOver)
        {
            playGameOverNarration();
        }
    }

    public void ConfirmOptions()
    {
        // TODO: add the methods for changing options
		Debug.Log(CurrentState);
        if (CurrentState == GameState.HandednessSet)
        {
			// TODO: say the reminder prompt
            AudioManager.Instance.StopAllNarration();
            Debug.Log("Setting up experiment");
            hasStartedGame = true;
            CurrentState = GameState.BallStart;
            SetupExperiment();
            StartCoroutine(CheckSixMinuteTimeLimit());
            StartCoroutine(KickBall());
        }
    }

    public void restartGame()
    {
        // TODO: make this smoother
        SceneManager.LoadScene("ShowdownDrill");
    }

    public void goToMainMenu()
    {
        // TODO: make this smoother
        SceneManager.LoadScene("Main_Menu");
    }

    private IEnumerator CheckSixMinuteTimeLimit()
    {
        yield return new WaitForSeconds(360);
        hasReached6Minutes = true;
    }

    /// <summary>
    /// Initialize variables - this is run once when the game starts
    /// </summary>
    private void SetupExperiment()
    {
        ballPositions = CreateBallPositions();
        shuffledBallPositions = ShuffleArray(ballPositions);
        currentBall = -1;
        shouldAnnounceDetailedBallTrajectory = true;
        prevHits = new int[] { 0, 0, 0, 0, 0, 0 };


        createBall(new Vector3(0, -2000, 0)); // TODO: Replace with start2? Currently way below the board
    }

    /// <summary>
    /// Creates a list of ball positions for later use
    /// </summary>
    /// <returns>A list of all possible ball paths</returns>
    private List<BallPath> CreateBallPositions()
    {
        List<BallPath> ballPositions = new List<BallPath>();

        //Left Start
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(0, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.farLeft
        });
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(11, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.centerLeft
        });
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(21, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.center
        });
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(31, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.centerRight
        });
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(42, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.farRight
        });

        //Center Start
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(-21, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.farLeft
        });
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(-11, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.centerLeft
        });
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(0, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.center
        });
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(11, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.centerRight
        });
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(21, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.farRight
        });

        //Right Start
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(-42, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.farLeft
        });
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(-31, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.centerLeft
        });
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(-21, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.center
        });
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(-11, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.centerRight
        });
        ballPositions.Add(new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(0, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.farRight
        });

        return ballPositions;
    }

    /// <summary>
    /// Utilty to shuffle the array of Exp ball locations
    /// </summary>
    /// <returns>An enumerator of shuffled array indices</returns>
    private IEnumerator<int> ShuffleArray<T>(List<T> inList)
    {
        List<int> expListLoc = new List<int>();
        for (int i = 0; i < inList.Count; i++) //Range of positions
        {
            for (int j = 0; j < 2; j++) //Times per position
            {
                expListLoc.Add(i);
            }
        }
        System.Random rng = new System.Random();
        int n = expListLoc.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int value = expListLoc[k];
            expListLoc[k] = expListLoc[n];
            expListLoc[n] = value;
        }
        return expListLoc.GetEnumerator();
    }

    /// <summary>
    /// Create a copy of the ball prefab and set up collision helpers
    /// </summary>
    /// <param name="origin">The location to create the ball at</param>
    private void createBall(Vector3 origin)
    {
        ball = Instantiate(BallPrefab, origin, new Quaternion());
        ballScript = ball.GetComponent<BallScript>();
        ballScript.onBallCollisionEvent = new UnityEvent();
        ballScript.onBallCollisionEvent.AddListener(setCollisionSnapshot);
    }

    public void setCollisionSnapshot()
    {
        collisionSnapshot = new Snapshot()
        {
            batPos = playerPaddle.transform.position,
            ballPos = ball.transform.position
        };
    }

    private void SaveSnapshotOfGame()
    {
        if (ball != null && ball.transform.position.z < -75 && ball.transform.position.z > -85)
        {
            endSnapshot = new Snapshot()
            {
                ballPos = ball.transform.position,
                batPos = playerPaddle.transform.position
            };
        }
    }

    /// <summary>
    /// Pick a random location to kick the ball from and then kick it
    /// </summary>
    private IEnumerator KickBall()
    {
        Debug.Log("Kicking ball");
        currentBall++;
        int currentBallType = shuffledBallPositions.Current;
        ballAndPosText.text = "Ball: " + currentBall + "   Pos: " + currentBallType + "   Level: " + playerLevel;

        currentBallPath = ballPositions[currentBallType];

        bool isNextBallAvailable = shuffledBallPositions.MoveNext();
        if (!isNextBallAvailable)
        {
            Debug.Log("Reshuffle ball paths");
            shuffledBallPositions = ShuffleArray(ballPositions);
        }

        if (shouldAnnounceDetailedBallTrajectory && playerLevel < 3) // We should only announce ball positions when player level is 0, 1, or 2
        {
            yield return AnnounceBallPos(PlayerHintLength, currentBallPath);
        }
        else
        {
            bool shouldAnnounceGameScore = ((UnityEngine.Random.Range(0, 3) == 0 && currentBall != -1 && gamePoints != 1)
            || currentBall == 29); //Randomly 1/3 of the time say how many points
            if (shouldAnnounceGameScore)
            {
                NumberSpeech.Instance.PlayExpPointsAudio(gamePoints);
            }
            else
            {
                AudioManager.Instance.PlayNarration(nextBallClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
                yield return new WaitForSeconds(nextBallClip.length);
            }
        }

        isFirstPass = true;

        // Set up and kick the ball
        CurrentState = GameState.BallStart;
        maxDistance = -130;
        checkMissCoroutine = StartCoroutine(CheckMiss(shouldAnnounceDetailedBallTrajectory));
        SetupBall(currentBallPath.Origin);
        ballScript.KickBallTowards(currentBallPath.Destination, BallSpeed);
    }

    private void SetupBall(Vector3 start)
    {
        Debug.Log("Setup Ball");
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
        ball.transform.position = (start);
        ballScript.ballHitOnce = false;
    }

    private bool shouldLevelUp()
    {
        int numHits = 0;
        for (int i = 0; i < prevHits.Length; i++)
        {
            numHits += prevHits[i];
        }
        return (numHits > 3); // at least 4/6 hits
    }

    /// <summary>
    /// Announces the ball postion when a new ball is created
    /// </summary>
    /// <param name="hintLength"></param>
    /// <returns></returns>
    private IEnumerator AnnounceBallPos(HintLength hintLength, BallPath ballPath)
    {
        Debug.Log("Announce something plz");
        if (hintLength == HintLength.full)
        {
            SetupFullHintAudio();
        }
        else if (hintLength == HintLength.shortLen)
        {
            SetupShortLenHintAudio();
        }
        else if (hintLength == HintLength.nonspatial)
        {
            SetupNonSpatialHintAudio();
        }
        //Play where the ball is starting

        Debug.Log(ballPath.BallOriginType);
        if (ballPath.BallOriginType == BallOriginType.left) //Left Start
        {
            yield return playStartLeft();
        }
        else if (ballPath.BallOriginType == BallOriginType.center) //Center Start
        {
            yield return playStartCenter();
        }
        else if (ballPath.BallOriginType == BallOriginType.right) //Right Start
        {
            yield return playStartRight();
        }

        yield return playAndIs();

        //Play the destination

        if (ballPath.BallDestType == BallDestType.farLeft)
        {
            yield return playEndFarLeft();
        }
        else if (ballPath.BallDestType == BallDestType.centerLeft)
        {
            yield return playEndCenterLeft();
        }
        else if (ballPath.BallDestType == BallDestType.center)
        {
            yield return playEndCenter();
        }
        else if (ballPath.BallDestType == BallDestType.centerRight)
        {
            yield return playEndCenterRight();
        }
        else if (ballPath.BallDestType == BallDestType.farRight)
        {
            yield return playEndFarRight();
        }

        yield return null;
    }

    /// <summary>
    /// Sets the game hints based on the level of the game and also 
    /// takes a snapshot of the game for the correction hints
    /// </summary>
    private void SetGameHints()
    {

        if (playerLevel < 3)
        {
            if (IsTactileDouse)
            {
                TactileDouse();
            }
            if (IsMidPointAnnounce)
            {
                //  Debug.Log("Hello!");
                StartCoroutine(PlayMidPointAudio());
            }
            shouldAnnounceDetailedBallTrajectory = true;
        }

        if (IsCorrectionHints)
        {
            SaveSnapshotOfGame();
        }
    }

    /// <summary>
    /// Calculations of where the ball was
    /// </summary>
    /// <returns></returns>
    private float GetActualXDestination(BallPath ballPath)
    {
        var bt = ballPath.BallOriginType;
        int startXPos = 0;
        if (bt == BallOriginType.center)
        {
            startXPos = centerStartXPos;
        }
        else if (bt == BallOriginType.left)
        {
            startXPos = leftStartXPos;
        }
        else if (bt == BallOriginType.right)
        {
            startXPos = rightStartXPos;
        }
        return startXPos + (2 * ballPath.Destination.x);
    }

    /// <summary>
    /// Adds vibration to the bat based on the X distance away from the ball destination
    /// </summary>
    private void TactileDouse()
    {
        Vector3 batPos = playerPaddle.transform.position;
        if (PreferenceManager.Instance.ControllerRumble)
        {
            float absDist = Math.Abs(batPos.x - GetActualXDestination(currentBallPath));
            //float distAwayFromDest = 100 - absDist;
            if (absDist < 30 && absDist > 20)
            {
                JoyconController.RumbleJoycon(160, 320, 0.1f, 200);
            }
            else if (absDist <= 20 && absDist > 10)
            {
                JoyconController.RumbleJoycon(160, 320, 0.3f, 200);
            }
            else if (absDist < 10)
            {
                JoyconController.RumbleJoycon(160, 320, 0.5f, 200);
            }
        }
    }

    private HintLength PlayerHintLength
    {
        get
        {
            if (playerLevel == 0)
            {
                return HintLength.full;
            }
            if (playerLevel == 1)
            {
                return HintLength.shortLen;
            }
            return HintLength.nonspatial;
        }
    }

    private void Update()
    {
        if (hasStartedGame)
        {
            clockText.text = clockString;

            if (CurrentState != GameState.BallInactive)
            {
                CheckBallPosition();
            }

            SetGameHints();
        }
			
    }

    private void CheckBallPosition()
    {
        // We don't care if a ball isn't here yet
        if (ball == null)
        {
            return;
        }

        if (CurrentState == GameState.BallStart)
        {
            // Check if the ball has made it to the player's half of the stage
            if (ball.transform.position.z < 0)
            {
                // It's on the player's side
                CurrentState = GameState.BallYourSide;
                maxDistance = -130;
            }
        }
        else if (CurrentState == GameState.BallYourSide)
        {
            if (ballScript.ballHitOnce && maxDistance < ball.transform.position.z)
            {
                maxDistance = ball.transform.position.z;
            }

            // Check if the ball has made it to the opponent's half of the stage
            if (ball.transform.position.z > 0)
            {
                // It's on the opponent's side again - we hit it!
                CurrentState = GameState.BallFinish;
            }
        }
        else if (CurrentState == GameState.BallFinish)
        {
            if (ballScript.ballHitOnce && maxDistance < ball.transform.position.z)
            {
                maxDistance = ball.transform.position.z;
            }

            // Check if we get a perfect hit
            if (maxDistance > 10)
            {
                StopCoroutine(checkMissCoroutine);
                checkMissCoroutine = null;
                StartCoroutine(HandlePerfectHit());
            }
        }
    }

    private IEnumerator CheckMiss(bool shouldAnnounce)
    {
        if (shouldAnnounce)
        {
            yield return new WaitForSeconds(10);
        }
        else
        {
            yield return new WaitForSeconds(8);
        }
        yield return HandleMiss();
    }

    /// <summary>
    /// When the ball lands in the opponent's goal (i.e. we get a point)
    /// </summary>
    public void OpponentGoal()
    {
        Debug.Log("OPPONENT GOAL");
        if (checkMissCoroutine != null)
        {
            StopCoroutine(checkMissCoroutine);
        }
        StartCoroutine(HandlePerfectHit());
    }

    /// <summary>
    /// When we score an own goal (i.e. this is bad)
    /// </summary>
    public void OwnGoal()
    {
        Debug.Log("OWN GOAL");
        if (checkMissCoroutine != null)
        {
            StopCoroutine(checkMissCoroutine);
        }
        StartCoroutine(HandleMiss());
    }

    private IEnumerator HandleMiss()
    {
        if (CurrentState == GameState.BallInactive)
        {
            yield break;
        }

        ballScript.StopBallSound();

        Debug.Log("Miss");
        // TODO: Update ball state to new shit

        Debug.Log("Rigidbody constraints all opponent");
        ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        CurrentState = GameState.BallInactive;

        //        AudioManager.Instance.PlaySfx(loseClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        //      yield return new WaitForSeconds(loseClip.length);

        yield return FinishScoring();
    }

    private IEnumerator HandlePerfectHit()
    {
        if (CurrentState == GameState.BallInactive)
        {
            yield break;
        }



        CurrentState = GameState.BallInactive;
        yield return new WaitForSeconds(1.5f);
        ballScript.StopBallSound();
        Debug.Log("Perfect hit");
        // TODO

        Debug.Log("Stop the ball");
        ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        //AudioManager.Instance.PlaySfx(winClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        //yield return new WaitForSeconds(winClip.length);

        yield return FinishScoring();
    }

    private IEnumerator FinishScoring()
    {

        HitRes hr = CurrentHitRes;
        gamePoints += hr == HitRes.miss ? 0 : (int)hr - 1;
        if (hr == HitRes.pastHalfHit || hr == HitRes.goal)
        {
            Debug.Log("Didn't miss");
            prevHits[currentBall % 6] = 1;
            if (shouldLevelUp())
            {
                // TODO: Extract this into separate level up function
                playerLevel++;
                prevHits = new int[6] { 0, 0, 0, 0, 0, 0 };

                AudioManager.Instance.PlayNarration(levelUpClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
                yield return new WaitForSeconds(levelUpClip.length);
            }

            AudioManager.Instance.PlaySfx(clappingClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

            int randomReinforcementToPlay = UnityEngine.Random.Range(0, positiveReinforcementSuccessClips.Length - 1);
            AudioManager.Instance.PlayNarration(positiveReinforcementSuccessClips[randomReinforcementToPlay], AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(Math.Max(positiveReinforcementSuccessClips[randomReinforcementToPlay].length, clappingClip.length));
        }
        else
        {
            Debug.Log("Missed");
            prevHits[currentBall % 6] = 0;
            //Randomly, 1/3 of the time, play a random lose voice sound effect
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                int randomReinforcementToPlay = UnityEngine.Random.Range(0, positiveReinforcementMissClips.Length - 1);
                AudioManager.Instance.PlayNarration(positiveReinforcementMissClips[randomReinforcementToPlay], AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
                yield return new WaitForSeconds(positiveReinforcementMissClips[randomReinforcementToPlay].length);
            }

            if (IsCorrectionHints)
            {
                Debug.Log("Read correction hint");
                yield return StartCoroutine(ReadHitCorrection(hr));
            }
        }
        if (!hasReached6Minutes)
        {
            yield return KickBall();
        }
        else
        {
            yield return handleGameOver();
        }
    }

    private IEnumerator handleGameOver()
    {
        CurrentState = GameState.GameOver;
        Debug.Log("Game over!");
        AudioManager.Instance.PlayNarration(yourFinalScoreWas, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        yield return new WaitForSeconds(yourFinalScoreWas.length);

        yield return NumberSpeech.Instance.PlayExpPointsAudio(gamePoints);

        playGameOverNarration();
		StartCoroutine (checkTenSecondTimeLimit ());
        yield return null;
    }

    /// <summary>
    /// Sets up aduio files for full hints that are spatialized
    /// </summary>
    private void SetupFullHintAudio()
    {
        hintLength = HintLength.full;
    }

    /// <summary>
    /// Sets up audio file for quicker and shorter hints that are spatialized
    /// </summary>
    private void SetupShortLenHintAudio()
    {
        hintLength = HintLength.shortLen;
    }

    /// <summary>
    /// Sets up audio files for quicker shorter hints that are NOT spatialized
    /// </summary>
    private void SetupNonSpatialHintAudio()
    {
        hintLength = HintLength.nonspatial;
    }

    /// <summary>
    /// Gets the ball speed
    /// </summary>
    private int BallSpeed
    {
        get
        {
            if (playerLevel <= 4)
            {
                return 40;
            }
            else if (playerLevel == 5)
            {
                return 50;
            }
            else if (playerLevel == 6)
            {
                return 60;
            }
            else // if playerLevel >= 7
            {
                return 70;
            }
        }
    }

    private HitRes CurrentHitRes
    {
        get
        {
            if ((ballScript.ballHitOnce) && maxDistance < 0)
            {
                return HitRes.hitNotPastHalf;
            }
            else if ((ballScript.ballHitOnce) && maxDistance >= 0)
            {
                return HitRes.pastHalfHit;
            }
            else if ((ballScript.ballHitOnce))
            {
                return HitRes.tipped;
            }
            return HitRes.miss;
        }
    }

    #region Audio Players
    private IEnumerator playStartLeft()
    {
        Debug.Log("At start left I guess");
        if (hintLength == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(startLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Start1]);
            yield return new WaitForSeconds(startLeftClip.length);
        }
        else if (hintLength == HintLength.shortLen)
        {
            AudioManager.Instance.PlayNarration(leftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Start1]);
            yield return new WaitForSeconds(leftClip.length);
        }
        else // oldHintLen == HintLengthNonSpatial
        {
            AudioManager.Instance.PlayNarration(leftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
            yield return new WaitForSeconds(leftClip.length);
        }

        // TODO: Add wait for else case, make last else case elif
    }

    private IEnumerator playStartCenter()
    {
        if (hintLength == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(startCenterClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Start2]);
            yield return new WaitForSeconds(startCenterClip.length);
        }
        else if (hintLength == HintLength.shortLen)
        {
            AudioManager.Instance.PlayNarration(centerClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Start2]);
            yield return new WaitForSeconds(centerClip.length);
        }
        else // oldHintLen == HintLengthNonSpatial
        {
            AudioManager.Instance.PlayNarration(centerClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
            yield return new WaitForSeconds(centerClip.length);
        }

        // TODO: Add wait for else case, make last else case elif
    }

    private IEnumerator playStartRight()
    {
        Debug.Log("at start right i guess");
        if (hintLength == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(startRightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Start3]);
            yield return new WaitForSeconds(startRightClip.length);
        }
        else if (hintLength == HintLength.shortLen)
        {
            AudioManager.Instance.PlayNarration(rightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Start3]);
            yield return new WaitForSeconds(rightClip.length);
        }
        else // oldHintLen == HintLengthNonSpatial
        {
            AudioManager.Instance.PlayNarration(rightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
            yield return new WaitForSeconds(rightClip.length);
        }

        // TODO: Add wait for else case, make last else case elif
    }

    private IEnumerator playEndFarLeft()
    {
        Debug.Log("at end far left I guess");
        if (hintLength == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(farLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End1]);
            yield return new WaitForSeconds(farLeftClip.length);
        }
        else if (hintLength == HintLength.shortLen)
        {
            AudioManager.Instance.PlayNarration(leftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End1]);
            yield return new WaitForSeconds(leftClip.length);
        }
        else // oldHintLen == HintLengthNonSpatial
        {
            AudioManager.Instance.PlayNarration(leftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
            yield return new WaitForSeconds(leftClip.length);
        }

        // TODO: Add wait for else case, make last else case elif
    }

    private IEnumerator playEndCenterLeft()
    {
        if (hintLength == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(centerLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End2]);
            yield return new WaitForSeconds(centerLeftClip.length);
        }
        else if (hintLength == HintLength.shortLen)
        {
            AudioManager.Instance.PlayNarration(centerLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End2]);
            yield return new WaitForSeconds(centerLeftClip.length);
        }
        else // oldHintLen == HintLengthNonSpatial
        {
            AudioManager.Instance.PlayNarration(centerLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
            yield return new WaitForSeconds(centerLeftClip.length);
        }

        // TODO: Add wait for else case, make last else case elif
    }

    private IEnumerator playEndCenter()
    {
        if (hintLength == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(centerClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End3]);
            yield return new WaitForSeconds(centerClip.length);
        }
        else if (hintLength == HintLength.shortLen)
        {
            AudioManager.Instance.PlayNarration(centerClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End3]);
            yield return new WaitForSeconds(centerClip.length);
        }
        else // oldHintLen == HintLengthNonSpatial
        {
            AudioManager.Instance.PlayNarration(centerClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
            yield return new WaitForSeconds(centerClip.length);
        }

        // TODO: Add wait for else case, make last else case elif
    }

    private IEnumerator playEndCenterRight()
    {
        Debug.Log("at center right i guess");
        if (hintLength == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(centerRightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End4]);
            yield return new WaitForSeconds(centerRightClip.length);
        }
        else if (hintLength == HintLength.shortLen)
        {
            AudioManager.Instance.PlayNarration(centerRightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End4]);
            yield return new WaitForSeconds(centerRightClip.length);
        }
        else // oldHintLen == HintLengthNonSpatial
        {
            AudioManager.Instance.PlayNarration(centerRightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
            yield return new WaitForSeconds(centerRightClip.length);
        }

        // TODO: Add wait for else case, make last else case elif
    }

    private IEnumerator playEndFarRight()
    {
        Debug.Log("at right i guess");
        if (hintLength == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(farRightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End5]);
            yield return new WaitForSeconds(farRightClip.length);
        }
        else if (hintLength == HintLength.shortLen)
        {
            AudioManager.Instance.PlayNarration(rightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End5]);
            yield return new WaitForSeconds(rightClip.length);
        }
        else // oldHintLen == HintLengthNonSpatial
        {
            AudioManager.Instance.PlayNarration(rightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
            yield return new WaitForSeconds(rightClip.length);
        }

        // TODO: Add wait for else case, make last else case elif
    }

    private IEnumerator playAndIs()
    {
        if (hintLength == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(andIsClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
            yield return new WaitForSeconds(andIsClip.length);
        }
        else if (hintLength == HintLength.shortLen)
        {
            AudioManager.Instance.PlayNarration(toClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
            yield return new WaitForSeconds(toClip.length);
        }
        else if (hintLength == HintLength.nonspatial)
        {
            AudioManager.Instance.PlayNarration(toClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
            yield return new WaitForSeconds(toClip.length);
        }
        else
        {
            yield return new WaitForSeconds(toClip.length);
        }
    }

    /// <summary>
    /// Plays audio depending if the bat is too far to the right or to the left
    /// </summary>
    private IEnumerator PlayMidPointAudio()
    {
        if (isFirstPass && ball.transform.position.z < 5 && ball.transform.position.z > -5)
        {
            isFirstPass = false;
            var snapShotBatPos = playerPaddle.transform.position;
            float absDist = Math.Abs(snapShotBatPos.x - GetActualXDestination(currentBallPath));

            if (absDist > 20)
            {
                if (snapShotBatPos.x < GetActualXDestination(currentBallPath))
                {
                    AudioManager.Instance.PlayNarration(moveRightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
                    yield return new WaitForSeconds(moveRightClip.length);
                }
                else
                {
                    AudioManager.Instance.PlayNarration(moveLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
                    yield return new WaitForSeconds(moveLeftClip.length);
                }
            }
        }
    }

    /// <summary>
    /// Plays audio for a correction hint based on a hit result of the last ball
    /// This is a Coroutine menthod and by nature is async
    /// </summary>
    /// <param name="hitRes">The results of the last ball</param>
    /// <returns></returns>
    private IEnumerator ReadHitCorrection(HitRes hitRes)
    {
        var snapShotBatPos = endSnapshot.batPos;
        var snapShotBallPos = endSnapshot.ballPos;
        float absDist = Math.Abs(snapShotBatPos.x - snapShotBallPos.x);
        if (hitRes == HitRes.tipped)
        {
            if (collisionSnapshot.ballPos.x < collisionSnapshot.batPos.x - 5)
            {
                AudioManager.Instance.PlayNarration(tippedClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End3]);
                yield return new WaitForSeconds(tippedClip.length);
                AudioManager.Instance.PlayNarration(reachLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End1]);
                yield return new WaitForSeconds(reachLeftClip.length);
            }
            else if (collisionSnapshot.ballPos.x > collisionSnapshot.batPos.x + 5)
            {
                AudioManager.Instance.PlayNarration(tippedClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End3]);
                yield return new WaitForSeconds(tippedClip.length);
                AudioManager.Instance.PlayNarration(reachRightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End5]);
                yield return new WaitForSeconds(reachRightClip.length);
            }
            else if (collisionSnapshot.ballPos.z < collisionSnapshot.batPos.z)
            {
                AudioManager.Instance.PlayNarration(backwardsClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End3]);
                yield return new WaitForSeconds(backwardsClip.length);
            }
        }
        else if (absDist < 10)
        {
            if (snapShotBatPos.z > snapShotBallPos.z)
            {
                //Reached too far forward too soon.
                AudioManager.Instance.PlayNarration(tooForwardClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
                yield return new WaitForSeconds(tooForwardClip.length);
            }
            else
            {
                //Reached too far back
                AudioManager.Instance.PlayNarration(tooBackClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
                yield return new WaitForSeconds(tooBackClip.length);
            }
        }
        else if (snapShotBallPos.x > 0 && snapShotBallPos.x > snapShotBatPos.x)
        {
            //Reach further to the right
            float distOff = snapShotBallPos.x - snapShotBatPos.x;
            AudioManager.Instance.PlayNarration(reachRightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End5]);
            yield return new WaitForSeconds(reachRightClip.length);

            yield return NumberSpeech.Instance.PlayFancyNumberAudio((int)distOff);

            yield return new WaitForSeconds(2.5f);
        }
        else if (snapShotBallPos.x > 0 && snapShotBallPos.x < snapShotBatPos.x)
        {
            //Too far to the right
            float distOff = snapShotBatPos.x - snapShotBallPos.x;

            AudioManager.Instance.PlayNarration(tooFarRightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End5]);
            yield return new WaitForSeconds(tooFarRightClip.length);

            yield return NumberSpeech.Instance.PlayFancyNumberAudio((int)distOff);

            yield return new WaitForSeconds(2.5f);
        }
        else if (snapShotBallPos.x < 0 && snapShotBallPos.x > snapShotBatPos.x)
        {
            //Too far to the left
            float distOff = Math.Abs(snapShotBatPos.x - snapShotBallPos.x);

            AudioManager.Instance.PlayNarration(endTooFarLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End1]);
            yield return new WaitForSeconds(endTooFarLeftClip.length);

            yield return NumberSpeech.Instance.PlayFancyNumberAudio((int)distOff);

            yield return new WaitForSeconds(2.5f);
        }
        else if (snapShotBallPos.x < 0 && snapShotBallPos.x < snapShotBatPos.x)
        {
            //Reach futher to the left
            float distOff = Math.Abs(snapShotBallPos.x - snapShotBatPos.x);

            AudioManager.Instance.PlayNarration(reachLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End1]);
            yield return new WaitForSeconds(reachLeftClip.length);

            yield return NumberSpeech.Instance.PlayFancyNumberAudio((int)distOff);

            yield return new WaitForSeconds(2.5f);
        }
        else if (snapShotBallPos.x == 0)
        {
            //Put it right in the middle
            AudioManager.Instance.PlayNarration(middleAudioClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End3]);
            yield return new WaitForSeconds(middleAudioClip.length);
        }
        yield break;
    }

    #endregion
}