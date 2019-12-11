namespace Locomotion
{
    using System.Collections.Generic;
    using UnityEngine;
    using Utils;

    public class LocomotionHover : ILocomotionBehaviour
    {
        private readonly List<GameObject> hoverObjects = new List<GameObject>();

        public LocomotionHover()
        {
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
            Transform localPlayArea = SteamVRControllerInput.Instance.transform;
            localPlayArea.Translate(
                getMoveDirection() * SteamVRControllerInput.Instance.SpeedPerFrame);
        }

        private static Vector3 getMoveDirection()
        {
            return Vector3.ProjectOnPlane(VrLocomotionTrackers.Instance.HipTracker.up, Vector3.up);
        }

        private Vector3 clampForwardVectorsToXZPlane(Vector3 forward, Vector3 otherForward)
        {
            var projectForward = Vector3.ProjectOnPlane(forward, Vector3.up);
            var projectOther = Vector3.ProjectOnPlane(otherForward, Vector3.up);
            return (projectForward + projectOther).normalized;
        }
    }
}