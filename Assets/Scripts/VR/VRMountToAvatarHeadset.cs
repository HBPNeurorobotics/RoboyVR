using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMountToAvatarHeadset : MonoBehaviour {

    public Transform newVivePosition;

    private Vector3 formerVivePosition;
    private Vector3 avatarOldPosition;
    private Vector3 difference = Vector3.zero;

    private GameObject avatar;
    //private GameObject tmp;

    private void Start()
    {
        formerVivePosition = newVivePosition.localPosition;
        avatarOldPosition = Vector3.zero;
    }

    void Update () {
        if(avatar != null)
        {
            Debug.Log(newVivePosition.position);
            if (Mathf.Abs(avatarOldPosition.x - avatar.transform.position.x) > 0.02 || Mathf.Abs(avatarOldPosition.y - avatar.transform.position.y) > 0.02 || Mathf.Abs(avatarOldPosition.z - avatar.transform.position.z) > 0.02)
            {
                if (difference == Vector3.zero)
                {
                    difference = newVivePosition.localPosition;
                    difference -= new Vector3(0, 1.6f, 0);
                }

                this.gameObject.transform.position = avatar.transform.position - difference;
                avatarOldPosition = avatar.transform.position;
            }
            if (Mathf.Abs(formerVivePosition.x - newVivePosition.localPosition.x) > 0.01 || Mathf.Abs(formerVivePosition.y - newVivePosition.localPosition.y) > 0.01 || Mathf.Abs(formerVivePosition.z - newVivePosition.localPosition.z) > 0.01)
            {
                this.gameObject.transform.localPosition += (formerVivePosition - newVivePosition.localPosition);
                formerVivePosition = newVivePosition.localPosition;
                difference = newVivePosition.localPosition;
                difference -= new Vector3(0, 1.6f, 0);
            }
        }
        else
        {
            avatar = GameObject.Find("user_avatar_default_owner");
        }

        //if (tmp == null)
        //{
        //    tmp = GameObject.Find("user_avatar_default_owner::user_avatar_basic::body::head_visual");
        //}
        //else
        //{
        //    //Debug.Log("Tmp: " + tmp.transform.position);
        //}
        
        
    }
}
