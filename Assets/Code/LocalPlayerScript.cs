using UnityEngine;
using System.Collections;

public class LocalPlayerScript : MonoBehaviour {

	private GameObject personalCamera;
	private float cameraDistance = 2f;
	private float allPlayerRotationX = 0f;
	private float cameraValueX = 0f;
	private float cameraValueY = -17f;
	private Vector3 centerOfCamera = new Vector3 (0.3f, 1.3f, 0f);
	private Vector3 lastPositionCursor;
	private float sensitivityX = 10f;
	private float sensitivityY = 5f;

	private static float baseSpeed = 3f;
	private float turboSpeed = baseSpeed*(1.7f); // 70% ES LO QUE AUMENTA LA VELOCIDAD EL SPRINT DEL PICARO EN EL WOW
	private float characterSpeed = baseSpeed;

	public string lastAnimationOrder = "Idle01";

	public GameObject visualAvatar;

	public bool receiveInput = true;

	private float notMoving = 0f;

	// Use this for initialization
	void Start () {

		GlobalData.Start ();

		personalCamera = this.gameObject.transform.FindChild ("PersonalCamera").gameObject;
		visualAvatar = Instantiate (Resources.Load("Prefabs/ToonSoldier") as GameObject);
		visualAvatar.transform.parent = this.gameObject.transform;
		visualAvatar.transform.localPosition = new Vector3 (0, 0, 0);
		visualAvatar.name = "VisualAvatar";

		GameObject crossHair = Instantiate (Resources.Load("Prefabs/CanvasCrossHair") as GameObject);
		crossHair.transform.SetParent(GameObject.Find ("Canvas").transform);
		crossHair.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0, 0);
		crossHair.name = "CanvasCrossHair";

		lastPositionCursor = Input.mousePosition;
	
	}
	
	// Update is called once per frame
	void Update () {

		handleCameraChanges ();
		handleRegularInput();
	
	}

	void FixedUpdate() {

		handleMovementInput ();

	}

	void handleRegularInput() {

		if (receiveInput) {

			if (Input.GetKey(KeyCode.LeftShift)) {
				characterSpeed = turboSpeed;
			}
			else {
				characterSpeed = baseSpeed;
			}
			
			if (Input.GetKeyDown(KeyCode.Space) && IsGrounded()) {
				this.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(this.gameObject.GetComponent<Rigidbody>().velocity.x, 6f, this.gameObject.GetComponent<Rigidbody>().velocity.z);
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
			SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Idle01");

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
				if (movement.x > 0) { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move01_L"); }
				else { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move01_R"); }
			}
			else {
				if (movement.y > 0) { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move01_F"); }
				else { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move01_B"); }
			}

			this.gameObject.GetComponent<Rigidbody>().MovePosition(this.gameObject.GetComponent<Rigidbody>().position + (this.gameObject.transform.forward*movement.y -this.gameObject.transform.right*movement.x)*characterSpeed*Time.fixedDeltaTime);
			//this.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0f, this.gameObject.GetComponent<Rigidbody>().velocity.y, 0f) + (this.gameObject.transform.forward*movement.y -this.gameObject.transform.right*movement.x)*characterSpeed;

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


}
