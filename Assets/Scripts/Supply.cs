using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Supply : MonoBehaviour
{
    [SerializeField]
    List<SpriteRenderer> sr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr.OrderBy(o => Random.value).First().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        int sortingOrder = Mathf.RoundToInt(-transform.position.y * 10f);
        foreach (SpriteRenderer r in sr)
        {
            r.sortingOrder = sortingOrder;
        }
    }
}
