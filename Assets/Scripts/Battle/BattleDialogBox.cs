using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] Text dialogText;


    [SerializeField]  GameObject actionSelect;
    [SerializeField]  GameObject movementSelect;
    [SerializeField]  GameObject movementDesc;
    [SerializeField] private GameObject yesNoBox;

    [SerializeField] List<Text> actionText;
    [SerializeField] List<Text> movementTexts;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;
    [SerializeField] Text yesText, noText;

    public float charactersPerSecond = 120.0f;
    [SerializeField] Color selectedColor = Color.red;

    public bool isWriting  = false;

    public IEnumerator SetDialog(string message)
    {
        isWriting = true;
        dialogText.text = "";
        foreach (var character in message)
        {
            dialogText.text += character;
            yield return new WaitForSeconds(1 / charactersPerSecond);
        }
        yield return new WaitForSeconds(1f);
        isWriting = false;
    }

    public void ToggleDialogText(bool activated)
    {
        dialogText.enabled = activated;
    }
    public void ToggleActions(bool activated)
    {
        actionSelect.SetActive(activated);
    }
    public void ToggleMovements(bool activated)
    {
        movementSelect.SetActive(activated);
        movementDesc.SetActive(activated);
    }
    public void ToggleYesNoBox(bool activated)
    {
        yesNoBox.SetActive(activated);
    }
    public  void SelectAction(int selectedAction)
    {
        for (int i = 0; i < actionText.Count; i++)
        {
            actionText[i].color = (i == selectedAction ? selectedColor : Color.black);
         

        }
    }
    public void SetPokemonMovements(List<Move> moves)
    {
        for (int i = 0; i < movementTexts.Count; i++)
        {
            if (i < moves.Count)
            {
                movementTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                movementTexts[i].text = "---";
            }

        }

    }


    public void SelectMovement(int selectedMovement, Move move)
    {
        for (int i = 0; i < movementTexts.Count; i++)
        {
            movementTexts[i].color = (i == selectedMovement ? selectedColor : Color.black);

        }
        ppText.text = $"PP:{move.Pp}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString().ToUpper();

        ppText.color = (move.Pp <= 0 ? Color.red:Color.black);
    }
    public void SelectYesNoAction(bool yesSelected)
    {
        if(yesSelected)
        {
            yesText.color = Color.red;
            noText.color = Color.black;
        }
        else
        {
            yesText.color = Color.black;
            noText.color = Color.red;
        }
    }




}