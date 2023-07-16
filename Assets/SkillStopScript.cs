using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class SkillStopScript : MonoBehaviour
{
    private int _moduleId;
    static int _moduleIdCounter = 1;

    public KMNeedyModule Module;
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMSelectable Button;

    private const float DisarmRange = 5.2f;
    private const float AcceptRange = 23.1f;
    private const float NeedleRange = 60;
    private bool Pause;

    void Awake()
    {
        _moduleId = _moduleIdCounter++;
        Button.OnInteract += delegate { if (!Pause) StartCoroutine(ButtonPress()); return false; };
        Module.OnTimerExpired += delegate { Module.HandleStrike(); };
        StartCoroutine(AnimNeedle());
    }

    private IEnumerator ButtonPress()
    {
        Pause = true;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, Button.transform);
        if (Mathf.Abs(Button.transform.localEulerAngles.y - 180) < DisarmRange)
        {
            Audio.PlaySoundAtTransform("solve", Button.transform);
            Module.HandlePass();
        }
        else if (Mathf.Abs(Button.transform.localEulerAngles.y - 180) >= AcceptRange)
        {
            Module.HandleStrike();
            Module.HandlePass();
        }
        else
            Audio.PlaySoundAtTransform("bleep", Button.transform);
        float timer = 0;
        while (timer < 0.5f)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        Pause = false;
    }

    private IEnumerator AnimNeedle(float speed = 0.45f)
    {
        Button.transform.localEulerAngles = new Vector3(0, 180 - NeedleRange, 0);
        while (true)
        {
            float timer = 0;
            while (timer < speed)
            {
                yield return null;
                timer += Time.deltaTime;
                while (Pause)
                    yield return null;
                Button.transform.localEulerAngles = new Vector3(0, Easing.InOutSine(timer, 180 - NeedleRange, 180 + NeedleRange, speed), 0);
            }
            timer = 0;
            while (timer < speed)
            {
                yield return null;
                timer += Time.deltaTime;
                while (Pause)
                    yield return null;
                Button.transform.localEulerAngles = new Vector3(0, Easing.InOutSine(timer, 180 + NeedleRange, 180 - NeedleRange, speed), 0);
            }
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "Use '!{0} disarm' to disarm the module.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        if (command != "disarm")
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
        yield return null;
        Audio.PlaySoundAtTransform("solve", Button.transform);
        Module.HandlePass();
    }
}
