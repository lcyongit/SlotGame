using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class SymbolDataGenerator
{
    [MenuItem("Tools/Slot/🧱 一鍵建立 Symbol 資料 & ListSO")]
    public static void GenerateDefaultSymbolsAndList()
    {
        string folderPath = "Assets/SymbolData";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "SymbolData");
        }

        var defaultData = new Dictionary<Symbol, (float probability, int payoutMultiplier)>
        {
            { Symbol.Cherry, (25, 2) },
            { Symbol.Lemon, (20, 3) },
            { Symbol.Orange, (15, 4) },
            { Symbol.Grape, (10, 8) },
            { Symbol.Watermelon, (5, 16) },
            { Symbol.Bell, (3, 30) },
            { Symbol.Diamond, (2, 50) },
            { Symbol.Seven, (1, 100) },
        };

        List<SymbolDataSO> createdSymbols = new();

        // 產生每個Symbol的SO
        foreach (var data in defaultData)
        {
            Symbol symbol = data.Key;
            var (probability, payoutMultiplier) = data.Value;

            SymbolDataSO asset = ScriptableObject.CreateInstance<SymbolDataSO>();
            asset.symbol = symbol;
            asset.probability = probability;
            asset.payoutMultiplier = payoutMultiplier;

            string iconPath = $"Assets/Icon/{symbol}.png";
            asset.icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

            string assetName = $"{folderPath}/SymbolData_{data.Key}.asset";
            AssetDatabase.CreateAsset(asset, assetName);

            createdSymbols.Add(asset);
        }

        // 建立SymbolDataListSO
        SymbolDataListSO listSO = ScriptableObject.CreateInstance<SymbolDataListSO>();
        listSO.symbolDataList = createdSymbols;

        string listSOPath = $"{folderPath}/SymbolDataListSO.asset";
        AssetDatabase.CreateAsset(listSO, listSOPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ 全部 SymbolDataSO 與 SymbolDataListSO 建立完成！");
    }
}
