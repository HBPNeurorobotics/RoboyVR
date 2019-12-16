using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning
{
    public class AnimatorControl : MonoBehaviour
    {
        [SerializeField] private GameObject _localAvatar;

        [SerializeField] private float _timeStretchFactor = 1f;

        public Animator Animator { private set; get; }

        private UserAvatarIKControl _ikControl = null;

        public bool IsAnimationRunning
        {
            get { return 1f > Animator.GetCurrentAnimatorStateInfo(0).normalizedTime; }
        }

        public float TimeStretchFactor
        {
            get { return _timeStretchFactor; }
        }

        /// <summary>
        /// This code runs in Awake (instead of Start/OnEnable) since it is a dependency
        /// of multiple other components.
        /// </summary>
        private void Awake()
        {
            //_localAvatar = UserAvatarService.Instance.LocalAvatar;
            Assert.IsNotNull(_localAvatar);

            Animator = _localAvatar.GetComponent<Animator>();
            _ikControl = _localAvatar.GetComponent<UserAvatarIKControl>();

            Assert.IsNotNull(Animator);
            Assert.IsNotNull(_ikControl);

            PrepareRigForPlayback();

            Debug.LogWarning("The PIDTuningService GameObject is enabled. This will disable tracking and IK functionality.");
        }

        public void ResetUserAvatar()
        {
            PrepareRigForPlayback();
            Animator.Play("idle");
        }

        public IEnumerator StartAnimationPlayback(string state)
        {
            PrepareRigForPlayback();
            Animator.Play(state);

            yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).IsName(state));
        }

        /// <summary>
        /// Validates that all animation states exist given the current animator
        /// </summary>
        public void ValidateStateList(List<string> states)
        {
            Animator = _localAvatar.GetComponent<Animator>();
            Assert.IsNotNull(Animator);

            foreach (var state in states)
            {
                Assert.IsTrue(Animator.HasState(0, Animator.StringToHash(state)));
            }
        }

        private void PrepareRigForPlayback()
        {
            Animator.enabled = true;

            // Disable IK in test environment
            _ikControl.ikActive = false;

            // Apply time stretch to account for low simulations speeds
            Animator.speed = 1f / _timeStretchFactor;
        }
    }
}
