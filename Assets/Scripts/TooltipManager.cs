using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public Tooltip currentTooltip;
    public Tooltip hoverTooltip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (TentPlacementManager.currentlyPlacing == null)
        {
            currentTooltip.Hide();
        }
        else
        {
            currentTooltip.ShowTooltip(TentPlacementManager.currentlyPlacing);
        }

        if (Tent.HoveringTent == null)
        {
            hoverTooltip.Hide();
        }
        else
        {
            hoverTooltip.ShowTooltip(Tent.HoveringTent);
        }
    }
}
