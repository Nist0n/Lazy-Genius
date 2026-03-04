using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerStateMachine : MonoBehaviour
    {
        private Dictionary<PlayerState, IPlayerState> _states = new Dictionary<PlayerState, IPlayerState>();
        private IPlayerState _currentState;
        private PlayerState _currentStateType;
        
        public PlayerState CurrentStateType => _currentStateType;
        
        public void RegisterState(PlayerState stateType, IPlayerState state)
        {
            _states[stateType] = state;
        }
        
        public void ChangeState(PlayerState newStateType)
        {
            if (_currentState != null && _currentStateType == newStateType) return;
            if (!_states.ContainsKey(newStateType)) return;
            
            _currentState?.OnExit();
            
            _currentState = _states[newStateType];
            _currentStateType = newStateType;
            _currentState?.OnEnter();
        }
        
        private void Update()
        {
            _currentState?.OnUpdate();
        }
        
        private void FixedUpdate()
        {
            _currentState?.OnFixedUpdate();
        }
    }
    
    public interface IPlayerState
    {
        void OnEnter();
        void OnUpdate();
        void OnFixedUpdate();
        void OnExit();
    }
}

