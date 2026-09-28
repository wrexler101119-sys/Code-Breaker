using UnityEngine;

public class MobileJoystickInput : MonoBehaviour
{
    public static MobileJoystickInput Instance;

    public Joystick joystick;

    private void Awake()
    {
        Instance = this;
    }

    public Vector2 MoveInput
    {
        get
        {
            return new Vector2(
                joystick.Horizontal,
                joystick.Vertical
            );
        }
    }
}