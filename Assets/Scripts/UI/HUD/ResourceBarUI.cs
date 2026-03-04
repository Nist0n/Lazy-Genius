using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace UI.HUD
{
    public class ResourceBarUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private float updateSpeed = 10f;
        
        private float _targetFillAmount;
        private Coroutine _updateCoroutine;
        
        public void Initialize(float current, float max)
        {
            float fillAmount = max > 0 ? current / max : 0;
            if (fillImage) fillImage.fillAmount = fillAmount;
            _targetFillAmount = fillAmount;
            
            UpdateText(current, max);
        }
        
        public void UpdateValue(float current, float max)
        {
            float fillAmount = max > 0 ? current / max : 0;
            _targetFillAmount = fillAmount;
            
            UpdateText(current, max);
            
            if (_updateCoroutine == null && isActiveAndEnabled)
            {
                _updateCoroutine = StartCoroutine(AnimateFill());
            }
        }
        
        private void UpdateText(float current, float max)
        {
            if (valueText)
            {
                valueText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
            }
        }
        
        private IEnumerator AnimateFill()
        {
            while (fillImage && Mathf.Abs(fillImage.fillAmount - _targetFillAmount) > 0.001f)
            {
                fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, _targetFillAmount, Time.deltaTime * updateSpeed);
                yield return null;
            }
            
            if (fillImage) fillImage.fillAmount = _targetFillAmount;
            _updateCoroutine = null;
        }
        
        private void OnDisable()
        {
            if (fillImage) fillImage.fillAmount = _targetFillAmount;
            _updateCoroutine = null;
        }
    }
}
