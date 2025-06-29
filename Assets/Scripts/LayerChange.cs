using UnityEngine;

public class LayerChange : MonoBehaviour
{
    [SerializeField]
    int change;

    SpriteRenderer sr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        sr.sortingOrder += change;
    }
}
