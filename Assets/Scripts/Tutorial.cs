using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField]
    List<CanvasGroup> canvasGroups;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            Next();
        }
        cd -= Time.deltaTime;
    }

    int current = 0;
    float cd = 1f;
    void Next()
    {
        if (cd > 0f)
        {
            return;
        }
        cd = 1f;

        canvasGroups[current].DOFade(0f, 0f);
        if (current + 1 == canvasGroups.Count)
        {
            Destroy(gameObject);
        }
        else
        {
            canvasGroups[current + 1].DOFade(1f, 1f);
            current++;
        }
        
    }
}
