namespace Code.Input.Touch
{
	using System.Collections;
	using Code.Cameras;
	using Code.Core;
	using Cysharp.Threading.Tasks;
	using UniRx;
	using UnityEngine;
	using UnityEngine.InputSystem;
	using UnityEngine.InputSystem.EnhancedTouch;
	using Utilities.Extensions;
	using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

	public class NewTouchInput : ITouchInput, IInitializable
	{
		private CameraController cameraController;
		private IInputManager inputManager;

		private bool isTouching;
		private float touchWorldHeight;
		private bool waitTouch;
		public int TouchCount => Touch.activeTouches.Count;

		private InputActions.TouchActions TouchActions => inputManager.Touch;
		private Vector2Int ScreenSize => new Vector2Int(Screen.width, Screen.height);


		public void Initialize()
		{
			EnhancedTouchSupport.Enable();
			TouchSimulation.Enable();

			inputManager.Touch.TouchPress.SubscribeToPerformed(OnTouchPress);
			inputManager.Touch.TouchRelease.SubscribeToPerformed(OnTouchRelease);
			inputManager.Touch.TouchPosition.SubscribeToPerformed(OnPositionChanged);
		}


		public Vector2 TouchPosition => TouchActions.TouchPosition.ReadValue<Vector2>();
		public ReactiveCommand<TouchData> OnTouchStart { get; } = new ReactiveCommand<TouchData>();
		public ReactiveCommand<TouchData> OnTouchEnd { get; } = new ReactiveCommand<TouchData>();
		public ReactiveCommand<TouchData> OnTouchPositionChanged { get; } = new ReactiveCommand<TouchData>();


		public void Construct(IInputManager inputManager, CameraController cameraController)
		{
			this.inputManager = inputManager;
			this.cameraController = cameraController;
		}

		private void OnTouchPress(InputAction.CallbackContext ctx)
		{
			OnStartTouch(ctx);
		}

		private void OnTouchRelease(InputAction.CallbackContext ctx)
		{
			OnEndTouch(ctx);
		}

		private void OnPositionChanged(InputAction.CallbackContext ctx)
		{
			if (isTouching == false || waitTouch)
				return;

			var touchPos = TouchPosition;
			var touchData = CreateTouchData(TouchPosition);

			OnTouchPositionChanged.Execute(touchData);
		}

		private void OnStartTouch(InputAction.CallbackContext ctx)
		{
			if (isTouching)
				return;

			isTouching = true;

			// New Input System return 0,0 when first time take a value
			// https://forum.unity.com/threads/first-position-of-touch-contact-is-0-0.1039135/
			//StartCoroutine(StartTouchRoutine(ctx));

			StartTouchRoutine(ctx).ToUniTask();
		}

		private IEnumerator StartTouchRoutine(InputAction.CallbackContext ctx)
		{
			waitTouch = true;
			yield return new WaitForSeconds(0.05f);

			waitTouch = false;
			StartTouchBehaviour(ctx);
		}

		private void StartTouchBehaviour(InputAction.CallbackContext ctx)
		{
			var touchPos = TouchPosition;
			var touchData = CreateTouchData(touchPos);

			OnTouchStart.Execute(touchData);
		}

		private void OnEndTouch(InputAction.CallbackContext ctx)
		{
			if (isTouching == false)
				return;

			isTouching = false;

			OnTouchEnd.Execute(default);
		}

		private TouchData CreateTouchData(Vector2 touchPos)
		{
			var worldPosOfMainCam = TouchToWorld(touchPos, 0);
			var worldDirOfMainCam = WorldPosToDirection(worldPosOfMainCam, 0);

			var worldPosOfMergeCam = TouchToWorld(touchPos, 1);
			var worldDirOfMergeCam = WorldPosToDirection(worldPosOfMergeCam, 1);

			var touchData = new TouchData
			{
				ScreenPosition = touchPos,
				WorldProjectionFromMergeCamera = worldPosOfMergeCam,
				WorldDirectionFromMergeCamera = worldDirOfMergeCam,
				WorldProjectionFromMainCamera = worldPosOfMainCam,
				WorldDirectionFromMainCamera = worldDirOfMainCam,
				MergeCameraPosition = cameraController.ActiveCameraPosition()
			};
			return touchData;
		}


		private Vector3 TouchToWorld(Vector2 touchPos, int cameraIndex)
		{
			Vector2 screenSize = ScreenSize;

			var screenRelativePos = new Vector2(touchPos.x / screenSize.x, touchPos.y / screenSize.y);
			var worldPos = cameraController.GetPointAtHeight(screenRelativePos.x, screenRelativePos.y, touchWorldHeight);
			return worldPos;
		}

		private Vector3 WorldPosToDirection(Vector3 worldPos, int cameraIndex)
		{
			var camPos = cameraController.ActiveCameraPosition();
			return worldPos - camPos;
		}
	}
}