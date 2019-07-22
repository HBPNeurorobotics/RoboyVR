namespace Locomotion
{
    using UnityEngine;
    using Utils;

    public class LocomotionTracker : ILocomotionBehaviour
    {
        private const float maxMovementSpeedInMPerS = 7;
        private const float fixedUpdateRefreshRate = 60;
        private const float epsilonForMovementRegistration = 0.05f;
        private const float maximalStepLength = 2f;

        private const float maxMovementSpeedPerFrame =
            maxMovementSpeedInMPerS / fixedUpdateRefreshRate;

        private const float sawSlowingOfMovementPerFrame = 0.002f;
        private const int noMovementDistance = 0;
        private float currentMovementSpeedPerFrame = maxMovementSpeedPerFrame;
        private bool makingStep;
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
            if (!isValidMovement(epsilonForMovementRegistration))
                return noMovementReset();

            makingStep = true;
            CurrentStepLength += currentMovementSpeedPerFrame;
            return currentMovementSpeedPerFrame -= sawSlowingOfMovementPerFrame;
        }

        private float noMovementReset()
        {
            if (!(VrLocomotionTrackers.Instance.DistanceTrackersOnPlane <
                  epsilonForMovementRegistration)) return noMovementDistance;
            CurrentStepLength = 0;
            currentMovementSpeedPerFrame = maxMovementSpeedPerFrame;

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
                   CurrentStepLength <= maximalStepLength && currentMovementSpeedPerFrame >= 0;
        }
    }
}