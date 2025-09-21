using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class gameController : MonoBehaviour
{
    /// Motion
    public float MainMotionSpeed;
    public float MainMotionDistance;

/// Player Scores
    public TextMeshProUGUI P1ScoreTxt;
    public TextMeshProUGUI P2ScoreTxt;
    public TextMeshProUGUI StatusTxt;

    /// Turns
    int P1Score = 0;
    int P2Score = 0;
    bool P1Turn = true;
    enum ThrowNumber { t1 = 1, t2, t3 };
    ThrowNumber throwNumber = ThrowNumber.t1;


    /// Dart Throw
    public Vector3 dartOffset;
    public Vector3 dartAngle;
    public GameObject dartPre;
    GameObject currentDart;
    Queue<GameObject> darts = new Queue<GameObject>();

    public ScoringValue scoringValue;


    /// Camera Motion
    public enum Mode { Main, MainMotion, Dart };
    public float transitionTime;
    public Mode mode;
    private Mode current;
    private Mode last;
    private float progressUnsmoothed;
    private float progress;

    //Throw Delays
    public float throwDelay = 1.0f; // 1 second delay between throws
private bool canThrow = true;
    private float throwCooldownTimer = 0f;

    void Start()
{
    current = mode;
    last = mode;
    progress = 0;
    
    InitializeScoringValues();
    AutoDetectDartboardCenter();
}

    void Update()
{
    P1ScoreTxt.text = P1Score.ToString();
    P2ScoreTxt.text = P2Score.ToString();

    // Handle throw cooldown
    if (!canThrow)
    {
        throwCooldownTimer -= Time.deltaTime;
        if (throwCooldownTimer <= 0f)
        {
            canThrow = true;
            if (StatusTxt != null && mode == Mode.Main) 
                StatusTxt.text = "Click to aim!";
        }
    }

    if (current != last)
    {
        progressUnsmoothed += Time.deltaTime / transitionTime;
        if (progressUnsmoothed >= 1)
        {
            progressUnsmoothed = 0;
            last = current;
        }
        progress = smooth(progressUnsmoothed);
    }
    else if (current != mode)
    {
        current = mode;
    }
    updatePos();

    // Only process clicks if we can throw
    if (canThrow && Input.GetMouseButtonDown(0))
    {
        if (mode == Mode.Main)
        {
            mode = Mode.MainMotion;
            canThrow = false; // Start cooldown
            throwCooldownTimer = throwDelay;
        }
        else if(mode == Mode.MainMotion && progress == 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                mode = Mode.Dart;
                canThrow = false; // Start cooldown
                throwCooldownTimer = throwDelay;
                
                GameObject dart = (GameObject)Instantiate(dartPre, hit.point + new Vector3(0, 0, -0.35f), Quaternion.Euler(Vector3.zero));
                dart.GetComponentInChildren<Animation>().Play("Throw");
                currentDart = dart;
                darts.Enqueue(dart);
                Score score = decodeScore(hit.point);
                if (P1Turn)
                {
                    P1Score += score.Points;
                    StatusTxt.text = "Player one";
                }
                else
                {
                    P2Score += score.Points;
                    StatusTxt.text = "Player two";
                }
                StatusTxt.text += " scored <i>" + score.Points + "</i>.";

                if(throwNumber == ThrowNumber.t3)
                {
                    P1Turn = !P1Turn;
                    StatusTxt.text += " Next player";
                    throwNumber = ThrowNumber.t1;
                }
                else { throwNumber++; }
            }
        }
        else if(mode == Mode.Dart && progress == 0)
        {
            mode = Mode.Main;
            if (throwNumber == ThrowNumber.t1)
            {
                while (darts.Count > 0)
                {
                    GameObject d = darts.Dequeue();
                    d.GetComponentInChildren<Animation>().Play("Drop"); ;
                    Destroy(d, 1f);
                }
            }
        }
    }
}
    float smooth(float t)
    {
        return t;
    }
    // ADD THESE METHODS TO YOUR gameController CLASS

    void AutoDetectDartboardCenter()
    {
        // Try to find dartboard in scene if not assigned
        if (scoringValue.center == Vector3.zero)
        {
            GameObject dartboardObj = GameObject.FindGameObjectWithTag("Dartboard");
            if (dartboardObj != null)
            {
                scoringValue.center = dartboardObj.transform.position;
                Debug.Log("Auto-detected dartboard center: " + scoringValue.center);
            }
            else
            {
                Debug.LogWarning("No object found with 'Dartboard' tag! Using default center.");
            }
        }
    }

    void InitializeScoringValues()
{
    // Set default scoring values (adjust these based on your scene scale)
    scoringValue.center = GameObject.FindGameObjectWithTag("Dartboard").transform.position;
    scoringValue.BE2xRadius = 0.5f;    // 50 points (bullseye)scoringValue.BE1xRadius = 1.0f;    // 25 points (inner bull)  
scoringValue.min3X = 2.0f;         // Start of triple ring
scoringValue.max3X = 3.0f;         // End of triple ring
scoringValue.min2X = 4.0f;         // Start of double ring  
scoringValue.max2X = 5.0f;         // End of double ring
    // Initialize scoring angles (simplified for now)
    scoringValue.scoringAngles = new ScoringAngles();
    
    Debug.Log("Scoring values initialized. Bullseye radius: " + scoringValue.BE2xRadius);
}
    public void DartHit(Vector3 hitPosition)
    {
        // Calculate score based on ACTUAL hit position
        Score score = decodeScore(hitPosition);

        Debug.Log("Dart hit at: " + hitPosition + " | Score: " + score.Points);

        if (P1Turn)
        {
            P1Score += score.Points;
            if (StatusTxt != null) StatusTxt.text = "Player one scored: " + score.Points;
        }
        else
        {
            P2Score += score.Points;
            if (StatusTxt != null) StatusTxt.text = "Player two scored: " + score.Points;
        }

        UpdateUI();

        // Handle turn progression
        if (throwNumber == ThrowNumber.t3)
        {
            P1Turn = !P1Turn;
            if (StatusTxt != null) StatusTxt.text += " Next player!";
            throwNumber = ThrowNumber.t1;

            // Clear darts after delay
            StartCoroutine(ClearDartsAfterDelay(2f));
        }
        else
        {
            throwNumber++;
            mode = Mode.Main;
            if (StatusTxt != null) StatusTxt.text += " Throw again!";
        }
    }

    public void DartMissed()
    {
        Debug.Log("Dart missed!");

        if (StatusTxt != null) StatusTxt.text = "Missed!";

        // Handle turn progression
        if (throwNumber == ThrowNumber.t3)
        {
            P1Turn = !P1Turn;
            if (StatusTxt != null) StatusTxt.text += " Next player!";
            throwNumber = ThrowNumber.t1;
            StartCoroutine(ClearDartsAfterDelay(1f));
        }
        else
        {
            throwNumber++;
            mode = Mode.Main;
            if (StatusTxt != null) StatusTxt.text += " Try again!";
        }
    }

    IEnumerator ClearDartsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        while (darts.Count > 0)
        {
            GameObject dart = darts.Dequeue();
            if (dart != null)
            {
                Destroy(dart);
            }
        }

        mode = Mode.Main;
        if (StatusTxt != null) StatusTxt.text = "Player " + (P1Turn ? "1" : "2") + " - Click to throw!";
    }

    void UpdateUI()
    {
        if (P1ScoreTxt != null) P1ScoreTxt.text = "P1: " + P1Score;
        if (P2ScoreTxt != null) P2ScoreTxt.text = "P2: " + P2Score;
    }
    void updatePos()
    {
        Dictionary<Mode, posRot> positions = new Dictionary<Mode, posRot>();

        positions[Mode.Main] = new posRot(
            new Vector3(
                0, 0, -20),
            Quaternion.Euler(0, 0, 0));

        positions[Mode.MainMotion] = new posRot(
            new Vector3(
               Mathf.Sin(Time.time * MainMotionSpeed) * MainMotionDistance,
               Mathf.Cos(Time.time * MainMotionSpeed) * MainMotionDistance,
               -20
            ),
            Quaternion.Euler(0, 0, 0));
        if (currentDart != null)
        {
            positions[Mode.Dart] = new posRot(currentDart.transform.position + dartOffset, Quaternion.Euler(dartAngle));
        }
        else
        {
            positions[Mode.Dart] = new posRot(Vector3.zero, Quaternion.Euler(Vector3.zero));
        }

        Vector3 finalPos;
        Quaternion finalRot;

        finalPos = Vector3.Lerp(positions[last].pos, positions[current].pos, progress);
        finalRot = Quaternion.Lerp(positions[last].rot, positions[current].rot, progress);

        transform.position = finalPos;
        transform.rotation = finalRot;
    }
    struct posRot
    {
        public Vector3 pos;
        public Quaternion rot;
        public posRot(Vector3 _pos, Quaternion _rot)
        {
            pos = _pos;
            rot = _rot;
        }
    }

    void AutoScaleScoringZones()
    {
        float scaleFactor = 20f; // Adjust this based on your scene scale

        scoringValue.BE1xRadius = 0.1f * scaleFactor;
        scoringValue.BE2xRadius = 0.05f * scaleFactor;
        scoringValue.min3X = 0.4f * scaleFactor;
        scoringValue.max3X = 0.45f * scaleFactor;
        scoringValue.min2X = 0.7f * scaleFactor;
        scoringValue.max2X = 0.75f * scaleFactor;

        Debug.Log("Scoring zones scaled by factor: " + scaleFactor);
    }
    public struct ScoringValue
    {
        public Vector3 center;
        public float BE1xRadius;
        public float BE2xRadius;
        public float min3X;
        public float max3X;
        public float min2X;
        public float max2X;

        public ScoringAngles scoringAngles;
    }
    [System.Serializable]
    public struct ScoringAngles
    {

        public float angle5_20;
        public float angle20_1;
        public float angle1_18;
        public float angle18_4;
        public float angle4_13;
        public float angle13_6;
        public float angle6_10;
        public float angle10_15;
        public float angle15_2;
        public float angle2_17;
        public float angle17_3;
        public float angle3_19;
        public float angle19_7;
        public float angle7_16;
        public float angle16_8;
        public float angle8_11;
        public float angle11_14;
        public float angle14_9;
        public float angle9_12;
        public float angle12_5;
    }
    [System.Serializable]
    public struct Score
    {
        public enum ScoreMultiplier { x1 = 1, x2, x3 };
        public ScoreMultiplier scoreMultiplier;
        public int Number;
        public int Points { get { return Number * (int)scoreMultiplier; } }
    }

    public Score decodeScore(Vector3 pos)
{
    Vector2 offset = new Vector2((pos - scoringValue.center).x, (pos - scoringValue.center).z);
    float distance = offset.magnitude;
    
    Debug.Log("Distance from center: " + distance);
    
    Score score = new Score();
    
    // TEMPORARY: Simple scoring for testing
    if (distance < 1.0f)
    {
        score.Number = 50;
        score.scoreMultiplier = Score.ScoreMultiplier.x1;
        Debug.Log("50 points!");
    }
    else if (distance < 2.0f)
    {
        score.Number = 25;
        score.scoreMultiplier = Score.ScoreMultiplier.x1;
        Debug.Log("25 points!");
    }
    else if (distance < 9.0f)
    {
        score.Number = 20;
        score.scoreMultiplier = Score.ScoreMultiplier.x3;
        Debug.Log("Triple 20! 60 points!");
    }
    else if (distance < 15.0f)
    {
        score.Number = 20;
        score.scoreMultiplier = Score.ScoreMultiplier.x2;
        Debug.Log("Double 20! 40 points!");
    }
    else
    {
        score.Number = 0;
        score.scoreMultiplier = Score.ScoreMultiplier.x1;
        Debug.Log("Miss! 0 points");
    }
    
    return score;
}

    void OnDrawGizmos()
    {
        // Draw scoring zones
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(scoringValue.center, scoringValue.BE2xRadius); // Bullseye

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(scoringValue.center, scoringValue.BE1xRadius); // Inner bull

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(scoringValue.center, scoringValue.max3X); // Triple ring

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(scoringValue.center, scoringValue.max2X); // Double ring
    }
    void DebugScoringValues()
    {
        Debug.Log("Current Scoring Values:");
        Debug.Log("Center: " + scoringValue.center);
        Debug.Log("BE1xRadius (25): " + scoringValue.BE1xRadius);
        Debug.Log("BE2xRadius (50): " + scoringValue.BE2xRadius);
        Debug.Log("Triple Ring: " + scoringValue.min3X + " - " + scoringValue.max3X);
        Debug.Log("Double Ring: " + scoringValue.min2X + " - " + scoringValue.max2X);
        Debug.Log("Board Radius: " + scoringValue.max2X);
    }

}