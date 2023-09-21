using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideImageChameleon3 : MonoBehaviour
{
    public ChameleonScript chameleonScript;
    public Animator animator;

    // Update is called once per frame
    void Update()
    {
        if(chameleonScript.hide3)
        {
            animator.SetBool("hide", true);
        }
        else
        {
            animator.SetBool("hide", false);

        }
    }
}
