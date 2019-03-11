using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class CameraControllerScript : MonoBehaviour {

    public float smooth = 0.07F;
    public static float CameraDeltaZ;
    private float yVelocity = 0.0F;
    private float xVelocity = 0.0F;
    private float zVelocity = 0.0F;
    private float startingZPosition;

    private Transform _debugCube;

    private void Start()
    {
        startingZPosition = transform.position.z;
        CameraDeltaZ = 0;
    }

    /// <summary>
    /// Moves the position/view of the camera based on where the head of the player is facing
    /// The camera also follows the players body.
    /// Lastly, it goes through a smoothing function because data is quite noisey
    /// </summary>
    private void FixedUpdate()
    {
        CameraSpacePoint closestZPoint = BodySourceView.closestZPoint;
        CameraSpacePoint headPos = BodySourceView.headPosition;

        float centerXPoint = BodySourceView.baseKinectPosition.X != 0 ? BodySourceView.baseKinectPosition.X : closestZPoint.X;
        float maxZPoint = BodySourceView.baseKinectPosition.Z != 0 ? BodySourceView.baseKinectPosition.Z : BodySourceView.MaxZDistance;

        CameraDeltaZ = (maxZPoint - closestZPoint.Z) * 100;
        float xDiff = (headPos.X - centerXPoint) * 100;

        Vector3 newPosition = new Vector3(xDiff, transform.position.y, startingZPosition + CameraDeltaZ);
        transform.position = Vector3.Lerp(transform.position, newPosition, 0.01f);

        Quaternion fr = BodySourceView.faceRotation;
        float yAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, -fr.eulerAngles.y, ref yVelocity, smooth);
        float xAngle = Mathf.SmoothDampAngle(transform.eulerAngles.x, -fr.eulerAngles.x, ref xVelocity, smooth);
        float zAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z, fr.eulerAngles.z, ref zVelocity, smooth);

        transform.localRotation = Quaternion.Euler(xAngle, yAngle, zAngle);
    }

}
