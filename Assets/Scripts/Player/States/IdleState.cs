using UnityEngine;

namespace Player.States
{
    public class IdleState : IPlayerState
    {
        private PlayerController _player;
        
        public IdleState(PlayerController playerController)
        {
            _player = playerController;
        }
        
        public void OnEnter()
        {
            _player.PlAnimator?.PlayIdleAnimation();
        }
        
        public void OnUpdate()
        {
            if (_player.Movement)
            {
                if (_player.Movement.Velocity.magnitude > 0.1f)
                {
                    if (_player.InputHandler && _player.InputHandler.SprintPressed)
                    {
                        _player.StateMachine.ChangeState(PlayerState.Running);
                    }
                    else
                    {
                        _player.StateMachine.ChangeState(PlayerState.Walking);
                    }
                }
            }
        }
        
        public void OnFixedUpdate()
        {
            // Физические обновления
        }
        
        public void OnExit()
        {
            // Логика при выходе из состояния
        }
    }
}

