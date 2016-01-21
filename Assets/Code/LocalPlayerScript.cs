using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class LocalPlayerScript : MonoBehaviour {

    public GameScript gameScript;
	private GameObject personalCamera;
	//private float cameraDistance = 3.2f;
    private float cameraDistance = 0f;
	private float allPlayerRotationX = 0f;
	private float cameraValueX = 0f;
	private float cameraValueY = -17f;
	//private Vector3 centerOfCamera = new Vector3 (0.4f, 1.4f, 0f);
    public static Vector3 centerOfCamera = new Vector3(0f, 1.4f, 0f);
	private Vector3 lastPositionCursor;
	private float sensitivityX = 10f;
	private float sensitivityY = 5f;
    public static float stabbingDistance = 2.5f;

	private static float baseSpeed = 5f;
	private float turboSpeed = baseSpeed*(1.5f); // 70% ES LO QUE AUMENTA LA VELOCIDAD EL SPRINT DEL PICARO EN EL WOW
	private float characterSpeed = baseSpeed;

	public string lastAnimationOrder = "Idle01";

	public GameObject visualAvatar;
	public GameObject materialCarrier;

	public bool receiveInput = true;

	private float notMoving = 0f;

	private float timeStealth = 0f;
	public string currentMode = "regular";

	public Skills skills;

	public float sprintCooldownCurrent = 0f;
	public float sprintCooldownMax = 5f;
	public float sprintActive = 0f;

	public MeleeWeaponTrail sprintTrail;

	private GameObject crossHair;
	public GameObject crossHairTargeted;

    public AudioSource audioSource1;

    private List<AudioClip> stabbingClips = new List<AudioClip>();

	// Use this for initialization
	void Start () {

		GlobalData.Start ();

		personalCamera = this.gameObject.transform.FindChild ("PersonalCamera").gameObject;
		visualAvatar = Instantiate (Resources.Load("Prefabs/Subject") as GameObject);
		visualAvatar.transform.parent = this.gameObject.transform;
		visualAvatar.transform.localPosition = new Vector3 (0, 0, 0);
		visualAvatar.transform.localScale = new Vector3 (0.9f, 0.9f, 0.9f);
		visualAvatar.name = "VisualAvatar";
		materialCarrier = visualAvatar.transform.FindChild ("Mesh").gameObject;
		sprintTrail = visualAvatar.transform.FindChild ("Mesh/Trail").gameObject.GetComponent<MeleeWeaponTrail>();

		crossHair = Instantiate (Resources.Load("Prefabs/CanvasCrossHair") as GameObject);
		crossHair.transform.SetParent(GameObject.Find ("Canvas").transform);
		crossHair.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0, 0);
		crossHair.name = "CanvasCrossHair";

		skills = new Skills (this);

		lastPositionCursor = Input.mousePosition;

        audioSource1 = this.gameObject.AddComponent<AudioSource>();

        stabbingClips.Add(Resources.Load("Sound/Effects/Knife_Stab_001") as AudioClip);
        stabbingClips.Add(Resources.Load("Sound/Effects/Knife_Stab_002") as AudioClip);
        stabbingClips.Add(Resources.Load("Sound/Effects/Knife_Stab_003") as AudioClip);

		GameObject sun = GameObject.Find ("Sun");
		if (sun != null) {
			SunShafts s = personalCamera.GetComponent<SunShafts> ();
			s.sunTransform = sun.transform;
		} else {
			Destroy(personalCamera.GetComponent<SunShafts>());
		}
	
	}
	
	// Update is called once per frame
	void Update () {

		if (sprintActive > 0f) {
			sprintActive -= Time.deltaTime;
			if (sprintActive < 0f) {
				sprintActive = 0f;
			}
		} else if (sprintCooldownCurrent > 0f) {
			sprintCooldownCurrent -= Time.deltaTime;
			if (sprintCooldownCurrent < 0f) {
				sprintCooldownCurrent = 0f;
			}
		}

		handleCameraChanges ();
		handleRegularInput();
		skills.Update ();
	
	}

	void FixedUpdate() {

		handleMovementInput ();
		handleStealth ();
		checkIfLookingAtPlayer ();

	}

	void checkIfLookingAtPlayer() {

		crossHair.GetComponent<Image>().color = new Color(1f, 1f, 1f);
		crossHairTargeted = null;

		RaycastHit[] hits;
		hits = Physics.RaycastAll (personalCamera.transform.position, personalCamera.transform.forward, stabbingDistance);
		Array.Sort (hits, delegate(RaycastHit r1, RaycastHit r2) { return r1.distance.CompareTo(r2.distance); });

		for (int i = 0; i < hits.Length; i++) {
			if (hits[i].collider.gameObject != this.visualAvatar && hits[i].collider.gameObject != this.gameObject) {

				if (hits[i].collider.gameObject.GetComponent<PlayerMarker>() != null) {
					crossHair.GetComponent<Image>().color = new Color(1f, 0f, 0f);
					crossHairTargeted = hits[i].collider.gameObject;
					break;
				}
				else {
					break;
				}

			}
		}

	}

	void handleRegularInput() {

		if (receiveInput) {

			if (Input.GetKeyDown(KeyCode.LeftShift) && sprintCooldownCurrent == 0f) {
				sprintActive = 2.5f;
				sprintCooldownCurrent = sprintCooldownMax;
			}

			if (sprintActive > 0f) {
				characterSpeed = turboSpeed;
				if (!sprintTrail.Emit) { sprintTrail.Emit = true; }
			}
			else {
				characterSpeed = baseSpeed;
				if (sprintTrail.Emit) { sprintTrail.Emit = false; }
			}

			
			if (Input.GetKeyDown(KeyCode.Space) && IsGrounded()) {

				this.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(this.gameObject.GetComponent<Rigidbody>().velocity.x, 6f, this.gameObject.GetComponent<Rigidbody>().velocity.z);

				timeStealth = 0f;
				currentMode = "regular";

			}

            if (Input.GetMouseButtonDown(0))
            {
                if (!audioSource1.isPlaying)
                {
                    audioSource1.clip = stabbingClips[UnityEngine.Random.Range(0, stabbingClips.Count)];
                    audioSource1.Play();
                }
                if (gameScript != null)
                {
                    gameScript.requestAttack(personalCamera.transform.forward);
                }
            }

			if (currentMode == "regular") {
				timeStealth += Time.deltaTime;
				if (timeStealth > 30f) {
					currentMode = "stealth";
				}
			}

		}

	}

	void handleMovementInput() {

		Vector2 movement = new Vector3 (0, 0);

		if (receiveInput) {

			if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow)) {
				movement = new Vector3(movement.x, 1f);
			} 
			else if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow)) {
				movement = new Vector3(movement.x, -1f);
			}
			if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {
				movement = new Vector3(movement.x +1f, movement.y);
			}
			if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {
				movement = new Vector3(movement.x -1f, movement.y);
			}
		}


		if (movement.x == 0 && movement.y == 0) {
			// NO INPUT
			SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Idle");

			float aux = this.gameObject.GetComponent<Rigidbody>().velocity.y;

			if (notMoving <= 0f) {

				notMoving += Time.deltaTime;
				// ESTO ES PARA QUE AL SUBIR COLINAS Y PARAR NO HAGA UN BOUNCE HACIA ARRIBA RARO, ASI LE PERMITE REAJUSTARSE PERO DE FORMA SUAVE
				if (aux > 0.3f && IsGrounded()) { aux = 0.3f; }

			}

			this.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0f, aux, 0f);

		} else {

			notMoving = 0f;

			movement.Normalize();

			if (Mathf.Abs (movement.x) >= Mathf.Abs(movement.y)) {
				if (movement.x > 0) { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move_L"); }
				else { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move_R"); }
			}
			else {
				if (movement.y > 0) { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move_F"); }
				else { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move_B"); }
			}

			this.gameObject.GetComponent<Rigidbody>().MovePosition(this.gameObject.GetComponent<Rigidbody>().position + (this.gameObject.transform.forward*movement.y -this.gameObject.transform.right*movement.x)*characterSpeed*Time.fixedDeltaTime);

		}

	}

	void handleStealth() {


		if (currentMode == "regular") {

			Color c = Color.Lerp(materialCarrier.GetComponent<SkinnedMeshRenderer> ().material.GetColor("_Color"), new Color(1f, 1f, 1f, 1f), Time.fixedDeltaTime*5f);
			materialCarrier.GetComponent<SkinnedMeshRenderer> ().material.SetColor("_Color", c);
			//Color c2 = Color.Lerp(materialCarrier.GetComponent<SkinnedMeshRenderer> ().material.GetColor("_OutlineColor"), new Color(1f, 1f, 1f, 0f), Time.fixedDeltaTime*5f);
			//materialCarrier.GetComponent<SkinnedMeshRenderer> ().material.SetColor("_OutlineColor", c2);

		} else if (currentMode == "stealth") {

			Color c = Color.Lerp(materialCarrier.GetComponent<SkinnedMeshRenderer> ().material.GetColor("_Color"), new Color(1f, 1f, 1f, 0.4f), Time.fixedDeltaTime*5f);
			materialCarrier.GetComponent<SkinnedMeshRenderer> ().material.SetColor("_Color", c);
			//Color c2 = Color.Lerp(materialCarrier.GetComponent<SkinnedMeshRenderer> ().material.GetColor("_OutlineColor"), new Color(1f, 1f, 1f, 0.65f), Time.fixedDeltaTime*5f);
			//materialCarrier.GetComponent<SkinnedMeshRenderer> ().material.SetColor("_OutlineColor", c2);

		}

	}

	bool IsGrounded()  {
		float distToGround = (float)this.gameObject.GetComponent<CapsuleCollider>().bounds.extents.y;
		return Physics.Raycast(this.gameObject.transform.position + this.gameObject.GetComponent<CapsuleCollider>().center, -Vector3.up, distToGround + 0.3f);
	}

	Vector3 IsGroundedVector3() {
		float distToGround = (float)this.gameObject.GetComponent<CapsuleCollider>().bounds.extents.y;
		RaycastHit hit;
		if (Physics.Raycast(this.gameObject.transform.position + this.gameObject.GetComponent<CapsuleCollider>().center, -Vector3.up, out hit, distToGround + 5f)) {
			return hit.normal;
		}
		return Vector3.up;
	}

	void SmartCrossfade(Animator animator, string animation) {

		if (lastAnimationOrder != animation && !animator.GetCurrentAnimatorStateInfo(0).IsName(animation)) {
			animator.CrossFade(animation, GlobalData.crossfadeAnimation);
			lastAnimationOrder = animation;
		}

	}

	void handleCameraChanges() {

		if (receiveInput) {
			cameraValueY += (Input.GetAxis("Mouse Y"))*sensitivityY;
		}
		cameraValueY = Mathf.Clamp (cameraValueY, -60f, 60f);

		float compoundValueX = cameraValueX;
		float compoundValueY = cameraValueY;

		personalCamera.transform.localEulerAngles = new Vector3 (-compoundValueY, compoundValueX, personalCamera.transform.localEulerAngles.z);


		personalCamera.transform.localPosition = centerOfCamera;

		RaycastHit hit;
		Vector3 direction = -personalCamera.transform.forward;
		if (Physics.Raycast (personalCamera.transform.position, direction, out hit, cameraDistance)) {
			personalCamera.transform.position = hit.point;
		}
		else {
			personalCamera.transform.position = personalCamera.transform.position +(-personalCamera.transform.forward*cameraDistance);
		}


		float changeX = 0f;
		if (receiveInput) { changeX = (Input.GetAxis("Mouse X"))*sensitivityX; }

		this.gameObject.transform.localEulerAngles = new Vector3 (this.gameObject.transform.localEulerAngles.x, this.gameObject.transform.localEulerAngles.y +changeX, this.gameObject.transform.localEulerAngles.z);

		lastPositionCursor = Input.mousePosition;

	}

	public class Skills {

		public LocalPlayerScript parent;

		public GameObject root;

		public GameObject sprintBase;
		public GameObject sprintCooldown;
		public Material sprintCooldownMaterial;

		private float minCutoff = 0.09f;
		private float maxCutoff = 0.90f;

		public Skills(LocalPlayerScript auxParent) {

			parent = auxParent;

			root = Instantiate (Resources.Load("Prefabs/CanvasSkills") as GameObject);
			root.GetComponent<RectTransform>().SetParent(GameObject.Find("Canvas").transform);
			root.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
			root.transform.localScale = new Vector3(1f, 1f, 1f);


			sprintBase = root.transform.FindChild("SprintBase").gameObject;
			sprintCooldown = root.transform.FindChild("SprintCooldown").gameObject;
			sprintCooldownMaterial = sprintCooldown.GetComponent<Image>().material;

		}

		public void Update() {

			float currentSprintCutoff = minCutoff + (maxCutoff - minCutoff)*(1f - parent.sprintCooldownCurrent/parent.sprintCooldownMax);

			sprintCooldownMaterial.SetFloat("_Cutoff", currentSprintCutoff);

		}

	}


}
