using System;
using UnityEngine;

public class VrLocomotionTrackers : Singleton<VrLocomotionTrackers>
{
    [SerializeField] private Transform _hipTracker;
    [SerializeField] private Transform _leftFootTracker;
    [SerializeField] private Transform _rightFootTracker;
    [SerializeField] private bool _shouldShowAxis;
    private Vector3 _trackingPlane;

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
        get { return getDistanceBetweenTrackerOnPlane(_trackingPlane); }
    }

    private void Start()
    {
        initializeDefaultDistance();
    }

    public void initializeDefaultDistance()
    {
        float distanceBetweenFeet = getDistanceBetweenTrackerOnPlane(createTrackingPlaneNormalBetweenTrackers());
        if (RightFootTracker.position.y >= LeftFootTracker.position.y)
            RightFootTracker.position = new Vector3(RightFootTracker.position.x, RightFootTracker.position.y - distanceBetweenFeet, RightFootTracker.position.z);
        else
            LeftFootTracker.position = new Vector3(LeftFootTracker.position.x, LeftFootTracker.position.y - distanceBetweenFeet, LeftFootTracker.position.z);
    }

    private void initializeTrackerRotation(Transform tracker, Vector3 axis)
    {
        var upVectorTracker = Vector3.ProjectOnPlane(tracker.up, axis).normalized;
        var upVectorGeneral = Vector3.ProjectOnPlane(Vector3.up, axis).normalized;
        var degreesTotal = Vector3.SignedAngle(upVectorTracker, upVectorGeneral, axis);
        var directionToRotate = degreesTotal / Math.Abs(degreesTotal);
        if (degreesTotal >= 4f || degreesTotal <= -4f) ;
        tracker.Rotate(tracker.forward, directionToRotate, Space.World);
    }

    public void initializeTracking()
    {
        initializeTrackerRotation(_leftFootTracker, _leftFootTracker.forward);
        initializeTrackerRotation(_rightFootTracker, _rightFootTracker.forward);
        initializeTrackerRotation(_hipTracker, _hipTracker.forward);
        //initializeTrackerRotation(_hipTracker, Vector3.right);
    }

    private void Update()
    {
        _trackingPlane = createTrackingPlaneNormalBetweenTrackers();
        Debug.DrawRay(Vector3.zero, _trackingPlane);
        if (_shouldShowAxis)
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