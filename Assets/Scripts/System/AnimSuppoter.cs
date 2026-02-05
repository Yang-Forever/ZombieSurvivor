using UnityEngine;

/// <summary>
/// 좀비 애니메이션 클립 묶음
/// Animator Override 또는 상태별 재생용 데이터 컨테이너
/// </summary>
[System.Serializable]
public class Anim
{
    public AnimationClip Idle;
    public AnimationClip Move;
    public AnimationClip Attack;
    public AnimationClip Rage;
    public AnimationClip Dash;
    public AnimationClip Die;
}

public enum AnimState
{
    idle,
    trace,
    attack,
    rage,
    dash,
    die,
    count
}