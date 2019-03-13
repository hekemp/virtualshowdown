using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowdownDrillManager : MonoBehaviour {

    public GameObject ourBall;
    private GameObject currentBall;
    private BallScript bs;

	// Use this for initialization
	void Start () {
        currentBall = Instantiate(ourBall, new Vector3(37, 5, 110), new Quaternion());
        bs = currentBall.GetComponent<BallScript>();
        bs.KickBallTowards(new Vector3(-42, 5, -110), 40);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
