using UnityEngine;

public class ExileSign : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer sr;
    [SerializeField]
    Color hoverColor;
    [SerializeField]
    float interactionDistance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }



    // Update is called once per frame
    void Update()
    {

        sr.color = Color.white;
        if (TentPlacementManager.currentlyPlacing == null)
            return;

        if (GameController.Instance.CurrentState == GameController.State.Placement)
        {
            Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(mouse,transform.position) < interactionDistance)
            {
                sr.color = hoverColor;
                if (Input.GetMouseButtonUp(0))
                {
                    var tpm = FindFirstObjectByType<TentPlacementManager>();

                    if (TentPlacementManager.currentlyPlacing.isFirstTent)
                    {
                        PopupController.Popup("Can't exile chief tent.", transform.position + Vector3.left * 7, Color.red);

                    }
                    else
                    {
                        SoundManager.Play(SoundManager.Sound.exile);
                        tpm.ClearTent();
                    }
                       
                }
            }
        }
    }
}
