using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    public static GameController Instance { get; private set; }

    [SerializeField]
    List<Location> locations;

    [SerializeField]
    List<Tent> startingTents;
    public List<Tent> currentTents;

    public List<Tent> availableTents = new();

    Location currentLocation;
    public Location CurrentLocation => currentLocation;

    [SerializeField]
    AudioMixer mixer;


    public enum State
    {
        StartGame, //Menu screen
        PreRound, //setting up grid
        Placement, // placing all the tent blocks
        Evening, //evening
        TentSelection, // leaving animation choose tents from pool for each new tent earned
        Map, // cleaning up tent blocks, showing map
        Win, //show game win screen
        Lose //show lose game screen
    }

    State currentState;
    public State CurrentState => currentState;

    int currentRound = -1;
    public int CurrentRound => currentRound;


    [SerializeField]
    CanvasGroup loseFood;

    [SerializeField]
    CanvasGroup loseFaith;
    [SerializeField]
    CanvasGroup win;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        else
        {
            SoundManager.PreloadEverything();
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
    }

    private void Start()
    {
        TransitionTo(State.StartGame);
        muteImage.sprite = muted ? muteSprite : unMuteSprite;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateState();
    }

    #region State Machine
    public event Action<State> OnTransitionTo;
    public event Action<State> OnTransitionFrom;

    void UpdateState()
    {
        switch (currentState)
        {
            case State.StartGame:
                break;
            case State.PreRound:
                if (SceneManager.GetActiveScene().buildIndex == 1)
                    TransitionTo(State.Placement);
                break;
            case State.Placement:
                break;
            case State.TentSelection:
                break;
            case State.Map:
                break;

            case State.Win:
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    TransitionTo(State.StartGame);
                }
                break;
            case State.Lose:
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    TransitionTo(State.StartGame);
                }
                break;
        }
    }

    public void TransitionTo(State state)
    {
        if (currentState != state)
            TransitionFrom(currentState);

        currentState = state;
        switch (currentState)
        {
            case State.StartGame:
                SoundManager.PlayMusic(SoundManager.Sound.MainMenu);
                if (SceneManager.GetActiveScene().buildIndex != 0)
                {
                    SceneManager.LoadScene(0);
                }
                OnTransitionTo = null;
                OnTransitionFrom = null;
                currentRound = -1;
                break;
            case State.PreRound:
                currentRound++;

                break;
            case State.Evening:
                StartCoroutine(Night());
                break;
            case State.Placement:
                SoundManager.PlayMusic(currentLocation.music);
                SoundManager.Play(SoundManager.Sound.dayAmbiance1, () => currentState != State.Placement);
                SoundManager.Play(SoundManager.Sound.dayAmbiance2, () => currentState != State.Placement);

                break;
            case State.TentSelection:
            case State.Map:
            case State.Win:
            case State.Lose:

                break;
        }
        OnTransitionTo?.Invoke(state);
    }

    void TransitionFrom(State state)
    {
        switch (state)
        {
            case State.StartGame:
                SceneManager.LoadScene(1);
                currentTents.Clear();
                currentTents.AddRange(startingTents);
                break;
            case State.PreRound:
            case State.Placement:
            case State.TentSelection:
            case State.Map:
            case State.Win:
            case State.Lose:

                break;
        }
        OnTransitionFrom?.Invoke(state);
    }

    #endregion


    public enum LoseReason { 
        OutOfFood,
        OutOfFaith
    }
    public void Lose(LoseReason reason)
    {
        if (currentState == State.Lose)
            return;
        TransitionTo(State.Lose);

        if (reason == LoseReason.OutOfFaith)
        {
            loseFaith.blocksRaycasts = true;
            loseFaith.DOFade(1f, 1f);
            SoundManager.PlayMusic(SoundManager.Sound.Music_faithLoss);
        }
        if (reason == LoseReason.OutOfFood)
        {
            loseFood.blocksRaycasts = true;
            loseFood.DOFade(1f, 1f);
            SoundManager.PlayMusic(SoundManager.Sound.Music_hunger);
        }
    }

    public void CreateLocation(int round)
    {
        if (round < locations.Count)
        {
            EndLocation();
            currentLocation = Instantiate(locations[round]);
        }

    }

    public void CreateWinScreen()
    {
        //YAYYY
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(5f);
        sequence.AppendCallback(() => win.blocksRaycasts = true);
        sequence.Append(win.DOFade(1f, 2f));
    }

    public bool IsAnotherRound()
    {
        return locations.Count > currentRound+1;
    }

    void EndLocation()
    {
        if (currentLocation != null)
            Destroy(currentLocation.gameObject);
    }

    IEnumerator Night()
    {
        UnityEngine.Rendering.VolumeProfile volumeProfile = FindFirstObjectByType<UnityEngine.Rendering.Volume>()?.profile;

        SoundManager.PlayMusic(SoundManager.Sound.nightTime);

        if (!volumeProfile.TryGet<WhiteBalance>(out WhiteBalance whiteBalance)) {
            throw new Exception();
        }
        if (!volumeProfile.TryGet<ColorAdjustments>(out ColorAdjustments colorAdjustments))
        {
            throw new Exception();
        }

        float targetTemp = -55;
        float targetExposure = -2;

        float duration = 2f;

        DOTween.To(() => whiteBalance.temperature.value, v => whiteBalance.temperature.value = v, targetTemp, duration);

        DOTween.To(() => colorAdjustments.postExposure.value, v => colorAdjustments.postExposure.value = v, targetExposure, duration);
        yield return new WaitForSeconds(duration);

        DOTween.To(() => colorAdjustments.postExposure.value, v => colorAdjustments.postExposure.value = v, 0f, duration);
        DOTween.To(() => whiteBalance.temperature.value, v => whiteBalance.temperature.value = v, 0f, duration);

        yield return new WaitForSeconds(duration);

        if (CurrentState == State.Lose)
            yield break;

        SoundManager.StopMusic();

        if (GameController.Instance.IsAnotherRound())
        {
            TransitionTo(State.TentSelection);
            SoundManager.Play(SoundManager.Sound.dayAmbiance1, () => currentState != State.TentSelection);
            SoundManager.Play(SoundManager.Sound.dayAmbiance2, () => currentState != State.TentSelection);
        }
        else
        {
            TransitionTo(GameController.State.Win);
            SoundManager.PlayMusic(SoundManager.Sound.Music_win);
            SoundManager.Play(SoundManager.Sound.dayAmbiance1, () => currentState != State.Win);
            SoundManager.Play(SoundManager.Sound.dayAmbiance2, () => currentState != State.Win);
            CreateWinScreen();
        }




    }

    public void Restart()
    {
        TransitionTo(State.StartGame);
    }
    public void StartGame()
    {
        TransitionTo(State.PreRound);
    }

    [SerializeField]
    Image muteImage;
    [SerializeField]
    Sprite muteSprite;
    [SerializeField]
    Sprite unMuteSprite;
    bool muted;
    public void ToggleMute()
    {
        muted = !muted;
        muteImage.sprite = muted ? muteSprite : unMuteSprite;

        mixer.SetFloat("Volume", muted ? -80f : 0f);
    }
}
