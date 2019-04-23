namespace Locomotion
{
    using UnityEngine;

    public class LocomotionTracker : ILocomotionBehaviour
    {
        public void moveForward()
        {
            SteamVRControllerInput.Instance.transform.Translate(Vector3.ProjectOnPlane(VRLocomotionTrackers.Instance.HipTracker.forward, Vector3.up) * 0.01f);
        }
    }
}