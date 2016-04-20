using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class LocalPlayerScript : MonoBehaviour {

    public GameScript gameScript;
	public GameObject personalCamera;
	private GameObject firstPersonCamera;
	//private float cameraDistance = 3.2f;
    private float cameraDistance = 0f;
	private float allPlayerRotationX = 0f;
	private float cameraValueX = 0f;
	private float cameraValueY = -17f;
	public static Vector3 centerOfCamera = new Vector3 (0f, 1.4f, 0f);
	//public static Vector3 centerOfCamera = new Vector3 (0.4f, 1.4f, 0f);
    //public static Vector3 centerOfCamera = new Vector3(0f, 1.4f, 0f);
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
	private bool receiveInput2 = true;

	private float notMoving = 0f;

	public float impulseResource = 3f;
	public float impulsing = 0f;

	//public MeleeWeaponTrail sprintTrail;

	public GameObject crosshairHack;
    public GameObject crosshairHackDot;
    public GameObject crosshairHackTriclip;
    public List<GameObject> crosshairHackTriangles = new List<GameObject>();
	public GameObject crosshairHackTimer;
    public GameObject crosshairHackBig;
    public List<GameObject> crosshairHackCharges = new List<GameObject>();
    public List<GameObject> crosshairHackChargesFull = new List<GameObject>();

    private int nextCharge = 1;
    public float chargeResource = 3f;

    public AnimationCurve crosshairHackTriclipSizeCurve;
    public AnimationCurve crosshairHackTriangleCurve;
    public AnimationCurve crosshairHackTriangleSizeCurve;

	public GameObject impulseText;

	//public AnimationCurve attackCameraDistance;

	public bool nextCooldownFree = false;
	private static float attackChargeCooldownMax = 0.5f;
	public float attackChargeCooldown = 0f;

    private Vector3 attackOldPosition;
    private Vector3 attackTargetPosition;

    private float crosshairHackTriclipOldZ = 0f;
    private float crosshairHackTriclipTargetZ = 0f;
    private Vector3 crosshairHackTriclipOriginalScale;
    private Vector3 crosshairHackTriangleOriginalScale;

    private float crosshairHackBigTargetZ = 0f;

	private GameObject firstPersonObjects;
	private GameObject armRight;

    private GameObject alertHacked;

	private float auxFieldOfView = 0f;
	private float maxFieldOfView = 75f;

	private bool lastTimeGrounded = true;
	private static float footStepCooldownMax = 0.35f;
	private float footStepCooldown = footStepCooldownMax/2f;

	public bool gameEnded = false;

	void Awake () {

		GlobalData.Start ();

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		personalCamera = this.gameObject.transform.FindChild ("PersonalCamera").gameObject;
		personalCamera.transform.localPosition = centerOfCamera;
		firstPersonCamera = this.gameObject.transform.FindChild ("PersonalCamera/FirstPersonCamera").gameObject;
		visualAvatar = Instantiate (Resources.Load("Prefabs/BOT") as GameObject);
		visualAvatar.transform.parent = this.gameObject.transform;
		visualAvatar.transform.localPosition = new Vector3 (0, 0, 0);
		visualAvatar.transform.localScale = new Vector3 (0.9f, 0.9f, 0.9f);
		visualAvatar.name = "VisualAvatar";
		materialCarrier = visualAvatar.transform.FindChild ("Mesh").gameObject;
		materialCarrier.layer = LayerMask.NameToLayer ("DontRender");
		//sprintTrail = visualAvatar.transform.FindChild ("Mesh/Trail").gameObject.GetComponent<MeleeWeaponTrail>();

		crosshairHack = Instantiate (Resources.Load("Prefabs/CrosshairHack") as GameObject);
		crosshairHack.transform.SetParent(GameObject.Find ("Canvas").transform);
		crosshairHack.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0, 0);
		crosshairHack.name = "CrosshairHack";
		crosshairHack.GetComponent<RectTransform> ().localScale = new Vector3 (1f, 1f, 1f);
        crosshairHack.SetActive(true);

        crosshairHackDot = crosshairHack.transform.FindChild("Dot").gameObject;
        crosshairHackTriclip = crosshairHack.transform.FindChild("Triclip").gameObject;
		crosshairHackTriclipOriginalScale = crosshairHackTriclip.GetComponent<RectTransform>().localScale;
        GameObject sourceTriangle = crosshairHack.transform.FindChild("Triangle").gameObject;
        int num_triangles = 3;
        for (int i = 0; i < num_triangles; i++)
        {
            GameObject newTriangle = Instantiate(sourceTriangle);
            newTriangle.transform.SetParent(crosshairHack.transform);
            newTriangle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            float aux = (float)i / (float)num_triangles;
            newTriangle.GetComponent<RectTransform>().eulerAngles = new Vector3(0f, 0f, aux * 360f);
            Vector2 upVector2 = new Vector2(newTriangle.GetComponent<RectTransform>().up.x, newTriangle.GetComponent<RectTransform>().up.y);
            newTriangle.GetComponent<RectTransform>().anchoredPosition = upVector2*20f;
            newTriangle.name = "Triangle_" + (i + 1);
			newTriangle.GetComponent<RectTransform> ().localScale = sourceTriangle.GetComponent<RectTransform>().localScale;
            crosshairHackTriangles.Add(newTriangle);
        }
        crosshairHackTriangleOriginalScale = sourceTriangle.GetComponent<RectTransform>().localScale;
        Destroy(sourceTriangle);
		crosshairHackTimer = crosshairHack.transform.FindChild("Timer").gameObject;
		crosshairHackTimer.GetComponent<Image>().material.SetFloat("_Cutoff", 1f);
		crosshairHackTimer.SetActive (false);
        crosshairHackBig = crosshairHack.transform.FindChild("Big").gameObject;
        GameObject sourceCharge = crosshairHack.transform.FindChild("Charge").gameObject;
        int num_charges = 3;
        for (int i = 0; i < num_charges; i++)
        {
            GameObject newCharge = Instantiate(sourceCharge);
            newCharge.transform.SetParent(crosshairHackBig.transform);
            newCharge.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            float aux = (float)i / (float)num_charges;
            newCharge.GetComponent<RectTransform>().eulerAngles = new Vector3(0f, 0f, (aux * 360f) + 60f);
            Vector2 upVector2 = new Vector2(newCharge.GetComponent<RectTransform>().up.x, newCharge.GetComponent<RectTransform>().up.y);
            newCharge.GetComponent<RectTransform>().anchoredPosition = upVector2 * 900f;
            newCharge.name = "Charge_" + (i + 1);
            newCharge.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            crosshairHackCharges.Add(newCharge);
            crosshairHackChargesFull.Add(newCharge.transform.FindChild("Full").gameObject);
        }
        Destroy(sourceCharge);

        alertHacked = Instantiate(Resources.Load("Prefabs/Alert") as GameObject);
        alertHacked.transform.SetParent(GameObject.Find("Canvas").transform);
        alertHacked.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        alertHacked.name = "Alert";
        alertHacked.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        alertHacked.SetActive(true);
        alertHacked.GetComponent<Image>().material.SetFloat("_Cutoff", 1f);

		impulseText = Instantiate (Resources.Load("Prefabs/ImpulseText") as GameObject);
		impulseText.transform.SetParent(GameObject.Find ("Canvas").transform);
		impulseText.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (-64, 35);
		impulseText.name = "ImpulseText";
		impulseText.GetComponent<RectTransform> ().localScale = new Vector3 (1f, 1f, 1f);

		lastPositionCursor = Input.mousePosition;

		GameObject sun = GameObject.Find ("Sun");
		if (sun != null) {
			SunShafts s = personalCamera.GetComponent<SunShafts> ();
			s.sunTransform = sun.transform;
		} else {
			Destroy(personalCamera.GetComponent<SunShafts>());
		}

		firstPersonObjects = this.transform.FindChild ("PersonalCamera/FirstPersonCamera/FirstPersonObjects").gameObject;
		armRight = this.transform.FindChild ("PersonalCamera/FirstPersonCamera/FirstPersonObjects/Arm_2").gameObject;

	}

	void OnApplicationFocus(bool focusStatus) {
		if (focusStatus && !Application.isEditor) {
			StartCoroutine(Hacks.LockCursor(-1f));
		}
	}
	
	// Update is called once per frame
	void Update () {

		handleAttack ();
		handleCameraChanges ();
		//adjustFirstPersonObjects ();
		handleRegularInput();

        interceptMockUp();
        alertMockUp();
	
	}

	void FixedUpdate() {

		handleMovementInput ();

	}

    void alertMockUp()
    {

        float cutoff = alertHacked.GetComponent<Image>().material.GetFloat("_Cutoff");

        if (Input.GetKeyDown(KeyCode.Return) && cutoff == 1f)
        {
            cutoff -= Time.deltaTime;
            alertHacked.GetComponent<Image>().material.SetFloat("_Cutoff", cutoff);
        }

        float min = 0.1f;

        if (cutoff < 1f && cutoff > min) {
            cutoff -= Time.deltaTime*2f;
            if (cutoff <= min) { cutoff = 1f; }
            alertHacked.GetComponent<Image>().material.SetFloat("_Cutoff", cutoff);
        }

    }

    void interceptMockUp()
    {

        if (chargeResource < 3f)
        {
            chargeResource += Time.deltaTime*(1f/2f);
            if (chargeResource >= 3f) { chargeResource = 3f; }
        }

        // TODAS VACIAS
        for (int i = 0; i < crosshairHackCharges.Count; i++)
        {
            crosshairHackChargesFull[i].SetActive(false);
        }

        float auxCharge = chargeResource;
        int auxNext = nextCharge;

        // SE LLENAN LAS QUE TOCAN
        while (auxCharge >= 1f)
        {
            crosshairHackChargesFull[auxNext].SetActive(true);
            auxCharge -= 1f;
            auxNext++;
            if (auxNext >= crosshairHackCharges.Count) { auxNext = 0; }
        }

        // CLICK DERECHO
        if (Input.GetMouseButtonDown(1) && chargeResource >= 1f)
        {
			
            chargeResource -= 1f;
            nextCharge++;
            if (nextCharge >= crosshairHackCharges.Count) { nextCharge = 0; }
            crosshairHackBigTargetZ -= 120f;
            if (crosshairHackBigTargetZ < 0f)
            {
                crosshairHackBigTargetZ += 360f;
            }

			if (gameScript != null) {
				if (Network.isServer) {
					gameScript.serverScript.interceptAttack (gameScript.clientScript.myCode);
				} else {
					gameScript.clientScript.GetComponent<NetworkView>().RPC("interceptAttackRPC", RPCMode.Server, gameScript.clientScript.myCode);
				}
			}

        }

        crosshairHackBig.GetComponent<RectTransform>().eulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(crosshairHackBig.GetComponent<RectTransform>().eulerAngles.z, crosshairHackBigTargetZ, Time.deltaTime*5f));

    }

	void adjustFirstPersonObjects() {

		float difference = 0f;

		if (personalCamera.transform.eulerAngles.x > 180f) {
			// CUANTO MAS LEJOS DE 360, MAS ALTO MIRAS
			difference = -Mathf.Abs(360f-personalCamera.transform.eulerAngles.x)/90f;
		} else {
			// CUANTO MAS LEJOS DE 0, MAS BAJO MIRAS
			difference = Mathf.Abs(0f-personalCamera.transform.eulerAngles.x)/90f;
		}

		float maxAmount = 0.15f;
		firstPersonObjects.transform.localPosition = new Vector3 (0f, difference*maxAmount, 0f);

	}

	void handleAttack() {

		if (attackChargeCooldown > 0f) {

			attackChargeCooldown -= Time.deltaTime;

			if (attackChargeCooldown <= 0f) { 
                attackChargeCooldown = 0f;
            }

            float aux = 1 - (attackChargeCooldown / attackChargeCooldownMax);
            crosshairHackTriclip.GetComponent<RectTransform>().eulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(crosshairHackTriclipOldZ, crosshairHackTriclipTargetZ, aux));

        }
		else if (attackChargeCooldown <= 0f) {

			if (Input.GetMouseButtonDown (0)) {
				
				crosshairHackTriclipTargetZ = crosshairHackTriclipOldZ - 120f;
				if (crosshairHackTriclipTargetZ < 0f) { crosshairHackTriclipTargetZ += 360f; }
				attackChargeCooldown = attackChargeCooldownMax;

				if (gameScript != null) {
					if (Network.isServer) {
						gameScript.serverScript.hackAttack (gameScript.clientScript.myCode);
					} else {
						gameScript.clientScript.GetComponent<NetworkView>().RPC("hackAttackRPC", RPCMode.Server, gameScript.clientScript.myCode);
					}
				}

			}

		}

        crosshairHackTriclip.GetComponent<RectTransform>().localScale = crosshairHackTriclipOriginalScale * crosshairHackTriclipSizeCurve.Evaluate(1f - attackChargeCooldown / attackChargeCooldownMax);

        adjustCrossHairHackTriangles();

	}

    void adjustCrossHairHackTriangles()
    {

        for (int i = 0; i < crosshairHackTriangles.Count; i++)
        {
            Vector2 upVector2 = new Vector2(crosshairHackTriangles[i].GetComponent<RectTransform>().up.x, crosshairHackTriangles[i].GetComponent<RectTransform>().up.y);
            float multiplier = crosshairHackTriangleCurve.Evaluate(1f - attackChargeCooldown / attackChargeCooldownMax);
            Vector2 targetAnchor = upVector2 * 15f * multiplier;
            crosshairHackTriangles[i].GetComponent<RectTransform>().anchoredPosition = targetAnchor;

            crosshairHackTriangles[i].GetComponent<RectTransform>().localScale = crosshairHackTriangleOriginalScale * crosshairHackTriangleSizeCurve.Evaluate(1f - attackChargeCooldown / attackChargeCooldownMax);
        }

    }

	void handleRegularInput() {

		impulseResource = Mathf.Min (3f, impulseResource + Time.deltaTime*(1f/4f));
		impulseText.GetComponent<Text> ().text = impulseResource.ToString ("0.#");

		if (impulsing > 0f) {

			receiveInput2 = false;

			impulsing -= Time.deltaTime;
			if (impulsing <= 0f) { impulsing = 0f; }

			this.transform.GetComponent<Rigidbody> ().velocity = new Vector3 (0f, 0f, 0f);
			this.transform.GetComponent<Rigidbody> ().MovePosition (this.transform.GetComponent<Rigidbody> ().position + personalCamera.transform.forward * Time.deltaTime * 20f);

			auxFieldOfView = Mathf.Min (1f, auxFieldOfView + Time.deltaTime*10f);
			maxFieldOfView = Mathf.Lerp (maxFieldOfView, 80f, Time.deltaTime * 5f);

		} else if (receiveInput) {

			receiveInput2 = true;

			if (Input.GetKeyDown(KeyCode.LeftShift) && impulseResource >= 1f && impulsing == 0f) {
				impulseResource -= 1f;
				impulsing = 0.25f;
			}


			/* HACK ESTO ES PARA VER COMO FUNCIONABA EL EMIT, SI NO SE USA EL SPRINT, QUITARLO
			if (sprintActive > 0f) {
				characterSpeed = turboSpeed;
				if (!sprintTrail.Emit) { sprintTrail.Emit = true; }
			}
			else {
				characterSpeed = baseSpeed;
				if (sprintTrail.Emit) { sprintTrail.Emit = false; }
			}
			*/


			if (Input.GetKeyDown(KeyCode.Space) && IsGrounded()) {

				this.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(this.gameObject.GetComponent<Rigidbody>().velocity.x, 6f, this.gameObject.GetComponent<Rigidbody>().velocity.z);

			}

		}

	}

	void handleMovementInput() {

		Vector2 movement = new Vector3 (0, 0);

		if (receiveInput && receiveInput2) {

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

		bool falling = false;
		if (!IsGrounded ()) {
			falling = true;
			SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Fall");
		}


		if (movement.x == 0 && movement.y == 0) {
			// NO INPUT
			if (!falling) {
				SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Idle");
			}


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

			if (!falling) {
				if (Mathf.Abs (movement.x) >= Mathf.Abs(movement.y)) {
					if (movement.x > 0) { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move_L"); }
					else { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move_R"); }
				}
				else {
					if (movement.y > 0) { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move_F"); }
					else { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move_B"); }
				}
			}

			this.gameObject.GetComponent<Rigidbody>().MovePosition(this.gameObject.GetComponent<Rigidbody>().position + (this.gameObject.transform.forward*movement.y -this.gameObject.transform.right*movement.x)*characterSpeed*Time.fixedDeltaTime);

			if (IsGrounded ()) {
				footStepCooldown -= Time.deltaTime *(characterSpeed/baseSpeed);
				if (footStepCooldown <= 0f) {
					footStepCooldown = footStepCooldownMax*UnityEngine.Random.Range(0.95f, 1.05f);
					FootStep ();
				}
			}

		}

		personalCamera.GetComponent<Camera> ().fieldOfView = Mathf.SmoothStep (70f, maxFieldOfView, auxFieldOfView);
		firstPersonCamera.GetComponent<Camera> ().fieldOfView = personalCamera.GetComponent<Camera> ().fieldOfView;

		if (!lastTimeGrounded && IsGrounded ()) {
			// JUST LANDED
			float amount = Mathf.Min(Mathf.Abs(this.gameObject.GetComponent<Rigidbody>().velocity.y)/Mathf.Abs(Physics.gravity.y)*0.1f, 0.15f);
			personalCamera.transform.localPosition = personalCamera.transform.localPosition - personalCamera.transform.up*amount;
			AudioSource audio = Hacks.GetAudioSource ("Sound/Effects/JumpLand");
			audio.volume = 0.7f * (amount/0.15f);
			audio.pitch = UnityEngine.Random.Range (0.85f, 1.15f);
			audio.Play ();
		}

		lastTimeGrounded = IsGrounded ();

	}

	void FootStep() {

		int amountFootSteps = 4;
		int aux = UnityEngine.Random.Range(1, amountFootSteps +1);
		AudioSource audio = Hacks.GetAudioSource ("Sound/Effects/Footsteps/Footstep_"+aux.ToString("00"));
		audio.volume = 0.3f;
		audio.pitch = UnityEngine.Random.Range (0.8f, 1f);
		audio.Play ();

	}

	bool IsGrounded()  {
		float distToGround = (float)this.gameObject.GetComponent<CapsuleCollider>().bounds.extents.y;
		RaycastHit[] hits;
		hits = Physics.RaycastAll (this.gameObject.transform.position + this.gameObject.GetComponent<CapsuleCollider>().center, -Vector3.up, distToGround + 0.3f);
		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].collider.gameObject.GetComponent<PlayerMarker> () == null) {
				return true;
			}
		}
		return false;
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
			animator.CrossFadeInFixedTime(animation, GlobalData.crossfadeAnimation);
			lastAnimationOrder = animation;
		}

	}

	void handleCameraChanges() {

		if (receiveInput2) {
			cameraValueY += (Input.GetAxis("Mouse Y"))*sensitivityY;
		}

		//cameraValueY = Mathf.Clamp (cameraValueY, -60f, 60f);
		cameraValueY = Mathf.Clamp (cameraValueY, -90f, 90f);

		float compoundValueX = cameraValueX;
		float compoundValueY = cameraValueY;

		personalCamera.transform.localEulerAngles = new Vector3 (-compoundValueY, compoundValueX, personalCamera.transform.localEulerAngles.z);


		personalCamera.transform.localPosition = Vector3.Lerp(personalCamera.transform.localPosition, centerOfCamera, Time.deltaTime*5f);

		RaycastHit hit;
		Vector3 direction = -personalCamera.transform.forward;
		if (Physics.Raycast (personalCamera.transform.position, direction, out hit, cameraDistance)) {
			personalCamera.transform.position = hit.point;
		}
		else {
			personalCamera.transform.position = personalCamera.transform.position +(-personalCamera.transform.forward*cameraDistance);
		}


		float changeX = 0f;
		if (receiveInput2) { changeX = (Input.GetAxis("Mouse X"))*sensitivityX; }

		this.gameObject.transform.localEulerAngles = new Vector3 (this.gameObject.transform.localEulerAngles.x, this.gameObject.transform.localEulerAngles.y +changeX, this.gameObject.transform.localEulerAngles.z);

		lastPositionCursor = Input.mousePosition;

	}



	void LateUpdate() {

		GameObject head = visualAvatar.transform.FindChild ("Armature/Pelvis/Spine/Chest/Neck/Head").gameObject;

		float target = -personalCamera.transform.eulerAngles.x;

		head.transform.eulerAngles = new Vector3 (head.transform.eulerAngles.x, head.transform.eulerAngles.y, target);

	}

}
