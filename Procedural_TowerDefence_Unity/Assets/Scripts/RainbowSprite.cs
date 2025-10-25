using UnityEngine;
using UnityEngine.UI;

public class RainbowSprite : MonoBehaviour
{
    [SerializeField] private Image image; 
    [SerializeField] private float speed = 1.0f; // Speed of the color change

    void Start()
    {
            // Get the Image component
            image = GetComponent<Image>();
    }

    void Update()
    {
        // Calculate the hue based on time to create a cycling effect
        float hue = (Time.time * speed) % 1.0f; // Hue cycles between 0 and 1
        Color rainbowColor = Color.HSVToRGB(hue, 1.0f, 1.0f); // Full saturation and value
        image.color = rainbowColor; // Apply the color to the image
    }
}