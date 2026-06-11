using UnityEngine;
using FMODUnity;

public class MusicManager : MonoBehaviour
{
    [Header("Music Events")]
    [SerializeField] private EventReference menuMusicEvent;
    [SerializeField] private EventReference levelMusicEvent;
    [SerializeField] private EventReference winningMusicEvent;

    private FMOD.Studio.EventInstance currentMusicInstance;

    public void PlayMenuMusic()
    {
        SwitchMusic(menuMusicEvent);
    }

    public void PlayLevelMusic()
    {
        SwitchMusic(levelMusicEvent);
    }

    public void PlayWinningMusic()
    {
        SwitchMusic(winningMusicEvent);
    }

    public void StopMusic()
    {
        if (currentMusicInstance.isValid())
        {
            currentMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentMusicInstance.release();
        }
    }

    private void SwitchMusic(EventReference newMusicEvent)
    {
        // Alte Musik stoppen & freigeben
        if (currentMusicInstance.isValid())
        {
            currentMusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentMusicInstance.release();
        }

        // Neue Musik starten
        currentMusicInstance = FMODUnity.RuntimeManager.CreateInstance(newMusicEvent);
        currentMusicInstance.start();
    }
}