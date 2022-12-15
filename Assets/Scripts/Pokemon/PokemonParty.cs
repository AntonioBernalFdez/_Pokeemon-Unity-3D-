using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PokemonParty : MonoBehaviour
{

    
    

    [SerializeField] private List<Pokemon> pokemons;
    public const int MaxPokemonsInParty = 6;

    public List<Pokemon> Pokemons
      {


        get => pokemons;



      }
    private void Start()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.InitPokemon();
        }
    }
    public Pokemon GetFirstAlivePokemon()
    {
       foreach (var pokemon in pokemons)
        {
            if (pokemon.HP > 0)
            {
                return pokemon;
            }

        }
        return null;
    }

    public int GetPositionFromPokemon(Pokemon pokemon)
    {
        for (int i = 0; i < Pokemons.Count; i++)
        {

            if (pokemon == Pokemons[i])
            {
                return  i;
            }
        }
        return -1;
    }

  
    public void AddPokemonToParty(Pokemon pokemon)
    {
        if(pokemons.Count < MaxPokemonsInParty)
        {
            pokemons.Add(pokemon);
            
        }
        else
        {
            //Pc de Bill
        }


    }
   

    public void healFaintedPokemon()
    {
        foreach (var pokemon in pokemons)
        {
            if (pokemon.HP <= 0)
            {
                pokemon.HP = pokemon.MaxHp;
                pokemon.CureStatusCondition();
                pokemon.CureVolatileStatusCondition();
            }

        }
       
    }


    public void healPokemon()
    {
        foreach (var pokemon in pokemons)
        {
          
                pokemon.HP = pokemon.MaxHp;
                pokemon.CureStatusCondition();
                pokemon.CureVolatileStatusCondition();


        }

    }

}
