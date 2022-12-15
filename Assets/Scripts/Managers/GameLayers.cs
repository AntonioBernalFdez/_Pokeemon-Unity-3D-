using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] private LayerMask solidObjectsLayer, pokemonLayer, interactable,playerLayer,fovLayer,door;

    public LayerMask SolidObjectsLayer => solidObjectsLayer;
    public LayerMask PokemonLayer => pokemonLayer;
    public LayerMask Interactable=> interactable;

    public LayerMask PlayerLayer => playerLayer;
    public LayerMask FovLayer => fovLayer;
    public LayerMask Door => door;

    public static GameLayers SharedInstance;

    private void Awake()
    {
        if (SharedInstance == null)
        {
            SharedInstance = this;
        }
    }

    public LayerMask CollisionLayers => SolidObjectsLayer | Interactable | PlayerLayer;

}
