
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Text;

using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;

//Check firmware of Talons - too old?
//Lights - add over current on a motor, brown out, robot powered on
//Keep reading manual for Emergency Stop and other light indicators for board

namespace Hero_Arcade_Drive {
    public class Program {

        /*static TalonSRX backRight = new TalonSRX(5); //5 was 1 before
        static TalonSRX backLeft = new TalonSRX(2);
        static TalonSRX frontRight = new TalonSRX(3);
        static TalonSRX frontLeft = new TalonSRX(4);*/

        static TalonSRX frontLeft = new TalonSRX(5); //5 was 1 before
        static TalonSRX frontRight = new TalonSRX(2);
        static TalonSRX backLeft = new TalonSRX(3);
        static TalonSRX backRight = new TalonSRX(4);

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
        
        /**************************************************************************************************************************************************/

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

        /**************************************************************************************************************************************************/

        static void Drive() {

            double forward = Joy.GetAxis(1);
            double turn = Joy.GetAxis(2); 

            Deadzone(ref forward);
            Deadzone(ref turn);

            double leftThrot = turn - forward;
            double rightThrot = turn + forward;

            /*backLeft.SetInverted(true);
            frontLeft.SetInverted(true);
            backRight.SetInverted(true);
            frontRight.SetInverted(true);/*

            //double newLThrot = -0.7 * leftThrot;
            //double newRThrot = -0.7 * rightThrot;

            /*backLeft.Set(ControlMode.PercentOutput, newLThrot);
            frontLeft.Set(ControlMode.PercentOutput, newLThrot);
            backRight.Set(ControlMode.PercentOutput, newRThrot);
            frontRight.Set(ControlMode.PercentOutput, newRThrot);*/

            backLeft.Set(ControlMode.PercentOutput, leftThrot * 0.55);
            frontLeft.Set(ControlMode.PercentOutput, leftThrot * 0.55);
            backRight.Set(ControlMode.PercentOutput, rightThrot * 0.55);
            frontRight.Set(ControlMode.PercentOutput, rightThrot * 0.55);

            stringBuilder.Append("\t");
            stringBuilder.Append(forward);
            stringBuilder.Append("\t");
            stringBuilder.Append(turn);
            stringBuilder.Append("\t");

            // Print connection status
            stringBuilder.Append(Joy.GetConnectionStatus());

            stringBuilder.Append(pdp.GetVoltage() + "volts");

        }

        /**************************************************************************************************************************************************/

        public static void Main() {

            while (true) {

                CTRE.Phoenix.Watchdog.Feed();

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
                //connection_pin.Write(Joy.GetConnectionStatus() == UsbDeviceConnection.Connected);

                //Current Limits 
	            backRight.ConfigContinuousCurrentLimit(40, 10);
	            backRight.ConfigPeakCurrentLimit(60, 10);
	            backRight.ConfigPeakCurrentDuration(400, 10);
	            backRight.EnableCurrentLimit(true);

	            backLeft.ConfigContinuousCurrentLimit(40, 10);
	            backLeft.ConfigPeakCurrentLimit(60, 10);
	            backLeft.ConfigPeakCurrentDuration(400, 10);
	            backLeft.EnableCurrentLimit(true);

	            frontRight.ConfigContinuousCurrentLimit(40, 10);
	            frontRight.ConfigPeakCurrentLimit(60, 10);
	            frontRight.ConfigPeakCurrentDuration(400, 10);
	            frontRight.EnableCurrentLimit(true);

	            frontLeft.ConfigContinuousCurrentLimit(40, 10);
	            frontLeft.ConfigPeakCurrentLimit(60, 10);
	            frontLeft.ConfigPeakCurrentDuration(400, 10);
	            frontLeft.EnableCurrentLimit(true);

                /*// Low battery 
                if (pdp.GetVoltage() < 10) {
                    low_battery_pin.Write(true);
                }
                
                else {
                    low_battery_pin.Write(false);
                }

                // Fix
                power_pin.Write(true);

                // Brownout
                if (pdp.GetVoltage() < 8) {
                    brownout_pin.Write(true);
                }

                else {
                    brownout_pin.Write(false);
                }

                // Fix
                current_pin.Write(pdp.GetChannelCurrent(1) > 30);*/

                Drive();
                
                Debug.Print(stringBuilder.ToString());
                stringBuilder.Clear();

                // Run this task every 20ms
                Thread.Sleep(20);
            }
        }
    }
}