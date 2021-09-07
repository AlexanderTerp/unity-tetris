using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayAreaController : MonoBehaviour
{

    public event Action<BlockTransformation> BlockTransformationEvent;
    public event Action BlockPlacedEvent;

    private Dictionary<Coordinate, GameCell> _cellsByCoordinate;
    private DimensionsHandler _dimensions;

    void Start()
    {
        _cellsByCoordinate = GetComponent<PlayAreaSetupper>().InitializeGameCells();
        _dimensions = GetComponent<DimensionsHandler>();
    }

    public void AddBlock(Block currentBlock)
    {
        new List<Coordinate>(currentBlock.PiecesByCoordinate.Keys)
            .ForEach(coordinate => _cellsByCoordinate[coordinate].BlockPiece = currentBlock.PiecesByCoordinate[coordinate]);
    }

    public void TryMove(Block currentBlock, int xShift, int yShift)
    {
        if (xShift != 0 && yShift != 0)
        {
            throw new InvalidOperationException("Cannot move diagonally!");
        }

        BlockTransformation blockTransformation = currentBlock.CalculateLinearTransformation(xShift, yShift);

        if (!IsValidPlacement(blockTransformation.Block, new List<Coordinate>(blockTransformation.OldToNewCoordinates.Values)))
        {
            if (IsDownwardsMovement(yShift))
            {
                PlaceBlock(currentBlock);
            }
        }
        else if (blockTransformation.IsValid())
        {
            ShiftBlock(currentBlock, blockTransformation);
        }
    }

    public void TryRotate(Block currentBlock, RotationDirection rotationDirection)
    {
        BlockTransformation blockTransformation =
            currentBlock.CalculateRotatedCoordinates(currentBlock, rotationDirection, coordinates => IsValidPlacement(currentBlock, coordinates));
        if (!blockTransformation.IsValid()) return;
        currentBlock.PerformTransformation(blockTransformation);
        UpdateCellTable(blockTransformation.OldToNewCoordinates);
        EventUtil.SafeInvoke(BlockTransformationEvent, blockTransformation);
    }

    private void ShiftBlock(Block currentBlock, BlockTransformation blockTransformation)
    {
        currentBlock.PerformTransformation(blockTransformation);
        UpdateCellTable(blockTransformation.OldToNewCoordinates);
        EventUtil.SafeInvoke(BlockTransformationEvent, blockTransformation);
    }

    private void UpdateCellTable(Dictionary<Coordinate, Coordinate> oldToNewCoordinateDict)
    {
        Dictionary<Coordinate, BlockPiece> piecesByNewCoordinate = new Dictionary<Coordinate, BlockPiece>();

        foreach (Coordinate oldCoordinate in oldToNewCoordinateDict.Keys)
        {
            GameCell gameCellAtOldCoordinate = _cellsByCoordinate[oldCoordinate];
            BlockPiece? blockPieceToMove = gameCellAtOldCoordinate.BlockPiece;

            if (blockPieceToMove == null) continue;

            gameCellAtOldCoordinate.BlockPiece = null;
            piecesByNewCoordinate.Add(oldToNewCoordinateDict[oldCoordinate], blockPieceToMove);
        }

        foreach (Coordinate newCoordinate in piecesByNewCoordinate.Keys)
        {
            _cellsByCoordinate[newCoordinate].BlockPiece = piecesByNewCoordinate[newCoordinate];
        }
    }

    private bool CoordinateWillStillContainBlockPiece(Coordinate oldCoordinate, Dictionary<Coordinate, Coordinate> oldToShiftedCoordinateDict)
    {
        return oldToShiftedCoordinateDict.Values.Contains(oldCoordinate);
    }

    private void PlaceBlock(Block currentBlock)
    {
        currentBlock.IsPlaced = true;
        // todo run line-completion logic
        EventUtil.SafeInvoke(BlockPlacedEvent);
    }

    private bool IsValidPlacement(Block currentBlock, List<Coordinate> potentialNewCoordinates)
    {
        return AreWithinBounds(potentialNewCoordinates) && CoordinatesAreOpen(currentBlock, potentialNewCoordinates);
    }

    private bool AreWithinBounds(List<Coordinate> shiftedCoordinates)
    {
        return shiftedCoordinates.TrueForAll(coordinate => coordinate.X < _dimensions.NumberXCells
            && coordinate.X >= 0
            && coordinate.Y < _dimensions.NumberYCells
            && coordinate.Y >= 0);
    }

    private bool CoordinatesAreOpen(Block currentBlock, List<Coordinate> shiftedCoordinates)
    {
        return shiftedCoordinates.TrueForAll(coordinate =>
            _cellsByCoordinate[coordinate].IsEmpty() || currentBlock.PiecesByCoordinate.ContainsKey(coordinate));
    }

    private static bool IsDownwardsMovement(int yShift)
    {
        return yShift > 0;
    }

    private static List<Coordinate> CalculateShiftedCoordinates(List<Coordinate> coordinatesToShift, int xShift, int yShift)
    {
        return coordinatesToShift.Select(coordinate => coordinate.Shifted(xShift, yShift)).ToList();
    }
}
