using System;
using UI;

namespace UI.Settings.MVC
{
    public sealed class SettingsTabsController : IDisposable
    {
        private readonly SettingsController _view;
        private bool _disposed;

        public SettingsTabsController(SettingsController view)
        {
            if (view) _view = view;
            else throw new ArgumentNullException(nameof(view));
            
            _view.EnsureInitialized();
            _view.TabClicked += OnTabClicked;
            
            if (_view.VolumeApply)
            {
                _view.SetActiveTab(_view.VolumeApply);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _view.TabClicked -= OnTabClicked;
        }

        private void OnTabClicked(ButtonApplies tab)
        {
            if (!tab) return;
            _view.SetActiveTab(tab);
        }
    }
}

