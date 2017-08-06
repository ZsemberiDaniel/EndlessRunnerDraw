using System.Collections.Generic;
using UnityEngine;

public static class ShapeRecognizer {

    /// <summary>
    /// This function does every needed step to a list of points.
    /// </summary>
    public static List<Vector2> DoEverything(List<Vector2> points) {
        points = ResampleListToEquidistantPoints(points);
        points = TranslatePointsToOrigin(points);
        points = RotateListToZero(points, GetCentroidOfPoints(points));
        points = ScalePointsToSquare(points);

        return points;
    }

    /// <summary>
    /// Translates the list of points so it's centroid is the origin
    /// </summary>
    /// <param name="centroid">Pass it in if you have a precalculated centroid points</param>
    public static List<Vector2> TranslatePointsToOrigin(List<Vector2> points, Vector2? centroid = null) {
        List<Vector2> _newPoints = new List<Vector2>();

        Vector2 _centroid = centroid.HasValue ? centroid.Value : GetCentroidOfPoints(points);

        for (int i = 0; i < points.Count; i++)
            _newPoints.Add(points[i] - _centroid);

        return _newPoints;
    }
    /// <summary>
    /// Scales the list of points so it fits in a size*size square
    /// </summary>
    /// <param name="rect">Pass it in if you have a precalculated bounding rect</param>
    public static List<Vector2> ScalePointsToSquare(List<Vector2> points, float size = 1f, Rect? rect = null) {
        List<Vector2> _newPoints = new List<Vector2>();

        Rect _rect = rect.HasValue ? rect.Value : GetBoindgBoxOfPoints(points);
        float _scale = Mathf.Min(size / _rect.width, size / _rect.height);

        for (int i = 0; i < points.Count; i++) {
            _newPoints.Add(new Vector2(
                points[i].x * _scale,
                points[i].y * _scale
            ));
        }

        return _newPoints;
    }
    /// <summary>
    /// Rotates the list of points so the first point is looking towards zero (-> this way)
    /// </summary>
    /// <param name="centroid">Pass it in if you have a precalculated centroid points</param>
    public static List<Vector2> RotateListToZero(List<Vector2> points, Vector2? centroid = null) {
        // We have no points to rotate
        if (points == null || points.Count == 0) return null;

        // calculate centroid of our shape
        Vector2 _centroid = centroid.HasValue ? centroid.Value : GetCentroidOfPoints(points);

        // what angle to rotate by so the first point is at zero (-> this way)
        float _dY = points[0].y - _centroid.y;
        float _dX = points[0].x - _centroid.x;
        float _radToRotate = Mathf.Atan(_dY / _dX);

        if (_dX < 0) _radToRotate += Mathf.PI;

        return RotateListToAngle(points, -_radToRotate, _centroid);
    }
    /// <summary>
    /// Rotates list to given angle. The given angle has to be in radian!
    /// </summary>
    /// <param name="centroid">Pass it in if you have a precalculated centroid points</param>
    public static List<Vector2> RotateListToAngle(List<Vector2> points, float rad, Vector2? centroid = null) {
        List<Vector2> _newPoints = new List<Vector2>();

        // used for further calculation
        float _cosOfAngle = Mathf.Cos(rad);
        float _sinOfAngle = Mathf.Sin(rad);

        // calculate centroid of our shape
        Vector2 _centroid = centroid.HasValue ? centroid.Value : GetCentroidOfPoints(points);

        // rotate each point by radToRotate
        for (int i = 0; i < points.Count; i++) {
            _newPoints.Add(new Vector2(
                _centroid.x + (points[i].x - _centroid.x) * _cosOfAngle - (points[i].y - _centroid.y) * _sinOfAngle,
                _centroid.y + (points[i].x - _centroid.x) * _sinOfAngle + (points[i].y - _centroid.y) * _cosOfAngle
           ));
        }

        return _newPoints;
    }
    /// <summary>
    /// Resamples the given points list so it has pointCount points equidistant from each other
    /// </summary>
    public static List<Vector2> ResampleListToEquidistantPoints(List<Vector2> points, int pointCount = 64) {
        // We have no points to resample
        if (points == null || points.Count == 0) return null;

        List<Vector2> _newPoints = new List<Vector2>();
        _newPoints.Add(points[0]);

        // the new distance between the points (somOfDistance / (_pointCount - 1))
        float _equidistance = 0f;
        for (int i = 1; i < points.Count; i++) _equidistance += (points[i] - points[i - 1]).magnitude;
        _equidistance /= pointCount - 1;

        // the distance so far 
        float _distSoFar = 0f;

        for (int i = 1; i < points.Count; i++) {
            float _d = (points[i] - points[i - 1]).magnitude;

            // we need a/more new point/points
            if (_distSoFar + _d >= _equidistance) {
                // we have more points on this segment than 1
                if ((_distSoFar + _d) / _equidistance >= 2) {
                    // how many points have been generated on this segment
                    int _pc = 0;

                    // add points until we can't add more without going through points[i]
                    do {
                        _pc++;
                        // add a point (_equidistance * _pc - _distSoFar) distance away from points[i-1]
                        _newPoints.Add(points[i - 1] + (_equidistance * _pc - _distSoFar) / _d * (points[i] - points[i - 1]));
                    } while (_equidistance * (_pc + 1) < _distSoFar + _d);

                    // this is how much distance we should have left after adding the points
                    _distSoFar = _d + _distSoFar - _equidistance * _pc;
                } else {
                    _newPoints.Add(points[i - 1] + (_equidistance - _distSoFar) / _d * (points[i] - points[i - 1]));

                    _distSoFar += _d - _equidistance;
                }
            } else {
                _distSoFar += _d;
            }
        }

        return _newPoints;
    }

    /// <summary>
    /// The centroid points of the given shape
    /// </summary>
    public static Vector2 GetCentroidOfPoints(List<Vector2> points) {
        // Stole this right off wikipedia so not exactly sure how it works
        Vector2 _centroid = new Vector2();
        float _area = 0f;
        float _a;

        int i;
        for (i = 0; i < points.Count - 1; i++) {
            _a = points[i].x * points[i + 1].y - points[i + 1].x * points[i].y;
            _area += _a;
            _centroid.x += (points[i].x + points[i + 1].x) * _a;
            _centroid.y += (points[i].y + points[i + 1].y) * _a;
        }

        // Do last vertex separately to avoid performing an expensive
        // modulus operation in each iteration.
        _a = points[i].x * points[0].y - points[0].x * points[i].y;
        _area += _a;
        _centroid.x += (points[i].x + points[0].x) * _a;
        _centroid.y += (points[i].y + points[0].y) * _a;

        _area *= 0.5f;
        _centroid /= 6f * _area;

        return _centroid;
    }
    /// <summary>
    /// The bounding box of the given shape
    /// </summary>
    public static Rect GetBoindgBoxOfPoints(List<Vector2> points) {
        Vector2 _min = points[0];
        Vector2 _max = points[0];

        for (int i = 1; i < points.Count; i++) {
            if (points[i].x < _min.x) _min.x = points[i].x;
            if (points[i].y > _max.x) _max.x = points[i].x;

            if (points[i].y < _min.y) _min.y = points[i].y;
            if (points[i].y > _max.y) _max.y = points[i].y;
        }

        Vector2 _size = _max - _min;
        _size.x = _size.x == 0f ? 0.1f : _size.x;
        _size.y = _size.y == 0f ? 0.1f : _size.y;
        return new Rect(_min, _size);
    }
}
