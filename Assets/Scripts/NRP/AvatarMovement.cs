using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;
using Pose = Thalmic.Myo.Pose;
using VibrationType = Thalmic.Myo.VibrationType;
using LockingPolicy = Thalmic.Myo.LockingPolicy;
using UnlockType = Thalmic.Myo.UnlockType;

public class AvatarMovement : MonoBehaviour {

    #region PRIVATE_MEMBER_VARIABLES

    /// <summary>
    /// Reference to the Myo component in the scene.
    /// </summary>
    private ThalmicMyo _thalmicMyo;

    /// <summary>
    /// The pose from the last update. This is used to determine if the pose has changed
    /// so that actions are only performed upon making them rather than every frame during
    /// which they are active.
    /// </summary>
    private Pose _lastPose = Pose.Unknown;

    /// <summary>
    /// Private GameObject reference to the avatar.
    /// </summary>
    private GameObject _avatar;

    /// <summary>
    /// Myo transform to get the transformations of the Myo
    /// </summary>
    private Transform _myoTransform = null;

    /// <summary>
    /// Private vector storing the direction in which the avatar should move.
    /// </summary>
    private Vector3 _movementDirection;

    /// <summary>
    /// The identifier to uniquely identify the user's avatar and the corresponding topics
    /// </summary>
    private string _avatarId = "";

    /// <summary>
    /// Private variable storing the desired speed for the movement of the avatar.
    /// </summary>
    private float _speed = 3f;

    /// <summary>
    /// This is the maximal allowed spped the user can use
    /// </summary>
    private float _speedMax = 6f;

    /// <summary>
    /// This is the minimum speed which is usually used
    private float _speedMin = 3f;

    /// <summary>
    /// Minimal arm movement along th y-axis to trigger movement.
    /// </summary>
    private float _deflectionMin = -0.7f;

    /// <summary>
    /// As the Joystick always returns a value after it was moved ones, a threshold of 0.4 and -0.4 is used to differentiate between input and noise
    /// </summary>
    private float _joystickThreshold = 0.4f;

    /// <summary>
    /// Factor to determine if we want to move forward (1) or backward (-1)
    /// </summary>
    private float _directionFactor = 1;

    /// <summary>
    /// To know when the player should move
    /// </summary>
    private bool _move = false;

    /// <summary>
    /// This boolean is used to determine if the movement direction has just changed to zero and should therefore be published to the server,
    /// or is still zero and should thus not be published to reduce traffic.
    /// </summary>
    private bool _zeroBefore = false;

    #endregion


    #region PUBLIC_MEMBER_VARIABLES

    /// <summary>
    /// Enum representing the different control types of the avatar.
    /// </summary>
    public enum ControlType { Joystick, Gesture };

    /// <summary>
    /// Currently used control tpye for navigating the avatar.
    /// </summary>
    public ControlType contrType;

    /// <summary>
    /// Public reference to the script VRMountToAvatarHeadset, needed to set the offset between camera and avatar while moving
    /// </summary>
    public VRMountToAvatarHeadset vrHeadset = null;

    #endregion


    /// <summary>
    /// Initialize the Myo specific components
    /// </summary>
    private void Awake()
    {
        // Get's the transform component of the Myo through the static instance of the ThalmicHub
        _myoTransform = ThalmicHub.instance.gameObject.transform.GetChild(0);

        // Access the ThalmicMyo component attached to the child of the ThalmicHub
        _thalmicMyo = ThalmicHub.instance.GetComponentInChildren<ThalmicMyo>();
    }

