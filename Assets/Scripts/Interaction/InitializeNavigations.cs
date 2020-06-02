using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializeNavigations : MonoBehaviour {
    private PlayMakerFSM m_FSM;


    public void InitializeNavs()
    {
        m_FSM = this.GetComponent<PlayMakerFSM>();
        //UAP_AccessibilityManager.ResetOneFingerSwipeUpHandler();
        //UAP_AccessibilityManager.ResetOneFingerSwipeDownHandler();
        //UAP_AccessibilityManager.ResetLongPressHandler();
        //UAP_AccessibilityManager.ResetDoubleTapHandler();

        ResetAllEvents();

        for (int i = 0; i < m_FSM.FsmEvents.Length; i++)
        {
            if (m_FSM.FsmEvents[i].Name == "OneFingerSwipeUp")
            {
                UAP_AccessibilityManager.ResetOneFingerSwipeUpHandler();
                UAP_AccessibilityManager.OnTapEvent oneFingerSwipeUp = new UAP_AccessibilityManager.OnTapEvent(OneFingerSwipeUpCall);
                UAP_AccessibilityManager.SetOneFingerSwipeUpHandler(oneFingerSwipeUp);
            }
            if (m_FSM.FsmEvents[i].Name == "OneFingerSwipeDown")
            {
                UAP_AccessibilityManager.ResetOneFingerSwipeDownHandler();
                UAP_AccessibilityManager.OnTapEvent oneFingerSwipeDown = new UAP_AccessibilityManager.OnTapEvent(OneFingerSwipeDownCall);
                UAP_AccessibilityManager.SetOneFingerSwipeDownHandler(oneFingerSwipeDown);

            }
            if (m_FSM.FsmEvents[i].Name == "OneFingerDoubleTap")
            {
                UAP_AccessibilityManager.ResetDoubleTapHandler();
                UAP_AccessibilityManager.OnTapEvent oneFingerDoubleTap = new UAP_AccessibilityManager.OnTapEvent(OneFingerDoubleTapCall);
                UAP_AccessibilityManager.SetDoubleTapHandler(oneFingerDoubleTap);

            }
            if (m_FSM.FsmEvents[i].Name == "LongPress")
            {
                UAP_AccessibilityManager.ResetLongPressHandler();
                UAP_AccessibilityManager.OnTapEvent longPress = new UAP_AccessibilityManager.OnTapEvent(LongPressCall);
                UAP_AccessibilityManager.SetLongPressHandler(longPress);

            }
            if (m_FSM.FsmEvents[i].Name == "OneFingerSwipeRight")
            {
                UAP_AccessibilityManager.ResetOneFingerSwipeRightHandler();
                UAP_AccessibilityManager.OnTapEvent oneFingerSwipeRight = new UAP_AccessibilityManager.OnTapEvent(OneFingerSwipeRightCall);
                UAP_AccessibilityManager.SetOneFingerSwipeRightHandler(oneFingerSwipeRight);
            }
            if (m_FSM.FsmEvents[i].Name == "OneFingerSwipeLeft")
            {
                UAP_AccessibilityManager.ResetOneFingerSwipeLeftHandler();
                UAP_AccessibilityManager.OnTapEvent oneFingerSwipeLeft = new UAP_AccessibilityManager.OnTapEvent(OneFingerSwipeLeftCall);
                UAP_AccessibilityManager.SetOneFingerSwipeLeftHandler(oneFingerSwipeLeft);
            }
            if (m_FSM.FsmEvents[i].Name == "TwoFingerSwipeLeft")
            {
                UAP_AccessibilityManager.ResetTwoFingerSwipeLeftHandler();
                UAP_AccessibilityManager.OnTapEvent twoFingerSwipeLeft = new UAP_AccessibilityManager.OnTapEvent(TwoFingerSwipeLeftCall);
                UAP_AccessibilityManager.SetTwoFingerSwipeLeftHandler(twoFingerSwipeLeft);
            }
            if (m_FSM.FsmEvents[i].Name == "TwoFingerSwipeRight")
            {
                UAP_AccessibilityManager.ResetTwoFingerSwipeRightHandler();
                UAP_AccessibilityManager.OnTapEvent twoFingerSwipeRight = new UAP_AccessibilityManager.OnTapEvent(TwoFingerSwipeRightCall);
                UAP_AccessibilityManager.SetTwoFingerSwipeRightHandler(twoFingerSwipeRight);
            }
            if (m_FSM.FsmEvents[i].Name == "TwoFingerSwipeUp")
            {
                UAP_AccessibilityManager.ResetTwoFingerSwipeUpHandler();
                UAP_AccessibilityManager.OnTapEvent twoFingerSwipeUp = new UAP_AccessibilityManager.OnTapEvent(TwoFingerSwipeUpCall);
                UAP_AccessibilityManager.SetTwoFingerSwipeUpHandler(twoFingerSwipeUp);
            }
            if (m_FSM.FsmEvents[i].Name == "TwoFingerSwipeDown")
            {
                UAP_AccessibilityManager.ResetTwoFingerSwipeDownHandler();
                UAP_AccessibilityManager.OnTapEvent twoFingerSwipeDown = new UAP_AccessibilityManager.OnTapEvent(TwoFingerSwipeDownCall);
                UAP_AccessibilityManager.SetTwoFingerSwipeDownHandler(twoFingerSwipeDown);
            }
        }
    }

    private void OneFingerDoubleTapCall()
    {
        m_FSM.SendEvent("OneFingerDoubleTap");
    }

    private void LongPressCall()
    {
        m_FSM.SendEvent("LongPress");
    }

    private void OneFingerSwipeUpCall()
    {
        m_FSM.SendEvent("OneFingerSwipeUp");
    }

    private void OneFingerSwipeDownCall()
    {
        m_FSM.SendEvent("OneFingerSwipeDown");
    }

    private void OneFingerSwipeLeftCall()
    {
        m_FSM.SendEvent("OneFingerSwipeLeft");
    }

    private void OneFingerSwipeRightCall()
    {
        m_FSM.SendEvent("OneFingerSwipeRight");
    }

    private void TwoFingerSwipeUpCall()
    {
        m_FSM.SendEvent("TwoFingerSwipeUp");
    }

    private void TwoFingerSwipeDownCall()
    {
        m_FSM.SendEvent("TwoFingerSwipeDown");
    }

    private void TwoFingerSwipeRightCall()
    {
        Debug.Log("Right" + name);
        m_FSM.SendEvent("TwoFingerSwipeRight");
    }

    private void TwoFingerSwipeLeftCall()
    {
        Debug.Log("Left" + name);
        m_FSM.SendEvent("TwoFingerSwipeLeft");
    }

    private void ThreeFingerSwipeUpCall()
    {
        m_FSM.SendEvent("ThreeFingerSwipeUp");
    }

    private void ThreeFingerSwipeDownCall()
    {
        m_FSM.SendEvent("ThreeFingerSwipeDown");
    }

    public void ResetAllEvents()
    {
        UAP_AccessibilityManager.ResetOneFingerSwipeUpHandler();
        UAP_AccessibilityManager.ResetOneFingerSwipeDownHandler();
        UAP_AccessibilityManager.ResetTwoFingerSwipeLeftHandler();
        UAP_AccessibilityManager.ResetTwoFingerSwipeRightHandler();
        UAP_AccessibilityManager.ResetOneFingerSwipeLeftHandler();
        UAP_AccessibilityManager.ResetOneFingerSwipeRightHandler();
        UAP_AccessibilityManager.ResetTwoFingerSwipeUpHandler();
        UAP_AccessibilityManager.ResetTwoFingerSwipeDownHandler();
        UAP_AccessibilityManager.ResetDoubleTapHandler();
        UAP_AccessibilityManager.ResetLongPressHandler();

    }
}
