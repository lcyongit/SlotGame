using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using UnityEngine.Windows;

public class GameManager : Singleton<GameManager>
{
    [Header("遊戲狀態")]
    private GameStatus gameStatus = GameStatus.Stop;
    private bool isSpinning = false;

    [Header("統計數據")]
    public List<Text> symbolCountTextList = new();
    private Dictionary<Symbol, int> symbolCountDict = new();
    private int bet;
    private int credit;
    private int count = 0;
    private int simulateCount;

    private int initialBet;
    private int initialCredit;
    private int initialSimulateCount;



    [Header("UI元件")]
    public InputField betInputField;
    public InputField creditInputField;
    public InputField winInputField;
    public InputField countInputField;
    public InputField simulateInputField;
    public InputField rtpInputField;
    public Button autoStartBtn;
    public Text autoStartText;
    public Text rtpText;

    [Header("輪軸設定")]
    public float spinDuration;
    public float stopInterval;
    public float autoDelay;

    [Header("圖案列表")]
    public SymbolDataListSO symbolDataListSO;

    [Header("轉盤位置1~9")]
    public List<GameObject> slots;

    protected override void Awake()
    {
        base.Awake();

    }

    private void Start()
    {
        int.TryParse(betInputField.text, out bet);
        int.TryParse(creditInputField.text, out credit);
        int.TryParse(simulateInputField.text, out simulateCount);

        int.TryParse(betInputField.text, out initialBet);
        int.TryParse(creditInputField.text, out initialCredit);
        int.TryParse(simulateInputField.text, out initialSimulateCount);

        betInputField.onEndEdit.AddListener(OnBetInputChanged);
        creditInputField.onEndEdit.AddListener(OnCreditInputChanged);
        simulateInputField.onEndEdit.AddListener(OnSimulateInputChanged);

        foreach (var data in symbolDataListSO.symbolDataList)
        {
            symbolCountDict[data.symbol] = 0;
        }

        rtpText.text = $"{symbolDataListSO.rtpPercent}";

    }

    #region Function

    // 產生隨機圖案
    public SymbolDataSO GetRandomSymbolDataSO()
    {
        List<float> normalized = NormalizeProbabilities(symbolDataListSO.symbolDataList);

        float randomValue = UnityEngine.Random.Range(0f, 1f);

        float current = 0f;
        for (int i = 0; i < symbolDataListSO.symbolDataList.Count; i++)
        {
            current += normalized[i];
            if (randomValue <= current)
            {
                return symbolDataListSO.symbolDataList[i];
            }
        }

        return null;

    }

    // 計算分數
    int CalculateScore(List<SymbolDataSO> board)
    {
        int totalScore = 0;
        List<Symbol> results = new();

        int[][] paylines = new int[][]
        {
        new[] { 2, 5, 8 }, // 中線
        new[] { 1, 4, 7 }, // 上線
        new[] { 3, 6, 9 }, // 下線
        new[] { 1, 5, 9 }, // 對角 ↘
        new[] { 3, 5, 7 }, // 對角 ↙
        };

        foreach (var line in paylines)
        {
            Symbol first = board[line[0] - 1].symbol;
            if (first == Symbol.Empty) continue;

            int matchCount = 1;

            // 判斷後續是否相同（或 Wild）
            for (int i = 1; i < line.Length; i++)
            {
                Symbol current = board[line[i] - 1].symbol;

                if (current == first || current == Symbol.Wild || first == Symbol.Wild)
                    matchCount++;
                else
                    break;


            }

            // 3個圖案連線獲得分數
            var symbolScoreTable = BuildScoreTable();
            if (symbolScoreTable.ContainsKey(first) && matchCount >= 3)
            {
                totalScore += symbolScoreTable[first][matchCount - 1];
                results.Add(first);
            }


        }

        // 更新連線計數
        UpdateSymbolCounts(results);

        return totalScore;
    }

