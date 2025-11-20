using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
	public static PlayerController main;

	[Header("Movement Settings")]
	[SerializeField] float speed = 10.0f;
	[SerializeField] float sensitivity = 30.0f;
	public float collisionCheckDistance = 0.5f;
	[SerializeField] GameObject cam;
	public bool stuck = false;
	float moveFB, moveLR;
	float rotX, rotY;
	float gravity = -9.8f;
	CharacterController character;

	[Header("Object Pickup Settings")]
	[SerializeField] LayerMask ignorLayerMask;
	[SerializeField] float pickupDistance = 3f;
	[SerializeField] float holdDistance = 1.5f;
	[SerializeField] float throwForce = 10f;
	GameObject heldObject = null;
	Rigidbody heldObjectRb;	
	bool ignoreFirstFrame = false;
	[HideInInspector] public bool isHolding = false;

	void Awake()
	{
		if (main != null && main != this)
		{
			Destroy(gameObject);
			return;
		}

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
			Menu.main.OpenMenu();
		}

		if (Input.GetMouseButtonDown(0) && !GameObject.FindGameObjectWithTag("Popup"))
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

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
		if (cam == null) return;

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
		if (cam == null) return;

		if (Input.GetMouseButtonDown(0))
		{
			if (!isHolding && Input.GetMouseButtonDown(0) && 
				Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, pickupDistance, ~ignorLayerMask)
			)
			{
				GameObject pickableObj = FindPickableObject(hit);
				TaskItem taskItem = null;

				if (pickableObj != null)
                {
                    taskItem = pickableObj.GetComponent<TaskItem>();
                }
				
				if (pickableObj != null && hit.rigidbody != null)
				{
					PickUpObject(pickableObj);

					if (QuestMarker.main != null && taskItem != null && taskItem.taskTarget != null)
                    {
                        QuestMarker.main.target = taskItem.taskTarget;
                    }					
				}
			}
			else if (isHolding)
			{
				DropObject(true);
			}
		}

		if (Input.GetMouseButtonDown(1) && isHolding && heldObjectRb)
		{
			DropObject(false);
		}

		if (isHolding && heldObject != null)
		{
			Vector3 offset = new Vector3(heldObject.GetComponent<TaskItem>().offsetX, heldObject.GetComponent<TaskItem>().offsetY, 0f);
			Vector3 holdPosition = cam.transform.position + offset + cam.transform.forward * holdDistance;
			heldObjectRb.velocity = (holdPosition - heldObject.transform.position) * 60f;
		}
	}

	GameObject FindPickableObject(RaycastHit hit)
	{
		if (hit.collider.CompareTag("Pickable")) 
			return hit.collider.gameObject;
		
		if (hit.collider.transform?.parent?.CompareTag("Pickable") == true)
			return hit.collider.transform.parent.gameObject;
		
		return null;
	}

	void PickUpObject(GameObject obj)
	{
		heldObject = obj;
		heldObjectRb = heldObject.GetComponent<Rigidbody>();
		
		if (heldObjectRb == null) 
		{
			heldObject = null;
			return;
		}
		
		heldObjectRb.isKinematic = false;
		heldObjectRb.useGravity = false;
		heldObjectRb.freezeRotation = true;
		isHolding = true;
	}

	public void DropObject(bool throwObject = false)
    {
		if (heldObjectRb == null)
        {
            return;
        }

        heldObjectRb.useGravity = true;

		if (throwObject)
        {
            heldObjectRb.AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);
        }

		heldObjectRb.freezeRotation = false;
		heldObject = null;
		heldObjectRb = null;
		isHolding = false;
		QuestMarker.main.target = null;
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

	public GameObject GetCam()
    {
        return cam;
    }

	void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
    }

	void OnDestroy()
    {
        cam = null;
		character = null;

        if (main == this)
        {
            main = null;
        }
    }
}
