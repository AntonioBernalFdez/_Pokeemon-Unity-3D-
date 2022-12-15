using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class StatusConditionsFactory : MonoBehaviour
{

    public static void InitFactory()
    {
        foreach (var condition in StatusConditions)
        {
            var id = condition.Key;
            var scond = condition.Value;
            scond.id = id;
        }
    }

    public static Dictionary<StatusConditionID, StatusCondition> StatusConditions { get; set; } =
        new Dictionary<StatusConditionID, StatusCondition>()
        { {
            StatusConditionID.psn,
            new StatusCondition()
            {
                Name = "Poison",
                Description = "Hace que el pokemon sufra daño en cada turno",
                StartMessage = "ha sido envenado.",
                OnFinishTurn = PoisonEffect
            }
          },
          {
                 StatusConditionID.brn,
            new StatusCondition()
            {
                Name = "Burn",
                Description = "Hace que el pokemon sufra daño en cada turno",
                StartMessage = "ha sido quemado.",
                OnFinishTurn = BurnEffect
            }
          },
          {
            StatusConditionID.slp,
            new StatusCondition()
            {
                Name = "Sleep",
                Description = "Hace que el Pokemon duerma durante un numero fijo de turnos",
                StartMessage = "se ha quedado dormido",
                OnApplyStatusCondition = (Pokemon pokemon) =>
                {
                    pokemon.StatusNumTurns = Random.Range(1,4);
                    Debug.Log($"El pokemon dormirña {pokemon.StatusNumTurns} turnos");
                },
                OnStartTurn = (Pokemon pokemon) =>
                {
                    if (pokemon.StatusNumTurns <= 0)
                    {
                        pokemon.CureStatusCondition();
                        pokemon.StatusChangeMessages.Enqueue($"¡{pokemon.Base.Name} ha despertado!");
                        return true;

                    }
                    pokemon.StatusNumTurns--;
                    pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sigue dormido.");
                    return false;
                }
            }
          },
             {
                StatusConditionID.par,
                new StatusCondition()
                {
                    Name = "Paralyzed",
                    Description = "Hace que el Pokemon pueda estar paralizado en el turno.",
                    StartMessage = "ha sido paralizado",
                    OnStartTurn = ParalyzedEffect
                }
            },
            {
                StatusConditionID.frz,
                new StatusCondition()
                {
                    Name = "Frozen",
                    Description = "Hace que el Pokemon esté congelado, pero se puede curar aleatoriamente durante un turno.",
                    StartMessage = "ha sido congelado",
                    OnStartTurn = FrozenEffect
                }
            },
                       {
                StatusConditionID.conf,
                new StatusCondition()
                {
                    Name = "Confusión",
                    Description = "Hace que el Pokemon esté confundido y pueda atacarse a si mismo.",
                    StartMessage = "ha sido confundido",
                    OnApplyStatusCondition = (Pokemon pokemon) =>
                    {
                        pokemon.VolatileStatusNumTurns = Random.Range(1, 6);
                        Debug.Log($"El pokemon estará confundido durante {pokemon.VolatileStatusNumTurns} turnos");
                    },
                    OnStartTurn = (Pokemon pokemon) =>
                    {
                        if (pokemon.VolatileStatusNumTurns<=0)
                        {
                            pokemon.CureVolatileStatusCondition();
                            pokemon.StatusChangeMessages.Enqueue($"¡{pokemon.Base.Name} ha salido del estado confusión!");
                            pokemon.StatusChangeMessages.Enqueue($"                      ");
                            return true;
                        }

                        pokemon.VolatileStatusNumTurns--;
                        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sigue confundido.");

                        if (Random.Range(0, 2) == 0)
                        {
                            return true;
                        }
                        //Debemos dañarnos a nosotros mismos por la confusión
                        pokemon.UpdateHp(pokemon.MaxHp/6);
                        pokemon.StatusChangeMessages.Enqueue("¡Tan confuso que se hiere a si mismo!");
                        return false;
                    }
                }
            }
        };






    static void PoisonEffect(Pokemon pokemon)
    {
        pokemon.UpdateHp(pokemon.MaxHp / 8);
        pokemon.StatusChangeMessages.Enqueue($"El veneno resta salud a {pokemon.Base.Name}.  ");
        
    }
    static void BurnEffect(Pokemon pokemon)
    {
        pokemon.UpdateHp(pokemon.MaxHp / 15);
        pokemon.StatusChangeMessages.Enqueue($"La quemadura resta salud a {pokemon.Base.Name}.  ");

    }
    static bool ParalyzedEffect(Pokemon pokemon)
    {
        if (Random.Range(0, 100) < 25)
        {
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} está paralizado y no puede moverse.");
            return false;
        }
        return true;
    }

    static bool FrozenEffect(Pokemon pokemon)
    {
        if (Random.Range(0, 100) < 25)
        {
            pokemon.CureStatusCondition();
            pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} ya no está congelado.");
            return true;
        }

        pokemon.StatusChangeMessages.Enqueue($"{pokemon.Base.Name} sigue congelado.");
        return false;
    }

}

public enum StatusConditionID
{
   none, brn,frz,par,psn,slp,conf
}
