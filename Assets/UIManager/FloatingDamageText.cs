using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloatingDamageText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float fadeOutTime = 1f;
    private TextMeshProUGUI text;

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        text.color = Color.Lerp(text.color, Color.clear, Time.deltaTime / fadeOutTime);

        if (text.color.a <= 0.1f)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string textToDisplay)
    {
        text = GetComponent<TextMeshProUGUI>();
        text.SetText(textToDisplay);
    }
}