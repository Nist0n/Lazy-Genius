using Abstractions.Audio;
using Audio;

namespace Infrastructure.Services
{
    public sealed class UnityAudioService : IAudioService
    {
        public void PlayMusic(string name) => AudioManager.Instance.PlayMusic(name);
        public void StopMusic() => AudioManager.Instance.StopMusic();
        public void PlaySfx(string name) => AudioManager.Instance.PlaySFX(name);
    }
}

