using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Settings
{
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] private ButtonApplies otherApply;
        [SerializeField] private ButtonApplies volumeApply;
        [SerializeField] private ButtonApplies graphicsApply;
        [SerializeField] private ButtonApplies bindingsApply;
        
        public event Action<ButtonApplies> TabClicked;

        private readonly List<ButtonApplies> _tabs = new List<ButtonApplies>(4);
        private readonly Dictionary<ButtonApplies, UnityAction> _clickHandlers = new Dictionary<ButtonApplies, UnityAction>(4);
        
        public ButtonApplies VolumeApply => volumeApply;

        private bool _isInitialized;

        private void OnEnable()
        {
            EnsureInitialized();
            HookButtons();
        }

        private void OnDisable()
        {
            UnhookButtons();
        }

        public void EnsureInitialized()
        {
            if (_isInitialized) return;

            _tabs.Clear();
            if (otherApply) _tabs.Add(otherApply);
            if (graphicsApply) _tabs.Add(graphicsApply);
            if (bindingsApply) _tabs.Add(bindingsApply);
            if (volumeApply) _tabs.Add(volumeApply);

            _isInitialized = true;
        }

        public void SetActiveTab(ButtonApplies active)
        {
            if (!_isInitialized) EnsureInitialized();

            foreach (var tab in _tabs)
            {
                if (!tab) continue;
                if (tab == active) tab.ActivateTab();
                else tab.DeactivateTab();
            }
        }

        private void HookButtons()
        {
            Hook(otherApply);
            Hook(graphicsApply);
            Hook(volumeApply);
            Hook(bindingsApply);
        }

        private void UnhookButtons()
        {
            Unhook(otherApply);
            Unhook(graphicsApply);
            Unhook(volumeApply);
            Unhook(bindingsApply);
        }

        private void Hook(ButtonApplies apply)
        {
            if (!apply || !apply.button) return;
            if (_clickHandlers.ContainsKey(apply)) return;

            UnityAction handler = () => TabClicked?.Invoke(apply);
            _clickHandlers.Add(apply, handler);
            apply.button.onClick.AddListener(handler);
        }

        private void Unhook(ButtonApplies apply)
        {
            if (!apply || !apply.button) return;
            if (_clickHandlers.TryGetValue(apply, out var handler))
            {
                apply.button.onClick.RemoveListener(handler);
                _clickHandlers.Remove(apply);
            }
        }
    }
}