    /// <summary>
    /// Catches user input to control the avatar movements either through WASD or Joystick.
    /// </summary>
    void Update () {

        if (_avatarId != "")
        {
            if (_avatar != null)
            {
                #region MOVEMENT_WITH_MYO
                if (contrType == ControlType.Gesture)
                {
                    Debug.Log(_myoTransform.forward);
                    //if (thalmicMyo.pose != _lastPose)
                    //{
                    //    if (thalmicMyo.pose == Pose.WaveOut)
                    //    {
                    //        Debug.Log("WaveOut");
                    //    }
                    //    else if (thalmicMyo.pose == Pose.WaveIn)
                    //    {
                    //        Debug.Log("WaveIn");
                    //    }
                    //    _lastPose = thalmicMyo.pose;
                    //}

                    //Debug.Log("Y direction: " + _myoTransform.forward.y);
                    //Debug.Log("X: " + _myoTransform.forward.x + " z: " + _myoTransform.forward.z);
                    //Debug.Log(Vector3.Angle((avatar.transform.rotation * (_directionFactor * new Vector3(_myoTransform.forward.x, 0, _myoTransform.forward.z))), avatar.transform.forward));


                    //Debug.Log("Myo: "+_myoArmMirroring * _myoTransform.forward);

                    if (_myoTransform.forward.y > _deflectionMin)
                    {
                        // Move forward
                        _move = true;
                        _directionFactor = -1;

                    }
                    else
                    {
                        // Don't move
                        _move = false;
                    }

                    if (_move)
                    {
                        publishMovementInDirection(_directionFactor * new Vector3(_myoTransform.forward.x, 0, _myoTransform.forward.z) * _speed);
                        _zeroBefore = false;
                    }
                    else
                    {
                        if (!_zeroBefore)
                        {
                            publishMovementInDirection(Vector3.zero);
                        }
                        _zeroBefore = true;
                    }
                }
                #endregion


                #region MOVEMENT_WITH_JOYSTICK
                if(contrType == ControlType.Joystick)
                {
                    _movementDirection = Vector3.zero;

                    // As the Joystick always returns a value after it was moved ones, a threshold of 0.4 and -0.4 is used to differentiate between input and noise.
                    if (Input.GetAxis("LeftJoystickX") > _joystickThreshold || Input.GetAxis("LeftJoystickX") < -_joystickThreshold)
                    {
                        _movementDirection.x = Input.GetAxis("LeftJoystickX");
                    }
                    if (Input.GetAxis("LeftJoystickY") > _joystickThreshold || Input.GetAxis("LeftJoystickY") < -_joystickThreshold)
                    {
                        _movementDirection.z = Input.GetAxis("LeftJoystickY") * -1;
                    }

                    // Only publish the movement direction to the server, if it is not zero all the time
                    if (!(_zeroBefore && _movementDirection == Vector3.zero))
                    {
                        // To take the rotation into account as well when performing a movement, the gameObject avatar's rotation is used to transform the direction vector into the right coordinate frame.
                        // Thereby, it is important to take the quaternion as the first factor of the multiplication and the vector as the second (quaternion * vector).
                        // The resulting vector is then multiplied with the predefined speed.
                        publishMovementInDirection((_avatar.transform.rotation * _movementDirection) * _speed);

                        if (_movementDirection == Vector3.zero)
                        {
                            _zeroBefore = true;
                        }
                        else
                        {
                            _zeroBefore = false;
                        }
                    }
                    

                    #region SPEED_CONTROL
                    if (Input.GetAxis("RightJoystick5th") > _joystickThreshold || Input.GetAxis("RightJoystick5th") < -_joystickThreshold)
                    {
                        _speed = (Mathf.Abs(Input.GetAxis("RightJoystick5th")) - _joystickThreshold) * ((_speedMax - _speedMin) / (1 - _joystickThreshold)) + _speedMin;
                    }
                    else
                    {
                        _speed = _speedMin;
                    }
                    #endregion
                }
                #endregion

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
    /// Publishes the movement vector of the avatar, in the correct format to the appropriate topic at ROSBridge.
    /// </summary>
    /// <param name="movement">This vector specifies where the avatar should go to.</param>
    private void publishMovementInDirection(Vector3 movement)
    {
        //Debug.Log("Mov: " + movement);
        ROSBridge.Instance.ROS.Publish(ROSAvatarVelPublisher.GetMessageTopic(), new Vector3Msg((double)movement.x, (double)movement.z, (double)movement.y));
    }

    /// <summary>
    /// Extend the unlock if ThalmcHub's locking policy is standard, and notifies the given myo that a user action was recognized.
    /// This code is from the script JointOrientation out of the Myo Sample project
    /// </summary>
    /// <param name="myo">A reference to the ThalmicMyo component in the scene.</param>
    public static void ExtendUnlockAndNotifyUserActionForMyo(ThalmicMyo myo)
    {
        ThalmicHub hub = ThalmicHub.instance;

        if (hub.lockingPolicy == LockingPolicy.Standard)
        {
            myo.Unlock(UnlockType.Timed);
        }

        myo.NotifyUserAction();
    }

    /// <summary>
    /// This method solves a linear equation system with two unknowns of the form:
    /// y1 = k * x1 + d
    /// y2 = k * x2 + d
    /// The unknowns are k and d and are returned as a Vector2(float k, float d).
    /// </summary>
    /// <param name="y1"></param>
    /// <param name="x1"></param>
    /// <param name="y2"></param>
    /// <param name="x2"></param>
    /// <returns></returns>
    public static Vector2 solveLinearEquationWithTwoUnknowns(float y1, float x1, float y2, float x2)
    {
        Matrix4x4 factorEquation = Matrix4x4.identity;
        factorEquation[0, 0] = x1;
        factorEquation[1, 0] = x2;
        factorEquation[0, 1] = 1;
        Vector4 resultingSpeedFactors = factorEquation.inverse * new Vector4(y1, y2, 1, 1);
        return new Vector2(resultingSpeedFactors.x, resultingSpeedFactors.y);
    }
}
