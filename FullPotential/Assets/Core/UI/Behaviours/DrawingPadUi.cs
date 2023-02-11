using System;
using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Gameplay.Drawing;
using FullPotential.Api.Ioc;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Core.UI.Events;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.UI.Behaviours
{
    public class DrawingPadUi : MonoBehaviour
    {
        public event EventHandler<OnDrawingStopEventArgs> OnDrawingStop;

        private const int MarkerSize = 6;
        private const int MinimumMarkers = 4;
        private const int DistanceTolerance = 20;

        // ReSharper disable InconsistentNaming
        private const float _lineThickness = 1f;
        private const float _minDistanceToDraw = .1f;
        // ReSharper restore InconsistentNaming

#pragma warning disable 0649
        [SerializeField] private Material _drawMaterial;
        [SerializeField] private Sprite _markerSprite;
        [SerializeField] private int _directionAngleTolerance;
        [SerializeField] private int _circleAngleTolerance;
#pragma warning restore 0649

        //Services
        private IDrawingService _drawingService;

        //Other variables
        private CanvasRenderer _canvasRenderer;
        private Vector3 _normalMinusZ;
        private Vector3 _normalPlusZ;
        private Mesh _mesh;
        private Vector2 _lastMousePosition;
        private bool _isDrawing;
        private List<Vector2> _currentShapeMarkers;
        private bool _isDrawingCircle;
        private List<string> _drawnShapes;
        private string _eventSource;
        private string _itemId;
        private SlotGameObjectName? _slotGameObjectName;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _canvasRenderer = GetComponent<CanvasRenderer>();
            _canvasRenderer.SetMaterial(_drawMaterial, null);

            _normalMinusZ = new Vector3(0, 0, -1);
            _normalPlusZ = _normalMinusZ * -1;

            _currentShapeMarkers = new List<Vector2>();
            _drawnShapes = new List<string>();

            _drawingService = DependenciesContext.Dependencies.GetService<IDrawingService>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            Draw();
        }

        public void InitialiseForAssign(string eventSource, string itemId)
        {
            if (!_eventSource.IsNullOrWhiteSpace())
            {
                return;
            }

            _eventSource = eventSource;
            _itemId = itemId;
        }

        public void InitialiseForEquip(string eventSource, SlotGameObjectName slotGameObjectName)
        {
            if (!_eventSource.IsNullOrWhiteSpace())
            {
                return;
            }

            _eventSource = eventSource;
            _slotGameObjectName = slotGameObjectName;
        }

        public void StartDrawing()
        {
            var rawMousePosition = Mouse.current.position.ReadValue();
            _lastMousePosition = GetAdjustedMousePosition(rawMousePosition);

            _mesh = new Mesh();
            _mesh.MarkDynamic();

            var vertices = new Vector3[3];
            vertices[0] = new Vector3(_lastMousePosition.x, _lastMousePosition.y);
            vertices[1] = new Vector3(_lastMousePosition.x, _lastMousePosition.y);
            vertices[2] = new Vector3(_lastMousePosition.x, _lastMousePosition.y);

            var triangles = new int[3];

            triangles[0] = 0;
            triangles[1] = 2;
            triangles[2] = 1;

            _mesh.vertices = vertices;
            _mesh.uv = new Vector2[3];
            _mesh.triangles = triangles;

            UpdateMesh();

            _isDrawing = true;
        }

        private void Draw()
        {
            if (!_isDrawing)
            {
                return;
            }

            var rawMousePosition = Mouse.current.position.ReadValue();

            var adjustedMousePosition = GetAdjustedMousePosition(rawMousePosition);

            if (Vector2.Distance(adjustedMousePosition, _lastMousePosition) < _minDistanceToDraw)
            {
                return;
            }

            var mouseForwardVector = (adjustedMousePosition - _lastMousePosition).normalized;

            var newVertices = new Vector3[_mesh.vertices.Length + 2];
            var newTriangles = new int[_mesh.triangles.Length + 6];

            _mesh.vertices.CopyTo(newVertices, 0);
            _mesh.triangles.CopyTo(newTriangles, 0);

            var newVertexUp = (Vector3)adjustedMousePosition + Vector3.Cross(mouseForwardVector, _normalMinusZ) * _lineThickness;
            var newVertexDown = (Vector3)adjustedMousePosition + Vector3.Cross(mouseForwardVector, _normalPlusZ) * _lineThickness;

            newVertices[newVertices.Length - 2] = newVertexUp;
            newVertices[newVertices.Length - 1] = newVertexDown;

            //NOTE: Vertices must be added in a clockwise order to face the camera

            newTriangles[newTriangles.Length - 6] = newVertices.Length - 4; //OldUp
            newTriangles[newTriangles.Length - 5] = newVertices.Length - 2; //NewUp
            newTriangles[newTriangles.Length - 4] = newVertices.Length - 3; //OldDown

            newTriangles[newTriangles.Length - 3] = newVertices.Length - 3; //OldDown
            newTriangles[newTriangles.Length - 2] = newVertices.Length - 2; //NewUp
            newTriangles[newTriangles.Length - 1] = newVertices.Length - 1; //NewDown

            _mesh.vertices = newVertices;
            _mesh.uv = new Vector2[_mesh.vertices.Length];
            _mesh.triangles = newTriangles;

            _lastMousePosition = adjustedMousePosition;

            UpdateMesh();

            CheckForShape(rawMousePosition);
        }

        public void StopDrawing()
        {
            _isDrawing = false;

            _canvasRenderer.SetMesh(new Mesh());

            if (!_isDrawingCircle)
            {
                if (_currentShapeMarkers.Count >= MinimumMarkers)
                {
                    var direction = _currentShapeMarkers.Last() - _currentShapeMarkers.First();
                    RecordShapeDrawn(direction, GetLength());
                }
                //else
                //{
                //    Debug.Log("There was an uncompleted line");
                //}
            }
            //else
            //{
            //    Debug.Log("There was an uncompleted circle");
            //}

            _isDrawingCircle = false;

            ClearMarkers();

            var finalShape = string.Join("-", _drawnShapes);

            OnDrawingStop?.Invoke(this, new OnDrawingStopEventArgs(_eventSource, finalShape, _itemId, _slotGameObjectName));

            _drawnShapes.Clear();
            _eventSource = null;
            _itemId = null;
            _slotGameObjectName = null;
        }

        private void UpdateMesh()
        {
            _canvasRenderer.SetMesh(_mesh);
        }

        private Vector2 GetAdjustedMousePosition(Vector2 rawMousePosition)
        {
            var halfWidth = Camera.main.pixelRect.width / 2;
            var halfHeight = Camera.main.pixelRect.height / 2;

            var vector = new Vector2(rawMousePosition.x - halfWidth, rawMousePosition.y - halfHeight);

            return vector;
        }

        private void CreateMarker(Vector2 rawMousePosition)
        {
            var markerNumber = _currentShapeMarkers.Count + 1;

            var go = new GameObject("Marker " + markerNumber.ToString("D3"));

            go.transform.SetParent(transform);
            go.transform.position = rawMousePosition;

            var img = go.AddComponent<Image>();
            img.sprite = _markerSprite;

            go.GetComponent<RectTransform>().sizeDelta = new Vector2(MarkerSize, MarkerSize);

            _currentShapeMarkers.Add(rawMousePosition);
        }

        private void CheckForShape(Vector2 rawMousePosition)
        {
            if (_currentShapeMarkers.Count > 0
                && (_currentShapeMarkers.Last() - rawMousePosition).magnitude < DistanceTolerance)
            {
                return;
            }

            if (_currentShapeMarkers.Count < MinimumMarkers)
            {
                CreateMarker(rawMousePosition);
                return;
            }

            //if (_isDrawingCircle)
            //{
            //    if ((_currentShapeMarkers.First() - rawMousePosition).magnitude < DistanceTolerance)
            //    {
            //        RecordShapeDrawn(DrawShape.Circle);
            //        _isDrawingCircle = false;
            //        CreateMarker(rawMousePosition);
            //        return;
            //    }

            //    CreateMarker(rawMousePosition);
            //    return;
            //}

            //if (_currentShapeMarkers.Count == MinimumMarkers)
            //{
            //    var angle1 = Vector2.Angle(Vector2.up, _currentShapeMarkers[1] - _currentShapeMarkers[0]);
            //    var angle2 = Vector2.Angle(Vector2.up, _currentShapeMarkers[2] - _currentShapeMarkers[1]);
            //    var angle3 = Vector2.Angle(Vector2.up, _currentShapeMarkers[3] - _currentShapeMarkers[2]);

            //    var diff1 = Mathf.Abs(angle1 - angle2);
            //    var diff2 = Mathf.Abs(angle2 - angle3);
            //    var diff3 = Mathf.Abs(angle1 - angle3);

            //    //Debug.Log($"diff1:{diff1}, diff2:{diff2}, diff3:{diff3}");

            //    if (diff1 < _circleAngleTolerance
            //        && diff2 < _circleAngleTolerance
            //        && diff3 < _circleAngleTolerance)
            //    {
            //        //Debug.Log("Line");
            //    }
            //    else
            //    {
            //        //Debug.Log("_isDrawingCircle = true");
            //        _isDrawingCircle = true;
            //        CreateMarker(rawMousePosition);
            //        return;
            //    }
            //}

            var initialDirection = _currentShapeMarkers.ElementAt(1) - _currentShapeMarkers.First();
            var currentDirection = rawMousePosition - _currentShapeMarkers.ElementAt(_currentShapeMarkers.Count - 1);

            //When going downwards, treat -175 and 175 the same
            //When going upwards, treat -45 and 45 as different
            var lineAngle = currentDirection.y < 0 ? Vector2.Angle(Vector2.up, initialDirection) : Vector2.SignedAngle(Vector2.up, initialDirection);
            var markerAngle = currentDirection.y < 0 ? Vector2.Angle(Vector2.up, currentDirection) : Vector2.SignedAngle(Vector2.up, currentDirection);

            //Debug.Log($"lineAngle:{lineAngle}, markerAngle:{markerAngle}, diff:{Mathf.Abs(lineAngle - markerAngle)}");

            if (Mathf.Abs(lineAngle - markerAngle) > _directionAngleTolerance)
            {
                //Debug.Log("Over DirectionAngleTolerance");
                RecordShapeDrawn(initialDirection, GetLength());
                return;
            }

            CreateMarker(rawMousePosition);
        }

        private int GetLength()
        {
            return (_currentShapeMarkers.Count - 1) * DistanceTolerance;
        }

        private void ClearMarkers()
        {
            _currentShapeMarkers.Clear();
            gameObject.DeleteAllChildren();
        }

        private void RecordShapeDrawn(Vector2 direction, int length)
        {
            _drawnShapes.Add(_drawingService.GetDrawingCode(direction, length));

            //Debug.Log("That was a " + _drawnShapes.Last());

            ClearMarkers();
        }
    }
}
