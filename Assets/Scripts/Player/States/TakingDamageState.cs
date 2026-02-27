using UnityEngine;

namespace Player.States
{
    public class TakingDamageState : IPlayerState
    {
        private readonly PlayerController _player;
        private float _stunTimer;

        private const float StunDuration = 0.25f;

        public TakingDamageState(PlayerController playerController)
        {
            _player = playerController;
        }

        public void OnEnter()
        {
            _stunTimer = StunDuration;
            _player.PlAnimator?.PlayHitAnimation();
        }

        public void OnUpdate()
        {
            _stunTimer -= Time.deltaTime;
            if (_stunTimer > 0f)
            {
                return;
            }

            if (!_player.Movement)
            {
                return;
            }

            var velocity = _player.Movement.Velocity;

            if (!_player.Movement.IsGrounded)
            {
                _player.StateMachine.ChangeState(PlayerState.Falling);
                return;
            }

            if (velocity.magnitude < 0.1f)
            {
                _player.StateMachine.ChangeState(PlayerState.Idle);
                return;
            }

            if (_player.InputHandler && _player.InputHandler.SprintPressed)
            {
                _player.StateMachine.ChangeState(PlayerState.Running);
            }
            else
            {
                _player.StateMachine.ChangeState(PlayerState.Walking);
            }
        }

        public void OnFixedUpdate()
        {
        }

        public void OnExit()
        {
        }
    }
}

