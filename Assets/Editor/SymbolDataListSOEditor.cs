using Codice.CM.Client.Differences.Graphic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SymbolDataListSO))]
public class SymbolDataListSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SymbolDataListSO symbolDataListSO = (SymbolDataListSO)target;

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("🧩 圖案機率列表", EditorStyles.boldLabel);

        float total = 0f;
        var symbolList = symbolDataListSO.symbolDataList;
        foreach (var s in symbolList)
        {
            if (s != null) total += s.probability;
        }

        var sortedList = symbolList
                        .Where(s => s != null)
                        .OrderByDescending(s => s.probability)
                        .ToList();

        foreach (var s in sortedList)
        {
            if (s == null) continue;

            string name = s.name;
            float prob = s.probability;
            float normalized = (total > 0f) ? prob / total : 0f;

            EditorGUILayout.LabelField($"- {name}：原始 {prob}，機率 {normalized:P2}");
        }

        EditorGUILayout.Space(10);

        // 計算 RTP
        float rtpPercent = CaculateRTP(sortedList, total) * 100f;
        symbolDataListSO.rtpPercent = rtpPercent;
        EditorUtility.SetDirty(symbolDataListSO);

        EditorGUILayout.LabelField($"🎯 理論 RTP：{rtpPercent:F2}%");

        EditorGUILayout.Space(10);

        if (GUILayout.Button("📊 顯示標準化機率（印到 Console）"))
        {
            symbolDataListSO.PrintNormalizedProbabilities();
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("🧩 模擬抽取", EditorStyles.boldLabel);

        EditorGUILayout.Space(10);

        // 這是可輸入的欄位
        int simulateCount = EditorGUILayout.IntField("模擬次數", 1000);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("🧪 模擬抽取單個圖案"))
        {
            SimulateDraws(symbolList, simulateCount);
        }

        EditorGUILayout.Space(10);

        //int n = 6;

        //for (int m = 1; m <= n; m++)
        //{
        //    EditorGUILayout.LabelField($"C({n}, {m})：{Combinations(n, m)}");

        //}

    }


    private void SimulateDraws(List<SymbolDataSO> symbolList, int simulateCount)
    {
        Dictionary<string, int> stats = new();
        var normalized = GameManager.NormalizeProbabilities(symbolList);

        for (int i = 0; i < simulateCount; i++)
        {
            float r = Random.Range(0f, 1f);
            float sum = 0f;

            for (int j = 0; j < symbolList.Count; j++)
            {
                sum += normalized[j];
                if (r <= sum)
                {
                    string name = symbolList[j].symbol.ToString();
                    if (!stats.ContainsKey(name)) stats[name] = 0;
                    stats[name]++;
                    break;
                }
            }
        }

        Debug.Log($"🎰 模擬抽取單個圖案 {simulateCount} 次：");

        foreach (var kv in stats.OrderByDescending(kv => kv.Value))
        {
            Debug.Log($"- {kv.Key}：{kv.Value} 次（{(kv.Value / (float)simulateCount):P2}）");
        }

    }

    // 計算RTP
    private float CaculateRTP(List<SymbolDataSO> sortedList, float total)
    {
        // 單個圖案的連線組合
        // 1條橫向連線組合
        List<HashSet<int>> oneHorizontalLines = new List<HashSet<int>>
        {
            new HashSet<int> {7, 8, 9},
            new HashSet<int> {4, 5, 6},
            new HashSet<int> {1, 2, 3},
        };

        // 1條對角連線組合
        List<HashSet<int>> oneDiagonalLines = new List<HashSet<int>>
        {
            new HashSet<int> {3, 5, 7},
            new HashSet<int> {1, 5, 9}
        };

        // 2條橫向連線組合
        List<HashSet<int>> twoHorizontalLines = new List<HashSet<int>>
        {
            new HashSet<int> {1, 2, 3, 4, 5, 6},
            new HashSet<int> {1, 2, 3, 7, 8, 9},
            new HashSet<int> {4, 5, 6, 7, 8, 9},
        };

        // 1條橫向+1條對角連線組合
        List<HashSet<int>> oneHorizontalOneDiagonalLines = new List<HashSet<int>>
        {
            new HashSet<int> {1, 2, 3, 5, 9},
            new HashSet<int> {1, 2, 3, 5, 7},
            new HashSet<int> {4, 5, 6, 1, 9},
            new HashSet<int> {4, 5, 6, 3, 7},
            new HashSet<int> {7, 8, 9, 1, 5},
            new HashSet<int> {7, 8, 9, 3, 5},
        };

        // 2條對角連線組合
        List<HashSet<int>> twoDiagonalLines = new List<HashSet<int>>
        {
            new HashSet<int> {1, 5, 9, 3, 7},
        };

        // 2條橫向+1條對角連線組合
        List<HashSet<int>> twoHorizontalOneDiagonalLines = new List<HashSet<int>>
        {
            new HashSet<int> {1, 2, 3, 4, 5, 6, 7},
            new HashSet<int> {1, 2, 3, 4, 5, 6, 9},
            new HashSet<int> {4, 5, 6, 7, 8, 9, 1},
            new HashSet<int> {4, 5, 6, 7, 8, 9, 3},
        };

        // 2條橫向+2條對角連線組合 (7圖案)
        List<HashSet<int>> twoHorizontaltwoDiagonalLines7 = new List<HashSet<int>>
        {
            new HashSet<int> {1, 2, 3, 7, 8, 9, 5},
        };

        // 2條橫向+2條對角連線組合 (8圖案)
        List<HashSet<int>> twoHorizontaltwoDiagonalLines8 = new List<HashSet<int>>
        {
            new HashSet<int> {1, 2, 3, 4, 5, 6, 7, 9},
            new HashSet<int> {4, 5, 6, 7, 8, 9, 1, 3},
        };

        // 1條橫向+2條對角連線組合
        List<HashSet<int>> oneHorizontalTwoDiagonalLines = new List<HashSet<int>>
        {
            new HashSet<int> {4, 5, 6, 1, 3, 7, 9},
        };

        HashSet<int> allPositions = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        float rtp = 0f;
        // 計算一種圖案連線
        for (int i = 0; i < sortedList.Count; i++)
        {
            if (sortedList[i] == null) continue;
            float p = sortedList[i].probability / total;
            float np = 1 - p;

            // ***** 要扣除其他位置不能出現要計算的圖案，不然會重複計算 *****
            // ***** 但是其他圖案也可能湊成連線，要排除 *****

            // ------------------ 計算 1 條相同圖案連線的 RTP ------------------
            // 3個圖案
            // 橫線+對角 (組合=5)
            // 1條橫向連線
            float lineProbability = 0.0f;
            foreach (var line in oneHorizontalLines)
            {
                float p_line = Mathf.Pow(p, 3);

                // 其餘位置不能構成連線
                HashSet<int> remaining = new(allPositions);
                remaining.ExceptWith(line);

                float p_non_line = 0.0f;
                for (int r = 2; r <= 6; r++)
                {
                    if (r == 2)
                        p_non_line += (CombinationsRecursive(6, r) - 6) * Mathf.Pow(np, r) * Mathf.Pow(p, 6 - r);
                    if (r == 3)
                        p_non_line += (CombinationsRecursive(6, r) - 2) * Mathf.Pow(np, r) * Mathf.Pow(p, 6 - r);
                    else
                        p_non_line += CombinationsRecursive(6, r) * Mathf.Pow(np, r) * Mathf.Pow(p, 6 - r);
                }
                lineProbability += p_line * p_non_line;

                // 扣除其他圖案連線
                // 同樣圖案的1條和2條連線
                p_non_line = 0.0f;
                for (int j = i + 1; j < sortedList.Count; j++)
                {
                    float p2 = sortedList[j].probability / total;
                    float np2 = 1 - p2;
                    p_non_line += Mathf.Pow(p2, 3) * Mathf.Pow(np2, 3) * 2;
                    p_non_line += Mathf.Pow(p2, 4) * Mathf.Pow(np2, 2) * 6;
                    p_non_line += Mathf.Pow(p2, 5) * Mathf.Pow(np2, 1) * 6;
                    p_non_line += Mathf.Pow(p2, 6) * 1;

                    // 不同圖案的各1條連線
                    for (int k = j + 1; k < sortedList.Count; k++)
                    {
                        float p3 = sortedList[k].probability / total;
                        p_non_line += Mathf.Pow(p2, 3) * Mathf.Pow(p3, 3) * 2;
                    }

                }
                lineProbability -= p_line * p_non_line;

            }

            // 1條對角連線
            foreach (var line in oneDiagonalLines)
            {
                float p_line = Mathf.Pow(p, 3);

                // 其餘位置不能構成連線
                HashSet<int> remaining = new(allPositions);
                remaining.ExceptWith(line);

                float p_non_line = 0.0f;
                for (int r = 3; r <= 6; r++)
                {
                    if (r == 3)
                        p_non_line += (CombinationsRecursive(6, r) - 12) * Mathf.Pow(np, r) * Mathf.Pow(p, 6 - r);
                    if (r == 4)
                        p_non_line += (CombinationsRecursive(6, r) - 3) * Mathf.Pow(np, r) * Mathf.Pow(p, 6 - r);
                    else
                        p_non_line += CombinationsRecursive(6, r) * Mathf.Pow(np, r) * Mathf.Pow(p, 6 - r);
                }
                lineProbability += p_line * p_non_line;
            }
            rtp += lineProbability * sortedList[i].payoutMultiplier * 1;

            // ------------------ 計算 2 條相同圖案連線的 RTP ------------------
            // 6個圖案 (組合=3)
            // ooo ooo xxx
            // ooo xxx ooo
            // xxx ooo ooo 
            // 2條橫向連線
            lineProbability = 0.0f;
            foreach (var line in twoHorizontalLines)
            {
                float p_line = Mathf.Pow(p, 6);

                // 其餘位置不能構成連線
                HashSet<int> remaining = new(allPositions);
                remaining.ExceptWith(line);

                float p_non_line = 0.0f;
                if (remaining.SetEquals(new HashSet<int> { 4, 5, 6 }))
                {
                    for (int r = 1; r <= 3; r++)
                    {
                        if (r == 1)
                            p_non_line += (CombinationsRecursive(3, r) - 2) * Mathf.Pow(np, r) * Mathf.Pow(p, 3 - r);
                        if (r == 2)
                            p_non_line += (CombinationsRecursive(3, r) - 1) * Mathf.Pow(np, r) * Mathf.Pow(p, 3 - r);
                        else
                            p_non_line += CombinationsRecursive(3, r) * Mathf.Pow(np, r) * Mathf.Pow(p, 3 - r);
                    }
                }
                else
                {
                    for (int r = 2; r <= 3; r++)
                    {
                        if (r == 2)
                            p_non_line += (CombinationsRecursive(3, r) - 2) * Mathf.Pow(np, r) * Mathf.Pow(p, 3 - r);
                        else
                            p_non_line += CombinationsRecursive(3, r) * Mathf.Pow(np, r) * Mathf.Pow(p, 3 - r);
                    }
                }
                lineProbability += p_line * p_non_line;

                // 扣除其他圖案連線
                // 同樣圖案的1條連線
                p_non_line = 0.0f;
                for (int j = i + 1; j < sortedList.Count; j++)
                {
                    float p2 = sortedList[j].probability / total;
                    p_non_line += Mathf.Pow(p2, 3);
                }
                lineProbability -= p_line * p_non_line;

            }
            // 5個圖案 (組合=7)
            // ooo ooo  oxx xxo  oxx xxo  
            // xox xox  ooo ooo  xox xox  
            // oxx xxo  xxo oxx  ooo ooo  
            // 1條橫向+1條對角連線
            foreach (var line in oneHorizontalOneDiagonalLines)
            {
                float p_line = Mathf.Pow(p, 5);

                // 其餘位置不能構成連線
                HashSet<int> remaining = new(allPositions);
                remaining.ExceptWith(line);

                float p_non_line = 0.0f;
                if (remaining.SetEquals(new HashSet<int> { 4, 5, 6, 1, 9 }) || remaining.SetEquals(new HashSet<int> { 4, 5, 6, 3, 7 }))
                {
                    for (int r = 2; r <= 4; r++)
                    {
                        if (r == 2)
                            p_non_line += (CombinationsRecursive(4, r) - 3) * Mathf.Pow(np, r) * Mathf.Pow(p, 4 - r);
                        else
                            p_non_line += CombinationsRecursive(4, r) * Mathf.Pow(np, r) * Mathf.Pow(p, 4 - r);
                    }
                }
                else
                {
                    for (int r = 2; r <= 4; r++)
                    {
                        if (r == 2)
                            p_non_line += (CombinationsRecursive(4, r) - 4) * Mathf.Pow(np, r) * Mathf.Pow(p, 4 - r);
                        if (r == 3)
                            p_non_line += (CombinationsRecursive(4, r) - 1) * Mathf.Pow(np, r) * Mathf.Pow(p, 4 - r);
                        else
                            p_non_line += CombinationsRecursive(4, r) * Mathf.Pow(np, r) * Mathf.Pow(p, 4 - r);
                    }
                }
                lineProbability += p_line * p_non_line;

            }

            // oxo
            // xox
            // oxo
            // 2條對角連線
            foreach (var line in twoDiagonalLines)
            {
                float p_line = Mathf.Pow(p, 5);

                // 其餘位置不能構成連線
                HashSet<int> remaining = new(allPositions);
                remaining.ExceptWith(line);

                float p_non_line = 0.0f;
                for (int r = 3; r <= 4; r++)
                {
                    if (r == 3)
                        p_non_line += (CombinationsRecursive(4, r) - 2) * Mathf.Pow(np, r) * Mathf.Pow(p, 4 - r);
                    else
                        p_non_line += CombinationsRecursive(4, r) * Mathf.Pow(np, r) * Mathf.Pow(p, 4 - r);

                }
                lineProbability += p_line * p_non_line;

            }

            rtp += lineProbability * sortedList[i].payoutMultiplier * 2;

            // ------------------ 計算 3 條相同圖案連線的 RTP ------------------
            // 7個圖案 (組合=5)
            // ooo ooo  oxx xxo  
            // ooo ooo  ooo ooo  
            // oxx xxo  ooo ooo  
            // 2條橫向+1條對角連線
            lineProbability = 0.0f;
            foreach (var line in twoHorizontalOneDiagonalLines)
            {
                float p_line = Mathf.Pow(p, 7);

                // 其餘位置不能構成連線
                HashSet<int> remaining = new(allPositions);
                remaining.ExceptWith(line);

                float p_non_line = 0.0f;
                for (int r = 1; r <= 2; r++)
                {
                    if (r == 1)
                        p_non_line += (CombinationsRecursive(2, r) - 1) * Mathf.Pow(np, r) * Mathf.Pow(p, 2 - r);
                    else
                        p_non_line += CombinationsRecursive(2, r) * Mathf.Pow(np, r) * Mathf.Pow(p, 2 - r);

                }
                lineProbability += p_line * p_non_line;

            }

            // oxo
            // ooo
            // oxo
            // 1條橫向 + 2條對角連線
            foreach (var line in oneHorizontalTwoDiagonalLines)
            {
                float p_line = Mathf.Pow(p, 7);

                // 其餘位置不能構成連線
                HashSet<int> remaining = new(allPositions);
                remaining.ExceptWith(line);

                float p_non_line = 0.0f;
                for (int r = 2; r <= 2; r++)
                {
                    p_non_line += CombinationsRecursive(2, r) * Mathf.Pow(np, r) * Mathf.Pow(p, 2 - r);

                }
                lineProbability += p_line * p_non_line;

            }

            rtp += lineProbability * sortedList[i].payoutMultiplier * 3;

            // ------------------ 計算 4 條相同圖案連線的 RTP ------------------
            // 7個圖案 (組合=1)
            // ooo 
            // xox 
            // ooo 
            // 2條橫向+2條對角連線組合 (7圖案)
            lineProbability = 0.0f;
            foreach (var line in twoHorizontaltwoDiagonalLines7)
            {
                float p_line = Mathf.Pow(p, 7);

                // 其餘位置不能構成連線
                HashSet<int> remaining = new(allPositions);
                remaining.ExceptWith(line);

                float p_non_line = 0.0f;
                for (int r = 1; r <= 2; r++)
                {
                    p_non_line += CombinationsRecursive(2, r) * Mathf.Pow(np, r) * Mathf.Pow(p, 2 - r);

                }
                lineProbability += p_line * p_non_line;

            }

            // 8個圖案 (組合=2)
            // ooo oxo 
            // ooo ooo 
            // oxo ooo 
            // 2條橫向+2條對角連線組合 (8圖案)
            foreach (var line in twoHorizontaltwoDiagonalLines8)
            {
                float p_line = Mathf.Pow(p, 8);

                // 其餘位置不能構成連線
                HashSet<int> remaining = new(allPositions);
                remaining.ExceptWith(line);

                float p_non_line = 0.0f;
                for (int r = 1; r <= 1; r++)
                {
                    p_non_line += CombinationsRecursive(1, r) * Mathf.Pow(np, r) * Mathf.Pow(p, 1 - r);

                }
                lineProbability += p_line * p_non_line;

            }

            rtp += lineProbability * sortedList[i].payoutMultiplier * 4;

            // ------------------ 計算 5 條相同圖案連線的 RTP ------------------
            // 9個圖案 (組合=1)
            rtp += Mathf.Pow(p, 9) * sortedList[i].payoutMultiplier * 5 * 1;

        }

        // 計算兩種圖案連線
        for (int i = 0; i < sortedList.Count; i++)
        {
            for (int j = i + 1; j < sortedList.Count; j++)
            {
                float p1 = sortedList[i].probability / total;
                float p2 = sortedList[j].probability / total;
                float np = 1 - p1 - p2;

                float lineProbability = 0.0f;

                // 組合=3*2
                /*
                111 111 xxx  222 222 xxx
                222 xxx 111  111 xxx 222
                xxx 222 222  xxx 111 111
                 */
                lineProbability += Mathf.Pow(p1, 3) * Mathf.Pow(p2, 3) * Mathf.Pow(np, 3);

                // 扣除第三種圖案連線
                for (int k = j + 1; k < sortedList.Count; k++)
                {
                    float p3 = sortedList[k].probability / total;
                    lineProbability -= Mathf.Pow(p1, 3) * Mathf.Pow(p2, 3) * Mathf.Pow(p3, 3);
                }

                rtp += lineProbability * (sortedList[i].payoutMultiplier + sortedList[j].payoutMultiplier) * 6;

                // 組合=3
                /*
                111 111 222
                111 222 111
                222 111 111
                 */
                lineProbability = 0.0f;

                lineProbability += Mathf.Pow(p1, 6) * Mathf.Pow(p2, 3);

                rtp += lineProbability * (sortedList[i].payoutMultiplier + sortedList[j].payoutMultiplier) * 3;

                // 組合=3
                /*
                222 222 111
                222 111 222
                111 222 222
                 */
                lineProbability = 0.0f;

                lineProbability += Mathf.Pow(p1, 3) * Mathf.Pow(p2, 6);

                rtp += lineProbability * (sortedList[i].payoutMultiplier + sortedList[j].payoutMultiplier) * 3;

            }
        }

        // 計算三種圖案連線
        for (int i = 0; i < sortedList.Count; i++)
        {
            for (int j = i + 1; j < sortedList.Count; j++)
            {
                for (int k = j + 1; k < sortedList.Count; k++)
                {
                    float p1 = sortedList[i].probability / total;
                    float p2 = sortedList[j].probability / total;
                    float p3 = sortedList[k].probability / total;

                    // 組合=6
                    /*
                    111 111 222 222 333 333
                    222 333 111 333 111 222
                    333 222 333 111 222 111
                     */
                    rtp += Mathf.Pow(p1, 3) * Mathf.Pow(p2, 3) * Mathf.Pow(p3, 3) * (sortedList[i].payoutMultiplier + sortedList[j].payoutMultiplier + sortedList[k].payoutMultiplier) * 6;
                }
            }
        }

        return rtp;
    }

    // 組合計算 C(n, r)
    private int Combinations(int n, int r)
    {
        if (r > n) return 0;
        double result = 1;
        for (int i = 0; i < r; i++)
        {
            result *= (n - i) / (double)(i + 1);
        }
        return (int)System.Math.Round(result);
    }

    // 計算組合數 C(n, k)
    private int CombinationsRecursive(int n, int k)
    {
        if (k == 0 || k == n) return 1;
        return CombinationsRecursive(n - 1, k - 1) + CombinationsRecursive(n - 1, k);
    }



}
