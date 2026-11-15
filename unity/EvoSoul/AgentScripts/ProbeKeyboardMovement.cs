using System;
using System.Reflection;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif

public static class ProbeKeyboardMovement
{
    public static object Run()
    {
        var player = GameObject.Find("VirginDropPlayer");
        var controller = player != null ? player.GetComponent<VirginDropPlayerController>() : null;
        if (controller == null)
            throw new InvalidOperationException("VirginDropPlayerController is unavailable.");

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null)
            throw new InvalidOperationException("No active keyboard device is available to the Input System.");

        var readMovement = typeof(VirginDropPlayerController).GetMethod("ReadMovement", BindingFlags.Instance | BindingFlags.NonPublic);
        if (readMovement == null)
            throw new InvalidOperationException("ReadMovement is unavailable.");

        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.W));
        InputSystem.Update();
        var wInput = (Vector2)readMovement.Invoke(controller, null);

        InputSystem.QueueStateEvent(Keyboard.current, default(KeyboardState));
        InputSystem.Update();

        return new { keyboard = Keyboard.current.name, wInput, acceptsW = wInput.y > 0.9f };
#else
        return new { supported = false, reason = "ENABLE_INPUT_SYSTEM is disabled." };
#endif
    }
}
