﻿using UnityEngine;

namespace RPG.Utils.FSM {
    public interface FSMState {
        void Update(FSM fsm, GameObject go);
    }
}
