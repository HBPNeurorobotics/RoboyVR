using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;

public class AvatarMovement : MonoBehaviour {

    #region PRIVATE_MEMBER_VARIABLES

    /// <summary>
    /// Private GameObject reference to the avatar.
    /// </summary>
    private GameObject avatar;

    /// <summary>
    /// Private vector storing the direction in which the avatar should move.
    /// </summary>
    private Vector3 movementDirection;

    /// <summary>
    /// Private variable storing the desired speed for the movement of the avatar.
    /// </summary>
    private float speed = 1f;

    /// <summary>
    /// This is the maximal allowed spped the user can use
    /// </summary>
    private float speedMax = 6f;

    /// <summary>
    /// This is the minimum speed which is usually used
    private float speedMin = 3f;

    /// <summary>
    /// As the Joystick always returns a value after it was moved ones, a threshold of 0.4 and -0.4 is used to differentiate between input and noise
    /// </summary>
    private float joystickThreshold = 0.4f;

    /// <summary>
    /// The identifier to uniquely identify the user's avatar and the corresponding topics
    /// </summary>
    private string avatarId = "";

    #endregion


    #region PUBLIC_MEMBER_VARIABLES

    /// <summary>
    /// Public reference to the script VRMountToAvatarHeadset, needed to set the offset between camera and avatar while moving
    /// </summary>
    public VRMountToAvatarHeadset vrHeadset = null;

    #endregion


    /// <summary>
    /// Catches user input to control the avatar movements either through WASD or Joystick.
    /// </summary>
    void Update () {

        if (avatarId != "")
        {
            if (avatar != null)
            {
                // To take the rotation into account as well when performing a movement, the gameObject avatar's rotation is used to transform the direction vector into the right coordinate frame.
                // Thereby, it is important to take the quaternion as the first factor of the multiplication and the vector as the second (quaternion * vector).
                // The resulting vector is then multiplied with the predefined speed.

                #region MOVEMENT_WITH_JOYSTICK
                movementDirection = Vector3.zero;
                // As the Joystick always returns a value after it was moved ones, a threshold of 0.4 and -0.4 is used to differentiate between input and noise.
                if (Input.GetAxis("LeftJoystickX") > joystickThreshold || Input.GetAxis("LeftJoystickX") < -joystickThreshold)
                {
                    movementDirection.x = Input.GetAxis("LeftJoystickX");
                }
                if (Input.GetAxis("LeftJoystickY") > joystickThreshold || Input.GetAxis("LeftJoystickY") < -joystickThreshold)
                {
                    movementDirection.z = Input.GetAxis("LeftJoystickY") * -1;
                }
                publishMovementInDirection((avatar.transform.rotation * movementDirection) * speed);
                //The VR camera is moved a little bit in front of the avatar to prevent the body from being in the camera image and irritating the user while moving
                if (vrHeadset != null)
                    {
                        if (movementDirection.z < 0)
                        {
                            movementDirection = Vector3.zero;
                        }
                        if(movementDirection!= Vector3.zero)
                            vrHeadset.setCameraMovementOffset(avatar.transform.rotation * movementDirection);
                    }
                #endregion

                #region SPEED_CONTROL_WITH_JOYSTICK
                if(Input.GetAxis("RightJoystick5th") > joystickThreshold || Input.GetAxis("RightJoystick5th") < -joystickThreshold)
                {
                    speed = (Mathf.Abs(Input.GetAxis("RightJoystick5th")) - joystickThreshold) * ((speedMax - speedMin)/(1 - joystickThreshold)) + speedMin;
                }
                else
                {
                    speed = speedMin;
                }
                #endregion

            }
            else
            {
                avatar = GameObject.Find("user_avatar_" + avatarId);
            }
        }
        else
        {
            avatarId = GzBridgeManager.Instance.avatarId;
        }

    }

    /// <summary>
    /// Publishes the movement vector of the avatar, in the correct format to the appropriate topic at ROSBridge.
    /// </summary>
    /// <param name="movement">This vector specifies where the avatar should go to.</param>
    private void publishMovementInDirection(Vector3 movement)
    {
        ROSBridge.Instance.ROS.Publish(ROSAvatarVelPublisher.GetMessageTopic(), new Vector3Msg((double)movement.x, (double)movement.z, (double)movement.y));
    }

}
