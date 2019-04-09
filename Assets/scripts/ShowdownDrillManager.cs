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

public class ShowdownDrillManager : MonoBehaviour
{


    public GameObject ourBall;
    private GameObject currentBall;
    private BallScript currentBallScript;
    public PaddleScript playerPaddle;
    private BallScript bs;
    private int[] prevHits;
    public static Snapshot CollisionSnapshot { get; set; }
    private Snapshot endSnapshot;
    public AudioClip[] positiveReinforcementSuccessClips;
    public AudioClip[] positiveReinforcementMissClips;
    public AudioClip clappingClip;
    public AudioClip levelUpClip;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip nextBallClip;

    private IEnumerator<int> _expList;
    public AudioClip clickClip;
    public AudioSource announcerWithStereoPanAltering;
    private Coroutine hitPastHalfCoroutine;

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

    // Use this for initialization
     void Start () {
        /*
         currentBall = Instantiate(ourBall, new Vector3(37, 5, 110), new Quaternion());
         bs = currentBall.GetComponent<BallScript>();
         bs.KickBallTowards(new Vector3(-42, 5, -110), 40);
         */

        startDrills();
         ////////////
         ///
     }

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

    private void SaveSnapshotOfGame()
    {
        if (currentBall != null && currentBall.transform.position.z < -75 && currentBall.transform.position.z > -85)
        {
            endSnapshot = new Snapshot()
            {
                ballPos = currentBall.transform.position,
                batPos = playerPaddle.transform.position
            };
        }
    }

    /// <summary>
    /// Creation of list of ball positions and their destinations.
    /// </summary>
    private void CreateBallPosition()
    {
        //Left Start
        ballPositions.Add(0, new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(0, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.farLeft
        });
        ballPositions.Add(1, new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(11, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.centerLeft
        });
        ballPositions.Add(2, new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(21, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.center
        });
        ballPositions.Add(3, new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(31, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.centerRight
        });
        ballPositions.Add(4, new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(42, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.farRight
        });

        //Center Start
        ballPositions.Add(5, new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(-21, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.farLeft
        });
        ballPositions.Add(6, new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(-11, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.centerLeft
        });
        ballPositions.Add(7, new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(0, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.center
        });
        ballPositions.Add(8, new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(11, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.centerRight
        });
        ballPositions.Add(9, new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(21, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.farRight
        });

        //Right Start
        ballPositions.Add(10, new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(-42, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.farLeft
        });
        ballPositions.Add(11, new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(-31, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.centerLeft
        });
        ballPositions.Add(12, new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(-21, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.center
        });
        ballPositions.Add(13, new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(-11, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.centerRight
        });
        ballPositions.Add(14, new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(0, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.farRight
        });
    }

    /// <summary>
    /// Checks if a player can level by checking if the last 6 hits were hit or not.
    /// </summary>
    /// <returns></returns>
    private bool CheckLevelUp()
    {
        int hits = 0;
        foreach (int h in prevHits)
        {
            if (h == 1)
            {
                hits++;
            }
        }
        if (hits > 3) // 4 out of 6 hits, level up!
        {
            prevHits = new int[6] { 0, 0, 0, 0, 0, 0 };
            return true;
        }
        return false;
    }

    public void PlayerScoreCoroutine()
    {
        StartCoroutine(playerScores());
    }

    public void OpponentScoresCoroutine()
    {
        StartCoroutine(opponentScores());
    }

