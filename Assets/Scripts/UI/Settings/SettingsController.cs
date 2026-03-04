using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Settings
{
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] private ButtonApplies otherApply;
        [SerializeField] private ButtonApplies volumeApply;
        [SerializeField] private ButtonApplies graphicsApply;
        [SerializeField] private ButtonApplies bindingsApply;
        
        private List<ButtonApplies> _buttonApplies = new List<ButtonApplies>();

        private void OnEnable()
        {
            Initialize();
        }

        public void Initialize()
        {
            SetupUI();
            SetupButtonsActions();
            OnVolumeButtonClicked();
        }

        private void SetupButtonsActions()
        {
            otherApply.button.onClick.AddListener(OnOtherButtonClicked);
            graphicsApply.button.onClick.AddListener(OnGraphicsButtonClicked);
            volumeApply.button.onClick.AddListener(OnVolumeButtonClicked);
            bindingsApply.button.onClick.AddListener(OnBindingsButtonClicked);
        }

        private void SetupUI()
        {
            _buttonApplies.Add(otherApply);
            _buttonApplies.Add(graphicsApply);
            _buttonApplies.Add(bindingsApply);
            _buttonApplies.Add(volumeApply);
        }

        private void OnOtherButtonClicked()
        {
            otherApply.ActivateTab();
            foreach (var apply in _buttonApplies)
            {
                if (apply != otherApply)
                {
                    apply.DeactivateTab();
                }
            }
        }
        
        private void OnVolumeButtonClicked()
        {
            volumeApply.ActivateTab();
            foreach (var apply in _buttonApplies)
            {
                if (apply != volumeApply)
                {
                    apply.DeactivateTab();
                }
            }
        }
        
        private void OnBindingsButtonClicked()
        {
            bindingsApply.ActivateTab();
            foreach (var apply in _buttonApplies)
            {
                if (apply != bindingsApply)
                {
                    apply.DeactivateTab();
                }
            }
        }
        
        private void OnGraphicsButtonClicked()
        {
            graphicsApply.ActivateTab();
            foreach (var apply in _buttonApplies)
            {
                if (apply != graphicsApply)
                {
                    apply.DeactivateTab();
                }
            }
        }
    }
}
