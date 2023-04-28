using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueCollision : MonoBehaviour
{
    private Rigidbody2D rb;
    public OctopusScript octopusScript;

    void Start()
    { 
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Blue")
        {
            octopusScript.isMatch = true;
            octopusScript.currentObj.SetActive(false);
            octopusScript.currentObj.transform.position = octopusScript.startPosition;
        }
    }
}

