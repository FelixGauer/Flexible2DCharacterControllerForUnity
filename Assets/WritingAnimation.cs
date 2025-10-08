using System.Collections;
using UnityEngine;
using TMPro;

public class WritingAnimation : MonoBehaviour
{
    [SerializeField] private TMP_Text textComponent;  // Reference to the text UI component
    [SerializeField] private float typingSpeed = 0.05f; // Delay between each character

    private Coroutine typingCoroutine;   // Reference to the running coroutine
    private string fullText;             // The full text to type out


    /// Initialize with TMP_Text and optionally typing speed.

    public void Initialize(TMP_Text textComp, float speed = 0.05f)
    {
        // Assign the passed TMP_Text component to our local reference
        textComponent = textComp;

        // Set typing speed, or keep default if none provided
        typingSpeed = speed;

        // Optionally clear any existing text at initialization
        if (textComponent != null)
            textComponent.text = "";
    }


    /// Starts typing the provided text with animation.
    public void StartTyping(string text)
    {
        // If there's an ongoing typing coroutine, stop it to start fresh
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // Store the full text to be typed out
        fullText = text;

        // Start the typing coroutine which will display text gradually
        typingCoroutine = StartCoroutine(TypingCoroutine());
    }


    /// Immediately finish typing and display full text.
    public void SkipTyping()
    {
        // If a typing animation is running, stop it immediately
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // Instantly display the full text without animation
        if (textComponent != null && fullText != null)
        {
            textComponent.text = fullText;
        }
    }


    /// Returns true if typing animation is ongoing.

    public bool IsTyping()
    {
        // If typingCoroutine is not null, typing animation is in progress
        return typingCoroutine != null;
    }


    /// The coroutine that types text character-by-character.
    private IEnumerator TypingCoroutine()
    {
        // Clear the text component at the start
        textComponent.text = "";

        // Loop through each character in fullText
        for (int i = 0; i < fullText.Length; i++)
        {
            // Add the next character to the text component
            textComponent.text += fullText[i];

            // Wait for the delay between characters based on typingSpeed
            yield return new WaitForSeconds(typingSpeed);
        }

        // Typing finished, clear the coroutine reference
        typingCoroutine = null;

        // Optionally: Trigger an event or callback here to signal typing completion
    }
}
