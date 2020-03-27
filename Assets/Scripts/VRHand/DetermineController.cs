using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class DetermineController : Singleton<DetermineController>
{

    public SteamVR_Input_Sources inputSource;

    public SteamVR_Input input;

    //private Dictionary<uint, SteamVR_Input_Sources> dictSteamVRInputSources = new Dictionary<uint, SteamVR_Input_Sources>();

    public bool useIndexController = false;

    // Use this for initialization
    void Start()
    {
        //input.GetLocalizedName(originHandle, VRInputString_ControllerType);
        //SteamVRControllerInput.
        //SteamVR_Input.GetLocalizedName(SteamVR_Input.origin activeOrigin, "VRInputString_ControllerType");
        useIndexController = true;
    }

    // Update is called once per frame
    void Update()
    {

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
