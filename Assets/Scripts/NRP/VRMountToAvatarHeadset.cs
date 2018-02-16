using System.Collections;
using System.Collections.Generic;
using System;
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
    private Vector3 _formerVivePosition = Vector3.zero;

    /// <summary>
    /// Vector to store the last position of the avatar.
    /// </summary>
    private Vector3 _formerAvatarPosition = Vector3.zero;

    /// <summary>
    /// This is the offset between the camera and the avatar while moving
    /// </summary>
    private Vector3 _cameraMovementOffset = Vector3.zero;

    /// <summary>
    /// Vector to store the offset between the center of the play area [CameraRig] and the VR camera.
    /// </summary>
    private Vector3 _viveOffset = Vector3.zero;

    /// <summary>
    /// Private GameObject reference to the avatar.
    /// </summary>
    private GameObject _avatar;

    /// <summary>
    /// Private GameObject reference to the head of the avatar.
    /// </summary>
    private GameObject _avatarHead;

    /// <summary>
    /// The offset between avatar's head and its body.
    /// </summary>
    private float _distanceHeadToBody = 1.6f;
    
    /// <summary>
    /// The identifier to uniquely identify the user's avatar and the corresponding topics
    /// </summary>
    private string _avatarId = "";

    /// <summary>
    /// Determines if the Vive should stick to the head (active) or move freely (not active).
    /// </summary>
    private bool _active = false;

    /// <summary>
    /// Allows the script to set the avatar position at the start of the simulation, although movement mode isn't enabled.
    /// </summary>
    private bool _initiatePosition = true;

    #endregion


    /// <summary>
    /// Catches position changes of the avatar and the VR headset to either follow them (avatar) or compensate them (VR headset).
    /// The compensation is done to keep the position of the VR headset stable and prevent the user from moving the headset without moving the avatar.
    /// </summary>
    void Update () {
        if(_avatarId != "")
        {
            if (_avatar != null)
            {
                if (_initiatePosition)
                {
                    // The viveOffset is needed to align the VR headset with the head of the avatar..
                    if (_viveOffset == Vector3.zero)
                    {
                        _viveOffset = newVivePosition.localPosition;
                        _viveOffset -= new Vector3(0f, _distanceHeadToBody, 0);
                    }

                    this.gameObject.transform.position = _avatar.transform.position - _viveOffset;
                    _initiatePosition = false;
                }
                if (_active)
                {
                    // Position change of the avatar
                    if (Mathf.Abs(_formerAvatarPosition.x - _avatar.transform.position.x) > 0.01 || Mathf.Abs(_formerAvatarPosition.y - _avatar.transform.position.y) > 0.01 || Mathf.Abs(_formerAvatarPosition.z - _avatar.transform.position.z) > 0.01)
                    {
                        // The viveOffset is needed to align the VR headset with the head of the avatar..
                        if (_viveOffset == Vector3.zero)
                        {
                            _viveOffset = newVivePosition.localPosition;
                            _viveOffset -= new Vector3(0f, _distanceHeadToBody, 0);
                        }

                        this.gameObject.transform.position = _avatar.transform.position - _viveOffset;
                        _formerAvatarPosition = _avatar.transform.position;
                    }
                    // Position change of the VR headset
                    if (Mathf.Abs(_formerVivePosition.x - newVivePosition.localPosition.x) > 0.01 || Mathf.Abs(_formerVivePosition.y - newVivePosition.localPosition.y) > 0.01 || Mathf.Abs(_formerVivePosition.z - newVivePosition.localPosition.z) > 0.01)
                    {
                        this.gameObject.transform.localPosition += (_formerVivePosition - newVivePosition.localPosition);
                        _formerVivePosition = newVivePosition.localPosition;
                        _viveOffset = newVivePosition.localPosition;
                        _viveOffset -= new Vector3(0f, _distanceHeadToBody, 0);
                    }
                }

                // Alternative code to only enable free movement in y direction when not in movement mode
                //else
                //{
                //    // Position change of the avatar
                //    if (Mathf.Abs(_formerAvatarPosition.x - _avatar.transform.position.x) > 0.01 || Mathf.Abs(_formerAvatarPosition.z - _avatar.transform.position.z) > 0.01)
                //    {
                //        // The viveOffset is needed to align the VR headset with the head of the avatar..
                //        if (_viveOffset == Vector3.zero)
                //        {
                //            _viveOffset = new Vector3(newVivePosition.localPosition.x, 0, newVivePosition.localPosition.z);
                //        }
                //        this.gameObject.transform.position = new Vector3(_avatar.transform.position.x - _viveOffset.x, this.gameObject.transform.position.y, _avatar.transform.position.z - _viveOffset.z);
                //        _formerAvatarPosition = _avatar.transform.position;
                //    }
                //    // Position change of the VR headset
                //    if (Mathf.Abs(_formerVivePosition.x - newVivePosition.localPosition.x) > 0.01 || Mathf.Abs(_formerVivePosition.z - newVivePosition.localPosition.z) > 0.01)
                //    {
                //        this.gameObject.transform.localPosition += new Vector3((_formerVivePosition - newVivePosition.localPosition).x, 0, (_formerVivePosition - newVivePosition.localPosition).z);
                //        _formerVivePosition = newVivePosition.localPosition;
                //        _viveOffset = new Vector3(newVivePosition.localPosition.x, 0, newVivePosition.localPosition.z);
                //    }
                //}
                
            }
            else
            {
                _avatar = GameObject.Find("user_avatar_" + _avatarId);
                _formerVivePosition = newVivePosition.localPosition;
            }

        }
        else
        {
            _avatarId = GzBridgeManager.Instance.avatarId;
        }
    }

    /// <summary>
    /// Activates the fixation of the Vive to the avatar's head, which prevents free movement of the Vive in any direction.
    /// </summary>
    /// <param name="state"></param>
    public void activateViveFixationToHead(bool state)
    {
        _active = state;
    }

}