    public IEnumerator playerScores()
    {
        if (currentBall != null)
        {
            Debug.Log("Rigidbody constraints all player");
            currentBall.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            AudioManager.Instance.PlaySfx(winClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(winClip.length);

            newBallOk = true;
            if (hitPastHalfCoroutine != null)
            {
                StopCoroutine(hitPastHalfCoroutine);
                hitPastHalfCoroutine = null;
            }
            StartNextBall(HitRes.goal);
        }
        
    }

    public IEnumerator opponentScores()
    {
        if (currentBall != null)
        {
            Debug.Log("Rigidbody constraints all opponent");
            currentBall.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            newBallOk = true;

            AudioManager.Instance.PlaySfx(loseClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(loseClip.length);

            if (currentBallScript.ballHitOnce)
            {
                StartNextBall(HitRes.tipped);
            }
            else
            {
                StartNextBall(HitRes.miss);
            }
        }
    }

    public void setCollisionSnapshot()
    {
        CollisionSnapshot = new Snapshot()
        {
            batPos = playerPaddle.transform.position,
            ballPos = currentBall.transform.position
        };
    }

    /// <summary>
    /// Sets the ball speed per ball
    /// </summary>
    /// <returns></returns>
    private int DetermineCurrBallSpeed()
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

    /// <summary>
    /// Resets the game points for the game.
    /// This method is called a fwe times just to be safe
    /// </summary>
    private void ResetGamePoints()
    {
        gamePoints = 0;
        playerLevel = 0;
        newBallOk = true;
        prevHits = new int[6] { 0, 0, 0, 0, 0, 0 };
        past6Min = false;
    }

    /// <summary>
    /// Plays audio on how many points the player has.
    /// This is a Coroutine menthod and by nature is async
    /// </summary>
    /// <returns></returns>
    private void ReadGamePoints()
    {
        NumberSpeech.Instance.PlayExpPointsAudio(gamePoints);
    }

    /// <summary>
    /// Audio for next ball and calls spawnBall to start a new ball
    /// </summary>
    /// <returns></returns>
    private IEnumerator NextBallComing()
    {
        Debug.Log("Next ball coming");
        if ((UnityEngine.Random.Range(0, 3) == 0 && _currBallNumber != -1 && gamePoints != 1)
            || _currBallNumber == 29) //Randomly 1/3 of the time say how many points
        {
            NumberSpeech.Instance.PlayExpPointsAudio(gamePoints);
        }
        else if (!IsAnnounceBall)
        {
            AudioManager.Instance.PlayNarration(nextBallClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(nextBallClip.length);
        }

        yield return SpawnBall();
        timerStarted = true;
        oldTime = Time.time;
        //newBallOk = true;
    }

    /// <summary>
    /// Utilty to shuffle the array of Exp ball locations
    /// </summary>
    private void ShuffleArray()
    {
        List<int> expListLoc = new List<int>();
        for (int i = 0; i <= 14; i++) //Range of positions
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
        _expList = expListLoc.GetEnumerator();
    }


    /// <summary>
    /// /////////////////////////
    /// </summary>

    public Text ballAndPosText;
    public Text clockText;

    public bool IsTactileDouse;
    public bool IsCorrectionHints;
    public bool IsMidPointAnnounce;

    public static bool TactileAndAudio { private get; set; }
    public static string globalClockString;

    private string clockString;
    private BallPath _currBallPath;
    private Dictionary<int, BallPath> ballPositions = new Dictionary<int, BallPath>();
    private int _currBallNumber;
    private int _currBallType;
    private int _currBallSpeed;
    private bool playerReady;
    private AudioClip batSound;
    private float oldTime;
    private bool timerStarted;
    private float maxDistance;
    private Timer clockTimer;
    private Timer globalTimer;
    private bool canPressStartButton;
    private int gamePoints;
    private enum HitRes { miss = 0, tipped = 1, hitNotPastHalf = 2, pastHalfHit = 3, goal = 4 }
    private HitRes thisHitres;
    private const int rightStartXPos = 37;
    private const int leftStartXPos = -37;
    private const int centerStartXPos = 0;
    private bool IsAnnounceBall;
    private bool newBallOk;
    private DateTime startTime;
    private bool canPressButton;
    private int playerLevel;

    private enum HintLength { full, shortLen, nonspatial }
    private HintLength oldHintLen;
    private bool firstPass;
    private bool past6Min;

    public void startDrills()
    {
        _currBallNumber = -1;
        CreateBallPosition();
        ShuffleArray();

        playerReady = true;
        // TODO: Play Intro Music
        // StartCoroutine(GameUtils.PlayIntroMusic());
        newBallOk = true;
        clockTimer = new Timer(100);
        clockTimer.Elapsed += ClockTimer_Elapsed;
        globalTimer = new Timer(100);
        globalTimer.Elapsed += GlobalTimer_Elapsed;
        globalTimer.Start();
        gamePoints = 0;
        playerLevel = 0;
        past6Min = false;
        prevHits = new int[6] { 0, 0, 0, 0, 0, 0 };

        /*
         * 
         */
        clockTimer.Start();
        startTime = DateTime.Now;
        StartNextBall(HitRes.hitNotPastHalf); //Starting game, params don't matter here.
        ResetGamePoints();
    }

    private void createBall(Vector3 origin)
    {
        currentBall = Instantiate(ourBall, origin, new Quaternion());
        currentBallScript = currentBall.GetComponent<BallScript>();
        currentBallScript.onBallCollisionEvent = new UnityEvent();
        currentBallScript.onBallCollisionEvent.AddListener(setCollisionSnapshot);
    }

    private void resetBall(Vector3 origin)
    {
        Debug.Log("Ressting ball");
        currentBall.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Debug.Log("Rigidbody constraints none");
        currentBall.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        currentBall.transform.position = (origin);
        currentBallScript.ballHitOnce = false;
    }


    private void Update()
    {

        if (TactileAndAudio)
        {
            IsTactileDouse = true;
            IsMidPointAnnounce = true;
        }
        else
        {
            IsTactileDouse = false;
            IsMidPointAnnounce = true;
        }

        clockText.text = clockString;

        CheckHitResult();

        SetGameHints();

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
                PlayMidPointAudio();
            }
            IsAnnounceBall = true;
        }

        if (IsCorrectionHints)
        {
            SaveSnapshotOfGame();
        }
    }

    /// <summary>
    /// Checks the results of the ball if it was hit or not.
    /// </summary>
    private void CheckHitResult()
    {

        // If we don't have a ball we don't care
        if (currentBall == null)
        {
            return;
        }

        //Perfect hit, start new ball
        if ((currentBallScript.ballHitOnce) && maxDistance > 10)
        {
            Debug.Log("Perfect hit!");
            timerStarted = false;

            hitPastHalfCoroutine = StartCoroutine(HitPastHalfStartNextBall());
            maxDistance = -130;
            return;
        }

        //Wait for result of hit
        if (timerStarted)
        {
            if (currentBallScript.ballHitOnce && maxDistance <= currentBall.transform.position.z)
            {
                // Debug.Log("Been hit and on other side! Yay: " + maxDistance);
                maxDistance = currentBall.transform.position.z;
            }
            else
            {
                maxDistance = -130;
            }

            int timerInterval = IsAnnounceBall ? 10 : 8;

            if (Time.time > oldTime + timerInterval)
            {
                oldTime = Time.time;
                //expState = ExpState.noBall;
                newBallOk = true;
                StartNextBall(DetermineHitRes(currentBallScript));
            }
        }
    }

    /// <summary>
    /// Plays audio depending if the bat is too far to the right or to the left
    /// </summary>
    private IEnumerator PlayMidPointAudio()
    {
        if (currentBall != null)
        {
            if (firstPass && currentBall.transform.position.z < 5 && currentBall.transform.position.z > -5)
            {
                firstPass = false;
                var snapShotBatPos = playerPaddle.transform.position;
                float absDist = Math.Abs(snapShotBatPos.x - GetActualXDestination());

                if (absDist > 20)
                {
                    if (snapShotBatPos.x < GetActualXDestination())
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
    }

    /// <summary>
    /// Adds vibration to the bat based on the X distance away from the ball destination
    /// </summary>
    private void TactileDouse()
    {
        Vector3 batPos = playerPaddle.transform.position;
        if (currentBall != null && PreferenceManager.Instance.ControllerRumble)
        {
            float absDist = Math.Abs(batPos.x - GetActualXDestination());
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


    /// <summary>
    /// Calculations of where the ball was
    /// </summary>
    /// <returns></returns>
    private float GetActualXDestination()
    {
        var bt = _currBallPath.BallOriginType;
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
        return startXPos + (2 * _currBallPath.Destination.x);
    }

    /// <summary>
    /// Timer for the global events clock
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GlobalTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        globalClockString = e.SignalTime.ToLongTimeString() + " +" + e.SignalTime.Millisecond; ;
    }

    /// <summary>
    /// Timer set to 6 min and counting the seconds of the exp trial
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ClockTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        TimeSpan diff = e.SignalTime - startTime;
        clockString = diff.Minutes + ":" + diff.Seconds + "." + diff.Milliseconds;
        if (diff.Minutes > 5)
        {
            past6Min = true;
        }
    }

    /// <summary>
    /// Starts a new ball after waiting 1.5 seconds based on a perfect hit (a hit that went past the halfway point)
    /// </summary>
    /// <returns></returns>
    private IEnumerator HitPastHalfStartNextBall()
    {
        Debug.Log("Hit past half start next ball");
        yield return new WaitForSeconds(1.5f); //Time allowed once ball goes past halfway point
        //BallScript.BallHitOnce = false;
        newBallOk = true;
        StartNextBall(HitRes.pastHalfHit);
    }

    /// <summary>
    /// Determines if the ball was hit, and didn't go past the halfway or it was missed
    /// </summary>
    /// <param name="ball"></param>
    /// <returns>HitRes.miss or HitRes.hit</returns>
    private HitRes DetermineHitRes(BallScript ball)
    {
        if ((ball.ballHitOnce) && maxDistance > -50)
        {
            return HitRes.hitNotPastHalf;
        }
        else if ((ball.ballHitOnce))
        {
            return HitRes.tipped;
        }
        return HitRes.miss;
    }

    private void FinishExp()
    {
        // TODO: Load the we're done, wanna play again or leave
    }

    /// <summary>
    /// Spawns a new ball based on the 30 balls of the experiment list.
    /// </summary>
    private IEnumerator SpawnBall()
    {
        if (playerReady) //Double check to present sending two balls in transition
        {
            if (past6Min)
            {
                FinishExp();
            }
            _currBallNumber++;
            _currBallType = _expList.Current;
            ballAndPosText.text = "Ball: " + _currBallNumber + "   Pos: " + _currBallType + "   Level: " + playerLevel;

            _currBallPath = ballPositions[_currBallType];

            bool isNewBallAvail = _expList.MoveNext();
            if (!isNewBallAvail)
            {
                ShuffleArray();
                yield break;
            }

            if (IsAnnounceBall)
            {
                if (playerLevel == 0)
                {
                    yield return AnnounceBallPos(HintLength.full);
                }
                if (playerLevel == 1)
                {
                    yield return AnnounceBallPos(HintLength.shortLen);
                }
                if (playerLevel == 2)
                {
                    yield return AnnounceBallPos(HintLength.nonspatial);
                }
            }

            newBallOk = false;
            firstPass = true;
            if (currentBall == null)
            {
                createBall(_currBallPath.Origin);
            } else
            {
                resetBall(_currBallPath.Origin);
            }

            //expState = ExpState.ballInPlay;

            _currBallSpeed = DetermineCurrBallSpeed();

            // TODO: play the clickClip sound here
            currentBallScript.KickBallTowards(ballPositions[_currBallType].Destination, _currBallSpeed);
            
        }
    }

    /// <summary>
    /// Announces the ball postion when a new ball is created
    /// </summary>
    /// <param name="hintLength"></param>
    /// <returns></returns>
    private IEnumerator AnnounceBallPos(HintLength hintLength)
    {
 
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
        if (_currBallPath.BallOriginType == BallOriginType.left) //Left Start
        {
            playStartLeft();
        }
        else if (_currBallPath.BallOriginType == BallOriginType.center) //Center Start
        {
            playStartCenter();
        }
        else if (_currBallPath.BallOriginType == BallOriginType.right) //Right Start
        {
            playStartRight();
        }

        playAndIs();

        //Play the destination

        if (_currBallPath.BallDestType == BallDestType.farLeft)
        {
            playEndFarLeft();
        }
        else if (_currBallPath.BallDestType == BallDestType.centerLeft)
        {
            playEndCenterLeft();
        }
        else if (_currBallPath.BallDestType == BallDestType.center)
        {
            playEndCenter();
        }
        else if (_currBallPath.BallDestType == BallDestType.centerRight)
        {
            playEndCenterRight();
        }
        else if (_currBallPath.BallDestType == BallDestType.farRight)
        {
            playEndFarRight();
        }

        yield return new WaitForSeconds(0);

    }

    /// <summary>
    /// Sets up aduio files for full hints that are spatialized
    /// </summary>
    private void SetupFullHintAudio()
    {
        oldHintLen = HintLength.full;
    }

    /// <summary>
    /// Sets up audio file for quicker and shorter hints that are spatialized
    /// </summary>
    private void SetupShortLenHintAudio()
    {
        oldHintLen = HintLength.shortLen;
    }

    /// <summary>
    /// Sets up audio files for quicker shorter hints that are NOT spatialized
    /// </summary>
    private void SetupNonSpatialHintAudio()
    {
        oldHintLen = HintLength.nonspatial;
    }

    /// <summary>
    /// Starts the next ball and adds to the total gamePoints
    /// </summary>
    /// <param name="hitres"></param>
    /// <param name="pointsToAdd"></param>
    private void StartNextBall(HitRes hitres)
    {
        Debug.Log("StartNextBall");
        Debug.Log(hitres);
        if (!playerReady)
        {
            return;
        }
        if (newBallOk)
        {
            newBallOk = false;
            Debug.Log("Sending Ball");
            // Sending Ball
            if (_currBallNumber != -1)
            {
                gamePoints += hitres == HitRes.miss ? 0 : (int)hitres - 1; //The points correlate to the hitres
            }

            if (_currBallNumber != -1)
            {
                StartCoroutine((hitres != HitRes.miss && hitres != HitRes.tipped) ? NextBallHit() : NextBallMissed(hitres));
            }
            else
            {
                StartCoroutine(NextBallComing());
            }
        }

    }

    /// <summary>
    /// Audio for a hit ball and starts a new ball
    /// </summary>
    /// <returns></returns>
    private IEnumerator NextBallHit()
    {
        prevHits[_currBallNumber % 6] = 1;
        if (CheckLevelUp())
        {
            playerLevel++;

            AudioManager.Instance.PlayNarration(levelUpClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(levelUpClip.length);
        }

        AudioManager.Instance.PlaySfx(clappingClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);

        int randomReinforcementToPlay = UnityEngine.Random.Range(0, positiveReinforcementSuccessClips.Length - 1);
        AudioManager.Instance.PlayNarration(positiveReinforcementSuccessClips[randomReinforcementToPlay], AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
        yield return new WaitForSeconds(positiveReinforcementSuccessClips[randomReinforcementToPlay].length);
  
        yield return NextBallComing();
    }



    /// <summary>
    /// Audio for a missed ball and starts a new ball
    /// </summary>
    /// <returns></returns>
    private IEnumerator NextBallMissed(HitRes hitRes)
    {
        prevHits[_currBallNumber % 6] = 0;
        //Randomly, 1/3 of the time, play a random lose voice sound effect
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            int randomReinforcementToPlay = UnityEngine.Random.Range(0, positiveReinforcementMissClips.Length - 1);
            AudioManager.Instance.PlayNarration(positiveReinforcementMissClips[randomReinforcementToPlay], AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
            yield return new WaitForSeconds(positiveReinforcementMissClips[randomReinforcementToPlay].length);
        }

        if (IsCorrectionHints)
        {
            yield return ReadHitCorrection(hitRes);
        }

        yield return NextBallComing();
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
            if (CollisionSnapshot.ballPos.x < CollisionSnapshot.batPos.x - 5)
            {
                AudioManager.Instance.PlayNarration(tippedClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End3]);
                yield return new WaitForSeconds(tippedClip.length);
                AudioManager.Instance.PlayNarration(reachLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End1]);
                yield return new WaitForSeconds(reachLeftClip.length);
            }
            else if (CollisionSnapshot.ballPos.x > CollisionSnapshot.batPos.x + 5)
            {
                AudioManager.Instance.PlayNarration(tippedClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End3]);
                yield return new WaitForSeconds(tippedClip.length);
                AudioManager.Instance.PlayNarration(reachRightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End5]);
                yield return new WaitForSeconds(reachRightClip.length);
            }
            else if (CollisionSnapshot.ballPos.z < CollisionSnapshot.batPos.z)
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

            NumberSpeech.Instance.PlayFancyNumberAudio((int)distOff);

            yield return new WaitForSeconds(2.5f);
        }
        else if (snapShotBallPos.x > 0 && snapShotBallPos.x < snapShotBatPos.x)
        {
            //Too far to the right
            float distOff = snapShotBatPos.x - snapShotBallPos.x;

            AudioManager.Instance.PlayNarration(tooFarRightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End5]);
            yield return new WaitForSeconds(tooFarRightClip.length);

            NumberSpeech.Instance.PlayFancyNumberAudio((int)distOff);

            yield return new WaitForSeconds(2.5f);
        }
        else if (snapShotBallPos.x < 0 && snapShotBallPos.x > snapShotBatPos.x)
        {
            //Too far to the left
            float distOff = Math.Abs(snapShotBatPos.x - snapShotBallPos.x);

            AudioManager.Instance.PlayNarration(endTooFarLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End1]);
            yield return new WaitForSeconds(endTooFarLeftClip.length);

            NumberSpeech.Instance.PlayFancyNumberAudio((int)distOff);

            yield return new WaitForSeconds(2.5f);
        }
        else if (snapShotBallPos.x < 0 && snapShotBallPos.x < snapShotBatPos.x)
        {
            //Reach futher to the left
            float distOff = Math.Abs(snapShotBallPos.x - snapShotBatPos.x);

            AudioManager.Instance.PlayNarration(reachLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End1]);
            yield return new WaitForSeconds(reachLeftClip.length);

