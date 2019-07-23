using Locomotion;
using UnityEngine;

public class dummyController : MonoBehaviour
{
    private void Update()
    {
        initializeTrackerFromKeyboard();
    }

    private static void initializeTrackerFromKeyboard()
    {
        if (Input.GetKey(KeyCode.Alpha2))
            VrLocomotionTrackers.Instance.initializeTrackerOrientation();
        if (Input.GetKey(KeyCode.Alpha1))
            VrLocomotionTrackers.Instance.initializeFeetDistance();

        if (Input.GetKey(KeyCode.Alpha3))
            VrLocomotionTrackers.Instance.initializeTrackerHeading();
    }
}