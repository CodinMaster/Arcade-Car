﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CarController : MonoBehaviour
{
  public Rigidbody theRB;

  public float forwardAccel = 8f, reverseAccel = 4f, maxSpeed = 50f, turnStrength = 180f, gravityForce = 10f, dragOnGround = 3f;

  private float speedInput, turnInput;
  private bool grounded;
  public LayerMask whatIsGround;
  public float groundRayLength = 0.5f;
  public Transform groundRayPoint;

  public Transform leftFrontWheel, rightFrontWheel;
  public float maxWheelTurn = 25f;

  public ParticleSystem[] dustTrail;
  public float maxEmission = 25f;
  private float emissionRate;

  private int count;
  private float startTime;
  private bool gameFinished;
  public TextMeshProUGUI countText;
  public TextMeshProUGUI timerText;

  // Start is called before the first frame update
  void Start()
  {
    theRB.transform.parent = null;
    count = 0;
    startTime = Time.time;

    SetCountText();
  }

  // Update is called once per frame
  void Update()
  {

    speedInput = 0f;
    if (Input.GetAxis("Vertical") > 0)
    {
      speedInput = Input.GetAxis("Vertical") * forwardAccel * 1000f;
    }
    else if (Input.GetAxis("Vertical") < 0)
    {
      speedInput = Input.GetAxis("Vertical") * reverseAccel * 1000f;
    }

    turnInput = Input.GetAxis("Horizontal");
    if (grounded)
    {
      transform.rotation = Quaternion.Euler(
          transform.rotation.eulerAngles + new Vector3(
              0f,
              turnInput * turnStrength * Time.deltaTime * Input.GetAxis("Vertical"),
              0f
          )
      );
    }

    leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
    rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x, turnInput * maxWheelTurn, rightFrontWheel.localRotation.eulerAngles.z);

    // car's position == sphere's position
    transform.position = theRB.transform.position;

    SetTimerText();
  }

  private void FixedUpdate()
  {
    grounded = false;
    RaycastHit hit;

    if (Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, whatIsGround))
    {
      grounded = true;

      transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
    }

    emissionRate = 0;

    if (grounded)
    {
      theRB.drag = dragOnGround;

      // absoulte because for reverse it'll be negative
      if (Mathf.Abs(speedInput) > 0)
      {
        theRB.AddForce(transform.forward * speedInput);
        emissionRate = maxEmission;
      }
    }
    else
    {
      theRB.drag = 0.1f;

      // push back down towards ground
      theRB.AddForce(-Vector3.up * gravityForce * 100f);
    }

    foreach (ParticleSystem part in dustTrail)
    {
      var emissionModule = part.emission;
      emissionModule.rateOverTime = emissionRate;
    }
  }

  void SetTimerText()
  {
    if (gameFinished) return;

    float timeSinceTimerStart = Time.time - startTime;
    string minutes = ((int)timeSinceTimerStart / 60).ToString();
    string seconds = (timeSinceTimerStart % 60).ToString("f0");
    string milliSeconds = (((timeSinceTimerStart % 60) * 100) % 100).ToString("f0");

    timerText.text = padZero(minutes) + ":" + padZero(seconds) + ":" + padZero(milliSeconds);
  }

  string padZero(string a)
  {
    if (a.Length == 1)
    {
      a = "0" + a;
    }
    return a;
  }

  void SetCountText()
  {
    countText.text = "Stars: " + count + "/6";

    if (count >= 6)
    {
      gameFinished = true;
      timerText.color = Color.yellow;
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    Debug.Log("YO");
    if (other.gameObject.CompareTag("PickUp"))
    {
      other.gameObject.SetActive(false);
      count++;

      SetCountText();
    }
  }
}
