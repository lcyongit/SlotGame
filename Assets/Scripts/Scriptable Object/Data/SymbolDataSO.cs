using UnityEngine;

[CreateAssetMenu(fileName = "SymbolDataSO", menuName = "Scriptable Objects/Data/SymbolDataSO")]
public class SymbolDataSO : ScriptableObject
{
    public Symbol symbol;      // 圖案名稱
    public Sprite icon;            // 對應圖示（可在 Inspector 設定）
    public float probability;      // 出現機率（總和可任意，會內部標準化）
    public int payoutMultiplier;   // 連線得分倍率
    public bool isWild;            // 是否是 Wild 百搭
    public bool isScatter;         // 是否是 Scatter 分散圖案

}
