using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

[RequireComponent(typeof(UnityEngine.AudioSource))]
public class PaddleScript : MonoBehaviour {

    public static float TableEdge { get; set; }
    public static float CenterX { get; set; }

    public bool keyboardControl;
    public AudioClip wallHitSound;

    private Rigidbody rb;
    private bool batUpOnce;
    private bool batDownOnce;
    private float oldTime;
    private float halfBatLen;
    private float halfBatThick;
    private const float estAvgError = 7f; //Prev 26
    private const float unityTableEdge = 130f;


    private UnityEngine.AudioSource _paddleAudio;

    void PlayWallHit()
    {
        if (!_paddleAudio.isPlaying)
        {
            _paddleAudio.clip = wallHitSound;
            _paddleAudio.Play();
        }
    }

    void StopWallHit()
    {
        _paddleAudio.Pause();
    }

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        halfBatLen = transform.localScale.x / 2;
        halfBatThick = transform.localScale.z / 2;
        batUpOnce = false;
        batDownOnce = false;
        TableEdge = 0;
        CenterX = 0;

        _paddleAudio = GetComponent<UnityEngine.AudioSource>();
        _paddleAudio.loop = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (BodySourceManager.Instance == null)
        {
            return;
        }

        float centerXPoint = CheckCalibratedX(BodySourceManager.Instance.baseKinectPosition.X);
        float maxZPoint = CheckCalibratedZ(BodySourceManager.Instance.baseKinectPosition.Z);

        //Calculate the position of the paddle based on the distance from the mid spine join
        float xPos = (centerXPoint - BodySourceManager.Instance.handPosition.X) * 100,
              zPos = (maxZPoint - BodySourceManager.Instance.handPosition.Z) * 100,
              yPos = transform.position.y;


        //Smoothing applied to slow down bat so it doesn't phase through ball
        Vector3 newPosition = new Vector3(-xPos, yPos, (zPos - unityTableEdge - estAvgError));
        //Smooting factor of fixedDeltaTime*20 is to keep the paddle from moving so quickly that is
        //phases through the ball on collision.
        rb.MovePosition(Vector3.Lerp(rb.position, newPosition, Time.fixedDeltaTime * 13));

        if (keyboardControl)
        {
            float movehorizontal = Input.GetAxis("Horizontal");
            float movevertical = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(movehorizontal, 0.0f, movevertical);
            rb.MovePosition(transform.position + movement * Time.deltaTime * 300);
        }

        RotateBat(BodySourceManager.Instance.wristPosition, BodySourceManager.Instance.handPosition);

		if ((ShowdownManager.currentGameState >= ShowdownManager.ShowdownGameState.SettingBall)|| 
			(ShowdownDrillManager.CurrentState >= ShowdownDrillManager.GameState.BallStart)) {
        	CheckBatInGame();
		}
    }

    private float CheckCalibratedX(float xPos)
    {
        return CenterX != 0 ? CenterX : xPos;
    }

    private float CheckCalibratedZ(float zPos)
    {
        return TableEdge != 0 ? TableEdge : zPos;
    }

    private void CheckBatInGame()
    {
        float outOfBoundsBy = 0;
        float xSide = 50 - halfBatLen;
        float zSide = 130 - halfBatThick;

        if ((transform.position.x > xSide || transform.position.x < -xSide || transform.position.z < -zSide))
        {

            if (Mathf.Abs(transform.position.x) > xSide)
            {
                outOfBoundsBy = Mathf.Abs(transform.position.x) - xSide;
            }
            else if (transform.position.z < -zSide)
            {
                outOfBoundsBy = Mathf.Abs(transform.position.z) - zSide;
            }
        }

        if (outOfBoundsBy == 0)
        {
            StopWallHit();

            if(PreferenceManager.Instance.ControllerRumble)
            {
                JoyconController.RumbleJoycon(0, 0, 0);
            }
        }
        else
        {
            PlayWallHit();

            if (PreferenceManager.Instance.ControllerRumble)
            {
                // TODO: Make the rumble relative to how far out of bounds you are with over 30 being .9 and everything else being relative?
             
                // If they're only out of bounds by a small amount (< 10) only rumble by a small amount (40%)
                if (outOfBoundsBy < 10)
                {
                    JoyconController.RumbleJoycon(90, 270, 0.4f);
                }
                // If they're out of bounds by a fair amount (< 30) rumble a fair amount (70%)
                else if (outOfBoundsBy < 30)
                {
                    JoyconController.RumbleJoycon(90, 270, 0.7f);
                }
                // If they're very far out of bounds (>= 30) rumble by a large amount (90%)
                else
                {
                    JoyconController.RumbleJoycon(90, 270, 0.9f);
                }
            }
                
            _paddleAudio.volume = GameUtils.Scale(0, 45, 0.1f, 1, outOfBoundsBy);
        }
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if(other.gameObject.tag == "GoalTrigger")
    //    {
    //        JoyconController.RumbleJoycon(100, 400, 0.2f);
    //    }
    //}
    //private void OnTriggerExit(Collider other)
    //{
    //    JoyconController.RumbleJoycon(0, 0, 0);
    //}

    /// <summary>
    /// Calculates the rotation of the bat in the virtual world
    /// </summary>
    /// <param name="handBasePos">Distance of the base of the hand from the Kinect</param>
    /// <param name="handTipPos">Distance of the tip of the hand from the Kinect</param>
    private void RotateBat(CameraSpacePoint handBasePos, CameraSpacePoint handTipPos)
    {
        float o = handBasePos.Z - handTipPos.Z,
              a = handBasePos.X - handTipPos.X,
              angle = Mathf.Rad2Deg * Mathf.Atan2(o, a);

        Quaternion newRotation = Quaternion.AngleAxis(0, Vector3.up);

        if (-35 <= angle && angle < 35)
        {
            newRotation = Quaternion.AngleAxis(0, Vector3.up);
        }
        else if (angle >= 35 && angle < 90)
        {
            newRotation = Quaternion.AngleAxis(45, Vector3.up);
        }
        else if (angle >= 90 && angle < 135)
        {
            newRotation = Quaternion.AngleAxis(135, Vector3.up);
        }
        else if (angle >= 135)
        {
            newRotation = Quaternion.AngleAxis(180, Vector3.up);
        }
        else if (angle < -35)
        {
            newRotation = Quaternion.AngleAxis(-45, Vector3.up);
        }
        rb.rotation = Quaternion.Slerp(transform.rotation, newRotation, .05f);

        //No snapping or smoothing
        //rb.MoveRotation(Quaternion.Euler(0, angle, 0));
    }
}
