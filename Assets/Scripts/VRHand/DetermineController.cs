using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class DetermineController : Singleton<DetermineController>
{

    //private SteamVR_Input_Sources inputSource;

    //private SteamVR_Input input;

    //private Dictionary<uint, SteamVR_Input_Sources> dictSteamVRInputSources = new Dictionary<uint, SteamVR_Input_Sources>();

    private bool useKnucklesController = false;
    private bool determined = false;

    // Use this for initialization
    void Start()
    {
        //input.GetLocalizedName(originHandle, VRInputString_ControllerType);
        //SteamVRControllerInput.
        //SteamVR_Input.GetLocalizedName(SteamVR_Input.origin activeOrigin, EVRInputStringBits.VRInputString_All);
        //VRInputValueHandle_t inputHandleHandLeft = new VRInputValueHandle_t();
        //SteamVR_Input.GetInputSourceHandle("/user/hand/left", inputHandleHandLeft);
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool UseKnucklesControllers()
    {
        if (this.determined == false)
        {
            string[] controllers = Input.GetJoystickNames();
            bool knucklesControllerLeft = false;
            bool knucklesControllerRight = false;

            foreach (string controller in controllers)
            {
                //Debug.Log(controller);
                if (controller.IndexOf("OpenVR Controller(Knuckles Left)") != -1)
                {
                    knucklesControllerLeft = true;
                }
                else if (controller.IndexOf("OpenVR Controller(Knuckles Right)") != -1)
                {
                    knucklesControllerRight = true;
                }
            }

            if (knucklesControllerLeft && knucklesControllerRight)
            {
                this.useKnucklesController = true;
                Debug.Log("Using Valve Knuckles Controllers");
            }
            else
            {
                this.useKnucklesController = false;
                Debug.Log("Using HTC Vive Controllers");
            }

            this.determined = true;
        }

        return this.useKnucklesController;
    }

    /*
    public void CheckInputSource()
    {
        foreach (KeyValuePair<uint, TrackingReferenceObject> entry in trackingReferences)
        {
            uint deviceIndex = entry.Key;
            TrackingReferenceObject trackingReference = entry.Value;
        }
    }
    */
}
