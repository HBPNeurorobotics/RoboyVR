using Locomotion;
using UnityEngine;

public class SteamVRControllerInput : Singleton<SteamVRControllerInput> {

    [SerializeField] SteamVR_TrackedObject _rightControllerObject;
    public SteamVR_TrackedObject RightControllerObject { get {return _rightControllerObject; }  }
    SteamVR_Controller.Device _rightController;
    [SerializeField] SteamVR_TrackedObject _leftControllerObject;
    public SteamVR_TrackedObject LeftControllerObject { get { return _leftControllerObject; } }
    SteamVR_Controller.Device _leftController;

    Valve.VR.EVRButtonId _touchpad = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;
    bool _touchpadPressLeft = false;
    bool _touchpadPressRight = false;
    [SerializeField] bool _simulateMovePress = false;

    [SerializeField] bool _changeLocomotionBehaviour = false;
    LocomotionBehaviour _locomotionBehaviour = LocomotionBehaviour.hover;

    void Update ()
    {
        if (_changeLocomotionBehaviour)
        {
            _changeLocomotionBehaviour = false;
            _locomotionBehaviour = (LocomotionBehaviour)(((int)_locomotionBehaviour + 1) % (int)LocomotionBehaviour.behaviourCount);
            changeLocomotionBehaviour(_locomotionBehaviour);
        }
        if (_simulateMovePress)
        {
            return;
        }

        //DebugStuff();
        _rightController = SteamVR_Controller.Input((int)_rightControllerObject.index);
        _leftController = SteamVR_Controller.Input((int)_leftControllerObject.index);

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
            case LocomotionBehaviour.hover:
                LocomotionHandler.changeLocomotionBehaviour(new LocomotionHover());
                break;
            case LocomotionBehaviour.tracker:
                LocomotionHandler.changeLocomotionBehaviour(new LocomotionTracker());
                break;
            case LocomotionBehaviour.ghost:
                LocomotionHandler.changeLocomotionBehaviour(new LocomotionGhost());
                break;
            default:
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
    }

    private void spawnBot()
    {
        if (_leftController.GetPressDown(SteamVR_Controller.ButtonMask.Grip) || _rightController.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
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

    void DebugStuff()
    {
        Debug.DrawRay(_rightControllerObject.transform.position, _rightControllerObject.transform.forward, Color.green);
        Debug.DrawRay(_leftControllerObject.transform.position, _leftControllerObject.transform.forward, Color.green);
        Debug.DrawRay(VRLocomotionTrackers.Instance.RightFootTracker.transform.position, VRLocomotionTrackers.Instance.RightFootTracker.transform.forward, Color.green);
        Debug.DrawRay(VRLocomotionTrackers.Instance.LeftFootTracker.transform.position, VRLocomotionTrackers.Instance.LeftFootTracker.transform.forward, Color.green);
        Debug.DrawRay(VRLocomotionTrackers.Instance.HipTracker.transform.position, VRLocomotionTrackers.Instance.HipTracker.transform.forward, Color.green);
        Debug.DrawRay(VRLocomotionTrackers.Instance.RightFootTracker.transform.position, VRLocomotionTrackers.Instance.RightFootTracker.transform.up, Color.red);
        Debug.DrawRay(VRLocomotionTrackers.Instance.LeftFootTracker.transform.position, VRLocomotionTrackers.Instance.LeftFootTracker.transform.up, Color.red);
        Debug.DrawRay(VRLocomotionTrackers.Instance.HipTracker.transform.position, VRLocomotionTrackers.Instance.HipTracker.transform.up, Color.red);
    }
}
