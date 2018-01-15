using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;

public class AvatarMovement : MonoBehaviour {

    #region PRIVATE_MEMBER_VARIABLES

    private GameObject avatar;

    private Vector3 movementDirection;

    private float speed = 10f;

    #endregion
	
	void Update () {
        if(avatar != null)
        {
            // To take the rotation into account as well the gameObject avatar's rotation is used to transform the direction vector into the right coordinate frame.
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
            //Debug.Log("rot: " + avatar.transform.rotation.x + " " + avatar.transform.rotation.y + " " + avatar.transform.rotation.z + " " + avatar.transform.rotation.w);
            if(Input.GetAxis("RightJoystick4th") > 0.4 || Input.GetAxis("RightJoystick4th") < -0.4)
            {
                //Debug.Log(Input.GetAxis("RightJoystick4th") * 5 + " / " + avatar.transform.rotation.eulerAngles.y);
                //Quaternion q = Quaternion.Euler(0, Input.GetAxis("RightJoystick4th") * 5 + avatar.transform.rotation.eulerAngles.y, 0);
                //Quaternion q = Quaternion.Euler(0, Input.GetAxis("RightJoystick4th") * 180, 0);
                Quaternion q = Quaternion.AngleAxis(Input.GetAxis("RightJoystick4th") * 90, avatar.transform.up);
                publishRotation(q);
            }
            #endregion
        }
        else
        {
            avatar = GameObject.Find("user_avatar_default_owner");
        }

    }

    private void publishMovementInDirection(Vector3 direction)
    {
        // Publishes the new velocity of the avatar to the appropriate topic
        ROSBridge.Instance.ROS.Publish(ROSAvatarVelPublisher.GetMessageTopic(), new Vector3Msg((double)direction.x, (double)direction.z, (double)direction.y));
    }

    private void publishRotation(Quaternion rotation)
    {
        Debug.Log(rotation);
        // Publishes the new rotation of the avatar to the appropriate topic
        ROSBridge.Instance.ROS.Publish(ROSAvatarRotPublisher.GetMessageTopic(), new QuaternionMsg((double)rotation.x, (double)rotation.z, (double)rotation.y, (double)rotation.w));
    }
}
