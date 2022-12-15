using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum GameState { Travel,Battle,Dialog,CutScene }
public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private Camera worldMainCamera;
    [SerializeField] private Image transitionPanel;
    public GameState _gameState;

    public AudioClip worldClip, battleClip,battleLeaderClip;

    public static GameManager SharedInstance;

    private TrainerController trainer;

    private void Awake()
    {
        if (SharedInstance != null)
        {
            Destroy(this);
        }
        SharedInstance = this;
        _gameState = GameState.Travel;
    }
    void Start()
    {
        StatusConditionsFactory.InitFactory();
        SoundManager.SharedInstance.PlayMusic(worldClip);
        playerController.OnPokemonEncountered1 += StartPokemonBattle;
        playerController.OnPokemonEncountered2 += StartPokemonBattle2;
        playerController.OnPokemonEncountered3 += StartPokemonBattle3;
        playerController.OnPokemonEncountered4 += StartPokemonBattle4;
        playerController.OnEnterTrainerFov += (Collider2D trainerCollider)  =>
        {
            var trainer = trainerCollider.GetComponentInParent<TrainerController>();
            if (trainer != null)
            {
                _gameState = GameState.CutScene;
                StartCoroutine(trainer.TriggerTrainerBattle(playerController));
            }
            
        }; 
        battleManager.OnBattleFinish += FinishPokemonBattle;
        DialogManager.SharedInstance.OnDialogStart += () =>
       {
           _gameState = GameState.Dialog;
       };
        DialogManager.SharedInstance.OnDialogFinish += () => 
        {    if (_gameState == GameState.Dialog)
                _gameState = GameState.Travel;



        };
    }

    void StartPokemonBattle() { StartCoroutine(FadeInBattle()); }
    void StartPokemonBattle2() { StartCoroutine(FadeInBattle2()); }
    void StartPokemonBattle3() { StartCoroutine(FadeInBattle3()); }
    void StartPokemonBattle4() { StartCoroutine(FadeInBattle4()); }


    public void StartTrainerBattle(TrainerController trainer)
    {
        this.trainer = trainer;
        StartCoroutine(FadeInTrainerBattle(trainer));
    }
    IEnumerator FadeInBattle()
    {

        _gameState = GameState.Battle;
        SoundManager.SharedInstance.PlayMusic(battleClip);
        yield return transitionPanel.DOFade(1.0f, 1.0f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);

        battleManager.gameObject.SetActive(true);
        worldMainCamera.gameObject.SetActive(false);
        var playerParty = playerController.GetComponent<PokemonParty>();


        var wildPokemon = FindObjectOfType<PokemonMapArea>().GetComponent<PokemonMapArea>().GetRandomWildPokemon();
        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);
        battleManager.HandleStartBattle(playerParty, wildPokemonCopy);
        yield return transitionPanel.DOFade(0.0f, 1.0f).WaitForCompletion();

    }
    IEnumerator FadeInBattle2()
    {
        _gameState = GameState.Battle; SoundManager.SharedInstance.PlayMusic(battleClip);
        yield return transitionPanel.DOFade(1.0f, 1.0f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);
        battleManager.gameObject.SetActive(true);
        worldMainCamera.gameObject.SetActive(false);
        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<PokemonMapArea2>().GetComponent<PokemonMapArea2>().GetRandomWildPokemon();
        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);
        battleManager.HandleStartBattle(playerParty, wildPokemonCopy);
        yield return transitionPanel.DOFade(0.0f, 1.0f).WaitForCompletion();
    }
    IEnumerator FadeInBattle3()
    {
        _gameState = GameState.Battle; SoundManager.SharedInstance.PlayMusic(battleClip);
        yield return transitionPanel.DOFade(1.0f, 1.0f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);
        battleManager.gameObject.SetActive(true);
        worldMainCamera.gameObject.SetActive(false);
        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<PokemonMapArea3>().GetComponent<PokemonMapArea3>().GetRandomWildPokemon();
        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);
        battleManager.HandleStartBattle(playerParty, wildPokemonCopy);
        yield return transitionPanel.DOFade(0.0f, 1.0f).WaitForCompletion();
    }
    IEnumerator FadeInBattle4()
    {
        _gameState = GameState.Battle; SoundManager.SharedInstance.PlayMusic(battleClip);
        battleManager.battleGroundImage[2].gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        yield return transitionPanel.DOFade(1.0f, 1.0f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);
        battleManager.gameObject.SetActive(true);
        worldMainCamera.gameObject.SetActive(false);
        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<WaterPokemonMapArea>().GetComponent<WaterPokemonMapArea>().GetRandomWildPokemon();
        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);
        battleManager.HandleStartBattle(playerParty, wildPokemonCopy);
        yield return transitionPanel.DOFade(0.0f, 1.0f).WaitForCompletion();
    }




    IEnumerator FadeInTrainerBattle(TrainerController trainer)
    {

        _gameState = GameState.Battle;
        SoundManager.SharedInstance.PlayMusic(battleLeaderClip);
        yield return transitionPanel.DOFade(1.0f, 1.0f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);

        battleManager.gameObject.SetActive(true);
        worldMainCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleManager.HandleStartTrainerBattle(playerParty, trainerParty);
        yield return transitionPanel.DOFade(0.0f, 1.0f).WaitForCompletion();

    }
    void FinishPokemonBattle(bool playerHasWon)
    {
        if (trainer != null && playerHasWon)
        {
            trainer.AfterTrainerLoseBattle();
            trainer = null;
        }
      StartCoroutine(FadeOutBattle());
    }

    IEnumerator FadeOutBattle()
    {
        yield return transitionPanel.DOFade(1.0f, 1.0f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);
        SoundManager.SharedInstance.PlayMusic(worldClip);
        battleManager.battleGroundImage[2].gameObject.SetActive(false);
        battleManager.gameObject.SetActive(false);
        worldMainCamera.gameObject.SetActive(true);

       
        
        _gameState = GameState.Travel;
        yield return transitionPanel.DOFade(0.0f, 1.0f).WaitForCompletion();




    }


    private void Update()
    {
        if (_gameState == GameState.Travel)
        {
            playerController.HandleUpdate();
        }
        else if (_gameState == GameState.Battle)
        { 
            battleManager.HandleUpdate();
        }
        else if (_gameState == GameState.Dialog)
        {
            DialogManager.SharedInstance.HandleUpdate();
        }
    }
}
