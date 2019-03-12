using UnityEngine;
using System.Collections;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using System.Collections.Generic;
using Joint = UnityEngine.Joint;

public class BodySourceManager : MonoBehaviour
{
    public static BodySourceManager Instance;

    private KinectSensor _sensor;
    private BodyFrameReader _reader;
    private Body[] _data;
    private FaceFrameResult[] _faceData;
    private FaceFrameSource[] _faceFrameSources;
    private FaceFrameReader[] _faceFrameReaders;
	private List<ulong> _knownBodyIds = new List<ulong>();
    private Dictionary<JointType, JointType> _BoneMap = new Dictionary<JointType, JointType>()
    {
        { JointType.FootLeft, JointType.AnkleLeft },
        { JointType.AnkleLeft, JointType.KneeLeft },
        { JointType.KneeLeft, JointType.HipLeft },
        { JointType.HipLeft, JointType.SpineBase },

        { JointType.FootRight, JointType.AnkleRight },
        { JointType.AnkleRight, JointType.KneeRight },
        { JointType.KneeRight, JointType.HipRight },
        { JointType.HipRight, JointType.SpineBase },

        { JointType.HandTipLeft, JointType.HandLeft },
        { JointType.ThumbLeft, JointType.HandLeft },
        { JointType.HandLeft, JointType.WristLeft },
        { JointType.WristLeft, JointType.ElbowLeft },
        { JointType.ElbowLeft, JointType.ShoulderLeft },
        { JointType.ShoulderLeft, JointType.SpineShoulder },

        { JointType.HandTipRight, JointType.HandRight },
        { JointType.ThumbRight, JointType.HandRight },
        { JointType.HandRight, JointType.WristRight },
        { JointType.WristRight, JointType.ElbowRight },
        { JointType.ElbowRight, JointType.ShoulderRight },
        { JointType.ShoulderRight, JointType.SpineShoulder },

        { JointType.SpineBase, JointType.SpineMid },
        { JointType.SpineMid, JointType.SpineShoulder },
        { JointType.SpineShoulder, JointType.Neck },
        { JointType.Neck, JointType.Head },
    };

    private const double FaceRotationIncrementInDegrees = 0.01;

    public CameraSpacePoint handPosition;
    public CameraSpacePoint wristPosition;
    public CameraSpacePoint baseKinectPosition;
    public CameraSpacePoint headPosition;
    public CameraSpacePoint closestZPoint;
    public float maxZDistance;
    public Quaternion faceRotation;
    public bool bodyFound;
    
    public int BodyCount
    {
        get
        {
            return _sensor.BodyFrameSource.BodyCount;
        }
    }

    public bool MultipleBodiesDetected
    {
        get { return BodyCount > 1; }
    }

    public ulong[] TrackedBodyIds
    {
        get { return _knownBodyIds.ToArray(); }
    }

