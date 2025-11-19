namespace Utilities.UI
{
	using System;
	using Sirenix.OdinInspector;
	using UnityEngine;
	using UnityEngine.UI;

	[ExecuteInEditMode]
	public class CircularLayoutGroup : LayoutGroup
	{
		public enum CircularDirection
		{
			CCW,
			CW
		}
		
		public enum AngleStepType
		{
			Fixed,
			Auto
		}
		
		public float radius = 100f;    // Радиус окружности
		public float angleOffset = 0f; // Смещение начального угла
		public CircularDirection direction;
		public AngleStepType angleStepType;
		[ShowIf( "angleStepType", AngleStepType.Fixed )]
		public float fixedAngleStep;

		protected override void OnEnable()
		{
			base.OnEnable();
			CalculateRadial();
		}

		public override void SetLayoutHorizontal()
		{
		}

		public override void SetLayoutVertical()
		{
		}

		public override void CalculateLayoutInputHorizontal()
		{
			CalculateRadial();
		}

		public override void CalculateLayoutInputVertical()
		{
			CalculateRadial();
		}

		private void CalculateRadial()
		{
			m_Tracker.Clear();
			if ( transform.childCount == 0 )
				return;

			// Вычисляем угол между элементами
			float angleStep = 0;
			
			switch ( angleStepType )
			{
				case AngleStepType.Fixed:
					angleStep = fixedAngleStep;
					break;
				case AngleStepType.Auto:
					angleStep = 360f / transform.childCount;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			for ( int i = 0; i < transform.childCount; i++ )
			{
				RectTransform child = (RectTransform)transform.GetChild( i );
				if ( child == null )
					continue;

				m_Tracker.Add( this, child,
							   DrivenTransformProperties.Anchors |
							   DrivenTransformProperties.AnchoredPosition |
							   DrivenTransformProperties.Pivot );

				// Устанавливаем якоря и пивот для центрирования элемента
				child.anchorMin = child.anchorMax = child.pivot = new Vector2( 0.5f, 0.5f );

				// Вычисляем угол для текущего элемента
				float dirMul = direction == CircularDirection.CCW ? 1f : -1f;
				float angle = angleOffset + dirMul * angleStep * i;
				float radians = angle * Mathf.Deg2Rad;

				// Вычисляем позицию элемента на окружности
				Vector2 childPosition = new Vector2(
					Mathf.Cos( radians ) * radius,
					Mathf.Sin( radians ) * radius
				);

				// Устанавливаем позицию элемента
				child.anchoredPosition = childPosition;
			}
		}
	}
}