using System.Collections;
using UnityEngine;

public class LeavingManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameController.Instance.OnTransitionTo += TransitionTo;
        GameController.Instance.OnTransitionFrom += TransitionFrom;
    }

    private void TransitionFrom(GameController.State state)
    {
        if (state == GameController.State.TentSelection)
        {
            //Destroy all people
        }
    }

    private void TransitionTo(GameController.State state)
    {
        if (state == GameController.State.TentSelection)
        {
            StartCoroutine(LeavingCoroutine());
        }
    }



    IEnumerator LeavingCoroutine()
    {
        //From each tent emerges a person per square
        //They hammer for a second and deconstruct the tent.
        //Then they walk off to the left
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
