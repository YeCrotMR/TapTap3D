using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioAnormaly : MonoBehaviour
{
    [Header("音频资源")]
    public AudioClip morningGreeting;    // 早晨问候语
    public AudioClip noonGreeting;       // 午间问候语（异常）

    [Header("触发设置")]
    public float triggerRadius = 5f;
    public GameObject Player = null;


    public int loopIndex;
    private AudioSource audioSource;
    private bool isTurnOn = false;
    private Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        isTurnOn = false;
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        if (Player == null)
            Player = GameObject.FindGameObjectWithTag("Player");
        //    loopIndex = 
        playerTransform = Player.transform;
        if (loopIndex == 2)
            audioSource.clip = noonGreeting;
        else
            audioSource.clip = morningGreeting;
    }

    // Update is called once per frame
    void Update()
    {
        playerTransform = Player.transform;
        if(!isTurnOn)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if(distance <= triggerRadius)
            {
                PlayGreeting();
            }
        }
    }
    void PlayGreeting()
    {
        isTurnOn = true;
        audioSource.Play();
        Debug.Log("Greeting!");
    }
}
