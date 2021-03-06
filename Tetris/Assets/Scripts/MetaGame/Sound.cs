using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Sound
{
    public SoundEnum SoundEnum;
    public AudioClip Clip;
    public SoundChannel Channel;

    [Range(0, 1)]
    public float Volume = 0.8f;

    [HideInInspector]
    public AudioSource Source;
}

public enum SoundEnum
{
    UiBeep,
    BlockPlacementBeep,
    GameOver,
}

public enum SoundChannel
{
    Music,
    Effects,
}
