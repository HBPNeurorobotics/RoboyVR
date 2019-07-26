namespace Locomotion
{
    using System;
    using UnityEngine;

    public class VrLocomotionTrackers : Singleton<VrLocomotionTrackers>
    {
        private float distanceBetweenFeet;
        [SerializeField] private Transform hipTracker;
        [SerializeField] private Transform leftFootTracker;
        [SerializeField] private Transform rightFootTracker;
        [SerializeField] private bool shouldShowAxis;

        private Vector3 trackingPlane;

        private Transform LeftFootTracker
        {
            get { return leftFootTracker; }
        }

        private Transform RightFootTracker
        {
            get { return rightFootTracker; }
        }

        public Transform HipTracker
        {
            get { return hipTracker; }
        }

        public float DistanceTrackersOnPlane
        {
            get { return getDistanceBetweenTrackerOn(trackingPlane); }
        }

        private void Start()
        {
            initializeFeetDistance();
        }

        public void initializeFeetDistance()
        {
            distanceBetweenFeet =
                getDistanceBetweenTrackerOn(createTrackingPlaneNormal());
            var rightFootPosition = RightFootTracker.position;
            var leftFootPosition = LeftFootTracker.position;
            if (rightFootPosition.y >= leftFootPosition.y)
                RightFootTracker.position = moveDownFootTrackerFrom(rightFootPosition);
            else
                LeftFootTracker.position = moveDownFootTrackerFrom(leftFootPosition);
        }

        private Vector3 moveDownFootTrackerFrom(Vector3 footPosition)
        {
            return new Vector3(footPosition.x, footPosition.y - distanceBetweenFeet,
                footPosition.z);
        }

        private void initializeTrackerRotation(Transform tracker, Vector3 axis)
        {
            var upVectorTracker = Vector3.ProjectOnPlane(tracker.up, axis).normalized;
            var upVectorGeneral = Vector3.ProjectOnPlane(Vector3.up, axis).normalized;
            var degreesTotal = Vector3.SignedAngle(upVectorTracker, upVectorGeneral, axis);
            var directionToRotate = degreesTotal / Math.Abs(degreesTotal);
            if (degreesTotal >= 4f || degreesTotal <= -4f) tracker.Rotate(axis, directionToRotate, Space.World);
        }

        public void initializeTrackerOrientation()
        {
            initializeTrackerRotation(LeftFootTracker, LeftFootTracker.forward);
            initializeTrackerRotation(RightFootTracker, RightFootTracker.forward);
            initializeTrackerRotation(HipTracker, HipTracker.forward);
        }

        public void initializeTrackerHeading()
        {
            initializeTrackerRotation(HipTracker, HipTracker.right);
            initializeFeetDistance();
        }

        private void Update()
        {
            trackingPlane = createTrackingPlaneNormal();
            Debug.DrawRay(Vector3.zero, trackingPlane);
            if (shouldShowAxis)
                showAxisForTrackers();
        }

        private float getDistanceBetweenTrackerOn(Vector3 trackingPlaneNormal)
        {
            var left = Vector3.ProjectOnPlane(LeftFootTracker.position, trackingPlaneNormal);
            var right = Vector3.ProjectOnPlane(RightFootTracker.position, trackingPlaneNormal);
            return Vector3.Distance(right, left);
        }

        private Vector3 createTrackingPlaneNormal()
        {
            var directionRightToLeft = LeftFootTracker.position - RightFootTracker.position;
            var directionRightToLeftOnPlane =
                Vector3.ProjectOnPlane(directionRightToLeft, Vector3.up);
            return directionRightToLeftOnPlane.normalized;
        }


        public static void showAxisForTrackers()
        {
            var rightFootTransform1 = Instance.RightFootTracker.transform;
            var leftFootTransform = Instance.LeftFootTracker.transform;
            var hipTransform = Instance.HipTracker.transform;

            Debug.DrawRay(leftFootTransform.position,
                leftFootTransform.forward, Color.green);
            Debug.DrawRay(rightFootTransform1.position,
                rightFootTransform1.forward, Color.green);
            Debug.DrawRay(hipTransform.position,
                hipTransform.forward, Color.green);
            Debug.DrawRay(rightFootTransform1.position,
                rightFootTransform1.up, Color.red);
            Debug.DrawRay(leftFootTransform.position,
                leftFootTransform.up, Color.red);
            Debug.DrawRay(hipTransform.position,
                hipTransform.up, Color.red);
        }
    }
}