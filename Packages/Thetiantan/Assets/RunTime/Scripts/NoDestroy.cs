using UnityEngine;

namespace GData
{
    public class NoDestroy : MonoBehaviour
    {
        public static NoDestroy noDestroyInstence;
        private void Awake()
        {
            if (noDestroyInstence != null)
            {
                Destroy(gameObject);
                return;
            }
            noDestroyInstence = this;
            DontDestroyOnLoad(noDestroyInstence);
        }
        void Start()
        {
            
            if (this.transform.parent)
                this.transform.parent = null;

        }
    }
}