namespace Locomotion
{
    using System;
    using UnityEngine;

    public class LocomotionTracker : ILocomotionBehaviour
    {
        float _movementSpeedInMperS;
        readonly float _fixedUpdateRefreshRate = 60; 
        readonly float _epsilonForMovementRegistration = 0.015f;
        readonly float _maximalStepLength = 1;
        private float _currentStepLength = 0;
        public float currentStepLength { get { return _currentStepLength; } set { _currentStepLength = value; } }

        public void moveForward()
        {
            SteamVRControllerInput.Instance.transform.Translate(getMoveDirection() * calculateMovement(_epsilonForMovementRegistration, _movementSpeedInMperS));
        }

        private static Vector3 getMoveDirection()
        {
            return Vector3.ProjectOnPlane(VRLocomotionTrackers.Instance.HipTracker.forward, Vector3.up);
        }

        private float calculateMovement(float epsilon, float speed)
        {
            if (VRLocomotionTrackers.Instance.DistanceTrackersOnPlane >= epsilon)
                return calculateMovementDistance(speed);
            else
                currentStepLength = 0;
            return currentStepLength;
        }

        private float calculateMovementDistance(float speed)
        {
            if (_currentStepLength >= _maximalStepLength)
                return 0;
            else
                return speed / _fixedUpdateRefreshRate; 
        }
    }
}