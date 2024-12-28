using UnityEngine;

namespace Runtime
{
    public class Hand : BaseNetworkBehaviour
    {
        [SerializeField] private Transform m_Elbow;
        [SerializeField] private Transform _arm;
        [SerializeField] private Transform _armParent;

        protected override void ClientUpdate()
        {
            ArmIK();
        }

        private void ArmIK()
        {
            _armParent.LookAt(m_Elbow);
            _arm.transform.localEulerAngles = new Vector3(0, -180, _armParent.parent.localEulerAngles.z);
        }
    }
}