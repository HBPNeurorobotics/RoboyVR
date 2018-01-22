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
    private float speed = 10f;

    bool instantiate = true;

    #endregion
	
    /// <summary>
    /// Catches user input to control the avatar movements either through WASD or Joystick.
    /// </summary>
	void Update () {

        if (avatar != null)
        {
            // To take the rotation into account as well when performing a movement, the gameObject avatar's rotation is used to transform the direction vector into the right coordinate frame.
            // Thereby, it is important to take the quaternion as the first factor of the multiplication and the vector as the second (quaternion * vector).
            // The resulting vector is then multiplied with the predefined speed.

            #region MOVEMENT_WITH_WASD
            if (Input.GetKey(KeyCode.S))
            {
                movementDirection = avatar.transform.rotation * new Vector3(0, 0, 1);
                publishMovementInDirection(movementDirection * -1 * speed);
            }
            else if (Input.GetKey(KeyCode.W))
            {
                movementDirection = avatar.transform.rotation * new Vector3(0, 0, 1);
                publishMovementInDirection(movementDirection * speed);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                movementDirection = avatar.transform.rotation * new Vector3(1, 0, 0);
                publishMovementInDirection(movementDirection * speed);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                movementDirection = avatar.transform.rotation * new Vector3(1, 0, 0);
                publishMovementInDirection(movementDirection * -1 * speed);
            }
            #endregion

            #region MOVEMENT_WITH_JOYSTICK
            movementDirection = Vector3.zero;
            // As the Joystick always returns a value after it was moved ones, a threshold of 0.4 and -0.4 is used to differentiate between input and noise.
            if (Input.GetAxis("LeftJoystickX") > 0.4 || Input.GetAxis("LeftJoystickX") < -0.4)
            {
                movementDirection.x = Input.GetAxis("LeftJoystickX");
            }
            if(Input.GetAxis("LeftJoystickY") > 0.4 || Input.GetAxis("LeftJoystickY") < -0.4)
            {
                movementDirection.z = Input.GetAxis("LeftJoystickY") * -1;
            }
            if(movementDirection != Vector3.zero)
            {
                publishMovementInDirection((avatar.transform.rotation * movementDirection) * speed);
            }
            #endregion

            #region ROTATION_WITH_JOYSTICK
            if(Input.GetAxis("RightJoystick4th") > 0.4 || Input.GetAxis("RightJoystick4th") < -0.4)
            {
                // As the published rotation is not added to the orgininal one, but rather replaces it, the addition is done here, by multiplying the  quaternion of the current rotation to the quaternion of the additional rotation.
                Quaternion rotation = Quaternion.AngleAxis(Input.GetAxis("RightJoystick4th") * 5, avatar.transform.up);
                rotation *= ROSBridge.Instance.serverAvatarRot;
                publishRotation(rotation);
            }
            #endregion
        }
        else
        {
            avatar = GameObject.Find("user_avatar_default_owner");
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

    /// <summary>
    /// Publishes the rotation of the avatar, in the correct format to the appropriate topic at ROSBridge.
    /// The specified rotation is not added to the original one, but replaces it.
    /// </summary>
    /// <param name="rotation">The rotation in quaternion specifies the new rotation of the avatar.</param>
    private void publishRotation(Quaternion rotation)
    {
        Debug.Log("Rotation send to the server: "+rotation);
        ROSBridge.Instance.ROS.Publish(ROSAvatarRotPublisher.GetMessageTopic(), new QuaternionMsg(-(double)rotation.x, -(double)rotation.z, -(double)rotation.y, (double)rotation.w));
    }
}
