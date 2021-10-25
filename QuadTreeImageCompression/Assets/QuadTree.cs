using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class QuadTree
{
    public QuadTree[] children;
    //          |
    //    child | child
    //      0   |   1
    //   ---------------
    //    child | child
    //      3   |   2
    //          |

    public Texture2D texture;

    //returns true if able to split
    //able to split if not down to minimum unit size (1 pixel)
    public static void Split(QuadTree curQuadTree)
    {
        int halfWidth = Mathf.FloorToInt(curQuadTree.texture.width / 2);
        int halfHeight = Mathf.FloorToInt(curQuadTree.texture.height / 2);
        if (!(halfHeight <= 1 || halfWidth <= 1))
        {
            //Debug.Log("A split has occurred. hW=" + halfWidth);
            curQuadTree.children = new QuadTree[4];
            curQuadTree.children[0] = new QuadTree();
            curQuadTree.children[1] = new QuadTree();
            curQuadTree.children[2] = new QuadTree();
            curQuadTree.children[3] = new QuadTree();

            curQuadTree.children[0].texture = new Texture2D(halfWidth, halfHeight);
            curQuadTree.children[0].texture.SetPixels(curQuadTree.texture.GetPixels(0, 0, halfWidth, halfHeight));
            curQuadTree.children[0].texture.Apply();

            curQuadTree.children[1].texture = new Texture2D(halfWidth, halfHeight);
            curQuadTree.children[1].texture.SetPixels(curQuadTree.texture.GetPixels(halfWidth, 0, halfWidth, halfHeight));
            curQuadTree.children[1].texture.Apply();

            curQuadTree.children[2].texture = new Texture2D(halfWidth, halfHeight);
            curQuadTree.children[2].texture.SetPixels(curQuadTree.texture.GetPixels(halfWidth, halfHeight, halfWidth, halfHeight));
            curQuadTree.children[2].texture.Apply();

            curQuadTree.children[3].texture = new Texture2D(halfWidth, halfHeight);
            curQuadTree.children[3].texture.SetPixels(curQuadTree.texture.GetPixels(0, halfHeight, halfWidth, halfHeight));
            curQuadTree.children[3].texture.Apply();

            //curQuadTree.texture = null;
            foreach (QuadTree child in curQuadTree.children)
                QuadTree.Split(child);
        }
    }

    //called on a parent Quadtree to combine its children into it
    //returns true if has children that can be combined
    public static void Combine(QuadTree curQuadTree)
    {
        if (curQuadTree.children != null)
        {
            QuadTree.Combine(curQuadTree.children[0]);
            QuadTree.Combine(curQuadTree.children[1]);
            QuadTree.Combine(curQuadTree.children[2]);
            QuadTree.Combine(curQuadTree.children[3]);
        }
        else
            return;

        Color[] childColors =
        {
            curQuadTree.children[0].texture.GetPixel(0,0),
            curQuadTree.children[1].texture.GetPixel(0,0),
            curQuadTree.children[2].texture.GetPixel(0,0),
            curQuadTree.children[3].texture.GetPixel(0,0)
        };

        Color avgChildrenColor = new Color();
        avgChildrenColor.r = (childColors[0].r +
                                childColors[1].r +
                                childColors[2].r +
                                childColors[3].r) / 4;
        avgChildrenColor.g = (childColors[0].g +
                                childColors[1].g +
                                childColors[2].g +
                                childColors[3].g) / 4;
        avgChildrenColor.b = (childColors[0].b +
                                childColors[1].b +
                                childColors[2].b +
                                childColors[3].b) / 4;
        avgChildrenColor.a = childColors[0].a;
        //Color[] texColors = curQuadTree.texture.GetPixels();
        //for (int pixel = 0; pixel < texColors.Length; ++pixel)
        //texColors[pixel] = avgChildrenColor;
        //curQuadTree.texture.SetPixels(texColors);

        //determine color distance of each child color to the average color
        float dist0 = ((childColors[0].r - avgChildrenColor.r) * (childColors[0].r - avgChildrenColor.r) +
                    (childColors[0].g - avgChildrenColor.g) * (childColors[0].g - avgChildrenColor.g) +
                    (childColors[0].b - avgChildrenColor.b) * (childColors[0].b - avgChildrenColor.b));
        float dist1 = ((childColors[1].r - avgChildrenColor.r) * (childColors[1].r - avgChildrenColor.r) +
                    (childColors[1].g - avgChildrenColor.g) * (childColors[1].g - avgChildrenColor.g) +
                    (childColors[1].b - avgChildrenColor.b) * (childColors[1].b - avgChildrenColor.b));
        float dist2 = ((childColors[2].r - avgChildrenColor.r) * (childColors[2].r - avgChildrenColor.r) +
                    (childColors[2].g - avgChildrenColor.g) * (childColors[2].g - avgChildrenColor.g) +
                    (childColors[2].b - avgChildrenColor.b) * (childColors[2].b - avgChildrenColor.b));
        float dist3 = ((childColors[3].r - avgChildrenColor.r) * (childColors[3].r - avgChildrenColor.r) +
                    (childColors[3].g - avgChildrenColor.g) * (childColors[3].g - avgChildrenColor.g) +
                    (childColors[3].b - avgChildrenColor.b) * (childColors[3].b - avgChildrenColor.b));

        curQuadTree.texture = new Texture2D(curQuadTree.children[0].texture.width + curQuadTree.children[1].texture.width, curQuadTree.children[0].texture.height + curQuadTree.children[3].texture.height);
        float tolerance = .05f;
        //if maximum tolerance distance is not exceeded, children are disposed, making the parent the end node
        if (Mathf.Sqrt(dist0) <= tolerance && Mathf.Sqrt(dist1) <= tolerance && Mathf.Sqrt(dist2) <= tolerance && Mathf.Sqrt(dist3) <= tolerance)
        {
            //if under tolerance set parent texture = to average
            //Debug.Log("Under tolerance");
            Color[] texColors = curQuadTree.texture.GetPixels();
            for (int pixel = 0; pixel < texColors.Length; ++pixel)
                texColors[pixel] = avgChildrenColor;
            curQuadTree.texture.SetPixels(texColors);
            curQuadTree.texture.Apply();
            curQuadTree.children = null;
        }
        else
        {
            //Debug.Log("Over tolerance");
            curQuadTree.texture.SetPixels(0, 0, curQuadTree.children[0].texture.width, curQuadTree.children[0].texture.height, curQuadTree.children[0].texture.GetPixels());
            //curQuadTree.texture.Apply();
            curQuadTree.texture.SetPixels(curQuadTree.children[0].texture.width, 0, curQuadTree.children[0].texture.width, curQuadTree.children[0].texture.height, curQuadTree.children[1].texture.GetPixels());
            //curQuadTree.texture.Apply();
            curQuadTree.texture.SetPixels(curQuadTree.children[0].texture.width, curQuadTree.children[0].texture.height, curQuadTree.children[0].texture.width, curQuadTree.children[0].texture.height, curQuadTree.children[2].texture.GetPixels());
            //curQuadTree.texture.Apply();
            curQuadTree.texture.SetPixels(0, curQuadTree.children[0].texture.height, curQuadTree.children[0].texture.width, curQuadTree.children[0].texture.height, curQuadTree.children[3].texture.GetPixels());
            curQuadTree.texture.Apply();
            curQuadTree.children = null;
        }
    }

    public static void CombineToOriginal(QuadTree curQuadTree)
    {
        if (curQuadTree.children != null)
        {
            QuadTree.CombineToOriginal(curQuadTree.children[0]);
            QuadTree.CombineToOriginal(curQuadTree.children[1]);
            QuadTree.CombineToOriginal(curQuadTree.children[2]);
            QuadTree.CombineToOriginal(curQuadTree.children[3]);
        }
        else
            return;
        curQuadTree.texture = new Texture2D(curQuadTree.children[0].texture.width + curQuadTree.children[1].texture.width, curQuadTree.children[0].texture.height + curQuadTree.children[3].texture.height);

        curQuadTree.texture.SetPixels(0, 0, curQuadTree.children[0].texture.width, curQuadTree.children[0].texture.height, curQuadTree.children[0].texture.GetPixels());
        curQuadTree.texture.Apply();
        curQuadTree.texture.SetPixels(curQuadTree.children[0].texture.width, 0, curQuadTree.children[0].texture.width, curQuadTree.children[0].texture.height, curQuadTree.children[1].texture.GetPixels());
        curQuadTree.texture.Apply();
        curQuadTree.texture.SetPixels(curQuadTree.children[0].texture.width, curQuadTree.children[0].texture.height, curQuadTree.children[0].texture.width, curQuadTree.children[0].texture.height, curQuadTree.children[2].texture.GetPixels());
        curQuadTree.texture.Apply();
        curQuadTree.texture.SetPixels(0, curQuadTree.children[0].texture.height, curQuadTree.children[0].texture.width, curQuadTree.children[0].texture.height, curQuadTree.children[3].texture.GetPixels());
        curQuadTree.texture.Apply();
        curQuadTree.children = null;
    }

    public void SaveTexture()
    {
        //first Make sure you're using RGB24 as your texture format
        //Texture2D outTexture = new Texture2D(texture.width, texture.height, texture.format, false);

        GameObject.FindGameObjectWithTag("Image").GetComponent<RawImage>().texture = texture;
        //then Save To Disk as PNG
        byte[] bytes = texture.EncodeToJPG();
        var dirPath = Application.dataPath + "/SaveImages/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "Image" + ".jpg", bytes);
        Debug.Log("Image Saved");
        /*Color[] colors = texture.GetPixels(0, 0, 500, 500);
        for (int i = 0; i < colors.Length; ++i)
            colors[i] = Color.red;
        texture.SetPixels(0, 0, 500, 500, colors);
        texture.Apply();*/
    }
}
