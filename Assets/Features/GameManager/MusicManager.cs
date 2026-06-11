using UnityEngine;
using FMODUnity;

public class MusicManager : MonoBehaviour
{
    [Header("Music Events")]
    [SerializeField] private EventReference menuMusicEvent;
    [SerializeField] private EventReference levelMusicEvent;
    [SerializeField] private EventReference winningMusicEvent;
    [SerializeField] private EventReference donkeyHoovesEvent;
    private FMOD.Studio.EventInstance donkeyHoovesInstance;
    private bool isHoovesSoundPlaying = false;

    private FMOD.Studio.EventInstance currentMusicInstance;

    public void PlayMenuMusic()
    {
        SwitchMusic(menuMusicEvent);
    }

    public void PlayLevelMusic()
    {
        SwitchMusic(levelMusicEvent);
    }

    public void PlayDonkeyHoovesSound()
    {
        if (!isHoovesSoundPlaying)
        {
            donkeyHoovesInstance = FMODUnity.RuntimeManager.CreateInstance(donkeyHoovesEvent);
            donkeyHoovesInstance.start();
            isHoovesSoundPlaying = true;
        }
    }

    public void StopDonkeyHoovesSound()
    {
        if (isHoovesSoundPlaying)
        {
            donkeyHoovesInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            donkeyHoovesInstance.release();
            isHoovesSoundPlaying = false;
        }
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
            currentMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentMusicInstance.release();
        }

        // Neue Musik starten
        currentMusicInstance = FMODUnity.RuntimeManager.CreateInstance(newMusicEvent);
        currentMusicInstance.start();
    }
}