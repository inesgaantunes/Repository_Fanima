using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TravelScript : MonoBehaviour
{
    public Animator animator;
    private bool finish;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetBool("finish", true);

            SceneManager.LoadScene("Frog");

        }
    }
}
