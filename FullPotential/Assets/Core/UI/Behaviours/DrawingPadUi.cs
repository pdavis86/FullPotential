using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.UI.Behaviours
{
    public class DrawingPadUi : MonoBehaviour
    {
        // ReSharper disable InconsistentNaming
        private const float _lineThickness = 1f;
        private const float _minDistanceToDraw = .1f;
        // ReSharper restore InconsistentNaming

#pragma warning disable 0649
        [SerializeField] private Material _drawMaterial;
#pragma warning restore 0649

        private CanvasRenderer _canvasRenderer;
        private Vector3 _normal2DUp;
        private Vector3 _normal2DDown;
        private Mesh _mesh;
        private Vector3 _lastMousePosition;
        private bool _isDrawing;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _canvasRenderer = GetComponent<CanvasRenderer>();
            _canvasRenderer.SetMaterial(_drawMaterial, null);

            _normal2DUp = new Vector3(0, 0, -1);
            _normal2DDown = _normal2DUp * -1;
        }

        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            Draw();
        }

        public void StartDrawing()
        {
            _lastMousePosition = GetCurrentMousePosition();

            SetNewMesh();

            var vertices = new Vector3[4];
            vertices[0] = new Vector3(_lastMousePosition.x, _lastMousePosition.y);
            vertices[1] = new Vector3(_lastMousePosition.x, _lastMousePosition.y);
            vertices[2] = new Vector3(_lastMousePosition.x, _lastMousePosition.y);
            vertices[3] = new Vector3(_lastMousePosition.x, _lastMousePosition.y);

            var triangles = new int[6];

            triangles[0] = 0;
            triangles[1] = 3;
            triangles[2] = 1;

            triangles[3] = 1;
            triangles[4] = 3;
            triangles[5] = 2;

            _mesh.vertices = vertices;
            _mesh.uv = new Vector2[4];
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

            var currentMousePosition = GetCurrentMousePosition();

            if (Vector3.Distance(currentMousePosition, _lastMousePosition) < _minDistanceToDraw)
            {
                return;
            }

            var mouseForwardVector = (currentMousePosition - _lastMousePosition).normalized;

            //Debug.Log("mouseForwardVector " + mouseForwardVector);

            //Debug.Log("Drawing at " + currentMousePosition);

            var newVertices = new Vector3[_mesh.vertices.Length + 2];
            var newTriangles = new int[_mesh.triangles.Length + 6];

            _mesh.vertices.CopyTo(newVertices, 0);
            _mesh.triangles.CopyTo(newTriangles, 0);

            var newVertexUp = currentMousePosition + Vector3.Cross(mouseForwardVector, _normal2DUp) * _lineThickness;
            var newVertexDown = currentMousePosition + Vector3.Cross(mouseForwardVector, _normal2DDown) * _lineThickness;

            newVertices[newVertices.Length - 2] = newVertexUp;
            newVertices[newVertices.Length - 1] = newVertexDown;

            //NOTE: Vertices must be added in a clockwise order to face the camera

            newTriangles[newTriangles.Length - 6] = newVertices.Length - 4;
            newTriangles[newTriangles.Length - 5] = newVertices.Length - 2;
            newTriangles[newTriangles.Length - 4] = newVertices.Length - 3;

            newTriangles[newTriangles.Length - 3] = newVertices.Length - 3;
            newTriangles[newTriangles.Length - 2] = newVertices.Length - 2;
            newTriangles[newTriangles.Length - 1] = newVertices.Length - 1;

            _mesh.vertices = newVertices;
            _mesh.uv = new Vector2[_mesh.vertices.Length];
            _mesh.triangles = newTriangles;

            _lastMousePosition = currentMousePosition;

            UpdateMesh();
        }

        public void StopDrawing()
        {
            _isDrawing = false;
            SetNewMesh();
            UpdateMesh();
        }

        private void SetNewMesh()
        {
            _mesh = new Mesh();
            _mesh.MarkDynamic();
        }

        private void UpdateMesh()
        {
            _canvasRenderer.SetMesh(_mesh);
        }

        private Vector3 GetCurrentMousePosition()
        {
            Vector3 rawValue = Mouse.current.position.ReadValue();

            //Debug.Log("Raw value: " + rawValue);

            var halfWidth = Camera.main.pixelRect.width / 2;
            var halfHeight = Camera.main.pixelRect.height / 2;

            var vector = new Vector3(rawValue.x - halfWidth, rawValue.y - halfHeight);

            //Debug.Log("CurrentMousePosition: " + vector);

            return vector;
        }

    }
}
