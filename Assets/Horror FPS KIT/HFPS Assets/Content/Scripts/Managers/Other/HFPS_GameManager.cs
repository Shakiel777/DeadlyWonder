/*
 * HFPS_GameManager.cs - script written by ThunderWire Games
 * ver. 1.32
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.PostProcessing;

public enum spriteType
{
    Interact, Grab, Examine
}

/// <summary>
/// HFPS - Game Manager
/// </summary>
public class HFPS_GameManager : MonoBehaviour {

    private ConfigHandler configHandler;

    private PostProcessingBehaviour processingBehaviour;
    private PostProcessingProfile processingProfile;

    private ColorGradingModel.Settings colorGrading;

    [Header("Main")]
    public GameObject Player;
    public InputController inputManager;
    public Inventory inventoryScript;
    public string m_sceneLoader;

    private SaveGameHandler saveHandler;

    [HideInInspector]
    public ScriptManager scriptManager;

    [HideInInspector]
    public HealthManager healthManager;

    [Header("Cursor")]
    public bool m_ShowCursor = false;

    [Header("Game Panels")]
    public GameObject PauseGamePanel;
    public GameObject MainGamePanel;
    public GameObject PlayerDeadPanel;
    public GameObject TabButtonPanel;

    [Header("Pause UI")]
    public KeyCode ShowPauseMenuKey = KeyCode.Escape;
    public bool reallyPause = false;
    public bool useGreyscale = true;
    public float greyscaleFadeSpeed;

    private bool isGreyscaled = false;

    [HideInInspector] public bool isPaused = false;

    [Header("Paper UI")]
    public GameObject PaperTextUI;
    public Text PaperReadText;

    [Header("UI Percentagles")]
    public GameObject BatteryRemaining;
    public GameObject OilRemaining;

    [Header("Valve UI")]
    public Slider ValveSlider;

    private float slideTime;
    private float slideValue;


    [Header("Notification UI")]
    public GameObject saveNotification;
    public GameObject NotificationPanel;
    public GameObject NotificationPrefab;
    public Sprite WarningSprite;
    public float saveFadeSpeed;

    private List<GameObject> Notifications = new List<GameObject>();

    [Header("Hints UI")]
    public Text HintText;

    [Header("Crosshair")]
    public Image Crosshair;

    [Header("UI Amounts")]
    public Text HealthText;
    public GameObject AmmoUI;
    public Text BulletsText;
    public Text MagazinesText;

    [Header("Right Buttons")]
    public bool useSprites;
    public GameObject InteractSprite;
    public GameObject InteractSprite1;

    [Header("Down Examine Buttons")]
    public GameObject DownExamineUI;
    public GameObject ExamineButton1;
    public GameObject ExamineButton2;
    public GameObject ExamineButton3;

    [Header("Down Grab Buttons")]
    public GameObject DownGrabUI;
    public GameObject GrabButton1;
    public GameObject GrabButton2;
    public GameObject GrabButton3;
    public GameObject GrabButton4;

    public Sprite DefaultSprite;

    [HideInInspector]
    public bool isHeld;

    [HideInInspector]
    public bool canGrab;
    [HideInInspector]
    public bool isGrabbed;

    private float fadeHint;
    private bool startFadeHint = false;

    private string GrabKey;
    private string ThrowKey;
    private string RotateKey;
    private KeyCode InventoryKey;

    private bool uiInteractive = true;
    private bool isOverlapping;

    public bool isPressed;

    [HideInInspector]
    public bool ConfigError;

    void Awake()
    {
        configHandler = GetComponent<ConfigHandler>();
        healthManager = Camera.main.transform.root.gameObject.GetComponent<HealthManager>();
        scriptManager = Player.transform.GetChild(0).transform.GetChild(0).GetComponent<ScriptManager>();
        saveHandler = GetComponent<SaveGameHandler>();

        uiInteractive = true;
    }

    void Start()
    {
        TabButtonPanel.SetActive(false);
        saveNotification.SetActive(false);
        HideSprites(spriteType.Interact);
        HideSprites(spriteType.Grab);
        HideSprites(spriteType.Examine);
        Unpause();

        if (m_ShowCursor) {
            Cursor.visible = (true);
            Cursor.lockState = CursorLockMode.None;
        } else {
            Cursor.visible = (false);
            Cursor.lockState = CursorLockMode.Locked;
        }

        processingBehaviour = Camera.main.gameObject.GetComponent<PostProcessingBehaviour>();
        processingProfile = processingBehaviour.profile;
        colorGrading = processingProfile.colorGrading.settings;

        if (useGreyscale)
        {
            processingProfile.colorGrading.enabled = true;
        }
    }

    void Update()
    {
        HintText.gameObject.GetComponent<CanvasRenderer>().SetAlpha(fadeHint);

        if (inputManager.HasInputs())
        {
            GrabKey = inputManager.GetInput("Pickup").ToString();
            ThrowKey = inputManager.GetInput("Throw").ToString();
            RotateKey = inputManager.GetInput("Fire").ToString();
            InventoryKey = inputManager.GetInput("Inventory");
        }

        if (configHandler.ContainsSectionKey("Game", "Volume"))
        {
            float volume = float.Parse(configHandler.Deserialize("Game", "Volume"));
            AudioListener.volume = volume;
        }

        //Fade Out Hint
        if (fadeHint > 0 && startFadeHint)
        {
            fadeHint -= Time.deltaTime;
        }
        else
        {
            startFadeHint = false;
        }

        if (!uiInteractive) return;

        if (Input.GetKeyDown(ShowPauseMenuKey) && !isPressed)
        {
            isPressed = true;
            PauseGamePanel.SetActive(!PauseGamePanel.activeSelf);
            MainGamePanel.SetActive(!MainGamePanel.activeSelf);

            if (useGreyscale)
            {
                StartCoroutine(Greyscale());
            }

            isPaused = !isPaused;
        }
        else if (isPressed)
        {
            isPressed = false;
        }

        if (PauseGamePanel.activeSelf && isPaused && isPressed)
        {
            Crosshair.enabled = false;
            LockStates(true, true, true, true, 3);
            scriptManager.GetScript<PlayerFunctions>().enabled = false;
            if (reallyPause)
            {
                Time.timeScale = 0;
            }
        }
        else if (isPressed)
        {
            Crosshair.enabled = true;
            LockStates(false, true, true, true, 3);
            scriptManager.GetScript<PlayerFunctions>().enabled = true;
            if (TabButtonPanel.activeSelf)
            {
                TabButtonPanel.SetActive(false);
            }
            if (reallyPause)
            {
                Time.timeScale = 1;
            }
        }

        if (Input.GetKeyDown(InventoryKey) && !isPressed && !isPaused && !isOverlapping)
        {
            isPressed = true;
            TabButtonPanel.SetActive(!TabButtonPanel.activeSelf);
        }
        else if (isPressed)
        {
            isPressed = false;
        }

        if (TabButtonPanel.activeSelf && isPressed)
        {
            Crosshair.enabled = false;
            LockStates(true, true, true, true, 0);
            HideSprites(spriteType.Interact);
            HideSprites(spriteType.Grab);
            HideSprites(spriteType.Examine);
        }
        else if (isPressed)
        {
            Crosshair.enabled = true;
            LockStates(false, true, true, true, 0);
        }

        if (Notifications.Count > 3)
        {
            Destroy(Notifications[0]);
            Notifications.RemoveAll(GameObject => GameObject == null);
        }

        processingProfile.colorGrading.settings = colorGrading;
    }

    IEnumerator Greyscale()
    {
        if (!isGreyscaled)
        {
            while(colorGrading.basic.saturation > 0)
            {
                colorGrading.basic.saturation -= Time.fixedDeltaTime * greyscaleFadeSpeed;
                yield return null;
            }

            colorGrading.basic.saturation = 0;
            isGreyscaled = true;
        }
        else
        {
            while (colorGrading.basic.saturation <= 1)
            {
                colorGrading.basic.saturation += Time.fixedDeltaTime * greyscaleFadeSpeed;
                yield return null;
            }

            colorGrading.basic.saturation = 1;
            isGreyscaled = false;
        }
    }

    public void Unpause()
    {
        if (TabButtonPanel.activeSelf)
        {
            TabButtonPanel.SetActive(false);
        }

        if (useGreyscale)
        {
            isGreyscaled = true;
            StartCoroutine(Greyscale());
        }

        Crosshair.enabled = true;
        LockStates(false, true, true, true, 3);
        PauseGamePanel.SetActive(false);
        MainGamePanel.SetActive(true);
        isPaused = false;

        if (reallyPause)
        {
            Time.timeScale = 1;
        }
    }


    /// <summary>
    /// Restrict some Player functions.
    /// </summary>
    /// <param name="LockState">True = Lock, False = Unlock</param>
    /// <param name="Interact">Restrict Player interact function?</param>
    /// <param name="Controller">Restrict Player movement?</param>
    /// <param name="CursorVisible">Show, Hide cursor?</param>
    /// <param name="BlurLevel">0,1,2,3 = Blur Levels</param>
    public void LockStates(bool LockState, bool Interact, bool Controller, bool CursorVisible, int BlurLevel) {
        switch (LockState) {
            case true:
                Player.transform.GetChild(0).GetChild(0).GetComponent<MouseLook>().enabled = false;
                if (Interact) {
                    scriptManager.GetScript<InteractManager>().inUse = true;
                }
                if (Controller) {
                    Player.GetComponent<PlayerController>().controllable = false;
                    scriptManager.GetScript<PlayerFunctions>().enabled = false;
                }
                if (BlurLevel > 0) {
                    if (BlurLevel == 1) { scriptManager.MainCameraBlur.enabled = true; }
                    if (BlurLevel == 2) { scriptManager.ArmsCameraBlur.enabled = true; }
                    if (BlurLevel == 3)
                    {
                        scriptManager.MainCameraBlur.enabled = true;
                        scriptManager.ArmsCameraBlur.enabled = true;
                    }
                }
                if (CursorVisible) {
                    ShowCursor(true);
                }
                break;
            case false:
                Player.transform.GetChild(0).GetChild(0).GetComponent<MouseLook>().enabled = true;
                if (Interact) {
                    scriptManager.GetScript<InteractManager>().inUse = false;
                }
                if (Controller) {
                    Player.GetComponent<PlayerController>().controllable = true;
                    scriptManager.GetScript<PlayerFunctions>().enabled = true;
                }
                if (BlurLevel > 0) {
                    if (BlurLevel == 1) { scriptManager.MainCameraBlur.enabled = false; }
                    if (BlurLevel == 2) { scriptManager.ArmsCameraBlur.enabled = false; }
                    if (BlurLevel == 3)
                    {
                        scriptManager.MainCameraBlur.enabled = false;
                        scriptManager.ArmsCameraBlur.enabled = false;
                    }
                }
                if (CursorVisible) {
                    ShowCursor(false);
                }
                break;
        }
    }

    public void UIPreventOverlap(bool State)
    {
        isOverlapping = State;
    }

    public void MouseLookState(bool State)
    {
        switch (State) {
            case true:
                Player.transform.GetChild(0).GetChild(0).GetComponent<MouseLook>().enabled = true;
                break;
            case false:
                Player.transform.GetChild(0).GetChild(0).GetComponent<MouseLook>().enabled = false;
                break;
        }
    }

    public void ShowCursor(bool state)
    {
        switch (state) {
            case true:
                Cursor.visible = (true);
                Cursor.lockState = CursorLockMode.None;
                break;
            case false:
                Cursor.visible = (false);
                Cursor.lockState = CursorLockMode.Locked;
                break;
        }
    }

    public void AddPickupMessage(string itemName)
    {
        GameObject PickupMessage = Instantiate(NotificationPrefab);
        Notifications.Add(PickupMessage);
        PickupMessage.transform.SetParent(NotificationPanel.transform);
        PickupMessage.GetComponent<ItemPickupNotification>().SetPickupNotification(itemName);
    }

    public void AddMessage(string message)
    {
        GameObject Message = Instantiate(NotificationPrefab);
        Notifications.Add(Message);
        Message.transform.SetParent(NotificationPanel.transform);
        Message.GetComponent<ItemPickupNotification>().SetNotification(message);
    }

    public void WarningMessage(string warning)
    {
        GameObject Message = Instantiate(NotificationPrefab);
        Notifications.Add(Message);
        Message.transform.SetParent(NotificationPanel.transform);
        Message.GetComponent<ItemPickupNotification>().SetNotificationIcon(warning, WarningSprite);
    }

    public void ShowHint(string hint)
    {
        StopCoroutine(FadeWaitHint());
        fadeHint = 1f;
        startFadeHint = false;
        HintText.gameObject.SetActive(true);
        HintText.text = hint;
        HintText.color = Color.white;
        StartCoroutine(FadeWaitHint());
    }

    IEnumerator FadeWaitHint()
    {
        yield return new WaitForSeconds(3f);
        startFadeHint = true;
    }

    public void NewValveSlider(float start, float time)
    {
        ValveSlider.gameObject.SetActive(true);
        StartCoroutine(MoveValveSlide(start, 10f, time));
    }

    public void DisableValveSlider()
    {
        ValveSlider.gameObject.SetActive(false);
        StopCoroutine(MoveValveSlide(0,0,0));
    }

    public IEnumerator MoveValveSlide(float start, float end, float time)
    {
        var currentValue = start;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / (time * 10);
            ValveSlider.value = Mathf.Lerp(currentValue, end, t);
            yield return null;
        }
    }

    public void ShowSaveNotification(float time)
    {
        StartCoroutine(FadeInSave(time));
    }

    IEnumerator FadeInSave(float t)
    {
        saveNotification.SetActive(true);
        Color color = saveNotification.GetComponent<Image>().color;

        color.a = 0;
        saveNotification.GetComponent<Image>().color = color;

        while(saveNotification.GetComponent<Image>().color.a <= 0.95f)
        {
            color.a += Time.fixedDeltaTime * saveFadeSpeed;
            saveNotification.GetComponent<Image>().color = color;
            yield return null;
        }

        color.a = 1;
        saveNotification.GetComponent<Image>().color = color;

        yield return new WaitForSecondsRealtime(t);
        StartCoroutine(FadeOutSave());
    }

    IEnumerator FadeOutSave()
    {
        Color color = saveNotification.GetComponent<Image>().color;

        while (saveNotification.GetComponent<Image>().color.a >= 0.1)
        {
            color.a -= Time.fixedDeltaTime * saveFadeSpeed;
            saveNotification.GetComponent<Image>().color = color;
            yield return null;
        }

        color.a = 0;
        saveNotification.GetComponent<Image>().color = color;

        saveNotification.SetActive(false);
    }

    public bool CheckController()
	{
		return Player.GetComponent<PlayerController> ().controllable;
	}

    public void ShowInteractSprite(int num, string name, string Key)
    {
		if (!isHeld) {
			switch (num) {
				case 1:
					InteractSprite.SetActive (true);
					Image bg = InteractSprite.transform.GetChild (0).GetComponent<Image> ();
					Text buttonKey = InteractSprite.transform.GetChild (1).gameObject.GetComponent<Text> ();
					Text txt = InteractSprite.gameObject.GetComponent<Text> ();
					buttonKey.text = Key;
					txt.text = name;
					if (Key == "Mouse0" || Key == "Mouse1" || Key == "Mouse2") {
						bg.sprite = GetKeySprite (Key);
						buttonKey.gameObject.SetActive (false);
					} else {
						bg.sprite = DefaultSprite;
						buttonKey.gameObject.SetActive (true);
					}
				break;
				case 2:
					InteractSprite1.SetActive (true);
					Image bg1 = InteractSprite1.transform.GetChild (0).GetComponent<Image> ();
					Text buttonKey1 = InteractSprite1.transform.GetChild (1).gameObject.GetComponent<Text> ();
					Text txt1 = InteractSprite1.gameObject.GetComponent<Text> ();
					buttonKey1.text = Key;
					txt1.text = name;
					if (Key == "Mouse0" || Key == "Mouse1" || Key == "Mouse2") {
						bg1.sprite = GetKeySprite (Key);
						buttonKey1.gameObject.SetActive (false);
					} else {
						bg1.sprite = DefaultSprite;
						buttonKey1.gameObject.SetActive (true);
					}
				break;
			}
		}
    }

    public void ShowExamineSprites(string UseKey, string ExamineKey)
    {
        SetKeyCodeSprite(ExamineButton1.transform, UseKey);
        SetKeyCodeSprite(ExamineButton2.transform, RotateKey);
        SetKeyCodeSprite(ExamineButton3.transform, ExamineKey);
        DownExamineUI.SetActive(true);
    }

    public void ShowGrabSprites()
    {
        SetKeyCodeSprite(GrabButton1.transform, GrabKey);
        SetKeyCodeSprite(GrabButton2.transform, RotateKey);
        SetKeyCodeSprite(GrabButton3.transform, ThrowKey);
        GrabButton4.SetActive(true); //ZoomKey
        DownGrabUI.SetActive(true);
    }

    private void SetKeyCodeSprite(Transform Button, string Key)
    {
        if (Key == "Mouse0" || Key == "Mouse1" || Key == "Mouse2")
        {
            Button.GetChild(1).GetComponent<Text>().text = Key;
            Button.GetChild(0).GetComponent<Image>().sprite = GetKeySprite(Key);
            Button.GetChild(0).GetComponent<Image>().rectTransform.sizeDelta = new Vector2(25, 25);
            Button.GetChild(0).GetComponent<Image>().type = Image.Type.Simple;
            Button.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            Button.GetChild(1).GetComponent<Text>().text = Key;
            Button.GetChild(0).GetComponent<Image>().sprite = DefaultSprite;
            Button.GetChild(0).GetComponent<Image>().rectTransform.sizeDelta = new Vector2(34, 34);
            Button.GetChild(0).GetComponent<Image>().type = Image.Type.Sliced;
            Button.GetChild(1).gameObject.SetActive(true);
        }
        if(Key == "None")
        {
            Button.GetChild(1).GetComponent<Text>().text = "NO";
            Button.GetChild(0).GetComponent<Image>().sprite = DefaultSprite;
            Button.GetChild(0).GetComponent<Image>().rectTransform.sizeDelta = new Vector2(34, 34);
            Button.GetChild(0).GetComponent<Image>().type = Image.Type.Sliced;
            Button.GetChild(1).gameObject.SetActive(true);
        }
    }

	public void HideSprites(spriteType type)
	{
		switch (type) {
            case spriteType.Interact:
			InteractSprite.SetActive (false);
			InteractSprite1.SetActive (false);
			break;
            case spriteType.Grab:
            DownGrabUI.SetActive(false);
			break;
            case spriteType.Examine:
            DownExamineUI.SetActive(false);		
			break;
		}
	}

    public void ShowDeadPanel()
    {
        LockStates(true, true, true, true, 0);
        scriptManager.GetScript<ItemSwitcher>().DisableItems();

        PauseGamePanel.SetActive(false);
        MainGamePanel.SetActive(false);
        PlayerDeadPanel.SetActive(true);

        uiInteractive = false;
    }

    public void ChangeScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    public void LoadNextScene(string scene)
    {
        if (saveHandler)
        {
            if (saveHandler.dataBetweenScenes)
            {
                saveHandler.SaveNextSceneData();

                if (!isPaused)
                {
                    LockStates(true, true, true, false, 0);
                }

                if (saveHandler.fadeControl)
                {
                    saveHandler.fadeControl.FadeInPanel();
                }

                StartCoroutine(LoadScene(scene, false));
            }
        }
    }

    public void Retry()
    {
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().name, true));
    }

    private IEnumerator LoadScene(string scene, bool LoadSceneData)
    {
        yield return new WaitUntil(() => !saveHandler.fadeControl.isFading());

        PlayerPrefs.SetString("LoadSaveName", GetComponent<SaveGameHandler>().lastSave);
        PlayerPrefs.SetInt("LoadGame", System.Convert.ToInt32(LoadSceneData));
        PlayerPrefs.SetString("LevelToLoad", scene);
        SceneManager.LoadScene(m_sceneLoader);
    }

	public Sprite GetKeySprite(string Key)
	{
		return Resources.Load<Sprite>(Key);
	}
}