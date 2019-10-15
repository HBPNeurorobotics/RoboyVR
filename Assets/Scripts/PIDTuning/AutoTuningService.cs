using System;
using System.Collections;
using System.Collections.Generic;
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
    [RequireComponent(typeof(TestRunner), typeof(PidConfigurationStorage))]
    public class AutoTuningService : MonoBehaviour
    {
        [SerializeField]
        private GameObject _localAvatar;

        private TestRunner _testRunner;

        private PidConfigurationStorage _pidConfigStorage;

        private Animator _localAvatarAnimator;

        private RigAngleTracker _localAvatarRig;

        private bool _tuningInProgress;

        private void Start()
        {
            Assert.IsNotNull(_testRunner = GetComponent<TestRunner>());
            Assert.IsNotNull(_pidConfigStorage = GetComponent<PidConfigurationStorage>());

            Assert.IsNotNull(_localAvatar);
            Assert.IsNotNull(_localAvatarAnimator = _localAvatar.GetComponent<Animator>());
            Assert.IsNotNull(_localAvatarRig = _localAvatar.GetComponent<RigAngleTracker>());

            // DEBUG
            StartCoroutine(TuneAllJoints());
        }

        public IEnumerator TuneAllJoints()
        {
            Assert.IsFalse(_tuningInProgress);
            _tuningInProgress = true;

            _localAvatarAnimator.enabled = false;

            // TODO: Tune!

            // DEBUG
            float targetAngle = 30f;

            while (true)
            {
                _localAvatarRig.SetJointEulerAngle("mixamorig_LeftArm_y", targetAngle *= -1f);

                yield return new WaitForSeconds(3f);
            }

            _tuningInProgress = false;
        }
    }
}