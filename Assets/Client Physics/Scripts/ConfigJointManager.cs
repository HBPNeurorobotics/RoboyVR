using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigJointManager : MonoBehaviour {

    List<HumanBodyBones> noJoint = new List<HumanBodyBones>();
    SoftJointLimit jointLimit = new SoftJointLimit();

    // Use this for initialization
    void Start () {
        noJoint.Add(HumanBodyBones.Hips);
        noJoint.Add(HumanBodyBones.Spine);
        noJoint.Add(HumanBodyBones.UpperChest);
        noJoint.Add(HumanBodyBones.LeftShoulder);
        noJoint.Add(HumanBodyBones.RightShoulder);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void AssignJoint(HumanBodyBones bone, Dictionary<HumanBodyBones, GameObject> dict)
    {
            dict[bone].AddComponent<ConfigurableJoint>();
            
    }

    public void SetupJoints(Dictionary<HumanBodyBones, GameObject> dict)
    {      
        foreach (HumanBodyBones bone in dict.Keys)
        {
            if (!noJoint.Contains(bone))
            {
                AssignJoint(bone, dict);
                SetJoint(bone, dict);
            }
        }
    }

    void SetJoint(HumanBodyBones bone, Dictionary<HumanBodyBones, GameObject> dict)
    {
        if (!noJoint.Contains(bone))
        {
            dict[bone].GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Locked;
            dict[bone].GetComponent<ConfigurableJoint>().yMotion = ConfigurableJointMotion.Locked;
            dict[bone].GetComponent<ConfigurableJoint>().zMotion = ConfigurableJointMotion.Locked;

            dict[bone].GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Limited;
            dict[bone].GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Limited;
            dict[bone].GetComponent<ConfigurableJoint>().angularZMotion = ConfigurableJointMotion.Limited;

            switch (bone)
            {
                #region Left Arm

                case HumanBodyBones.LeftUpperArm:
                    ConfigureJoint(bone, dict, dict[HumanBodyBones.LeftShoulder].GetComponent<Rigidbody>(), new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), -90, 45, 85, 25);
                    break;
                case HumanBodyBones.LeftLowerArm:
                    ConfigureJoint(bone, dict, dict[HumanBodyBones.LeftUpperArm].GetComponent<Rigidbody>(), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1), -177, 0, 15, 15);
                    break;
                case HumanBodyBones.LeftHand:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftLowerArm].GetComponent<Rigidbody>();
                    break;
                #region Left Hand
                //Left Thumb
                case HumanBodyBones.LeftThumbProximal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftThumbIntermediate:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftThumbProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftThumbDistal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftIndexIntermediate].GetComponent<Rigidbody>();
                    break;

                //Left Index Finger
                case HumanBodyBones.LeftIndexProximal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftIndexIntermediate:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftIndexProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftIndexDistal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftIndexIntermediate].GetComponent<Rigidbody>();
                    break;

                //Left Middle Finger
                case HumanBodyBones.LeftMiddleProximal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftMiddleIntermediate:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftMiddleProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftMiddleDistal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftMiddleIntermediate].GetComponent<Rigidbody>();
                    break;

                //Left Ring Finger
                case HumanBodyBones.LeftRingProximal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftRingIntermediate:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftRingProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftRingDistal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftRingIntermediate].GetComponent<Rigidbody>();
                    break;

                //Left Little Finger
                case HumanBodyBones.LeftLittleProximal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftLittleIntermediate:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftLittleProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftLittleDistal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.LeftLittleIntermediate].GetComponent<Rigidbody>();
                    break;
                #endregion
                #endregion

                #region Right Arm

                case HumanBodyBones.RightUpperArm:
                    ConfigureJoint(bone, dict, dict[HumanBodyBones.RightShoulder].GetComponent<Rigidbody>(), new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), -45, 90, 85, 25);

                    break;
                case HumanBodyBones.RightLowerArm:
                    ConfigureJoint(bone, dict, dict[HumanBodyBones.RightUpperArm].GetComponent<Rigidbody>(), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1), 0, 177, 15, 15);
                    break;
                case HumanBodyBones.RightHand:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightLowerArm].GetComponent<Rigidbody>();
                    break;

                #region Right Hand

                //Right Thumb
                case HumanBodyBones.RightThumbProximal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightThumbIntermediate:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightThumbProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightThumbDistal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightIndexIntermediate].GetComponent<Rigidbody>();
                    break;

                //Right Index Finger
                case HumanBodyBones.RightIndexProximal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightIndexIntermediate:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightIndexProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightIndexDistal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightIndexIntermediate].GetComponent<Rigidbody>();
                    break;

                //Right Middle Finger
                case HumanBodyBones.RightMiddleProximal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightMiddleIntermediate:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightMiddleProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightMiddleDistal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightMiddleIntermediate].GetComponent<Rigidbody>();
                    break;

                //Right Ring Finger
                case HumanBodyBones.RightRingProximal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightRingIntermediate:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightRingProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightRingDistal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightRingIntermediate].GetComponent<Rigidbody>();
                    break;

                //Right Little Finger
                case HumanBodyBones.RightLittleProximal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightLittleIntermediate:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightLittleProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightLittleDistal:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.RightLittleIntermediate].GetComponent<Rigidbody>();
                    break;
                #endregion
                #endregion

                #region Torso

                case HumanBodyBones.LeftShoulder:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.UpperChest].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightShoulder:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.UpperChest].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.Neck:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.UpperChest].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.Head:
                    ConfigureJoint(bone, dict, dict[HumanBodyBones.Neck].GetComponent<Rigidbody>(), new Vector3(0, -0.05f, 0), new Vector3(1, 0, 0), new Vector3(0,0,1), -75, 75, 25, 25);
                    break;
                case HumanBodyBones.UpperChest:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.Chest].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.Chest:
                    dict[bone].GetComponent<ConfigurableJoint>().connectedBody = dict[HumanBodyBones.Spine].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.Spine:
                    ConfigureJoint(bone, dict, dict[HumanBodyBones.Hips].GetComponent<Rigidbody>(), new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1), -45, 15, 15, 15);
                    break;

                #endregion

                default: break;
            }
        }
    }

    void ConfigureJoint(HumanBodyBones bone, Dictionary<HumanBodyBones, GameObject> dict, Rigidbody connectedBody, Vector3 anchor, Vector3 axis, Vector3 secondaryAxis, float lowAngularXLimit, float highAngularXLimit, float angularYLimit, float angularZLimit)
    {
        //Connected Body
        dict[bone].GetComponent<ConfigurableJoint>().connectedBody = connectedBody;

        //Anchor
        dict[bone].GetComponent<ConfigurableJoint>().anchor = anchor;

        //Axis
        dict[bone].GetComponent<ConfigurableJoint>().axis = axis;
        //Secondary Axis
        dict[bone].GetComponent<ConfigurableJoint>().secondaryAxis = secondaryAxis;

        //Limits
        jointLimit.limit = lowAngularXLimit;
        dict[bone].GetComponent<ConfigurableJoint>().lowAngularXLimit = jointLimit;
        jointLimit.limit = highAngularXLimit;
        dict[bone].GetComponent<ConfigurableJoint>().highAngularXLimit = jointLimit;
        jointLimit.limit = angularYLimit;
        dict[bone].GetComponent<ConfigurableJoint>().angularYLimit = jointLimit;
        jointLimit.limit = angularZLimit;
        dict[bone].GetComponent<ConfigurableJoint>().angularZLimit = jointLimit;
    }
}
