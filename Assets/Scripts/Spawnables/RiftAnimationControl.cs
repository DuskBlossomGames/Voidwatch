using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiftAnimationControl : MonoBehaviour
{
  public NSpriteAnimation animationState;
    // Start is called before the first frame update
    void Start()
    {
      animationState.SwapState("idle");

    }

    // Update is called once per frame
    void Update()
    {

    }
}
