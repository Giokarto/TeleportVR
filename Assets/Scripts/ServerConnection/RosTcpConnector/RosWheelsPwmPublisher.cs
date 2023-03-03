using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

using Int16Msg = RosMessageTypes.Std.Int16Msg;
using System.Linq;
using System;

public class RosWheelsPwmPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicRoot = "/roboy/pinky/middleware/espchair/wheels/";
    public string leftTopicName, rightTopicName; 
    public float publishMessageFrequency = 0.02f;
    public int PWM_MIN = 0;
    public int PWM_MAX = 30;
    private Int16Msg leftMsg, rightMsg;

    private float timeElapsed;
    


    // Start is called before the first frame update
    void Start()
    {
        leftTopicName = topicRoot + "left";
        rightTopicName = topicRoot + "right";

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Int16Msg>(leftTopicName);
        ros.RegisterPublisher<Int16Msg>(rightTopicName);

        leftMsg = new Int16Msg();
        rightMsg = new Int16Msg();

    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            GetLatestWheelCommand(out leftMsg.data, out rightMsg.data);
            ros.Publish(leftTopicName, leftMsg);
            ros.Publish(rightTopicName, rightMsg);
            timeElapsed = 0;
        }
    }

    void GetLatestWheelCommand(out short leftCmd, out short rightCmd)
    {
        // TODO @zuzkau refactor input manager & uncomment line 54:55
        var linear = 0; // InputManager.Instance.GetControllerJoystick(true).y;
        var angular = 0; //-InputManager.Instance.GetControllerJoystick(false).x;

        double l = linear - angular / 2;
        double r = linear + angular / 2;

        double[] drive = new double[] { l, r };
        drive = drive.Select(x => Math.Min(1, Math.Max(-1, x))).ToArray();

        drive = MinPower(drive, min_angular: 20 / PWM_MAX, max_angular: 31 / PWM_MAX, min_linear: 20 / PWM_MAX);
        double[] pwm = drive.Select(x => Math.Sign(x) * MapPwm(Math.Abs(x), PWM_MIN, PWM_MAX)).ToArray();
        leftCmd = (short) Math.Round(pwm[0]);
        rightCmd = (short) Math.Round(pwm[1]);

    }

    double Map(double x, double start = 0.1, double min_power = 0.1, double max_power = 1)
    {
        double m = (max_power - min_power) / (1 - start);
        if (Math.Abs(x) <= start)
        {
            // cubic hermite interpolation in abs(x) <= start
            double h_01(double t) => -2 * Math.Pow(t, 3) + 3 * Math.Pow(t, 2);
            double h_11(double t) => Math.Pow(t, 3) - Math.Pow(t, 2);
            return Math.Sign(x) * h_01(Math.Abs(x) / start) * min_power + h_11(Math.Abs(x) / start) * start * m;
        }
        else
        {
            // linear interpolation in start < abs(x)
            double linear(double a) => m * (a - start) + min_power;
            return Math.Sign(x) * linear(Math.Abs(x));
        }
    }

    double[] MinPower(double[] x, double min_angular = 0.1, double max_angular = 1, double min_linear = 0.1, double max_linear = 1)
    {
        // rotation & scaling matrices to convert L/R to angular/linear representation
        double[,] R = new double[,] { { 0.5, -0.5 }, { 0.5, 0.5 } };
        double[,] R_inv = new double[,] { { 1, 1 }, { -1, 1 } };
        double ang = R[0, 0] * x[0] + R[0, 1] * x[1];
        double lin = R[1, 0] * x[0] + R[1, 1] * x[1];
        double[] mapped = new double[] { Map(ang, start: 0.01, min_power: min_angular, max_power: max_angular), Map(lin, start: 0.05, min_power: min_linear, max_power: max_linear) };
        ang = R_inv[0, 0] * mapped[0] + R_inv[0, 1] * mapped[1];
        lin = R_inv[1, 0] * mapped[0] + R_inv[1, 1] * mapped[1];
        return new double[] { ang, lin };
    }

    double MapPwm(double x, double out_min, double out_max)
    {
        return x * (out_max - out_min) + out_min;
    }




}
