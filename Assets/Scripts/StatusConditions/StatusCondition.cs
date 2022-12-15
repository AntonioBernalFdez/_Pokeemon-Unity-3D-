using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class StatusCondition 
{
   public StatusConditionID id { get; set; }
   public string Name { get; set; }
   public string Description { get; set; }
   public string StartMessage { get; set; }
   public Action<Pokemon> OnApplyStatusCondition { get; set; }
   
   public Func<Pokemon,bool> OnStartTurn { get; set; }  
   public Action<Pokemon> OnFinishTurn { get; set; }




}
