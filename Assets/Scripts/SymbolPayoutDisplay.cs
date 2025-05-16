using UnityEngine;
using UnityEngine.UI;

public class SymbolPayoutDisplay : MonoBehaviour
{
    public SymbolDataSO SymbolDataSO;

    private Sprite icon;
    private string payoutText;

    private void Start()
    {
        icon = SymbolDataSO.icon;
        payoutText = $"= {SymbolDataSO.payoutMultiplier}";

        var image = GetComponentInChildren<Image>();
        image.sprite = icon;
        var text = GetComponentInChildren<Text>();
        text.text = payoutText;
    }
}
