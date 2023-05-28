using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMusik : MonoBehaviour
{
    public int indexMusik;
    // Start is called before the first frame update
    void Start()
    {
        if( GameObject.Find("MusicBg")  != null )
        {
            MusikControl.Instance.ChangeMusik(indexMusik);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
