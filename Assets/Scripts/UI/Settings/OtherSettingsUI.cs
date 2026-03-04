using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Settings
{
    public class OtherSettingsUI : MonoBehaviour
    {
        [Header("Sensitivity Settings")]
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private TextMeshProUGUI sensitivityValueText;

        private void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (!GameSettingsManager.Instance) return;
            
            if (sensitivitySlider)
            {
                float currentSens = GameSettingsManager.Instance.MouseSensitivity;
                sensitivitySlider.value = currentSens;
                UpdateSensitivityText(currentSens);
                
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            }
        }

        private void OnSensitivityChanged(float value)
        {
            GameSettingsManager.Instance.SetMouseSensitivity(value);
            UpdateSensitivityText(value);
        }

        private void UpdateSensitivityText(float value)
        {
            if (sensitivityValueText)
            {
                sensitivityValueText.text = value.ToString("F2");
            }
        }

        private void OnDestroy()
        {
            if (sensitivitySlider)
            {
                sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
            }
        }
    }
}
