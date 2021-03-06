using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(AssociatedTwineNodes))]
public class NextTwineNodeInspector : Editor
{

	AssociatedTwineNodes associatedTwineNodes;

	public void Awake()
	{
		this.associatedTwineNodes = (AssociatedTwineNodes)target;
	}

	public override void OnInspectorGUI()
	{
		TwineNode[] nodes = FindObjectsOfType(typeof(TwineNode)) as TwineNode[];

		if (nodes == null || nodes.Length == 0) {
			PrairieGUI.warningLabel ("No Twine Node objects found. Have you imported your story and dragged it into the object hierarchy?");
		} else {

			List<int> selectedTwineNodeIndices = this.GetSelectedIndicesFromObjectList (associatedTwineNodes, nodes);

			if (selectedTwineNodeIndices.Count == 0) {
				// If the twine node has no associated indices set yet, try to auto-select
				//	a node with the same name as this object.
				int suggestedIndex = this.GetSuggestedTwineNodeIndex (nodes);
				selectedTwineNodeIndices = new List<int> (){ suggestedIndex };
			}

			EditorGUI.BeginChangeCheck();
			selectedTwineNodeIndices = PrairieGUI.drawTwineNodeDropdownList ("Associated Twine Nodes", "Twine Node Object",
				nodes, selectedTwineNodeIndices);

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(associatedTwineNodes, "Change Associated Twine Nodes");
				associatedTwineNodes.associatedTwineNodeObjects = this.GetTwineObjectsFromIndices (nodes, selectedTwineNodeIndices);
			}
		}

	}

	/// <summary>
	/// Search the game objects associated with the Twine node (associatedTwineNodes), and gather their indices
	/// relative to the allNodes list.
	/// </summary>
	/// <returns>The selected indices from object list.</returns>
	/// <param name="associatedTwineNodes">The associated twine nodes component being edited.</param>
	/// <param name="allNodes">All Twine nodes in the scene.</param>
	private List<int> GetSelectedIndicesFromObjectList(AssociatedTwineNodes associatedTwineNodes, TwineNode[] allNodes)
	{
		List<int> selectedTwineNodeIndices = new List<int> ();

		for (int i = 0; i < associatedTwineNodes.associatedTwineNodeObjects.Count; i++) {
			GameObject nodeObject = associatedTwineNodes.associatedTwineNodeObjects [i];

			for (int j = 0; j < allNodes.Length; j++) {
				TwineNode node = allNodes [j];
				if (node.gameObject == nodeObject) {
					selectedTwineNodeIndices.Add (j);
				}
			}
		}

		return selectedTwineNodeIndices;
	}

	/// <summary>
	/// Get a list of Twine node GameObjects based on the list of selected indices and the list of nodes.
	/// </summary>
	/// <returns>The twine objects from the allNodes list with the given indices.</returns>
	/// <param name="allNodes">All the Twine nodes in the scene.</param>
	/// <param name="selectedTwineNodeIndices">The selected indices in the editor.</param>
	private List<GameObject> GetTwineObjectsFromIndices(TwineNode[] allNodes, List<int> selectedTwineNodeIndices)
	{
		List<GameObject> twineGameObjects = new List<GameObject> ();

		foreach (int index in selectedTwineNodeIndices) {
			GameObject twineNodeObject = allNodes [index].gameObject;
			twineGameObjects.Add (twineNodeObject);
		}

		return twineGameObjects;
	}

	/// <summary>
	/// Checks if any of the Twine Nodes (from the list of options for the dropdown)
	/// has the same name as the object this component is attached to.
	/// 
	/// Suggests the first item in the list if there is no name match.
	/// </summary>
	/// <returns>The suggested twine node index.</returns>
	/// <param name="nodes">Nodes.</param>
	private int GetSuggestedTwineNodeIndex(TwineNode[] nodes)
	{
		GameObject attachedGameObject = ((AssociatedTwineNodes)target).gameObject;
		for (int i = 0; i < nodes.Length; i++) {
			TwineNode node = nodes [i];
			if (node.name.Equals (attachedGameObject.name)) {
				return i;
			}
		}

		// If no name match found, then choose the first node in the list:
		return 0;
	}

}