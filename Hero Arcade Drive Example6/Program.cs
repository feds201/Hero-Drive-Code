
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Text;

using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;

//Deploy to Board
//Fix IDs of Talons
//Fix Drivetrain
//Keep reading manual for Emergency Stop and other light indicators for board
//Put on Github

namespace Hero_Arcade_Drive {
    public class Program {

        static TalonSRX rightSlave = new TalonSRX(0);
        static TalonSRX right = new TalonSRX(1);
        static TalonSRX leftSlave = new TalonSRX(2);
        static TalonSRX left = new TalonSRX(3);

        static StringBuilder stringBuilder = new StringBuilder();

        // Has to be in DInput to work - putting on XInput should disable the robot
        static GameController Joy = new GameController(UsbHostDevice.GetInstance());

        // Indicator lights
        static OutputPort battery_pin = new OutputPort(CTRE.HERO.IO.Port3.Pin3, false);
        static OutputPort connection_pin = new OutputPort(CTRE.HERO.IO.Port3.Pin6, false);

        // Deadzone taken from FRC bot
        static void Deadzone(ref double value) {

            if (System.Math.Abs(value) < .15) {
                value = 0.0;
            }

            else {
                if (value > 0) {
                    value = (value - .15) / (1 - .15);
                }
                else {
                    value = (value + .15) / (1 - .15);
                }
            }
        }

        static void Drive() {

            double x = Joy.GetAxis(0);
            double y = -1 * Joy.GetAxis(1);
            double twist = Joy.GetAxis(2);

            Deadzone(ref x);
            Deadzone(ref y);
            Deadzone(ref twist);

            double leftThrot = y + twist;
            double rightThrot = y - twist;

            left.Set(ControlMode.PercentOutput, leftThrot);
            leftSlave.Set(ControlMode.PercentOutput, leftThrot);
            right.Set(ControlMode.PercentOutput, -rightThrot);
            rightSlave.Set(ControlMode.PercentOutput, -rightThrot);

            stringBuilder.Append("\t");
            stringBuilder.Append(x);
            stringBuilder.Append("\t");
            stringBuilder.Append(y);
            stringBuilder.Append("\t");
            stringBuilder.Append(twist);
            stringBuilder.Append("\t");

            // Print connection status
            stringBuilder.Append(Joy.GetConnectionStatus());

        }

        public static void Main() {

            while (true) {

                // From PDF - if controller is connected enable HERO Board
                if (Joy.GetConnectionStatus() == UsbDeviceConnection.Connected) {
                    /* feed watchdog to keep Talon's enabled */
                    CTRE.Phoenix.Watchdog.Feed();
                }

                else {
                    stringBuilder.Append("\n"); 
                    stringBuilder.Append("WARNING: Watchdog not fed");
                }

                // Update indicator
                connection_pin.Write(Joy.GetConnectionStatus() == UsbDeviceConnection.Connected);

                // Fix
                battery_pin.Write(true);

                Drive();

                Debug.Print(stringBuilder.ToString());
                stringBuilder.Clear();

                // Run this task every 20ms
                Thread.Sleep(20);
            }
        }
    }
}