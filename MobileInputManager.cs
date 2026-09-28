using UnityEngine;

public class MobileInputManager : MonoBehaviour
{
    public static MobileInputManager Instance;

    [SerializeField]
    private Joystick joystick;

    private void Awake()
    {
        Instance = this;
    }

    public Vector2 MoveInput
    {
        get
        {
            if (joystick == null)
                return Vector2.zero;

            return new Vector2(
                joystick.Horizontal,
                joystick.Vertical);
        }
    }
}