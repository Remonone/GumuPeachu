﻿using System;
using System.Collections.Generic;
using System.Linq;
using RPG.Quests.Objectives;
using Object = UnityEngine.Object;

namespace RPG.Quests {
    [Serializable]
    public class QuestState {
        private Quest _quest;
        private Stack<QuestObjective> _objectivesList = new();
        private QuestStore _store;

        private QuestObjective _currentObjective;
        private int _objectiveIndex;

        public bool IsCompleted => _objectivesList.Count < 1;
        public int ObjectiveIndex => _objectiveIndex;
        
        public Quest Quest => _quest;
        
        public QuestState(QuestStore store, Quest quest) {
            _store = store;
            _quest = quest;
            InitQuest();
        }
        
        private void InitQuest() {
            foreach(var objective in _quest.Objectives) _objectivesList.Push(objective);
            var task = _objectivesList.Pop();
            _currentObjective = task;
            task.OnObjectiveCompeted += OnObjectiveFinished;
        }
        
        
        private void OnObjectiveFinished() {
            _objectiveIndex++;
            _store.UpdateState(this);
            _currentObjective.OnObjectiveCompeted -= OnObjectiveFinished;
            Object.Destroy(_currentObjective);
            if (!_objectivesList.TryPop(out var task)) return;
            _currentObjective = task;
        }

    }
}
