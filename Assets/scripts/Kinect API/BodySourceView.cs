using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using UnityEngine.UI;
using Microsoft.Kinect.Face;
using System;
using Windows.Kinect;

public class BodySourceView : MonoBehaviour
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;

    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },

        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },

        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };

    private const double FaceRotationIncrementInDegrees = 0.01;

    public static bool leftyMode;

    public static CameraSpacePoint handPosition;
    public static CameraSpacePoint wristPosition;
    public static CameraSpacePoint baseKinectPosition;
    public static CameraSpacePoint headPosition;
    public static CameraSpacePoint closestZPoint;
    public static float MaxZDistance;

    public static Quaternion faceRotation;
    public static bool BodyFound;
    
    public static BodySourceView Instance;

    public void Start()
    {
        // We only ever want 1 copy of this game object!
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
		
        // We want preferences to persist throughout the menus
        DontDestroyOnLoad(this);
		
        Instance = this;
        
        leftyMode = PreferenceManager.Instance.PlayerHandedness == Handedness.Left;

    }

    void Update()
    {
        if (PreferenceManager.Instance.PlayerHandedness != null)
        {
            leftyMode = PreferenceManager.Instance.PlayerHandedness == Handedness.Left;
        }


        if (BodySourceManager == null)
        {
            return;
        }

        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }

        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            BodyFound = false;
            return;
        }

        FaceFrameResult[] faceData = _BodyManager.GetFaceData();

        if (faceData[0] != null)
        {
            faceRotation = new Quaternion(faceData[0].FaceRotationQuaternion.X, faceData[0].FaceRotationQuaternion.Y, faceData[0].FaceRotationQuaternion.Z, faceData[0].FaceRotationQuaternion.W);
        }


        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                trackedIds.Add(body.TrackingId);
            }
        }

        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

        // First delete untracked bodies
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
                BodyFound = false;
            }
        }

        foreach (var body in data)
        {
            if (body == null)
            {
                BodyFound = false;
                continue;
            }

            if (body.IsTracked)
            {
                BodyFound = true;
                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }

                RefreshBodyObject(body, _Bodies[body.TrackingId]);
            }
        }
    }

    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        BodyFound = true;

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);

            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }

        return body;
    }

    private void RefreshBodyObject(Body body, GameObject bodyObject)
    {
        if (leftyMode) //Left handed
        {
            handPosition = body.Joints[JointType.HandTipLeft].Position;
            wristPosition = body.Joints[JointType.HandLeft].Position;
        }
        else
        {
            handPosition = body.Joints[JointType.HandTipRight].Position;
            wristPosition = body.Joints[JointType.HandRight].Position;
        }

        headPosition = body.Joints[JointType.Head].Position;

        MaxZDistance = 
            Math.Max(body.Joints[JointType.Head].Position.Z, 
            Math.Max(body.Joints[JointType.Head].Position.Z, 
            Math.Max(body.Joints[JointType.Neck].Position.Z, 
            Math.Max(body.Joints[JointType.SpineMid].Position.Z, 
            Math.Max(body.Joints[JointType.SpineShoulder].Position.Z, 
            Math.Max(body.Joints[JointType.HipLeft].Position.Z,
                body.Joints[JointType.HipRight].Position.Z))))));

        //float minZBodyDist =
        //   Math.Min(body.Joints[JointType.Head].Position.Z,
        //   Math.Min(body.Joints[JointType.Head].Position.Z,
        //   Math.Min(body.Joints[JointType.Neck].Position.Z,
        //   Math.Min(body.Joints[JointType.SpineMid].Position.Z,
        //   Math.Min(body.Joints[JointType.SpineShoulder].Position.Z,
        //   Math.Min(body.Joints[JointType.HipLeft].Position.Z,
        //       body.Joints[JointType.HipRight].Position.Z))))));

        float minZDistance =
            Math.Min(body.Joints[JointType.Head].Position.Z,
            Math.Min(body.Joints[JointType.Head].Position.Z,
            Math.Min(body.Joints[JointType.Neck].Position.Z,
                body.Joints[JointType.SpineShoulder].Position.Z)));

        baseKinectPosition = new CameraSpacePoint()
        {
            X = body.Joints[JointType.SpineShoulder].Position.X,
            Y = body.Joints[JointType.Head].Position.Y,
            Z = MaxZDistance
        };

        closestZPoint = new CameraSpacePoint()
        {
            X = body.Joints[JointType.SpineMid].Position.X,
            Y = body.Joints[JointType.SpineMid].Position.Y,
            Z = minZDistance
        };

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;

            if (_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }

            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);

            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if (targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lr.SetColors(GetColorForState(sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                lr.enabled = false;
            }
        }
    }

    private static UnityEngine.Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
            case Kinect.TrackingState.Tracked:
                return UnityEngine.Color.green;

            case Kinect.TrackingState.Inferred:
                return UnityEngine.Color.red;

            default:
                return UnityEngine.Color.black;
        }
    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

}
