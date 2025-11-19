using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utilities.UI;

[CustomEditor( typeof(ManualHorizontalLayoutGroup) )]
[CanEditMultipleObjects]
public class ManualHorizontalLayoutGroupEditor : UnityEditor.UI.HorizontalOrVerticalLayoutGroupEditor
{
	private SerializedProperty targetChildrenPositionsX;

	protected override void OnEnable()
	{
		base.OnEnable();
		targetChildrenPositionsX = serializedObject.FindProperty( "targetChildrenPositionsX" );
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		serializedObject.Update();

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField( targetChildrenPositionsX, new GUIContent( "Target Positions X" ) );

		serializedObject.ApplyModifiedProperties();
	}
}