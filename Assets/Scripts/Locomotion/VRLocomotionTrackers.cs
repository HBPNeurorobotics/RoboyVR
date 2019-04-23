using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRLocomotionTrackers : Singleton<VRLocomotionTrackers>
{
    [SerializeField] Transform _leftFootTracker;
    public Transform LeftFootTracker { get { return _leftFootTracker; } }
    [SerializeField] Transform _rightFootTracker;
    public Transform RightFootTracker { get { return _rightFootTracker; } }
    [SerializeField] Transform _hipTracker;
    public Transform HipTracker { get { return _hipTracker; } }
    float distanceTrackersOnPlane;

    private void Update()
    {
        Vector3 trackingPlane = createTrackingPlaneBetweenTrackers();
        distanceTrackersOnPlane = getDistanceBetweenTrackerOnPlane(trackingPlane);

    }

    public float getDistanceBetweenTrackerOnPlane(Vector3 trackingPlane)
    {
        Vector3 left = Vector3.ProjectOnPlane(_leftFootTracker.position, trackingPlane);
        Vector3 rigth = Vector3.ProjectOnPlane(_rightFootTracker.position, trackingPlane);
        return Vector3.Distance(rigth, left);
    }

    public Vector3 createTrackingPlaneBetweenTrackers()
    {
        Vector3 directionRightToLeft = _leftFootTracker.position - _rightFootTracker.position;
        return directionRightToLeft.normalized;
    }
}