    // ScriptableObject 自動建立分數表
    private Dictionary<Symbol, int[]> BuildScoreTable()
    {
        var table = new Dictionary<Symbol, int[]>();

        foreach (var data in symbolDataListSO.symbolDataList)
        {
            if (data == null) continue;

            int[] scoreArray = new int[3]; // index 0: 1圖案, 1: 2圖案, 2: 3圖案
            scoreArray[2] = data.payoutMultiplier * bet; // 只設定3個圖案連線才得分

            table[data.symbol] = scoreArray;
        }

        return table;
    }

    // 把機率轉成標準化比例（0~1 之間）
    public static List<float> NormalizeProbabilities(List<SymbolDataSO> symbolDataList)
    {
        float total = symbolDataList.Sum(s => s.probability); // 先算總和
        if (total == 0f)
            return symbolDataList.Select(s => 0f).ToList(); // 避免除以 0

        return symbolDataList.Select(s => s.probability / total).ToList();
    }

    // 輪軸動畫
    private IEnumerator SpinSlot(GameObject slot, SymbolDataSO finalSymbolData, float spinDuration, System.Action onComplete)
    {
        // Text text = slot.GetComponentInChildren<Text>();
        Image icon = slot.GetComponentInChildren<Image>();

        float timer = 0f;
        float interval = 0.05f; // 每幾秒切換一次圖案

        while (timer <= spinDuration)
        {
            // 顯示隨機圖案（不是最終結果，只是動畫用）
            SymbolDataSO randomSymbolData = GetRandomSymbolDataSO();
            // text.text = randomSymbolData.symbol.ToString();
            icon.sprite = randomSymbolData != null ? randomSymbolData.icon : null;

            yield return new WaitForSeconds(interval);
            timer += interval;
        }

        // 動畫結束後，顯示最終結果
        // text.text = finalSymbolData.symbol.ToString();
        icon.sprite = finalSymbolData != null ? finalSymbolData.icon : null;

        // 執行結束回調
        onComplete?.Invoke();

    }

    // 統計圖案連線次數
    public void UpdateSymbolCounts(List<Symbol> results)
    {
        // 更新symbolCountDict次數
        foreach (var symbol in results)
        {
            if (symbolCountDict.ContainsKey(symbol))
                symbolCountDict[symbol]++;
        }

        // 更新UI
        var symbols = symbolCountDict.Keys.OrderBy(s => s).ToList(); // 照著enum順序排序

        for (int i = 0; i < symbols.Count && i < symbolCountTextList.Count; i++)
        {
            var symbol = symbols[i];
            symbolCountTextList[i].text = $"{symbolCountDict[symbol]}";
        }

    }

    [ContextMenu("🧪 模擬抽卡 1000 次")]
    public void SimulateDraws()
    {
        int count = 1000;
        Dictionary<Symbol, int> result = new();

        for (int i = 0; i < count; i++)
        {
            SymbolDataSO randomSymbolData = GetRandomSymbolDataSO();
            Symbol symbol = randomSymbolData.symbol;
            if (!result.ContainsKey(symbol))
                result[symbol] = 0;
            result[symbol]++;
        }

        Debug.Log($"🎲 模擬 {count} 次抽卡結果：");

        foreach (var kv in result.OrderByDescending(kv => kv.Value))
        {
            float rate = kv.Value / (float)count;
            Debug.Log($"- {kv.Key}: {kv.Value} 次（{rate:P2}）");
        }
    }

    #endregion

    #region Events

    // 開始遊戲 按鈕
    public void OnClickStartBtn()
    {
        if (isSpinning || gameStatus == GameStatus.Auto)
            return;

        // CREDIT扣除BET的金額
        credit -= bet;

        // 計數器
        count += 1;
        countInputField.text = count.ToString();

        StartCoroutine(HandleSpinAndScore());
    }

    // 自動切換 按鈕
    public void OnClickToggleAutoSpin()
    {
        if (gameStatus == GameStatus.Stop)
        {
            StartAutoSpin();
        }
        else if (gameStatus == GameStatus.Auto)
        {
            StopAutoSpin();
        }

    }

    // 開始自動 
    public void StartAutoSpin()
    {
        gameStatus = GameStatus.Auto;
        autoStartText.text = "停止";

        StartCoroutine(AutoSpinLoop());

    }

