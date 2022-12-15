using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.UI;

public enum BattleState
{
    StartBattle,
    ActionSelection,
    MovementSelection,
    PerformMovement,
    RunTurn,
    Busy,
    YesNoChoice,
    PartySelectScreen,
    ForgetMovement,
    FinishBattle,


}

public enum BattleAction
{
    Move,SwitchPokemon,UseItem,Run
}
public enum BattleType
{
    WildPokemon,
    Trainer,
    Leader
}

public class BattleManager : MonoBehaviour
{
    [SerializeField]  BattleUnit playerUnit;
    

    [SerializeField] BattleUnit enemyUnit;

    public Animator _animator;

    [SerializeField] BattleDialogBox battleDialogBox;

    [SerializeField] PartyHUD partyHUD;

    [SerializeField] GameObject pokeball;

    public List<ParticleSystem> particles;

    

    [SerializeField] private Image playerImage, trainerImage;

    public List<Image> battleGroundImage;

    [SerializeField] SelectionMovementUi selectMoveUI;
    private MoveBase move;
    
    private float timeSinceLastClick;
    private float timeBetweenClicks = 0.25f;

    private int currentSelectedAction;
    private int currentSelectedMovement;
    private int currentSelectedPokemon;
    private bool currentSelectedChoice = true;

    private int escapeAttemps;
    private MoveBase moveToLearn;

    public PlayerController player;
    private TrainerController trainer;

    public BattleState state;
    public BattleState? previousState;

    public BattleType type;

    public AudioClip attackClip, damageClip, levelUpClip, pokeballClip, pressAClip, escapeClip;

    public event System.Action<bool> OnBattleFinish;

    private PokemonParty playerParty;
    private PokemonParty trainerParty;
    private Pokemon wildPokemon;
    
