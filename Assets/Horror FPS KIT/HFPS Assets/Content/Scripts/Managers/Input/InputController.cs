/*
 * InputController.cs - by ThunderWire Studio
 * Ver. 1.3
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using ThunderWire.Helper.Parser;

/// <summary>
/// Script which control main Input functions
/// </summary>
public class InputController : MonoBehaviour {

    private ConfigHandler configHandler;
    private UICustomOptions Options;

    [Header("Main")]
    [Tooltip("All rebindable buttons must be added here.")]
    public ControlsHelper controlsHelper;

	private List<string> InputKeysCache = new List<string> ();
    private Dictionary<string, string> AllInputs = new Dictionary<string, string>();

    private bool rebind;
	private Text buttonText;
	private string inputName;
	private string defaultKey;

    void Awake()
    {
        if (GetComponent<ConfigHandler>() && GetComponent<UICustomOptions>())
        {
            configHandler = GetComponent<ConfigHandler>();
            Options = GetComponent<UICustomOptions>();
        }
    }

    void Start () {
        if (!GetComponent<ConfigHandler>() && !GetComponent<UICustomOptions>())
        {
            Debug.LogError("Input Error: Missing ConfigHandler or UICustomOptions script in " + gameObject.name);
            return;
        }

        if (!controlsHelper)
        {
            Debug.LogError("Input Error: ControlsHelper field cannot be null!");
            return;
        }

        for (int i = 0; i < controlsHelper.InputsList.Count; i++)
        {
            controlsHelper.InputsList[i].InputButton.GetComponent<Button>().onClick.AddListener(delegate { RebindSelected(); });
        }

        if (!configHandler.Error)
        {
            Deserialize();
        }
        else
        {
            configHandler.CreateSection("Input");
            UseDefault();
        }

        LoadInputsToList();
    }
	
	public void RebindSelected()
	{
        var go = EventSystem.current.currentSelectedGameObject;
        foreach(var input in controlsHelper.InputsList)
        {
            if (go.name == input.InputButton.gameObject.name && !rebind)
            {
                buttonText = input.InputButton.transform.GetChild(0).gameObject.GetComponent<Text>();
                defaultKey = buttonText.text;
                buttonText.text = "Press Button";
                inputName = input.Input;
                rebind = true;
            }

            input.InputButton.interactable = false;
        }

        Options.ApplyButton.interactable = false;
        Options.BackButton.interactable = false;
    }
	
	void Update()
	{
		foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKeyDown (kcode) && rebind) {
                if (kcode != KeyCode.Escape)
                {
                    if (kcode.ToString() == defaultKey)
                    {
                        buttonText.text = defaultKey;
                        buttonText = null;
                        inputName = null;
                        rebind = false;

                        Options.ApplyButton.interactable = true;
                        Options.BackButton.interactable = true;

                        foreach (var input in controlsHelper.InputsList)
                        {
                            input.InputButton.interactable = true;
                        }
                    }
                    else
                    {
                        RebindKey(kcode.ToString());
                    }
                }
                else
                {
                    BackRewrite();
                }
			}
		}
	}

	void RebindKey(string kcode)
	{
		if (!InputKeysCache.Contains(kcode)) {
			buttonText.text = kcode;
			SerializeInput (inputName, kcode);
			UpdateInputCache();
			buttonText = null;
			inputName = null;
			rebind = false;

            Options.ApplyButton.interactable = true;
            Options.BackButton.interactable = true;

            foreach (var input in controlsHelper.InputsList)
            {
                input.InputButton.interactable = true;
            }
        } else {
            Options.DuplicateInputGo.SetActive(true);
            Options.DuplicateInputGo.transform.GetChild(0).GetComponent<Text>().text = "Input key \"" + kcode + "\" is already defined";
            Options.RewriteKeycode = kcode;
			rebind = false;
		}
	}

    public void Rewrite(string RewriteKeycode)
    {
        foreach (var input in controlsHelper.InputsList)
        {
            Text DuplicateKeyText = input.InputButton.transform.GetChild(0).gameObject.GetComponent<Text>();

            if (DuplicateKeyText.text == RewriteKeycode)
            {
                DuplicateKeyText.text = "None";
                SerializeInput(input.Input, "None");
            }

            input.InputButton.interactable = true;
        }

        Options.ApplyButton.interactable = true;
        Options.BackButton.interactable = true;

        buttonText.text = RewriteKeycode;
        SerializeInput(inputName, RewriteKeycode);
        UpdateInputCache();
        inputName = null;
    }

    public void BackRewrite()
    {
        buttonText.text = defaultKey;
        buttonText = null;
        inputName = null;
        rebind = false;

        Options.ApplyButton.interactable = true;
        Options.BackButton.interactable = true;

        foreach (var input in controlsHelper.InputsList)
        {
            input.InputButton.interactable = true;
        }
    }

    public void RefreshInputs()
    {
        AllInputs.Clear();
        Deserialize();
    }
	
	void SerializeInput(string input, string button)
	{
        configHandler.Serialize("Input", input, button);
	}

	void LoadInputsToList()
	{
		for (int i= 0; i < controlsHelper.InputsList.Count; i++)
		{
			string value = configHandler.Deserialize("Input", controlsHelper.InputsList[i].Input);
			InputKeysCache.Add (value);
		}
	}

	void UpdateInputCache()
	{
		InputKeysCache.Clear ();
		for (int i= 0; i < controlsHelper.InputsList.Count; i++)
		{
			string value = configHandler.Deserialize("Input", controlsHelper.InputsList[i].Input);
			InputKeysCache.Add (value);
		}
	}
	
	void Deserialize()
	{
        for (int i = 0; i < controlsHelper.InputsList.Count; i++)
		{
            //Set UI Inputs
			string value = configHandler.Deserialize("Input", controlsHelper.InputsList[i].Input);
			Text bText = controlsHelper.InputsList[i].InputButton.transform.GetChild(0).gameObject.GetComponent<Text>();
            bText.text = value;

            //Set Inputs Dictionary
            UpdateInputs(controlsHelper.InputsList[i].Input, value);
		}
	}

    void UseDefault()
    {
        for (int i = 0; i < controlsHelper.InputsList.Count; i++)
        {
            Text bText = controlsHelper.InputsList[i].InputButton.transform.GetChild(0).gameObject.GetComponent<Text>();
            KeyCode keycode = controlsHelper.InputsList[i].DefaultKey;
            if (keycode != KeyCode.None)
            {
                bText.text = keycode.ToString();
                SerializeInput(controlsHelper.InputsList[i].Input, keycode.ToString());
            }
            else
            {
                bText.text = "No Default Key!";
            }
        }
    }

    void UpdateInputs(string key, string value)
    {
        if (AllInputs.ContainsKey(key))
        {
            AllInputs[key] = value;
        }
        else
        {
            AllInputs.Add(key, value);
        }
    }

    public KeyCode GetInput(string Key)
    {
        if (AllInputs.ContainsKey(Key))
        {
            return Parser.Convert<KeyCode>(AllInputs[Key]);
        }
        else
        {
            return KeyCode.None;
        }
    }

    public int Count()
    {
        return AllInputs.Count;
    }

    public bool HasInputs()
    {
        return AllInputs.Count > 0;
    }
}
