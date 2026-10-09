using System.Collections.Generic;
using Godot;

// Author : Aidan Bachelez

namespace Com.IsartDigital.Sokoban
{
    public partial class StackCamera : Camera2D
    {
        private static StackCamera instance;

        public static StackCamera GetInstance()
        {
            if (instance == null) instance = new StackCamera();
            return instance;
        }

        [Export] private float rotationSpeed = 50;
        public bool canRotate = true;

        private const float DEFAULT_Y_ZOOM = 0.7f;
        private const float ROTATION_STEP = Mathf.Pi * 0.5f;

        private float targetRotation;
        public float TargetRotation => targetRotation;

        public override void _Ready()
        {
            if (instance != null)
            {
                QueueFree();
                GD.Print(nameof(StackCamera) + " INSTANCE ALREADY EXISTS, DESTROYING THE LAST ADDED");
                return;
            }

            instance = this;
            base._Ready();

            Reset();
            InputManager lInput = InputManager.GetInstance();
            lInput.OnRotateCameraInput += CameraRotation;
            lInput.onSizeChanged += Reset;
        }

        public override void _Process(double pDelta)
        {
            float lDelta = (float)pDelta;
            Rotation = Mathf.LerpAngle(Rotation, targetRotation, lDelta * rotationSpeed);
        }

        public void SetZoomLevel(float pZoomLevel)
        {
            pZoomLevel = Mathf.Min(pZoomLevel, 1.5f);
            Zoom = new Vector2(pZoomLevel, pZoomLevel * DEFAULT_Y_ZOOM);
        }

        public void Reset()
        {
            canRotate = true;
            targetRotation = 0;
            Rotation %= Mathf.Tau;

            List<List<Node2D>> lStaticGrid = GridManager.staticGrid;
            int lYOffset = lStaticGrid.Count % 2 == 0 ? 0 : GridManager.HALF_SPRITE_SIZE;
            int lXOffset = lStaticGrid[lStaticGrid.Count / 2].Count % 2 == 0 ? 0 : GridManager.HALF_SPRITE_SIZE;

            GlobalPosition = GridManager.GetInstance().GetMiddle() + new Vector2(lXOffset, lYOffset);
        }

        public void CameraRotation(int pAxis)
        {
            // Rotates of 90°
            if (!canRotate) return;
            targetRotation += -pAxis * ROTATION_STEP;
        }

        protected override void Dispose(bool pDisposing)
        {
            InputManager lInput = InputManager.GetInstance();
            lInput.OnRotateCameraInput -= CameraRotation;
            lInput.onSizeChanged -= Reset;
            instance = null;
            base.Dispose(pDisposing);
        }
    }
}
