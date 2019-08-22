using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning
{
    public class AnimatorControl : MonoBehaviour
    {
        [SerializeField] private GameObject _userAvatar;

        private Animator _animator = null;
        private UserAvatarVisualsIKControl _ikControl = null;

        private void OnEnable()
        {
            Assert.IsNotNull(_userAvatar);

            _animator = _userAvatar.GetComponent<Animator>();
            _ikControl = _userAvatar.GetComponent<UserAvatarVisualsIKControl>();

            Assert.IsNotNull(_animator);
            Assert.IsNotNull(_ikControl);
        }

        public void PlayAnimation(string stateName)
        {
            _ikControl.ikActive = false;

            _animator.Play(stateName);
        }

        public CustomYieldInstruction WaitForAnimationToEnd()
        {
            return new PlayAnimationYieldInstruction(_animator);
        }

        private class PlayAnimationYieldInstruction : CustomYieldInstruction
        {
            private readonly Animator _animator;

            public override bool keepWaiting
            {
                get { return _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f; }
            }

            public PlayAnimationYieldInstruction(Animator animator)
            {
                Assert.IsNotNull(_animator = animator);
            }
        }
    }
}
