using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utilities.UI;

[CustomEditor( typeof(ManualHorizontalLayoutGroup) )]
[CanEditMultipleObjects]
public class ManualHorizontalLayoutGroupEditor : UnityEditor.UI.HorizontalOrVerticalLayoutGroupEditor
{
	/*SerializedProperty activeElement;
	SerializedProperty spacesChangeDuration;
	SerializedProperty spacingEasing;
	SerializedProperty debugActiveElement;
	
	private SerializedProperty startChildrenPositionsX;
	private SerializedProperty animatedChildrenPositionsX;*/
	
	private SerializedProperty targetChildrenPositionsX;

	protected override void OnEnable()
	{
		base.OnEnable();
		/*activeElement = serializedObject.FindProperty( "activeElement" );
		spacesChangeDuration = serializedObject.FindProperty( "spacesChangeDuration" );
		spacingEasing = serializedObject.FindProperty( "spacingEasing" );
		debugActiveElement = serializedObject.FindProperty( "debugActiveElement" );
		
		startChildrenPositionsX = serializedObject.FindProperty( "startChildrenPositionsX" );
		animatedChildrenPositionsX = serializedObject.FindProperty( "animatedChildrenPositionsX" );*/
		targetChildrenPositionsX = serializedObject.FindProperty( "targetChildrenPositionsX" );
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		serializedObject.Update();

		EditorGUILayout.Space();
		/*EditorGUILayout.LabelField( "Adaptive Settings", EditorStyles.boldLabel );
		EditorGUILayout.PropertyField( activeElement, new GUIContent( "Active Element" ) );
		EditorGUILayout.PropertyField( spacesChangeDuration, new GUIContent( "Spaces Change Duration" ) );
		EditorGUILayout.PropertyField( spacingEasing, new GUIContent( "Easing" ) );
		EditorGUILayout.PropertyField( debugActiveElement, new GUIContent( "Debug Active Element" ) );
		
		EditorGUILayout.PropertyField( startChildrenPositionsX, new GUIContent( "Start Positions X" ) );
		EditorGUILayout.PropertyField( animatedChildrenPositionsX, new GUIContent( "Anim Positions X" ) );*/
		EditorGUILayout.PropertyField( targetChildrenPositionsX, new GUIContent( "Target Positions X" ) );

		serializedObject.ApplyModifiedProperties();
	}
}