    // 停止自動 
    public void StopAutoSpin()
    {
        gameStatus = GameStatus.Stop;
        autoStartText.text = "自動";

    }

    // 模擬快速轉輪軸 按鈕
    public void OnClickSimulateSpinBtn()
    {
        if (isSpinning || gameStatus == GameStatus.Auto)
            return;

        isSpinning = true;

        float totalBet = 0f;
        float totalWin = 0f;

        for (int i = 0; i < simulateCount; i++)
        {
            // CREDIT扣除BET的金額
            credit -= bet;
            totalBet += bet;

            // 計數器
            count += 1;
            countInputField.text = count.ToString();

            // 1. 預先決定每個位置的結果（結果先算好）
            List<SymbolDataSO> finalResults = new();
            for (int j = 0; j < slots.Count; j++)
            {
                finalResults.Add(GetRandomSymbolDataSO());
            }

            // 3. 計算分數
            int winScore = CalculateScore(finalResults);

            credit += winScore;
            totalWin += winScore;

            winInputField.text = winScore.ToString();
            creditInputField.text = credit.ToString();

        }

        // 計算 RTP
        float rtp = (totalWin / totalBet) * 100f;

        rtpInputField.text = $"{rtp:F1}%";
        
        isSpinning = false;

    }

    // 重置 按鈕
    public void OnClickResetBtn()
    {
        betInputField.text = initialBet.ToString();
        creditInputField.text = initialCredit.ToString();
        simulateInputField.text = initialSimulateCount.ToString();
        countInputField.text = "0";
        winInputField.text = "0";
        rtpInputField.text = "0";

        int.TryParse(betInputField.text, out bet);
        int.TryParse(creditInputField.text, out credit);
        int.TryParse(simulateInputField.text, out simulateCount);
        int.TryParse(countInputField.text, out count);

        // 重置UI
        for (int i = 0; i < symbolCountTextList.Count; i++)
        {
            symbolCountTextList[i].text = $"0";
        }

    }

    // 自動轉輪軸
    private IEnumerator AutoSpinLoop()
    {
        while (gameStatus == GameStatus.Auto)
        {
            // CREDIT扣除BET的金額
            credit -= bet;

            // 計數器
            count += 1;
            countInputField.text = count.ToString();

            yield return StartCoroutine(HandleSpinAndScore());

            // 可加入 delay 控制節奏
            yield return new WaitForSeconds(autoDelay);
        }
    }

    private IEnumerator HandleSpinAndScore()
    {
        isSpinning = true;

        // 1. 預先決定每個位置的結果（結果先算好）
        List<SymbolDataSO> finalResults = new();
        for (int i = 0; i < slots.Count; i++)
        {
            finalResults.Add(GetRandomSymbolDataSO());
        }

        // 2. 依序執行動畫：例如每個 slot 滾動 2 秒，最後停在指定圖案
        // 準備完成狀態紀錄表
        bool[] spinDone = new bool[slots.Count];

        // 同時執行所有 Slot 的 SpinSlot
        for (int i = 0; i < slots.Count; i++)
        {
            int index = i; // 關鍵：避免閉包錯誤
            StartCoroutine(SpinSlot(slots[i], finalResults[i], spinDuration + i * stopInterval, () =>
            {
                spinDone[index] = true;
            }));
        }

        // 等待所有轉盤完成
        yield return new WaitUntil(() => spinDone.All(done => done));

        // 3. 計算分數
        int winScore = CalculateScore(finalResults);

        credit += winScore;
        winInputField.text = winScore.ToString();
        creditInputField.text = credit.ToString();

        isSpinning = false;



    }

    // 輸入變更
    public void OnBetInputChanged(string input)
    {
        int.TryParse(input, out bet);
    }

    public void OnCreditInputChanged(string input)
    {
        int.TryParse(input, out credit);
    }
    public void OnSimulateInputChanged(string input)
    {
        int.TryParse(input, out simulateCount);
    }

    #endregion

}
