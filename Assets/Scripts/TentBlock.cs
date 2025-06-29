using UnityEngine;

public class TentBlock : MonoBehaviour
{
    [SerializeField]
    Color placeableColor;

    [SerializeField]
    Color unplaceableColor;

    [SerializeField]
    SpriteRenderer spriteRenderer;

    public bool beingPlaced;
    public bool actuallyPlaced;

    public bool anyAdjacentTents;

    Vector3 localPos;
    private void Start()
    {
        localPos = transform.localPosition;
    }

    public static TentBlock HoveringBlock { get; private set; }

    private void Update()
    {
        Vector2Int v = Camera.main.ScreenToWorldPoint(Input.mousePosition).RoundToIntOffsetReverse();

        if (!beingPlaced)
        {
            if (v == transform.position.RoundToInt())
                HoveringBlock = this;
            else if (HoveringBlock == this)
            {
                HoveringBlock = null;
            }
        }


    }

    private void LateUpdate()
    {
        if (beingPlaced)
        {
            transform.localPosition = localPos;
            transform.position = transform.position.RoundToIntOffset().ToV3();
            spriteRenderer.color = CanPlace(out _) ? placeableColor : unplaceableColor;
        }
        else
        {
            if (!actuallyPlaced)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, .2f);
                spriteRenderer.sortingLayerName = "Background";
                spriteRenderer.sortingOrder = 100;
            }
            else
            {
                spriteRenderer.color = Color.clear;
                spriteRenderer.enabled = false;
            }
        }
    }



    public bool CanPlace(out CantPlaceReason cantPlaceReason)
    {
        if (!anyAdjacentTents)
        {
            cantPlaceReason = CantPlaceReason.NotAdjacent;
            return false;
        }
        if (!CurrentGrid.Instance.CanPlace(transform.position.RoundToInt(), out cantPlaceReason))
            return false;

        cantPlaceReason = CantPlaceReason.None;
        return true;
    }

    public void Place()
    {
        CurrentGrid.Instance.PlaceTentBlock(this);
    }
}

public enum CantPlaceReason
{
    None,
    OutsideOfGrid,
    OnTop,
    NotAdjacent
}
