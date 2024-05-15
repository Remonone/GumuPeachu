﻿using RPG.Lobby;
using RPG.Network.Management.Managers;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.Network.Management {
    [RequireComponent(typeof(NetworkManager))]
    public class ApplicationManager : MonoBehaviour {

        private IManager _manager;
        
        public string Token { get; set; }

        public IManager Manager => _manager;

        public void HostServer(LobbyPack pack) {
            if (!ReferenceEquals(_manager, null)) return;
            _manager = gameObject.AddComponent<HostManager>();
            _manager.Token = Token;
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
            FindObjectOfType<LobbyProcessor>().Init(pack);
            var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
            var model = playerObj.gameObject.transform.GetChild(0);
            model.gameObject.SetActive(false);
        }
        
        
        public void ConnectToServer(string ip, ushort port) {
            if (!ReferenceEquals(_manager, null)) return;
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, port);
            _manager = gameObject.AddComponent<ClientManager>();
            _manager.Token = Token;
            NetworkManager.Singleton.StartClient();
        }
        
    }
}
