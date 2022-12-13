using BeeEngine.Vector;

namespace BeeEngine.Drawing
{
    public sealed class Transform
    {
        //internal Matrix transformMatrix = new Matrix();
        //public Vector2 ZeroCoords { get; private set; } = Vector2.Zero();
        private Vector3 _position = Vector3.Zero();
        private Vector3 _eulerAngles = Vector3.Zero();
        private Vector3 _scale = Vector3.One();

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }
        public Vector3 EulerAngles
        {
            get { return _eulerAngles; }
            set { _eulerAngles = value; }
        }
        public Vector3 Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
            }
        }
        public void Translate(Vector3 a)
        {
            this._position += a;
        }
        public GameObject ParentObject { get; private set; }

        public float X => _position.X;
        public float Y => _position.Y;
        public float Z => _position.Z;
        public float ScaleX => _scale.X;
        public float ScaleY => _scale.Y;
        public float ScaleZ => _scale.Z;

        public Transform(GameObject parentObject)
        {
            ParentObject = parentObject ?? throw new ArgumentNullException("parentObject");
        }
        private Transform()
        {

        }

        internal Transform(Transform transform, GameObject gameObject): this(gameObject)
        {
            _eulerAngles = transform._eulerAngles;
            _scale = transform._scale;
            _position = transform._position;
        }
        internal static Transform Empty
        {
            get
            {
                Transform transform = new Transform();
                return transform;
            }
        }

        /*public Transform(BaseDrawnableObject parentObject, Vector2 ZeroCoords):this(parentObject)
        {
            this.ZeroCoords = ZeroCoords;
            transformMatrix.Translate(ZeroCoords.X, ZeroCoords.Y);
        }*/
        /*public Transform(DrawnableObject parentObject, Vector2 Position, Vector2 Scale, float angle) : this(parentObject)
        {
            transformMatrix.Scale(Scale.X, Scale.Y);
            transformMatrix.Rotate(angle);
            transformMatrix.Translate(Position.X, Position.Y);
        }*/
        public void Translate(float x, float y)
        {
            _position.X += x;
            _position.Y += y;
        }

        public void Translate(Vector2 translationVector)
        {
            _position.X += translationVector.X * _scale.X;
            _position.Y += translationVector.Y * _scale.Y;
        }

        public void ScaleTransform(float x, float y)
        {
            _scale.X += x;
            _scale.Y += y;
        }
        public void ScaleTransform(float x)
        {
            _scale.X = x;
            _scale.Y = x;
        }
        public void ScaleTransform(Vector2 scalingVector)
        {
            _scale.X += scalingVector.X;
            _scale.Y += scalingVector.Y;
        }

        public void RotateTransform(float angle)
        {
            _eulerAngles.X += angle;
        }

        public void Reset()
        {
            ResetPosition();
            ResetScale();
            ResetRotation();
        }
        public void ResetPosition()
        {
            _position = Vector3.Zero();
        }
        public void ResetScale()
        {
            _scale = Vector3.One();
        }
        public void ResetRotation()
        {
            _eulerAngles = Vector3.Zero();
        }
        /*public void Multiply(Matrix matrix)
        {
            transformMatrix.Multiply(matrix);
        }*/
        public void Multiply(Transform transform)
        {
            Translate(transform.Position);
            ScaleTransform(transform.Scale);
            RotateTransform(transform.EulerAngles);
        }

        private void RotateTransform(Vector3 eulerAngles)
        {
            _eulerAngles += eulerAngles;
        }

        private void ScaleTransform(Vector3 scale)
        {
            _scale += scale;
        }
    }
}
