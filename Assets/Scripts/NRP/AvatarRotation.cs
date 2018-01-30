using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;

public class AvatarRotation : MonoBehaviour {

    #region PRIVATE_MEMBER_VARIABLES

    /// <summary>
    /// Private GameObject reference to the avatar.
    /// </summary>
    private GameObject _avatar = null;

    /// <summary>
    /// Private variable storing the y component of the quaternion from the former rotation 
    /// </summary>
    private float _formerRotationY = 0f;

    /// <summary>
    /// Private variable storing the w component of the quaternion from the former rotation 
    /// </summary>
    private float _formerRotationW = 0f;

    /// <summary>
    /// The identifier to uniquely identify the user's avatar and the corresponding topics
    /// </summary>
    private string _avatarId = "";

    #endregion

    /// <summary>
    /// Catches user input to control the avatar movements either through WASD or Joystick.
    /// </summary>
    void Update()
    {

        if (_avatarId != "")
        {

            if (_avatar != null)
            {

                if (Mathf.Abs(this.gameObject.transform.rotation.y - _formerRotationY) > 0.05 || Mathf.Abs(this.gameObject.transform.rotation.w - _formerRotationW) > 0.05)
                {
                    Quaternion rotation = new Quaternion(0, this.gameObject.transform.rotation.y, 0, this.gameObject.transform.rotation.w);
                    // The new rotation is published to the server
                    publishRotation(rotation);
                    // The avatar in Unity directly takes the rotation from the VR headset to countervail latency and prevent glitches
                    _avatar.transform.rotation = rotation;
                    _formerRotationY = this.gameObject.transform.rotation.y;
                    _formerRotationW = this.gameObject.transform.rotation.w;
                }

                //#region ROTATION_WITH_JOYSTICK
                //if (Input.GetAxis("RightJoystick4th") > 0.4 || Input.GetAxis("RightJoystick4th") < -0.4)
                //{
                //    // As the published rotation is not added to the orgininal one, but rather replaces it, the addition is done here, by multiplying the  quaternion of the current rotation to the quaternion of the additional rotation.
                //    Quaternion rotation = Quaternion.AngleAxis(Input.GetAxis("RightJoystick4th") * 5, avatar.transform.up);
                //    if (ROSBridge.Instance.serverAvatarRot.y > 0.01)
                //    {
                //        rotation *= ROSBridge.Instance.serverAvatarRot;
                //    }
                //    publishRotation(rotation);
                //}
                //#endregion

            }
            else
            {
                _avatar = GameObject.Find("user_avatar_" + _avatarId);
            }

        }
        else
        {
            _avatarId = GzBridgeManager.Instance.avatarId;
        }

    }


    /// <summary>
    /// Publishes the rotation of the avatar, in the correct format to the appropriate topic at ROSBridge.
    /// The specified rotation is not added to the original one, but replaces it.
    /// </summary>
    /// <param name="rotation">The rotation in quaternion specifies the new rotation of the avatar.</param>
    private void publishRotation(Quaternion rotation)
    {
        //Debug.Log("Rotation send to the server: " + rotation);
        ROSBridge.Instance.ROS.Publish(ROSAvatarRotPublisher.GetMessageTopic(), new QuaternionMsg(-(double)rotation.x, -(double)rotation.z, -(double)rotation.y, (double)rotation.w));
    }
}
