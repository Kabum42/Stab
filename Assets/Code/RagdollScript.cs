using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RagdollScript : MonoBehaviour {

	public List<Articulation> articulations = new List<Articulation>();
	private Animator animator;
	private bool transformsInitialized = false;

	// Use this for initialization
	void Start () {

		TraverseHierarchy (this.gameObject.transform);
		TraverseHierarchy2 (this.gameObject.transform);

		Disable ();
	
	}

	void Update() {

		if (Input.GetKeyDown(KeyCode.O)) {
			Disable();
		}
		else if (Input.GetKeyDown(KeyCode.P)) {
			Enable ();
		}

	}

	void LateUpdate() {

		if (animator.enabled && transformsInitialized) {
			for (int i = 0; i < articulations.Count; i++) {
				articulations [i].gameobject.transform.localPosition = Hacks.LerpVector3 (articulations [i].lastPosition, articulations [i].gameobject.transform.localPosition, Time.deltaTime * 5f);
				articulations [i].gameobject.transform.localEulerAngles = Hacks.LerpVector3Angle (articulations [i].lastEulerAngles, articulations [i].gameobject.transform.localEulerAngles, Time.deltaTime * 5f);
			}
		} else if (!animator.enabled && !transformsInitialized) {
			for (int i = 0; i < articulations.Count; i++) {
				articulations [i].lastPosition = articulations [i].gameobject.transform.localPosition;
				articulations [i].lastEulerAngles = articulations [i].gameobject.transform.localEulerAngles;
			}
			transformsInitialized = true;
		}

		for (int i = 0; i < articulations.Count; i++) {
			articulations [i].lastPosition = articulations [i].gameobject.transform.localPosition;
			articulations [i].lastEulerAngles = articulations [i].gameobject.transform.localEulerAngles;
		}


	}

	void TraverseHierarchy(Transform root) {
		
		articulations.Add (new Articulation (root.gameObject));

		foreach(Transform child in root)
		{
			TraverseHierarchy(child);
		}

	}

	void TraverseHierarchy2(Transform root) {

		if (root.gameObject.GetComponent<Animator> () != null) {
			animator = root.gameObject.GetComponent<Animator> ();
		} else {
			TraverseHierarchy2 (root.parent);
		}

	}

	public void Enable() {

		animator.enabled = false;

		for (int i = 0; i < articulations.Count; i++) {
			if (articulations [i].rigidbody != null) {
				articulations [i].rigidbody.isKinematic = false;
			}
			if (articulations [i].collider != null) {
				articulations [i].collider.isTrigger = false;
			}
		}

	}

	public void Disable() {

		for (int i = 0; i < articulations.Count; i++) {
			if (articulations [i].rigidbody != null) {
				articulations [i].rigidbody.isKinematic = true;
			}
			if (articulations [i].collider != null) {
				articulations [i].collider.isTrigger = true;
			}
		}

		animator.enabled = true;

	}

	public bool IsArticulation (GameObject g) {

		for (int i = 0; i < articulations.Count; i++) {
			if (articulations [i].gameobject == g) {
				return true;
			}
		}

		return false;

	}
	

}

public class Articulation {

	public GameObject gameobject;
	public Rigidbody rigidbody;
	public Collider collider;
	public Vector3 lastPosition;
	public Vector3 lastEulerAngles;

	public Articulation (GameObject source) {

		gameobject = source;

		if (gameobject.GetComponent<Rigidbody> () != null) {
			rigidbody = gameobject.GetComponent<Rigidbody> ();
		}

		if (gameobject.GetComponent<Collider> () != null) {
			collider = gameobject.GetComponent<Collider> ();
		}

	}

}
