using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TGBOnDragHoverClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] CharacterUI CharacterUIRef;
    [SerializeField] TabGroupButton tabButton;


    #endregion UI References


    [Space]
    [Header("Variables")]
    [SerializeField] bool IsDragHovering;
    [SerializeField] float Timer;
    [SerializeField] float HoverTimeUntillClick = 0.3f;


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        tabButton = this.GetComponent<TabGroupButton>();
        IsDragHovering = false;
        Timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDragHovering)
            Timer += Time.deltaTime;

        if (Timer > HoverTimeUntillClick)
        {
            tabButton.OnPointerClick(null);
            Timer = 0;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CharacterUIRef != null && CharacterUIRef.ItemDragData.CurrentlyDraggedItem != null)
            IsDragHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsDragHovering = false;
        Timer = 0;
    }


    #endregion UNITY METHODS


    #region METHODS



    #endregion METHODS
}
