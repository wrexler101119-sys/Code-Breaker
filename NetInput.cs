using Fusion;
using UnityEngine;

public enum InputButton
{
    Jump,
    UseAbility, // ← THIS is your KILL button
    Fire
}

public struct NetInput : INetworkInput
{
    public NetworkButtons Buttons;
    public Vector2 Direction;
    public Vector2 LookDelta;
}