﻿using RPG.Combat.DamageDefinition;
using RPG.Combat.Modifiers.BaseTypes;
using RPG.Core.Predicate;
using RPG.Utils;
using UnityEngine;

namespace RPG.Combat.Modifiers {
    [CreateAssetMenu(menuName = "GumuPeachu/Combat/Create On Hit Event Modifier")]
    public class OnHitEventModifier : Modification {
        
        protected Health Performer;
        [ReadOnly] [TextArea] public string returnArguments = "(0): attacker, (1): damage, (2): type, (3): target";

        public override void RegisterModification(GameObject performer) {
            Performer = performer.GetComponent<Health>();
            Performer.EventStorage.Subscribe<DamageReport>("OnHitEvent", OnHit);
        }
        private void OnHit(DamageReport report) {
            var attackerID = ((PredicateMonoBehaviour)report.Attacker.GetComponent(_performerComponent)).ComponentID;
            var targetID = ((PredicateMonoBehaviour)report.Target.GetComponent(_performToComponent)).ComponentID;
            var preparedString = string.Format(_actionPredicate, attackerID, report.Damage, report.Type,
                targetID);
            PredicateWorker.ParsePredicate(preparedString, Performer.ComponentID);
        }
        public override void UnregisterModification() {
            Performer.EventStorage.Unsubscribe<DamageReport>("OnHitEvent", OnHit);
        }
    }
}
