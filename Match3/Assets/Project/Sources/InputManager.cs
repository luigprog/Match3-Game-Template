using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// The InputManager holds logic that identify, process and cache input from the user. It also
/// perform some visual feedbacks for input. Clients(other
/// classes) must use the public interface of the InputManager without caring too much about what happens
/// behind the scenes. Swap tiles input is supported by tap and swipe.
/// PS: The InputManager is a "pseudo singleton"
/// </summary>
public class InputManager : MonoBehaviour
{
    #region Fields

    private static InputManager instance;

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private GameObject inputFeedbackCursor;

    private bool processingInput;
    private Tile previouslySelectedTile;
    private float sqrDistanceToConsiderSwipe;

    /// <summary>
    /// Used to cache the last swap input happened. This cache is very important for logics that
    /// happen after the frame where swap input was identified, such as animating cursor feedback,
    /// or checking for matches only based on the "moved" tiles
    /// </summary>
    private SwapInputInfo lastSwapInputInfo;

    /// <summary>
    /// Delegate that is triggered when a swap input happen.
    /// </summary>
    public Action<SwapInputInfo> OnSwapInput;

    #endregion

    #region Properties

    public static InputManager Instance { get { return instance; } }

    public SwapInputInfo LastSwapInputInfo { get { return lastSwapInputInfo; } }

    #endregion

    private void Awake()
    {
        // Pseudo Singleton stuff
        instance = this;
    }

    private void Start()
    {
        sqrDistanceToConsiderSwipe = ((TileManager.TILE_SIZE / 2) + (grid.Spacement / 2)) * ((TileManager.TILE_SIZE / 2) + (grid.Spacement / 2));
    }

    /// <summary>
    /// Liberate the execution of the logic that identify and process user touch input.
    /// </summary>
    public void StartProcessingInput()
    {
        processingInput = true;
    }

    /// <summary>
    /// The process input logic is pumped by the MonoBehaviour Update
    /// </summary>
    private void Update()
    {
        if (processingInput)
        {
            UpdateProcessTapInput();
            UpdateProcessSwipeInput();
        }
    }

    private void UpdateProcessTapInput()
    {
        // Mouse Left Click / touch
        if (Input.GetMouseButtonDown(0))
        {
            Cell tappedCell = grid.GetCellCloseToWorldPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (tappedCell != null)
            {
                // Store the current tapped tile
                Tile currentlySelectedTile = tappedCell.AttachedTile;

                bool currentMustBecomePrevious = false;

                // There is a valid previously tapped tile? (tapped in a previous frame)
                if (previouslySelectedTile != null)
                {
                    // Yes. There is a previously selected tile!

                    // Is the current tapped tile not the same as the previous one?
                    // If they are the same, do nothing (_else)
                    if (currentlySelectedTile != previouslySelectedTile)
                    {
                        // Current and previous are different!

                        if (Tile.AreCrossNeighbors(previouslySelectedTile, currentlySelectedTile))
                        {
                            // Current and previous are cross neighbors, swap input has happened
                            SwapInputHappened(previouslySelectedTile, currentlySelectedTile);
                        }
                        else
                        {
                            // Current and previous are not neighbors Cache the current as the NEW
                            // previous, so later frames can continue the logic of swap input
                            // identification 
                            currentMustBecomePrevious = true;
                        }
                    }
                }
                else
                {
                    // No. There is no previously selected tile. Cache the current as the previous
                    // one, so later frames can continue the logic of swap input identification
                    currentMustBecomePrevious = true;
                }

                if (currentMustBecomePrevious)
                {
                    previouslySelectedTile = currentlySelectedTile;

                    // Update the cursor feedback
                    SetInputFeedbackCursorPosition(previouslySelectedTile.transform.position);
                    inputFeedbackCursor.SetActive(true);
                }
            }
        }
    }

