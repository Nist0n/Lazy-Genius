namespace Player.States
{
    public class JumpingState : IPlayerState
    {
        private PlayerController _player;
        
        public JumpingState(PlayerController playerController)
        {
            _player = playerController;
        }
        
        public void OnEnter()
        {
            // Логика при входе в состояние прыжка
        }
        
        public void OnUpdate()
        {
            if (_player.Movement)
            {
                if (_player.Movement.IsGrounded && _player.Movement.Velocity.y <= 0.1f)
                {
                    if (_player.Movement.Velocity.magnitude < 0.1f)
                    {
                        _player.StateMachine.ChangeState(PlayerState.Idle);
                    }
                    else
                    {
                        _player.StateMachine.ChangeState(PlayerState.Walking);
                    }
                }
                else if (_player.Movement.Velocity.y < 0)
                {
                    _player.StateMachine.ChangeState(PlayerState.Falling);
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

