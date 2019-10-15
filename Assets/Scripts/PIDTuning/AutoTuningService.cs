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
    public class AutoTuningService : MonoBehaviour
    {
        [SerializeField]
        private TestRunner _testRunner;

        [SerializeField]
        private PidConfigurationStorage _pidConfigStorage;

        [SerializeField]
        private Animator _localAvatar;

        private bool _tuningInProgress;

        private void Start()
        {
            Assert.IsNotNull(_testRunner);
            Assert.IsNotNull(_pidConfigStorage);
            Assert.IsNotNull(_localAvatar);
        }

        public IEnumerator TuneAllJoints()
        {
            throw new NotImplementedException();

            Assert.IsFalse(_tuningInProgress);

            _tuningInProgress = true;
            
            // TODO: Tune!

            _tuningInProgress = false;
        }
    }
}