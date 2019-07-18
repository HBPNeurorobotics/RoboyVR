namespace Locomotion
{
    using System;
    using UnityEngine;
    using Utils;

    public class LocomotionTracker : ILocomotionBehaviour
    {
        private const float _maxMovementSpeedInMPerS = 7;
        private const float _fixedUpdateRefreshRate = 60;
        private const float _epsilonForMovementRegistration = 0.05f;
        private const float _maximalStepLength = 2f;

        private const float _maxMovementSpeedPerFrame =
            _maxMovementSpeedInMPerS / _fixedUpdateRefreshRate;

        private const float _sawSlowingOfMovementPerFrame = 0.002f;
        private const int noMovementDistance = 0;
        private float _currentMovementSpeedPerFrame = _maxMovementSpeedPerFrame;
        private bool makingStep;
        private float CurrentStepLength { get; set; }

        public void moveForward()
        {
            SteamVRControllerInput.Instance.transform.Translate(
                getMoveDirection() * calculateMovementDistancePerFrame());
            if (Math.Abs(calculateMovementDistancePerFrame() - noMovementDistance) < 0.01)
                SteamVRControllerInput.Instance.checkIfMovementStopped(1,
                    calculateMovementDistancePerFrame);
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
                makingStep = true;
                CurrentStepLength += _currentMovementSpeedPerFrame;
                return _currentMovementSpeedPerFrame -= _sawSlowingOfMovementPerFrame;
            }

            return noMovementReset();
        }

        private float noMovementReset()
        {
            if (!(VrLocomotionTrackers.Instance.DistanceTrackersOnPlane <
                  _epsilonForMovementRegistration)) return noMovementDistance;
            CurrentStepLength = 0;
            _currentMovementSpeedPerFrame = _maxMovementSpeedPerFrame;

            if (makingStep)
            {
                AudioManager.Instance.playFootStep();
                makingStep = false;
            }

            return noMovementDistance;
        }

        private bool isValidMovement(float epsilon)
        {
            return VrLocomotionTrackers.Instance.DistanceTrackersOnPlane >= epsilon &&
                   CurrentStepLength <= _maximalStepLength && _currentMovementSpeedPerFrame >= 0;
        }
    }
}