using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ThunderWire.Helper.Random;

public class DynamicObject : MonoBehaviour {

    public enum dynamic
    {
        Door,
        Drawer,
        Lever,
        Valve,
        MovableInteract
    }

    public enum type
    {
        Normal,
        Locked,
        Jammed,
    }

    public enum i_type
    {
        Mouse,
        Animation
    }

    public enum key_type
    {
        Normal,
        Inventory
    }

    public dynamic dynamicType = dynamic.Door;

    [Tooltip("Use only on Drawer or Door")]
    public type useType = type.Normal;
    public i_type interactType = i_type.Mouse;
    public key_type keyType = key_type.Normal;

    public int keyID = 0;

    public Animation m_animationObj;
    public string useAnim;
    public string backUseAnim;

    public List<GameObject> IgnoreColliders = new List<GameObject>();

    public bool useJammedPlanks;
    public List<Rigidbody> Planks = new List<Rigidbody>();

    public UnityEvent InteractEvent;
    public UnityEvent DisabledEvent;

    [Tooltip("If empty, text to show is set to default.")]
    public string CustomLockedText;

    //Sounds
    public AudioClip UnlockSound;
    public AudioClip LockedTry;
    public AudioClip Open;
    public AudioClip Close;
    [Range(0, 1)] public float soundVolume = 1;

    //Drawer
    [Tooltip("If true default move vector will be X, if false default vector is Z")]
    public bool moveX;
    public float InteractPos;
    public float MinMove;
    public float MaxMove;
    public bool reverseMove;

    //Lever
    public AudioClip LeverUpSound;
    public bool upLock;
    public float angleStop;

    //Valve
    public AudioClip[] valveTurnSounds;
    public float valveSoundAfter;
    public float valveTurnSpeed;
    public float valveTurnTime;

    private bool turnSound;

    [HideInInspector] public bool isHeld;

    [HideInInspector] public float angle;
    [HideInInspector] public float leverAngle;
    [HideInInspector] public float rotateValue;

    [HideInInspector] public bool isUnlocked;
    [HideInInspector] public bool hasKey;
    [HideInInspector] public bool isOpen = false;
    [HideInInspector] public bool isRotated;
    [HideInInspector] public bool isUp;
    [HideInInspector] public bool isHolding;
    [HideInInspector] public bool hold;
    [HideInInspector] public bool isInvoked;

    [HideInInspector] public Inventory inv;
    [HideInInspector] public float mouseMove;

    public bool disableLoad;

    public bool DebugLeverAngle;
    public bool DebugDoorAngle;

    private Transform obj;
    private Transform old_parent;

    private bool onceUnlock;
    private bool valveInvoked;
    private bool isPlayed;
    private bool isPressed;

    private float defaultAngle;
    private float minCloseAngle;
    private float maxCloseAngle;

    private HingeJoint joint;


    public void ParseUseType(int value)
    {
        useType = (type)value;
    }

    private void OnEnable()
    {
        defaultAngle = transform.eulerAngles.y;
        minCloseAngle = defaultAngle - 10f;
        maxCloseAngle = defaultAngle + 10f;
    }

