namespace Abstractions.Audio
{
    public interface IAudioService
    {
        void PlayMusic(string name);
        void StopMusic();
        void PlaySfx(string name);
    }
}

