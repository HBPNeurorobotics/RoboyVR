using Locomotion;
using UnityEngine;
using Valve.VR;

public class SteamVRControllerInput : Singleton<SteamVRControllerInput>
{
    [SerializeField] private readonly bool _simulateMovePress = false;

    [SerializeField] private readonly float _speed = 0.01f;

    private readonly EVRButtonId _touchpad = EVRButtonId.k_EButton_SteamVR_Touchpad;
    [SerializeField] private bool _changeLocomotionBehaviour;

    private SteamVR_Controller.Device _leftController;
    [SerializeField] private SteamVR_TrackedObject _leftControllerObject;
    private LocomotionBehaviour _locomotionBehaviour = LocomotionBehaviour.Hover;
    private SteamVR_Controller.Device _rightController;

    [SerializeField] private SteamVR_TrackedObject _rightControllerObject;
    private bool _touchpadPressLeft;
    private bool _touchpadPressRight;

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
        if (_changeLocomotionBehaviour)
        {
            _changeLocomotionBehaviour = false;
            _locomotionBehaviour =
                (LocomotionBehaviour) (((int) _locomotionBehaviour + 1) %
                                       (int) LocomotionBehaviour.BehaviourCount);
            changeLocomotionBehaviour(_locomotionBehaviour);
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
            Debug.LogError("At least one Controller not found");
            return;
        }

        movePlayer();
        initializeTrackerWithControllers();
    }

    private void initializeTrackerWithControllers()
    {
        if (_leftController.GetPress(EVRButtonId.k_EButton_ApplicationMenu))
            VrLocomotionTrackers.Instance.initializeTracking();
        if (_rightController.GetPressDown(EVRButtonId.k_EButton_ApplicationMenu))
            VrLocomotionTrackers.Instance.initializeDefaultDistance();
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
            LocomotionHandler.moveForward();
        else
            LocomotionHandler.stopMoving();
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