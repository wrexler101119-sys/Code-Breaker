using UnityEngine;

public class PanelOpener : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panel1;
    public GameObject panel2;
    public GameObject panel3;
    public GameObject panel4;

    // Button 1
    public void OpenPanel1()
    {
        CloseAllPanels();
        panel1.SetActive(true);
    }

    // Button 2
    public void OpenPanel2()
    {
        CloseAllPanels();
        panel2.SetActive(true);
    }

    // Button 3
    public void OpenPanel3()
    {
        CloseAllPanels();
        panel3.SetActive(true);
    }

    // Button 4
    public void OpenPanel4()
    {
        CloseAllPanels();
        panel4.SetActive(true);
    }

    // Exit button for any panel
    public void ExitPanel()
    {
        CloseAllPanels();
    }

    private void CloseAllPanels()
    {
        if (panel1 != null)
            panel1.SetActive(false);

        if (panel2 != null)
            panel2.SetActive(false);

        if (panel3 != null)
            panel3.SetActive(false);

        if (panel4 != null)
            panel4.SetActive(false);
    }
}