    private void UpdateProcessSwipeInput()
    {
        // Mouse Left Click / Touch
        // Process Swipe logic only if there is already a previously selected tile
        if (Input.GetMouseButton(0) && previouslySelectedTile != null)
        {
            // Finger/Mouse position
            Vector3 fingerPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            fingerPosition.z = 0.0f;

            // Lets calcule a vector from the previously select tile to the current finger/mouse position
            // It will be the swipe input vector
            Vector3 swipeInputVector = fingerPosition - previouslySelectedTile.Cell.Position;
            float sqrMagnitudeOfSwipeInputVector = Vector3.SqrMagnitude(swipeInputVector);

            // When the swipe input vector reachs a ceirtain sqrMagnitude, it is considered a swipe
            if (sqrMagnitudeOfSwipeInputVector > sqrDistanceToConsiderSwipe)
            {
                bool swapInputValidated = false;

                // Now that we have a swipe input, we need to take the direction of the swipeInput
                // vector and test it against known directions(left,right,up,down) to see which one
                // it represents
                Vector3 swipeInputDirection = swipeInputVector.normalized;
                const float DOT_PRODUCT_DIRECTION_VALIDATION = 0.9f;

                // Lets test first against left and right
                float horizontalDotProduct = Vector3.Dot(swipeInputDirection, Vector3.right);
                if (horizontalDotProduct > DOT_PRODUCT_DIRECTION_VALIDATION)
                {
                    // Satisfied the metric, the swipe input vector represents RIGHT
                    swapInputValidated = true;
                    SwapInputHappened(previouslySelectedTile, grid.Cells[previouslySelectedTile.Cell.xIndex + 1, previouslySelectedTile.Cell.yIndex].AttachedTile);

                }
                else if (horizontalDotProduct < -DOT_PRODUCT_DIRECTION_VALIDATION)
                {
                    // Satisfied the -metric, the swipe input vector represents LEFT
                    swapInputValidated = true;
                    SwapInputHappened(previouslySelectedTile, grid.Cells[previouslySelectedTile.Cell.xIndex - 1, previouslySelectedTile.Cell.yIndex].AttachedTile);
                }

                if (!swapInputValidated)
                {
                    // Not validated yet, so the input vector is neither right or left
                    // Lets test agains up and down

                    float verticalDotProduct = Vector3.Dot(swipeInputDirection, Vector3.up);
                    if (verticalDotProduct > DOT_PRODUCT_DIRECTION_VALIDATION)
                    {
                        // Satisfied the metric, the swipe input vector represents UP
                        SwapInputHappened(previouslySelectedTile, grid.Cells[previouslySelectedTile.Cell.xIndex, previouslySelectedTile.Cell.yIndex - 1].AttachedTile);
                    }
                    else if (verticalDotProduct < -DOT_PRODUCT_DIRECTION_VALIDATION)
                    {
                        // Satisfied the -metric, the swipe input vector represents DOWN
                        SwapInputHappened(previouslySelectedTile, grid.Cells[previouslySelectedTile.Cell.xIndex, previouslySelectedTile.Cell.yIndex + 1].AttachedTile);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Stop the execution of the logic that identify and process user touch input.
    /// </summary>
    public void StopProcessingInput()
    {
        processingInput = false;
        previouslySelectedTile = null;
    }

    private void SwapInputHappened(Tile tileA, Tile tileB)
    {
        // Cache the swap input for further usage
        lastSwapInputInfo.tileA = tileA;
        lastSwapInputInfo.tileB = tileB;

        if (OnSwapInput != null)
        {
            OnSwapInput(lastSwapInputInfo);
        }
    }

    private void SetInputFeedbackCursorPosition(Vector3 newPosition)
    {
        // Keep the z intact
        newPosition.z = inputFeedbackCursor.transform.position.z;
        inputFeedbackCursor.transform.position = newPosition;
    }

    /// <summary>
    /// Executes a animation where the feedback cursor "shows" indicates the swap input that just
    /// happened, using the cache LastSwapInputInfo.
    /// </summary>
    /// <param name="onDone">Delegate triggered when the animation complete.</param>
    public void AnimateCursorIndicateSwap(float animationTime, Action onDone = null)
    {
        StartCoroutine(CoroutineAnimateCursorIndicateSwap(animationTime, onDone));
    }

    private IEnumerator CoroutineAnimateCursorIndicateSwap(float animationTime, Action onDone)
    {
        Vector3 from = lastSwapInputInfo.tileA.Cell.Position;
        from.z = inputFeedbackCursor.transform.position.z;
        Vector3 to = lastSwapInputInfo.tileB.Cell.Position;
        to.z = inputFeedbackCursor.transform.position.z;

        inputFeedbackCursor.transform.position = from;
        inputFeedbackCursor.SetActive(true);

        float timer = 0.0f;
        bool animationDone = false;
        while (!animationDone)
        {
            timer += Time.deltaTime;
            float ratio = Mathf.Clamp01(timer / animationTime);
            if (Mathf.Approximately(ratio, 1.0f))
            {
                ratio = 1.0f;
                animationDone = true;
            }

            inputFeedbackCursor.transform.position = Vector3.Lerp(from, to, ratio);

            yield return new WaitForEndOfFrame();
        }

        inputFeedbackCursor.SetActive(false);

        if (onDone != null)
        {
            onDone();
        }
    }

    public struct SwapInputInfo
    {
        public Tile tileA;
        public Tile tileB;
    }
}