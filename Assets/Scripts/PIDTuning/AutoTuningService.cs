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

        public bool mirror = true;
        public bool showUnneededJoints = true;

        private TestRunner _testRunner;

        private PidConfigurationStorage _pidConfigStorage;

        public PoseErrorTracker PoseErrorTracker { private set; get; }

        private TestEnvSetup _testEnvSetup;

        private Animator _localAvatarAnimator;

        private RigAngleTracker _localAvatarRig;
        private RigAngleTracker _remoteAvatarRig;

        private AnimatorControl _animatorControl;

        private bool _tuningInProgress;

        private Dictionary<HumanBodyBones, GameObject> _remoteBones;

        public TuningResult LastTuningData { private set; get; }

        //[Header("Initial PID tuning values")]
        //public float InitialP = 300f;

        //public float InitialI = 30f;

        //public float InitialD = 10f; // https://en.wikipedia.org/wiki/Initial_D

        private void Start()
        {
            Assert.IsNotNull(UserAvatarService.Instance);

            _localAvatar = UserAvatarService.Instance.LocalAvatar;
            Assert.IsNotNull(_localAvatar);
            Assert.IsNotNull(_localAvatarAnimator = _localAvatar.GetComponent<Animator>());
            Assert.IsNotNull(_localAvatarRig = _localAvatar.GetComponent<RigAngleTracker>());

            _testRunner = GetComponent<TestRunner>();
            _pidConfigStorage = GetComponent<PidConfigurationStorage>();
            PoseErrorTracker = GetComponent<PoseErrorTracker>();
            _testEnvSetup = GetComponent<TestEnvSetup>();
            _animatorControl = GetComponent<AnimatorControl>();
            _remoteBones = UserAvatarService.Instance._avatarManager.GetGameObjectPerBoneRemoteAvatarDictionary();
            _remoteAvatarRig = PoseErrorTracker.RemoteRig;
        }

        void OnEnable()
        {
            Assert.IsNotNull(UserAvatarService.Instance);

            _localAvatar = UserAvatarService.Instance.LocalAvatar;
            Assert.IsNotNull(_localAvatar);
            Assert.IsNotNull(_localAvatarAnimator = _localAvatar.GetComponent<Animator>());
            Assert.IsNotNull(_localAvatarRig = _localAvatar.GetComponent<RigAngleTracker>());

            _testRunner = GetComponent<TestRunner>();
            _pidConfigStorage = GetComponent<PidConfigurationStorage>();
            PoseErrorTracker = GetComponent<PoseErrorTracker>();
            _testEnvSetup = GetComponent<TestEnvSetup>();
            _animatorControl = GetComponent<AnimatorControl>();
            _remoteBones = UserAvatarService.Instance._avatarManager.GetGameObjectPerBoneRemoteAvatarDictionary();
            _remoteAvatarRig = PoseErrorTracker.RemoteRig;
        }

        public IEnumerator TuneAllJoints()
        {
            Assert.IsFalse(_tuningInProgress);

            Dictionary<string, float> safeCopy = new Dictionary<string, float>();
            foreach (var joint in _localAvatarRig.GetJointToRadianMapping())
            {
                safeCopy.Add(joint.Key, joint.Value);
            }

            foreach (var joint in safeCopy.Keys)
            {
                yield return TuneSingleJoint(joint, true);
            }

            _tuningInProgress = false;
        }

        public IEnumerator TuneSingleJoint(string joint, bool fromTuneAll = false)
        {
            bool gazebo = UserAvatarService.Instance.use_gazebo;
            //_localAvatarRig.gameObject.GetComponent<Animator>().enabled = false;
            ConfigurableJoint currentJoint = LocalPhysicsToolkit.GetRemoteJointOfCorrectAxisFromString(joint, _remoteBones);

            if (!gazebo)
            {
                _animatorControl.PrepareRigForPlayback();
            }

            //we can just copy the values for the right side from the tuning of the left side, since the values are approximately the same.
            if (!gazebo && fromTuneAll && mirror && joint.StartsWith("Right"))
            {
                Debug.Log("Skipped " + joint + " because of symmetry");
            }
            else
            {
                //if the joint cannot rotate (e.g y axis in the knee) we do not tune it (for quicker results in autotuning)
                if (!gazebo && fromTuneAll && currentJoint.lowAngularXLimit.limit == 0 && currentJoint.highAngularXLimit.limit == 0)
                {
                    Debug.Log("Skipped " + joint + " because of angular limit 0");
                    yield return null;//new WaitForFixedUpdate();
                }
                else
                {

                    // Make sure we are only tuning one joint at a time
                    Assert.IsFalse(_tuningInProgress);
                    _tuningInProgress = true;

                    // Disable animator so we can move joints as we please
                    var previousAnimatorState = _localAvatarAnimator.enabled;
                    _localAvatarAnimator.enabled = false;

                    //unlock ConfigurableJoint
                    LocalPhysicsToolkit.GetRemoteJointOfCorrectAxisFromString(joint, _remoteBones).angularXMotion = ConfigurableJointMotion.Free;

                    // Set starting angle of joint
                    _localAvatarRig.SetJointEulerAngle(joint, RelayStartAngle);

                    // Wait for sim to catch up
                    yield return _testEnvSetup.RunSimulationReset();

                    // Run Bang-Bang control for a while to get oscillation data
                    var bangBangStepData = new Box<PidStepData>(null);

                    yield return RunBangBangEvaluation(joint, bangBangStepData, gazebo, fromTuneAll);

                    /*
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
                    */
                    _tuningInProgress = false;

                    // Restore animator state
                    _localAvatarAnimator.enabled = previousAnimatorState;
                    if (!UserAvatarService.Instance.use_gazebo)
                    {
                        _animatorControl.EnableIKNonGazebo();
                    }
                    _tuningInProgress = false;
                }
            }
        }

        private IEnumerator RunBangBangEvaluation(string joint, Box<PidStepData> evaluation, bool gazebo, bool fromTuneAll)
        {
            // TODO: Explain all of this m8
            TimeSpan warmupSeconds = TimeSpan.FromSeconds(TestWarmupSeconds);
            TimeSpan measurementSeconds = TimeSpan.FromSeconds(TestDurationSeconds);

            OscillationAnalysisResult? oscillation = new OscillationAnalysisResult();

            if (!gazebo)
            {
                //Prepare the remote avatar. We do not want any other joint force to influence the tuning process.
                UserAvatarService.Instance._avatarManager.LockAvatarJointsExceptCurrent(LocalPhysicsToolkit.GetRemoteJointOfCorrectAxisFromString(joint, _remoteBones));
            }

            // Save PID configuration to restore it later
            var oldPidParameters = _pidConfigStorage.Configuration.Mapping[joint];

            // Disable PID controller
            _pidConfigStorage.Configuration.Mapping[joint] = PidParameters.FromParallelForm(0f, 0f, gazebo ? 0f : 0.1f); 
            _pidConfigStorage.TransmitFullConfiguration(false, RelayConstantForce, mirror);

            //Disable ConfigurableJoint
            UserAvatarService.Instance._avatarManager.tuningInProgress = true;
            ConfigurableJoint configurableJoint = LocalPhysicsToolkit.GetRemoteJointOfCorrectAxisFromString(joint, _remoteBones);

            HumanBodyBones bone = (HumanBodyBones)System.Enum.Parse(typeof(HumanBodyBones), joint.Remove(joint.Length - 1));

            //set limb radius at joint based on estimated values for joint radius in template
            float radius = GetRadiusEstimation((HumanBodyBones)System.Enum.Parse(typeof(HumanBodyBones), joint.Remove(joint.Length - 1)));

            //We copy the joint in the avatar template to restore its values later
            ConfigurableJoint configurableJointCopy = UserAvatarService.Instance._avatarManager.GetJointInTemplate(bone, configurableJoint.axis);
            GameObject bodyPart = configurableJoint.gameObject;
            
            //get total mass driven by joint for non gazebo case
            float totalMass = 0;
            if (!gazebo)
            {
                Rigidbody rb = bodyPart.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    foreach (Transform child in bodyPart.transform)
                    {
                        totalMass += MassAdder(child, rb.mass);
                    }
                }
            }
            else
            {
                totalMass = 1;
            }

            // Set set-point to 0 (even if the PID won't control the joint for now)
            // We are trying to reach the set-point using relay control only for the test
            _localAvatarRig.SetJointEulerAngle(joint, RelayTargetAngle);

            var startTime = DateTime.Now;

            while (DateTime.Now - startTime < warmupSeconds)
            {
                yield return gazebo ? null : new WaitForFixedUpdate();
                AdjustRelayForce(joint, gazebo, RelayConstantForce, radius, totalMass, bodyPart, configurableJoint);
            }

            bool valueFound = false;
            int iteration = 1;
            
            while (!valueFound)
            {
                Debug.Log("Tuning " + joint + ", iteration #"+  iteration + " mass: " + totalMass);
                _testRunner.ResetTestRunner();
                _testRunner.StartManualRecord();

                startTime = DateTime.Now;

                while (DateTime.Now - startTime < measurementSeconds)
                {
                    //We have to update in sync with the physics for better results
                    yield return gazebo ? null : new WaitForFixedUpdate();
                    AdjustRelayForce(joint, gazebo, RelayConstantForce, radius, totalMass, bodyPart, configurableJoint);
                }

                // Stop the test and collect data
                evaluation.Value = _testRunner.StopManualRecord()[joint];
                //continue to search for value, this ensures that a value will be found (it may take longer)
                oscillation = PeakAnalysis.AnalyzeOscillation(evaluation.Value);

                valueFound = oscillation.HasValue;

                iteration++;
            }

            //save values for EditAvatarTemplate -> data would be lost after exiting play mode
            if (!gazebo) _pidConfigStorage.TransmitFullConfiguration(true, RelayConstantForce, mirror);

            // Get rid of any force
            SetConstantForceForJoint(joint, 0f, 1, 1, gazebo, bodyPart, configurableJoint);

            // Restore pre-test configurations
            LocalPhysicsToolkit.CopyPasteComponent(configurableJoint, configurableJointCopy);
            UserAvatarService.Instance._avatarManager.UnlockAvatarJoints();
            UserAvatarService.Instance._avatarManager.tuningInProgress = false;

            _pidConfigStorage.Configuration.Mapping[joint] = oldPidParameters;
            _pidConfigStorage.TransmitFullConfiguration(false, RelayConstantForce, mirror);

            LastTuningData = TuningResult.GenerateFromOscillation(joint, oscillation.Value, RelayConstantForce, _animatorControl.TimeStretchFactor);
            // Acquire the final tuned parameters
            var tunedPid = ZieglerNicholsTuning.FromBangBangAnalysis(TuningHeuristic, oscillation.Value, RelayConstantForce, _animatorControl.TimeStretchFactor);

            // Apply the tuning and transmit it to the simulation
            _pidConfigStorage.Configuration.Mapping[joint] = tunedPid;
            _pidConfigStorage.TransmitSingleJointConfiguration(joint, RelayConstantForce);

            yield return gazebo ? null : new WaitForFixedUpdate();

        }

        private void AdjustRelayForce(string joint, bool gazebo, float relay, float radius, float totalMass, GameObject bodyPart, ConfigurableJoint jointInScene)
        {
            if (1f == Mathf.Sign(PoseErrorTracker.GetCurrentStepDataForJoint(joint).Measured - RelayTargetAngle))
            {
                SetConstantForceForJoint(joint, -relay, radius, totalMass, gazebo, bodyPart, jointInScene);
            }
            else
            {
                SetConstantForceForJoint(joint, relay, radius, totalMass, gazebo, bodyPart, jointInScene);
            }
        }

        private void SetConstantForceForJoint(string joint, float force, float radius, float totalMass, bool gazebo, GameObject bodyPart, ConfigurableJoint jointInScene)
        {
            if (gazebo)
            {
                string topic = "/" + UserAvatarService.Instance.avatar_name + "/avatar_ybot/" + joint + "/set_constant_force";

                ROSBridgeService.Instance.websocket.Publish(topic, new Vector3Msg(force, 0f, 0f));
            }
            else
            {
                Rigidbody rb = bodyPart.GetComponent<Rigidbody>();
                if (force == 0)
                {
                    /*
                    * These lines might look like cheating, but we have to make sure that the starting orientation of the re-enabled ConfigurableJoint matches exactly the rotatation of the local avatar.
                    * Otherwise there will always be an offset in the movement.
                    */
                    rb.velocity = rb.angularVelocity = Vector3.zero;
                    _remoteAvatarRig.SetJointEulerAngle(joint, 0);
                }
                else
                {
                    if (rb != null)
                    {
                        //apply torque in direction that the joint can rotate in

                        //rb.AddTorque(jointInScene.axis * force, ForceMode.Force);
                        //jointInScene.targetAngularVelocity = force * new Vector3(1,0,0) / (rb.mass * Time.fixedDeltaTime);
                        /*
                        float combinedMass = rb.mass;
                        foreach (Transform child in bodyPart.transform)
                        {
                            MassAdder(child, combinedMass);
                        }

                        rb.angularVelocity += Time.fixedDeltaTime * jointInScene.axis * force / combinedMass;
                        */


                        jointInScene.targetAngularVelocity = Mathf.Sign(force) * new Vector3(1e8f, 0, 0);//Mathf.Sqrt(Mathf.Abs(force)/(totalMass * radius)), 0, 0);
                    }
                }
            }
        }

        float GetRadiusEstimation(HumanBodyBones bone)
        {
            switch (bone)
            {
                case HumanBodyBones.Spine:
                case HumanBodyBones.Chest:
                case HumanBodyBones.UpperChest:     return 0.23f;

                case HumanBodyBones.LeftUpperLeg:
                case HumanBodyBones.RightUpperLeg:  return 0.16f;

                case HumanBodyBones.LeftUpperArm:
                case HumanBodyBones.RightUpperArm:
                case HumanBodyBones.Neck:
                case HumanBodyBones.Head:           return 0.12f;

                case HumanBodyBones.LeftLowerArm:
                case HumanBodyBones.RightLowerArm:
                case HumanBodyBones.LeftFoot:
                case HumanBodyBones.RightFoot:      return 0.08f;

                case HumanBodyBones.LeftToes:      
                case HumanBodyBones.RightToes:      return 0.05f;

                case HumanBodyBones.LeftHand:
                case HumanBodyBones.RightHand:      return 0.06f;

                case HumanBodyBones.LeftThumbProximal:  
                case HumanBodyBones.RightThumbProximal:     return 0.04f;

                case HumanBodyBones.LeftIndexProximal:
                case HumanBodyBones.LeftMiddleProximal:
                case HumanBodyBones.LeftRingProximal:
                case HumanBodyBones.LeftLittleProximal:
                case HumanBodyBones.RightIndexProximal:
                case HumanBodyBones.RightMiddleProximal:
                case HumanBodyBones.RightRingProximal:
                case HumanBodyBones.RightLittleProximal:    return 0.025f;

                default: return 0.015f;
            }
        }

        float MassAdder(Transform parent, float mass)
        {
            Rigidbody rbParent = parent.gameObject.GetComponent<Rigidbody>();
            if (rbParent != null)
            {
                mass += rbParent.mass;
            }
            else
            {
                return 0;
            }

            foreach (Transform child in parent.transform)
            {
                Rigidbody rb = child.gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                   mass = MassAdder(child, mass);
                }
            }

            return mass;
        }

        /// <summary>
        /// We use this thing to get around the fact that iterator methods
        /// cannot properly return stuff (including in the form of ref or out
        /// parameters).
        ///
        /// With a Box, we can just change the inner value whenever we please.
        /// </summary>
        class Box<T>
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