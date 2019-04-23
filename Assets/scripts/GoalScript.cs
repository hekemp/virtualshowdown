using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GoalScript : MonoBehaviour {

    public bool isInDrillMode = true;
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
            // this if should be only if drill mode
            if (gameObject.CompareTag("NorthGoal") && other.gameObject.GetComponent<BallScript>() != null)
            {
                if (isInDrillMode)
                {
                    if (other.gameObject.GetComponent<BallScript>().ballHitOnce)
                    {
                        onPlayerGoalScore.Invoke();
                    }
                } else
                {
                    onPlayerGoalScore.Invoke();
                }
                
            }
        }
    }
}
