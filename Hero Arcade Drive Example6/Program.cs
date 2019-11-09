
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Text;

using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;

//Make sure Axises are right for controller?
//Check firmware of Talons - too old?
//Lights - add over current on a motor, brown out, robot powered on
//Keep reading manual for Emergency Stop and other light indicators for board

namespace Hero_Arcade_Drive {
    public class Program {

        static TalonSRX rightSlave = new TalonSRX(1);
        static TalonSRX right = new TalonSRX(2);
        static TalonSRX leftSlave = new TalonSRX(3);
        static TalonSRX left = new TalonSRX(4);

        static PowerDistributionPanel pdp = new PowerDistributionPanel(0);

        static StringBuilder stringBuilder = new StringBuilder();

        // Has to be in DInput to work - putting on XInput should disable the robot
        static GameController Joy = new GameController(UsbHostDevice.GetInstance());

        // Indicator lights
        static OutputPort low_battery_pin = new OutputPort(CTRE.HERO.IO.Port3.Pin3, false);
        static OutputPort connection_pin = new OutputPort(CTRE.HERO.IO.Port3.Pin4, false);
        static OutputPort current_pin = new OutputPort(CTRE.HERO.IO.Port3.Pin5, false);
        static OutputPort power_pin = new OutputPort(CTRE.HERO.IO.Port3.Pin6, false); //Is code needed for this?
        static OutputPort brownout_pin = new OutputPort(CTRE.HERO.IO.Port3.Pin7, false);

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

            double forward = Joy.GetAxis(1);
            double turn = Joy.GetAxis(4); //Changed from one to four and took out negative
            //double twist = Joy.GetAxis(2); Is this needed?

            Deadzone(ref forward);
            Deadzone(ref turn);
            //Deadzone(ref twist);

            double leftThrot = turn - forward;
            double rightThrot = turn + forward;

            left.Set(ControlMode.PercentOutput, leftThrot);
            leftSlave.Set(ControlMode.PercentOutput, leftThrot);
            right.Set(ControlMode.PercentOutput, rightThrot);
            rightSlave.Set(ControlMode.PercentOutput, rightThrot);

            stringBuilder.Append("\t");
            stringBuilder.Append(forward);
            stringBuilder.Append("\t");
            stringBuilder.Append(turn);
            stringBuilder.Append("\t");
            //stringBuilder.Append(twist);
            //stringBuilder.Append("\t");

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

                // Update indicators
                connection_pin.Write(Joy.GetConnectionStatus() == UsbDeviceConnection.Connected);

                // Fix
                low_battery_pin.Write(true);

                // Fix
                power_pin.Write(true);

                // Fix
                brownout_pin.Write(false);

                // Fix
                current_pin.Write(pdp.GetChannelCurrent(1) > 30);

                Drive();

                Debug.Print(stringBuilder.ToString());
                stringBuilder.Clear();

                // Run this task every 20ms
                Thread.Sleep(20);
            }
        }
    }
}