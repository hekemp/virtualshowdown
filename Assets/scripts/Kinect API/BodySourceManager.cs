using UnityEngine;
using System.Collections;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using System.Collections.Generic;

public class BodySourceManager : MonoBehaviour
{
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;
    private FaceFrameResult[] _FaceData;
    private FaceFrameSource[] faceFrameSources = null;
    private FaceFrameReader[] faceFrameReaders = null;
    
    public static BodySourceManager Instance;

    public static bool leftyMode;

    public CameraSpacePoint handPosition;
    public CameraSpacePoint wristPosition;
    public CameraSpacePoint baseKinectPosition;
    public CameraSpacePoint headPosition;
    public CameraSpacePoint closestZPoint;
    public float MaxZDistance;

    public Quaternion faceRotation;
    private const double FaceRotationIncrementInDegrees = 0.01;


    public Body[] GetData()
    {
        return _Data;
    }

    public FaceFrameResult[] GetFaceData()
    {
        return _FaceData;
    }

    public bool MultipleBodiesDetected() {
        return _Sensor.BodyFrameSource.BodyCount > 1;
    }

    public bool BodyFound() {
        return _Sensor.BodyFrameSource.BodyCount > 0;
    }


    void Start()
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

        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }


        this.faceFrameSources = new FaceFrameSource[_Sensor.BodyFrameSource.BodyCount];
        this.faceFrameReaders = new FaceFrameReader[_Sensor.BodyFrameSource.BodyCount];
        // specify the required face frame results
        FaceFrameFeatures faceFrameFeatures =
                FaceFrameFeatures.RotationOrientation
                | FaceFrameFeatures.FaceEngagement
                | FaceFrameFeatures.LookingAway;

        for (int i = 0; i < _Sensor.BodyFrameSource.BodyCount; i++)
        {
            // create the face frame source with the required face frame features and an initial tracking Id of 0
            faceFrameSources[i] = FaceFrameSource.Create(_Sensor, 0, faceFrameFeatures);

            // open the corresponding reader
            faceFrameReaders[i] = faceFrameSources[i].OpenReader();
        }
    }

    void Update()
    {
        leftyMode = PreferenceManager.Instance.PlayerHandedness == Handedness.Left;

        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (_Data == null)
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];
                    _FaceData = new FaceFrameResult[_Sensor.BodyFrameSource.BodyCount];
                }
                if (_FaceData != null)
                {
                    faceRotation = new Quaternion(_FaceData[0].FaceRotationQuaternion.X, _FaceData[0].FaceRotationQuaternion.Y, _FaceData[0].FaceRotationQuaternion.Z, _FaceData[0].FaceRotationQuaternion.W);
                }

                RefreshBodyData();
                frame.GetAndRefreshBodyData(_Data);
                //_FaceData = processFaceData(_Sensor.BodyFrameSource.BodyCount);
                List<FaceFrameResult> res = new List<FaceFrameResult>();
                // iterate through each body and update face source
                for (int i = 0; i < _Sensor.BodyFrameSource.BodyCount; i++)
                {
                    // check if a valid face is tracked in this face source				
                    if (faceFrameSources[i].IsTrackingIdValid)
                    {
                        using (FaceFrame faceFrame = faceFrameReaders[i].AcquireLatestFrame())
                        {
                            if (faceFrame != null)
                            {
                                if (faceFrame.TrackingId == 0)
                                {
                                    continue;
                                }

                                // do something with result
                                var result = faceFrame.FaceFrameResult;
                                res.Add(result);
                            }
                        }
                    }
                    else
                    {
                        // check if the corresponding body is tracked 
                        if (_Data[i].IsTracked)
                        {
                            // update the face frame source to track this body
                            faceFrameSources[i].TrackingId = _Data[i].TrackingId;
                        }
                    }
                }
                if (res.Count > 0)
                {
                    _FaceData = res.ToArray();
                }

                frame.Dispose();
                frame = null;
            }
        }
    }

    private void RefreshBodyData() {
        if (_Sensor.BodyFrameSource.BodyCount > 0 && _Data[0] != null) {
            Body body = _Data[0];
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
                Mathf.Max(body.Joints[JointType.Head].Position.Z,
                Mathf.Max(body.Joints[JointType.Head].Position.Z,
                Mathf.Max(body.Joints[JointType.Neck].Position.Z,
                Mathf.Max(body.Joints[JointType.SpineMid].Position.Z,
                Mathf.Max(body.Joints[JointType.SpineShoulder].Position.Z,
                Mathf.Max(body.Joints[JointType.HipLeft].Position.Z,
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
                Mathf.Min(body.Joints[JointType.Head].Position.Z,
                Mathf.Min(body.Joints[JointType.Head].Position.Z,
                Mathf.Min(body.Joints[JointType.Neck].Position.Z,
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
        }
    }

    private FaceFrameResult[] processFaceData(int bodyCount)
    {
        FaceFrameResult[] results = new FaceFrameResult[bodyCount];
        // create a face frame source + reader to track each face in the FOV
        for (int i = 0; i < bodyCount; i++)
        {
            if (faceFrameSources[i].IsTrackingIdValid)
            {
                using (FaceFrame frame = faceFrameReaders[i].AcquireLatestFrame())
                {
                    if (frame != null)
                    {
                        if (frame.TrackingId == 0)
                        {
                            continue;
                        }
                        results[i] = frame.FaceFrameResult;
                    }
                }
            }
        }
        return results;
    }

    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}
