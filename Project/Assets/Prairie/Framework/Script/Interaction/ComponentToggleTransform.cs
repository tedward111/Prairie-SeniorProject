using UnityEngine;
using System.Collections;

[AddComponentMenu("Prairie/Interactions/Transform Component")]
public class ComponentToggleTransform : PromptInteraction
{
	public GameObject[] targets = new GameObject[0];
	public int trX = 0;
	public int trY = 0;
	public int trZ = 0;

	void OnDrawGizmosSelected()
	{
		// sets line specifications
		Gizmos.color = Color.red;
		for (int i = 0; i < targets.Length; i++)
		{
			// Draw red line(s) between the object and the objects whose Behaviours it toggles
            if (targets[i] != null)
            {
                Gizmos.DrawLine(transform.position, targets[i].transform.position);
            }

		}
	}

	// for all attached targets, when the object with this component attached is
	// clicked, switch the enable boolean of each target
	// turns behaviors on/off for light switches
	protected override void PerformAction ()
	{
		for (int i = 0; i < targets.Length; i++)
		{
			targets[i].transform.Translate(trX, trY, trZ);
		}
	}

	override public string defaultPrompt {
		get {
			return "Transform Something";
		}
	}
}
