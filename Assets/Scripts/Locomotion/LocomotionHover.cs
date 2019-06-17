﻿namespace Locomotion
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

        public virtual void moveForward()
        {
            Debug.Log("moveForward");
            AudioManager.Instance.startHovering();
            ParticleManager.Instance.startJets();
            translateForwardHip();
        }

        public virtual void stopMoving()
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
                Vector3.ProjectOnPlane(VrLocomotionTrackers.Instance.HipTracker.up, Vector3.up)
                    .normalized * SteamVRControllerInput.Instance.Speed);
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
                                     rightControllerForward,
                                     leftControllerForward) *
                                 SteamVRControllerInput.Instance
                                     .Speed);
        }

        private Vector3 clampForwardVectorsToXZPlane(Vector3 forward, Vector3 otherForward)
        {
            var projectForward = Vector3.ProjectOnPlane(forward, Vector3.up);
            var projectOther = Vector3.ProjectOnPlane(otherForward, Vector3.up);
            return (projectForward + projectOther).normalized;
        }
    }
}