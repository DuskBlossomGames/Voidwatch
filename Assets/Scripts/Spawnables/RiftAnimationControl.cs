using UnityEngine;
using Util;

namespace Spawnables
{
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
}
