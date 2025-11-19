using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
	public static PlayerController main;

	[Header("Movement Settings")]
	public float speed = 10.0f;
	public float sensitivity = 30.0f;
	public float collisionCheckDistance = 0.5f;
	public GameObject cam;
	public bool stuck = false;
	float moveFB, moveLR;
	float rotX, rotY;
	float gravity = -9.8f;
	CharacterController character;

	[Header("Object Pickup Settings")]
	public LayerMask ignorLayerMask;
	public float pickupDistance = 3f;
	public float holdDistance = 1.5f;
	public float throwForce = 10f;
	GameObject heldObject = null;
	Rigidbody heldObjectRb;	
	bool ignoreFirstFrame = false;
	[HideInInspector] public bool isHolding = false;

	void Awake()
	{
		main = this;
	}

	void Start()
	{
		character = GetComponent<CharacterController>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			Menu.main.GetComponent<Menu>().OpenMenu();
		}

		if (Input.GetMouseButtonDown(0) && !GameObject.FindGameObjectWithTag("Popup"))
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		moveFB = Input.GetAxis("Horizontal") * speed;
		moveLR = Input.GetAxis("Vertical") * speed;

		rotX = Input.GetAxis("Mouse X") * sensitivity;
		rotY = Input.GetAxis("Mouse Y") * sensitivity;

		Vector3 movement = new Vector3(moveFB, gravity, moveLR);

		movement = transform.rotation * movement;

		if (!stuck)
		{
			HandleMovement();
		}
		
		HandleCameraRotation();
		HandleObjectPickupAndThrow();
	}

	void HandleMovement()
	{
		moveFB = Input.GetAxis("Horizontal") * speed;
		moveLR = Input.GetAxis("Vertical") * speed;

		Vector3 movement = new Vector3(moveFB, gravity, moveLR);
		movement = transform.rotation * movement;
		character.Move(movement * Time.deltaTime);
	}

	void HandleCameraRotation()
	{
		rotX = Input.GetAxis("Mouse X") * sensitivity;
		rotY = Input.GetAxis("Mouse Y") * sensitivity;

		if (ignoreFirstFrame)
		{
			ignoreFirstFrame = false;
			return;
		}

		if (Cursor.lockState == CursorLockMode.Locked)
		{
			transform.Rotate(0, rotX * Time.deltaTime, 0);
			cam.transform.Rotate(-rotY * Time.deltaTime, 0, 0);

			Vector3 currentRotation = cam.transform.localEulerAngles;
			if (currentRotation.x > 180) currentRotation.x -= 360;
			currentRotation.x = Mathf.Clamp(currentRotation.x, -80, 80);
			cam.transform.localEulerAngles = currentRotation;
		}
	}

	void HandleObjectPickupAndThrow()
	{
		Ray ray = new Ray(cam.transform.position, cam.transform.forward);
		RaycastHit hit;

		if (Input.GetMouseButtonDown(0))
		{
			if (!isHolding && Physics.Raycast(ray, out hit, pickupDistance) && Physics.Raycast(ray, out hit, Mathf.Infinity, ~ignorLayerMask))
			{
				GameObject pickableObj = null;

				if (hit.collider.transform && hit.collider.transform.parent && hit.collider.transform.parent.tag == "Pickable")
				{
					pickableObj = hit.collider.transform.parent.gameObject;
				}

				if ((hit.collider.CompareTag("Pickable") || pickableObj != null) && hit.rigidbody != null)
				{
					if (pickableObj != null)
					{
						heldObject = pickableObj;
					}
					else
					{
						heldObject = hit.collider.gameObject;
					}

					if (heldObject.GetComponent<TaskItem>() && heldObject.GetComponent<TaskItem>().taskTarget != null && !heldObject.GetComponent<TaskItem>().completed)
					{
						QuestMarker.main.GetComponent<QuestMarker>().target = heldObject.GetComponent<TaskItem>().taskTarget;
					}

					heldObject.GetComponent<Rigidbody>().rotation = Quaternion.identity;
					heldObjectRb = heldObject.GetComponent<Rigidbody>();
					heldObjectRb.isKinematic = false;
					heldObjectRb.useGravity = false;
					heldObjectRb.freezeRotation = true;
					isHolding = true;
				}
			}
			else if (isHolding && heldObjectRb)
			{
				heldObjectRb.useGravity = true;
				heldObjectRb.freezeRotation = false;
				heldObjectRb.AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);
				heldObject = null;
				heldObjectRb = null;
				isHolding = false;
				QuestMarker.main.GetComponent<QuestMarker>().target = null;
			}
		}

		if (Input.GetMouseButtonDown(1) && isHolding && heldObjectRb)
		{
			heldObjectRb.useGravity = true;
			heldObjectRb.freezeRotation = false;
			heldObject = null;
			heldObjectRb = null;
			isHolding = false;
			QuestMarker.main.GetComponent<QuestMarker>().target = null;
		}

		if (isHolding && heldObject != null)
		{
			Vector3 offset = new Vector3(heldObject.GetComponent<TaskItem>().offsetX, heldObject.GetComponent<TaskItem>().offsetY, 0f);
			Vector3 holdPosition = cam.transform.position + offset + cam.transform.forward * holdDistance;
			heldObjectRb.velocity = (holdPosition - heldObject.transform.position) * 60f;
		}
	}

	public void Lowering()
	{
		Ray ray = new Ray(cam.transform.position, cam.transform.forward);
		RaycastHit hit;

		if (isHolding)
		{
			heldObjectRb.useGravity = true;
			heldObjectRb.freezeRotation = false;
			heldObject = null;
			heldObjectRb = null;
			isHolding = false;
			QuestMarker.main.GetComponent<QuestMarker>().target = null;
		}
	}

	void CameraRotation(GameObject cam, float rotX, float rotY)
	{
		transform.Rotate(0, rotX * Time.deltaTime, 0);
		cam.transform.Rotate(-rotY * Time.deltaTime, 0, 0);
	}
	
	void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			Input.ResetInputAxes();
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}
}
