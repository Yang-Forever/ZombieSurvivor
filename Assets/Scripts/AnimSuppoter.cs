using UnityEngine;

[System.Serializable]
public class Anim
{
    public AnimationClip Idle;
    public AnimationClip Move;
    public AnimationClip Attack;
    public AnimationClip hit;
    public AnimationClip Die;
}

public enum AnimState
{
    idle,
    move,
    trace,
    attack,
    hit,
    die,
    count
}