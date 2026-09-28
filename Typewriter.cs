using TMPro;
using UnityEngine;
using System.Collections;

public class ShiftingText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public string fullText = "CODE BREAKER";
    public float delay = 0.2f;

    private Coroutine shiftCoroutine;

    void OnEnable()
    {
        // Stop previous coroutine if any
        if (shiftCoroutine != null)
        {
            StopCoroutine(shiftCoroutine);
        }

        if (text != null)
        {
            text.text = fullText; // Reset text when enabling
            shiftCoroutine = StartCoroutine(ShiftLoop());
        }
    }

    void OnDisable()
    {
        // Stop coroutine when object is disabled
        if (shiftCoroutine != null)
        {
            StopCoroutine(shiftCoroutine);
            shiftCoroutine = null;
        }
    }

    IEnumerator ShiftLoop()
    {
        string currentText = fullText;

        while (true)
        {
            if (text != null)
                text.text = currentText;

            yield return new WaitForSeconds(delay);

            // Move first letter to the end
            currentText = currentText.Substring(1) + currentText[0];
        }
    }
}