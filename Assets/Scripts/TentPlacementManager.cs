using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TentPlacementManager : MonoBehaviour
{
    List<Tent> tents = new();

    Tent lastTent;
    public int NumTentsLeft => tents.Count;

    public Tent NextTent => tents.First();

    public static Tent currentlyPlacing;

    public event Action<Tent> OnPlace;
    public event Action<Tent> OnClearTent;

    public int KickOutBonus;
    public int StartingKickOutBonus;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameController.Instance.OnTransitionTo += (state) =>
        {
            if (state == GameController.State.Placement)
            {
                SetUpPlacement();
            }
            KickOutBonus = StartingKickOutBonus;
        };
    }

    public void ClearTent()
    {
        if (currentlyPlacing != null)
        {
            ResourceManager.Instance.EnactResourceChange(new ResourceChange() { changes = new() { { ResourceManager.Resource.Food, KickOutBonus } } },GameController.Instance.CurrentLocation.currentStagingArea.position );

            Tent t = currentlyPlacing;

            ; 
            GameController.Instance.currentTents.Remove(GameController.Instance.currentTents.First(t => t.Name == currentlyPlacing.Name));
            Destroy(currentlyPlacing.gameObject);
            PopNextTent();
            OnClearTent?.Invoke(t);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentlyPlacing != null)
        {
            if (currentlyPlacing.beingPlaced == false)
            {
                Tent t = currentlyPlacing;
                PopNextTent();
                OnPlace?.Invoke(t);
            }
        }

    }

    void PopNextTent()
    {
        if (tents.Count == 0)
        {
            currentlyPlacing = null;
            StartCoroutine(WaitTilTentsPlaced());
            return;
        }

        Tent t = tents[0];
        tents.RemoveAt(0);
        SetPlacing(t);
    }

    IEnumerator WaitTilTentsPlaced()
    {
        yield return new WaitUntil(() => CurrentGrid.Instance.PlacedTents.All(t => t.actuallyPlaced));
        yield return new WaitForSeconds(.5f);
        GameController.Instance.TransitionTo(GameController.State.Evening);
        
    }

    private void OnDestroy()
    {
        currentlyPlacing = null;
    }

    void SetUpPlacement()
    {
        tents.AddRange(GameController.Instance.currentTents.Select(t => Instantiate(t)));
        tents.ForEach(t => t.gameObject.SetActive(false));
        Tent startingTent = tents[0];
        tents.RemoveAt(0);

        tents = tents.OrderBy(t => UnityEngine.Random.value).ToList();

        startingTent.isFirstTent = true;
        
        SetPlacing(startingTent);
    }

    void SetPlacing(Tent tent)
    {
        currentlyPlacing = tent;
        lastTent = tent;
        tent.beingPlaced = true;
        tent.gameObject.SetActive(true);
    }

}
