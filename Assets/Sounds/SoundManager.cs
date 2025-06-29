using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public static class SoundManager 
{
    public enum Sound
    {
        click,
        dayAmbiance1,
        dayAmbiance2,
        Footsteps,
        MainMenu,
        Music_early,
        Music_end,
        Music_faithLoss,
        Music_hunger,
        Music_map,
        Music_middle,
        nightTime,
        pickUp,
        placeFoundation,
        putDown,
        smoke,
        exile,
        Music_win,
    }

    static AudioSource music1;
    static AudioSource music2;
    static int currentMusic;

    static Dictionary<Sound, float> volumes = new()
    {
        { Sound.Music_early, .4f},
        { Sound.Music_middle, .4f},
        { Sound.Music_end, .4f},
         { Sound.Music_map, .3f},
           { Sound.dayAmbiance1, .3f},
            { Sound.dayAmbiance2, .3f},
            { Sound.nightTime, .2f},
             { Sound.smoke, .25f},
             { Sound.pickUp, .1f},
             { Sound.putDown, .1f},
             { Sound.Footsteps, .07f},
             { Sound.placeFoundation, .6f},
             { Sound.click, .7f},
             { Sound.exile, .4f},
             { Sound.Music_win, .6f},
    };

    public static void PreloadEverything()
    {
        foreach (var Sound in Enum.GetValues(typeof(Sound)))
        {
            Resources.Load(Sound.ToString());
        }
    }

    public static AudioSource Play(Sound sound, bool autoDelete = true)
    {
        AudioClip clip = (AudioClip)Resources.Load(sound.ToString());

        AudioSource source = ((GameObject)Resources.Load("sound")).GetComponent<AudioSource>();
        AudioSource soundSource = GameObject.Instantiate(source);

        soundSource.volume = volumes.ContainsKey(sound) ? volumes[sound] : 1f;
        soundSource.clip = clip;
        soundSource.pitch = UnityEngine.Random.Range(.9f, 1.1f);
        soundSource.Play();
        GameObject.DontDestroyOnLoad(soundSource.gameObject);
        if (autoDelete)
        {
            Sequence s = DOTween.Sequence();
            s.AppendInterval(clip.length);
            s.AppendCallback(() => {
                if (soundSource.gameObject != null)
                {
                    GameObject.Destroy(soundSource.gameObject);
                }
            });
        }

        

        return soundSource;
    }
    static IEnumerator WaitUntil(Func<bool> until, Action doThis)
    {
        yield return new WaitUntil(until);
        doThis?.Invoke();
    }

    public static void Play(Sound sound, Func<bool> until)
    {
        AudioSource source = Play(sound, false);
        source.loop = true;

        source.GetComponent<SoundObject>().StartCoroutine(
            WaitUntil(until, () =>
            {
                if (source == null)
                    return;

                source.DOFade(0f, .5f);
                Sequence s = DOTween.Sequence();
                s.AppendInterval(.5f);
                s.AppendCallback(() =>
                {
                    if (source != null)
                        GameObject.Destroy(source.gameObject);
                });

            })
            );

        
    }



    public static void PlayMusic(Sound sound)
    {
        string clipLocation = sound.ToString();
        object o = Resources.Load(clipLocation);
        AudioClip clip = (AudioClip)o;
        

        if (music1 == null)
        {
            AudioSource source = ((GameObject) Resources.Load("musicSource")).GetComponent<AudioSource>();
            music1 = GameObject.Instantiate(source);
            GameObject.DontDestroyOnLoad(source.gameObject);
        }
        if (music2 == null)
        {
            AudioSource source = ((GameObject)Resources.Load("musicSource")).GetComponent<AudioSource>();
            music2 = GameObject.Instantiate(source);
            GameObject.DontDestroyOnLoad(source.gameObject);
        }

        float target = volumes.ContainsKey(sound) ? volumes[sound] : 1f;
        if (currentMusic == 1)
        {
            music2.clip = clip;
            music2.Play();
            music2.DOFade(target, .5f);
            music1.DOFade(0f, .5f);
            currentMusic = 2;
        }
        else
        {
            music1.clip = clip;
            music1.Play();
            music1.DOFade(target, .5f);
            music2.DOFade(0f, .5f);
            currentMusic = 1;
        }
    }

    public static void StopMusic()
    {
        music1?.DOFade(0f, .5f);
        music2?.DOFade(0f, .5f);
    }
}
