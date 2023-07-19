using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

// Pixel Perfect Collider
// Use texture as collision mask, check each pixel data
// In I Wanna Fangame, this collision method is usually adopted

public class PixelPerfectCollider : MonoBehaviour
{
    private MaskData maskData;
    private SpriteRenderer maskRenderer;

    private int Left { get => maskData.left; }
    private int Right { get => maskData.right; }
    private int Top { get => maskData.top; }
    private int Bottom { get => maskData.bottom; }

    private bool[,] BoolData { get => maskData.boolData; }

    private bool[,] ScaledBoolData
    {
        get
        {
            if (!enableChangeScale)
            {
                return scaledBoolDataStart;
            }
            else
            {
                return GetScaledBoolData(maskData.boolData, XScale, YScale);
            }
        }
    }

    private bool[,] scaledBoolDataStart;

    private int Width { get => maskData.width; }
    private int Height { get => maskData.height; }

    private float XPos { get => _transform.position.x; }
    private float YPos { get => _transform.position.y; }

    private int XPivot { get => (int)(maskRenderer.sprite.pivot.x); }
    private int YPivot { get => (int)(maskRenderer.sprite.pivot.y); }

    private float XScale
    {
        get
        {
            if (!enableChangeScale)
            {
                return xScaleStart;
            }
            else
            {
                return _transform.localScale.x;
            }
        }
    }

    private float xScaleStart;

    private float YScale
    {
        get
        {
            if (!enableChangeScale)
            {
                return yScaleStart;
            }
            else
            {
                return _transform.localScale.y;
            }
        }
    }

    private float yScaleStart;

    private float Rotation
    {
        get
        {
            if (!enableChangeRotation)
            {
                return rotationStart;
            }
            else
            {
                return _transform.rotation.eulerAngles.z;
            }
        }
    }

    private float rotationStart;

    private Transform _transform;

    public bool enableSpriteAnimator = false;

    // Check if the Scale may change from the start
    public bool enableChangeScale = false;

    // Check if the Rotation may change from the start
    public bool enableChangeRotation = false;

    private SpriteAnimator animator;

    private void Start()
    {
        _transform = transform;

        xScaleStart = _transform.localScale.x;
        yScaleStart = _transform.localScale.y;
        rotationStart = _transform.rotation.eulerAngles.z;

        maskRenderer = GetComponent<SpriteRenderer>();
        Texture2D texture = maskRenderer.sprite.texture;
        Rect rect = maskRenderer.sprite.rect;

        // Get mask data
        if (!enableSpriteAnimator)
        {
            if (!World.instance.maskDataManager.ContainsKey(texture))
            {
                var maskData = GetMaskData(texture, rect);

                // Add to mask data manager to ensure we don't load repeatedly
                World.instance.maskDataManager[texture] = maskData;
            }
            else
            {
                // Load data directly from the mask data manager
                maskData = World.instance.maskDataManager[texture];
            }
        }
        else
        {
            animator = gameObject.GetComponent<SpriteAnimator>();
            foreach (var i in animator.animations)
            {
                foreach (var j in i.sprites)
                {
                    texture = j.texture;

                    if (!World.instance.maskDataManager.ContainsKey(texture))
                    {
                        var maskData = GetMaskData(texture, rect);

                        // Add to mask data manager to ensure we don't load repeatedly
                        World.instance.maskDataManager[texture] = maskData;
                    }
                    else
                    {
                        // Load data directly from the mask data manager
                        maskData = World.instance.maskDataManager[texture];
                    }
                }
            }
        }

        scaledBoolDataStart = GetScaledBoolData(BoolData, XScale, YScale);

        // Add to colliders
        if (gameObject.tag == null)
            Debug.LogWarning($"Pixel perfect collidable game object \"{gameObject.name}\" is using a empty string tag !");

        if (!World.instance.colliders.ContainsKey(gameObject.tag))
        {
            World.instance.colliders[gameObject.tag] = new List<PixelPerfectCollider>();
        }
        World.instance.colliders[gameObject.tag].Add(this);
    }

    public bool PlaceMeeting(float x, float y, string tag)
    {
        return InstancePlace(x, y, tag) != null;
    }

