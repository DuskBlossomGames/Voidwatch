using UnityEngine;

namespace Player
{
  public class PlayerVFXController : MonoBehaviour
  {

    public ParticleSystem bif;
    public ParticleSystem StunElectricity;
    public GameObject spikeRight;

    public void RunBif(){  bif.Play();}

    public void RunStun(){  StunElectricity.Play();}

    public void RunSpike()
    {
      //SpikeRight.StartRotation3D = new Vector3(0, 0, 0);
      var spike = Instantiate(spikeRight);
      spike.transform.position = transform.position;
      spike.transform.rotation = transform.rotation;
      //spike.GetComponent<ParticleSystem>().Play();
    }

  }
}
