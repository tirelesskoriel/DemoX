using System.Collections.Generic;
using DemoX.Framework.Bridge.Event;
using DemoX.Framework.Core;
using Mirror;
using Unity.XR.CoreUtils;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace DemoX.Framework
{
    public class NetPlayer : NetworkBehaviour
    {
        [SerializeField] private Transform _debugLoggerContainer;
        [SerializeField] private Transform _aiUIContainer;
        [SerializeField] private Transform _aiBody;
        [SerializeField] private ECSOHandPoseTrigger _ecHandPoseTrigger;
        [SerializeField] private SkinnedMeshRenderer _headRenderer;

        private readonly SyncList<HandController> _serOwnedHands = new();

        private readonly List<HandController> _serOwnedHandList = new();

        public List<HandController> SerOwnedHands
        {
            get
            {
                _serOwnedHandList.Clear();
                _serOwnedHandList.AddRange(_serOwnedHands);
                return _serOwnedHandList;
            }
        }

        private BodyController _bodyController;

        private void Awake()
        {
            _bodyController = GetComponent<BodyController>();
        }

        public void SerAddOwnedHand(HandController handController)
        {
            handController.SerSetBodyCtrl(_bodyController);
            _serOwnedHands.Add(handController);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            DontDestroyOnLoad(gameObject);

            if (_aiUIContainer)
            {
                _aiUIContainer.gameObject.SetActive(isOwned);
            }

            if (_aiBody)
            {
                _aiBody.gameObject.SetActive(isOwned);
            }

            if (isOwned)
            {
                transform.EnableAnyWhereComponents<XROrigin>(true);
                transform.EnableAnyWhereComponents<InputActionManager>(true);
                transform.EnableAnyWhereComponents<PXR_Manager>(true);
                transform.EnableAnyWhereComponents<HandRayLine>(true);
                transform.EnableAnyWhereComponents<HandPoseTrigger>(true);
                transform.EnableAnyWhereComponents<PXR_Hand>(true);
                transform.EnableAnyWhereComponents<PXR_HandPose>(true);
                transform.EnableAnyWhereComponents<Camera>(true);
                transform.EnableAnyWhereComponents<AudioListener>(true);
                transform.EnableAnyWhereComponents<UIHandController>(true);
                transform.EnableAnyWhereComponents<TrackedPoseDriver>(true);
                transform.EnableAnyWhereComponents<TransformOffset>(true);
                transform.EnableAnyWhereComponents<RotateWarp>(true);
                transform.EnableAnyWhereComponents<FollowTo>(true);
                transform.EnableAnyWhereComponents<SceneMenu>(true);
                transform.EnableAnyWhereComponents<CameraSetup>(true);

                // event
                // transform.EnableAnyWhereComponents<XRInteractionManager>(true);
                // transform.EnableAnyWhereComponents<EventSystem>(true);
                // transform.EnableAnyWhereComponents<XRUIInputModule>(true);

                _headRenderer.enabled = false;

                if (_debugLoggerContainer && _debugLoggerContainer.parent.gameObject.activeSelf)
                {
                    XRLogger.Init();
                }

                if (_ecHandPoseTrigger)
                {
                    _ecHandPoseTrigger.ShowDebugLoggerTrigger.AddListener(DebugLoggerVisibleTrigger);
                }
            }
            else
            {
                transform.EnableAnyWhereComponents<XROrigin>(false);
                transform.EnableAnyWhereComponents<InputActionManager>(false);
                transform.EnableAnyWhereComponents<PXR_Manager>(false);
                transform.EnableAnyWhereComponents<HandRayLine>(false);
                transform.EnableAnyWhereComponents<HandPoseTrigger>(false);
                transform.EnableAnyWhereComponents<PXR_Hand>(false);
                transform.EnableAnyWhereComponents<PXR_HandPose>(false);
                transform.EnableAnyWhereComponents<Camera>(false);
                transform.EnableAnyWhereComponents<AudioListener>(false);
                transform.EnableAnyWhereComponents<UIHandController>(false);
                transform.EnableAnyWhereComponents<TrackedPoseDriver>(false);
                transform.EnableAnyWhereComponents<TransformOffset>(false);
                transform.EnableAnyWhereComponents<RotateWarp>(false);
                transform.EnableAnyWhereComponents<FollowTo>(false);
                transform.EnableAnyWhereComponents<SceneMenu>(false);
                transform.EnableAnyWhereComponents<CameraSetup>(false);


                // event
                // transform.EnableAnyWhereComponents<XRInteractionManager>(false);
                // transform.EnableAnyWhereComponents<EventSystem>(false);
                // transform.EnableAnyWhereComponents<XRUIInputModule>(false);

                _headRenderer.enabled = true;
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            transform.EnableAnyWhereComponents<XROrigin>(false);
            transform.EnableAnyWhereComponents<PXR_Manager>(false);
            transform.EnableAnyWhereComponents<HandPoseTrigger>(false);
            transform.EnableAnyWhereComponents<PXR_HandPose>(false);
            transform.EnableAnyWhereComponents<UIHandController>(false);
            transform.EnableAnyWhereComponents<SceneMenu>(false);
            transform.EnableAnyWhereComponents<FollowTo>(false);

            transform.EnableAnyWhereComponents<Animator>(false);
            transform.EnableAnyWhereComponents<CameraSetup>(false);


            if (Application.platform == RuntimePlatform.WindowsServer)
            {
                transform.EnableAnyWhereComponents<Camera>(false);
                transform.EnableAnyWhereComponents<AudioListener>(false);
            }
        }

        public void DebugLoggerVisibleTrigger()
        {
            if (_debugLoggerContainer)
            {
                _debugLoggerContainer.gameObject.SetActive(!_debugLoggerContainer.gameObject.activeSelf);
            }
        }
    }
}