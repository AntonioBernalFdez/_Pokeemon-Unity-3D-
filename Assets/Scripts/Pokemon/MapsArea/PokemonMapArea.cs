using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonMapArea : MonoBehaviour
{
    [SerializeField] List<Pokemon> wildPokemons;

    public Pokemon GetRandomWildPokemon()

    {
        var pokemon  = wildPokemons[Random.Range(0,wildPokemons.Count)];
        pokemon.InitPokemon();
        return pokemon;
    }
}
