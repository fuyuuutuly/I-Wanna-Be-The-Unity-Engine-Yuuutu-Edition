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

    private int left { get => maskData.left; }
    private int right { get => maskData.right; }
    private int top { get => maskData.top; }
    private int bottom { get => maskData.bottom; }

    private bool[] boolData { get => maskData.boolData; }

    private int width { get => maskData.width; }
    private int height { get => maskData.height; }

    private float xPos { get => _transform.position.x; }
    private float yPos { get => _transform.position.y; }

    private int xPivot { get => (int)(maskRenderer.sprite.pivot.x); }
    private int yPivot { get => (int)(maskRenderer.sprite.pivot.y); }

    private float xScale
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

    private float yScale
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

    private float rotation
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
    public bool enableChangeScale = false;
    public bool enableChangeRotation = false;
    private SpriteAnimator animator;

    private void Start()
    {
        _transform = transform;

        xScaleStart = _transform.localScale.x;
        yScaleStart = _transform.localScale.y;
        rotationStart = _transform.rotation.eulerAngles.z;

        maskRenderer = GetComponent<SpriteRenderer>();
        var texture = maskRenderer.sprite.texture;

        // Get mask data
        if (!enableSpriteAnimator)
        {
            if (!World.instance.maskDataManager.ContainsKey(texture))
            {
                var maskData = GetMaskData(texture);

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
                        var maskData = GetMaskData(texture);

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
        GetBoundingBox(left, right, top, bottom, out var left1, out var right1, out var top1, out var bottom1,
            x1, y1, xScale, yScale, rotation);

        // For each colliders
        foreach (var col in cders)
        {
            if (col == this) // Don't check himself
                continue;

            if (col.enableSpriteAnimator)
                col.maskData = World.instance.maskDataManager[col.maskRenderer.sprite.texture];

            var x2 = col.xPos;
            var y2 = col.yPos;

            // Get other bounding box with transform
            GetBoundingBox(col.left, col.right, col.top, col.bottom, out var left2, out var right2, out var top2, out var bottom2,
                x2, y2, col.xScale, col.yScale, col.rotation);

            // Get intersection
            int iLeft = Max(left1, left2);
            int iRight = Min(right1, right2);
            int iBottom = Max(bottom1, bottom2);
            int iTop = Min(top1, top2);

            if (iLeft > iRight || iBottom > iTop)
                continue;

            // Check each pixel
            if (rotation == 0 && col.rotation == 0)
            {
                // Not rotated
                var xo1 = xPivot;
                var yo1 = yPivot;

                var xo2 = col.xPivot;
                var yo2 = col.yPivot;

                for (int yy = iBottom; yy <= iTop; yy++)
                {
                    for (int xx = iLeft; xx <= iRight; xx++)
                    {
                        var px1 = (int)((xx - x1) / xScale + xo1);
                        var py1 = (int)((yy - y1) / yScale + yo1);
                        var p1 = px1 >= 0 && py1 >= 0 && px1 < width && py1 < height && boolData[px1 + py1 * width];

                        var px2 = (int)((xx - x2) / col.xScale + xo2);
                        var py2 = (int)((yy - y2) / col.yScale + yo2);
                        var p2 = px2 >= 0 && py2 >= 0 && px2 < col.width && py2 < col.height && col.boolData[px2 + py2 * col.width];

                        if (p1 && p2)
                            return col.gameObject;
                    }
                }
            }
            else
            {
                // Rotated
                var sina1 = Sin(-rotation * Deg2Rad);
                var cosa1 = Cos(-rotation * Deg2Rad);
                var xo1 = xPivot;
                var yo1 = yPivot;

                var sina2 = Sin(-col.rotation * Deg2Rad);
                var cosa2 = Cos(-col.rotation * Deg2Rad);
                var xo2 = col.xPivot;
                var yo2 = col.yPivot;

                for (int yy = iBottom; yy <= iTop; yy++)
                {
                    for (int xx = iLeft; xx <= iRight; xx++)
                    {
                        var lx1 = xx - x1;
                        var ly1 = yy - y1;
                        RotateAround(lx1, ly1, 0, 0, sina1, cosa1, out var lx1a, out var ly1a);
                        var px1 = (int)(lx1a / xScale + xo1);
                        var py1 = (int)(ly1a / yScale + yo1);
                        var p1 = px1 >= 0 && py1 >= 0 && px1 < width && py1 < height && boolData[px1 + py1 * width];

                        var lx2 = xx - x2;
                        var ly2 = yy - y2;
                        RotateAround(lx2, ly2, 0, 0, sina2, cosa2, out var lx2a, out var ly2a);
                        var px2 = (int)(lx2a / col.xScale + xo2);
                        var py2 = (int)(ly2a / col.yScale + yo2);
                        var p2 = px2 >= 0 && py2 >= 0 && px2 < col.width && py2 < col.height && col.boolData[px2 + py2 * col.width];

                        if (p1 && p2)
                            return col.gameObject;
                    }
                }
            }
        }
        return null;
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

    public MaskData GetMaskData(Texture2D texture)
    {
        maskData = new MaskData();

        // Get bool data
        var boolData = new bool[texture.width * texture.height];
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                boolData[x + y * texture.width] = ((Color32)texture.GetPixel(x, y)).a != 0;
            }
        }
        maskData.boolData = boolData;

        // Get relative texture bounding box

        // Get bbox bottom
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                if (((Color32)texture.GetPixel(x, y)).a != 0)
                {
                    maskData.bottom = y - yPivot;
                    goto OutBottom;
                }
            }
        }
        OutBottom:

        // Get bbox top
        for (var y = texture.height - 1; y >= 0; y--)
        {
            for (var x = 0; x < texture.width; x++)
            {
                if (((Color32)texture.GetPixel(x, y)).a != 0)
                {
                    maskData.top = y - yPivot;
                    goto OutTop;
                }
            }
        }
        OutTop:

        // Get bbox left
        for (var x = 0; x < texture.width; x++)
        {
            for (var y = 0; y < texture.height; y++)
            {
                if (((Color32)texture.GetPixel(x, y)).a != 0)
                {
                    maskData.left = x - xPivot;
                    goto OutLeft;
                }
            }
        }
        OutLeft:

        // Get bbox right
        for (var x = texture.width - 1; x >= 0; x--)
        {
            for (var y = 0; y < texture.height; y++)
            {
                if (((Color32)texture.GetPixel(x, y)).a != 0)
                {
                    maskData.right = x - xPivot;
                    goto OutRight;
                }
            }
        }
        OutRight:

        // Other stuff
        maskData.width = texture.width;
        maskData.height = texture.height;

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

    public bool[] boolData;
}