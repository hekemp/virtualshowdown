using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class BallScript : MonoBehaviour {

    private Rigidbody rb;

    public AudioClip wallHitSound;
    public AudioClip outOfTableBoundsSound;
    public AudioClip ballRollingSound;
    public AudioClip hitPaddleSound;

    public AudioMixerSnapshot farSideSnap;
    public AudioMixerSnapshot closeSideSnap;

    private AudioSource _ballSoundSource;
    private const float maxspeed = 250;

    public bool ballHitOnce = false;



    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        _ballSoundSource = GetComponent<AudioSource>();
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
    }

    /// <summary>
    /// Modifys the ball sound based on where the ball is and the speed of the ball
    /// </summary>
    private void DynamicAudioChanges()
    {
        //Change and limit pitch change on ball
        _ballSoundSource.pitch = GameUtils.Scale(0, maxspeed, 0.8f, 1.25f, Mathf.Abs(rb.velocity.magnitude));

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
            _ballSoundSource.panStereo = -1;
        }
        else if (rb.position.x >= -30 && rb.position.x < -10)
        {
            _ballSoundSource.panStereo = -0.5f;
        }
        else if (rb.position.x >= -10 && rb.position.x < 10)
        {
            _ballSoundSource.panStereo = 0;
        }
        else if (rb.position.x >= 10 && rb.position.x < 30)
        {
            _ballSoundSource.panStereo = 0.5f;
        }
        else if (rb.position.x >= 30)
        {
            _ballSoundSource.panStereo = 1;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "oppo")
        {
            ballHitOnce = true;

            float impulse = collision.impulse.sqrMagnitude;
            if (collision.gameObject.tag == "Player" && PreferenceManager.Instance.ControllerRumble)
            {
                float rumbleAmp = GameUtils.Scale(0, 243382, 0.3f, 0.9f, impulse);
                JoyconController.RumbleJoycon(160, 320, rumbleAmp, 200);
            }
            _ballSoundSource.volume = GameUtils.Scale(0, 243382, 0.07f, 0.3f, impulse);
            StartPaddleCollideSound();
            
        }
        if (collision.gameObject.tag == "Wall")
        {
            StartWallCollideSound();
        }
    }

    private void StartBallSound()
    {
        if (!_ballSoundSource.isPlaying)
        {
            _ballSoundSource.loop = true;
            _ballSoundSource.bypassEffects = false;
            _ballSoundSource.bypassReverbZones = true;
            _ballSoundSource.priority = 64;
            _ballSoundSource.volume = .25f;
            _ballSoundSource.panStereo = 0.0f;
            _ballSoundSource.spatialBlend = 1.0f;
            _ballSoundSource.reverbZoneMix = 0.559f;
            _ballSoundSource.clip = ballRollingSound;
            _ballSoundSource.Play();
        }
    }

    private void StartWallCollideSound()
    {
        if (!_ballSoundSource.isPlaying)
        {
            _ballSoundSource.priority = 64;
            _ballSoundSource.bypassReverbZones = true;
            _ballSoundSource.bypassEffects = true;
            _ballSoundSource.loop = false;
            _ballSoundSource.pitch = 1.0f;
            _ballSoundSource.spatialBlend = 1.0f;
            _ballSoundSource.volume = 0.391f;
            _ballSoundSource.panStereo = 0.0f;
            _ballSoundSource.clip = wallHitSound;
            _ballSoundSource.Play();
        }
    }

    private void StartPaddleCollideSound()
    {
        if (!_ballSoundSource.isPlaying)
        {
            _ballSoundSource.bypassReverbZones = true;
            _ballSoundSource.bypassEffects = true;
            _ballSoundSource.loop = false;
            _ballSoundSource.priority = 64;
            _ballSoundSource.spatialBlend = 1.0f;
            _ballSoundSource.reverbZoneMix = 0.559f;
            _ballSoundSource.pitch = 1.0f;
            _ballSoundSource.panStereo = 0.0f;
            _ballSoundSource.clip = hitPaddleSound;
            _ballSoundSource.Play();
        }
    }

}
