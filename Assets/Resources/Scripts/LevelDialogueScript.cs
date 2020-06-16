using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LevelDialogueScript : MonoBehaviour
{
    public string message;
    public float lifeTime = 5f;
    private float textSpeed = 0.05f;

    Collider2D col;
    Text dialogueText;
    // Start is called before the first frame update
    void Start()
    {
        col = gameObject.GetComponent<Collider2D>();
        dialogueText = gameObject.GetComponent<Text>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            col.enabled = false;
            StartCoroutine(DialogueDisplay());
        }
    }

    IEnumerator DialogueDisplay()
    {
        int l = message.Length;
        for(int x = 0;x <l;x++)
        {
            dialogueText.text += message[x];
            yield return new WaitForSeconds(textSpeed);
        }
        yield return new WaitForSeconds(lifeTime);
        Color c = dialogueText.color;
        float a = c.a;
        string removeText = message;

        int spaceCounter = 0;//Counts additional spaces added when replacing letters w/ two spaces
        for (int x = 0; x <l; x++)
        {
            if(removeText.Substring(x+spaceCounter,1) != " ")
            {
                removeText = removeText.Remove(x + spaceCounter, 1).Insert(x, "  ");//Replaces with  2space s
                spaceCounter++;
            }
            else
                removeText = removeText.Remove(x + spaceCounter, 1).Insert(x, " ");//replace with one space
            dialogueText.text = removeText;
            yield return new WaitForSeconds(textSpeed);
        }
    }
}
