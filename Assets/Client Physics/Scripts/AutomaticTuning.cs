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
    public class AutomaticTuning : MonoBehaviour
    {
        [SerializeField]
        public UserAvatarService _userAvatarService;

        [SerializeField]
        private GameObject _localAvatar;

        public ZieglerNicholsVariant TuningVariant = ZieglerNicholsVariant.Classic;

        // TODO: Explain
        public float RelayConstantForce = 500f;

        // TODO: Explain
        public float RelayTestStartAngle = 15f;

        // TODO: Explain
        public float TestWarmupSeconds = 30f;

        // TODO: Explain
        public float TestDurationSeconds = 30f;

        private TestRunner _testRunner;

        private PidConfigurationStorage _pidConfigStorage;

        private PoseErrorTracker _poseErrorTracker;

        private TestEnvSetup _testEnvSetup;

        private Animator _localAvatarAnimator;

        private RigAngleTracker _localAvatarRig;

        private bool _tuningInProgress;

        private AvatarManager avatarManager;

        //[Header("Initial PID tuning values")]
        //public float InitialP = 300f;

        //public float InitialI = 30f;

        //public float InitialD = 10f; // https://en.wikipedia.org/wiki/Initial_D

        private void Start()
        {
            avatarManager = gameObject.GetComponent<AvatarManager>();
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
            _localAvatarRig.SetJointEulerAngle(joint, RelayTestStartAngle);

            // Wait for sim to catch up
            yield return _testEnvSetup.RunSimulationReset();

            // Run Bang-Bang control for a while to get oscillation data
            var bangBangStepData = new Box<PidStepData>(null);

            yield return RunBangBangEvaluation(joint, bangBangStepData);

            // Evaluate the oscillation data
            var evaluation = PeakAnalysis.AnalyzeOscillation(bangBangStepData.Value);
            try
            {
                Assert.IsTrue(evaluation.HasValue, "It seems the Bang-Bang control did not create a suitable oscillation pattern. Please modify some parameters and try again.");
            }
            finally
            {
                _tuningInProgress = false;
            }

            // Acquire the final tuned parameters
            var tunedPid = ZieglerNicholsTuning.FromBangBangAnalysis(TuningVariant, evaluation.Value, RelayConstantForce);

            // Apply the tuning and transmit it to the simulation
            _pidConfigStorage.Configuration.Mapping[joint] = tunedPid;
            _pidConfigStorage.TransmitPidConfiguration();

            // Restore animator state
            _localAvatarAnimator.enabled = previousAnimatorState;

            _tuningInProgress = false;
        }

        private IEnumerator RunBangBangEvaluation(string joint, Box<PidStepData> evaluation)
        {
            // TODO: Explain all of this m8
            TimeSpan warmupSeconds = TimeSpan.FromSeconds(TestWarmupSeconds);
            TimeSpan measurementSeconds = TimeSpan.FromSeconds(TestDurationSeconds);

            // Save PID configuration to restore it later
            var oldPidParameters = _pidConfigStorage.Configuration.Mapping[joint];

            // Disable PID controller
            _pidConfigStorage.Configuration.Mapping[joint] = PidParameters.FromParallelForm(0f, 0f, 0f);
            _pidConfigStorage.TransmitPidConfiguration();

            // Set set-point to 0 (even if the PID won't control the joint for now)
            // We are trying to reach the set-point using relay control only for the test
            _localAvatarRig.SetJointEulerAngle(joint, 0f);

            var startTime = DateTime.Now;

            while (DateTime.Now - startTime < warmupSeconds)
            {
                AdjustRelayForce(joint);

                yield return null;
            }

            _testRunner.ResetTestRunner();
            _testRunner.StartManualRecord();

            startTime = DateTime.Now;

            while (DateTime.Now - startTime < measurementSeconds)
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
                SetConstantForceForJoint(joint, -RelayConstantForce);
            }
            else
            {
                SetConstantForceForJoint(joint, RelayConstantForce);
            }
        }

        private void SetConstantForceForJoint(string joint, float force)
        {
            string topic = "/" + _userAvatarService.avatar_name + "/avatar_ybot/" + joint + "/set_constant_force";
            Dictionary <HumanBodyBones, GameObject> dictionary = avatarManager.GetGameObjectPerBoneAvatarDictionary();
            foreach (GameObject obj in dictionary.Values)
            {
                if (obj.name.Equals(joint))
                {
                    obj.GetComponent<Rigidbody>().AddForce(new Vector3(force, 0f, 0f));
                    return;
                }
            }    
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