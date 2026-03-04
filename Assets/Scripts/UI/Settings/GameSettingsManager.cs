using System;
using UnityEngine;

namespace UI.Settings
{
    public class GameSettingsManager : MonoBehaviour
    {
        public static GameSettingsManager Instance { get; private set; }

        [Header("Controls")]
        [SerializeField] private float defaultSensitivity = 1.0f;
        [SerializeField] private float minSensitivity = 0.01f;
        [SerializeField] private float maxSensitivity = 10f;

        private const string PrefSensitivity = "MouseSensitivity";

        public float MouseSensitivity { get; private set; }

        public event Action<float> OnSensitivityChanged;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadSettings()
        {
            MouseSensitivity = PlayerPrefs.GetFloat(PrefSensitivity, defaultSensitivity);
        }

        public void SetMouseSensitivity(float value)
        {
            MouseSensitivity = Mathf.Clamp(value, minSensitivity, maxSensitivity);
            PlayerPrefs.SetFloat(PrefSensitivity, MouseSensitivity);
            PlayerPrefs.Save();
            OnSensitivityChanged?.Invoke(MouseSensitivity);
        }
    }
}
