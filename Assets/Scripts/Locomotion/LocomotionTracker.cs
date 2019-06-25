namespace Locomotion
{
    using UnityEngine;

    public class LocomotionTracker : ILocomotionBehaviour
    {
        private const float _maxMovementSpeedInMPerS = 4;
        private const float _fixedUpdateRefreshRate = 60;
        private const float _epsilonForMovementRegistration = 0.05f;
        private const float _maximalStepLength = 0.5f;
        private const float _maxMovementSpeedPerFrame = _maxMovementSpeedInMPerS / _fixedUpdateRefreshRate;
        private float _currentMovementSpeedPerFrame = _maxMovementSpeedPerFrame;
        private const float _sawSlowingOfMovementPerFrame = 0.01f;
        private float CurrentStepLength { get; set; }

        public void moveForward()
        {
            SteamVRControllerInput.Instance.transform.Translate(
                getMoveDirection() * calculateMovementDistancePerFrame(_epsilonForMovementRegistration));
        }

        private static Vector3 getMoveDirection()
        {
            return Vector3.ProjectOnPlane(VrLocomotionTrackers.Instance.HipTracker.forward, Vector3.up).normalized;
        }

        private float calculateMovementDistancePerFrame(float epsilon)
        {
            if (isValidMovement(epsilon))
            {
                CurrentStepLength -= _currentMovementSpeedPerFrame;
                return _currentMovementSpeedPerFrame -= _sawSlowingOfMovementPerFrame;
            }
            else if (VrLocomotionTrackers.Instance.DistanceTrackersOnPlane < epsilon)
            {
                CurrentStepLength = 0;
                _currentMovementSpeedPerFrame = _maxMovementSpeedPerFrame; 
            }
            return 0;
        }

        private bool isValidMovement(float epsilon)
        {
            return VrLocomotionTrackers.Instance.DistanceTrackersOnPlane >= epsilon && CurrentStepLength <= _maximalStepLength && _currentMovementSpeedPerFrame >= 0;
        }

        public void stopMoving()
        {
            
        }

        public void reset()
        {
            
        }
    }
}