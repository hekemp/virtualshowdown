using System;
using UnityEngine;

public class BatAI : MonoBehaviour
{
    //Speed at which the AI moves
    public static float aiSpeed;
    public GameObject ball;
    private Rigidbody rb;

    private Vector3 hitPosition;
    public enum BatAIState
    {
        atHitPosition, atHome, hittingBall
    }
    public BatAIState batAIState;

    private Vector3 homePosition;

    public static bool AISetBall { get; internal set; }

    public static float ballHitSpeed { get; internal set; }

    private int AIDifficulty;

    public void updateDifficulty(int newDifficulty)
    {
        AIDifficulty = newDifficulty;

        if (newDifficulty == 0) //Easy
        {
            aiSpeed = 150;
            ballHitSpeed = 75;
        }
        else if (newDifficulty == 1) //Medium
        {
            aiSpeed = 200;
            ballHitSpeed = 90;
        }
        else if (newDifficulty == 2) //Hard
        {
            aiSpeed = 300;
            ballHitSpeed = 150;
        }
        // Else, leave them at what they were (for now)
    }

    private Transform oppoGoalTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        batAIState = BatAIState.hittingBall;
        //GameUtils.playState = GameUtils.GamePlayState.SettingBall;
        oppoGoalTransform = GameObject.FindGameObjectWithTag("SouthGoal").transform;
        ballHitSpeed = 50;
        aiSpeed = 75;
        homePosition = new Vector3(0, 4.5f, 128f);
    }

    // Update is called once per frame
    void Update()
    {
        //It might be our serve, but either way, we want to be by our goal when it happens
        if (ShowdownManager.currentGameState == ShowdownManager.ShowdownGameState.SettingBall || ShowdownManager.currentGameState == ShowdownManager.ShowdownGameState.GoalScored)
        {
            GoHome();
        }
        else if (ShowdownManager.currentGameState == ShowdownManager.ShowdownGameState.BallSet)
        {
            hitPosition = GameObject.FindGameObjectWithTag("Ball").transform.position;
        }
        else if (ShowdownManager.currentGameState == ShowdownManager.ShowdownGameState.BallInPlay)
        {
            if (batAIState == BatAIState.hittingBall)
            {
                HitBall();
            }
            else if (batAIState == BatAIState.atHitPosition)
            {
                GoHome();
            }
            else if (batAIState == BatAIState.atHome && AIColliderScript.ballInZone)
            {
                hitPosition = GameObject.FindGameObjectWithTag("Ball").transform.position;
                batAIState = BatAIState.hittingBall;
            }
        }

    }

    /// <summary>
    /// Sends the AI bat back to the home position after hitting a ball
    /// </summary>
    private void GoHome()
    {
        // Not sure why, TODO: to ask why only level 1 has a different speed modifier here, but for now, I'll leave it

        if (AIDifficulty == 1)
        {
            rb.position = Vector3.MoveTowards(transform.position, new Vector3(0, 4.5f, 128f), aiSpeed / 1.2f * Time.deltaTime);
        }
        else
        {
            rb.position = Vector3.MoveTowards(transform.position, new Vector3(0, 4.5f, 128f), aiSpeed * 1.5f * Time.deltaTime);
        }

        if (rb.position == homePosition)
        {
            batAIState = BatAIState.atHome;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ball")
        {

            batAIState = BatAIState.atHitPosition;
            var inverseBallSpeed = Mathf.Min(Mathf.Max(Math.Abs(collision.rigidbody.velocity.z), ballHitSpeed), 100);

            if (collision.rigidbody.velocity.z > 0)
            {
                inverseBallSpeed *= -1;
            }

            var rbVelocityInverse = new Vector3(collision.rigidbody.velocity.x * -1, 0, inverseBallSpeed * 1.5f * -1);
            collision.rigidbody.velocity = rbVelocityInverse;
        }
    }

    /// <summary>
    /// Sends the AI bat to attempt to hit a ball
    /// </summary>
    private void HitBall()
    {
        // The AI should only move to hit the ball when it's on their side. If it's over on the opponents side, they shouldn't move.
        if (this.rb.position.z > 0)
        {
            float ballX = hitPosition.x,
                  ballZ = hitPosition.z;
            if (ballX != transform.position.x || ballZ != transform.position.z)
            {
                rb.position = Vector3.MoveTowards(transform.position,
                            new Vector3(ballX, transform.position.y, ballZ),
                            aiSpeed * Time.deltaTime);
                transform.LookAt(oppoGoalTransform);
            }
            else
            {
                batAIState = BatAIState.atHitPosition;
            }
        }
    }

}
