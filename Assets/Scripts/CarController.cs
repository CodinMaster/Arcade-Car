using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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
  private float timeSinceTimerStart;
  public TextMeshProUGUI countText;
  public TextMeshProUGUI timerText;
  public TextMeshProUGUI highScoreText;

  //-- HomeScreen --
  public GameObject homeScreenParent;

  //-- GameOver--
  public GameObject gameOverParent;
  public GameObject newHighScore;
  public TextMeshProUGUI overScoreText;



  // Start is called before the first frame update
  void Start()
  {
    Time.timeScale = 0;

    theRB.transform.parent = null;
    count = 0;

    highScoreText.text = "";
    countText.text = "";
    timerText.text = "";

    gameOverParent.SetActive(false);
    newHighScore.SetActive(false);
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
    if (gameFinished || Time.timeScale == 0) return;

    timeSinceTimerStart = Time.time - startTime;
    timerText.text = formatTime(timeSinceTimerStart);
  }

  string formatTime(float time)
  {
    string minutes = ((int)time / 60).ToString();
    string seconds = (time % 60).ToString("f0");
    string milliSeconds = (((time % 60) * 100) % 100).ToString("f0");

    return padZero(minutes) + ":" + padZero(seconds) + ":" + padZero(milliSeconds);
  }

  string padZero(string a)
  {
    if (a.Length == 1)
    {
      a = "0" + a;
    }
    return a;
  }

  void GameOver()
  {
    gameFinished = true;
    Time.timeScale = 0;

    gameOverParent.SetActive(true);
    highScoreText.text = "";
    countText.text = "";
    timerText.text = "";

    overScoreText.text = formatTime(timeSinceTimerStart);

    if (PlayerPrefs.GetFloat("HighScore") == 0 || timeSinceTimerStart < PlayerPrefs.GetFloat("HighScore"))
    {
      PlayerPrefs.SetFloat("HighScore", timeSinceTimerStart);
      newHighScore.SetActive(true);
    }
  }

  public void StartGame()
  {
    homeScreenParent.SetActive(false);

    startTime = Time.time;
    Time.timeScale = 1;

    highScoreText.text = "HighScore: " + formatTime(PlayerPrefs.GetFloat("HighScore"));
    SetCountText();
  }

  public void RestartGame()
  {
    SceneManager.LoadScene(0);
  }

  void SetCountText()
  {
    countText.text = "Stars: " + count + "/6";

    if (count >= 6)
    {
      GameOver();
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.CompareTag("PickUp"))
    {
      other.gameObject.SetActive(false);
      count++;

      SetCountText();
    }
  }
}
