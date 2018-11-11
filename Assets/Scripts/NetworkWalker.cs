using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NetworkWalkerMode
{
    Once,
    Loop,
    PingPong
}

public class NetworkWalker : MonoBehaviour
{

    public Network network;
    public float speed;
    public float offset;
    public bool lookForward;
    public NetworkWalkerMode mode;

    private bool goingForward = true;
    private Spline currentSpline;
    private float progress;
    private bool halted = false;

    private void Start()
    {
        currentSpline = network.splines[0];
        progress = offset;
    }

    public void JumpToNextLink()
    {
        if(currentSpline.outNode.outLinks.Count == 0)
        {
            halted = true;
            Destroy(this.gameObject);
        }
        else
        {
            progress = 0f;
            currentSpline = currentSpline.outNode.outLinks[UnityEngine.Random.Range(0, currentSpline.outNode.outLinks.Count)];
        }
    }

    public void JumpToPreviousLink()
    {
        if (currentSpline.inNode.inLinks.Count == 0)
        {
            halted = true;
            Destroy(this.gameObject);

        }
        else
        {
            currentSpline = currentSpline.inNode.inLinks[UnityEngine.Random.Range(0, currentSpline.inNode.inLinks.Count)];
            progress = currentSpline.curve.GetTotalLength();
        }
    }

    IEnumerator WaitForTimer(float t)
    {
        yield return new WaitForSeconds(t);
        halted = false;
    }

    public void CheckForWaiting(Node n)
    {
        if(n.type == NodeType.WaitPoint && n.waitTime > 0)
        {
            halted = true;
            StartCoroutine("WaitForTimer", n.waitTime);
        }
    }

    private void Update()
    {
        if (!halted)
        {
            MoveWalker();
        }
    }

    public void MoveWalker()
    {
        if (goingForward)
        {
            progress += Time.deltaTime * speed;
            if (progress >= currentSpline.curve.GetTotalLength())
            {
                CheckForWaiting(currentSpline.outNode);
                JumpToNextLink();
            }
        }
        else
        {
            progress -= Time.deltaTime * speed;
            if (progress < 0f)
            {
                JumpToPreviousLink();
            }
        }
        Vector3 position = currentSpline.curve.GetLinearPointFromDist(progress);
        transform.position = network.transform.TransformPoint(position);
        if (lookForward)
        {
            transform.LookAt(position + currentSpline.curve.GetDirection(progress));
        }
    }
}