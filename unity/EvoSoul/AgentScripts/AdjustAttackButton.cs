using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine.UI;

var root = GameObject.Find("MobileAttackPreview");
if (root == null)
    throw new System.InvalidOperationException("MobileAttackPreview was not found in the active scene.");

var rect = root.GetComponent<RectTransform>();
var button = root.GetComponent<Button>();
if (rect == null || button == null)
    throw new System.InvalidOperationException("MobileAttackPreview needs both RectTransform and Button.");

var oldSize = rect.sizeDelta;
rect.sizeDelta = new Vector2(230f, 140f);

var label = root.GetComponentInChildren<TMPro.TMP_Text>(true);
if (label != null)
    label.gameObject.SetActive(false);

var attackController = UnityEngine.Object.FindAnyObjectByType<StageNpcInteractionController>(FindObjectsInactive.Include);
if (attackController == null)
    throw new System.InvalidOperationException("StageNpcInteractionController was not found in the active scene.");

var legacyHandler = root.GetComponent<MobileAttackButton>();
if (legacyHandler != null)
    UnityEngine.Object.DestroyImmediate(legacyHandler);
button.onClick.RemoveAllListeners();
UnityEventTools.AddPersistentListener(button.onClick, attackController.RequestAttack);

EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
Debug.Log($"ATTACK_BUTTON_UPDATED: old={oldSize}; new={rect.sizeDelta}; persistentCalls={button.onClick.GetPersistentEventCount()}; labelActive={(label != null && label.gameObject.activeSelf)}");
