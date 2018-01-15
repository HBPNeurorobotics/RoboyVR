using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;

public class AvatarMovement : MonoBehaviour {

    #region PRIVATE_MEMBER_VARIABLES

    private GameObject avatar;

    private float speed = 10f;

    #endregion
	
	void Update () {
        if(avatar != null)
        {
            // To take the rotation into account as well the gameObject avatar is used to get the correct direction vector.
            // This vector is then multiplied with the predefined speed.
            if (Input.GetKey(KeyCode.S))
            {
                publishMovementInDirection(avatar.transform.forward * -1 * speed);
            }
            else if (Input.GetKey(KeyCode.W))
            {
                publishMovementInDirection(avatar.transform.forward * speed);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                publishMovementInDirection(avatar.transform.right * speed);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                publishMovementInDirection(avatar.transform.right * -1 * speed);
            }
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
