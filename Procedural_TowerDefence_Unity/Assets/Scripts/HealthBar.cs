using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarImage;

    // Call this to update the health bar (value between 0 and 1)
    public void SetHealth(float healthNormalized)
    {
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = Mathf.Clamp01(healthNormalized);
        }
    }
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }
}
