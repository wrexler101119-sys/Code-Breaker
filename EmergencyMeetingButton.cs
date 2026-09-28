using UnityEngine;

public class EmergencyMeetingButton : MonoBehaviour
{

    public void OnEmergencyPressed()
    {
        Player player = GAMEUIMANAGER.Singleton.LocalPlayer;

        if (player == null)
            return;
        
        if (player.IsGhost)
            return;

        // 🔥 Don't allow emergency if cooldown is active
        if (MeetingManager.Instance.EmergencyOnCooldown())
        {
            Debug.Log(
                $"Emergency Button Cooldown: {Mathf.CeilToInt(MeetingManager.Instance.GetEmergencyCooldownRemaining())}s remaining");

            return;
        }

        ReachDetector reach = player.GetComponentInChildren<ReachDetector>();

        if (reach == null)
            return;

        EmergencyButton emergency = reach.CurrentEmergencyButton;

        if (emergency == null)
            return;

        emergency.Press(player);

        MobileControlsManager.Instance.SetControlsEnabled(false);
        MobileControlsManager.Instance.ShowJoystick(false);
       
    }
}