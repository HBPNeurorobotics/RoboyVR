using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class HapticsHand : MonoBehaviour {

    public SteamVR_Input_Sources handType;

    public SteamVR_Action_Vibration hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");

    [SerializeField] public AssignMeshCollider assign;

    private bool initialized = false;

    private GameObject box1;
    private GameObject box2;
    private GameObject box3;
    private GameObject box4;

    private GameObject[] boxArray;

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if (UserAvatarService.Instance.IsRemoteAvatarPresent && !initialized)
        {
            box1 = GameObject.Find("box_0_0::link::collision__COLLISION_VISUAL__/Cube");
            box2 = GameObject.Find("box_0_0_0::link::collision__COLLISION_VISUAL__/Cube");
            box3 = GameObject.Find("box_0_0_0_0_0::link::collision__COLLISION_VISUAL__/Cube");
            box4 = GameObject.Find("box_0_0_0_0_0_0::link::collision__COLLISION_VISUAL__/Cube");

            boxArray = new GameObject[] { box1, box2, box3, box4 };

            initialized = true;
        }
        if(initialized && assign.GetInitalized())
        {
            for (int i = 0; i < boxArray.Length; i++)
            {
                if (boxArray[i].GetComponent<CollisionHandler>().GetVibrate())
                {
                    if (this.handType.ToString() == boxArray[i].GetComponent<CollisionHandler>().GetSide())
                    {
                        TriggerHapticPulse(0.2f, 10f, 30f);
                        break;
                    }
                }
            }
        }
	}

    public void TriggerHapticPulse(float duration, float frequency, float amplitude)
    {
        hapticAction.Execute(0, duration, frequency, amplitude, handType);
    }
}
