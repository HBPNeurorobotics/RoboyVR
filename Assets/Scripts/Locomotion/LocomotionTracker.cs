namespace Locomotion
{
    using UnityEngine;

    public class LocomotionTracker : ILocomotionBehaviour
    {
        static float _movementSpeedInMPerS = 1;
        static readonly float _fixedUpdateRefreshRate = 60; 
        readonly float _epsilonForMovementRegistration = 0.015f;
        readonly float _maximalStepLength = 0.5f;
        private readonly float _movementSpeedPerFrame = _movementSpeedInMPerS / _fixedUpdateRefreshRate;
        private float CurrentStepLength { get; set; }

        public void moveForward()
        {
            SteamVRControllerInput.Instance.transform.Translate(
                getMoveDirection() * calculateMovementDistancePerFrame(_epsilonForMovementRegistration));
        }

        private static Vector3 getMoveDirection()
        {
            return Vector3.ProjectOnPlane(VrLocomotionTrackers.Instance.HipTracker.up, Vector3.up);
        }

        private float calculateMovementDistancePerFrame(float epsilon)
        {
            if (isValidMovement(epsilon))
                return _movementSpeedPerFrame;
            return CurrentStepLength = 0;
        }

        private bool isValidMovement(float epsilon)
        {
            return VrLocomotionTrackers.Instance.DistanceTrackersOnPlane >= epsilon && CurrentStepLength <= _maximalStepLength;
        }

        public void stopMoving()
        {
            
        }

        public void reset()
        {
            
        }
    }
}