using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
    public class CrosshairUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Image crosshairImage;
        [SerializeField] private Color defaultColor = new Color(1, 1, 1, 0.5f);
        [SerializeField] private Color enemyColor = new Color(1, 0, 0, 0.8f);
        [SerializeField] private Color friendlyColor = new Color(0, 1, 0, 0.8f);
        [SerializeField] private float scaleSpeed = 10f;
        [SerializeField] private float hoverScale = 1.2f;

        private RectTransform _rectTransform;
        private Color _targetColor;
        private float _targetScale;
        
        private void Awake()
        {
            if (crosshairImage)
            {
                _rectTransform = crosshairImage.rectTransform;
            }
            
            SetState(CrosshairState.Normal);
        }

        private void Update()
        {
            if (!crosshairImage) return;

            crosshairImage.color = Color.Lerp(crosshairImage.color, _targetColor, Time.deltaTime * 10f);
            
            if (_rectTransform)
            {
                float currentScale = _rectTransform.localScale.x;
                float newScale = Mathf.Lerp(currentScale, _targetScale, Time.deltaTime * scaleSpeed);
                _rectTransform.localScale = new Vector3(newScale, newScale, 1f);
            }
        }

        public void SetState(CrosshairState state)
        {
            switch (state)
            {
                case CrosshairState.Normal:
                    _targetColor = defaultColor;
                    _targetScale = 1f;
                    break;
                case CrosshairState.Enemy:
                    _targetColor = enemyColor;
                    _targetScale = hoverScale;
                    break;
                case CrosshairState.Friendly:
                    _targetColor = friendlyColor;
                    _targetScale = hoverScale;
                    break;
            }
        }
    }

    public enum CrosshairState
    {
        Normal,
        Enemy,
        Friendly
    }
}
