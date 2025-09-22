
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems; // Required for TrackableType

public class gameController : MonoBehaviour
{
    // for getting the gameboard position
    public PlaceGameBoard gameBoard;
    public Vector3 boardPosition;
    public Vector3 boardUp;
    public float MainMotionSpeed;
    public float MainMotionDistance;

    public Text P1ScoreTxt;
    public Text P2ScoreTxt;
    public Text StatusTxt;

    int P1Score = 0;
    int P2Score = 0;
    bool P1Turn = true;
    enum ThrowNumber { tmax = 3 };
    ThrowNumber throwNumber = 0;

    public Vector3 dartOffset;
    public Vector3 hitPosition;
    public Vector3 dartAngle;
    public GameObject dartPre;
    GameObject currentDart;
    Queue<GameObject> darts = new Queue<GameObject>();

    public int currScore = 0;
    public ScoringValue scoringValue;

    // Main == start (nothing happens)
    // MainMotion == dart in motion (throw, player clicked on screen)
    public enum Mode { Main, MainMotion, Dart };
    public float transitionTime;
    public Mode mode;

    private Mode current;
    private Mode last;
    private float progressUnsmoothed;
    private float progress;
    // Use this for initialization
    void Start()
    {
        gameBoard.SetBoardPositionEvent += GetBoardPosition;
        gameBoard.SetBoardUpEvent += GetBoardUp;
        current = mode;
        last = mode;
        progress = 0;
    }
    void GetBoardPosition(Vector3 newPosition)
    {
        boardPosition = newPosition;
    }
    void GetBoardUp(Vector3 newPosition)
    {
        boardUp = newPosition;
    }


    // Update is called once per frame
    void Update()
    {

        if (P1Turn)
        {
            StatusTxt.text = "Player one";
        }
        else
        {
            StatusTxt.text = "Player two";
        }
        StatusTxt.text += " scored " + currScore;

        // if you click the mouse
        if (Input.GetMouseButtonDown(0))
        {
            // checks to see if you hit the board
            RaycastHit hit;
            // if hits the board
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                throwNumber++;
                // current mode is in dart moving mode
                current = Mode.MainMotion;

                // create dart
                GameObject dart = (GameObject)Instantiate(dartPre, hit.point, Quaternion.Euler(Vector3.zero));
                // play throw animation
                dart.GetComponentInChildren<Animation>().Play("Throw");
                // set currentDart to this new dart
                currentDart = dart;
                hitPosition = hit.point;
                // add dart to queue
                darts.Enqueue(dart);
                currScore += decodeScore(hit.point);
                if (P1Turn)
                {
                    P1Score += currScore;
                    P1ScoreTxt.text = $"{P1Score}";
                }
                else
                {
                    P2Score += currScore;
                    P2ScoreTxt.text = $"{P2Score}";
                }
                Debug.Log($"{throwNumber}");
                // if the throw is the third throw, switch players
                if (throwNumber == ThrowNumber.tmax)
                {
                    P1Turn = !P1Turn;
                    StatusTxt.text += " Next player";
                    currScore = 0;
                    //throwNumber = ThrowNumber.t1;
                }
            }
        }

        // if dart is moving
        if (current == Mode.MainMotion)
        {
            // used to smooth the animation
            progressUnsmoothed += Time.deltaTime / transitionTime;
            // check to see if animation is finished
            if (progressUnsmoothed >= 1)
            {
                currentDart.transform.position = hitPosition;

                currentDart.transform.localRotation = Quaternion.Euler(0, 0, 0);

                progressUnsmoothed = 0;
                current = Mode.Main;
            }
            // not finished, update animation
            else
            {
                progress = smooth(progressUnsmoothed);
                // i think this is redundant, but for safety, should probably throw an error
                if (currentDart != null)
                {
                    // update psoition
                    updatePos(currentDart);
                }
            }
        }
        // if dart is in hand
        else if (current == Mode.Main)
        {
            // checks if all the darts have been thrown
            if (throwNumber == ThrowNumber.tmax)
            {
                // animates darts coming back to hand
                while (darts.Count > 0)
                {
                    GameObject d = darts.Dequeue();
                    d.GetComponentInChildren<Animation>().Play("Drop"); ;
                    //Destroy(d, 1f);
                }
                throwNumber = 0;
            }
        }


    }
    float smooth(float t)
    {
        return t;

    }

    //animate
    void updatePos(GameObject currDart)
    {
        // Dictionary<Mode, posRot> positions = new Dictionary<Mode, posRot>();

        posRot startPos = new posRot(
            new Vector3(
                0, 0, -20),
            Quaternion.Euler(90, 0, 0));

        posRot endPos = new posRot(
            new Vector3(
               Mathf.Sin(Time.time * MainMotionSpeed) * MainMotionDistance,
               Mathf.Cos(Time.time * MainMotionSpeed) * MainMotionDistance,
               -20
            ),
            Quaternion.Euler(0, 90, 0));

        Vector3 finalPos;
        Quaternion finalRot;

        finalPos = Vector3.Lerp(startPos.pos, endPos.pos, progress);
        finalRot = Quaternion.Lerp(startPos.rot, endPos.rot, progress);

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

    [System.Serializable]
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

    // function for determining score (MAKE IT 2D WITH THE Y AND Z COORDINATES FOR BETTER ACCURACY)
    int decodeScore(Vector3 pos)
    {
        Vector3 offset = pos - boardPosition;
        float angle = Vector3.Angle(boardUp, offset.normalized);
        // Debug.Log("Board Pos: " + boardPosition);
        // Debug.Log("Pos: " + pos);
        Debug.Log("Offset: " + offset.magnitude);
        // Debug.Log("Up: " + boardUp);
        // Debug.Log("Angle is " + angle);

        // BULLSEYE
        if (offset.magnitude < .1f)
        {
            return 50;
        }
        // SECOND RING
        if (offset.magnitude < .2f)
        {
            return 25;
        }

        if (offset.magnitude < .5f)
        {
            // THIRD RING
            if (offset.magnitude > .25f && offset.magnitude < .35f)
            {
                return 10; // multiply by 3
            }
            // OUTER RING
            else if (offset.magnitude > .45f)
            {
                return 15; // multply by 2
            }
            else
            {
                return 1; // multiply by 1
            }
        }

        return 0;
   

    }


    }

