using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CurrentGrid : MonoBehaviour
{

    public static CurrentGrid Instance { get; private set; }


    List<TentBlock> blocks = new();

    List<Tent> placedTents = new();
    public IReadOnlyList<Tent> PlacedTents => placedTents;

    private void Awake()
    {
        Instance = this;
    }


    public void ClearTents()
    {
        placedTents.ForEach(tent => Destroy(tent.gameObject));
        placedTents.Clear();
        blocks.Clear();
    }

    public bool CanPlace(Vector2Int position, out CantPlaceReason cantPlaceReason)
    {
        if (blocks.Any(block => block.transform.position.RoundToInt() == position))
        {
            cantPlaceReason = CantPlaceReason.OnTop;
            return false;
        }
        Location l = GameController.Instance.CurrentLocation;
        if (position.x < l.min.x - 1 || position.y < l.min.y || position.x > l.max.x || position.y > l.max.y - 1)
        {
            cantPlaceReason = CantPlaceReason.OutsideOfGrid;
            return false;
        }
            
        if (GameController.Instance.CurrentLocation.blockers.Any(block => block.transform.position.RoundToInt() == position))
        {

            cantPlaceReason = CantPlaceReason.OnTop;
            return false;
        }

        cantPlaceReason = CantPlaceReason.None;
        return true;
    }


    public void PlaceTent(Tent tent)
    {
        placedTents.Add(tent);
    }
    public void PlaceTentBlock(TentBlock tentBlock)
    {
        blocks.Add(tentBlock);
    }

    public IEnumerable<Tent> AdjacentTents(Vector2Int position, IEnumerable<Tent> tents)
    {
        List<Tent> returnTents = new();

        foreach (Tent tent in tents)
        {
            foreach (TentBlock block in tent.TentBlocks)
            {
                if (Vector2Int.Distance(position, block.transform.position.RoundToInt()) < 1.1f)
                {
                    returnTents.Add(tent);
                    continue;
                }
            }
        }


        return returnTents.Distinct();
    }
}