    public GameObject InstancePlace(float x, float y, string tag)
    {
        if (!World.instance.colliders.ContainsKey(tag))
            return null;

        var cders = World.instance.colliders[tag];
        if (cders.Count == 0)
            return null;

        if (enableSpriteAnimator)
            maskData = World.instance.maskDataManager[maskRenderer.sprite.texture];

        var x1 = x;
        var y1 = y;

        // Get self bounding box with transform
        GetBoundingBox(Left, Right, Top, Bottom, out var left1, out var right1, out var top1, out var bottom1,
            x1, y1, XScale, YScale, Rotation);

        // For each colliders
        foreach (var col in cders)
        {
            if (col == this) // Don't check himself
                continue;

            if (col.enableSpriteAnimator)
                col.maskData = World.instance.maskDataManager[col.maskRenderer.sprite.texture];

            var x2 = col.XPos;
            var y2 = col.YPos;

            // Get other bounding box with transform
            GetBoundingBox(col.Left, col.Right, col.Top, col.Bottom, out var left2, out var right2, out var top2, out var bottom2,
                x2, y2, col.XScale, col.YScale, col.Rotation);

            // Get intersection
            int iLeft = Max(left1, left2);
            int iRight = Min(right1, right2);
            int iBottom = Max(bottom1, bottom2);
            int iTop = Min(top1, top2);

            if (iLeft > iRight || iBottom > iTop)
                continue;

            // Check each pixel
            if (Rotation == 0 && col.Rotation == 0)
            {
                // Not rotated
                var xo1 = XPivot;
                var yo1 = YPivot;

                var xo2 = col.XPivot;
                var yo2 = col.YPivot;

                for (int yy = iBottom; yy <= iTop; yy++)
                {
                    for (int xx = iLeft; xx <= iRight; xx++)
                    {
                        var px1 = RoundToInt((xx - x1) + xo1 * XScale);
                        var py1 = RoundToInt((yy - y1) + yo1 * YScale);
                        var p1 = px1 >= 0 && py1 >= 0 && px1 < Width * XScale && py1 < Height * YScale && ScaledBoolData[px1, py1];

                        var px2 = RoundToInt((xx - x2) + xo2 * col.XScale);
                        var py2 = RoundToInt((yy - y2) + yo2 * col.YScale);
                        var p2 = px2 >= 0 && py2 >= 0 && px2 < col.Width * col.XScale && py2 < col.Height * col.YScale && col.ScaledBoolData[px2, py2];

                        if (p1 && p2)
                        {
                            return col.gameObject;
                        }
                    }
                }
            }
            else
            {
                // Rotated
                var sina1 = Sin(-Rotation * Deg2Rad);
                var cosa1 = Cos(-Rotation * Deg2Rad);
                var xo1 = XPivot;
                var yo1 = YPivot;

                var sina2 = Sin(-col.Rotation * Deg2Rad);
                var cosa2 = Cos(-col.Rotation * Deg2Rad);
                var xo2 = col.XPivot;
                var yo2 = col.YPivot;

                for (int yy = iBottom; yy <= iTop; yy++)
                {
                    for (int xx = iLeft; xx <= iRight; xx++)
                    {
                        var lx1 = xx - x1;
                        var ly1 = yy - y1;
                        RotateAround(lx1, ly1, 0, 0, sina1, cosa1, out var lx1a, out var ly1a);
                        var px1 = RoundToInt(lx1a + xo1 * XScale);
                        var py1 = RoundToInt(ly1a + yo1 * YScale);
                        var p1 = px1 >= 0 && py1 >= 0 && px1 < Width * XScale && py1 < Height * YScale && ScaledBoolData[px1, py1];

                        var lx2 = xx - x2;
                        var ly2 = yy - y2;
                        RotateAround(lx2, ly2, 0, 0, sina2, cosa2, out var lx2a, out var ly2a);
                        var px2 = RoundToInt(lx2a + xo2 * col.XScale);
                        var py2 = RoundToInt(ly2a + yo2 * col.YScale);
                        var p2 = px2 >= 0 && py2 >= 0 && px2 < col.Width * col.XScale && py2 < col.Height * col.YScale && col.ScaledBoolData[px2, py2];

                        if (p1 && p2)
                            return col.gameObject;
                    }
                }
            }
        }
        return null;
    }

    public bool[,] GetScaledBoolData(bool[,] boolData, float xScale, float yScale)
    {
        bool[,] scaledBoolData = new bool[CeilToInt(boolData.GetLength(0) * xScale), CeilToInt(boolData.GetLength(1) * yScale)];

        for (int xx = 0; xx < scaledBoolData.GetLength(0); xx++)
        {
            for (int yy = 0; yy < scaledBoolData.GetLength(1); yy++)
            {
                scaledBoolData[xx, yy] = boolData[FloorToInt(xx / xScale), FloorToInt(yy / yScale)];
            }
        }
        return scaledBoolData;
    }

    public void MoveContactX(float maxX, string tag)
    {
        float x = _transform.position.x;
        float y = _transform.position.y;

        if (maxX > 0)
        {
            for (int i = 1; i <= maxX; i++)
            {
                if (PlaceMeeting(x + 1, y, tag))
                {
                    break;
                }
                x++;
            }
        }
        else
        {
            for (int i = -1; i >= maxX; i--)
            {
                if (PlaceMeeting(x - 1, y, tag))
                {
                    break;
                }
                x--;
            }
        }

        _transform.position = new Vector2(x, y);
    }

