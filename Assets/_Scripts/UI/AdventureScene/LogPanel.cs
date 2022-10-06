using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogPanel : MonoBehaviour
{
    [SerializeField] ScrollRect scrollRect;

	///<summary>
	///Scrolls the scrollview so that the most recent message is always visible.
	///</summary>
    private void OnEnable()
    {
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
}
