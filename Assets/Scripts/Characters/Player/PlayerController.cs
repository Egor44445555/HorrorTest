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
	[SerializeField] float maxHoldDistance = 7f;
	[SerializeField] float throwForce = 10f;
	[SerializeField] float pickupSmoothness = 10f;
	[SerializeField] float maxHoldSpeed = 10f;
	GameObject heldObject = null;
	Rigidbody heldObjectRb;	
	bool ignoreFirstFrame = false;
	[HideInInspector] public bool isHolding = false;


	[Header("Camera Shake")]
    public CameraShake cameraShake;
    public float shakeUpdateInterval = 0.3f;
    
    [Header("Chase Settings")]
    public bool isBeingChased = false;
    public float chaseIntensity = 1f;

	float lastShakeTime;


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
		cameraShake = cam.GetComponent<CameraShake>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void Update()
	{
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
		FindPickup();
		PickupAndThrow();
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

	void PickupAndThrow()
    {
        if (Input.GetMouseButtonDown(0))
		{
			if (isHolding)
			{
				ThrowObject();
			}
			else
			{
				TryPickup();
			}
		}

		if (Input.GetKeyDown(KeyCode.E))
		{
			if (!isHolding)
			{
				TryPickup();
			}
			else
			{
				DropObject();
			}
		}

        if (Input.GetMouseButtonDown(1) && isHolding)
		{
			DropObject();
		}

        if (isHolding)
        {
            UpdateHeldObjectPosition();
        }
    }

	void TryPickup()
	{
		Ray ray = new Ray(cam.transform.position, cam.transform.forward);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, pickupDistance, ~ignorLayerMask))
		{
			GameObject hitObject = hit.collider.gameObject;

			if (hitObject.CompareTag("Pickable") || (hit.collider.transform.parent && hit.collider.transform.parent.CompareTag("Pickable")))
			{
				if (hit.rigidbody != null)
				{
					heldObject = hitObject.CompareTag("Pickable") ? hitObject : hit.collider.transform.parent.gameObject;
					float objectDistance = Vector3.Distance(heldObject.transform.position, transform.position);
					float minDistance = 0.1f;

					heldObjectRb = heldObject.GetComponent<Rigidbody>();
					heldObjectRb.isKinematic = true;
					heldObjectRb.useGravity = false;
					heldObjectRb.freezeRotation = true;
					
					if (heldObject.GetComponent<Collider>() != null)
					{
						Physics.IgnoreCollision(character, heldObject.GetComponent<Collider>(), true);
					}
					
					holdDistance = objectDistance > minDistance ? objectDistance : minDistance;
					isHolding = true;
					UICursor.main.GrabCursor(false);
				}
			}
		}
	}

	void UpdateHeldObjectPosition()
	{
		if (heldObject != null && heldObjectRb != null)
		{
			Vector3 targetPosition = cam.transform.position + cam.transform.forward * holdDistance;
			
			heldObjectRb.MovePosition(Vector3.Lerp(
				heldObject.transform.position, 
				targetPosition, 
				pickupSmoothness * Time.deltaTime
			));
			
			heldObjectRb.MoveRotation(Quaternion.Lerp(
				heldObject.transform.rotation,
				transform.rotation,
				pickupSmoothness * Time.deltaTime
			));
		}

		if (heldObject == null || Vector3.Distance(heldObject.transform.position, transform.position) > maxHoldDistance)
		{
			DropObject();
		}
	}

	void FindPickup()
    {
		if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupDistance, ~ignorLayerMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.CompareTag("Pickable") || (hit.collider.transform.parent && hit.collider.transform.parent.CompareTag("Pickable")))
            {
                heldObject = hitObject.CompareTag("Pickable") ? hitObject : hit.collider.transform.parent.gameObject;

                if (hit.rigidbody != null && !isHolding)
                {
                    UICursor.main.GrabCursor(true);
                }
            }
            else
            {
                UICursor.main.GrabCursor(false);
            }
        }
        else
        {
            UICursor.main.GrabCursor(false);
        }
    }

	void ThrowObject()
    {
        if (heldObjectRb != null)
        {
            DropObject(true);
        }
    }

	public void DropObject(bool throwing = false)
	{
		if (heldObjectRb == null) return;

		heldObjectRb.isKinematic = false;
		heldObjectRb.useGravity = true;
		heldObjectRb.freezeRotation = false;
		
		if (heldObject != null && heldObject.GetComponent<Collider>() != null)
		{
			Physics.IgnoreCollision(character, heldObject.GetComponent<Collider>(), false);
		}

		if (throwing)
        {
            heldObjectRb.AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);    
        }
		
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
    
    public void StartChase()
    {
        isBeingChased = true;
        cameraShake.StartChaseShake();
    }
    
    public void EndChase()
    {
        isBeingChased = false;
        cameraShake.StopShake();
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
