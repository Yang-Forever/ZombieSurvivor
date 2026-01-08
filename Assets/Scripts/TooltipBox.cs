using UnityEngine;
using UnityEngine.UI;

public class TooltipBox : MonoBehaviour
{
    public GameObject tooltipBox;
    public Text descText;

    public static TooltipBox Inst = null;

    private void Awake()
    {
        Inst = this;
        Hide();
    }

    public void Show(ItemRuntimeData data, Vector3 mousePos)
    {
        descText.text = data.GetTooltipDesc();

        Vector3 offset = new Vector3(2f, 2f, 0);

        tooltipBox.SetActive(true);
        tooltipBox.transform.position = mousePos + offset;
    }

    public void Hide()
    {
        tooltipBox.SetActive(false);
    }
}
