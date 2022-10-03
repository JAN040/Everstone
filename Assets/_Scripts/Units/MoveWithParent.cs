using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithParent : MonoBehaviour
{
    [SerializeField] GameObject Parent;
    public Vector3 Offset;

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Parent.transform.position + Offset;
    }
}
