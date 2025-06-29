using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    [SerializeField]
    CanvasGroup canvasGroup;
    [SerializeField]
    TextMeshProUGUI nameText;
    [SerializeField]
    TextMeshProUGUI godText;
    [SerializeField]
    TextMeshProUGUI descriptionText;
    [SerializeField]
    Image shapeImage;

    public void ShowTooltip(Tent tent)
    {
        nameText.text = tent.Name;
        godText.text = tent.god.ToString();
        descriptionText.text = TextReplacer.Replace( tent.Description );
        canvasGroup.alpha = 1f;
        shapeImage.sprite = tent.shapeImge;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
    }
}
