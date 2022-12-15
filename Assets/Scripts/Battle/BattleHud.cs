using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleHud : MonoBehaviour
{
    public Text pokemonName;

    public Text pokemonLevel;

    public HealthBar healthBar;

    public GameObject expBar;
    public GameObject statusBox;

   

    private Pokemon _pokemon;
    public void SetPokemonData(Pokemon pokemon)
    {
        
        _pokemon = pokemon;
      
        SetLevelText();
        pokemonName.text = pokemon.Base.Name;
        pokemonLevel.text = $"Lv: {pokemon.Level}";
        healthBar.SetHp((float)pokemon.HP / pokemon.MaxHp);
        SetExp();
        UpdatePokemonData();
        SetStatusConditionData();
        _pokemon.OnStatusConditionChanged += SetStatusConditionData;
        
    }
    
    public void UpdatePokemonData()
    {
        if (_pokemon.HasHpChanged)
        {
            StartCoroutine(healthBar.SetSmoothHP(_pokemon));
            
            _pokemon.HasHpChanged = false;

        }


    }

    public void SetExp()
    {
        if (expBar == null)
        {
            return;
        }
        expBar.transform.localScale = new Vector3(NormalizedExp(),1,1);
    }

     public IEnumerator SetExpSmooth(bool needsToResetBar = false)
    {
        if (expBar == null)
        {
           yield break;
        }
        if (needsToResetBar)
        {
            expBar.transform.localScale = new Vector3(0,1,1);
        }
       yield return expBar.transform.DOScaleX(NormalizedExp(),2f).WaitForCompletion();
       

    }




    float NormalizedExp()
    {
        float currentLevelExp = _pokemon.Base.GetNecessaryExpForLevel(_pokemon.Level);
        float nextLevelExp = _pokemon.Base.GetNecessaryExpForLevel(_pokemon.Level + 1);
        float normalizedExp = (_pokemon.Experience - currentLevelExp) / (nextLevelExp - currentLevelExp);
        Debug.Log(normalizedExp);
        return Mathf.Clamp01(normalizedExp);
    }
    public void SetLevelText()
    {
        pokemonLevel.text = $"Lv {_pokemon.Level}";
    }
    void SetStatusConditionData()
    {
        if(_pokemon.StatusCondition == null)
        {
            statusBox.SetActive(false);
        }
        else if(_pokemon.StatusCondition.id != StatusConditionID.conf)
        {
            statusBox.SetActive(true);
            
            statusBox.GetComponentInChildren<Text>().text = _pokemon.StatusCondition.id.ToString().ToUpper();
            if (_pokemon.StatusCondition.id == StatusConditionID.psn)
            {
                statusBox.GetComponent<Image>().color = Color.magenta;
            }
            else if (_pokemon.StatusCondition.id == StatusConditionID.slp)
            {
                statusBox.GetComponent<Image>().color = Color.gray;
            }
            else if (_pokemon.StatusCondition.id == StatusConditionID.brn)
            {
                statusBox.GetComponent<Image>().color = Color.red;
            }
            else if (_pokemon.StatusCondition.id == StatusConditionID.par)
            {
                statusBox.GetComponent<Image>().color = Color.yellow;
            }
            else if (_pokemon.StatusCondition.id == StatusConditionID.frz)
            {
                statusBox.GetComponent<Image>().color = Color.cyan;
            }
        }
    }

 }
