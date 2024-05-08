﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RPG.Network.Controllers;
using RPG.Network.Model;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace RPG.Lobby {
    public class LobbyDataContainer : MonoBehaviour {
        private List<LobbyPack> _lobbies;

        public IEnumerable<LobbyPack> Lobbies => _lobbies;

        public Action OnUpdate;

        public void UpdateList() {
            StartCoroutine(LobbyController.GetLobbyList(OnLoad, OnFail));
        }
        private void OnFail(string error) {
            _lobbies = new List<LobbyPack>();
            // TODO: popup
            Debug.Log(error);
            OnUpdate?.Invoke();
        }
        private void OnLoad(List<LobbyPack> lobbies) {
            _lobbies = lobbies;
            OnUpdate?.Invoke();
        }

        public void ConnectToLobby(ulong lobbyId) {
            StartCoroutine(LobbyController.JoinLobby(new LobbyPayload { RoomID = lobbyId }, OnJoin, OnJoinFailed));
        }
        
        public void ConnectToLobby(ulong lobbyId, string password) {
            StartCoroutine(LobbyController.JoinLobby(new LobbyPayload { RoomID = lobbyId, Password = password}, OnJoin, OnJoinFailed));
        }
        
        private void OnJoin(string obj) {
            var connectionData = JToken.Parse(obj);
            string ip = (string)connectionData["ip"];
            int port = (int)connectionData["port"];
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, (ushort)port);
            NetworkManager.Singleton.StartClient();
        }
        private void OnJoinFailed(string obj) {
            // TODO: popup
            Debug.Log(JToken.Parse(obj)["error_message"]);
        }
    }
}
