using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

var root = GameObject.Find("MobileAttackPreview");
var button = root == null ? null : root.GetComponent<Button>();
var attackController = UnityEngine.Object.FindAnyObjectByType<StageNpcInteractionController>();
if (root == null || button == null || attackController == null)
    throw new InvalidOperationException("Attack button or attack controller is missing in Play Mode.");

var queuedField = typeof(StageNpcInteractionController).GetField(
    "attackQueued",
    BindingFlags.Instance | BindingFlags.NonPublic);
if (queuedField == null)
    throw new InvalidOperationException("Could not inspect the attack queue.");

queuedField.SetValue(attackController, false);
button.onClick.Invoke();
var queued = (bool)queuedField.GetValue(attackController);
if (!queued)
    throw new InvalidOperationException("The attack button did not queue the selected NPC attack.");

var rect = root.GetComponent<RectTransform>();
var label = root.GetComponentInChildren<TMPro.TMP_Text>(true);
Debug.Log($"ATTACK_BUTTON_VALIDATED: size={rect.sizeDelta}; queued={queued}; labelActive={(label != null && label.gameObject.activeSelf)}");
