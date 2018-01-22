using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMountToAvatarHeadset : MonoBehaviour {
    #region PUBLIC_MEMBER_VARIABLES

    /// <summary>
    /// Reference to the Transform of the VR camera.
    /// </summary>
    public Transform newVivePosition;

    #endregion

    #region PRIVATE_MEMBER_VARIABLES

    /// <summary>
    /// Vector to store the last position of the VR camera.
    /// </summary>
    private Vector3 formerVivePosition = Vector3.zero;

    /// <summary>
    /// Vector to store the last position of the avatar.
    /// </summary>
    private Vector3 formerAvatarPosition = Vector3.zero;

    /// <summary>
    /// Vector to store the offset between the center of the play area [CameraRig] and the VR camera.
    /// </summary>
    private Vector3 viveOffset = Vector3.zero;

    /// <summary>
    /// Private GameObject reference to the avatar.
    /// </summary>
    private GameObject avatar;

    /// <summary>
    /// Private GameObject reference to the head of the avatar.
    /// </summary>
    private GameObject avatarHead;

    /// <summary>
    /// The offset between avatar's head and its body.
    /// </summary>
    private float distanceHeadToBody = 1.6f;

    /// <summary>
    /// The identifier to uniquely identify the user's avatar and the corresponding topics
    /// </summary>
    private string avatarId = "";

    #endregion


    /// <summary>
    /// Catches position changes of the avatar and the VR headset to either follow them (avatar) or compensate them (VR headset).
    /// The compensation is done to keep the position of the VR headset stable and prevent the user from moving the headset without moving the avatar.
    /// </summary>
    void Update () {
        if(avatarId != "")
        {
            if (avatar != null)
            {
                // Position change of the avatar
                if (Mathf.Abs(formerAvatarPosition.x - avatar.transform.position.x) > 0.01 || Mathf.Abs(formerAvatarPosition.y - avatar.transform.position.y) > 0.01 || Mathf.Abs(formerAvatarPosition.z - avatar.transform.position.z) > 0.01)
                {
                    // The viveOffset is needed to align the VR headset with the head of the avatar..
                    if (viveOffset == Vector3.zero)
                    {
                        viveOffset = newVivePosition.localPosition;
                        viveOffset -= new Vector3(0, 1.6f, 0);
                    }
                    this.gameObject.transform.position = avatar.transform.position - viveOffset;
                    formerAvatarPosition = avatar.transform.position;
                }
                // Position change of the VR headset
                if (Mathf.Abs(formerVivePosition.x - newVivePosition.localPosition.x) > 0.01 || Mathf.Abs(formerVivePosition.y - newVivePosition.localPosition.y) > 0.01 || Mathf.Abs(formerVivePosition.z - newVivePosition.localPosition.z) > 0.01)
                {
                    this.gameObject.transform.localPosition += (formerVivePosition - newVivePosition.localPosition);
                    formerVivePosition = newVivePosition.localPosition;
                    viveOffset = newVivePosition.localPosition;
                    viveOffset -= new Vector3(0, distanceHeadToBody, 0);
                }
            }
            else
            {
                avatar = GameObject.Find("user_avatar_" + avatarId);
                formerVivePosition = newVivePosition.localPosition;
            }

            // Set the layer of the avatar's head and its children to "AvatarHead" to prevent the VR camera from rendering it, which would cause the head to randomply appear.
            if (avatarHead != null)
            {
                avatarHead.layer = LayerMask.NameToLayer("AvatarHead");
                for (int i = 0; i < avatarHead.transform.childCount; i++)
                {
                    avatarHead.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("AvatarHead");
                }
            }
            else
            {
                avatarHead = GameObject.Find("user_avatar_" + avatarId + "::user_avatar_basic::body::head_visual");
            }
        }
        else
        {
            avatarId = GzBridgeManager.Instance.avatarId;
        }
    }
}
