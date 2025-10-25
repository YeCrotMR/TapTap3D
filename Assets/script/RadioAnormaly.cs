using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UnityEngine;

public class RadioAnormaly : MonoBehaviour
{
    [Header("音频资源")]
    public AudioClip morningGreeting;    // 早晨问候语
    public AudioClip noonGreeting1;       // 午间问候语（异常）
    public AudioClip noonGreeting2;

    [Header("触发设置")]
    public float triggerRadius = 5f;
    public GameObject Player = null;
    public int stageIndex;
    public int anormalStage = 2;

    private AudioSource audioSource;
    private bool isTurnOn = false;
    private Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        stageIndex = CorridorStageManager.currentStage;
        isTurnOn = false;
        audioSource = GetComponent<AudioSource>();
//        audioSource.loop = true;
        if (Player == null)
            Player = GameObject.FindGameObjectWithTag("Player");
        //    loopIndex = 
        playerTransform = Player.transform;
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
        if (stageIndex == anormalStage)
        {
            StartCoroutine(PlayNoonAudio());
        }
        else
        {
            audioSource.clip = morningGreeting;
            audioSource.Play();
        }
        Debug.Log("Greeting!");
    }
    IEnumerator PlayNoonAudio()
    {
        audioSource.clip = noonGreeting1;
        audioSource.Play();

        yield return new WaitForSeconds(noonGreeting1.length);

        audioSource.clip = noonGreeting2;
        audioSource.Play();
    }
}
