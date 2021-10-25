using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeCompression : MonoBehaviour
{
    public Texture2D image;
    private QuadTree rootQuad;
    private const float waitTime = 3.0f;
    private bool hasRan = false;

    public void Start()
    {
        
    }

    public void FixedUpdate()
    {
        if (waitTime < Time.realtimeSinceStartup && !hasRan)
        {
            hasRan = true;

            rootQuad = new QuadTree();
            rootQuad.texture = image;
            //while (rootQuad.Split()) { }
            QuadTree.Split(rootQuad);
            QuadTree.Combine(rootQuad);
            //QuadTree.CombineToOriginal(rootQuad);
            rootQuad.SaveTexture();
        }
    }
}
