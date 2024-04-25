using System;
using System.Linq;
using Cinemachine;
using Newtonsoft.Json.Linq;
using RPG.Core;
using RPG.Core.Cursors;
using RPG.Movement;
using RPG.Network.Client;
using RPG.Saving;
using RPG.SceneManagement.Network;
using RPG.UI.InfoBar;
using RPG.Utils.Constants;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static RPG.Utils.Constants.DataConstants;

namespace RPG.Creatures.Player {
    public class PlayerController : NetworkBehaviour {
        [SerializeField] private InputActionMap _map;
        [SerializeField] private CursorPreview[] _cursors;
        [SerializeField] private float _cameraRotationModifier = .5f;

        [Header("On Init")] 
        [SerializeField] private GameObject _UiContainer;
        [SerializeField] private GameObject _cameraHolder;
        [SerializeField] private GameObject _followCamera;
        [SerializeField] private Camera _camera;
        [SerializeField] private HpBar _bar;
        
        private string _id;
        private Mover _mover;
        private SavingSystem _system;
        private string _credentials;
        private CinemachineVirtualCamera _behaviour;
        
        // PUBLIC
        public InputActionMap Map => _map;

        private void Awake() {
            _system = FindObjectOfType<SavingWrapper>().System;
        }

        // PRIVATE

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (!IsOwner) return;
            
            _mover = GetComponent<Mover>();
            LoadClientServerRpc(ClientSingleton.Instance.Manager.Credentials);
        }

        [ServerRpc(RequireOwnership = true)]
        private void LoadClientServerRpc(string credentials, ServerRpcParams serverRpcParams = default) {
            StartCoroutine(_system.Load(gameObject, credentials,  data => {
                ClientRpcParams param = new ClientRpcParams {
                    Send = {
                        TargetClientIds = new[] { serverRpcParams.Receive.SenderClientId }
                    }
                };
                _credentials = credentials;
                _id = (string)data[PLAYER_ID];
                InitClientRpc(data.ToString(), param);
            }));
        }
        
        [ClientRpc]
        private void InitClientRpc(string stringifiedData, ClientRpcParams clientRpcParams = default) {
            if (!IsOwner) return;
            var data = JObject.Parse(stringifiedData);
            _id = (string)data[PLAYER_ID];
            int sceneIndex = data[SCENE_INDEX] == null ? PropertyConstants.STARTER_SCENE : (int)data[SCENE_INDEX];
            LoadSceneServerRpc(sceneIndex);
            // BAD SOLUTION
            SaveableEntity entity = GetComponent<SaveableEntity>();
            if (!entity) return;
            entity.RestoreFromJToken(data);
            ///////
            Instantiate(_UiContainer);
            _cameraHolder.SetActive(true);
            _cameraHolder.GetComponentInChildren<Camera>().tag = "MainCamera";
            _behaviour  = Instantiate(_followCamera).GetComponent<CinemachineVirtualCamera>();
            _behaviour.Follow = transform;
            var bar = Instantiate(_bar, transform);
            bar.gameObject.SetActive(false);
            bar.StartInit(_id, gameObject);
            _map.Enable();
        }
        
        [ServerRpc]
        private void LoadSceneServerRpc(int sceneId, ServerRpcParams serverRpcParams = default) {
            FindObjectOfType<SceneDistributor>().LoadScene(sceneId, transform.position, serverRpcParams.Receive.SenderClientId);
        }

        public void LoadScene(int sceneId, Vector3 position) {
            Debug.Log(sceneId);
            if (!IsOwner) return;
            SceneManager.LoadScene(sceneId);
            transform.position = position;
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            if (IsOwner) {
                _map.Disable();
            }
            if (IsServer) {
                _system.Save(GetComponent<SaveableEntity>(), _id, _credentials);
            }
        }

        private void Update() {
            if (!IsOwner) {
                SetCursor(CursorType.EMPTY);
                return;
            }

            if (TransferToDifferentScene()) return;
            if (InteractWithCamera()) return;
            if (InteractWithUI()) return;
            if (InteractWithComponent()) return;
            if (MoveTowardPoint()) return;
            SetCursor(CursorType.EMPTY);
        }
        private bool TransferToDifferentScene() {
            if (!_map["Transfer"].WasPerformedThisFrame()) return false;
            SceneManager.LoadScene("Training Polygon");
            return true;
        }

        private bool InteractWithCamera() {
            if (!_map["Camera Rotation"].IsPressed()) return false;
            var mouseDelta = _map["Mouse Delta"].ReadValue<Vector2>();
            var yMouseDelta = mouseDelta.x * _cameraRotationModifier;
            _behaviour.transform.Rotate(Vector3.up, yMouseDelta, Space.World);
            return true;
        }
        private bool InteractWithUI() {
            var isOverUI = EventSystem.current.IsPointerOverGameObject();
            var shouldActive = isOverUI && _map["Action"].WasPressedThisFrame();
            if (shouldActive) SetCursor(CursorType.UI);
            return shouldActive;
        }

        private bool InteractWithComponent() {
            var hits = SortedRaycast();
            foreach (var hit in hits) {
                var raycastables = hit.transform.GetComponents<ITrajectory>();
                foreach (var raycastable in raycastables) {
                    SetCursor(raycastable.GetCursorType());
                    if (raycastable.HandleRaycast(this)) return true;
                }
            }
            return false;
        }

        private RaycastHit[] SortedRaycast() {
            var hits = Physics.RaycastAll(GetMouseRay());
            var distances = new float[hits.Length];
            for (var i = 0; i < distances.Length; i++) {
                distances[i] = hits[i].distance;
            }
            Array.Sort(distances, hits);
            return hits;
        }
        
        private Ray GetMouseRay() => _camera.ScreenPointToRay(_map["Position"].ReadValue<Vector2>());
        
        private bool MoveTowardPoint() {
            Ray direction = GetMouseRay();
            Physics.Raycast(direction, out var hit, 100F);
            if (ReferenceEquals(hit.collider, null)) {
                SetCursor(CursorType.EMPTY);
                return false;
            }
            SetCursor(CursorType.MOVEMENT);

            if (_map["Action"].WasPressedThisFrame()) _mover.RequestToMove(hit.point);
            return true;
        }
        
        private void SetCursor(CursorType type) {
            var cursor = _cursors.Single(cursor => cursor.Type == type);
            Cursor.SetCursor(cursor.Image, cursor.Hotspot, CursorMode.Auto);
        }
    }

    [Serializable]
    internal sealed class CursorPreview {
        public CursorType Type;
        public Texture2D Image;
        public Vector2 Hotspot;
    }
}
