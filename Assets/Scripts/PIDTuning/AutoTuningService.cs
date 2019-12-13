using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
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
    [RequireComponent(typeof(TestEnvSetup), typeof(AnimatorControl))]
    public class AutoTuningService : MonoBehaviour
    {
        [SerializeField]
        public UserAvatarService _userAvatarService;

        [SerializeField]
        private GameObject _localAvatar;

        public ZieglerNicholsHeuristic TuningHeuristic = ZieglerNicholsHeuristic.PDControl;

        // TODO: Explain
        public float RelayConstantForce = 500f;

        public float RelayTargetAngle = 0f;

        // TODO: Explain
        public float RelayStartAngle = 15f;

        // TODO: Explain
        public float TestWarmupSeconds = 30f;

        // TODO: Explain
        public float TestDurationSeconds = 30f;

        private TestRunner _testRunner;

        private PidConfigurationStorage _pidConfigStorage;

        public PoseErrorTracker PoseErrorTracker { private set; get; }

        private TestEnvSetup _testEnvSetup;

        private Animator _localAvatarAnimator;

        private RigAngleTracker _localAvatarRig;

        private AnimatorControl _animatorControl;

        private bool _tuningInProgress;

        private bool _gazebo;

        private Dictionary<HumanBodyBones, GameObject> _remoteBones;

        public TuningResult LastTuningData { private set; get; }

        //[Header("Initial PID tuning values")]
        //public float InitialP = 300f;

        //public float InitialI = 30f;

        //public float InitialD = 10f; // https://en.wikipedia.org/wiki/Initial_D

        private void Start()
        {
            Assert.IsNotNull(_userAvatarService);


            _gazebo = _userAvatarService.GetUseGazebo();
            _testRunner = GetComponent<TestRunner>();
            _pidConfigStorage = GetComponent<PidConfigurationStorage>();
            PoseErrorTracker = GetComponent<PoseErrorTracker>();
            _testEnvSetup = GetComponent<TestEnvSetup>();
            _animatorControl = GetComponent<AnimatorControl>();
            _remoteBones = _userAvatarService._avatarManager.GetGameObjectPerBoneAvatarDictionary();

            Assert.IsNotNull(_localAvatar);
            Assert.IsNotNull(_localAvatarAnimator = _localAvatar.GetComponent<Animator>());
            Assert.IsNotNull(_localAvatarRig = _localAvatar.GetComponent<RigAngleTracker>());
        }

        public IEnumerator TuneAllJoints()
        {
            Assert.IsFalse(_tuningInProgress);
            _tuningInProgress = true;

            if (_gazebo)
            {

                foreach (var joint in _localAvatarRig.GetJointToRadianMapping().Keys)
                {
                    yield return TuneSingleJoint(joint);
                }
            }
            else
            {
                foreach(var bone in _localAvatarRig.GetJointMappingsClient().Keys)
                {
                    yield return TuneSingleJoint(bone.ToString());
                }
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
            if (_gazebo)
            {
                _localAvatarRig.SetJointEulerAngle(joint, RelayStartAngle);
            }
            else
            {
                //_localAvatarRig.SetJointQuaternion()
            }

            // Wait for sim to catch up
            yield return _testEnvSetup.RunSimulationReset();

            // Run Bang-Bang control for a while to get oscillation data
            var bangBangStepData = new Box<PidStepData>(null);

            yield return RunBangBangEvaluation(joint, bangBangStepData, _userAvatarService.GetUseGazebo());

            // Evaluate the oscillation data
            var oscillation = PeakAnalysis.AnalyzeOscillation(bangBangStepData.Value);
            try
            {
                Assert.IsTrue(oscillation.HasValue, "It seems the Bang-Bang control did not create a suitable oscillation pattern. Please modify some parameters and try again.");
                LastTuningData = TuningResult.GenerateFromOscillation(joint, oscillation.Value, RelayConstantForce, _animatorControl.TimeStretchFactor);
            }
            finally
            {
                _tuningInProgress = false;
            }

            // Acquire the final tuned parameters
            var tunedPid = ZieglerNicholsTuning.FromBangBangAnalysis(TuningHeuristic, oscillation.Value, RelayConstantForce, _animatorControl.TimeStretchFactor);

            // Apply the tuning and transmit it to the simulation
            _pidConfigStorage.Configuration.Mapping[joint] = tunedPid;
            _pidConfigStorage.TransmitSingleJointConfiguration(joint);

            // Restore animator state
            _localAvatarAnimator.enabled = previousAnimatorState;

            _tuningInProgress = false;
        }

        private IEnumerator RunBangBangEvaluation(string joint, Box<PidStepData> evaluation, bool gazebo)
        {
            // TODO: Explain all of this m8
            TimeSpan warmupSeconds = TimeSpan.FromSeconds(TestWarmupSeconds);
            TimeSpan measurementSeconds = TimeSpan.FromSeconds(TestDurationSeconds);

            // Save PID configuration to restore it later
            var oldPidParameters = _pidConfigStorage.Configuration.Mapping[joint];

            // Disable PID controller
            _pidConfigStorage.Configuration.Mapping[joint] = PidParameters.FromParallelForm(0f, 0f, 0f);
            _pidConfigStorage.TransmitFullConfiguration();

            // Set set-point to 0 (even if the PID won't control the joint for now)
            // We are trying to reach the set-point using relay control only for the test
            _localAvatarRig.SetJointEulerAngle(joint, RelayTargetAngle);

            var startTime = DateTime.Now;

            while (DateTime.Now - startTime < warmupSeconds)
            {
                AdjustRelayForce(joint, gazebo);
                
                yield return null;
            }

            _testRunner.ResetTestRunner();
            _testRunner.StartManualRecord();

            startTime = DateTime.Now;

            while (DateTime.Now - startTime < measurementSeconds)
            {
                AdjustRelayForce(joint, gazebo);

                yield return null;
            }

            // Stop the test and collect data
            evaluation.Value = _testRunner.StopManualRecord()[joint];

            // Get rid of any force
            SetConstantForceForJoint(joint, 0f, gazebo);

            // Restore pre-test configurations
            _pidConfigStorage.Configuration.Mapping[joint] = oldPidParameters;
            _pidConfigStorage.TransmitFullConfiguration();
        }

        private void AdjustRelayForce(string joint, bool gazebo)
        {
            if (1f == Mathf.Sign(PoseErrorTracker.GetCurrentStepDataForJoint(joint).Measured - RelayTargetAngle))
            {
                SetConstantForceForJoint(joint, -RelayConstantForce, gazebo);
            }
            else
            {
                SetConstantForceForJoint(joint, RelayConstantForce, gazebo);
            }
        }

        private void SetConstantForceForJoint(string joint, float force, bool gazebo)
        {
            if (gazebo)
            {
                string topic = "/" + _userAvatarService.avatar_name + "/avatar_ybot/" + joint + "/set_constant_force";

                ROSBridgeService.Instance.websocket.Publish(topic, new Vector3Msg(force, 0f, 0f));
            }
            else
            {
                HumanBodyBones bone = (HumanBodyBones)System.Enum.Parse(typeof(HumanBodyBones), joint);
                GameObject tmp;
                if(_remoteBones.TryGetValue(bone, out tmp)){
                    Rigidbody rb = _remoteBones[bone].GetComponent<Rigidbody>();
                    if(rb != null)
                    {
                        rb.AddForce(new Vector3(force, 0, 0));
                    }
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

    public class TuningResult
    {
        public readonly string Joint;
        public readonly Dictionary<ZieglerNicholsHeuristic, PidParameters> Tunings;
        public readonly Dictionary<ZieglerNicholsHeuristic, JointSettings> ClientTunings;

        private TuningResult(string joint, Dictionary<ZieglerNicholsHeuristic, PidParameters> tunings)
        {
            Joint = joint;
            Tunings = tunings;
        }

        private TuningResult(string joint, Dictionary<ZieglerNicholsHeuristic, JointSettings> tunings)
        {
            Joint = joint;
            ClientTunings = tunings;
        }

        public static TuningResult GenerateFromOscillation(string joint, OscillationAnalysisResult oscillation, float relayConstantForce, float timeStretchFactor)
        {
            var tunings = Enum.GetValues(typeof(ZieglerNicholsHeuristic))
                .Cast<ZieglerNicholsHeuristic>()
                .ToDictionary(variant => variant, variant => ZieglerNicholsTuning.FromBangBangAnalysis(variant, oscillation, relayConstantForce, timeStretchFactor));

            return new TuningResult(joint, tunings);
        }

        public JObject ToJson()
        {
            var json = new JObject();

            json["joint"] = Joint;

            var tuningArray = new JArray();

            foreach (var tuning in Tunings)
            {
                var tuningJson = new JObject();

                tuningJson["heuristic"] = tuning.Key.ToString();
                tuningJson["tuning"] = tuning.Value.ToJson();

                tuningArray.Add(tuningJson);
            }

            json["tunings"] = tuningArray;

            return json;
        }
    }
}