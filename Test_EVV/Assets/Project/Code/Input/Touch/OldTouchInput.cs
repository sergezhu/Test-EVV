namespace Code.Input.Touch
{
    using _Game._Scripts.Level;
    using UniRx;
    using UnityEngine;
    using Zenject;

    public class OldTouchInput : ITouchInput, IInitializable, ITickable
    {
       // [Inject] IInputManager _inputManager;
        [Inject] CMCameraController _cameraController;

        bool _isTouching;
        bool _waitTouch;
        float _touchWorldHeight;
        

        public Vector2 TouchPosition => Input.mousePosition;
        public ReactiveCommand<TouchData> OnTouchStart { get; } = new ReactiveCommand<TouchData>();
        public ReactiveCommand<TouchData> OnTouchEnd { get; } = new ReactiveCommand<TouchData>();
        public ReactiveCommand<TouchData> OnTouchPositionChanged { get; } = new ReactiveCommand<TouchData>();

        Vector2Int ScreenSize => new Vector2Int( Screen.width, Screen.height );


        public void Initialize()
        {
            
        }

        public void Tick()
        {
            if(Input.GetMouseButtonDown( 0 ))
                OnStartTouch();

            if ( Input.GetMouseButtonUp( 0 ) )
                OnEndTouch();

            if ( Input.GetMouseButton( 0 ) )
                OnPositionChanged();
        }


        void OnPositionChanged()
        {
            if (_isTouching == false || _waitTouch)
                return;

            var touchData = CreateTouchData( TouchPosition );
            OnTouchPositionChanged.Execute( touchData );
        }

        void OnStartTouch()
        {
            if(_isTouching)
                return;

            _isTouching = true;

            StartTouchBehaviour();
        }

        void StartTouchBehaviour()
        {
            var touchData = CreateTouchData( TouchPosition );
            OnTouchStart.Execute( touchData);
        }

        void OnEndTouch()
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


        Vector3 TouchToWorld( Vector2 touchPos, int cameraIndex )
        {
            Vector2 screenSize = ScreenSize;

            Vector2 screenRelativePos = new Vector2( touchPos.x / screenSize.x, touchPos.y / screenSize.y );
            Vector3 worldPos = _cameraController.GetPointAtHeight( cameraIndex, screenRelativePos.x, screenRelativePos.y, _touchWorldHeight );
            return worldPos;
        }

        Vector3 WorldPosToDirection( Vector3 worldPos, int cameraIndex )
        {
            var camPos = _cameraController.ActiveCameraPosition( cameraIndex );
            return worldPos - camPos;
        }
    }
}