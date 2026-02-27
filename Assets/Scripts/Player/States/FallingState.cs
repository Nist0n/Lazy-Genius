namespace Player.States
{
    public class FallingState : IPlayerState
    {
        private PlayerController _player;
        
        public FallingState(PlayerController playerController)
        {
            _player = playerController;
        }
        
        public void OnEnter()
        {
            // Логика при входе в состояние падения
        }
        
        public void OnUpdate()
        {
            if (_player.Movement)
            {
                if (_player.Movement.IsGrounded)
                {
                    if (_player.Movement.Velocity.magnitude < 0.1f)
                    {
                        _player.StateMachine.ChangeState(PlayerState.Idle);
                    }
                    else if (_player.InputHandler && _player.InputHandler.SprintPressed)
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

