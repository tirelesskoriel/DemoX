using UnityEngine;

public class Anim : MonoBehaviour
{
    public Transform target;
    public Animator planeAnimator;

    void Update()
    {
        // ��ȡĿ�����ĵ�ǰ��ת��Ԫ��
        Quaternion currentRotation = target.localRotation;
        // �ӵ�ǰ��ת��Ԫ������ȡ x ����ת�Ƕ�
        float angle = Quaternion.Angle(currentRotation, Quaternion.identity);
        // ���Ƕ�ӳ�䵽0��1֮��
        float progress = angle / 180f;

        Debug.Log(progress);


        // ��ȡ��ǰ����״̬��Ϣ
        AnimatorStateInfo stateInfo = planeAnimator.GetCurrentAnimatorStateInfo(0);

        // ����Ƿ��ڲ���ָ������
        if (stateInfo.IsName("AnimationParameter"))
        {
            // ���Ŷ����������ý���
            planeAnimator.Play(stateInfo.fullPathHash, 0, progress); // ����ʹ�� progress
        }
    }
}