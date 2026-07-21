using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class SFXActor : MonoBehaviour
{
    public bool RandomizePitch = false;
    [SerializeField] protected AudioSource _source;
    [SerializeField] protected AudioClip[] _clips;
    [SerializeField] protected Vector2 _pitchRange = new Vector2(0.9f, 1.1f);

    protected Dictionary<SFX, AudioClip> _dClips = new();
    protected virtual void Awake()
    {
        _source.mute = true;
        foreach (var clip in _clips)
        {
            if (Enum.TryParse(clip.name, out SFX key))
            { 
                _dClips[key] = clip;
            }
            else
            {
                #if UNITY_EDITOR
                    UnityEngine.Debug.LogError($"Clip name '{clip.name}' doesn't match SFX enum", this);
                #endif
            }
            
        }
        Invoke(nameof(UnmuteSource), 3f);
    }
    public virtual void PlaySound(SFX sfx)
    {
        if (_dClips.TryGetValue(sfx, out var clip))
            Play(clip);
        else
            LogMissing(sfx.ToString());
    }
    public virtual void PlaySound(SFX sfx, float pitch)
    {
        if (_dClips.TryGetValue(sfx, out var clip))
            Play(clip, pitch);
        else
            LogMissing(sfx.ToString());
    }

    public virtual void PlaySound_BUTTON(string sfx)
    {
        if (Enum.TryParse(sfx, out SFX key))
            PlaySound(key);
        else
            LogMissing(sfx);
    }

    private void Play(AudioClip clip)
    {
        if (RandomizePitch) _source.pitch = RandomPitch;
        _source.PlayOneShot(clip);
    }
    private void Play(AudioClip clip, float pitch)
    {
        _source.pitch = pitch;
        _source.PlayOneShot(clip);
    }

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    protected virtual void LogMissing(string name)
    {
        #if UNITY_EDITOR
                UnityEngine.Debug.LogError($"Clip {name} not found", this);
        #endif
    }
    protected virtual void UnmuteSource()
    {
        _source.mute = false;
    }
    protected float RandomPitch
    {
        get { return UnityEngine.Random.Range(_pitchRange.x, _pitchRange.y); }
    }
}
public enum SFX
{
    CheckPoint,
    FootstepGrass,
    Jump,
    ItemPickup,
    ItemRelease,
    RockHit,
    Select,
    Submit
}
