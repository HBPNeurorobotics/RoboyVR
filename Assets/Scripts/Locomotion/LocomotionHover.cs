namespace Locomotion
{
    using UnityEngine;

    public class LocomotionHover : ILocomotionBehaviour
    {
        System.Collections.Generic.List<GameObject> hoverObjects = new System.Collections.Generic.List<GameObject>();

        float speed = 0.01f;
        public LocomotionHover()
        {
            initializeHover();
        }

        protected virtual void initializeHover()
        {
            foreach (GameObject hoverObject in GameObject.FindGameObjectsWithTag("hover"))
            {
                foreach (Transform child in hoverObject.transform)
                {
                    hoverObjects.Add(child.gameObject);
                }
            }
            foreach (GameObject hoverObject in hoverObjects)
            {
                hoverObject.SetActive(true);
            }
        }

        public virtual void moveForward()
        {
            Debug.Log("moveForward");
            Utils.AudioManager.Instance.startHovering();
            Utils.ParticleManager.Instance.startJets();
            translateForwardHip(speed);
        }

        void translateForwardHip(float translationSpeed)
        {
            SteamVRControllerInput.Instance.transform.Translate(Vector3.ProjectOnPlane(VRLocomotionTrackers.Instance.HipTracker.up, Vector3.up).normalized * speed);
        }

        protected void translateForwardController()
        {
            SteamVRControllerInput.Instance.transform.Translate(clampForwardVectorsToXZPlane(
                            SteamVRControllerInput.Instance.RightControllerObject.transform.forward,
                            SteamVRControllerInput.Instance.LeftControllerObject.transform.forward) * 0.01f);
        }

        Vector3 clampForwardVectorsToXZPlane(Vector3 forward, Vector3 otherForward)
        {
            Vector3 projectForward = Vector3.ProjectOnPlane(forward, Vector3.up);
            Vector3 projectOther = Vector3.ProjectOnPlane(otherForward, Vector3.up);
            return (projectForward + projectOther).normalized;
        }

        public virtual void stopMoving()
        {
            Utils.AudioManager.Instance.stopHovering();
            Utils.ParticleManager.Instance.stopJets();
        }

        public virtual void reset()
        {
            foreach (GameObject hoverObject in hoverObjects)
            {
                hoverObject.SetActive(false);
            }
        }
    }

}