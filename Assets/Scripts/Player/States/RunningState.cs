namespace Player.States
{
    public class RunningState : IPlayerState
    {
        private PlayerController _player;
        
        public RunningState(PlayerController playerController)
        {
            _player = playerController;
        }
        
        public void OnEnter()
        {
            // Логика при входе в состояние бега
        }
        
        public void OnUpdate()
        {
            if (_player.Movement)
            {
                if (_player.Movement.Velocity.magnitude < 0.1f)
                {
                    _player.StateMachine.ChangeState(PlayerState.Idle);
                }
                else if (!_player.InputHandler || !_player.InputHandler.SprintPressed)
                {
                    _player.StateMachine.ChangeState(PlayerState.Walking);
                }
                else if (!_player.Movement.IsGrounded)
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

