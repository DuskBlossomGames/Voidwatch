using UnityEngine;

namespace Player
{
  public class PlayerVFXController : MonoBehaviour
  {

    public ParticleSystem bif;
    public ParticleSystem StunElectricity;

    public void RunBif(){  bif.Play();}

    public void RunStun(){  StunElectricity.Play();}
  }
}
