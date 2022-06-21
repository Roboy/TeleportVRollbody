﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Widgets;

public class InputManager : Singleton<InputManager>
{
    public List<UnityEngine.XR.InputDevice> controllerLeft = new List<UnityEngine.XR.InputDevice>();
    public List<UnityEngine.XR.InputDevice> controllerRight = new List<UnityEngine.XR.InputDevice>();
    public HandManager handManager;

    [SerializeField] VRGestureRecognizer vrGestureRecognizer;

    private bool lastMenuBtn;
    private bool lastGrabLeft;
    private bool lastGrabRight;
    bool nodded, waiting;

    void Awake()
    {
        GetLeftController();
        GetRightController();

        vrGestureRecognizer.Nodded += OnNodded;
        vrGestureRecognizer.HeadShaken += OnHeadShaken;
    }

    private bool GetControllerAvailable(bool leftController)
    {
        return leftController ? GetLeftControllerAvailable() : GetRightControllerAvailable();
    }

    public InputDevice GetController(bool leftController)
    {
        return leftController ? controllerLeft[0] : controllerRight[0];
    }

    /// try to get the left controller, if possible.<!-- return if the controller can be referenced.-->
    public bool GetLeftControllerAvailable()
    {
        if (controllerLeft.Count == 0)
        {
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left, controllerLeft);
        }
        return controllerLeft.Count > 0;
    }

    /// try to get the right controller, if possible.<!-- return if the controller can be referenced.-->
    public bool GetRightControllerAvailable()
    {
        if (controllerRight.Count == 0)
        {
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right, controllerRight);
        }
        return controllerRight.Count > 0;
    }

    /// try to get the left controller, if possible.<!-- return if the controller can be referenced.-->
    public bool GetLeftController()
    {
        if (controllerLeft.Count == 0)
        {
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Left, controllerLeft);
        }
        return controllerLeft.Count > 0;
    }

    /// try to get the right controller, if possible.<!-- return if the controller can be referenced.-->
    public bool GetRightController()
    {
        if (controllerRight.Count == 0)
        {
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Right, controllerRight);
        }
        return controllerRight.Count > 0;
    }

    public bool GetControllerBtn(InputFeatureUsage<bool> inputFeature, bool leftController)
    {
        if (GetControllerAvailable(leftController))
        {
            if (GetController(leftController).TryGetFeatureValue(inputFeature, out var btn))
            {
                return btn;
            }
        }

        return false;
    }

    public bool GetAnyControllerBtnPressed()
    {
        var ret = false;
        var ctrls = new List<bool> { true, false };
        foreach (var left in ctrls)
        {
            if (GetControllerAvailable(left))
            {
                GetController(left).TryGetFeatureValue(CommonUsages.primaryButton, out var btn1);
                GetController(left).TryGetFeatureValue(CommonUsages.secondaryButton, out var btn2);
                ret = ret || btn1 || btn2;
            }
        }
        return ret;
    }

    public Vector2 GetControllerJoystick(bool isLeft)
    {
        if (GetControllerAvailable(isLeft))
        {
            GetController(isLeft).TryGetFeatureValue(CommonUsages.primary2DAxis, out var joystick);
            return joystick;
        }
        return Vector2.zero;
    }

    void OnNodded()
    {
        nodded = true;
        Debug.LogError("Yes");
    }

    void OnHeadShaken()
    {
        Debug.LogError("no");
    }

    IEnumerator WaitForNod()
    {
        Debug.Log("waiting for a nod");
        waiting = true;
        nodded = false;
        yield return new WaitUntil(() => nodded);
        waiting = false;
        if (Training.TutorialSteps.Instance.waitingForNod)
        {
            Debug.Log("user confirmed");
            Training.TutorialSteps.Instance.Next();
        }
    }

    /// <summary>
    /// Handle input from the controllers.
    /// </summary>
    void Update()
    {

        //UnityAnimusClient.Instance.emotion_get();
        //if (StateManager.Instance.currentState == StateManager.States.HUD)
        //    UnityAnimusClient.Instance.EnableMotor(true);
        //else
        //    UnityAnimusClient.Instance.EnableMotor(false);

        if (!Widgets.WidgetInteraction.settingsAreActive)
        {
            bool btn;
            if (GetLeftController())
            {
                // Go to the other mode
                if (controllerLeft[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out btn) && btn && !lastMenuBtn)
                {
                    StateManager.Instance.GoToNextState();
                }
                lastMenuBtn = btn;

                // update the emotion buttons
                if (controllerLeft[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out btn))
                {
                    UnityAnimusClient.Instance.LeftButton1 = btn;
                    //UnityAnimusClient.Instance.currentEmotion = "shy";
                }
                if (controllerLeft[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out btn))
                {
                    UnityAnimusClient.Instance.LeftButton2 = btn;
                    //UnityAnimusClient.Instance.currentEmotion = "hearts";
                }

                if (Training.TutorialSteps.Instance != null &&
                    StateManager.Instance.currentState == StateManager.States.Training)
                {
                    // check if the arm is grabbing
                    if (Training.TutorialSteps.Instance.currentState == Training.TutorialSteps.TrainingStep.LEFT_HAND)
                    {
                        if (controllerLeft[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.Next();
                        }

                        if (controllerLeft[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out btn) &&
                           btn)
                        {
                            Training.TutorialSteps.Instance.CorrectUser("grip");
                        }
                    }

                    if (Training.TutorialSteps.Instance.currentState == Training.TutorialSteps.TrainingStep.RIGHT_HAND)
                    {
                        if (controllerRight[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.Next();
                        }
                        if (controllerRight[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.CorrectUser("grip");
                        }
                    }

                    // check left arm
                    if (Training.TutorialSteps.Instance.currentState == Training.TutorialSteps.TrainingStep.LEFT_ARM)
                    {
                        if (controllerLeft[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.CorrectUser("index");
                        }
                    }

                    //// check right arm
                    if (Training.TutorialSteps.Instance.currentState == Training.TutorialSteps.TrainingStep.RIGHT_ARM)
                    {

                        //if (controllerRight[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out btn) &&
                        //    btn)
                        //{
                        //    Training.TutorialSteps.Instance.NextStep(praise: true);
                        //}

                        if (controllerRight[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.CorrectUser("index");
                        }
                    }

                    //if (Training.TutorialSteps.Instance.currentStep == Training.TutorialSteps.TrainingStep.RIGHT_HAND)
                    //{
                    //    if (controllerRight[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out btn) &&
                    //        btn)
                    //    {
                    //        Training.TutorialSteps.Instance.CorrectUser("grip");
                    //    }
                    //}

                    //if (Training.TutorialSteps.Instance.currentStep == Training.TutorialSteps.TrainingStep.LEFT_HAND)
                    //{
                    //    if (controllerLeft[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out btn) &&
                    //        btn)
                    //    {
                    //        Training.TutorialSteps.Instance.CorrectUser("grip");
                    //    }
                    //}
                }

                //drive the wheelchair
#if !SENSEGLOVE
                //if (//StateManager.Instance.currentState == StateManager.States.Construct ||
                //    StateManager.Instance.currentState != StateManager.States.HUD)
                //{
                //    Vector2 joystick;
                //    if (controllerLeft[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out joystick))
                //    {
                //        bool wheelchairIsActive = joystick.sqrMagnitude > 0.01f;

                //        // Show that the wheelchair is active in the state manager
                //        WidgetInteraction.SetBodyPartActive(56, wheelchairIsActive);

                //        //if (wheelchairIsActive)
                //        //{
                //        //    if (StateManager.Instance.currentState == StateManager.States.Training &&
                //        //        Training.TutorialSteps.Instance.currentState == Training.TutorialSteps.TrainingStep.WHEELCHAIR)
                //        //    {
                //        //        Training.TutorialSteps.Instance.Next();
                //        //    }
                //        //}

                //        float speed = 0.05f;
                //        // move forward or backwards
                //        Debug.Log($"joystick y x: {joystick.y} {joystick.x}");
                //        DifferentialDriveControl.Instance.V_L = speed * joystick.y;
                //        DifferentialDriveControl.Instance.V_R = speed * joystick.y;

                //        //rotate
                //        if (joystick.x > 0)
                //        {
                //            DifferentialDriveControl.Instance.V_R -= 0.5f * speed * joystick.x;
                //        }
                //        else
                //        {
                //            DifferentialDriveControl.Instance.V_L += 0.5f * speed * joystick.x;
                //        }
                //    }
                //}
                //else
                //{
                //    // Show that the wheelchair is active in the state manager
                //    WidgetInteraction.SetBodyPartActive(56, false);
                //}
#endif
            }
            else
            {
                if (UnityAnimusClient.Instance != null)
                {
                    UnityAnimusClient.Instance.LeftButton1 = Input.GetKeyDown(KeyCode.F);
                    UnityAnimusClient.Instance.LeftButton2 = Input.GetKeyDown(KeyCode.R);
                }
            }
            if (GetRightController())
            {
                // update the emotion buttons
                if (controllerRight[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out btn))
                {
                    UnityAnimusClient.Instance.RightButton1 = btn;
                }
                if (controllerRight[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out btn))
                {
                    UnityAnimusClient.Instance.RightButton2 = btn;
                }

                if ( //StateManager.Instance.currentState == StateManager.States.Construct ||
                    StateManager.Instance.currentState == StateManager.States.Training)
                {
                    if (Training.TutorialSteps.Instance.currentState == Training.TutorialSteps.TrainingStep.HEAD)
                    {
                        if (!waiting & Training.TutorialSteps.Instance.waitingForNod) StartCoroutine(WaitForNod());
                        // if (nodded)


                    }
                }
            }
            else
            {
                if (UnityAnimusClient.Instance != null)
                {
                    UnityAnimusClient.Instance.RightButton1 = Input.GetKeyDown(KeyCode.G);
                    UnityAnimusClient.Instance.RightButton2 = Input.GetKeyDown(KeyCode.T);
                }
            }
        }
    }

}