    public void MoveContactY(float maxY, string tag)
    {
        float x = _transform.position.x;
        float y = _transform.position.y;

        if (maxY > 0)
        {
            for (int i = 1; i <= maxY; i++)
            {
                if (PlaceMeeting(x, y + 1, tag))
                {
                    break;
                }
                y++;
            }
        }
        else
        {
            for (int i = -1; i >= maxY; i--)
            {
                if (PlaceMeeting(x, y - 1, tag))
                {
                    break;
                }
                y--;
            }
        }

        _transform.position = new Vector2(x, y);
    }

    private static void GetBoundingBox(int left, int right, int top, int bottom, out int left1, out int right1, out int top1, out int bottom1,
        float x, float y, float xscale, float yscale, float angle)
    {
        if (angle == 0)
        {
            // Not rotated
            left1 = RoundToInt(x + left * xscale);
            right1 = RoundToInt(x + (right + 1) * xscale - 1);
            top1 = RoundToInt(y + (top + 1) * yscale - 1);
            bottom1 = RoundToInt(y + bottom * yscale);
            int tmp;
            if (left1 > right1)
            {
                tmp = right1;
                right1 = left1;
                left1 = tmp;
            }
            if (bottom1 > top1)
            {
                tmp = top1;
                top1 = bottom1;
                bottom1 = tmp;
            }
        }
        else
        {
            // Rotated
            angle *= Deg2Rad;
            var sina = Sin(angle);
            var cosa = Cos(angle);

            RotateAround(x + left * xscale, y + bottom * yscale, x, y, sina, cosa, out var xlb, out var ylb);
            RotateAround(x + (right + 1) * xscale - 1, y + bottom * yscale, x, y, sina, cosa, out var xrb, out var yrb);
            RotateAround(x + left * xscale, y + (top + 1) * yscale - 1, x, y, sina, cosa, out var xlt, out var ylt);
            RotateAround(x + (right + 1) * xscale - 1, y + (top + 1) * yscale - 1, x, y, sina, cosa, out var xrt, out var yrt);

            left1 = RoundToInt(Min(xlb, xrb, xlt, xrt));
            right1 = RoundToInt(Max(xlb, xrb, xlt, xrt));
            bottom1 = RoundToInt(Min(ylb, yrb, ylt, yrt));
            top1 = RoundToInt(Max(ylb, yrb, ylt, yrt));
        }
    }

    private static void RotateAround(float xs, float ys, float xo, float yo, float sina, float cosa, out float ox, out float oy)
    {
        ox = (xs - xo) * cosa - (ys - yo) * sina + xo;
        oy = (xs - xo) * sina + (ys - yo) * cosa + yo;
    }

    public MaskData GetMaskData(Texture2D texture, Rect rect)
    {
        maskData = new MaskData();

        int rectX = FloorToInt(rect.x);
        int rectY = FloorToInt(rect.y);
        int rectWidth = FloorToInt(rect.width);
        int rectHeight = FloorToInt(rect.height);

        // Get bool data
        var boolData = new bool[rectWidth, rectHeight];
        for (var y = 0; y < rectHeight; y++)
        {
            for (var x = 0; x < rectWidth; x++)
            {
                boolData[x, y] = ((Color32)texture.GetPixel(x + rectX, y + rectY)).a != 0;
            }
        }
        maskData.boolData = boolData;

        // Get relative texture bounding box

        // Get bbox bottom
        for (var y = 0; y < rectHeight; y++)
        {
            for (var x = 0; x < rectWidth; x++)
            {
                if (((Color32)texture.GetPixel(x + rectX, y + rectY)).a != 0)
                {
                    maskData.bottom = y - YPivot;
                    goto OutBottom;
                }
            }
        }
        OutBottom:

        // Get bbox top
        for (var y = rectHeight - 1; y >= 0; y--)
        {
            for (var x = 0; x < rectWidth; x++)
            {
                if (((Color32)texture.GetPixel(x + rectX, y + rectY)).a != 0)
                {
                    maskData.top = y - YPivot;
                    goto OutTop;
                }
            }
        }
        OutTop:

        // Get bbox left
        for (var x = 0; x < rectWidth; x++)
        {
            for (var y = 0; y < rectHeight; y++)
            {
                if (((Color32)texture.GetPixel(x + rectX, y + rectY)).a != 0)
                {
                    maskData.left = x - XPivot;
                    goto OutLeft;
                }
            }
        }
        OutLeft:

        // Get bbox right
        for (var x = rectWidth - 1; x >= 0; x--)
        {
            for (var y = 0; y < rectHeight; y++)
            {
                if (((Color32)texture.GetPixel(x + rectX, y + rectY)).a != 0)
                {
                    maskData.right = x - XPivot;
                    goto OutRight;
                }
            }
        }
        OutRight:

        // Other stuff
        maskData.width = rectWidth;
        maskData.height = rectHeight;

        return maskData;
    }

    private void OnDestroy()
    {
        World.instance.colliders[gameObject.tag].Remove(this);
    }
}

public class MaskData
{
    public int left;
    public int right;
    public int top;
    public int bottom;

    public int width;
    public int height;

    public bool[,] boolData;
}