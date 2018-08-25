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
    public float duration;
    public float offset;
    public bool lookForward;
    public NetworkWalkerMode mode;

    private bool goingForward = true;
    private Spline currentSpline;
    private float progress;

    private void Start()
    {
        currentSpline = network.splines[0];
        progress = offset;
    }

    private void Update()
    {
        if(goingForward) {
            progress += Time.deltaTime / duration;
            if (progress > 1f)
            {
                if (mode == NetworkWalkerMode.Once)
                {
                    progress = 0f;
                    currentSpline = currentSpline.outNode.outLinks[Random.Range(0, currentSpline.outNode.outLinks.Count)];
                }
                else if (mode == NetworkWalkerMode.Loop)
                {
                    progress = 0f;
                    currentSpline = currentSpline.outNode.outLinks[Random.Range(0, currentSpline.outNode.outLinks.Count)];
                }
                else
                {
                    progress = 0f;
                    currentSpline = currentSpline.outNode.outLinks[Random.Range(0, currentSpline.outNode.outLinks.Count)];
                }
            }
        }
		else {
            progress -= Time.deltaTime / duration;
            if (progress < 0f)
            {
                progress = -progress;
                goingForward = true;
            }
        }
        Vector3 position = currentSpline.curve.GetPoint(progress);
        transform.localPosition = position;
        if (lookForward)
        {
            transform.LookAt(position + currentSpline.curve.GetDirection(progress));
        }
    }
}