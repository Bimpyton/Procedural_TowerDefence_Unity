using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DebugConsole : MonoBehaviour
{
    private Queue<string> logQueue = new Queue<string>();
    private const int maxLogs = 30;
    [SerializeField] private Text logText;
    [SerializeField] private Canvas canvas;
    private bool logDirty = false;

    void Awake()
    {
    SetupUI();
    Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
    string color = "white";
    if (type == LogType.Warning) color = "yellow";
    else if (type == LogType.Error || type == LogType.Exception) color = "red";
    string formatted = $"<color={color}>{logString}</color>";
    logQueue.Enqueue(formatted);
    if (logQueue.Count > maxLogs) logQueue.Dequeue();
    logDirty = true;
    }

    void SetupUI()
    {
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("DebugCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }
        // Always set sorting order high for visibility
        canvas.sortingOrder = 999;
        if (logText == null)
        {
            GameObject textObj = new GameObject("DebugLogText");
            textObj.transform.SetParent(canvas.transform);
            logText = textObj.AddComponent<Text>();
            logText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            logText.fontSize = 16;
            logText.alignment = TextAnchor.LowerLeft;
            logText.horizontalOverflow = HorizontalWrapMode.Wrap;
            logText.verticalOverflow = VerticalWrapMode.Overflow;
            logText.color = Color.white;
            RectTransform rt = logText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0.4f);
            rt.offsetMin = new Vector2(10, 10);
            rt.offsetMax = new Vector2(-10, 200);
        }
    }

    void UpdateLogText()
    {
        if (logText == null) return;
        logText.text = string.Join("\n", logQueue.ToArray());
    }

    void Update()
    {
        if (logDirty)
        {
            UpdateLogText();
            logDirty = false;
        }
    }
}
