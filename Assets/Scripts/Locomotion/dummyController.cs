using Locomotion;
using UnityEngine;

public class dummyController : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
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