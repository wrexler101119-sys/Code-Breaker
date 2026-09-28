using UnityEngine;
using UnityEngine.UI;

public class AboutUsUI : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;

    [Header("About Us")]
    [SerializeField] private GameObject aboutUsPanel;

    [Header("Buttons")]
    [SerializeField] private Button aboutUsButton;
    [SerializeField] private Button backButton;
    [Header("How to play")]
    [SerializeField] private GameObject howToPlayPanel;

    [Header("Buttons")]
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button hbackButton;

    private void Awake()
    {
        // Make sure About Us starts hidden.
        if (aboutUsPanel != null)
        {
            aboutUsPanel.SetActive(false);
        }

        // About button
        if (aboutUsButton != null)
        {
            aboutUsButton.onClick.RemoveAllListeners();
            aboutUsButton.onClick.AddListener(OpenAboutUs);
        }

        // Back button
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(CloseAboutUs);
        }

        // How to Play button
        if (howToPlayButton != null)
        {
            howToPlayButton.onClick.RemoveAllListeners();
            howToPlayButton.onClick.AddListener(OpenHowToPlay);
        }

        // How to Play back button
        if (hbackButton != null)
        {
            hbackButton.onClick.RemoveAllListeners();
            hbackButton.onClick.AddListener(CloseHowToPlay);
        }
    }

    public void OpenAboutUs()
    {
        Debug.Log("ABOUT US BUTTON PRESSED");

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (aboutUsPanel != null)
        {
            aboutUsPanel.SetActive(true);
            aboutUsPanel.transform.SetAsLastSibling();
        }

        Debug.Log("ABOUT US PANEL OPENED");
    }

    public void CloseAboutUs()
    {
        Debug.Log("ABOUT US BACK BUTTON PRESSED");

        if (aboutUsPanel != null)
        {
            aboutUsPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        Debug.Log("ABOUT US PANEL CLOSED");
    }
    public void OpenHowToPlay()
    {
        Debug.Log("HOW TO PLAY BUTTON PRESSED");

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
            howToPlayPanel.transform.SetAsLastSibling();
        }

        Debug.Log("HOW TO PLAY PANEL OPENED");
    }

    public void CloseHowToPlay()
    {
        Debug.Log("HOW TO PLAY BACK BUTTON PRESSED");

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        Debug.Log("HOW TO PLAY PANEL CLOSED");
    }
}