            NumberSpeech.Instance.PlayFancyNumberAudio((int)distOff);

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

    


    /// <summary>
    /// Resets all the parameters of an experiment between Naive and our mode.
    /// </summary>
    private void ResetExp()
    {
        // TODO: figure out what's different from here and the start and if we can't just use that
        _currBallNumber = -1;
        gamePoints = 0;
        playerLevel = 0;
        playerReady = false;
        // TODO: play IntroMusic from old GameUtils

        newBallOk = true;
        prevHits = new int[6] { 0, 0, 0, 0, 0, 0 };
        past6Min = false;
        ResetGamePoints();
    }

    private IEnumerator playStartLeft()
    {
        if (oldHintLen == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(startLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Start1]);
            yield return new WaitForSeconds(startLeftClip.length);
        }
        else if (oldHintLen == HintLength.shortLen)
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
        if (oldHintLen == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(startCenterClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Start2]);
            yield return new WaitForSeconds(startCenterClip.length);
        }
        else if (oldHintLen == HintLength.shortLen)
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
        if (oldHintLen == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(startRightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Start3]);
            yield return new WaitForSeconds(startRightClip.length);
        }
        else if (oldHintLen == HintLength.shortLen)
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
        if (oldHintLen == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(farLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End1]);
            yield return new WaitForSeconds(farLeftClip.length);
        }
        else if (oldHintLen == HintLength.shortLen)
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
        if (oldHintLen == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(centerLeftClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End2]);
            yield return new WaitForSeconds(centerLeftClip.length);
        }
        else if (oldHintLen == HintLength.shortLen)
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
        if (oldHintLen == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(centerClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End3]);
            yield return new WaitForSeconds(centerClip.length);
        }
        else if (oldHintLen == HintLength.shortLen)
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
        if (oldHintLen == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(centerRightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End4]);
            yield return new WaitForSeconds(centerRightClip.length);
        }
        else if (oldHintLen == HintLength.shortLen)
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
        if (oldHintLen == HintLength.full)
        {
            AudioManager.Instance.PlayNarration(farRightClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.End5]);
            yield return new WaitForSeconds(farRightClip.length);
        }
        else if (oldHintLen == HintLength.shortLen)
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
        if (oldHintLen == HintLength.full)
        {  
            AudioManager.Instance.PlayNarration(andIsClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
            yield return new WaitForSeconds(andIsClip.length);
        }
        else if (oldHintLen == HintLength.shortLen)
        {
            AudioManager.Instance.PlayNarration(toClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
            yield return new WaitForSeconds(toClip.length);
        }
        else if (oldHintLen == HintLength.nonspatial)
        {
            AudioManager.Instance.PlayNarration(toClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Guideline]);
            yield return new WaitForSeconds(toClip.length);
        } else
        {
            yield return new WaitForSeconds(toClip.length);
        }
    }

}