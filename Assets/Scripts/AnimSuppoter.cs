using UnityEngine;

[System.Serializable]
public class Anim
{
    public AnimationClip Idle;
    public AnimationClip Move;
    public AnimationClip Attack;
    public AnimationClip Hit;
    public AnimationClip Rage;
    public AnimationClip Dash;
    public AnimationClip Die;
}

public enum AnimState
{
    idle,
    trace,
    attack,
    hit,
    rage,
    dash,
    die,
    count
}