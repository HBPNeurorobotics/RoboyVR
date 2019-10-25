using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ROSBridgeLib.geometry_msgs;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning
{
    /// <summary>
    /// Provides logic to determine a decent PID tuning for any humanoid avatar.
    /// Requirements:
    /// - Both local and remote avatar have a mapping via RigAngleTracker
    /// - When local and remote avatar have the same pose, PoseErrorTracker should
    ///   report an error of close to 0 for all joints. You can check this in the
    ///   the editor window "PID Test Ctrl"
    /// - There is no use interference during the tuning step. Hands off baby!
    ///
    /// TODO: This class could utilize symmetries (like left arm/right arm) to save some time.
    /// </summary>
    [RequireComponent(typeof(TestRunner), typeof(PidConfigurationStorage), typeof(PoseErrorTracker))]
    [RequireComponent(typeof(TestEnvSetup))]
    public class AutoTuningService : MonoBehaviour
    {
        private const float RELAY_CONSTANT_FORCE = 500f;
        const float RELAY_TEST_EXTREME_ANGLE = 15f;

        [SerializeField]
        public UserAvatarService _userAvatarService;

        [SerializeField]
        private GameObject _localAvatar;

        private TestRunner _testRunner;

        private PidConfigurationStorage _pidConfigStorage;

        private PoseErrorTracker _poseErrorTracker;

        private TestEnvSetup _testEnvSetup;

        private Animator _localAvatarAnimator;

        private RigAngleTracker _localAvatarRig;

        private bool _tuningInProgress;

        //[Header("Initial PID tuning values")]
        //public float InitialP = 300f;

        //public float InitialI = 30f;

        //public float InitialD = 10f; // https://en.wikipedia.org/wiki/Initial_D

        private void Start()
        {
            Assert.IsNotNull(_userAvatarService);

            _testRunner = GetComponent<TestRunner>();
            _pidConfigStorage = GetComponent<PidConfigurationStorage>();
            _poseErrorTracker = GetComponent<PoseErrorTracker>();
           _testEnvSetup = GetComponent<TestEnvSetup>();

            Assert.IsNotNull(_localAvatar);
            Assert.IsNotNull(_localAvatarAnimator = _localAvatar.GetComponent<Animator>());
            Assert.IsNotNull(_localAvatarRig = _localAvatar.GetComponent<RigAngleTracker>());
        }

        public IEnumerator TuneAllJoints()
        {
            Assert.IsFalse(_tuningInProgress);
            _tuningInProgress = true;

            foreach (var joint in _localAvatarRig.GetJointToRadianMapping().Keys)
            {
                yield return TuneSingleJoint(joint);
            }

            _tuningInProgress = false;
        }

        public IEnumerator TuneSingleJoint(string joint)
        {
            // Make sure we are only tuning one joint at a time
            Assert.IsFalse(_tuningInProgress);
            _tuningInProgress = true;

            // Disable animator so we can move joints as we please
            var previousAnimatorState = _localAvatarAnimator.enabled;
            _localAvatarAnimator.enabled = false;

            // Set starting angle of joint
            _localAvatarRig.SetJointEulerAngle(joint, RELAY_TEST_EXTREME_ANGLE);

            // Wait for sim to catch up
            yield return _testEnvSetup.RunSimulationReset();

            var bangBangEval = new Box<PerformanceEvaluation>(null);

            yield return RunBangBangEvaluation(joint, bangBangEval);

            // Restore animator state
            _localAvatarAnimator.enabled = previousAnimatorState;

            _tuningInProgress = false;
        }

        private IEnumerator RunBangBangEvaluation(string joint, Box<PerformanceEvaluation> evaluation)
        {
            // TODO: Explain all of this m8
            TimeSpan WARMUP_SECONDS = TimeSpan.FromSeconds(30.0);
            TimeSpan MEASUREMENT_SECONDS = TimeSpan.FromSeconds(30f);

            // Save PID configuration to restore it later
            var oldPidParameters = _pidConfigStorage.Configuration.Mapping[joint];

            // Disable PID controller
            _pidConfigStorage.Configuration.Mapping[joint] = PidParameters.FromParallelForm(0f, 0f, 0f);
            _pidConfigStorage.TransmitPidConfiguration();

            // Set set-point to 0 (even if the PID won't control the joint for now)
            // We are trying to reach the setpoint using relay control only
            _localAvatarRig.SetJointEulerAngle(joint, 0f);

            var startTime = DateTime.Now;

            while (DateTime.Now - startTime < WARMUP_SECONDS)
            {
                AdjustRelayForce(joint);
                
                yield return null;
            }

            _testRunner.ResetTestRunner();
            _testRunner.StartManualRecord();

            startTime = DateTime.Now;

            while (DateTime.Now - startTime < MEASUREMENT_SECONDS)
            {
                AdjustRelayForce(joint);

                yield return null;
            }

            // Stop the test and collect data
            evaluation.Value = _testRunner.StopManualRecord()[joint];

            // Get rid of any force
            SetConstantForceForJoint(joint, 0f);

            // Restore pre-test configurations
            _pidConfigStorage.Configuration.Mapping[joint] = oldPidParameters;
            _pidConfigStorage.TransmitPidConfiguration();
        }

        private void AdjustRelayForce(string joint)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (1f == Mathf.Sign(_poseErrorTracker.GetCurrentStepDataForJoint(joint).Measured))
            {
                SetConstantForceForJoint(joint, -RELAY_CONSTANT_FORCE);
            }
            else
            {
                SetConstantForceForJoint(joint, RELAY_CONSTANT_FORCE);
            }
        }

        private void SetConstantForceForJoint(string joint, float force)
        {
            string topic = "/" + _userAvatarService.avatar_name + "/avatar_ybot/" + joint + "/set_constant_force";

            ROSBridgeService.Instance.websocket.Publish(topic, new Vector3Msg(force, 0f, 0f));
        }

        /// <summary>
        /// We use this thing to get around the fact that iterator methods
        /// cannot properly return stuff (including in the form of ref or out
        /// parameters).
        ///
        /// With a Box, we can just change the inner value whenever we please.
        /// </summary>
        private class Box<T>
        {
            public T Value;

            public Box(T value)
            {
                Value = value;
            }
        }
    }
}