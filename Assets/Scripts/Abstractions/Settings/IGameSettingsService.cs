using System;

namespace Abstractions.Settings
{
    public interface IGameSettingsService
    {
        float MouseSensitivity { get; }
        event Action<float> SensitivityChanged;
        void SetMouseSensitivity(float value);
    }
}

