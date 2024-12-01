using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTransitionManager : MonoBehaviour
{

    private static MusicTransitionManager instance;
    // Start is called before the first frame update
    void Awake(){
      if(instance==null){
        instance = this;
        DontDestroyOnLoad(instance);
      } else {
          Destroy(gameObject);

      }



    }
}
