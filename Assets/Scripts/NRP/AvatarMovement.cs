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
    ///  A rotation that compensates for the Myo armband's orientation parallel to the ground, i.e. yaw.
    ///  Once set, the direction the Myo armband is facing becomes "forward" within the program.
    /// </summary>
    private Quaternion _antiYaw = Quaternion.identity;

    /// <summary>
    /// antiRoll represents a rotation about the myo Armband's forward axis adjusting for reference roll.
    /// </summary>
    private Quaternion _antiRoll = Quaternion.identity;

    /// <summary>
    /// Coroutine for making the Myo armband pulse
    /// </summary>
    private IEnumerator _coroutinePulse;

    /// <summary>
    /// The identifier to uniquely identify the user's avatar and the corresponding topics
    /// </summary>
    private string _avatarId = "";

    /// <summary>
    /// Private variable storing the desired speed for the movement of the avatar.
    /// </summary>
    private float _speed = 3f;

    /// <summary>
    /// Variable for speed / deflection mapping with speed = k * myo-deflection + d
    /// </summary>
    private float _myo_speedFunction_k = 1;

    /// <summary>
    /// Variable for speed / deflection mapping with speed = k * myo-deflection + d
    /// </summary>
    private float _myo_speedFunction_d = 0;

    /// <summary>
    /// Variable for speed / joystick mapping with speed = k * joystick + d
    /// </summary>
    private float _joystick_speedFunction_k = 1;

    /// <summary>
    /// Variable for speed / joystick mapping with speed = k * joystick + d
    /// </summary>
    private float _joystick_speedFunction_d = 0;

    /// <summary>
    /// This is the maximal allowed spped the user can use
    /// </summary>
    private float _speedMax = 6f;

    /// <summary>
    /// This is the minimum speed which is usually used
    private float _speedMin = 2f;

    /// <summary>
    /// Minimal arm movement along th y-axis to trigger movement.
    /// </summary>
    private float _deflectionMin = -0.8f;

    /// <summary>
    /// Maximal arm movement along the y-axis.
    /// </summary>
    private float _deflectionMax = 0.3f;

    /// <summary>
    /// As the Joystick always returns a value after it was moved ones, a threshold of 0.1 and -0.1 is used to differentiate between input and noise
    /// </summary>
    private float _joystickThreshold = 0.1f;

    /// <summary>
    /// Factor to determine if we want to move forward (1) or backward (-1)
    /// </summary>
    private float _directionFactor = 1;

    /// <summary>
    /// A reference angle representing how the armband is rotated about the wearer's arm, i.e. roll.
    /// Set together with _antiYaw
    /// </summary>
    private float _referenceRoll = 0.0f;

    /// <summary>
    /// Time when the last gesture was performed and recognized.
    /// </summary>
    private float _lastGestureTime = 0f;

    /// <summary>
    /// Height of the Vive at the time the user switches from movement to free mode.
    /// </summary>
    private float _originalHeight = 0f;

    /// <summary>
    /// To know when the player should move
    /// </summary>
    private bool _move = false;

    /// <summary>
    /// This boolean is used to determine if the movement direction has just changed to zero and should therefore be published to the server,
    /// or is still zero and should thus not be published to reduce traffic.
    /// </summary>
    private bool _zeroBefore = false;

    /// <summary>
    /// A boolean indicating if the Myo should be synchronized with the direction of the Vive.
    /// </summary>
    private bool _synch = true;

    /// <summary>
    /// Boolean to check if the coroutine is already running
    /// </summary>
    private bool _isCoroutineRunning = false;

    /// <summary>
    /// Boolean to control the movement mode.
    /// Only allow the user to control the avatar with gestures if movement mode is activated.
    /// </summary>
    private bool _movementModeActive = false;

    #endregion


    #region PUBLIC_MEMBER_VARIABLES

    /// <summary>
    /// Enum representing the different control types of the avatar.
    /// </summary>
    public enum ControlType { Joystick = 0, Gesture = 1 };

    /// <summary>
    /// Currently used control tpye for navigating the avatar.
    /// </summary>
    public ControlType contrType;

    /// <summary>
    /// Public reference to the script VRMountToAvatarHeadset, needed to set the offset between camera and avatar while moving
    /// </summary>
    public VRMountToAvatarHeadset vrHeadset = null;

    /// <summary>
    /// Transform of the Vive-HMD.
    /// Needed to ensure that both reference systems (Myo and Vive) face into the same direction
    /// </summary>
    public Transform vivePosition;

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
    /// Calculate the appropriate speed and deflection mapping
    /// </summary>
    private void Start()
    {
        Vector2 res = solveLinearEquationWithTwoUnknowns(_speedMin, _deflectionMin, _speedMax, _deflectionMax);
        _myo_speedFunction_k = res.x;
        _myo_speedFunction_d = res.y;
        res = solveLinearEquationWithTwoUnknowns(_speedMin, _joystickThreshold, _speedMax, 1);
        _joystick_speedFunction_k = res.x;
        _joystick_speedFunction_d = res.y;
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

                if(contrType == ControlType.Gesture)
                {

                    #region MYO_SYNCHRONIZATION
                    // Update references between Myo and Vive. 
                    if (_synch)
                    {
                        // _antiYaw represents a rotation of the Myo armband about the Y axis (up) which aligns the forward
                        // vector of the rotation with Z = 1 when the wearer's arm is pointing in the reference direction / Vive looking direction.
                        _antiYaw = Quaternion.FromToRotation(
                        new Vector3(_myoTransform.forward.x, 0, _myoTransform.forward.z),
                        new Vector3(vivePosition.forward.x, 0, vivePosition.forward.z)
                        );

                        // _referenceRoll represents how many degrees the Myo armband is rotated clockwise
                        // about its forward axis (when looking down the wearer's arm towards their hand) from the reference zero
                        // roll direction. This direction is calculated and explained below. When this reference is
                        // taken, the joint will be rotated about its forward axis such that it faces upwards when
                        // the roll value matches the reference.
                        Vector3 referenceZeroRoll = computeZeroRollVector(_myoTransform.forward);
                        _referenceRoll = rollFromZero(referenceZeroRoll, _myoTransform.forward, _myoTransform.up);

                        _synch = false;
                    }

                    // Current zero roll vector and roll value.
                    Vector3 zeroRoll = computeZeroRollVector(_myoTransform.forward);
                    float roll = rollFromZero(zeroRoll, _myoTransform.forward, _myoTransform.up);

                    // The relative roll is simply how much the current roll has changed relative to the reference roll.
                    // adjustAngle simply keeps the resultant value within -180 to 180 degrees.
                    float relativeRoll = normalizeAngle(roll - _referenceRoll);

                    // antiRoll represents a rotation about the myo Armband's forward axis adjusting for reference roll.
                    _antiRoll = Quaternion.AngleAxis(relativeRoll, _myoTransform.forward);
                    #endregion

                    #region CHANGE_MOVEMENT_MDOE
                    Vector3 tmp = _antiYaw * _antiRoll * _myoTransform.forward;
                    // Enter / Exit movement mode when any gesture was performed with sufficient strength (_thalmicMyo.emg[i] > 85) and the arm was pointing downwards and not backwards or sidewards (Mathf.Abs(relativeRoll) < 20)
                    if (_lastGestureTime + 0.5 < Time.time && _myoTransform.forward.y < _deflectionMin && Mathf.Abs(relativeRoll) < 20)
                    {
                        // Iterate through all emg sensors, if one has a value larger than 80 a gesture was performed
                        for (int i = 0; i < _thalmicMyo.emg.Length; i++)
                        {
                            if (_thalmicMyo.emg[i] > 85)
                            {
                                //Debug.Log(_thalmicMyo.emg[0] + " " + _thalmicMyo.emg[1] + " " + _thalmicMyo.emg[2] + " " + _thalmicMyo.emg[3] + " " +
                                //    _thalmicMyo.emg[4] + " " + _thalmicMyo.emg[5] + " " + _thalmicMyo.emg[6] + " " + _thalmicMyo.emg[7]);

                                // Only change into active movement mode if the user isn't kneeling on the ground
                                if (vivePosition.position.y > (_originalHeight - 0.1) || _movementModeActive)
                                {
                                    _movementModeActive = !_movementModeActive;
                                    _thalmicMyo.Vibrate(VibrationType.Medium);

                                    if (!_movementModeActive)
                                    {
                                        // Track the y position of the Vive to determine if the player is kneeling or not
                                        _originalHeight = vivePosition.position.y;
                                    }
                                    else
                                    {
                                        // Always synch the player when switching into movement mode
                                        _synch = true;
                                    }
                                }

                                // Stop movement when switching movement mode off
                                if (!_movementModeActive && _move)
                                {
                                    publishMovementInDirection(Vector3.zero);
                                    enableMovement(false);
                                }

                                _lastGestureTime = Time.time;
                                break;
                            }
                        }
                    }

                    #endregion

                    #region MOVEMENT_CONTROL
                    // If movement mode is activated track the movements of the Myo and translate them into avatar movements
                    if (_movementModeActive)
                    {
                        // Define the coroutine used to pulse the vibration the armband
                        _coroutinePulse = PulseVibration(1.5f, _thalmicMyo);

                        if (_myoTransform.forward.y > _deflectionMin)
                        {
                            // Move forward
                            enableMovement(true);
                            _directionFactor = 1;

                        }
                        else if (_myoTransform.forward.y <= _deflectionMin && Mathf.Abs(relativeRoll) >= 90)
                        {
                            // Move backward
                            enableMovement(true);
                            _directionFactor = -1;
                        }
                        else
                        {
                            // Don't move
                            enableMovement(false);
                        }

                        if (_move)
                        {
                            if (_directionFactor > 0)
                            {
                                // Move forward in the direction where the user is pointing with the Myo

                                _speed = _myoTransform.forward.y * _myo_speedFunction_k + _myo_speedFunction_d;
                                // Here the anti - roll and yaw rotations are applied to the myo Armband's forward direction to yield the correct orientation.
                                publishMovementInDirection(_directionFactor * (_antiYaw * _antiRoll * new Vector3(_myoTransform.forward.x, 0, _myoTransform.forward.z)) * _speed);
                            }
                            else
                            {
                                // Move backward in the inverse direction of the avatar's forward direction with constant speed

                                _speed = 3f;
                                // When performing a backward movement, the user should go into the inverse direction he is looking at right now.
                                // Therefore, one needs to take the avatar rotation into account as well, like when performing the movement with teh joystick
                                publishMovementInDirection(_directionFactor * (_avatar.transform.rotation * new Vector3(0, 0, 1)) * _speed);

                            }

                            // Start the pulsing of the Myo if needed
                            if (!_isCoroutineRunning) StartCoroutine(_coroutinePulse);

                            _zeroBefore = false;
                        }
                        else
                        {
                            if (!_zeroBefore)
                            {
                                publishMovementInDirection(Vector3.zero);
                            }

                            // Stop coroutine if running
                            if (_isCoroutineRunning) StopCoroutine(_coroutinePulse);

                            _zeroBefore = true;
                        }
                    }
                    #endregion
                }

                #endregion


                #region MOVEMENT_WITH_JOYSTICK
                if(contrType == ControlType.Joystick)
                {
                    _movementDirection = Vector3.zero;

                    // As the Joystick always returns a value after it was moved ones, a threshold of 0.1 and -0.1 is used to differentiate between input and noise.
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
                        Vector3 rotMovement = _avatar.transform.rotation * _movementDirection;
                        // Determine the speed given through the deflections of the left joystick
                        Vector3 speedVector = new Vector3(Mathf.Abs(_movementDirection.x) * _joystick_speedFunction_k + _joystick_speedFunction_d, 0, Mathf.Abs(_movementDirection.z) * _joystick_speedFunction_k + _joystick_speedFunction_d);
                        publishMovementInDirection(Vector3.Scale(rotMovement, speedVector));

                        if (_movementDirection == Vector3.zero)
                        {
                            _zeroBefore = true;
                            vrHeadset.activateViveFixationToHead(false);
                        }
                        else if(_zeroBefore)
                        {
                            _zeroBefore = false;
                            vrHeadset.activateViveFixationToHead(true);
                        }
                        
                    }
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
        ROSBridge.Instance.ROS.Publish(ROSAvatarVelPublisher.GetMessageTopic(), new Vector3Msg((double)movement.x, (double)movement.z, (double)movement.y));
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
    /// Compute a vector that points perpendicular to the forward direction,
    /// minimizing angular distance from world up (positive Y axis).
    /// This represents the direction of no rotation about its forward axis.
    /// </summary>
    /// <param name="forward"></param>
    /// <returns></returns>
    Vector3 computeZeroRollVector(Vector3 forward)
    {
        Vector3 antigravity = Vector3.up;
        Vector3 m = Vector3.Cross(_myoTransform.forward, antigravity);
        Vector3 roll = Vector3.Cross(m, _myoTransform.forward);

        return roll.normalized;
    }

    /// <summary>
    /// Compute the angle of rotation clockwise about the forward axis relative to the provided zero roll direction.
    /// As the armband is rotated about the forward axis this value will change, regardless of which way the
    /// forward vector of the Myo is pointing. The returned value will be between -180 and 180 degrees.
    /// IMPROTANT: This is the roll from the myo, not your arm; it is 0 if the Logo is facing upwards
    /// </summary>
    /// <param name="zeroRoll"></param>
    /// <param name="forward"></param>
    /// <param name="up"></param>
    /// <returns></returns>
    float rollFromZero(Vector3 zeroRoll, Vector3 forward, Vector3 up)
    {
        // The cosine of the angle between the up vector and the zero roll vector. Since both are
        // orthogonal to the forward vector, this tells us how far the Myo has been turned around the
        // forward axis relative to the zero roll vector, but we need to determine separately whether the
        // Myo has been rolled clockwise or counterclockwise.
        float cosine = Vector3.Dot(up, zeroRoll);

        // To determine the sign of the roll, we take the cross product of the up vector and the zero
        // roll vector. This cross product will either be the same or opposite direction as the forward
        // vector depending on whether up is clockwise or counter-clockwise from zero roll.
        // Thus the sign of the dot product of forward and it yields the sign of our roll value.
        Vector3 cp = Vector3.Cross(up, zeroRoll);
        float directionCosine = Vector3.Dot(forward, cp);
        float sign = directionCosine < 0.0f ? 1.0f : -1.0f;

        // Return the angle of roll (in degrees) from the cosine and the sign.
        return sign * Mathf.Rad2Deg * Mathf.Acos(cosine);
    }

    /// <summary>
    /// Adjust the provided angle to be within a -180 to 180.
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    float normalizeAngle(float angle)
    {
        if (angle > 180.0f)
        {
            return angle - 360.0f;
        }
        if (angle < -180.0f)
        {
            return angle + 360.0f;
        }
        return angle;
    }

    /// <summary>
    /// To make the Myo vibrate in specific time intervals
    /// </summary>
    /// <param name="waitTime"></param>
    /// <param name="thalmicMyo"></param>
    /// <returns></returns>
    private IEnumerator PulseVibration(float waitTime, ThalmicMyo thalmicMyo)
    {
        _isCoroutineRunning = true;

        while (_move)
        {
            thalmicMyo.Vibrate(VibrationType.Short);
            yield return new WaitForSeconds(waitTime);
        }

        _isCoroutineRunning = false;
    }

    /// <summary>
    /// Sets the movement indicator _move and enables / disables the fixation of the Vive to the avatar's head
    /// </summary>
    /// <param name="enable">bool to enable / disable movement</param>
    private void enableMovement(bool enable)
    {
        _move = enable;
        vrHeadset.activateViveFixationToHead(enable);
    }

    /// <summary>
    /// Sets the assumed height of the avatar.
    /// </summary>
    /// <param name="height"></param>
    public void setViveHeight(float height)
    {
        _originalHeight = height;
    }
}
