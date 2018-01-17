using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMountToAvatarHeadset : MonoBehaviour {

    public Transform newVivePosition;

    private Vector3 formerVivePosition;
    private Vector3 avatarOldPosition;
    private Vector3 difference = Vector3.zero;

    private GameObject avatar;

    private float distanceHeadToBody = 1.6f;

    private void Start()
    {
        formerVivePosition = newVivePosition.localPosition;
        avatarOldPosition = Vector3.zero;
    }

    void Update () {
        if(avatar != null)
        {
            //Debug.Log("Vive position: " + newVivePosition.position);
            if (Mathf.Abs(avatarOldPosition.x - avatar.transform.position.x) > 0.01 || Mathf.Abs(avatarOldPosition.y - avatar.transform.position.y) > 0.01 || Mathf.Abs(avatarOldPosition.z - avatar.transform.position.z) > 0.01)
            {
                if (difference == Vector3.zero)
                {
                    difference = newVivePosition.localPosition;
                    difference -= new Vector3(0, 1.6f, 0);
                }
                Vector3 newPos = avatar.transform.position - difference;
                this.gameObject.transform.position = newPos;
                avatarOldPosition = avatar.transform.position;
            }
            if (Mathf.Abs(formerVivePosition.x - newVivePosition.localPosition.x) > 0.01 || Mathf.Abs(formerVivePosition.y - newVivePosition.localPosition.y) > 0.01 || Mathf.Abs(formerVivePosition.z - newVivePosition.localPosition.z) > 0.01)
            {
                this.gameObject.transform.localPosition += (formerVivePosition - newVivePosition.localPosition);
                formerVivePosition = newVivePosition.localPosition;
                difference = newVivePosition.localPosition;
                difference -= new Vector3(0, distanceHeadToBody, 0);
            }
        }
        else
        {
            avatar = GameObject.Find("user_avatar_default_owner");
            formerVivePosition = newVivePosition.localPosition;
        }    
        
    }
}
