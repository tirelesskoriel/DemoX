using DemoX.Framework.Core;
using Mirror;
using UnityEngine;

namespace DemoX.Framework.Net
{
    public class PhysicsSimulator : NetworkBehaviour
    {
        PhysicsScene _physicsScene;
        PhysicsScene2D _physicsScene2D;

        bool _simulatePhysicsScene;
        bool _simulatePhysicsScene2D;

        public override void OnStartClient()
        {
            base.OnStartClient();
            enabled = false;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            enabled = true;
        }

        void Awake()
        {
            if (NetworkServer.active)
            {
                _physicsScene = gameObject.scene.GetPhysicsScene();
                _simulatePhysicsScene = _physicsScene.IsValid() && _physicsScene != Physics.defaultPhysicsScene;

                _physicsScene2D = gameObject.scene.GetPhysicsScene2D();
                _simulatePhysicsScene2D = _physicsScene2D.IsValid() && _physicsScene2D != Physics2D.defaultPhysicsScene;
            }
            else
            {
                enabled = false;
            }
        }

        [ServerCallback]
        void FixedUpdate()
        {
            if (_simulatePhysicsScene)
            {
                _physicsScene.Simulate(Time.fixedDeltaTime);
            }


            if (_simulatePhysicsScene2D)
                _physicsScene2D.Simulate(Time.fixedDeltaTime);
        }
    }
}