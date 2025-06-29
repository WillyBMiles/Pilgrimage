using DG.Tweening;
using TMPro;
using UnityEngine;

public class ResourcesUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI foodCurrent;
    [SerializeField]
    TextMeshProUGUI foodChangeText;


    [SerializeField]
    TextMeshProUGUI faithCurrent;
    [SerializeField]
    TextMeshProUGUI faithChangeText;

    [SerializeField]
    TextMeshProUGUI tentsCurrent;
    [SerializeField]
    TextMeshProUGUI tentsChangeText;

    [SerializeField]
    Vector3 onScreenPos;

    [SerializeField]
    Vector3 offScreenPos;

    RectTransform rectTransform => transform as RectTransform;


    private void Start()
    {
        GameController.Instance.OnTransitionTo += TransitionTo;
    }

    private void TransitionTo(GameController.State state)
    {
        if (state == GameController.State.Placement)
        {
            rectTransform.DOAnchorPos(onScreenPos, .5f);
        }
        if (state == GameController.State.Map)
        {
            rectTransform.DOAnchorPos(offScreenPos, .5f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        int food = ResourceManager.Instance.GetCurrentResource(ResourceManager.Resource.Food);
        int foodGoal = ResourceManager.Instance.FoodGoal;
        int foodChange = ResourceManager.Instance.GetProposedChange(ResourceManager.Resource.Food);


        int faith = ResourceManager.Instance.GetCurrentResource(ResourceManager.Resource.Faith);
        int faithChange = ResourceManager.Instance.GetProposedChange(ResourceManager.Resource.Faith);

        int tents = ResourceManager.Instance.GetCurrentResource(ResourceManager.Resource.Tents);
        int tentsChange = ResourceManager.Instance.GetProposedChange(ResourceManager.Resource.Tents);

        foodCurrent.text = food + "/" + foodGoal;
        if (food > foodGoal)
        {
            foodCurrent.color = Color.green;
        }
        else
        {
            foodCurrent.color = Color.white;
        }

        SetTextChange(foodChangeText, foodChange);

        faithCurrent.text = "" + faith;
        if (ResourceManager.Instance.GetCurrentResource(ResourceManager.Resource.Faith) < 0)
        {
            faithCurrent.color = Color.red;
        }
        else
        {
            faithCurrent.color = Color.white;
        }
        SetTextChange(faithChangeText, faithChange);

        tentsCurrent.text = "" + tents;
        if (tents > GameController.Instance.currentTents.Count)
        {
            tentsCurrent.color = Color.green;
        }
        else if (tents < GameController.Instance.currentTents.Count)
        {
            tentsCurrent.color = Color.red;
        }
        else
        {
            tentsCurrent.color = Color.white;
        }
        SetTextChange(tentsChangeText, tentsChange);

    }

    void SetTextChange(TextMeshProUGUI text, int amount)
    {
        if (amount == 0)
        {
            text.text = "";
        }
        if (amount > 0)
        {
            text.text = "+" + amount;
            text.color = Color.green;
        }
        if (amount < 0)
        {
            text.text = "" + amount;
            text.color = Color.red;
        }
    }
}
