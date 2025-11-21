namespace Code.Core
{
	using System;
	using Code.Cameras;
	using Code.Database;
	using Code.Input;
	using Code.Input.Touch;
	using Code.MergeSystem;
	using UnityEngine;

	public class Bootstrap : MonoBehaviour
	{
		[Header("Configs")]
		[SerializeField] private MergeConfig mergeConfig;
		[SerializeField] private ItemsLibrary itemsLibrary;
		
		[Header("Views")]
		[SerializeField] private MergeBoardView mergeBoardView;
		
		[Header("Services")]
		[SerializeField] private CameraController cameraController;
		[SerializeField] private CoroutineRunner coroutineRunner;

		
		private LifetimeController lifetimeController;
		private IInstantiator instantiator;
		private InputManager inputManager;
		private NewTouchInput touchInput;
		private MergeItemFactory mergeItemFactory;
		private MergeBoardController mergeBoardController;
		private BoardState boardState;
		private SceneRunner sceneRunner;
		private BoardCellFactory boardCellsFactory;


		private void Awake()
		{
			lifetimeController = new LifetimeController();
			instantiator = new Instantiator();
			
			InstallViews();
			InstallServices();

			// Initialize all IInitializable in orders as they was added
			lifetimeController.Initialize();
			
			sceneRunner = new SceneRunner(mergeBoardController, coroutineRunner);
			sceneRunner.Run();
		}

		private void InstallServices()
		{
			inputManager = new InputManager();
			lifetimeController.AddInitializable(inputManager);
			
			touchInput = new NewTouchInput(cameraController, inputManager, mergeConfig);
			lifetimeController.AddInitializable(touchInput);
			
			mergeItemFactory = new MergeItemFactory(mergeConfig, itemsLibrary, instantiator);
			boardState = new BoardState(mergeConfig.StartMergeLevel);
			
			mergeBoardController = new MergeBoardController(mergeBoardView, mergeItemFactory, mergeConfig, touchInput, boardState);
			lifetimeController.AddInitializable(mergeBoardController);
		}

		private void InstallViews()
		{
			boardCellsFactory = new BoardCellFactory(mergeConfig, itemsLibrary, instantiator);
			
			mergeBoardView.Construct(mergeConfig, boardCellsFactory);
			lifetimeController.AddInitializable(mergeBoardView);
		}
	}
}