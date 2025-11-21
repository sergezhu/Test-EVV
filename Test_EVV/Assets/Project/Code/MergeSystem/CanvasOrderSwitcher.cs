namespace Code.MergeSystem
{
	using System;
	using UnityEngine;

	[RequireComponent(typeof(Canvas))]
	public class CanvasOrderSwitcher : MonoBehaviour
	{
		[SerializeField] private int defaultOrder;
		[SerializeField] private int draggedOrder;
		
		private Canvas canvas;

		private void Awake()
		{
			canvas = GetComponent<Canvas>();
		}
		
		public void SetDefaultOrder()
		{
			canvas.sortingOrder = defaultOrder;
		}
		
		public void SetDraggedOrder()
		{
			canvas.sortingOrder = draggedOrder;
		}
	}
}