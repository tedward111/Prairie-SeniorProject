﻿using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

// Required to use with First Person Controller Component
[AddComponentMenu("Prairie/Player/First Person Interactor")]
public class FirstPersonInteractor : MonoBehaviour
{
	// Raycast-related: Raycasting is the process of an invisible ray from a point, 
	// specified direction to detect whether colliders lay in the path of the ray.

	// Default interaction range: how far away can the character trigger object interactions
	public float interactionRange = 3;
	private Camera viewpoint;

	// Selection-related

	// The object the player is currently looking at
	private GameObject highlightedObject;
	// List of potential annotation objects the player could reach
	public List<Annotation> areaAnnotationsInRange = new List<Annotation>();
	public bool annotationsEnabled = true;
	public bool worldFrozen = false;
	public bool startMenuOpen = false;
	public bool journalOpen = false;
	public bool inventoryAccess = false;
    public int dialogue = 0;
	public bool annotationOpen = false;

	// Control-related

	// By default, Unity use the same logic for each variable; 
	// private are NonSerialized and HideInInspector; public are SerializeField (and shown in inspector).
	//	- HideInInspector make sure a variable is not displayed.
	//	- NonSerialized make sure a variable state is reset to default on game state change.
	//	- SerializeField make sure a variable value instance has its own default value.
	[HideInInspector]

	private bool drawsGUI = true;

	/// --- Game Loop ---

	void Start ()
	{
		// set start point to main camera
		viewpoint = Camera.main;
	}

	void Update ()
	{
		// update our highlighted object
		this.highlightedObject = this.GetHighlightedObject();


		// else, hide overlay GUI in certain contexts (such as while slideshow is playing, etc.)
		if (this.drawsGUI)
		{
			// draw overlay UI on highlighted object
			if (this.annotationsEnabled) {
				drawSummaryAnnotation ();
			}
			drawPrompt ();
		}

		// process input
		if (Input.GetMouseButtonDown (0))
		{
			// enable left click for regular interaction
			this.AttemptInteract ();
		}
		if (Input.GetMouseButtonDown (1))
		{
			// enable right click for annotation
			this.AttemptReadAnnotation ();

		}

		if (Input.GetKeyDown (KeyCode.I)) {
			// enable Key I for interating with inventory UI
			this.AttemptInteractInventory ();
		}

		if (Input.GetKeyDown (KeyCode.F)) {
			// enable Key F for add to inventory
			this.AttemptAddToInventory ();
		}

		if (Input.GetKeyDown (KeyCode.E)) {
			// enable Key E for carrying the object
			this.AttemptCarry ();
		}

		// Prompt area annotiaion bar if annotation is enabled and there exist annotated objects within the radius 
		if (areaAnnotationsInRange.Count != 0 && this.annotationsEnabled)
		{
			for (int a = 0; a < areaAnnotationsInRange.Count; a++)
			{
				if (Input.GetKeyDown ((a+1).ToString()))
				{
					// Player interact with the selected annotation object
					areaAnnotationsInRange[a].Interact (this.gameObject);
				}
			}
		}
	}

	private void drawSummaryAnnotation() {
		SummaryAnnotationGui sa = this.GetComponentInChildren<SummaryAnnotationGui> ();

		if (this.highlightedObject != null) {
			// draw potential stub on highlighted annotation object
			Annotation annotation = this.highlightedObject.GetComponent<Annotation> ();
			if (annotation != null && annotation.enabled && this.annotationsEnabled && 
				annotation.annotationType == (int)AnnotationTypes.SUMMARY) {
				// draw UI if it is inactive or if the content changes
				if (!sa.isUIActive () || !annotation.summary.Equals(sa.GetCurrentText())) {
					sa.ActivateGui (annotation);
				}
			} else if (sa.isUIActive ()) {
				sa.DeactivateGui ();
			}
		} else {
			if (sa.isUIActive ()) {
				sa.DeactivateGui ();
			}
		}
	}

	private void drawPrompt() {
		PromptGui pg = this.GetComponentInChildren<PromptGui> ();

		if (this.highlightedObject != null) {
			// draw prompt on highlighted object
			Prompt prompt = this.highlightedObject.GetComponent<Prompt> ();
			if (prompt != null) {
				// draw UI if it is inactive or if the content changes
				if (!pg.isUIActive () || !prompt.GetPrompt().Equals(pg.GetCurrentText())) {
					pg.ActivateGui (prompt);
				}
			} else if (pg.isUIActive()) {
				pg.DeactivateGui ();
			}
		} else {
			if (pg.isUIActive ()) {
				pg.DeactivateGui ();
			}
		}
	}

	/// --- GUI ---

	void OnGUI()
	{
		if (!this.drawsGUI)
		{
			// hide all GUI in certain contexts (such as while slideshow is playing, etc.)
			return;
		}

		if (this.highlightedObject != null)
		{
			Prompt prompt = this.highlightedObject.GetComponent<Prompt> ();
			if (prompt == null || prompt.GetPrompt().Trim() == "")
			{
				// draw crosshair when the prompt is left blank
				this.drawCrosshair();
			}
		}
		else
		{
			// draw a crosshair when we have no highlighted object
			this.drawCrosshair();
		}
	}

	private void drawCrosshair()
	{
		Rect frame = new Rect (Screen.width / 2, Screen.height / 2, 10, 10);
		GUI.Box (frame, "");
	}

