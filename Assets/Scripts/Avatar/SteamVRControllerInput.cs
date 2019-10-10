using Locomotion;
using UnityEngine;
using Valve.VR;

public class SteamVRControllerInput : Singleton<SteamVRControllerInput>
{
    private const EVRButtonId initialzizeTrackerOrientationButton =
        EVRButtonId.k_EButton_ApplicationMenu;

    private const EVRButtonId initializeTrackerHeadingButton =
        EVRButtonId.k_EButton_ApplicationMenu;

    private const EVRButtonId movementButton = EVRButtonId.k_EButton_SteamVR_Touchpad;

    private const float fixedUpdateRefreshRate = 60;

    private SteamVR_Controller.Device _leftController;
    [SerializeField] private SteamVR_TrackedObject _leftControllerObject;
    private SteamVR_Controller.Device _rightController;
    [SerializeField] private SteamVR_TrackedObject _rightControllerObject;

    [SerializeField] private bool _simulateMovePress;
    private LocomotionBehaviour currentLocomotionBehaviour;
    private bool movementButtonPressedLeft;

    private bool movementButtonPressedRight;
    [SerializeField] private LocomotionBehaviour setLocomotionBehaviour;
    [SerializeField] private float speedInMPerS = 7f;
    private bool stoppedMovement = true;

    public SteamVR_TrackedObject RightControllerObject
    {
        get { return _rightControllerObject; }
    }

    public SteamVR_TrackedObject LeftControllerObject
    {
        get { return _leftControllerObject; }
    }

    public float SpeedPerFrame
    {
        get { return speedInMPerS / fixedUpdateRefreshRate; }
    }

    private void Start()
    {
        currentLocomotionBehaviour = setLocomotionBehaviour;
        changeLocomotionBehaviour(currentLocomotionBehaviour);
    }

    private void Update()
    {
        if (!setLocomotionBehaviour.Equals(currentLocomotionBehaviour))
        {
            currentLocomotionBehaviour = setLocomotionBehaviour;
            changeLocomotionBehaviour(currentLocomotionBehaviour);
        }

        if (_simulateMovePress) return;
        
        _rightController = SteamVR_Controller.Input((int) _rightControllerObject.index);
        _leftController = SteamVR_Controller.Input((int) _leftControllerObject.index);

        if (_rightController == null || _leftController == null)
        {
            Debug.LogError("At least one Controller not found");
            return;
        }

        //Doesn't work in fixed Update
        movementButtonPressed();

        initializeTracking();

        spawnBot();
    }

    private void initializeTracking()
    {
        if (_leftController.GetPress(initialzizeTrackerOrientationButton))
            VrLocomotionTrackers.Instance.initializeTrackerOrientation();
        if (_rightController.GetPress(initializeTrackerHeadingButton))
            VrLocomotionTrackers.Instance.initializeTrackerHeading();
    }

    private void movementButtonPressed()
    {
        if (_rightController.GetPressDown(movementButton) ||
            _leftController.GetPressDown(movementButton))
        {
            stoppedMovement = !stoppedMovement;
            Debug.Log("Toggle");
        }
    }

    private void changeLocomotionBehaviour(LocomotionBehaviour locomotionBehaviour)
    {
        switch (locomotionBehaviour)
        {
            case LocomotionBehaviour.Hover:
                LocomotionHandler.changeLocomotionBehaviour(new LocomotionHover());
                break;
            case LocomotionBehaviour.WalkInPlace:
                LocomotionHandler.changeLocomotionBehaviour(new LocomotionWalkInPlace());
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

        //Needs Fixed Update for speed calculation
        movePlayer();
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
        if (!stoppedMovement)
            LocomotionHandler.moveForward();
        else
            LocomotionHandler.stopMoving();
    }
}