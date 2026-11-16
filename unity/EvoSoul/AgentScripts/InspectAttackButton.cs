using System;
using UnityEngine;

var root = GameObject.Find("MobileAttackPreview");
var button = root == null ? null : root.GetComponent<UnityEngine.UI.Button>();
var rect = root == null ? null : root.GetComponent<RectTransform>();
var label = root == null ? null : root.GetComponentInChildren<TMPro.TMP_Text>(true);
var handlers = root == null ? Array.Empty<MobileAttackButton>() : root.GetComponentsInChildren<MobileAttackButton>(true);
UnityEngine.Debug.Log($"ATTACK_RUNTIME_INSPECT: root={(root == null ? "<missing>" : root.name)}|size={(rect == null ? "none" : rect.sizeDelta.ToString())}|labelActive={(label != null && label.gameObject.activeSelf)}|labelText={(label == null ? "<none>" : label.text)}|button={(button != null)}|handlers={handlers.Length}|persistentCalls={(button == null ? -1 : button.onClick.GetPersistentEventCount())}");
