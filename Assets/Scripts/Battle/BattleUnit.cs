using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class BattleUnit : MonoBehaviour
{
  public PokemonBase _base;
  private MoveBase _move;

 

    public List<ParticleSystem> particles;
    public List<ParticleSystem> attackParticles;


    public int _level;
  
  [SerializeField] bool isPlayer;
  [SerializeField] BattleHud hud;
  


  [SerializeField] BattleUnit attacker;
    public Pokemon Pokemon { get; set; }

    public bool IsPlayer => isPlayer;
  
    public BattleHud Hud => hud;


    private Image pokemonImage;

    public List<Image> attackBackGroundImage;
   


    private Vector3 initialPosition;

    private Color initialColor;

    [SerializeField]
    float startTimeAnim , attackTimeAnim , dieTimeAnim  , capturedTimeAnim = 0.6f,
    hitTimeAnim ; 

    
    private void Awake()
    {
         

         pokemonImage = GetComponent<Image>();
        initialPosition = pokemonImage.transform.localPosition;
        initialColor = pokemonImage.color; 
    }
    public void SetUpPokemon(Pokemon pokemon)
    {
        Pokemon = pokemon;
  
        pokemonImage.sprite =
            (isPlayer ? Pokemon.Base.BackSprite : Pokemon.Base.FrontSprite);
        pokemonImage.color = initialColor;
        pokemonImage.transform.localPosition = initialPosition;

        hud.gameObject.SetActive(true);
        hud.SetPokemonData(pokemon);
      
       


        PlayStartAnimation();
    }
    public void ClearHUD()
    {
        hud.gameObject.SetActive(false);
    }
    public void  PlayStartAnimation()
    {
     
        
           pokemonImage.transform.localPosition = 
            new Vector3( initialPosition.x+(isPlayer? - 1:1) * 20,initialPosition.y);

        pokemonImage.transform.DOLocalMoveX(initialPosition.x, 1.5f);
    }

    public void PlayAttackAnimation()
    {
        var seq = DOTween.Sequence();



        if (this.Pokemon.CurrentMove.Base.MoveType == MoveType.Special && this.Pokemon.CurrentMove.Base.Type != PokemonType.Psychic)
        {

            seq.Append(attackBackGroundImage[0].DOFade(1, 0.5f)).WaitForCompletion();
            if (this.Pokemon.CurrentMove.Base.Name == "Lanzallamas") { attackParticles[0].Play(); }
            else if (this.Pokemon.CurrentMove.Base.Name == "Burbuja") { attackParticles[1].Play(); }
            else if (this.Pokemon.CurrentMove.Base.Name == "Pistola Agua") { attackParticles[2].Play(); }
            
        }



        

        else if (this.Pokemon.CurrentMove.Base.Name == "Ataque Rapido") 
        {
            seq.Append(pokemonImage.transform.DOLocalMoveX(initialPosition.x + (isPlayer ? 1.5f : -1) * 2, 0.03f));
            seq.Append(pokemonImage.transform.DOLocalMoveX(initialPosition.x, 0.03f));
        }
        else if (this.Pokemon.CurrentMove.Base.Name == "Latigo")
        {
            seq.Append(pokemonImage.transform.DOLocalMoveX(initialPosition.x + (isPlayer ? 0.5f : - 0.5f) * 2, 0.35f));           
            seq.Append(pokemonImage.transform.DOLocalMoveX(initialPosition.x + (isPlayer ? - 0.5f : + 0.5f) * 2, 0.35f));
            seq.Append(pokemonImage.transform.DOLocalMoveX(initialPosition.x, 0.35f)).WaitForCompletion();
        }
        else if (this.Pokemon.CurrentMove.Base.Type == PokemonType.Psychic)
        {
            if (this.Pokemon.CurrentMove.Base.Name == "Psíquico")
            {
                seq.Append(attackBackGroundImage[1].DOFade(1, 0.5f)).WaitForCompletion();
            }


        }

        else
        {
            seq.Append(pokemonImage.transform.DOLocalMoveX(initialPosition.x + (isPlayer ? 0.5f : -0.5f) * 2, 0.35f));
            seq.Append(pokemonImage.transform.DOLocalMoveX(initialPosition.x, 0.35f)).WaitForCompletion();
        }











    }
    public void ReceiveAttackAnimation()
    {
      
        if (attacker.Pokemon.CurrentMove.Base.Name == "Picotazo Venenoso") { particles[2].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Name == "Latigo Cepa") { particles[4].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Name == "Hoja Afilada") { particles[5].Play(); }
    
        if (attacker.Pokemon.CurrentMove.Base.Name == "Somnifero") { particles[7].Play(); }
   
        if (attacker.Pokemon.CurrentMove.Base.Name == "Rayo" || attacker.Pokemon.CurrentMove.Base.Name == "Trueno") { particles[9].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Name == "Polvo Venenoso") { particles[10].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Name == "Onda Trueno") { particles[11].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Name == "Impactrueno") { particles[15].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Name == "Bomba Acida" || attacker.Pokemon.CurrentMove.Base.Name == "Acido") { particles[16].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Name == "Rayo Solar") { particles[17].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Name == "Paralizador") { particles[28].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Type == PokemonType.Fire) { particles[19].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Type == PokemonType.Flying) { particles[20].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Type == PokemonType.Bug) { particles[22].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Type == PokemonType.Ice) { particles[24].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Type == PokemonType.Rock || attacker.Pokemon.CurrentMove.Base.Type == PokemonType.Ground) { particles[26].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Type == PokemonType.Dark) { particles[27].Play(); }

        if (attacker.Pokemon.CurrentMove.Base.Type == PokemonType.Psychic) { particles[13].Play(); }



        if (attacker.Pokemon.CurrentMove.Base.Type == PokemonType.Dragon)
        {
            if (attacker.Pokemon.CurrentMove.Base.Name == "Hiperrayo") 
            {
                particles[12].Play();
            }

            else
            { 
              particles[21].Play();
            }
        }
        

        if (attacker.Pokemon.CurrentMove.Base.Type == PokemonType.Ghost)
        {
            if (attacker.Pokemon.CurrentMove.Base.Name == "Tinieblas")
            {
                particles[0].Play();
            }
            else
            {
                particles[23].Play();
            }

        }

        
        if (attacker.Pokemon.CurrentMove.Base.Type == PokemonType.Water)
        {

            
                particles[18].Play();
            

        }

        if (attacker.Pokemon.CurrentMove.Base.Type == PokemonType.Normal || attacker.Pokemon.CurrentMove.Base.Type == PokemonType.Fight)
        {
            if (attacker.Pokemon.CurrentMove.Base.Name == "Super Sónico")
            {
                particles[13].Play();
            }
            else if (attacker.Pokemon.CurrentMove.Base.Name == "Cuchillada")
            { 
                particles[14].Play(); 
            }
            else if (attacker.Pokemon.CurrentMove.Base.Name == "Pantalla Humo")
            {
                particles[3].Play();
            }

            else if (attacker.Pokemon.CurrentMove.Base.Name == "Rapidez")
            {
                particles[1].Play();
            }
            else if (attacker.Pokemon.CurrentMove.Base.Name == "Doble Filo")
            {
                particles[6].Play();

            }

            else
            {

                particles[25].Play();

            }
        }
           
       

        var seq = DOTween.Sequence();
        seq.Append(pokemonImage.DOColor(Color.black, hitTimeAnim));
        seq.Append(pokemonImage.DOColor(initialColor, hitTimeAnim));
        seq.Append(pokemonImage.DOColor(Color.black, hitTimeAnim));
        seq.Append(pokemonImage.DOColor(initialColor, hitTimeAnim));
        seq.Append(pokemonImage.DOColor(Color.black, hitTimeAnim));
        seq.Append(pokemonImage.DOColor(initialColor, hitTimeAnim)).WaitForCompletion();
       
        seq.Append(attackBackGroundImage[0].DOFade(0, 0.5f)).WaitForCompletion();
        seq.Append(attackBackGroundImage[1].DOFade(0, 0.5f)).WaitForCompletion();
       

    }



    public void PlayFaintAnimation()
    {
        var seq = DOTween.Sequence();
        seq.Append(pokemonImage.transform.DOLocalMoveY(initialPosition.y - 5, dieTimeAnim));
        seq.Join(pokemonImage.DOFade(0f, 0.5f));
    }

    public IEnumerator PlayCapturedAnimation()

    {
        var seq = DOTween.Sequence();
        seq.Append(pokemonImage.DOFade(0, 0.5f));
        seq.Join(transform.DOScale(new Vector3(0.01f,0.01f,0.01f),0.6f));
        seq.Join(transform.DOLocalMoveY(initialPosition.y + 2f, 0.6f));
        
        seq.Append(transform.DOScale(new Vector3(0.03f, 0.03f, 0.03f), 0.6f));
        yield return seq.WaitForCompletion();
        
    }
    public IEnumerator PlayEscapeAnimation()

    {
        var seq = DOTween.Sequence();
        seq.Append(pokemonImage.DOFade(1, 0.5f));
        seq.Join(transform.DOScale(new Vector3(0.03f, 0.03f, 0.03f), 0.6f));
        seq.Join(transform.DOLocalMoveY(initialPosition.y, 0.6f));
        yield return seq.WaitForCompletion();
    }
}
