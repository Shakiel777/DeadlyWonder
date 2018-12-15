
using UnityEngine;

public class DynamicObjectManager : MonoBehaviour {

	private InputController inputManager;
	private HFPS_GameManager gameManager;
	private DynamicObject dynamic;

	[Header("Raycast")]
	public float RayLength;
	public LayerMask CullLayers;
	public string InteractLayer = "Interact";
	public string DynamicObjectTag;

	[Header("Settings")]
	public float moveDoorSpeed;
	public float moveDrawerSpeed;
	public float moveLeverSpeed;

    private bool isHeld;

	private DelayEffect delay;

	private float mouse_x;
	private float mouse_y;
	private float mouselever_y;

	private bool firstGrab;

	private bool isDynamic;
	private bool isDoor;

	private JointMotor motor;
	private HingeJoint joint;

	//Drawer
	float mouseY = 0.0f;

	//Lever
	private bool isLever;
	private bool otherHeld;

    //Valve
    private bool isValve;

	private GameObject objectRaycast;
	private KeyCode UseKey;
	private Ray playerAim;

    private float distance;
    private bool release;

	void Start()
	{
		inputManager = GetComponent<ScriptManager>().GetScript<InputController>();
        gameManager = GetComponent<ScriptManager>().GetScript<HFPS_GameManager>();
		delay = gameObject.transform.GetChild (0).GetChild (1).GetChild (0).GetChild (0).gameObject.GetComponent<DelayEffect> ();
	}

	void Update () {
		if(inputManager.HasInputs())
		{
            UseKey = inputManager.GetInput("Use");
        }

		//Prevent Interact Dynamic Object when player is holding other object
		otherHeld = GetComponent<DragRigidbody> ().CheckHold ();

		if(objectRaycast && !otherHeld && isDynamic && !release)
		{
			if(Input.GetKey(UseKey)){
				isHeld = true;
			}else if(isHeld){
				isHeld = false;
            }
		}

		if (isHeld){
			if (!firstGrab){
				grabObject();
			}else{
				if (isDoor) {
					grabDoor ();
				} else if (!isLever && !isValve) {
					grabDrawer ();
				} else if (isLever && !isValve) {
					grabLever ();
				} else if (isValve) {
                    grabValve();
                }
			}
		}else if(firstGrab){
			Release ();
		}

		if(isHeld)
		{
            gameManager.MouseLookState (false);
			delay.isEnabled = false;
			if (objectRaycast && objectRaycast.GetComponent<DynamicObject> ()) {
				objectRaycast.GetComponent<DynamicObject> ().isHolding = true;
			}
            distance = Vector3.Distance(objectRaycast.transform.position, transform.root.position);
        }
        else{
            gameManager.MouseLookState (true);
            if (objectRaycast && objectRaycast.GetComponent<DynamicObject> ()) {
				objectRaycast.GetComponent<DynamicObject> ().isHolding = false;
			}
            release = false;
            distance = 0;
		}

        if(distance >= 3)
        {
            release = true;
            isHeld = false;
            Release();
        }

		mouse_x = Input.GetAxis("Mouse X")  * (moveDoorSpeed * 10);
		mouse_y = Input.GetAxis("Mouse Y");
		mouselever_y = Input.GetAxis("Mouse Y") * (moveLeverSpeed * 10);

		Ray playerAim = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
		RaycastHit hit;

		if(Physics.Raycast (playerAim, out hit, RayLength, CullLayers) && !isHeld){
			if (hit.collider.gameObject.layer == LayerMask.NameToLayer (InteractLayer)) {
				objectRaycast = hit.collider.gameObject;
				if (objectRaycast.GetComponent<DynamicObject> () && objectRaycast.tag == DynamicObjectTag) {
					dynamic = objectRaycast.GetComponent<DynamicObject> ();
					isDynamic = true;
				} else {
					isDynamic = false;
				}
			} else if(objectRaycast && !isHeld && !firstGrab){
				isDoor = false;
				isLever = false;
                isValve = false;
				objectRaycast = null;
			}
		}
		else if(objectRaycast && !isHeld && !firstGrab)
		{
			isDoor = false;
			isLever = false;
            isValve = false;
            objectRaycast = null;
		}

		if (objectRaycast && !isHeld && joint) {
			if (isLever || isDoor) {
				motor.targetVelocity = 0;
				joint.useMotor = false;
				joint.motor = motor;
			}
		}

        if (dynamic)
        {
            dynamic.isHeld = isHeld;
        }
	}

