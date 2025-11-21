namespace Code.Input.Touch
{
	using System.Collections;
	using Code.Cameras;
	using Code.Core;
	using Code.MergeSystem;
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
		private bool waitTouch;
		private MergeConfig mergeConfig;

		public NewTouchInput(CameraController cameraController, IInputManager inputManager, MergeConfig mergeConfig)
		{
			this.cameraController = cameraController;
			this.inputManager = inputManager;
			this.mergeConfig = mergeConfig;
		}

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


		public void Construct(IInputManager inputManager, CameraController cameraController, MergeConfig mergeConfig)
		{
			this.inputManager = inputManager;
			this.cameraController = cameraController;
			this.mergeConfig = mergeConfig;
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

			Vector2 touchPos = TouchPosition;
			TouchData touchData = CreateTouchData(TouchPosition);

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
			Vector2 touchPos = TouchPosition;
			TouchData touchData = CreateTouchData(touchPos);

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
			Vector3 worldPosOfCam = TouchToWorld(touchPos);
			Vector3 worldDirOfCam = WorldPosToDirection(worldPosOfCam);

			TouchData touchData = new TouchData
			{
				ScreenPosition = touchPos,
				
				WorldProjectionFromMainCamera = worldPosOfCam,
				WorldDirectionFromMainCamera = worldDirOfCam,
				MainCameraPosition = cameraController.ActiveCameraPosition()
			};
			return touchData;
		}


		private Vector3 TouchToWorld(Vector2 touchPos)
		{
			Vector2 screenSize = ScreenSize;

			Vector2 screenRelativePos = new Vector2(touchPos.x / screenSize.x, touchPos.y / screenSize.y);
			Vector3 worldPos = cameraController.GetPointAtHeight(screenRelativePos.x, screenRelativePos.y, mergeConfig.TouchHeight);
			return worldPos;
		}

		private Vector3 WorldPosToDirection(Vector3 worldPos)
		{
			Vector3 camPos = cameraController.ActiveCameraPosition();
			return worldPos - camPos;
		}
	}
}