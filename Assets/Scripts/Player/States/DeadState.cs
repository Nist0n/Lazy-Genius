namespace Player.States
{
    public class DeadState : IPlayerState
    {
        private PlayerController _player;
        
        public DeadState(PlayerController playerController)
        {
            _player = playerController;
        }
        
        public void OnEnter()
        {
            if (_player.Movement)
            {
                _player.Movement.SetDead(true);
            }
        }
        
        public void OnUpdate()
        {
        }
        
        public void OnFixedUpdate()
        {
        }
        
        public void OnExit()
        {
            if (_player.Movement)
            {
                _player.Movement.SetDead(false);
            }
        }
    }
}

