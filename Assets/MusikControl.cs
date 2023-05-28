using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusikControl : MonoBehaviour
{
    public static MusikControl Instance { get; set; }

    public AudioClip[] clipMusik;
    public AudioSource audioMusik;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void ChangeMusik(int indexMusik)
    {
        if (audioMusik.clip != clipMusik[indexMusik]) 
        {
            audioMusik.Stop();

            audioMusik.clip = clipMusik[indexMusik];

            audioMusik.Play();
        }
    }
}
