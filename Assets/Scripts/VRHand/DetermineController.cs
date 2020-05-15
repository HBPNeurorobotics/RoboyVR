using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class DetermineController : Singleton<DetermineController>
{
    private bool useKnucklesController = false;
    private bool determined = false;

    // Use this for initialization
    void Start()
    {

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
}
