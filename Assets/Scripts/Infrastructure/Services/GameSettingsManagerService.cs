using System;
using Abstractions.Settings;
using UI.Settings;

namespace Infrastructure.Services
{
    public sealed class GameSettingsManagerService : IGameSettingsService
    {
        public float MouseSensitivity => GameSettingsManager.Instance.MouseSensitivity;

        public event Action<float> SensitivityChanged
        {
            add => GameSettingsManager.Instance.OnSensitivityChanged += value;
            remove => GameSettingsManager.Instance.OnSensitivityChanged -= value;
        }

        public void SetMouseSensitivity(float value) => GameSettingsManager.Instance.SetMouseSensitivity(value);
    }
}