    public void HandleStartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        type = BattleType.WildPokemon;
        escapeAttemps = 0;
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon; 
        StartCoroutine(SetUpBattle());
    }

    public void HandleStartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty, bool IsLeader = false)
    {
        type = (IsLeader? BattleType.Leader :  BattleType.Trainer);
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
      
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();
        
        StartCoroutine(SetUpBattle());
    }
    public IEnumerator SetUpBattle()


    {
        state = BattleState.StartBattle;
        playerUnit.ClearHUD();
        enemyUnit.ClearHUD();
        if (type == BattleType.WildPokemon)
        {
            battleGroundImage[1].gameObject.SetActive(false);
            battleGroundImage[0].gameObject.SetActive(true);
            playerUnit.SetUpPokemon(playerParty.GetFirstAlivePokemon());
            battleDialogBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
            enemyUnit.SetUpPokemon(wildPokemon);
           
            battleDialogBox.ToggleActions(false);
            yield return battleDialogBox.SetDialog($"¡Un  {enemyUnit.Pokemon.Base.Name}  Salvaje  apareció!");
        }
        else
        {
            battleGroundImage[1].gameObject.SetActive(true);
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            var playerInitialPosition  = playerImage.transform.localPosition;    
            playerImage.transform.localPosition = playerInitialPosition - new Vector3(400f,0,0);
            playerImage.transform.DOLocalMoveX(playerInitialPosition.x, 1f);

            var trainerInitialPosition = trainerImage.transform.localPosition;
            trainerImage.transform.localPosition = trainerInitialPosition - new Vector3(-400f, 0, 0);
            trainerImage.transform.DOLocalMoveX(trainerInitialPosition.x, 1f);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.PlayerSprite;
            trainerImage.sprite = trainer.TrainerSprite;

            yield return battleDialogBox.SetDialog($"¡{trainer.TrainerName} quiere luchar!");
            yield return new WaitForSeconds(1.0f);

            //Enviar el primer pokemon del rival entrenador 
           
            yield return trainerImage.transform.DOLocalMoveX(trainerImage.transform.localPosition.x+400, 1f).WaitForCompletion();
            trainerImage.gameObject.SetActive(false);
            
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetFirstAlivePokemon();
       
            enemyUnit.SetUpPokemon(enemyPokemon);
            yield return battleDialogBox.SetDialog($"{trainer.TrainerName} envió a {enemyPokemon.Base.Name}");
            trainerImage.transform.localPosition = trainerInitialPosition;
            yield return new WaitForSeconds(1.0f);

            //Enviar el primer pokemon del jugador
            _animator.SetBool("IsThrowing", true);
            yield return new WaitForSeconds(0.5f);

            yield return playerImage.transform.DOLocalMoveX(playerImage.transform.localPosition.x - 400, 2f);
            var playerPokemon = playerParty.GetFirstAlivePokemon();
            yield return battleDialogBox.SetDialog($"Ve  {playerPokemon.Base.Name}");

            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
           
            playerUnit.SetUpPokemon(playerPokemon);
            battleDialogBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
            
            yield return new WaitForSeconds(1.0f);
            playerImage.transform.localPosition = playerInitialPosition;    
        }



        partyHUD.InitPartyHUD();

        yield return new WaitForSeconds(0.5f);
        
        
        PlayerActionSelection();




    }
    IEnumerator BattleFinish(bool playerHasWon)
    {
        if(!playerHasWon)
        {
            
            player.transportPlayer(474.35f, 31.6f, 0,-1);
            playerParty.healFaintedPokemon();
          
            
            state = BattleState.FinishBattle;
            StartCoroutine(battleDialogBox.SetDialog("No te quedan mas Pokemon...Pierdes el conocimiento..."));
            yield return new WaitForSeconds(2f);
           
        }

        state = BattleState.FinishBattle;
        playerParty.Pokemons.ForEach(p => p.OnBattleFinish());
        OnBattleFinish(playerHasWon);
        





    }


    

    void PlayerActionSelection()
    {
        state = BattleState.ActionSelection;
        StartCoroutine(battleDialogBox.SetDialog("Selecciona una acción: "));
        battleDialogBox.ToggleDialogText(true);
        battleDialogBox.ToggleActions(true);
        battleDialogBox.ToggleMovements(false);
        currentSelectedAction = 0;
        battleDialogBox.SelectAction(currentSelectedAction);

    }
     
    void PlayerMovementSelection()
    {
        state = BattleState.MovementSelection;
        battleDialogBox.ToggleDialogText(false);
        battleDialogBox.ToggleActions(false);
        battleDialogBox.ToggleMovements(true);
        currentSelectedMovement = 0;
        battleDialogBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);
    }
    IEnumerator YesNoChoice(Pokemon newTrainerPokemon)
    {
        state = BattleState.Busy;
        yield return battleDialogBox.SetDialog($"{trainer.TrainerName} va a sacar a {newTrainerPokemon.Base.Name}. ¿Quieres cambiar tu Pokemon?");  
        state = BattleState.YesNoChoice;
        battleDialogBox.ToggleYesNoBox(true);
    }
    void OpenPartySelectionScreen()
    {
        state = BattleState.PartySelectScreen;
        partyHUD.SetPartyData(playerParty.Pokemons);
        partyHUD.gameObject.SetActive(true);
        currentSelectedPokemon = playerParty.GetPositionFromPokemon(playerUnit.Pokemon);
        partyHUD.UpdateSelectedPokemon(currentSelectedPokemon);

    }
    

    public void HandleUpdate()
    {
        timeSinceLastClick += Time.deltaTime;
        if (timeSinceLastClick < timeBetweenClicks || battleDialogBox.isWriting)
        {
            return;
        }
       
        if (state == BattleState.ActionSelection)
        {
            HandlePlayerActionSelection();
        }
        else if(state == BattleState.MovementSelection)
        {
            HandlePlayerMovementSelection();
        }
        else if (state == BattleState.PartySelectScreen)
        {
            HandlePlayerPartySelection();
        }
        else if(state == BattleState.YesNoChoice)  
        {
            HandleYesNoChoice();
        }
       
        
        else if (state == BattleState.ForgetMovement)
        {
            selectMoveUI.HandleForgetMoveSelection((moveIndex) =>
            {
                if (moveIndex < 0)
                {
                    timeSinceLastClick = 0;
                    return;
                }

                 StartCoroutine(ForgetOldMove(moveIndex));
            });
           
        }
    }

    IEnumerator ForgetOldMove(int moveIndex)
    {
        selectMoveUI.gameObject.SetActive(false);
        if (moveIndex == PokemonBase.Number_Of_Learnable_Moves)
        {
            yield return 
            battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} no ha aprendido {moveToLearn.Name}.");
        }
        else
        {
            var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
          yield return  battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} " +
                $"olvidó {selectedMove.Name} y aprendió {moveToLearn.Name}.");
            playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);

        }

        moveToLearn = null;
       
        state = BattleState.FinishBattle;
    }
  
    void HandlePlayerActionSelection()

    {
        if(timeSinceLastClick < timeBetweenClicks)
        {
            return;
        }


        if (Input.GetAxisRaw("Vertical")!=0)
        {   
            timeSinceLastClick = 0;
            currentSelectedAction = (currentSelectedAction + 2) % 4;

           battleDialogBox.SelectAction(currentSelectedAction);
        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedAction = (currentSelectedAction + 1) % 2 +
                                    2 * Mathf.FloorToInt(currentSelectedAction / 2);
            battleDialogBox.SelectAction(currentSelectedAction);
        }


        if (Input.GetAxisRaw("Submit") != 0)
        {
            SoundManager.SharedInstance.PlaySound(pressAClip);
            timeSinceLastClick = 0;
            if (currentSelectedAction == 0)
            {
                PlayerMovementSelection();
            }
            else if (currentSelectedAction == 1)
            {
                previousState = state;
                OpenPartySelectionScreen();
            }
            else if (currentSelectedAction == 2)
            {
                StartCoroutine(RunTurns(BattleAction.UseItem));
            }
            else if (currentSelectedAction == 3)
            {
                StartCoroutine(RunTurns(BattleAction.Run));
            }

        }
     }

    
    void  HandlePlayerMovementSelection()
    {
        if (timeSinceLastClick < timeBetweenClicks)
        {
            return;
        }
        if (Input.GetAxisRaw("Vertical")!= 0)
        {
            timeSinceLastClick = 0;
            var oldSelectedMovement = currentSelectedMovement;
            currentSelectedMovement = (currentSelectedMovement + 2) % 4;
            if (currentSelectedMovement >= playerUnit.Pokemon.Moves.Count)
            {
                currentSelectedMovement = oldSelectedMovement;
            }
            Debug.Log(currentSelectedMovement + "-"+oldSelectedMovement);
            battleDialogBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);
        }
       

        else if(Input.GetAxisRaw("Horizontal")!= 0)
        {
            timeSinceLastClick = 0;
            var oldSelectedMovement = (currentSelectedMovement+1)%2 + 2 * Mathf.FloorToInt(currentSelectedMovement)%2 ;
            
            if (currentSelectedMovement <= 1)
            {
                currentSelectedMovement = (currentSelectedMovement + 1) % 2;
            }
            else //currentSelectedMovement >= 2
                {
                 currentSelectedMovement = (currentSelectedMovement + 1) % 2 +2;
                }
           
            if (currentSelectedMovement >= playerUnit.Pokemon.Moves.Count)
            {
                currentSelectedMovement = oldSelectedMovement;
            }
            battleDialogBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);
        }
        if (Input.GetAxisRaw("Submit")!=0)
        {
            SoundManager.SharedInstance.PlaySound(pressAClip);
            timeSinceLastClick = 0;
            battleDialogBox.ToggleMovements(false);
            battleDialogBox.ToggleDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        if (Input.GetAxisRaw("Cancel") != 0)
        {
         
            PlayerActionSelection();
        }
    }
    void HandlePlayerPartySelection()
    {
        if (timeSinceLastClick < timeBetweenClicks)
        {
            return;
        }

        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedPokemon -= (int)Input.GetAxisRaw("Vertical") * 2;

        }


        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            timeSinceLastClick = 0;
            currentSelectedPokemon += (int)Input.GetAxisRaw("Horizontal");

        }
        currentSelectedPokemon = Mathf.Clamp(currentSelectedPokemon, 0, playerParty.Pokemons.Count - 1);
        partyHUD.UpdateSelectedPokemon(currentSelectedPokemon);
        Debug.Log(currentSelectedPokemon);

        if (Input.GetAxisRaw("Submit") != 0)
        {
            SoundManager.SharedInstance.PlaySound(pressAClip);
            timeSinceLastClick = 0;
            var selectedPokemon = playerParty.Pokemons[currentSelectedPokemon];
            if (selectedPokemon.HP <= 0)
            {
                partyHUD.SetMessage("No puedes enviar in pokemon debilitado.");
                return;
            }
            else if (selectedPokemon == playerUnit.Pokemon)
            {
                partyHUD.SetMessage("No puedes seleccionar el pokemon en batalla.");
                return;
            }


            partyHUD.gameObject.SetActive(false);
            battleDialogBox.ToggleActions(false);

            if (previousState == BattleState.ActionSelection)
            {
                previousState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedPokemon));
            }
           


        }
        if (Input.GetAxisRaw("Cancel") != 0)
        {

            if(playerUnit.Pokemon.HP <= 0)
            {
                partyHUD.SetMessage("Tienes que seleccionar un pokemon para continuar...");
                return;
            }
            partyHUD.gameObject.SetActive(false);

            if(previousState == BattleState.YesNoChoice)
            {
                previousState=null;
                StartCoroutine(SendNextTrainerPokemonBattle());
            }
            else
            {
                PlayerActionSelection();
            }
            
        }
    }
    void HandleYesNoChoice()
    {
        if (Input.GetAxisRaw("Vertical")!=0)
      {
         timeSinceLastClick = 0;
         currentSelectedChoice = !currentSelectedChoice;
        
      }
        battleDialogBox.SelectYesNoAction(currentSelectedChoice);

        if (Input.GetAxisRaw("Submit") != 0)
        {
            SoundManager.SharedInstance.PlaySound(pressAClip);
            timeSinceLastClick = 0;
            battleDialogBox.ToggleYesNoBox(false);
            if (currentSelectedChoice)
            {
                previousState = BattleState.YesNoChoice;
                OpenPartySelectionScreen();
            }
            else
            {
                StartCoroutine(SendNextTrainerPokemonBattle());
            }
            
        }
        if (Input.GetAxisRaw("Cancel") != 0)
        {
            timeSinceLastClick = 0;
            battleDialogBox.ToggleYesNoBox(false);
            StartCoroutine(SendNextTrainerPokemonBattle());
        }


    }
    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunTurn;
        if (playerAction == BattleAction.Move)
        {
            
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentSelectedMovement];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.RandomMove();
            Debug.Log(enemyUnit.Pokemon.CurrentMove.Base.Name);
            bool playerGoesFirst = true;
            int enemyPriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;
            int playerPriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            if (enemyPriority > playerPriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyPriority == playerPriority)
            {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }

            

            var firstUnit = playerGoesFirst ? playerUnit : enemyUnit;
            var secondUnit = playerGoesFirst ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;
            //primer turno
            yield return RunMovement(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.FinishBattle)
            {
               yield break;
            }

            if (secondPokemon.HP > 0)
            {
                //segundo turno
                yield return RunMovement(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.FinishBattle)
                {
                    yield break;
                }
            }
            
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = playerParty.Pokemons[currentSelectedPokemon];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                battleDialogBox.ToggleActions(false);
                yield return ThrowPokeball();
            }else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscapeFromBattle();   
            }
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.RandomMove();
            yield return RunMovement(enemyUnit, playerUnit, enemyUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.FinishBattle)
            {
                yield break;
            }
        }
        if (state != BattleState.FinishBattle)
        {
            PlayerActionSelection();
        }
    }


    

    IEnumerator RunMovement(BattleUnit attacker,BattleUnit target, Move move)
    {
        bool canRunMovement = attacker.Pokemon.OnStartTurn();
        if (!canRunMovement)
        {
            yield return ShowStatsMessages(attacker.Pokemon);
             attacker.Hud.UpdatePokemonData();
            yield break;
        }
        yield return ShowStatsMessages(attacker.Pokemon);

        move.Pp--;
        yield return new WaitForSeconds(0.5f);
        yield return battleDialogBox.SetDialog($"{attacker.Pokemon.Base.Name} utilizó {move.Base.Name}.");

        if (MoveHits(move, attacker.Pokemon, target.Pokemon))
        {
            
            yield return RunMoveAnims(attacker, target);
            yield return new WaitForSeconds(0.5f);

            if (move.Base.MoveType == MoveType.Stats)
            {
                yield return RunMoveStats(attacker.Pokemon, target.Pokemon, move.Base.Effects, move.Base.Target);

            }
            else
            {
                var damageDesc = target.Pokemon.ReceiveDamage(attacker.Pokemon, move);
                target.Hud.UpdatePokemonData();
                yield return ShowDamageDescription(damageDesc);
            }
            if (move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count >0)
            {
               foreach (var sec in move.Base.SecondaryEffects)
                {
                    if ((sec.Target == MoveTarget.other && target.Pokemon.HP > 0) || 
                        (sec.Target  == MoveTarget.self && attacker.Pokemon.HP >0))
                    {
                        var rnd = Random.Range(0, 100);
                        if (rnd < sec.Chance)
                        {
                            yield return RunMoveStats(attacker.Pokemon, target.Pokemon,sec,sec.Target);
                        }

                    }
                }
            }

            if (target.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(target);

            }
           
          


        }
        else
        {
            yield return battleDialogBox.SetDialog($"El ataque de {attacker.Pokemon.Base.Name} ha fallado.");
        }


    }
    IEnumerator RunMoveAnims(BattleUnit attacker, BattleUnit target)
    {
        attacker.PlayAttackAnimation();
        yield return new WaitForSeconds(1);
        target.ReceiveAttackAnimation();
    }
    IEnumerator RunMoveStats(Pokemon attacker, Pokemon target , MoveStatEffect effect, MoveTarget moveTarget)
    {    //stats Boosting
        foreach (var boost in effect.Boostings)
        {
            if (boost.target == MoveTarget.self)
            {
                attacker.ApplyBoost(boost);
            }
            else
            {
                target.ApplyBoost(boost);
            }
        }
        //Status Condition
        if (effect.Status != StatusConditionID.none)
        {   if(moveTarget == MoveTarget.other)
            {
                target.SetConditionStatus(effect.Status);
            }
            else
            {
                attacker.SetConditionStatus(effect.Status);
            }
                  
        }
        //Volatile Status Condition
        if (effect.VolatileStatus != StatusConditionID.none)
        {
            if (moveTarget == MoveTarget.other)
            {
                target.SetVolatileConditionStatus(effect.VolatileStatus);
            }
            else
            {
                attacker.SetVolatileConditionStatus(effect.VolatileStatus);
            }
        }



        yield return ShowStatsMessages(attacker);
        yield return ShowStatsMessages(target);
    }
    bool MoveHits(Move move, Pokemon attacker, Pokemon target)
    {
        if (move.Base.AlwaysHit)
        {
            return true;
        }
        float rnd = Random.Range(0, 100);
        float moveAcc = move.Base.Accuracy;

        float precision = attacker.StatsBoosted[Stat.Precision];
        float evasion = target.StatsBoosted[Stat.Evasion];

        float multiplierPre = 1.0f + Mathf.Abs(precision) / 3.0f;
        float multiplierEv = 1.0f + Mathf.Abs(evasion) / 3.0f;

        if (precision >= 0)
        {
            moveAcc *= multiplierPre;
        }
        else
        {
            moveAcc /= multiplierPre;
        }
        if (evasion >= 0)
        {
            moveAcc /= multiplierEv;
        }
        else
        {
            moveAcc *= -multiplierEv;
        }

        return rnd < moveAcc;


    }

    IEnumerator ShowStatsMessages(Pokemon pokemon)
    {
        while(pokemon.StatusChangeMessages.Count > 0)
        {
            var message = pokemon.StatusChangeMessages.Dequeue();
            yield return battleDialogBox.SetDialog(message);
            yield return new WaitForSeconds(1);
        }
    }
    IEnumerator RunAfterTurn(BattleUnit attacker)
    {
        if (state == BattleState.FinishBattle)
        {
            yield break;
        }
        yield return new WaitUntil(() => state == BattleState.RunTurn);
        //comprobar estados alterados como envenenamiento y quemadura.
        attacker.Pokemon.OnFinishTurn();
        yield return ShowStatsMessages(attacker.Pokemon);
        attacker.Hud.UpdatePokemonData();
        if (attacker.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(attacker);

        }
        yield return new WaitUntil(() => state == BattleState.RunTurn);
    }

    void CheckForBattleFinish(BattleUnit faintedUnit)
    { 
       if(faintedUnit.IsPlayer)
        {
            var nextpokemon = playerParty.GetFirstAlivePokemon();
            if (nextpokemon != null)
            {
                OpenPartySelectionScreen();
            }
            else
            {
                
               StartCoroutine( BattleFinish(false));
                


            }
        }
       
       else
        {
            if (type == BattleType.WildPokemon)
            {
                StartCoroutine(BattleFinish(true));


            }
            else
            {
                var nextPokemon = trainerParty.GetFirstAlivePokemon();
               if (nextPokemon != null)
                {
                    StartCoroutine(YesNoChoice( nextPokemon));
                }
                else
                {
                    StartCoroutine(BattleFinish(true));

                }
                
            }
            

        }
    
    }

    IEnumerator SendNextTrainerPokemonBattle()
    {
        state = BattleState.Busy;
        var nextPokemon = trainerParty.GetFirstAlivePokemon();
        enemyUnit.SetUpPokemon(nextPokemon);
        yield return battleDialogBox.SetDialog($"{trainer.TrainerName} ha enviado a {nextPokemon.Base.Name}");
        state = BattleState.RunTurn;
        
    }

    IEnumerator ShowDamageDescription(DamageDescription desc)
    {
        if (desc.Critical > 1f)
        {
            yield return battleDialogBox.SetDialog("¡Un Golpe Critico!");
        }
        if (desc.Type > 1)
        {
            yield return battleDialogBox.SetDialog("!Es super efectivo!");

        }
        else if (desc.Type < 1)
        {
            yield return battleDialogBox.SetDialog("No es muy efectivo...");
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        

        if (playerUnit.Pokemon.HP > 0)
        {    
            yield return battleDialogBox.SetDialog($"Vuelve {playerUnit.Pokemon.Base.Name}! ");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1.5f);
        }

        playerUnit.SetUpPokemon(newPokemon);
       


        battleDialogBox.SetPokemonMovements(newPokemon.Moves);
        yield return battleDialogBox.SetDialog($" ¡ve {newPokemon.Base.Name}!");
        if (previousState == null)
        {
            state = BattleState.RunTurn;
        }
        else if (previousState == BattleState.YesNoChoice)
        {
            yield return SendNextTrainerPokemonBattle();
        }
        //state = BattleState.RunTurn;
       

    }

    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;


        if (type == BattleType.Trainer|| type == BattleType.Leader)
        {
            yield return battleDialogBox.SetDialog("No puedes robar los pokemon de otros entrenadores.");
            state = BattleState.RunTurn;
            yield break;

        }
        yield return battleDialogBox.SetDialog("¡Allá va una Pokeball!");
        
        var pokeballInst = Instantiate(pokeball,playerUnit.transform.position 
                                       + new Vector3(-1.5f,0),Quaternion.identity);

        var pokeballSpt = pokeballInst.GetComponent<SpriteRenderer>();
        



       yield return pokeballSpt.transform.DOLocalJump(enemyUnit.transform.position +
                                        new Vector3(-0.2f, 2.5f) ,2,1, 1f).WaitForCompletion();
       yield return enemyUnit.PlayCapturedAnimation();
       yield return pokeballSpt.transform.DOLocalMoveY
                                (enemyUnit.transform.position.y - 2.7f,0.3f ).WaitForCompletion();
        yield return pokeballSpt.transform.DOLocalMoveY
                          (enemyUnit.transform.position.y - 2.4f, 0.1f).WaitForCompletion();
        yield return pokeballSpt.transform.DOLocalMoveY
                             (enemyUnit.transform.position.y - 2.7f, 0.1f).WaitForCompletion();
        yield return pokeballSpt.transform.DOLocalMoveY
                             (enemyUnit.transform.position.y - 2.5f, 0.1f).WaitForCompletion();
        yield return pokeballSpt.transform.DOLocalMoveY
                            (enemyUnit.transform.position.y - 2.7f, 0.1f).WaitForCompletion();
        
         

        var numberOfShakes = TryToCatchPokemon(enemyUnit.Pokemon);

        for (int i = 0; i < Mathf.Min(numberOfShakes, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeballSpt.transform.DOPunchRotation(new Vector3(0,0,15f),0.6f).WaitForCompletion();
            
        }
        if (numberOfShakes == 4)
        {
            particles[0].Play();

            SoundManager.SharedInstance.PlayMusic(pokeballClip);
            yield return battleDialogBox.SetDialog($"¡{enemyUnit.Pokemon.Base.Name} capturado!");
            yield return pokeballSpt.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemonToParty(enemyUnit.Pokemon);

            Destroy(pokeballInst);
            StartCoroutine(BattleFinish(true)); 
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            pokeballSpt.DOFade(0, 0.2f);
            yield return enemyUnit.PlayEscapeAnimation();
            
            if (numberOfShakes < 2 )
            {
                yield return battleDialogBox.SetDialog($"¡{enemyUnit.Pokemon.Base.Name} ha escapado!");
            }
            else
            {
                yield return battleDialogBox.SetDialog("¡Casi lo has atrapado!");
            }
            Destroy(pokeballInst);
            state = BattleState.RunTurn;

        }

    }
    int TryToCatchPokemon(Pokemon pokemon)
    {
        float bonusPokeball = 1;
        float bonusStat = 1;
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * bonusPokeball * bonusStat/(3*pokemon.MaxHp);


    if (a >= 255)
        {
            return 4;
        }

      float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a ));

        int shakeCount = 0;

        while(shakeCount < 4)
        {
            if (Random.Range(0,65535) >=b)
            {
                break;
            }
            else
            {
                shakeCount++;   
            }
        }
        return shakeCount;  
    }

    
   IEnumerator TryToEscapeFromBattle()
    {
        state = BattleState.Busy;
        if (type != BattleType.WildPokemon)
        {
            yield return battleDialogBox.SetDialog("No puedes huir de combates contre entrenadores.");
            state = BattleState.RunTurn;
            yield break;
        }

        escapeAttemps++;

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (playerSpeed >= enemySpeed)
        {
            SoundManager.SharedInstance.PlaySound(escapeClip);
            yield return battleDialogBox.SetDialog("Has escapado con éxito.");
            state = BattleState.FinishBattle;
            yield return new WaitForSeconds(1);
            OnBattleFinish(true);
            yield return new WaitForSeconds(3);
        }
        else
        {
            int oddsScape = (Mathf.FloorToInt(playerSpeed * 128 / enemySpeed) + 30 * escapeAttemps) % 256;
            if (Random.Range(0,256) < oddsScape)
            {
                SoundManager.SharedInstance.PlaySound(escapeClip);
                yield return battleDialogBox.SetDialog("Has escapado con éxito.");
                state = BattleState.FinishBattle;
                yield return new WaitForSeconds(1);
                OnBattleFinish(true);
                yield return new WaitForSeconds(3);
            }
            else
            {
                yield return battleDialogBox.SetDialog("No puedes escapar.");
                state = BattleState.RunTurn;
            }
        }
    }
    IEnumerator HandlePokemonFainted(BattleUnit faintedUnity)
    {
        yield return battleDialogBox.SetDialog($"{faintedUnity.Pokemon.Base.Name} se debilitó.");
        yield return new WaitForSeconds(0.5f);
        faintedUnity.PlayFaintAnimation();
        yield return new WaitForSeconds(1.5f);

        if (!faintedUnity.IsPlayer)
            {
             int expBase = faintedUnity.Pokemon.Base.ExpBase;
             int level = faintedUnity.Pokemon.Level;
             float multiplier = (type == BattleType.WildPokemon ? 1 : 1.5f);
            int wonExp = Mathf.FloorToInt(expBase * level * multiplier / 7);
            playerUnit.Pokemon.Experience += wonExp;
            yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} ha ganado {wonExp} puntos de experiencia.");
            yield return playerUnit.Hud.SetExpSmooth();
            yield return new WaitForSeconds(0.5f);

            while (playerUnit.Pokemon.NeedsToLevelUp())
            {
                
                playerUnit.Hud.SetLevelText();
                playerUnit.Pokemon.HasHpChanged = true;
                playerUnit.Hud.UpdatePokemonData();
                SoundManager.SharedInstance.PlayMusic(levelUpClip);
                yield return battleDialogBox.SetDialog($"¡{playerUnit.Pokemon.Base.Name} ha subido al nivel {playerUnit.Pokemon.Level}! ");

                var newLearnableMove = playerUnit.Pokemon.GetLearnableMoveAtCurrentLevel();
                if (newLearnableMove != null)
                {
                    if (playerUnit.Pokemon.Moves.Count < PokemonBase.Number_Of_Learnable_Moves )
                    {
                        playerUnit.Pokemon.LearnMove(newLearnableMove);
                        yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} ha aprendido{newLearnableMove.Move.Name}");
                        
                        battleDialogBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} intenta aprender {newLearnableMove.Move.Name}");
                        yield return battleDialogBox.SetDialog($"Pero no Puedo aprender más de {PokemonBase.Number_Of_Learnable_Moves} movimientos");
                        yield return ChooseMovementToForget(playerUnit.Pokemon, newLearnableMove.Move);
                        yield return new WaitUntil(() => state != BattleState.ForgetMovement);
                    }

                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }


         }



        CheckForBattleFinish(faintedUnity);
    }
    IEnumerator ChooseMovementToForget(Pokemon learner ,MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return battleDialogBox.SetDialog("Selecciona el movimiento que quieres olvidar: ");
        selectMoveUI.gameObject.SetActive(true);
        selectMoveUI.SetMovements(learner.Moves.Select(mv => mv.Base).ToList(),newMove);
        moveToLearn = newMove;
        state = BattleState.ForgetMovement;
    }
}

   






 


