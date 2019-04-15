using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class BallScript : MonoBehaviour {

    private Rigidbody rb;

    public AudioClip wallHitSound;
    public AudioClip ballRollingSound;
    public AudioClip hitPaddleSound;
    public AudioClip clickSound;

    public AudioMixerSnapshot farSideSnap;
    public AudioMixerSnapshot closeSideSnap;

    public UnityEvent onBallCollisionEvent;

    private AudioSource[] _ballSoundSources;
    private enum BallSoundSource
    {
        Rolling = 0,
        PaddleHit = 1,
        HitWall = 2,
        Click = 3,
    }

    private const float maxspeed = 250;

    public bool ballHitOnce = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _ballSoundSources = GetComponents<AudioSource>();
    }

    private void Start()
    {
        //StartBallSound();
    }

    public float CurrentSpeed()
    {
        return rb.velocity.magnitude;
    }

    /// <summary>
    /// Method that checks if the ball is within the bounds of the table. 
    /// </summary>
    public bool CheckBallInGame()
    {
        return (rb.position.x < -51 || rb.position.x > 51 || rb.position.z < -130 || rb.position.z > 130);
    }

    private void FixedUpdate()
    {
        DynamicAudioChanges();

        //Add a speed limit to the ball
        Vector3 oldVel = rb.velocity;
        rb.velocity = Vector3.ClampMagnitude(oldVel, maxspeed);

    }

    public void KickBallTowards(Vector3 Destination, int speedOfKick)
    {
        rb.AddForce(Destination * speedOfKick, ForceMode.Acceleration);
        StartBallSound();
    }

    /// <summary>
    /// Modifys the ball sound based on where the ball is and the speed of the ball
    /// </summary>
    private void DynamicAudioChanges()
    {
        //Change and limit pitch change on ball
        _ballSoundSources[(int)BallSoundSource.Rolling].pitch = GameUtils.Scale(0, maxspeed, 0.8f, 1.25f, Mathf.Abs(rb.velocity.magnitude));

        //Change rolling sounds based on speed of ball
        //ballSoundSource.volume = GameUtils.Scale(0, maxspeed, 0, 1, Math.Abs(rb.velocity.magnitude));

        //Dynamic LowPass Audio filter snapshot changing when ball passes halfway point
        if (rb.position.z > 0)
        {
            farSideSnap.TransitionTo(0.1f);
        }
        else
        {
            closeSideSnap.TransitionTo(0.1f);
        }

        //Change Stereo Pan in buckets
        if (rb.position.x < -30)
        {
            _ballSoundSources[(int)BallSoundSource.Rolling].panStereo = -1;
        }
        else if (rb.position.x >= -30 && rb.position.x < -10)
        {
            _ballSoundSources[(int)BallSoundSource.Rolling].panStereo = -0.5f;
        }
        else if (rb.position.x >= -10 && rb.position.x < 10)
        {
            _ballSoundSources[(int)BallSoundSource.Rolling].panStereo = 0;
        }
        else if (rb.position.x >= 10 && rb.position.x < 30)
        {
            _ballSoundSources[(int)BallSoundSource.Rolling].panStereo = 0.5f;
        }
        else if (rb.position.x >= 30)
        {
            _ballSoundSources[(int)BallSoundSource.Rolling].panStereo = 1;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "oppo")
        {
            ballHitOnce = true;

            if (onBallCollisionEvent != null)
            {
                onBallCollisionEvent.Invoke();
            }
            

            float impulse = collision.impulse.sqrMagnitude;
            if (collision.gameObject.tag == "Player" && PreferenceManager.Instance.ControllerRumble)
            {
                float rumbleAmp = GameUtils.Scale(0, 243382, 0.3f, 0.9f, impulse);
                JoyconController.RumbleJoycon(160, 320, rumbleAmp, 200);
            }
            _ballSoundSources[(int)BallSoundSource.PaddleHit].volume = GameUtils.Scale(0, 243382, 0.07f, 0.3f, impulse);
            StartPaddleCollideSound();
            
        }
        if (collision.gameObject.tag == "Wall")
        {
            StartWallCollideSound();
        }
    }

    private void StartBallSound()
    {

        Debug.Log("Arrives at sound");
        // TODO: Optimize this to not set everything per call
        // (rb.velocity.x > 0 || rb.velocity.y > 0) && 
        if (!_ballSoundSources[(int)BallSoundSource.Rolling].isPlaying)
        {
            Debug.Log("Sets Stuff/was not playing");
            _ballSoundSources[(int)BallSoundSource.Rolling].loop = true;
            _ballSoundSources[(int)BallSoundSource.Rolling].bypassEffects = false;
            _ballSoundSources[(int)BallSoundSource.Rolling].bypassReverbZones = true;
            _ballSoundSources[(int)BallSoundSource.Rolling].priority = 64;
            _ballSoundSources[(int)BallSoundSource.Rolling].volume = .25f;
            _ballSoundSources[(int)BallSoundSource.Rolling].panStereo = 0.0f;
            _ballSoundSources[(int)BallSoundSource.Rolling].spatialBlend = 1.0f;
            _ballSoundSources[(int)BallSoundSource.Rolling].reverbZoneMix = 0.559f;
            _ballSoundSources[(int)BallSoundSource.Rolling].clip = ballRollingSound;
            _ballSoundSources[(int)BallSoundSource.Rolling].Play();
            _ballSoundSources[(int)BallSoundSource.Rolling].dopplerLevel = 0.25f;
            _ballSoundSources[(int)BallSoundSource.Rolling].spread = 0;

            _ballSoundSources[(int)BallSoundSource.Rolling].Play();
        }
    }

    public void StopBallSound()
    {
        _ballSoundSources[(int)BallSoundSource.Rolling].Stop();
    }

    private void StartWallCollideSound()
    {
        // TODO: Optimize this to not set everything per call
        if (!_ballSoundSources[(int)BallSoundSource.HitWall].isPlaying)
        {
            _ballSoundSources[(int)BallSoundSource.HitWall].priority = 64;
            _ballSoundSources[(int)BallSoundSource.HitWall].bypassReverbZones = true;
            _ballSoundSources[(int)BallSoundSource.HitWall].bypassEffects = true;
            _ballSoundSources[(int)BallSoundSource.HitWall].loop = false;
            _ballSoundSources[(int)BallSoundSource.HitWall].pitch = 1.0f;
            _ballSoundSources[(int)BallSoundSource.HitWall].spatialBlend = 1.0f;
            _ballSoundSources[(int)BallSoundSource.HitWall].volume = 0.391f;
            _ballSoundSources[(int)BallSoundSource.HitWall].panStereo = 0.0f;
            _ballSoundSources[(int)BallSoundSource.HitWall].clip = wallHitSound;
            _ballSoundSources[(int)BallSoundSource.HitWall].Play();
        }
    }

    private void StartPaddleCollideSound()
    {
        // TODO: Optimize this to not set everything per call
        if (!_ballSoundSources[(int)BallSoundSource.PaddleHit].isPlaying)
        {
            _ballSoundSources[(int)BallSoundSource.PaddleHit].bypassReverbZones = true;
            _ballSoundSources[(int)BallSoundSource.PaddleHit].bypassEffects = true;
            _ballSoundSources[(int)BallSoundSource.PaddleHit].loop = false;
            _ballSoundSources[(int)BallSoundSource.PaddleHit].priority = 64;
            _ballSoundSources[(int)BallSoundSource.PaddleHit].spatialBlend = 1.0f;
            _ballSoundSources[(int)BallSoundSource.PaddleHit].reverbZoneMix = 0.559f;
            _ballSoundSources[(int)BallSoundSource.PaddleHit].pitch = 1.0f;
            _ballSoundSources[(int)BallSoundSource.PaddleHit].panStereo = 0.0f;
            _ballSoundSources[(int)BallSoundSource.PaddleHit].clip = hitPaddleSound;
            _ballSoundSources[(int)BallSoundSource.PaddleHit].Play();
        }
    }

    public void StartClickSound()
    {
        // TODO: Add
        /*
         * 
         * mute = false
bypass effects = true
bypass reverb zone = true
play on awake = false
loop = false
priority = 64
volume = 1
pitch = 1
stereo pan = 0
spatial blend = 1
reverb zone mix = .559
*/
    }

}
