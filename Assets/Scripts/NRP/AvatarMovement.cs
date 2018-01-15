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
}
