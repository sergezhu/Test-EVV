namespace Utilities.UI
{
	using UnityEngine;
	using Utilities.Extensions;
	using Utilities.RxUI;

	public class TutorialMaskOwner : RectTransformOwner
	{
		[SerializeField] private UITutorialSingleMask _uiTutorialSingleMask;

		
		public void UpdateMask()
		{
			_uiTutorialSingleMask.SetSize( Size, 20, 20 );
			_uiTutorialSingleMask.SetPosition( Center() );
		}

		public void EnableTutorialMask()
		{
			_uiTutorialSingleMask.Show();
		}

		public void DisableTutorialMask()
		{
			_uiTutorialSingleMask.Hide();
		}

	}
}