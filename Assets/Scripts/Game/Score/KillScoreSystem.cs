using System;
using Abstractions.Audio;
using Game.Events;
using UnityEngine;

namespace Game.Score
{
    public sealed class KillScoreSystem : MonoBehaviour
    {
        [SerializeField] [Min(1)] private int victoryScoreThreshold = 5;
        [SerializeField] private string victoryMusicName = "VictoryMusic";

        private int _score;
        private bool _victoryMusicPlayed;
        private IAudioService _audio;

        public int CurrentScore => _score;

        public event Action<int> ScoreChanged;

        public void Initialize(IAudioService audio)
        {
            _audio = audio;
        }

        private void OnEnable()
        {
            GameEvents.MobKilled += HandleMobKilled;
        }

        private void OnDisable()
        {
            GameEvents.MobKilled -= HandleMobKilled;
        }

        private void HandleMobKilled(GameObject _)
        {
            _score++;
            ScoreChanged?.Invoke(_score);

            TryPlayVictoryMusic();
        }

        private void TryPlayVictoryMusic()
        {
            if (_victoryMusicPlayed || _score < victoryScoreThreshold || _audio == null)
            {
                return;
            }

            _victoryMusicPlayed = true;
            _audio.PlayMusic(victoryMusicName);
        }
    }
}
