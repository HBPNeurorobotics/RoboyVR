using UnityEngine;

public class VrLocomotionTrackers : Singleton<VrLocomotionTrackers>
{
    [SerializeField] private Transform _hipTracker;
    [SerializeField] private Transform _leftFootTracker;
    [SerializeField] private Transform _rightFootTracker;
    [SerializeField] bool _shouldShowAxis = false;
    private Vector3 _trackingPlane;
    private float defaultDistance;

    private Transform LeftFootTracker
    {
        get { return _leftFootTracker; }
    }

    private Transform RightFootTracker
    {
        get { return _rightFootTracker; }
    }

    public Transform HipTracker
    {
        get { return _hipTracker; }
    }

    public float DistanceTrackersOnPlane
    {
        get { return getDistanceBetweenTrackerOnPlane(_trackingPlane) + defaultDistance; }
    }

    private void Start()
    {
        initializeDefaultDistance();
    }

    private void initializeDefaultDistance()
    {
        defaultDistance =
            getDistanceBetweenTrackerOnPlane(createTrackingPlaneNormalBetweenTrackers());
    }

    private void initializeTrackerRotation(Transform tracker)
    {
        Vector3 upVectorTracker = Vector3.ProjectOnPlane(tracker.up, tracker.forward).normalized;
        Vector3 upVectorGeneral = Vector3.ProjectOnPlane(Vector3.up, tracker.forward).normalized;
        float degreesToRotate = Vector3.Angle(upVectorTracker, upVectorGeneral);
        tracker.Rotate(tracker.forward, degreesToRotate);
    }

    public void initializeTracking()
    {
        initializeDefaultDistance();
        initializeTrackerRotation(_leftFootTracker);
        initializeTrackerRotation(_rightFootTracker);
        initializeTrackerRotation(_hipTracker);
    }

    private void Update()
    {
        _trackingPlane = createTrackingPlaneNormalBetweenTrackers();
        Debug.DrawRay(Vector3.zero, _trackingPlane);
        if(_shouldShowAxis)
            showAxisForTrackers();
    }

    private float getDistanceBetweenTrackerOnPlane(Vector3 trackingPlaneNormal)
    {
        var left = Vector3.ProjectOnPlane(LeftFootTracker.position, trackingPlaneNormal);
        var right = Vector3.ProjectOnPlane(RightFootTracker.position, trackingPlaneNormal);
        return Vector3.Distance(right, left);
    }

    private Vector3 createTrackingPlaneNormalBetweenTrackers()
    {
        var directionRightToLeft = LeftFootTracker.position - RightFootTracker.position;
        var directionRightToLeftOnPlane = Vector3.ProjectOnPlane(directionRightToLeft, Vector3.up);
        return directionRightToLeftOnPlane.normalized;
    }

    public static void showAxisForTrackers()
    {
        Debug.DrawRay(Instance.RightFootTracker.transform.position,
            Instance.RightFootTracker.transform.forward, Color.green);
        Debug.DrawRay(Instance.LeftFootTracker.transform.position,
            Instance.LeftFootTracker.transform.forward, Color.green);
        Debug.DrawRay(Instance.HipTracker.transform.position,
            Instance.HipTracker.transform.forward, Color.green);
        Debug.DrawRay(Instance.RightFootTracker.transform.position,
            Instance.RightFootTracker.transform.up, Color.red);
        Debug.DrawRay(Instance.LeftFootTracker.transform.position,
            Instance.LeftFootTracker.transform.up, Color.red);
        Debug.DrawRay(Instance.HipTracker.transform.position,
            Instance.HipTracker.transform.up, Color.red);
    }
}