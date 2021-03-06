using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ComponentToggleEnable))]
public class ComponentToggleEnableEditor : Editor {

	ComponentToggleEnable componentToggle;

	public void Awake()
	{
		this.componentToggle = (ComponentToggleEnable)target;
	}

	public override void OnInspectorGUI ()
	{
		// Configuration:
		bool _repeatable = EditorGUILayout.Toggle ("Repeatable?", componentToggle.repeatable);
		GameObject[] _targets = PrairieGUI.drawObjectList<GameObject> ("Objects To Enable/Disable:", componentToggle.targets);

		// Save:
		if (GUI.changed) {
			Undo.RecordObject(componentToggle, "Modify Component Rotation");
			componentToggle.repeatable = _repeatable;
			componentToggle.targets = _targets;
		}

		// Warnings (after properties have been updated):
		this.DrawWarnings();
	}

	public void DrawWarnings()
	{
		foreach (GameObject obj in componentToggle.targets)
		{
			if (obj == null)
			{
				PrairieGUI.warningLabel ("You have one or more empty slots in your list of toggles.  Please fill these slots or remove them.");
				break;
			}
		}
	}
}
