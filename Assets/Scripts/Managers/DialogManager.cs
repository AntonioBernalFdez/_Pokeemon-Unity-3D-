using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DialogManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogBox;
    [SerializeField] private Text dialogText;
    [SerializeField] private int charactersPerSecond;

    public static DialogManager SharedInstance;

    public event System.Action OnDialogStart, OnDialogFinish;

    private float timeSinceLastClick;
    private float timeBetweenClicks = 0.25f;

    private Dialog currentDialog;
    private int currentLine = 0;
    private bool isWriting;

    public bool IsTalking;

    public bool IsBeingShown;
    private System.Action onDialogClose;

    private void Awake()
    {
        if (SharedInstance == null)
        {
            SharedInstance = this;
        }
        else
        {
            Destroy(this); 
        }
    }
    public void ShowDialog (Dialog dialog, System.Action onDialogFinish = null)
    {
        OnDialogStart?.Invoke();
        dialogBox.SetActive (true);
        currentDialog = dialog;
        IsBeingShown = true;
        currentLine = 0;
        this.onDialogClose = onDialogFinish;
        StartCoroutine(SetDialog(currentDialog.Lines[currentLine]));
    }
    public void HandleUpdate()
    {
        timeSinceLastClick += Time.deltaTime;
        if (Input.GetAxisRaw("Submit") != 0 && !isWriting )
        {
            if(timeSinceLastClick >= timeBetweenClicks)
            {
                timeSinceLastClick = 0;
                currentLine++;
                if (currentLine < currentDialog.Lines.Count)
                {
                    StartCoroutine(SetDialog(currentDialog.Lines[currentLine]));
                }
                else
                {
                    currentLine = 0;
                    IsBeingShown=false;
                    dialogBox.SetActive(false);
                    onDialogClose?.Invoke();
                    OnDialogFinish?.Invoke();
                }
            }
        }
    }
    public IEnumerator SetDialog(string message)
    {
        isWriting = true;
        dialogText.text = "";
        foreach (var character in message)
        {
            dialogText.text += character;
            yield return new WaitForSeconds(1 / charactersPerSecond);
        }
        yield return new WaitForSeconds(0.5f);
        isWriting = false;
    }
   



}
