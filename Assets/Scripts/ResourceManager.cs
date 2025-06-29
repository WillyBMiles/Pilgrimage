using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }


    public int currentFaith;
    public int currentFood;
    [SerializeField]
    int startingFaith;
    [SerializeField]
    int startingFood;

    public int newTents;

    public int currentTents;



    public int FoodGoal => GameController.Instance.CurrentLocation.foodGoal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentFaith = startingFaith;
        currentFood = startingFood;

        GameController.Instance.OnTransitionTo += TransitionTo;
        
    }

    bool consumed = false;
    private void TransitionTo(GameController.State state)
    {
        if (state == GameController.State.PreRound || state == GameController.State.Placement)
        {
            currentTents = GameController.Instance.currentTents.Count;
        }

        if (state == GameController.State.Evening)
        {
            Sequence s = DOTween.Sequence();
            s.AppendInterval(1f);
            s.AppendCallback(() =>
            {
                Dictionary<Tent, ResourceChange> changes = ApplyEffects(CurrentGrid.Instance.PlacedTents);
                foreach (var kvp in changes)
                {
                    EnactResourceChange(kvp.Value, kvp.Key.transform.position);
                }

                if (currentFood < FoodGoal)
                {
                    GameController.Instance.Lose(GameController.LoseReason.OutOfFood);
                }
                if (currentFaith < 0)
                {
                    GameController.Instance.Lose(GameController.LoseReason.OutOfFaith);
                }

                consumed = true;
                ChangeResource(Resource.Food, -FoodGoal, transform.position);
            });
        }


    }


    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.CurrentState == GameController.State.Placement)
        {
            consumed = false;
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ChangeResource(Resource.Food, 100, transform.position);
        }
#endif
    }

    public int GetCurrentResource(Resource resource, IEnumerable<Tent> tents = null)
    {
        int total = resource switch { Resource.Food => currentFood, Resource.Faith => currentFaith, Resource.Tents => currentTents, _ => 0 };
        if (consumed)
        {
            return total;
        }

        
        tents ??= CurrentGrid.Instance.PlacedTents;
        Dictionary<Tent, ResourceChange> changes = GetResourceChange(tents);
        foreach (var kvp in changes)
        {
            if (kvp.Value.changes.ContainsKey(resource))
            {
                total += kvp.Value.changes[resource];
            }
        }
        return total;
    }

    public int GetProposedChange(Resource resource)
    {
        int current = GetCurrentResource(resource);

        if (TentPlacementManager.currentlyPlacing == null)
            return 0;

        int newValue = GetCurrentResource(resource, CurrentGrid.Instance.PlacedTents.Append(TentPlacementManager.currentlyPlacing));

        return newValue - current;
    }


    void ChangeResource(Resource resource, int amount, Vector3 location)
    {

        PopupController.Popup(resource, (Vector2)location + UnityEngine.Random.insideUnitCircle, amount);
        //Show animation
        switch (resource)
        {
            case Resource.Food:
                currentFood += amount;
                break;
            case Resource.Faith:
                currentFaith += amount;
                break;
            case Resource.Tents:
                if (amount > 0)
                {
                    newTents += amount;
                }
                else
                {
                    //removing tents should only be done with specialness
                }
                currentTents += amount;
                break;
        }
    }

    public void EnactResourceChange(ResourceChange resourceChange, Vector3 location)
    {
        foreach (var change in resourceChange.changes)
        {
            ChangeResource(change.Key, change.Value, location);
            
        }
    }

    public enum Resource
    {
        Food,
        Faith,
        Tents
    }



    public Dictionary<Tent, ResourceChange> GetResourceChange(IEnumerable<Tent> allTents)
    {
        return Apply(allTents, (e, t1, t2, tents, adjacent) => e.GetResourceChange(t1, t2, tents, adjacent));
    }


    public Dictionary<Tent, ResourceChange> ApplyEffects(IEnumerable<Tent> allTents)
    {
        return Apply(allTents, (e, t1, t2, tents, adjacent) => e.ApplyEffect(t1, t2, tents, adjacent));
    }

    public Dictionary<Tent, ResourceChange> Apply(IEnumerable<Tent> allTents, Func<Effect, Tent, Tent, IEnumerable<Tent>, Dictionary<Tent, IEnumerable<Tent>>, ResourceChange> func)
    {
        Dictionary<Tent, IEnumerable<Tent>> adjacentTents = new();
        foreach (Tent t in allTents)
        {
            adjacentTents[t] = t.AdjacentTents(allTents);
        }

        Dictionary<Tent, ResourceChange> resourceChanges = new();

        var orderedTents = allTents.OrderBy(tent => tent.effectPriority);
        foreach (Tent t in orderedTents)
        {
            resourceChanges[t] = func?.Invoke(t.effect, t,t,allTents,adjacentTents);
        }

        return resourceChanges;
    }
}
