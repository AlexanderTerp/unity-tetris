using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{

    private const float IMMEDIATELY = 0;

    public float ShiftRepeatIntervalSeconds = 0.1f;

    private GameState _gameState;
    private GameConstants _gameConstants;
    private BlockController _blockController;
    private ShiftDirection? _previousFrameDirection;
    private float _nextMovementTimeSeconds;
    private bool _isAwaitingRowCompletion;

    void Awake()
    {
        _gameState = GoUtil.FindGameState();
        _gameState.RowsCompletedEvent += (ignored) => StartCoroutine(AwaitRowCompletion());
        _gameConstants = GoUtil.FindGameConstants();

        _blockController = GetComponent<BlockController>();
        _previousFrameDirection = null;
        _nextMovementTimeSeconds = IMMEDIATELY;
        _isAwaitingRowCompletion = false;
    }

    void Update()
    {
        if (!_gameState.IsGameInProgress()) return;
        RunPlayerInput();
    }

    private void RunPlayerInput()
    {
        RunPlayerPauseToggling();

        if (_gameState.IsPaused() || _isAwaitingRowCompletion) return;

        RunPlayerStashing();
        RunInstantPlace();
        RunPlayerTranslation();
        RunPlayerRotation();
    }

    private void RunPlayerPauseToggling()
    {
        if (KeyBindingsChecker.InputEscape())
        {
            _gameState.ToggleGamePaused();
        }
    }

    private void RunPlayerStashing()
    {
        if (KeyBindingsChecker.InputShift())
        {
            _blockController.TryBlockStashSwap();
        }
    }

    private void RunInstantPlace()
    {
        if (KeyBindingsChecker.InputSpace())
        {
            _blockController.InstantPlace();
        }
    }

    private void RunPlayerTranslation()
    {
        bool inputtingRight = KeyBindingsChecker.InputRight();
        bool inputtingLeft = KeyBindingsChecker.InputLeft();
        bool inputtingDown = KeyBindingsChecker.InputDown();
        ShiftDirection? inputDirection = DetermineShiftDirection(inputtingRight, inputtingLeft, inputtingDown);
        if (inputDirection == null) return;

        RunLastFrameDirectionCheck((ShiftDirection)inputDirection);
        if (Time.time < _nextMovementTimeSeconds) return;

        _nextMovementTimeSeconds = Time.time + ShiftRepeatIntervalSeconds;
        switch (inputDirection)
        {
            case (ShiftDirection.Right):
                _blockController.TryMove(1, 0);
                return;
            case (ShiftDirection.Left):
                _blockController.TryMove(-1, 0);
                return;
            case (ShiftDirection.Down):
                _blockController.TryMove(0, 1);
                return;
            default:
                throw new InvalidProgramException("Bug! Should not be possible to reach this!");
        }
    }

    private void RunLastFrameDirectionCheck(ShiftDirection inputDirection)
    {
        if (_previousFrameDirection == inputDirection) return;
        _previousFrameDirection = inputDirection;
        _nextMovementTimeSeconds = IMMEDIATELY;
    }

    private void RunPlayerRotation()
    {
        if (KeyBindingsChecker.InputUp())
        {
            _blockController.TryRotate(RotationDirection.Clockwise);
            return;
        }
    }

    private IEnumerator AwaitRowCompletion()
    {
        _isAwaitingRowCompletion = true;
        yield return new WaitForSeconds(_gameConstants.LineCompletionPauseSeconds);
        _isAwaitingRowCompletion = false;
    }

    private static ShiftDirection? DetermineShiftDirection(bool inputtingRight, bool inputtingLeft, bool inputtingDown)
    {
        if (MoreThanOneShiftInput(inputtingRight, inputtingLeft, inputtingDown)
            || NoShiftInput(inputtingRight, inputtingLeft, inputtingDown))
        {
            return null;
        }

        if (inputtingRight) return ShiftDirection.Right;
        if (inputtingLeft) return ShiftDirection.Left;
        if (inputtingDown) return ShiftDirection.Down;
        throw new InvalidProgramException("Bug! Should not be possible to reach this!");
    }

    private static bool MoreThanOneShiftInput(bool inputtingRight, bool inputtingLeft, bool inputtingDown)
    {
        return !(inputtingRight ^ inputtingLeft ^ inputtingDown);
    }

    private static bool NoShiftInput(bool inputtingRight, bool inputtingLeft, bool inputtingDown)
    {
        return !inputtingRight && !inputtingLeft && !inputtingDown;
    }
}