    void Start()
    {
        inv = GameObject.Find("GAMEMANAGER").GetComponent<HFPS_GameManager>().inventoryScript;

        if (dynamicType == dynamic.Door)
        {
            if (interactType == i_type.Mouse)
            {
                if (GetComponent<HingeJoint>())
                {
                    joint = GetComponent<HingeJoint>();
                }
                else
                {
                    Debug.LogError(transform.parent.gameObject.name + " requires Hinge Joint Component!");
                }
            }

            if (useType == type.Locked || useType == type.Jammed)
            {
                if (joint)
                {
                    GetComponent<Rigidbody>().freezeRotation = true;
                    joint.useLimits = false;
                }
                isUnlocked = false;
            }
            else if (useType == type.Normal)
            {
                if (joint)
                {
                    GetComponent<Rigidbody>().freezeRotation = false;
                    joint.useLimits = true;
                }
                isUnlocked = true;
            }
        }
        else if (dynamicType == dynamic.Drawer)
        {
            IgnoreColliders.Add(Camera.main.transform.root.gameObject);
            if (useType == type.Locked || useType == type.Jammed)
            {
                isUnlocked = false;
            }
            else if (useType == type.Normal)
            {
                isUnlocked = true;
            }
        }
        else if (dynamicType == dynamic.Lever)
        {
            isUnlocked = true;
        }

        if (IgnoreColliders.Count > 0)
        {
            for (int i = 0; i < IgnoreColliders.Count; i++)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), IgnoreColliders[i].GetComponent<Collider>());
            }
        }
    }

    void Update() {
        if (dynamicType == dynamic.Door) {
            angle = transform.eulerAngles.y;

            if (DebugDoorAngle && interactType == i_type.Mouse) {
                if (joint.limits.min < -1)
                {
                    Debug.Log("Angle: " + angle + " , Door Close: " + (defaultAngle - 0.5f));
                }
                else
                {
                    Debug.Log("Angle: " + angle + " , Door Close: " + (defaultAngle + 0.5));
                }
            }

            if (isUnlocked) {
                useType = type.Normal;
                if (interactType == i_type.Mouse) {
                    joint.useLimits = true;
                    GetComponent<Rigidbody>().freezeRotation = false;
                }
            }

            if (interactType == i_type.Mouse)
            {
                if (angle > 1f)
                {
                    if (joint.limits.min < -1)
                    {
                        if (angle <= (defaultAngle - 2f) && !isOpen)
                        {
                            if (Open)
                            {
                                AudioSource.PlayClipAtPoint(Open, transform.position);
                            }
                            isOpen = true;
                        }

                        if (angle > minCloseAngle && angle < maxCloseAngle && angle >= (defaultAngle - 0.5f) && isOpen)
                        {
                            if (Close)
                            {
                                AudioSource.PlayClipAtPoint(Close, transform.position);
                            }
                            isOpen = false;
                        }
                    }
                    else
                    {
                        if (angle >= (defaultAngle + 0.5f) && !isOpen)
                        {
                            if (Open)
                            {
                                AudioSource.PlayClipAtPoint(Open, transform.position);
                            }
                            isOpen = true;
                        }

                        if (angle <= (defaultAngle + 0.2f) && isOpen)
                        {
                            if (Close)
                            {
                                AudioSource.PlayClipAtPoint(Close, transform.position);
                            }
                            isOpen = false;
                        }
                    }
                }
            }

            if(useType == type.Jammed)
            {
                if (useJammedPlanks)
                {
                    CheckJammed();

                    if (Planks.Count < 1)
                    {
                        useType = type.Normal;
                        isUnlocked = true;
                        if (joint)
                        {
                            GetComponent<Rigidbody>().freezeRotation = false;
                            joint.useLimits = true;
                        }
                    }
                }
            }
        }
        else if(dynamicType == dynamic.Drawer)
        {        
            //Drawer Sound      
        }
        else if (dynamicType == dynamic.Lever) {
            if (reverseMove)
            {
                leverAngle = transform.localEulerAngles.x;
            }
            else
            {
                leverAngle = transform.localEulerAngles.y;
            }

            float minAngle = angleStop - 1f;
            float maxAngle = angleStop + 2f;

            if (DebugLeverAngle && interactType == i_type.Mouse) {
                Debug.Log("Angle: " + Mathf.Round(leverAngle) + " Min: " + minAngle + " Max: " + maxAngle);
            }

            if (interactType == i_type.Mouse)
            {
                if (upLock)
                {
                    if (hold)
                    {
                        GetComponent<Rigidbody>().isKinematic = true;
                        GetComponent<Rigidbody>().useGravity = false;
                    }
                    else
                    {
                        GetComponent<Rigidbody>().isKinematic = false;
                        GetComponent<Rigidbody>().useGravity = true;
                    }
                }
                else
                {
                    if (isHolding)
                    {
                        GetComponent<Rigidbody>().isKinematic = false;
                        GetComponent<Rigidbody>().useGravity = false;
                    }

                    if (!isHolding && hold)
                    {
                        GetComponent<Rigidbody>().isKinematic = true;
                        GetComponent<Rigidbody>().useGravity = false;
                    }
                    else if (!hold)
                    {
                        GetComponent<Rigidbody>().isKinematic = false;
                        GetComponent<Rigidbody>().useGravity = true;
                    }
                }

                if (!DebugLeverAngle)
                {
                    if (leverAngle > minAngle && leverAngle < maxAngle && leverAngle >= angleStop)
                    {
                        if (!disableLoad)
                        {
                            InteractEvent.Invoke();
                            if (!isPlayed && LeverUpSound)
                            {
                                AudioSource.PlayClipAtPoint(LeverUpSound, transform.position, 1f);
                                isPlayed = true;
                            }
                        }
                        hold = true;
                    }
                    else
                    {
                        DisabledEvent.Invoke();
                        disableLoad = false;
                        isPlayed = false;
                        hold = false;
                    }
                }
            }
            else
            {
                if(isUp && !isInvoked && !isOpen)
                {
                    if (!disableLoad)
                    {
                        StartCoroutine(WaitEventInvoke());
                    }
                    isOpen = true;
                }
            }
        }
        else if (dynamicType == dynamic.Valve)
        {
            if (rotateValue >= 1f && !valveInvoked)
            {
                if (!disableLoad)
                {
                    InteractEvent.Invoke();
                }
                valveInvoked = true;
            }
            else if(turnSound)
            {
                StartCoroutine(ValveSounds());
                turnSound = false;
            }
        }
        else if (dynamicType == dynamic.MovableInteract)
        {
            if (!isInvoked)
            {
                if (moveX)
                {
                    if (!reverseMove)
                    {
                        if (transform.localPosition.x >= InteractPos)
                        {
                            InteractEvent.Invoke();
                            isInvoked = true;
                        }
                    }
                    else
                    {
                        if (transform.localPosition.x <= InteractPos)
                        {
                            InteractEvent.Invoke();
                            isInvoked = true;
                        }
                    }
                }
                else
                {
                    if (!reverseMove)
                    {
                        if (transform.localPosition.z >= InteractPos)
                        {
                            InteractEvent.Invoke();
                            isInvoked = true;
                        }
                    }
                    else
                    {
                        if (transform.localPosition.z <= InteractPos)
                        {
                            InteractEvent.Invoke();
                            isInvoked = true;
                        }
                    }
                }
            }
        }

        if (!isHeld)
        {
            turnSound = true;
        }
    }

    IEnumerator ValveSounds()
    {
        while (isHeld && !valveInvoked)
        {
            AudioSource.PlayClipAtPoint(valveTurnSounds[Randomizer.Range(0, valveTurnSounds.Length)], transform.position, soundVolume);
            yield return new WaitForSeconds(valveSoundAfter);
        }

        yield return null;
    }

    void CheckJammed()
    {
        for(int i = 0; i < Planks.Count; i++)
        {
            if (!Planks[i].isKinematic)
            {
                Planks.RemoveAt(i);
            }
        }
    }

    public void UseObject()
    {
        if (dynamicType == dynamic.Door || dynamicType == dynamic.Drawer)
        {
            if (!onceUnlock && !isUnlocked)
            {
                if (LockedTry && !isUnlocked && !CheckHasKey())
                {
                    AudioSource.PlayClipAtPoint(LockedTry, transform.position, soundVolume);
                }

                TryUnlock();
            }
        }

        if (interactType == i_type.Animation && isUnlocked)
        {
            if (dynamicType == dynamic.Door || dynamicType == dynamic.Drawer || dynamicType == dynamic.Lever)
            {
                if (!m_animationObj.isPlaying && !hold)
                {
                    if (!isOpen)
                    {
                        m_animationObj.Play(useAnim);
                        if (Open) { AudioSource.PlayClipAtPoint(Open, transform.position, soundVolume); }
                        if (dynamicType == dynamic.Lever)
                        {
                            StartCoroutine(LeverSound());
                        }
                        isOpen = true;
                    }
                    else
                    {
                        m_animationObj.Play(backUseAnim);
                        if (Close && dynamicType == dynamic.Drawer) { AudioSource.PlayClipAtPoint(Close, transform.position, soundVolume); }
                        if(dynamicType == dynamic.Lever)
                        {
                            StartCoroutine(LeverSound());
                        }
                        isOpen = false;
                    }
                }

                if (hold) return;

                if(dynamicType == dynamic.Lever)
                {
                    StartCoroutine(WaitEventInvoke());

                    if (upLock)
                    {
                        hold = true;
                    }
                }
            }
        }
    }

    public bool CheckHasKey()
    {
        if(keyType == key_type.Inventory){
            if (inv)
            {
                return inv.CheckItemIDInventory(keyID);
            }
        }
        else if (hasKey)
        {
            return true;
        }

        return false;
    }

    private void TryUnlock()
    {
        if (keyType == key_type.Inventory)
        {
            if (inv && keyID != -1)
            {
                if (inv.CheckItemIDInventory(keyID))
                {
                    hasKey = true;
                    if (UnlockSound) { AudioSource.PlayClipAtPoint(UnlockSound, transform.position, soundVolume); }
                    StartCoroutine(WaitUnlock());
                    inv.RemoveItem(keyID); // TURNED OFF to not destroy key
                    onceUnlock = true;
                }
            }
            else if (!inv)
            {
                Debug.LogError("Inventory script is not set!");
            }
        }
        else
        {
            if (hasKey)
            {
                if (UnlockSound) { AudioSource.PlayClipAtPoint(UnlockSound, transform.position, soundVolume); }
                StartCoroutine(WaitUnlock());
                onceUnlock = true;
            }
        }
    }

    IEnumerator LeverSound()
    {
        yield return new WaitUntil(() => !m_animationObj.isPlaying);
        if (LeverUpSound) { AudioSource.PlayClipAtPoint(LeverUpSound, transform.position, soundVolume); }
    }

    IEnumerator WaitEventInvoke()
    {
        yield return new WaitUntil(() => !m_animationObj.isPlaying);
        if (!isInvoked)
        {
            InteractEvent.Invoke();
            isInvoked = true;
            isUp = true;
            yield return null;
        }
        else
        {
            DisabledEvent.Invoke();
            disableLoad = false;
            isInvoked = false;
            isUp = false;
            yield return null;
        }
    }

    IEnumerator WaitUnlock()
    {
        if (UnlockSound)
        {
            yield return new WaitForSeconds(UnlockSound.length);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        useType = type.Normal;
        isUnlocked = true;
    }

	public void UnlockDoor()
	{
		if (!isUnlocked) {
			if (UnlockSound) { AudioSource.PlayClipAtPoint(UnlockSound, transform.position, soundVolume); }
			useType = type.Normal;
			isUnlocked = true;
		}
	}

    public void LoadUnlock()
    {
        if (!isUnlocked)
        {
            useType = type.Normal;
            isUnlocked = true;
        }
    }

    void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.GetComponent<Rigidbody>() && dynamicType == dynamic.Drawer)
		{
			obj = collision.transform;
			old_parent = obj.transform.parent;
			obj.transform.SetParent(this.transform);
		}
	}

	void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject.GetComponent<Rigidbody>() && dynamicType == dynamic.Drawer)
		{
			obj.transform.SetParent(old_parent);
			obj = null;
		}
	}

    public void DoorCloseEvent()
    {
        if (Close) { AudioSource.PlayClipAtPoint(Close, transform.position, soundVolume); }
    }
}
