using System;
using Abstractions.Characters;
using Player;

namespace UI.MainMenu.MVC
{
    public sealed class CharacterCreationController : IDisposable
    {
        private readonly CharacterCreationUI _view;
        private readonly ICharacterService _characters;
        private bool _disposed;

        public CharacterCreationController(CharacterCreationUI view, ICharacterService characters)
        {
            if (view) _view = view;
            else throw new ArgumentNullException(nameof(view));
            
            _characters = characters;
            if (_characters == null)
            {
                throw new ArgumentNullException(nameof(characters));
            }

            _view.Initialize(_characters);
            _view.SetAvailableClasses(_characters.GetAvailableClasses());

            _view.CreateRequested += OnCreateRequested;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _view.CreateRequested -= OnCreateRequested;
        }

        private void OnCreateRequested(string name, PlayerClass playerClass)
        {
            var created = _characters.CreateCharacter(name, playerClass);
            if (created != null)
            {
                _view.OnCharacterCreated?.Invoke();
            }
            else
            {
                _view.ShowCreateFailed();
            }
        }
    }
}

