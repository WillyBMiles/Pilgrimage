using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Tent : SerializedMonoBehaviour
{
    readonly List<TentBlock> tentBlocks = new();
    public List<TentBlock> TentBlocks => tentBlocks;

    public int NumTentBlocks => GetComponentsInChildren<TentBlock>().Length;

    public bool beingPlaced;
    public bool isFirstTent;

    public enum God
    {
        Jasara, // Animals => production
        Xiraeth, //Sky => general "for a cost"
        Uborys, //River => adjacency boosting
    }

    public string Name;
    [TextArea(2,20)]
    public string Description;

    public God god;

    public Sprite shapeImge;
    public Effect effect;
    public int effectPriority;
    public Sprite combinedImage;
    public bool actuallyPlaced = false;

    public static Tent HoveringTent { get; set; }

    [SerializeField]
    SpriteRenderer[] spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tentBlocks.AddRange(GetComponentsInChildren<TentBlock>());
    }

    // Update is called once per frame
    void Update()
    {
        foreach (TentBlock tb in tentBlocks)
        {
            tb.beingPlaced = beingPlaced;
            tb.actuallyPlaced = actuallyPlaced;
        }

        if (beingPlaced)
        {
            bool anyAdjacent = isFirstTent || AnyAdjacentTents();
            tentBlocks.ForEach(tb => tb.anyAdjacentTents = anyAdjacent);

            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

            foreach (var item in spriteRenderer)
            {
                item.sortingLayerName = "Being Placed";
                item.color = new Color(item.color.r, item.color.g, item.color.b, .5f);
            }

            if (CanPlace() && Input.GetMouseButtonUp(0))
            {
                Place();
            }
        }
        else
        {

            if (actuallyPlaced)
            {
                foreach (var item in spriteRenderer)
                {
                    item.enabled = true;
                    item.sortingLayerName = "Default";
                    item.sortingOrder = Mathf.RoundToInt(-item.transform.position.y * 10f);
                    item.color = new Color(item.color.r, item.color.g, item.color.b, 1f);
                }

                bool hover = false;
                foreach (TentBlock tb in tentBlocks)
                {
                    if (tb == TentBlock.HoveringBlock)
                    {
                        hover = true;
                        break;
                    }
                }
                if (hover)
                {
                    HoveringTent = this;
                }
                else if (HoveringTent == this)
                {
                    HoveringTent = null;
                }
            }
            else
            {
                foreach (var item in spriteRenderer)
                {
                    item.enabled = false;
                }
            }
        }

    }

    public bool AnyAdjacentTents()
    {
        return AdjacentTents(CurrentGrid.Instance.PlacedTents).Any();
    }

    public bool CanPlace()
    {
        foreach (TentBlock block in tentBlocks)
        {
            if (!block.CanPlace(out CantPlaceReason reason))
            {
                string message = reason switch
                {
                    CantPlaceReason.OnTop => "Can't place on top of obstacles or tents",
                    CantPlaceReason.OutsideOfGrid => "Outside of area",
                    CantPlaceReason.NotAdjacent => "Must be adjacent to existing tent",
                    _ => ""
                };
                if (Input.GetMouseButtonDown(0))
                {
                    Sequence s = DOTween.Sequence();
                    s.AppendInterval(.1f);
                    s.AppendCallback(() =>
                    {
                        if (block != null)
                            PopupController.Popup(message, block.transform.position, Color.red);
                    });
                   
                }
                    

                return false;
            }
                
        }
        return true;
    }

    public void Place()
    {
        beingPlaced = false;
        //transform.position = transform.position.RoundToInt().ToV3();
        CurrentGrid.Instance.PlaceTent(this);
        foreach (TentBlock tb in tentBlocks)
        {
            tb.Place();
        }
        SoundManager.Play(SoundManager.Sound.placeFoundation);
    }

    public IReadOnlyList<Vector2Int> AllPoints()
    {
        List<Vector2Int> blocks = new();
        foreach (TentBlock block in tentBlocks)
        {
            blocks.Add(block.transform.position.RoundToInt());
        }
        return blocks;
    }

    public IEnumerable<Tent> AdjacentTents(IEnumerable<Tent> tents)
    {
        List<Tent> adjacentTents = new();
        foreach (TentBlock tentBlock in tentBlocks)
        {
            adjacentTents.AddRange(CurrentGrid.Instance.AdjacentTents(tentBlock.transform.position.RoundToInt(), tents));
        }
        return adjacentTents.Distinct().Except(new[] { this });
    }


}

public class ResourceChange
{
    public Dictionary<ResourceManager.Resource, int> changes = new();

    public static ResourceChange operator +(ResourceChange left, ResourceChange right)
    {
        ResourceChange rc = new ResourceChange();

        foreach (var change in left.changes)
        {
            rc.changes[change.Key] = change.Value;
        }

        foreach (var change in right.changes)
        {
            if (rc.changes.ContainsKey(change.Key))
                rc.changes[change.Key] += change.Value;
            else
                rc.changes[change.Key] = change.Value;
        }

        return rc;
        
    }
}

public static class Extensions
{
    public static Vector2Int RoundToInt(this Vector2 vector2)
    {
        return new Vector2Int(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.y));
    }

    public static Vector2Int RoundToInt(this Vector3 vector3)
    {
        return ((Vector2)vector3).RoundToInt();
    }


    public static Vector2Int RoundToIntOffset(this Vector2 vector2)
    {
        return new Vector2Int(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.y));
    }
    public static Vector2Int RoundToIntOffsetReverse(this Vector2 vector2)
    {
        return new Vector2Int(Mathf.RoundToInt(vector2.x - .5f), Mathf.RoundToInt(vector2.y - .5f));
    }

    public static Vector2Int RoundToIntOffset(this Vector3 vector3)
    {
        return ((Vector2)vector3).RoundToIntOffset();
    }

    public static Vector2Int RoundToIntOffsetReverse(this Vector3 vector3)
    {
        return ((Vector2)vector3).RoundToIntOffsetReverse();
    }


    public static Vector3 ToV3(this Vector2Int vector2Int)
    {
        return new Vector3(vector2Int.x, vector2Int.y, 0f);
    }
}