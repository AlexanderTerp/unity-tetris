using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coordinate
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public Transform? Transform { get; private set; }

    private Vector2Int _vector2IntRepresentation;
    private IOriginProvider _originProvider;

    public static Coordinate operator +(Coordinate a) => a;
    public static Coordinate operator -(Coordinate a) => new Coordinate(-a.X, -a.Y, a._originProvider, a.Transform);
    public static Coordinate operator +(Coordinate a, Coordinate b) => new Coordinate(a.X + b.X, a.Y + b.Y, a._originProvider);
    public static Coordinate operator -(Coordinate a, Coordinate b) => a + (-b);

    public Coordinate(int x, int y, IOriginProvider originProvider)
    {
        X = x;
        Y = y;
        _originProvider = originProvider;
        originProvider.OriginChangeEvent += OnOriginChange;
        Transform = null;
        _vector2IntRepresentation = new Vector2Int(x, y);
    }

    public Coordinate(int x, int y, IOriginProvider originProvider, Transform transform)
    {
        X = x;
        Y = y;
        _originProvider = originProvider;
        originProvider.OriginChangeEvent += OnOriginChange;
        Transform = transform;
        _vector2IntRepresentation = new Vector2Int(x, y);
    }

    public Coordinate XShifted(int xShift)
    {
        return Shifted(xShift, 0);
    }

    public Coordinate YShifted(int yShift)
    {
        return Shifted(0, yShift);
    }

    public Coordinate Shifted(int xShift, int yShift)
    {
        return new Coordinate(X + xShift, Y + yShift, _originProvider, Transform);
    }

    /**
    Equation = R * ([[x], [-y]] - [[Ox], [-Oy]]) + [[Ox], [-Oy]]
    Where R = Clockwise rotation matrix: [[-cos(x), sin(x)], [-sin(x), -cos(x)]] = [[0, 1], [-1, 0]]
    x = X coodinate of what's rotated.
    y = Y coordinate of what's rotated (where +y is up, which is opposite of how we model things, hence the -)
    Ox = X origin offset (pivot x)
    Oy = Y origin offset (pivot y)

    Final component equations after expanding:
    X = -y + Oy + Ox
    Y = -x + Ox - Oy
    */
    public Coordinate Rotated(Vector2 pivotOffset)
    {
        return new Coordinate(Mathf.RoundToInt(-Y + pivotOffset.y + pivotOffset.x),
            -Mathf.RoundToInt(-X + pivotOffset.x - pivotOffset.y), _originProvider, Transform);
    }

    public void UpdateTransform()
    {
        if (Transform == null) return;
        Transform.position = new Vector3(_originProvider.GetX() + X * Transform.localScale.x, _originProvider.GetY() - Y * Transform.localScale.y);
    }

    public Vector2Int AsVector2Int()
    {
        return _vector2IntRepresentation;
    }

    public override string ToString()
    {
        return String.Format("C({0}, {1})", X, Y);
    }

    public override bool Equals(object obj)
    {
        return obj is Coordinate coordinate &&
               X == coordinate.X &&
               Y == coordinate.Y;
    }

    public override int GetHashCode()
    {
        return (X, Y).GetHashCode();
    }

    private void OnOriginChange()
    {
        UpdateTransform();
    }
}
