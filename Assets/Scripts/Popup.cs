using DG.Tweening;
using TMPro;
using UnityEngine;

public class Popup : MonoBehaviour
{
    public TextMeshProUGUI text;


    public void PlayAnimation()
    {
        transform.localScale = new Vector3(.5f, .5f, .5f);
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);
        Sequence s = DOTween.Sequence();
        s.Append(transform.DOScale(1.2f, .2f));
        s.Join(text.DOFade(1f, .2f));
        s.Append(transform.DOScale(1f, 1.5f));
        s.Append(text.DOFade(0f, 1.5f));
        s.AppendCallback(() => { if (this != null) { Destroy(this.gameObject); } });
    }

}
