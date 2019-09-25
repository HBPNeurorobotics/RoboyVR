using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Applied to the avatar_rig GameObject. Allows us to efficiently track
/// all joint angles without stringly typed shenanigans.
///
/// This thing is lazily initialized and is designed not to rely on the
/// Unity Update cycle. This means you can use it directly after in-
/// stantiating it.
/// </summary>
public class RigAngleTracker : MonoBehaviour
{
    private int _lastTrackedFrame = -1;

    private Dictionary<string, Vector3> _jointToAngles = new Dictionary<string, Vector3>();

    public Dictionary<string, Vector3> GetJointToAngleMapping()
    {
        if (_lastTrackedFrame != Time.frameCount)
        {
            RebuildAngleCache();
            _lastTrackedFrame = Time.frameCount;
        }

        return _jointToAngles;
    }

    private void RebuildAngleCache()
    {
        Transform jointsParent = transform.Find("mixamorig_Hips");

        foreach (Transform child in jointsParent)
        {
            if (child == jointsParent) continue;

            //Quaternion rot_diff = Quaternion.FromToRotation(child.parent.forward, child.forward);
            Quaternion rot_diff = Quaternion.Inverse(child.parent.rotation) * child.rotation;

            Vector3 euler_angles = rot_diff.eulerAngles;
            euler_angles.x = euler_angles.x % 360;
            euler_angles.y = euler_angles.y % 360;
            euler_angles.z = euler_angles.z % 360;

            euler_angles.x = euler_angles.x > 180 ? euler_angles.x - 360 : euler_angles.x;
            euler_angles.y = euler_angles.y > 180 ? euler_angles.y - 360 : euler_angles.y;
            euler_angles.z = euler_angles.z > 180 ? euler_angles.z - 360 : euler_angles.z;

            //Vector3 euler_angles_rad = euler_angles * Mathf.Deg2Rad;

            string joint_name = child.name;
            /* 
             * in case the naming seems confusing, the ...Arm joints are actually placed at the shoulder and the ...Shoulder joints are closer to the spine for the ybot type of rigged model
             * the name of the joint indicates the child limb that is attached to it 
             * => ...Arm is the shoulder joint where the upper arm is attached, ...ForeArm is the elbow joint where the forearm is attached, etc.
             */
            if (child.name.Contains("LeftArm") || child.name.Contains("RightArm"))
            {
                // unfortunately, gazebo doesn't play well with joints having multiple DoFs / rotation axes (i.e. joint type ball, revolute2, universal)
                // revolute2, universal let you set positions but seem to run into gimbal-lock-alike problems for local rotation axes of the joints
                // specifically defining the two axes of rotation to be local y & z seems impossible 
                // we have to split the natural shoulder joint (ball joint) into three individual revolute joints, each covering one axis of rotation for the shoulder

                string joint_x_axis = child.name + "_x";
                string joint_y_axis = child.name + "_y";
                string joint_z_axis = child.name + "_z";

                euler_angles = euler_angles * Mathf.Deg2Rad;

                _jointToAngles[joint_x_axis] = new Vector3(euler_angles.x, 0, 0);
                _jointToAngles[joint_y_axis] = new Vector3(-euler_angles.z, 0, 0);
                _jointToAngles[joint_z_axis] = new Vector3(euler_angles.y, 0, 0);
            }
            else if (child.name.Contains("LeftForeArm") || child.name.Contains("RightForeArm"))
            {
                euler_angles = new Vector3(euler_angles.y, euler_angles.x, euler_angles.z);
                euler_angles = euler_angles * Mathf.Deg2Rad;

                _jointToAngles[joint_name] = new Vector3(euler_angles.x, euler_angles.y, euler_angles.z);
            }
            else if (child.name.Contains("UpLeg") || child.name.Contains("Leg") || child.name.Contains("Foot") || child.name.Contains("Shoulder"))
            {
                euler_angles = euler_angles * Mathf.Deg2Rad;

                _jointToAngles[joint_name] = new Vector3(euler_angles.x, euler_angles.y, euler_angles.z);
            }
        }
    }
}