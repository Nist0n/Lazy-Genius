using UnityEngine;

namespace Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;

        [Header("Animation States - Locomotion")]
        [SerializeField] private string idleState = "Idle";
        [SerializeField] private string walkForwardState = "WalkForward";
        [SerializeField] private string walkBackwardState = "WalkBackward";
        [SerializeField] private string runForwardState = "RunForward";
        [SerializeField] private string runBackwardState = "RunBackward";

        [Header("Animation States - Combat")]
        [SerializeField] private string hitState = "Hit";
        [SerializeField] private string deathState = "Death";
        [SerializeField] private string meleeAbilityState = "MeleeElbow";
        [SerializeField] private string rangedAbilityState = "RangedShot";

        public enum AbilityAnimationType
        {
            MeleeElbow,
            RangedShot
        }

        public Animator RawAnimator => animator;

        public void PlayIdleAnimation()
        {
            if (!animator || string.IsNullOrEmpty(idleState))
            {
                return;
            }

            animator.Play(idleState);
        }

        public void PlayWalkAnimation(Vector2 moveInput)
        {
            if (!animator)
            {
                return;
            }

            if (moveInput.magnitude <= 0.1f)
            {
                PlayIdleAnimation();
                return;
            }

            bool backward = moveInput.y < -0.1f;
            string stateToPlay;
            if (backward) stateToPlay = walkBackwardState;
            else stateToPlay = walkForwardState;

            if (!string.IsNullOrEmpty(stateToPlay))
            {
                animator.Play(stateToPlay);
            }
        }

        public void PlayRunAnimation(Vector2 moveInput)
        {
            if (!animator)
            {
                return;
            }

            if (moveInput.magnitude <= 0.1f)
            {
                PlayIdleAnimation();
                return;
            }

            bool backward = moveInput.y < -0.1f;
            string stateToPlay;
            if (backward) stateToPlay = runBackwardState;
            else stateToPlay = runForwardState;

            if (!string.IsNullOrEmpty(stateToPlay))
            {
                animator.Play(stateToPlay);
            }
        }

        public void PlayHitAnimation()
        {
            if (!animator || string.IsNullOrEmpty(hitState))
            {
                return;
            }

            animator.Play(hitState);
        }

        public void PlayDeathAnimation()
        {
            if (!animator || string.IsNullOrEmpty(deathState))
            {
                return;
            }

            animator.Play(deathState);
        }

        public void PlayAbilityAnimation(AbilityAnimationType type)
        {
            if (!animator)
            {
                return;
            }

            string stateToPlay = null;

            switch (type)
            {
                case AbilityAnimationType.MeleeElbow:
                    stateToPlay = meleeAbilityState;
                    break;
                case AbilityAnimationType.RangedShot:
                    stateToPlay = rangedAbilityState;
                    break;
            }

            if (!string.IsNullOrEmpty(stateToPlay))
            {
                animator.Play(stateToPlay);
            }
        }
    }
}

