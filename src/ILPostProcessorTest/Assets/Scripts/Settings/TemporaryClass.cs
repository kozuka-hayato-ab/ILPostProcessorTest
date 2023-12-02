using System.Collections;
using System.Collections.Generic;
using Settings;
using UnityEngine;

public class TemporaryClass : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ProjectDefine projectDefine = new ProjectDefine();
        Debug.Log(projectDefine.Value);
        Debug.Log(projectDefine.Value2);
    }
}
