using UnityEngine;

public class MainTentSprite : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Tent tent;
    private void Awake()
    {
        tent = GetComponentInParent<Tent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tent.beingPlaced)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, .5f);
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }
}
