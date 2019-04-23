namespace Locomotion
{
    using UnityEngine;

    public class LocomotionHover : ILocomotionBehaviour
    {
        public void moveForward()
        {
            SteamVRControllerInput.Instance.transform.Translate(clampForwardVectorsToXZPlane(
                SteamVRControllerInput.Instance.RightControllerObject.transform.forward,
                SteamVRControllerInput.Instance.LeftControllerObject.transform.forward) * 0.01f);
        }

        public Vector3 clampForwardVectorsToXZPlane(Vector3 forward, Vector3 otherForward)
        {
            Vector3 projectForward = Vector3.ProjectOnPlane(forward, Vector3.up);
            Vector3 projectOther = Vector3.ProjectOnPlane(otherForward, Vector3.up);
            return (projectForward + projectOther).normalized;
        }
    }

}