	void grabObject()
	{
		if (dynamic.dynamicType == DynamicObject.dynamic.Door) {
			isDoor = true;
			isLever = false;
            isValve = false;
        } else if (dynamic.dynamicType == DynamicObject.dynamic.Drawer) {
			isDoor = false;
			isLever = false;
            isValve = false;
        } else if (dynamic.dynamicType == DynamicObject.dynamic.Lever) {
			isLever = true;
			isDoor = false;
            isValve = false;
        } else if (dynamic.dynamicType == DynamicObject.dynamic.Valve) {
            isDoor = false;
			isLever = false;
            isValve = true;
            ValvePass();
        } else if (dynamic.dynamicType == DynamicObject.dynamic.MovableInteract) {
            isDoor = false;
            isLever = false;
            isValve = false;
        }
        firstGrab = true;
	}

	private void grabDoor()
	{
		if (dynamic) {
			if (dynamic.useType == DynamicObject.type.Locked) {
                if (!dynamic.CheckHasKey() && !dynamic.hasKey)
                {
                    if (string.IsNullOrEmpty(dynamic.CustomLockedText))
                    {
                        gameManager.ShowHint("The door is locked, you need a Key to open!");
                    }
                    else
                    {
                        gameManager.ShowHint(dynamic.CustomLockedText);
                    }
                }
				return;
			} else if (dynamic.useType == DynamicObject.type.Jammed) {
                if (string.IsNullOrEmpty(dynamic.CustomLockedText))
                {
                    gameManager.ShowHint("Door is Jammed!");
                }
                else
                {
                    gameManager.ShowHint(dynamic.CustomLockedText);
                }
				return;
			}
		}

        if (dynamic.interactType == DynamicObject.i_type.Mouse)
        {
            joint = objectRaycast.GetComponent<HingeJoint>();
            motor = joint.motor;
            motor.targetVelocity = mouse_x;
            motor.force = (moveDoorSpeed * 10);
            joint.motor = motor;
            joint.useMotor = true;
        }
	}

	private void grabDrawer()
	{
		bool vectorMoveX = dynamic.moveX;

		if (dynamic) {
			if (dynamic.useType == DynamicObject.type.Locked) {
                if (!dynamic.CheckHasKey() && !dynamic.hasKey)
                {
                    if (string.IsNullOrEmpty(dynamic.CustomLockedText))
                    {
                        gameManager.ShowHint("The drawer is locked, you need a Key to open!");
                    }
                    else
                    {
                        gameManager.ShowHint(dynamic.CustomLockedText);
                    }
                }
				return;
			} else if (dynamic.useType == DynamicObject.type.Jammed) {
                gameManager.ShowHint ("The drawer is Jammed.");
				return;
			}
		}

        if (dynamic.interactType == DynamicObject.i_type.Mouse)
        {
            if (dynamic.reverseMove)
            {
                mouseY = (-mouse_y * moveDrawerSpeed);
            }
            else
            {
                mouseY = (mouse_y * moveDrawerSpeed);
            }

            objectRaycast.GetComponent<DynamicObject>().mouseMove = mouseY;

            if (!vectorMoveX)
            {
                Vector3 move = new Vector3(0, 0, mouseY);
                Vector3 pos = objectRaycast.transform.localPosition;
                objectRaycast.transform.localPosition = new Vector3(pos.x, pos.y, Mathf.Clamp(pos.z, dynamic.MinMove, dynamic.MaxMove));
                objectRaycast.transform.Translate(move * Time.deltaTime);
            }
            else
            {
                Vector3 move = new Vector3(mouseY, 0, 0);
                Vector3 pos = objectRaycast.transform.localPosition;
                objectRaycast.transform.localPosition = new Vector3(Mathf.Clamp(pos.x, dynamic.MinMove, dynamic.MaxMove), pos.y, pos.z);
                objectRaycast.transform.Translate(move * Time.deltaTime);
            }
        }
	}

	private void grabLever ()
	{
        if (dynamic.interactType == DynamicObject.i_type.Mouse)
        {
            joint = objectRaycast.GetComponent<HingeJoint>();
            motor = joint.motor;
            motor.targetVelocity = mouselever_y;
            motor.force = (moveLeverSpeed * 10);
            joint.motor = motor;
            joint.useMotor = true;
        }
	}

    void ValvePass()
    {
        if (!(dynamic.rotateValue >= 1f))
        {
            gameManager.NewValveSlider(dynamic.rotateValue, dynamic.valveTurnTime);
        }
    }

    private void grabValve()
    {
        if(!(dynamic.rotateValue >= 1f)){
            float z = dynamic.valveTurnSpeed;
            Vector3 rotation = new Vector3(0, 0, -z);
            objectRaycast.transform.Rotate(rotation);
            dynamic.rotateValue = gameManager.ValveSlider.value;
        }
    }

	void Release()
	{
        StopAllCoroutines();
        gameManager.DisableValveSlider();
        delay.isEnabled = true;
		dynamic = null;
		firstGrab = false;
	}
}
