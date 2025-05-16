using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SymbolDataListSO", menuName = "Scriptable Objects/Data/SymbolDataListSO")]
public class SymbolDataListSO : ScriptableObject
{
    public float rtpPercent;
    public List<SymbolDataSO> symbolDataList;

    public SymbolDataSO GetSymbolData(Symbol symbolName)
    {
        return symbolDataList.Find(x => x.symbol == symbolName);

    }

    public void PrintNormalizedProbabilities()
    {
        float total = symbolDataList.Sum(s => s.probability);
        if (total == 0f)
        {
            Debug.LogWarning("總機率為 0，請檢查資料");
            return;
        }

        foreach (var symbolData in symbolDataList)
        {
            float normalized = symbolData.probability / total;
            Debug.Log($"圖案：{symbolData.name}，標準化機率：{normalized:F3}");
        }
    }
}
