using Com.IsartDigital.Sokoban.Tools;
using Godot;

// Author : Aidan Bachelez

namespace Com.IsartDigital.Sokoban
{
    [Tool]
    public partial class StackedObject : Node2D
    {
        [Export] public Texture2D spriteSheet;
        [Export] public Material material;
        [Export] private int nbSpritesInSpriteSheet;
        [Export] private int nbSpriteMultiplyer = 10;
        [Export] private float verticalSpacing;
        [Export] public float objectAngle;

        [ExportGroup("Editor Only")]
        [Export(PropertyHint.Range, "0.1,2")] private float cameraYZoom = 1;

        private const int MIN_SPRITE_HEIGHT = 1; // Must not be changed, otherwise the sprites couldn't be created
        private const int MAX_SPRITE_HEIGHT = 3;

        /// <summary>
        /// Corresponds to the number of time the sprite is rendered
        /// </summary>
        private float spritesHeight = 1;

        public override void _Ready()
        {
            StackSprites();
        }

        public override void _Process(double pDelta)
        {
            // Only for the editor view
            if (Engine.IsEditorHint())
            {
                if (GetChildCount() > 0)
                {
                    Vector2 lComparePos = GetViewportRect().Size / 2;
                    Vector2 lCameraUp = Vector2.Up;
                    float lDepth = (GlobalPosition - lComparePos).Dot(lCameraUp); // Using dot product to compare the direction and distance at once.
                    ZIndex = -(int)lDepth;

                    Sprite2D lSprite;
                    foreach (Node lChild in GetChildren())
                    {
                        lSprite = (Sprite2D)lChild;
                        lSprite.Rotation = objectAngle;
                    }

                    spritesHeight = (1 - cameraYZoom) * nbSpriteMultiplyer;
                    spritesHeight = (int)Mathf.Clamp(spritesHeight, MIN_SPRITE_HEIGHT, MAX_SPRITE_HEIGHT);
                }

                StackSprites();
                return;
            }

            Camera2D lCamera = GetViewport().GetCamera2D();

            if (GetChildCount() > 0)
            {
                bool lIsStackCam = lCamera is StackCamera; // Used in case we do not have a StackCamera

                Vector2 lComparePos = lIsStackCam ? lCamera.GlobalPosition : GetViewportRect().Size / 2;
                Vector2 lCameraUp = Vector2.Up.Rotated(lIsStackCam ? lCamera.Rotation : 0);
                float lDepth = (GlobalPosition - lComparePos).Dot(lCameraUp); // Using dot product to compare the direction and distance at once.
                ZIndex = -(int)lDepth;

                if (lIsStackCam) Rotation = lCamera.Rotation; // The object faces the camera

                foreach (Node lChild in GetChildren())
                    ((Sprite2D)lChild).Rotation = objectAngle - (lIsStackCam ? lCamera.Rotation : 0);

                spritesHeight = (1 - lCamera.Zoom.Y) * nbSpriteMultiplyer;
                spritesHeight = (int)Mathf.Clamp(spritesHeight, MIN_SPRITE_HEIGHT, MAX_SPRITE_HEIGHT);

            }

            StackSprites();
        }

        private void StackSprites()
        {
            float lPreviousRotation = 0;
            if (GetChildCount() > 0) // To avoid creating sprite facing the camera when rotation on pitch
            {
                lPreviousRotation = ((Sprite2D)GetChild(0)).Rotation;
                foreach (Node lChild in GetChildren())
                    lChild.QueueFree();
            }

            Sprite2D lSprite;
            int j;
            int lHeightFactor = 0;
            int lSpriteHeight = Mathf.CeilToInt(spritesHeight) * nbSpriteMultiplyer;

            for (int i = 0; i < nbSpritesInSpriteSheet; i++)
            {
                for (j = 0; j < lSpriteHeight; j++)
                {
                    lSprite = new Sprite2D
                    {
                        Texture = spriteSheet,
                        Hframes = nbSpritesInSpriteSheet,
                        Frame = i,
                        Position = Vector2.Up * (verticalSpacing * lHeightFactor),
                        Rotation = lPreviousRotation,
                        Material = material
                    };

                    lHeightFactor++;
                    CallDeferred(Utils.ADD_CHILD, lSprite);
                }
            }
        }
    }
}
