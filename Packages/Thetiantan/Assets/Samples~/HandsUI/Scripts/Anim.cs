using UnityEngine;

public class Anim : MonoBehaviour
{
    public Transform target;
    public Animator planeAnimator;

    void Update()
    {
        // 获取目标对象的当前旋转四元数
        Quaternion currentRotation = target.localRotation;
        // 从当前旋转四元数中提取 x 轴旋转角度
        float angle = Quaternion.Angle(currentRotation, Quaternion.identity);
        // 将角度映射到0到1之间
        float progress = angle / 180f;

        Debug.Log(progress);


        // 获取当前动画状态信息
        AnimatorStateInfo stateInfo = planeAnimator.GetCurrentAnimatorStateInfo(0);

        // 检查是否在播放指定动画
        if (stateInfo.IsName("AnimationParameter"))
        {
            // 播放动画，并设置进度
            planeAnimator.Play(stateInfo.fullPathHash, 0, progress); // 保持使用 progress
        }
    }
}