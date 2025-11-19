namespace Code.Input.Touch
{
    using System.Collections;
    using _Game._Scripts.Level;
    using _Game._Scripts.Utilities.Extensions;
    using Cysharp.Threading.Tasks;
    using UniRx;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.EnhancedTouch;
    using Zenject;

    public class NewTouchInput : ITouchInput, IInitializable
    {
        [Inject] IInputManager _inputManager;
        [Inject] CMCameraController _cameraController;

        bool _isTouching;
        bool _waitTouch;
        float _touchWorldHeight;
        

        public Vector2 TouchPosition => TouchActions.TouchPosition.ReadValue<Vector2>();
        public int TouchCount => UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count;
        public ReactiveCommand<TouchData> OnTouchStart { get; } = new ReactiveCommand<TouchData>();
        public ReactiveCommand<TouchData> OnTouchEnd { get; } = new ReactiveCommand<TouchData>();
        public ReactiveCommand<TouchData> OnTouchPositionChanged { get; } = new ReactiveCommand<TouchData>();

        InputActions.TouchActions TouchActions => _inputManager.Touch;
        Vector2Int ScreenSize => new Vector2Int( Screen.width, Screen.height );
        

        public void Initialize()
        {
            EnhancedTouchSupport.Enable();
            TouchSimulation.Enable();

            _inputManager.Touch.TouchPress.SubscribeToPerformed( OnTouchPress );
            _inputManager.Touch.TouchRelease.SubscribeToPerformed( OnTouchRelease );
            _inputManager.Touch.TouchPosition.SubscribeToPerformed( OnPositionChanged );
        }

        void OnTouchPress(InputAction.CallbackContext ctx)
        {
            OnStartTouch(ctx);
        }

        void OnTouchRelease(InputAction.CallbackContext ctx)
        {
            OnEndTouch(ctx);
        }

        void OnPositionChanged(InputAction.CallbackContext ctx)
        {
            if (_isTouching == false || _waitTouch)
                return;

            var touchPos = TouchPosition;
            var touchData = CreateTouchData( TouchPosition );

            OnTouchPositionChanged.Execute( touchData );
        }

        void OnStartTouch(InputAction.CallbackContext ctx)
        {
            if(_isTouching)
                return;

            _isTouching = true;

            // New Input System return 0,0 when first time take a value
            // https://forum.unity.com/threads/first-position-of-touch-contact-is-0-0.1039135/
            //StartCoroutine(StartTouchRoutine(ctx));

            StartTouchRoutine( ctx ).ToUniTask();
        }

        IEnumerator StartTouchRoutine(InputAction.CallbackContext ctx)
        {
            _waitTouch = true;
            yield return new WaitForSeconds(0.05f);

            _waitTouch = false;
            StartTouchBehaviour(ctx);
        }

        void StartTouchBehaviour(InputAction.CallbackContext ctx)
        {
            Vector2 touchPos = TouchPosition;
            var touchData = CreateTouchData( touchPos );

            OnTouchStart.Execute( touchData );
        }

        void OnEndTouch(InputAction.CallbackContext ctx)
        {
            if(_isTouching == false)
                return;

            _isTouching = false;

            OnTouchEnd.Execute( default );
        }

        private TouchData CreateTouchData( Vector2 touchPos )
        {
            var worldPosOfMainCam = TouchToWorld( touchPos, 0 );
            var worldDirOfMainCam = WorldPosToDirection( worldPosOfMainCam, 0 );

            var worldPosOfMergeCam = TouchToWorld( touchPos, 1 );
            var worldDirOfMergeCam = WorldPosToDirection( worldPosOfMergeCam, 1 );

            var touchData = new TouchData()
            {
                ScreenPosition = touchPos,
                WorldProjectionFromMergeCamera = worldPosOfMergeCam,
                WorldDirectionFromMergeCamera = worldDirOfMergeCam,
                WorldProjectionFromMainCamera = worldPosOfMainCam,
                WorldDirectionFromMainCamera = worldDirOfMainCam,
                MainCameraPosition = _cameraController.ActiveCameraPosition( 0 ),
                MergeCameraPosition = _cameraController.ActiveCameraPosition( 1 )
            };
            return touchData;
        }


        Vector3 TouchToWorld(Vector2 touchPos, int cameraIndex)
        {
            Vector2 screenSize = ScreenSize;

            Vector2 screenRelativePos = new Vector2(touchPos.x / screenSize.x, touchPos.y / screenSize.y);
            Vector3 worldPos = _cameraController.GetPointAtHeight(cameraIndex, screenRelativePos.x, screenRelativePos.y, _touchWorldHeight);
            return worldPos;
        }

        Vector3 WorldPosToDirection( Vector3 worldPos, int cameraIndex )
        {
            var camPos = _cameraController.ActiveCameraPosition(cameraIndex);
            return worldPos - camPos;
        }
    }
}