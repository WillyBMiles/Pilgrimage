using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AddTentMenu : MonoBehaviour
{


    [SerializeField]
    CanvasGroup canvasGroup;
    [SerializeField]
    List<AddTentInstance> instances = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameController.Instance.OnTransitionTo += TransitionTo;
       
    }

    private void TransitionTo(GameController.State state)
    {
        if (state == GameController.State.TentSelection)
        {
            StartCoroutine(NewTentCoroutine(ResourceManager.Instance.newTents));
            ResourceManager.Instance.newTents = 0;
        }
    }

    IEnumerator NewTentCoroutine(int newTents)
    {
        if (newTents == 0)
        {
            yield return new WaitForSeconds(3f);
        }

        for (int i =0; i < newTents; i++)
        {

            Tent selectedTent = null;
            IEnumerable<Tent> tents = new List<Tent>(GameController.Instance.availableTents);
            tents = tents.OrderBy(t => UnityEngine.Random.value);
            foreach (AddTentInstance instance in instances)
            {
                Tent t = tents.First();
                tents = tents.Skip(1);
                instance.Assign(t, () => selectedTent = t);
            }

            canvasGroup.blocksRaycasts = true;
            canvasGroup.DOFade(1f, 1f);
            yield return new WaitForSeconds(1f);





            yield return new WaitUntil(() => selectedTent != null);

            GameController.Instance.currentTents.Add(selectedTent);

            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0f, 1f);
            yield return new WaitForSeconds(1f);
            foreach (AddTentInstance instance in instances)
            {
                instance.StopShowing();
            }
        }

        GameController.Instance.TransitionTo(GameController.State.Map);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
