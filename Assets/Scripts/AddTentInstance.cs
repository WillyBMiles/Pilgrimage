using System;
using UnityEngine;
using UnityEngine.UI;

public class AddTentInstance : MonoBehaviour
{
     Action selected;

    [SerializeField]
    Button button;
    [SerializeField]
    Image tentImage;

    [SerializeField]
    Tooltip tooltip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.onClick.AddListener(() => selected?.Invoke());
    }

    public void Assign(Tent tent, Action selectionAction)
    {
        tooltip.ShowTooltip(tent);
        selected += selectionAction;
        tentImage.sprite = tent.combinedImage;
    }

    public void StopShowing()
    {

    }


}