	//Set the world active or not
	public void setWorldActive(string pauseType){
		if (pauseType == "Pause Menu") {
			if (startMenuOpen) {
				startMenuOpen = false;
			} else {
				startMenuOpen = true;
			}
		}
		if (pauseType == "Journal") {
			if (journalOpen) {
				journalOpen = false;
			} else {
				journalOpen = true;
			}
		}
		if (pauseType == "Inventory") {
			if (inventoryAccess) {
				inventoryAccess = false;
			} else {
				inventoryAccess = true;
			}
		}
		if (pauseType == "Annotation") {
			if (annotationOpen) {
				annotationOpen = false;
			} else {
				annotationOpen = true;
			}
		}

        if (pauseType == "DialogueOpen")
        {
            dialogue += 1;
        }
        if (pauseType == "DialogueClose")
        {
            dialogue -= 1;
        }

		if (!startMenuOpen && !journalOpen && !inventoryAccess && dialogue == 0 && !annotationOpen) {
			Time.timeScale = 1;
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		} else {
			Time.timeScale = 0;
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}

	}

	/// --- Trigger Areas ---

	// Public method override event driven functions
	// Handles annotation trigger w/ Colliders
	public void OnTriggerEnter(Collider other)
	{
		GameObject inside = other.gameObject;
		// automatically trigger area we're now inside of's interactions
		foreach (Interaction i in inside.GetComponents<Interaction> ())
		{
			if (!(i is Annotation))
			{
				i.Interact (this.gameObject);
			}
		}
	}

	/// --- Handling Interaction ---

	private void AttemptInteract ()
	{
		if (highlightedObject == null) {
			return;
		}
		
		foreach (Interaction i in this.highlightedObject.GetComponents<Interaction> ())
		{
			if (i is Annotation || i is Inventory || i is InventoryInteraction || i is Carry)
			{
				// special cases, handled by 'AttemptReadAnnotation', 
				// 'AttemptInteractInventory', AttemptAddToInventory', 'AttemptCarry' 
				continue;
			}

			if (i.enabled)
			{ 
				i.Interact (this.gameObject);		
			} 
		}

        if (this.highlightedObject.GetComponent<Prompt>() != null)
        {
            this.highlightedObject.GetComponent<Prompt>().CyclePrompt();
        }
    }


	private void AttemptReadAnnotation ()
	{
		if (!this.annotationsEnabled || highlightedObject == null) {
			return;
		}

		foreach (Interaction i in this.highlightedObject.GetComponents<Interaction> ()) 
		{
			if (i is Annotation) {
				Annotation a = (Annotation)i;

				// Only allow right click to open full annotation if it is a summary annotation.
				if (a.annotationType == (int)AnnotationTypes.SUMMARY) {
					i.Interact (this.gameObject);
				}
			}
		}
	}

	private void AttemptInteractInventory ()
	{
		Debug.Log ("Attempt to interact with inventory, should see cursor");
		// Hacky way of getting the inventory script in canvass. 
		// Only work if only one inventory is attached to one player.
		this.GetComponentsInChildren<Inventory> () [0].Interact (this.gameObject);
	}

	private void AttemptAddToInventory ()
	{
		if (highlightedObject == null) {
			return;
		}

		foreach (Interaction i in this.highlightedObject.GetComponents<Interaction> ()) 
		{
			if (i is InventoryInteraction)
			{
				i.Interact (this.gameObject);
			}
		}
	}

	private void AttemptCarry ()
	{
		if (highlightedObject == null) {
			return;
		}

		foreach (Interaction i in this.highlightedObject.GetComponents<Interaction> ()) 
		{
			if (i is Carry)
			{
				i.Interact (this.gameObject);
			}
		}
	}

	public GameObject GetHighlightedObject()
	{
		// perform a raycast from the main camera to an object in front of it
		// the object must have a collider to be hit, and an `Interaction` to be added
		// to this interactor's interaction list

		Vector3 origin = viewpoint.transform.position;
		Vector3 fwd = viewpoint.transform.TransformDirection (Vector3.forward);

		RaycastHit hit;		// we pass this into the raycast function and it populates it with a result

		if (Physics.Raycast (origin, fwd, out hit, interactionRange))
		{
			if (hit.collider.isTrigger)
			{
				// ignore non-physical colliders, such as trigger areas
				return null;
			}

			return hit.transform.gameObject;
		}
		else
		{
			return null;
		}
	}

	// --- Changing Player Abilities ---

	public void SetDrawsGUI(bool shouldDraw)
	{
		this.drawsGUI = shouldDraw;
	}

	public void SetCanMove(bool canMove)
	{
		var playerCompTypeA = this.gameObject.GetComponent<FirstPersonController> ();
		var playerCompTypeB = this.gameObject.GetComponent<RigidbodyFirstPersonController> ();

		if (playerCompTypeA != null)
		{
			playerCompTypeA.enabled = canMove;
		}
		if (playerCompTypeB != null)
		{
			playerCompTypeB.enabled = canMove;
		}
	}

	public void SetUseCursor(bool useCursor)
	{
		if (useCursor) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = true;
		}
	}

	// --- Gizmos --- (Used in editor mode) 
	// Gizmos are used to give visual debugging or setup aids in the scene view. 
	// All gizmo drawing has to be done in either OnDrawGizmos or OnDrawGizmosSelected functions of the script.
	// OnDrawGizmos is called every frame. All gizmos rendered within OnDrawGizmos are pickable. 
	// OnDrawGizmosSelected is called only if the object the script is attached to is selected.

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.gray;

		Vector3 origin = this.transform.position;
		Vector3 forward = this.transform.TransformDirection (Vector3.forward) * this.interactionRange;

		Gizmos.DrawRay(origin, forward);
	}

}
