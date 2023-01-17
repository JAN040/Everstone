using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Apply this script to a gameobject with the Unitys button script attached and set the Hotkey variable
/// This script checks for Key press every update, and triggers the button when the key press is detected.
/// </summary>
public class HotkeyButton : MonoBehaviour
{
    [SerializeField] KeyCode Hotkey;
    [SerializeField] bool HoldEnabled = false;
    private Button ButtonRef;

    // Start is called before the first frame update
    void Start()
    {
        ButtonRef = GetComponent<Button>();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    // Update is called once per frame
    void Update()
    {
        if (ButtonRef == null || Hotkey == KeyCode.None)
            return;

        if (Input.GetKeyDown(Hotkey))
        {
            if (HoldEnabled)
            {
                Debug.Log($"Button hold: {Hotkey}");
                StartCoroutine(Action());
            }
            else
            {
                ButtonRef.onClick.Invoke();
            }
        }
        if (Input.GetKeyUp(Hotkey) && HoldEnabled)
        {
            Debug.Log($"Button release: {Hotkey}");
            StopAllCoroutines();
        }
    }

    IEnumerator Action()
    {
        while (true)
        {
            Debug.Log($"Hotkey '{Hotkey}' Action!");
            ButtonRef.onClick.Invoke();

            yield return new WaitForSeconds(1f);
        }
    }
}
