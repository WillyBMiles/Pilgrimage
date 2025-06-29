using UnityEngine;
using UnityEngine.UIElements;

public class PopupController : MonoBehaviour
{

    static PopupController instance;
    public Popup prefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void Popup(ResourceManager.Resource resource, Vector3 position, int change)
    {
        Popup popup = Instantiate(instance.prefab, position, Quaternion.identity);
        popup.text.text = change + "<" + resource.ToString().ToLower() + ">";
        if (change > 0)
        {
            popup.text.text = "+" + popup.text.text;
            popup.text.color = Color.green;
        }
        else
        {
            popup.text.color = Color.red;
        }

        popup.text.text = TextReplacer.Replace(popup.text.text);
        popup.PlayAnimation();
    }

    public static void Popup(string message, Vector3 position, Color color)
    {
        Popup popup = Instantiate(instance.prefab, position, Quaternion.identity);
        popup.text.text = message;
        popup.text.color = color;
        popup.PlayAnimation();
    }
}