    public Body[] Bodies
    {
        get { return _data; }
    }

    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
        }

        Instance = this;
        DontDestroyOnLoad(this);
        
        _sensor = KinectSensor.GetDefault();
        if (_sensor == null)
        {
            // TODO: Handle this more gracefully
            Debug.LogError("Kinect not found!");
            return;
        }
        
        _reader = _sensor.BodyFrameSource.OpenReader();
        // If the Kinect isn't active, make it so
        if (!_sensor.IsOpen)
        {
            _sensor.Open();
        }
        
        this._faceFrameSources = new FaceFrameSource[BodyCount];
        this._faceFrameReaders = new FaceFrameReader[BodyCount];

        FaceFrameFeatures faceFrameFeatures =
            FaceFrameFeatures.RotationOrientation
            | FaceFrameFeatures.FaceEngagement
            | FaceFrameFeatures.LookingAway;

        for (int i = 0; i < BodyCount; i++)
        {
            // create the face frame source with the required face frame features and an initial tracking Id of 0
            _faceFrameSources[i] = FaceFrameSource.Create(_sensor, 0, faceFrameFeatures);
            
            // open the corresponding reader
            _faceFrameReaders[i] = _faceFrameSources[i].OpenReader();
        }
    }

    void Update()
    {
        if (_reader == null)
        {
            // Debug.LogError("Body frame reader is null!");
            return;
        }

        var frame = _reader.AcquireLatestFrame();
        if (frame == null)
        {
            // TODO: Log if this fails?
            return;
        }

        if (_data == null)
        {
            _data = new Body[BodyCount];
            _faceData = new FaceFrameResult[BodyCount];
        }
        
        frame.GetAndRefreshBodyData(_data);
        List<FaceFrameResult> res = new List<FaceFrameResult>();
        // iterate through each body and update face source
        for (int i = 0; i < BodyCount; i++)
        {
            // check if a valid face is tracked in this face source
            if (_faceFrameSources[i].IsTrackingIdValid)
            {
                using (FaceFrame faceFrame = _faceFrameReaders[i].AcquireLatestFrame())
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
                // Check if the corresponding body is tracked
                if (_data[i].IsTracked)
                {
                    // update the face frame source to track this body
                    _faceFrameSources[i].TrackingId = _data[i].TrackingId;
                }
            }
        }

        if (res.Count > 0)
        {
            _faceData = res.ToArray();
        }
        
        frame.Dispose();
        frame = null;
        
        bodyFound = false;

        // Check if we've seen anything
        if (_data == null)
        {
            return;
        }

        // If we have a face, get the rotation
        if (_faceData.Length > 0 && _faceData[0] != null)
        {
            faceRotation = new Quaternion(_faceData[0].FaceRotationQuaternion.X, _faceData[0].FaceRotationQuaternion.Y, _faceData[0].FaceRotationQuaternion.Z, _faceData[0].FaceRotationQuaternion.W);
        }
        
        // Get a list of all actively tracked body IDs
        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in _data)
        {
            if (body == null || !body.IsTracked)
            {
                continue;
            }
            trackedIds.Add(body.TrackingId);
        }
        
        // Delete untracked bodies
        List<ulong> knownBodyIdsCopy = new List<ulong>(_knownBodyIds);
        foreach (ulong trackingId in knownBodyIdsCopy)
        {
            if (!trackedIds.Contains(trackingId))
            {
                _knownBodyIds.Remove(trackingId);
            }
        }

        foreach (var body in _data)
        {
            if (body == null || !body.IsTracked)
            {
                continue;
            }

            bodyFound = true;
            if (!_knownBodyIds.Contains(body.TrackingId))
            {
                _knownBodyIds.Add(body.TrackingId);
            }

            // Update the body tracking information
            if (PreferenceManager.Instance.PlayerHandedness == Handedness.Left)
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

            var joints = new JointType[]
            {
                JointType.Head, JointType.Neck, JointType.SpineMid, JointType.SpineShoulder, JointType
                    .HipLeft,
                JointType.HipRight
            };
            
            maxZDistance = Mathf.NegativeInfinity;
            foreach (var jointType in joints)
            {
                maxZDistance = Mathf.Max(maxZDistance, body.Joints[jointType].Position.Z);
            }

            float minZDistance = Mathf.Infinity;
            foreach (var jointType in joints)
            {
                minZDistance = Mathf.Min(minZDistance, body.Joints[jointType].Position.Z);
            }

            baseKinectPosition = new CameraSpacePoint()
            {
                X = body.Joints[JointType.SpineShoulder].Position.X,
                Y = body.Joints[JointType.Head].Position.Y,
                Z = maxZDistance
            };

            closestZPoint = new CameraSpacePoint()
            {
                X = body.Joints[JointType.SpineMid].Position.X,
                Y = body.Joints[JointType.SpineMid].Position.Y,
                Z = minZDistance
            };
        }
    }

    private void OnApplicationQuit()
    {
        if (_reader != null)
        {
            _reader.Dispose();
            _reader = null;
        }

        if (_sensor != null)
        {
            if (_sensor.IsOpen)
            {
                _sensor.Close();
            }

            _sensor = null;
        }
    }
}
