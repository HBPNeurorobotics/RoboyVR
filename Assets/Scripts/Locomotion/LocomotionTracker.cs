namespace Locomotion
{
    using UnityEngine;
    using Utils;

    public class LocomotionTracker : ILocomotionBehaviour
    {
        private const float _maxMovementSpeedInMPerS = 1;
        private const float _fixedUpdateRefreshRate = 60;
        private const float _epsilonForMovementRegistration = 0.05f;
        private const float _maximalStepLength = 0.5f;

        private const float _maxMovementSpeedPerFrame =
            _maxMovementSpeedInMPerS / _fixedUpdateRefreshRate;

        private const float _sawSlowingOfMovementPerFrame = 0.0005f;
        private float _currentMovementSpeedPerFrame = _maxMovementSpeedPerFrame;
        private float CurrentStepLength { get; set; }

        public void moveForward()
        {
            SteamVRControllerInput.Instance.transform.Translate(
                getMoveDirection() * calculateMovementDistancePerFrame());
        }

        public void stopMoving()
        {
        }

        public void reset()
        {
        }

        private static Vector3 getMoveDirection()
        {
            return Vector3
                .ProjectOnPlane(VrLocomotionTrackers.Instance.HipTracker.forward, Vector3.up)
                .normalized;
        }

        private float calculateMovementDistancePerFrame()
        {
            if (isValidMovement(_epsilonForMovementRegistration))
            {
                CurrentStepLength -= _currentMovementSpeedPerFrame;
                return _currentMovementSpeedPerFrame -= _sawSlowingOfMovementPerFrame;
            }

            return noMovementReset();
        }

        private float noMovementReset()
        {
            const int noMovementDistance = 0;
            if (!(VrLocomotionTrackers.Instance.DistanceTrackersOnPlane <
                  _epsilonForMovementRegistration)) return noMovementDistance;
            CurrentStepLength = 0;
            _currentMovementSpeedPerFrame = _maxMovementSpeedPerFrame;
            AudioManager.Instance.playFootStep();
            return noMovementDistance;
        }

        private bool isValidMovement(float epsilon)
        {
            return VrLocomotionTrackers.Instance.DistanceTrackersOnPlane >= epsilon &&
                   CurrentStepLength <= _maximalStepLength && _currentMovementSpeedPerFrame >= 0;
        }
    }
}