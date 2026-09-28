using UnityEngine;

public class MobileControls : MonoBehaviour
{
    [SerializeField]
    private GameObject mobileUI;

    private void Start()
    {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
        mobileUI.SetActive(true);
#else
        mobileUI.SetActive(false);
#endif
    }
}