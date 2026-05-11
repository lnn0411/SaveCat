using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class catView : MonoBehaviour
{
    //动画控制器
    [SerializeField] private Animator _animator;
    
    //动画状态哈希
    private readonly int _animIdle = Animator.StringToHash("Idle");
    private readonly int _animRun = Animator.StringToHash("Walk");
    private readonly int _animDie = Animator.StringToHash("Run");

    public void UpdatePositionAndRoatation(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}
