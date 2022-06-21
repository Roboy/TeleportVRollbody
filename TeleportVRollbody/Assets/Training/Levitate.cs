﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Levitate : MonoBehaviour
{ 
    [SerializeField] private float height;
    [SerializeField] private float heightDiff;
    [SerializeField] private float speed = 1;
    
    /// <summary>
    /// Levitates this gameObject with the specified attributes
    /// </summary>
    void Update()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z)
                                  + Vector3.up * (height + heightDiff * 0.5f * Mathf.Sin(Time.time * speed));
    }
}
