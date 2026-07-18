using UnityEngine;

public class PinchColorCube : MonoBehaviour
{
    [SerializeField] private OVRHand hand;
    private Renderer rend;
    private bool wasPinching;

    private void Awake() => rend = GetComponent<Renderer>();

    private void Update()
    {
        if (hand == null || !hand.IsTracked) return;
        bool pinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        if (pinching && !wasPinching)
            rend.material.color = Random.ColorHSV(0f, 1f, 0.6f, 0.8f, 0.9f, 1f);
        wasPinching = pinching;
    }
}