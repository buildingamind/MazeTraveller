using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateScript : MonoBehaviour
{
    [Header("Should this object rotate? (True/False)")]
    public bool Rotate = true;

    [Header("The speed at which the object rotates: (Degrees/Second)")]
    public float rotateSpeed;

    [Header("Should the object occasionally change directions? (True/False)")]
    public bool randomlySwitchDirections;

    [Header("What is the longest duration a direction should be held? (Seconds)")]
    public float randomSwitchMax;

    [Header("What is the minimum duration a direction should be held? (Seconds)")]
    public float randomSwitchMin;

    [Header("[?] Should the object rotate based on Pivot or Center of Mass? (True/False)")]
    public bool UseCenterOfMass;

    private float randomSwitchTimer;
    private Renderer objectRenderer;

    // Start is called before the first frame update
    void Start()
    {
        randomSwitchTimer = Random.Range(randomSwitchMin, randomSwitchMax);
        objectRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        randomSwitchMin = Mathf.Max(0, randomSwitchMin);
        randomSwitchMax = Mathf.Max(randomSwitchMin + 1f, randomSwitchMax);

        if (Rotate)
        {
            randomSwitchTimer -= Time.deltaTime;
            if (randomlySwitchDirections)
            {
                if (randomSwitchTimer < 0)
                {
                    rotateSpeed *= -1;
                    randomSwitchTimer = Random.Range(randomSwitchMin, randomSwitchMax);
                }
            }
            if (!UseCenterOfMass || objectRenderer == null)
            {
                transform.Rotate(rotateSpeed * Time.deltaTime * Vector3.up);
            }
            else
            {
                transform.RotateAround(objectRenderer.bounds.center, Vector3.up, Time.deltaTime * rotateSpeed);
            }
        }
    }
}
