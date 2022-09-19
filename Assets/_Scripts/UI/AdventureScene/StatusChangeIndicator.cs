using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusChangeIndicator : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textField;
    [SerializeField] float lifeTime = 1f;
    [SerializeField] float minDistance = 1.5f;
    [SerializeField] float maxDistance = 2f;

    private Vector3 initPos;
    private Vector3 targetPos;
    [SerializeField] float timer;
    [SerializeField] string message;
    [SerializeField] float direction = 0f;

    [SerializeField] FacingDirection movementDirection;

    // Start is called before the first frame update
    void Start()
    {
        initPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        float halfTimer = lifeTime / 2f;

        if (timer > lifeTime)
            Destroy(gameObject);
        else if (timer > halfTimer)
            textField.color = Color.Lerp(textField.color, Color.clear, (timer - halfTimer) / (lifeTime - halfTimer));

        //move the popup towards targetPos
        transform.localPosition = Vector3.Lerp(initPos, targetPos, Mathf.Sin(timer / lifeTime));
        //transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Mathf.Sin(timer / lifeTime));
    }

    public void SetTextAndColor(string text, Color color, FacingDirection direction)
    {
        textField.text = text;
        message = text;
        textField.color = color;
        movementDirection = direction;

        Init();
    }

    
    /// <summary>
    /// Cant fully init in start() method cause we dont yet know the target direction
    /// </summary>
    private void Init()
    {
        direction = GetDirection();
        float distance = Random.Range(minDistance, maxDistance);

        targetPos = initPos + (Quaternion.Euler(0, 0, direction) * new Vector3(distance, distance, 0f));
        transform.localScale = Vector3.one;
    }

    private float GetDirection()
    {
        float randAngle = Random.rotation.eulerAngles.z;

        while (!IsAngleOk(randAngle))
        {
            randAngle = Random.rotation.eulerAngles.z;
        }

        return randAngle;
    }

    private bool IsAngleOk(float angle)
    {
        if (movementDirection == FacingDirection.Left)
            return angle > 90 && angle < 180;
        else
            return angle > 270;
    }
}
