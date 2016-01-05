using UnityEngine;
using System.Collections;

public class LocalPlayerScript : MonoBehaviour {

	private GameObject personalCamera;
	[SerializeField]
	private float cameraDistance = 2f;
	[SerializeField]
	private float allPlayerRotationX = 0f;
	[SerializeField]
	private float cameraValueX = 0f;
	[SerializeField]
	private float cameraValueY = -17f;
	[SerializeField]
	private Vector3 centerOfCamera = new Vector3 (0.5f, 1.3f, 0f);
	private Vector3 lastPositionCursor;
	private float sensitivityX = 0.5f;
	private float sensitivityY = 0.75f;

	private float characterSpeed = 3f;

	private string lastAnimationOrder = "Idle01";

	private GameObject visualAvatar;

	// Use this for initialization
	void Start () {

		GlobalData.Start ();

		//Cursor.visible = false;

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
		handleInput ();
	
	}

	void handleInput() {

		Vector2 movement = new Vector3 (0, 0);

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

		if (movement.x == 0 && movement.y == 0) {
			// NO INPUT
			SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Idle01");


		} else {

			movement.Normalize();
			movement = movement*Time.deltaTime*characterSpeed;

			if (Mathf.Abs (movement.x) >= Mathf.Abs(movement.y)) {
				if (movement.x > 0) { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move01_L"); }
				else { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move01_R"); }
			}
			else {
				if (movement.y > 0) { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move01_F"); }
				else { SmartCrossfade(visualAvatar.GetComponent<Animator>(), "Move01_B"); }
			}

			this.gameObject.transform.position = this.gameObject.transform.position + this.gameObject.transform.forward*movement.y -this.gameObject.transform.right*movement.x;

		}

	}

	void SmartCrossfade(Animator animator, string animation) {

		if (lastAnimationOrder != animation && !animator.GetCurrentAnimatorStateInfo(0).IsName(animation)) {
			animator.CrossFade(animation, GlobalData.crossfadeAnimation);
			lastAnimationOrder = animation;
		}

	}

	void handleCameraChanges() {


		cameraValueY += (Input.mousePosition.y - lastPositionCursor.y)*sensitivityY;
		cameraValueY = Mathf.Clamp (cameraValueY, -90f, 90f);

		float compoundValueX = cameraValueX;
		float compoundValueY = cameraValueY;

		personalCamera.transform.localEulerAngles = new Vector3 (-compoundValueY, compoundValueX, personalCamera.transform.localEulerAngles.z);


		personalCamera.transform.localPosition = centerOfCamera;
		personalCamera.transform.position = personalCamera.transform.position +(-personalCamera.transform.forward*cameraDistance);


		this.gameObject.transform.localEulerAngles = new Vector3 (this.gameObject.transform.localEulerAngles.x, this.gameObject.transform.localEulerAngles.y +(Input.mousePosition.x - lastPositionCursor.x)*sensitivityX, this.gameObject.transform.localEulerAngles.z);

		lastPositionCursor = Input.mousePosition;

	}


}
