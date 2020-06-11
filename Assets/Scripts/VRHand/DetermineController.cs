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
        StartCoroutine(CheckControllersAvailableCoroutine());
    }

    // Update is called once per frame
    void Update()
    {

    }

    //TODO: turn into async task, sometimes controllers aren't initialized properly (results in unrecognized knuckles controllers)
    public bool UseKnucklesControllers()
    {
        return this.useKnucklesController;
    }

    public bool IsReady()
    {
        return this.determined;
    }

    // transform into System.Threading.Tasks.Task (needs to target NET4.0 first)
    private IEnumerator CheckControllersAvailableCoroutine()
    {
        while (!this.determined)
        {
            string[] controllers = Input.GetJoystickNames();

            bool knucklesControllerLeft = false;
            bool knucklesControllerRight = false;
            bool viveControllerLeft = false;
            bool viveControllerRight = false;

            int numKnucklesControllers = 0;
            int numViveControllers = 0;

            foreach (string controller in controllers)
            {
                //Debug.Log(controller);
                if (controller.IndexOf("OpenVR Controller(Knuckles Left)") != -1)
                {
                    knucklesControllerLeft = true;
                    numKnucklesControllers++;
                }
                else if (controller.IndexOf("OpenVR Controller(Knuckles Right)") != -1)
                {
                    knucklesControllerRight = true;
                    numKnucklesControllers++;
                }
                else if (controller.IndexOf("VIVE Controller") != -1)
                {
                    numViveControllers++;
                }
            }

            if (knucklesControllerLeft && knucklesControllerRight && numKnucklesControllers == 2)
            {
                this.useKnucklesController = true;
                this.determined = true;
                Debug.Log("Coroutine - Using Valve Knuckles Controllers");
            }
            else if (numViveControllers == 2)
            {
                this.useKnucklesController = false;
                this.determined = true;
                Debug.Log("Coroutine - Using HTC Vive Controllers");
            }

            yield return null;
        }
    }
}
