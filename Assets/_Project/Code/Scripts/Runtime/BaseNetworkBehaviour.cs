using Mirror;
using UnityEngine;

namespace Runtime
{
    public class BaseNetworkBehaviour : NetworkBehaviour
    {
        protected bool IsServerStarted;
        protected bool IsClientStarted;

        public override void OnStartServer()
        {
            base.OnStartServer();
            IsServerStarted = true;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            IsServerStarted = false;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            IsClientStarted = true;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            IsClientStarted = false;
        }

        private void FixedUpdate()
        {
            if (isServer || isServerOnly)
            {
                ServerFixedUpdate();
            }
            else if (isClient || isClientOnly)
            {
                ClientFixedUpdate();
            }
        }

        [ServerCallback]
        protected virtual void ServerFixedUpdate()
        {
        }

        protected virtual void ClientFixedUpdate()
        {
        }

        private void Update()
        {
            if (isServer || isServerOnly)
            {
                ServerUpdate();
            }
            else if (isClient || isClientOnly)
            {
                ClientUpdate();
            }
        }

        [ServerCallback]
        protected virtual void ServerUpdate()
        {
        }

        protected virtual void ClientUpdate()
        {
        }

        private void LateUpdate()
        {
            if (isServer || isServerOnly)
            {
                ServerLateUpdate();
            }
            else if (isClient || isClientOnly)
            {
                ClientLateUpdate();
            }
        }

        [ServerCallback]
        protected virtual void ServerLateUpdate()
        {
        }

        protected virtual void ClientLateUpdate()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isServer || isServerOnly)
            {
                OnServerTriggerEnter(other);
            }
            else if (isClient || isClientOnly)
            {
                OnClientTriggerEnter(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (isServer || isServerOnly)
            {
                OnServerTriggerExit(other);
            }
            else if (isClient || isClientOnly)
            {
                OnClientTriggerExit(other);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (isServer || isServerOnly)
            {
                OnServerTriggerStay(other);
            }
            else if (isClient || isClientOnly)
            {
                OnClientTriggerStay(other);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (isServer || isServerOnly)
            {
                OnServerCollisionEnter(other);
            }
            else if (isClient || isClientOnly)
            {
                OnClientCollisionEnter(other);
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (isServer || isServerOnly)
            {
                OnServerCollisionExit(other);
            }
            else if (isClient || isClientOnly)
            {
                OnClientCollisionExit(other);
            }
        }

        private void OnCollisionStay(Collision other)
        {
            if (isServer || isServerOnly)
            {
                OnServerCollisionStay(other);
            }
            else if (isClient || isClientOnly)
            {
                OnClientCollisionStay(other);
            }
        }

        [ServerCallback]
        protected virtual void OnServerTriggerEnter(Collider other)
        {
        }

        protected virtual void OnClientTriggerEnter(Collider other)
        {
        }

        [ServerCallback]
        protected virtual void OnServerTriggerExit(Collider other)
        {
        }

        protected virtual void OnClientTriggerExit(Collider other)
        {
        }

        [ServerCallback]
        protected virtual void OnServerTriggerStay(Collider other)
        {
        }

        protected virtual void OnClientTriggerStay(Collider other)
        {
        }

        [ServerCallback]
        protected virtual void OnServerCollisionEnter(Collision other)
        {
        }


        protected virtual void OnClientCollisionEnter(Collision other)
        {
        }

        [ServerCallback]
        protected virtual void OnServerCollisionExit(Collision other)
        {
        }

        protected virtual void OnClientCollisionExit(Collision other)
        {
        }

        [ServerCallback]
        protected virtual void OnServerCollisionStay(Collision other)
        {
        }

        protected virtual void OnClientCollisionStay(Collision other)
        {
        }
        
        protected virtual void EnableTransform(Transform trans, bool enable)
        {
            if (trans)
            {
                trans.gameObject.SetActive(enable);
            }
        }

        protected virtual void EnableTransform(MonoBehaviour behaviour, bool enable)
        {
            if (behaviour)
            {
                behaviour.enabled = enable;
            }
        }
        
        protected virtual void EnableTransform(Renderer r, bool enable)
        {
            if (r)
            {
                r.enabled = enable;
            }
        }
    }
}