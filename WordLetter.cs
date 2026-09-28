using UnityEngine;
using UnityEngine.UI;

public class WordLetter : MonoBehaviour
{
    public string letter;

    [HideInInspector]
    public bool isUsed;

    private Button button;

    private Transform startParent;
    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;
    private Vector3 startLocalScale;

    private void Awake()
    {
        button = GetComponent<Button>();

        startParent = transform.parent;
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
        startLocalScale = transform.localScale;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickLetter);
        }
    }

    private void OnClickLetter()
    {
        if (LaptopTaskManager.Instance == null)
        {
            Debug.LogError(
                "LAPTOP LETTER FAILED | LaptopTaskManager.Instance is null");

            return;
        }

        if (isUsed)
        {
            ReturnToOriginal();
        }
        else
        {
            LaptopTaskManager.Instance.SelectLetter(this);
        }
    }

    public void MoveToSlot(Transform slot)
    {
        if (slot == null)
            return;

        isUsed = true;

        transform.SetParent(slot, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void ReturnToOriginal()
    {
        isUsed = false;

        transform.SetParent(startParent, false);
        transform.localPosition = startLocalPosition;
        transform.localRotation = startLocalRotation;
        transform.localScale = startLocalScale;

        if (LaptopTaskManager.Instance != null)
        {
            LaptopTaskManager.Instance.RemoveLetter(this);
        }
    }

    // Used when resetting the whole puzzle.
    // It does not call RemoveLetter again.
    public void ResetLetter()
    {
        isUsed = false;

        transform.SetParent(startParent, false);
        transform.localPosition = startLocalPosition;
        transform.localRotation = startLocalRotation;
        transform.localScale = startLocalScale;

        if (button != null)
        {
            button.interactable = true;
        }
    }
}