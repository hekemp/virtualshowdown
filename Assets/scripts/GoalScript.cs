using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GoalScript : MonoBehaviour {

    public UnityEvent onPlayerGoalScore;
    public UnityEvent onOpponentGoalScore;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            if (gameObject.CompareTag("SouthGoal"))
            {
                onOpponentGoalScore.Invoke();
            }
            if (gameObject.CompareTag("NorthGoal") && other.gameObject.GetComponent<BallScript>().ballHitOnce)
            {
                onPlayerGoalScore.Invoke();
            }
        }
    }
}
