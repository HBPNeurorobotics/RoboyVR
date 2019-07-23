namespace Locomotion
{
    using System.Collections.Generic;
    using UnityEngine;
    using Utils;

    public class LocomotionHover : ILocomotionBehaviour
    {
        private const float fixedUpdateRefreshRate = 60;
        private readonly List<GameObject> hoverObjects = new List<GameObject>();

        private readonly float maxMovementSpeedInMPerS =
            SteamVRControllerInput.Instance.SpeedInMPerS;

        private readonly float maxMovementSpeedPerFrame;

        public LocomotionHover()
        {
            maxMovementSpeedPerFrame = maxMovementSpeedInMPerS / fixedUpdateRefreshRate;
            initializeHover();
        }

        public void moveForward()
        {
            AudioManager.Instance.startHovering();
            ParticleManager.Instance.startJets();
            translateForwardHip();
        }

        public void stopMoving()
        {
            AudioManager.Instance.stopHovering();
            ParticleManager.Instance.stopJets();
        }

        public void reset()
        {
            foreach (var hoverObject in hoverObjects) hoverObject.SetActive(false);
        }

        private void initializeHover()
        {
            foreach (var hoverObject in GameObject.FindGameObjectsWithTag("hover"))
            foreach (Transform child in hoverObject.transform)
                hoverObjects.Add(child.gameObject);
            foreach (var hoverObject in hoverObjects) hoverObject.SetActive(true);
        }

        private void translateForwardHip()
        {
            SteamVRControllerInput.Instance.transform.Translate(
                getMoveDirection() * maxMovementSpeedPerFrame);
        }

        private static Vector3 getMoveDirection()
        {
            return Vector3.ProjectOnPlane(VrLocomotionTrackers.Instance.HipTracker.up, Vector3.up)
                .normalized;
        }

        private void translateForwardController()
        {
            var areaToMove = SteamVRControllerInput.Instance.transform;
            var rightControllerForward = SteamVRControllerInput.Instance
                .RightControllerObject
                .transform.forward;
            var leftControllerForward = SteamVRControllerInput.Instance
                .LeftControllerObject
                .transform.forward;
            areaToMove.Translate(clampForwardVectorsToXZPlane(
                                     rightControllerForward, leftControllerForward) *
                                 SteamVRControllerInput.Instance.SpeedInMPerS);
        }

        private Vector3 clampForwardVectorsToXZPlane(Vector3 forward, Vector3 otherForward)
        {
            var projectForward = Vector3.ProjectOnPlane(forward, Vector3.up);
            var projectOther = Vector3.ProjectOnPlane(otherForward, Vector3.up);
            return (projectForward + projectOther).normalized;
        }
    }
}