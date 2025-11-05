using UnityEngine;
using TMPro;

public class UpgradePopup : MonoBehaviour
{
    public TMP_Text popupText;
    public float floatSpeed = 1.5f;
    public float duration = 3f;

    void Start()
    {
        Destroy(gameObject, duration);
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }

    public void SetText(string text, Color? color = null)
    {
        if (popupText != null)
        {
            popupText.text = text;
            if (color.HasValue)
                popupText.color = color.Value;
        }
    }
}