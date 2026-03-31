using Abstractions.Audio;
using Abstractions.Characters;
using Abstractions.Save;
using Infrastructure.Interactors;
using Infrastructure.Services;
using UnityEngine;

namespace Composition
{
    [DefaultExecutionOrder(-10000)]
    public sealed class ProjectEntrypoint : MonoBehaviour
    {
        private void Awake()
        {
            if (App.IsInitialized)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            var services = new ServiceContainer();
            
            services.Register<IAudioService>(new UnityAudioService());
            services.Register<ICharacterSaveService>(new CharacterSaveInteractor());
            services.Register<ICharacterService>(new CharacterManagerCharacterService());

            App.Initialize(services);
        }
    }
}

