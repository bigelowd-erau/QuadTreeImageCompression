using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeCompression : MonoBehaviour
{
    public Texture2D image;
    private QuadTree rootQuad;

    public void Start()
    {
        rootQuad = new QuadTree();
        rootQuad.texture = image;
        //while (rootQuad.Split()) { }
        QuadTree.Split(rootQuad);
        QuadTree.Combine(rootQuad);
        //QuadTree.CombineToOriginal(rootQuad);
        rootQuad.SaveTexture();
    }

}
