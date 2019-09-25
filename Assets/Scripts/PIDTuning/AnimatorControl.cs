using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning
{
    public class AnimatorControl : MonoBehaviour
    {
        [SerializeField] private GameObject _userAvatar;

        private Animator _animator = null;
        private UserAvatarVisualsIKControl _ikControl = null;

        public bool IsAnimationRunning
        {
            get { return _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f; }
        }

        /// <summary>
        /// This code runs in Awake (instead of Start/OnEnable) since it is a dependency
        /// of multiple other components.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(_userAvatar);

            _animator = _userAvatar.GetComponent<Animator>();
            _ikControl = _userAvatar.GetComponent<UserAvatarVisualsIKControl>();

            Assert.IsNotNull(_animator);
            Assert.IsNotNull(_ikControl);

            // Disable IK in test environment
            _ikControl.ikActive = false;
        }

        public void ResetUserAvatar()
        {
            throw new NotImplementedException();
        }

        public void PlayAnimation(string state)
        {
            _animator.Play(state);
        }

        /// <summary>
        /// Validates that all animation states exist given the current animator
        /// </summary>
        public void ValidateStateList(List<string> states)
        {
            Assert.IsNotNull(_animator);

            foreach (var state in states)
            {
                Assert.IsTrue(_animator.HasState(0, Animator.StringToHash(state)));
            }
        }
    }
}
