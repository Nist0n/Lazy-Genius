using System;
using Abstractions.Characters;

namespace UI.MainMenu.MVC
{
    public sealed class CharacterSelectionController : IDisposable
    {
        private readonly CharacterSelectionUI _view;
        private readonly ICharacterService _characters;
        private bool _disposed;

        public CharacterSelectionController(CharacterSelectionUI view, ICharacterService characters)
        {
            if (view) _view = view;
            else throw new ArgumentNullException(nameof(view));
            
            _characters = characters;
            if (_characters == null)
            {
                throw new ArgumentNullException(nameof(characters));
            }

            _view.Initialize(_characters);

            _view.OnDeleteConfirmed += OnDeleteConfirmed;

            _characters.CharacterCreated += OnCharacterCreated;
            _characters.CharacterDeleted += OnCharacterDeleted;

            _view.RefreshCharacterList();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _view.OnDeleteConfirmed -= OnDeleteConfirmed;

            _characters.CharacterCreated -= OnCharacterCreated;
            _characters.CharacterDeleted -= OnCharacterDeleted;
        }

        private void OnDeleteConfirmed(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return;
            _characters.DeleteCharacter(guid);
        }

        private void OnCharacterCreated(SaveSystem.CharacterData _)
        {
            _view.RefreshCharacterList();
        }

        private void OnCharacterDeleted(string _)
        {
            _view.RefreshCharacterList();
        }
    }
}

