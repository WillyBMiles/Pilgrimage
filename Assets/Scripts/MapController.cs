using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapController : SerializedMonoBehaviour
{
    [SerializeField]
    Button skipButton;

    [SerializeField]
    SpriteRenderer map;

    [SerializeField]
    List<Transform> eachMapPoint = new();

    [SerializeField]
    List<List<SpriteRenderer>> instantiateForAnimation;

    [SerializeField]
    CanvasGroup canvasGroup;

    [SerializeField]
    List<string> storyLines;

    [SerializeField]
    TextMeshProUGUI text;

    bool transitionAway = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0f;
        skipButton.onClick.AddListener(Skip);
        GameController.Instance.OnTransitionTo += TransitionTo;
        GameController.Instance.CreateLocation(0);
    }

    private void TransitionTo(GameController.State state)
    {
        if (state == GameController.State.Map)
        {
            StartCoroutine(ShowMap());
        }
    }

    void Skip()
    {

        transitionAway = true;
        
    }


    IEnumerator ShowMap()
    {
        transitionAway = false;
        float fadeDuration = 1f;
        Vector3 startPos = transform.position - eachMapPoint[GameController.Instance.CurrentRound].position;

        map.transform.parent.localScale = Vector3.one;
        map.transform.position = startPos;

        float zoomSize = 10f;
        SoundManager.PlayMusic(SoundManager.Sound.Music_map);
        map.transform.parent.localScale = new Vector3(zoomSize, zoomSize, zoomSize);
        map.transform.parent.DOScale(1f, fadeDuration);

        foreach (SpriteRenderer sr in shownSprites)
        {
            sr.DOFade(1f, fadeDuration);
        }
        text.text = "";
        text.color = new Color(1f,1f,1f,0f);

        map.DOFade(1f, fadeDuration);
        map.transform.DOMove(new Vector3(), fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        

        canvasGroup.DOFade(1f, 1f);
        canvasGroup.blocksRaycasts = true;

        text.text = storyLines[GameController.Instance.CurrentRound];
        Sequence s = DOTween.Sequence();
        s.Append(text.transform.DOScale(1.1f, 2f));
        s.Join(text.DOFade(1f,1.5f));
        s.Append(text.transform.DOScale(1f, 1f));
        

        GameController.Instance.CreateLocation(GameController.Instance.CurrentRound + 1);
        CurrentGrid.Instance.ClearTents();
        FindFirstObjectByType<PersonManager>().DestroyAllPeople();

        StartCoroutine(ShowPath(instantiateForAnimation[GameController.Instance.CurrentRound]));

        Sequence s2 = DOTween.Sequence();
        s2.AppendInterval(12f);
        s2.AppendCallback(() =>
        {
            if (canvasGroup.alpha == 1f)
            {
                Sequence s3 = DOTween.Sequence();
                s3.Append(skipButton.transform.DOScale(1.5f, .5f));
                s3.Append(skipButton.transform.DOScale(1f, .5f));
            }
        });

        yield return new WaitUntil(() => transitionAway);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.DOFade(0f, 1f);
        yield return new WaitForSeconds(1f);

        Vector3 endPos = transform.position - eachMapPoint[GameController.Instance.CurrentRound + 1].position;

        map.transform.parent.DOScale(zoomSize, fadeDuration);
        map.DOFade(0f, fadeDuration);

        foreach (SpriteRenderer sr in shownSprites)
        {
            sr.DOFade(0f, fadeDuration);
        }
        map.transform.DOLocalMove(endPos, fadeDuration).SetEase(Ease.OutExpo);

        yield return new WaitForSeconds(fadeDuration);

        if (GameController.Instance.IsAnotherRound())
        {
            GameController.Instance.TransitionTo(GameController.State.PreRound);
        }

    }
    List<SpriteRenderer> shownSprites = new();

    IEnumerator ShowPath(List<SpriteRenderer> spriteRenderers)
    {
        shownSprites.AddRange(spriteRenderers);
        float gap = .2f;
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.DOFade(1f, gap);
            yield return new WaitForSeconds(gap);
            if (map.color.a < 1f)
            {
                sr.DOKill();
                sr.DOFade(0f, gap);
                yield break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
