using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingItem : MonoBehaviour
{
  public float rotationsPerMinute = 30f;

  // Update is called once per frame
  void Update()
  {
    transform.Rotate(0, 0, 6.0f * rotationsPerMinute * Time.deltaTime);
  }
}
