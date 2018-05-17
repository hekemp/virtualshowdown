﻿using UnityEngine;
using Windows.Kinect;

public class CameraController : MonoBehaviour
{
    public float smooth = 0.01F;
    public static float CameraDeltaZ;
    private float yVelocity = 0.0F;
    private float xVelocity = 0.0F;
    private float zVelocity = 0.0F;
    private float startingZPosition;

    private void Start()
    {
        startingZPosition = -130;
        CameraDeltaZ = 0;
    }

    private void FixedUpdate()
    {
        CameraSpacePoint closestZPoint = BodySourceView.closestZPoint;
        //float furtherestZPosition = BodySourceView.MaxZDistance;
        CameraSpacePoint headPos = BodySourceView.headPosition;
        float centerXPoint, maxZPoint;

        if (GameUtils.playState == GameUtils.GamePlayState.ExpMode)
        {
            centerXPoint = ExpManager.CenterX != 0 ? ExpManager.CenterX : closestZPoint.X;
            maxZPoint = ExpManager.TableEdge != 0 ? ExpManager.TableEdge : BodySourceView.MaxZDistance;
        }
        else
        {
            centerXPoint = SinglePManager.CenterX != 0 ? SinglePManager.CenterX : BodySourceView.closestZPoint.X;
            maxZPoint = SinglePManager.TableEdge != 0 ? SinglePManager.TableEdge : BodySourceView.MaxZDistance;
        }

        CameraDeltaZ = (maxZPoint - closestZPoint.Z) * 100;
        float xDiff = (headPos.X - centerXPoint) * 100;

        Vector3 newPosition = new Vector3(xDiff, transform.position.y, startingZPosition + CameraDeltaZ);
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.fixedDeltaTime * 3);

        Quaternion fr = BodySourceView.faceRotation;
        float yAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, -fr.eulerAngles.y, ref yVelocity, smooth);
        float xAngle = Mathf.SmoothDampAngle(transform.eulerAngles.x, -fr.eulerAngles.x, ref xVelocity, smooth);
        float zAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z, fr.eulerAngles.z, ref zVelocity, smooth);

        transform.localRotation = Quaternion.Euler(xAngle, yAngle, zAngle);
    }

}