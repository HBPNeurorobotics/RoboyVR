using System;
using System.Collections;
using Locomotion;
using UnityEngine;
using Valve.VR;

public class SteamVRControllerInput : Singleton<SteamVRControllerInput>
{
    private readonly EVRButtonId _touchpad = EVRButtonId.k_EButton_SteamVR_Touchpad;

    private SteamVR_Controller.Device _leftController;
    [SerializeField] private SteamVR_TrackedObject _leftControllerObject;
    private LocomotionBehaviour _locomotionBehaviour = LocomotionBehaviour.Hover;
    private SteamVR_Controller.Device _rightController;

    [SerializeField] private SteamVR_TrackedObject _rightControllerObject;
    [SerializeField] private bool _simulateMovePress;
    private bool stoppedMovement = true;

    [SerializeField] private float _speed = 0.1f;
    private bool _touchpadPressLeft;
    private bool _touchpadPressRight;
    private LocomotionBehaviour currentLocomotionBehaviour;
    [SerializeField] private LocomotionBehaviour setLocomotionBehaviour;

    public SteamVR_TrackedObject RightControllerObject
    {
        get { return _rightControllerObject; }
    }

    public SteamVR_TrackedObject LeftControllerObject
    {
        get { return _leftControllerObject; }
    }

    public float Speed
    {
        get { return _speed; }
    }


    private void Update()
    {
        if (!setLocomotionBehaviour.Equals(currentLocomotionBehaviour))
        {
            currentLocomotionBehaviour = setLocomotionBehaviour;
            changeLocomotionBehaviour(currentLocomotionBehaviour);
        }

        if (_simulateMovePress) return;

        //DebugStuff();
        _rightController = SteamVR_Controller.Input((int) _rightControllerObject.index);
        _leftController = SteamVR_Controller.Input((int) _leftControllerObject.index);

        if (_rightController == null || _leftController == null)
        {
            Debug.LogError("At least one Controller not found");
            return;
        }

        spawnBot();
    }

    private void changeLocomotionBehaviour(LocomotionBehaviour locomotionBehaviour)
    {
        switch (locomotionBehaviour)
        {
            case LocomotionBehaviour.Hover:
                LocomotionHandler.changeLocomotionBehaviour(new LocomotionHover());
                break;
            case LocomotionBehaviour.Tracker:
                LocomotionHandler.changeLocomotionBehaviour(new LocomotionTracker());
                break;
        }
    }

    private void FixedUpdate()
    {
        if (_simulateMovePress)
        {
            LocomotionHandler.moveForward();
            return;
        }

        if (_rightController == null || _leftController == null)
        {
            Debug.LogWarning("At least one Controller not found");
            return;
        }

        movePlayer();
        if (_leftController.GetPress(EVRButtonId.k_EButton_ApplicationMenu))
            VrLocomotionTrackers.Instance.initializeTrackerOrientation();
        if (_rightController.GetPress(EVRButtonId.k_EButton_ApplicationMenu))
            VrLocomotionTrackers.Instance.initializeTrackerHeading();
    }

    private void spawnBot()
    {
        if (_leftController.GetPressDown(SteamVR_Controller.ButtonMask.Grip) ||
            _rightController.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
        {
            Debug.Log("Grip");
            UserAvatarService.Instance.SpawnYBot();
        }
    }

    private void movePlayer()
    {
        _touchpadPressLeft = _leftController.GetPress(_touchpad);
        _touchpadPressRight = _rightController.GetPress(_touchpad);
        if (_touchpadPressLeft && _touchpadPressRight)
            stoppedMovement = false;

        if (!stoppedMovement)
            LocomotionHandler.moveForward();
        else
            LocomotionHandler.stopMoving();
    }

    public void checkIfMovementStopped(float seconds, Func<float> calculateMovementDistancePerFrame)
    {
        StartCoroutine(stopMovementAfterSeconds(seconds, calculateMovementDistancePerFrame()));
    }
    
    IEnumerator stopMovementAfterSeconds(float seconds, float movementDistance)
    {
        yield return new WaitForSeconds(seconds);
        if (Math.Abs(movementDistance) < 0.001)
            stoppedMovement = true;
    }

    private void DebugStuff()
    {
        Debug.DrawRay(_rightControllerObject.transform.position,
            _rightControllerObject.transform.forward, Color.green);
        Debug.DrawRay(_leftControllerObject.transform.position,
            _leftControllerObject.transform.forward, Color.green);
        VrLocomotionTrackers.showAxisForTrackers();
    }
    
}