using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lisa : MonoBehaviour
{
    [Header("玩家引用")]
    public Transform player;
    public FirstPersonCamera playerCamera;
    public GameObject lookat;

    [Header("参数设置")]
    public float showPlayerXThreshold = 5f;
    public float chaseSpeed = 3f;
    public float chaseSpeedMultiplier = 2f;
    public float chaseDistance = 2f;

    [Header("动画")]
    private Animator animator; // ✅ 绑定Lisa的Animator

    private bool isHidden = false;
    private bool isChasing = false;

    private Renderer[] allRenderers;
    private Collider[] allColliders;

    private void Start()
    {
        animator.GetComponent<Animator>();
        allRenderers = GetComponentsInChildren<Renderer>(true);
        allColliders = GetComponentsInChildren<Collider>(true);
        animator.SetBool("fuck",true);
    }

    private void Update()
    {
        if (player == null) return;

        if (isHidden && !isChasing)
        {
            if (player.position.x > showPlayerXThreshold)
            {
                ShowObject();
                isChasing = true;
            }
        }

        if (isChasing)
        {
            float playerY = player.eulerAngles.y;
            if (playerY > 180f) playerY -= 360f;

            float currentSpeed = chaseSpeed;
            if (playerY >= -180f && playerY <= 0f){
                
                currentSpeed *= chaseSpeedMultiplier;
                    
                }
            Vector3 direction = (player.position - transform.position).normalized;
            Vector3 move = direction * currentSpeed * Time.deltaTime;

            float groundY = transform.position.y;
            transform.position += move;
            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);

            Vector3 lookPos = player.position - transform.position;
            lookPos.y = 0;
            transform.rotation = Quaternion.LookRotation(lookPos);

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= chaseDistance)
            {
                isChasing = false;

                lookPos = player.position - transform.position;
                lookPos.y = 0;
                if (lookPos.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(lookPos);
                
                playerCamera.LockOn(lookat.transform);

                // ✅ 触发 Jump 动画
                

                StartCoroutine(DelayLoadScene());
            }
        }
    }

    private IEnumerator DelayLoadScene()
    {
        yield return new WaitForSeconds(1f);

        SceneLoader.LoadSceneByIndex(1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isChasing)
            {
                HideObject();
            }
        }
    }

    private void HideObject()
    {
        isHidden = true;
        foreach (Renderer rend in allRenderers) rend.enabled = false;
        foreach (Collider col in allColliders) col.enabled = false;
    }

    private void ShowObject()
    {
        isHidden = false;
        foreach (Renderer rend in allRenderers) rend.enabled = true;
        foreach (Collider col in allColliders) col.enabled = true;
    }
}
