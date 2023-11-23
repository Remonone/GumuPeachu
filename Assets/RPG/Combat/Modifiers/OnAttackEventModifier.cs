﻿using RPG.Combat.DamageDefinition;
using RPG.Combat.Modifiers.BaseTypes;
using RPG.Core.Predicate;
using RPG.Utils;
using UnityEngine;

namespace RPG.Combat.Modifiers {
    [CreateAssetMenu(menuName = "GumuPeachu/Combat/Create Fighter Event Modifier")]
    public class OnAttackEventModifier : Modification {
        
        protected Fighter Performer;
        [ReadOnly] [TextArea] public string returnArguments = "(0): attacker, (1): damage, (2): type, (3): target";

        public override void RegisterModification(GameObject performer) {
            Performer = performer.GetComponent<Fighter>();
            Performer.EventStorage.Subscribe<DamageReport>("OnAttack", OnAttackTarget);
        }
        private void OnAttackTarget(DamageReport report) {
            var attackerID = ((PredicateMonoBehaviour)report.Attacker.GetComponent(_performerComponent)).ComponentID;
            var targetID = ((PredicateMonoBehaviour)report.Target.GetComponent(_performToComponent)).ComponentID;
            var preparedString = string.Format(_actionPredicate, attackerID, report.Damage, report.Type,
                targetID);
            PredicateWorker.ParsePredicate(preparedString, Performer.ComponentID);
        }
        public override void UnregisterModification() {
            Performer.EventStorage.Unsubscribe<DamageReport>("AttackTarget", OnAttackTarget);
        }
    }
}
