#region Hardcode Header
/*
 * Created by SharpDevelop.
 * User: David
 * Date: 3/8/2008
 * Time: 1:10 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Phidgets;
using Phidgets.Events;
using Microsoft.DirectX.DirectInput;
using Microsoft.DirectX;


namespace ODonel_Simple_ROV_Controller
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	/// 
	#endregion
	public partial class MainForm
	{

		#region hardcode A
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}

		
	   	#region Declarations
	   	
	   	int deadzone = 100; //Deadzone value. used to keep small movements from the joystick from moving the ROV.
	   	
        bool joyfound = false; //This is used to allow the program to start up without needing to have a joystick plugged in.
        public static JoystickState state = new JoystickState(); //Direct X uses this to store relevant information about the joystick.
        private Device applicationDevice = null; //normally directX has a similar line elsewhere, but to have the modified startup mentioned above, this step was neccessary.
        
        public static int numPOVs = 0; //these are properties of the joystick
        int[] pov;//these are properties of the joystick
		byte[] buttons;//these are properties of the joystick
		
 		AdvancedServo servo1; //OOP.   Servo is a class that allows easy control of variables and functions related to the control of the motors or servos.
 		
 		int Phidget_Max = 231; //defining device max and min values is a good practice.
		int Phidget_Min = -23;//defining device max and min values is a good practice.
		bool servo1on; //we want to make sure that the system knows if we have a Phidget Motor controller attached(the property given by the phidget does not work correctly).
		
		const byte START = 0x80; //defining any constant values that need to be reference is also a good practice
        const byte DEVICE = 0x01;//defining any constant values that need to be reference is also a good practice
        const byte COMMAND = 0x04;//defining any constant values that need to be reference is also a good practice
        
		int Pololu_Max = 5500;//defining device max and min values is a good practice.
		int Pololu_Min = 500;//defining device max and min values is a good practice.
		
		//Customized program variables		
		int camAngle = 3000;        //Sometimes you may need to make changes depending on specific needs of the software, and ROV specifications.
		int camPulse;				//Sometimes you may need to make changes depending on specific needs of the software, and ROV specifications.
		bool pololuEnable = true;	//Sometimes you may need to make changes depending on specific needs of the software, and ROV specifications.
		bool joyDelay = true;		//Sometimes you may need to make changes depending on specific needs of the software, and ROV specifications.
        bool joyDelay2 = true;		//Sometimes you may need to make changes depending on specific needs of the software, and ROV specifications.
		int camSwitch=0;			//Sometimes you may need to make changes depending on specific needs of the software, and ROV specifications.
		int altCamSwitch = 0;		//Sometimes you may need to make changes depending on specific needs of the software, and ROV specifications.
        int counter = 0;

        int xAxisDirection = 1;
        int yAxisDirection = 1;
        int zAxisDirection = 1;
        int rxAxisDirection = 1;

        int camMaxAngle = 4480;
        int camMinAngle =1440;



		#endregion
		
		#region Servo Functions
		int Limit_Device(int variable, int max, int min){
				if(variable > max){variable = max;} 
				if(variable < min){variable = min;}  
				return variable;
		}
		byte[] Create_Message(int variable, int servonum){
		
			byte lsb = (byte)(variable & 0x7F);
            byte msb = (byte)(variable >> 7);
            
            byte servo = (byte)((int)(servonum));
            byte[] message = { START, DEVICE, COMMAND, servo, msb, lsb };
		return message;
		}
		void servo1_Attach(object sender, AttachEventArgs e){
	        servo1on = true;
	        
	    }
		void servo1_Detach(object sender, DetachEventArgs e){
			servo1on = false;
			
		}
		
		#endregion
		
		#region Joystick function
		     public bool InitDirectInput()
	        {
	            // Enumerate EasternEdgeControl2007s in the system.
	            foreach (DeviceInstance instance in Microsoft.DirectX.DirectInput.Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly))
	            {
	                // Create the device.  Just pick the first one
	                applicationDevice = new Device(instance.InstanceGuid);
	                break;
	            }
                joyfound = true;	
	            if (null == applicationDevice)
	            {
	                joyfound = false;
	                //return false;
	            }
	          if (joyfound){
	            // Set the data format to the c_dfDIEasternEdgeControl2007 pre-defined format.
	            applicationDevice.SetDataFormat(DeviceDataFormat.Joystick);
	            // Set the cooperative level for the device.
	            applicationDevice.SetCooperativeLevel(this, CooperativeLevelFlags.Exclusive | CooperativeLevelFlags.Foreground);
	            // Enumerate all the objects on the device.
	            foreach (DeviceObjectInstance d in applicationDevice.Objects)
	            {
	                // For axes that are returned, set the DIPROP_RANGE property for the
	                // enumerated axis in order to scale min/max values.
	
	                if ((0 != (d.ObjectId & (int)DeviceObjectTypeFlags.Axis)))
	                {
	                    // Set the range for the axis.
	                    applicationDevice.Properties.SetRange(ParameterHow.ById, d.ObjectId, new InputRange(-1000, +1000));
	                }
	                // Update the controls to reflect what
	                // objects the device supports.
	                UpdateControls(d);
	            }}
	            return true;
	            }     
	   		 public void GetData()
	        {
	            // Make sure there is a valid device.
	            if (null == applicationDevice)
	                return;
	            try
	            {
	                // Poll the device for info.
	                applicationDevice.Poll();
	            }
	            catch(InputException inputex)
	            {
	                if ((inputex is NotAcquiredException) || (inputex is InputLostException))
	                {
	                    // Check to see if either the app
	                    // needs to acquire the device, or
	                    // if the app lost the device to another
	                    // process.
	                    try
	                    {
	                        // Acquire the device.
	                        applicationDevice.Acquire();
	                    }
	                    catch(InputException)
	                    {
	                        // Failed to acquire the device.
	                        // This could be because the app
	                        // doesn't have focus.
	                        return;
	                    }
	                }
	            } //catch(InputException inputex)
	            // Get the state of the device.
	            try {state = applicationDevice.CurrentJoystickState;}
	            
	                // Catch any exceptions. None will be handled here, 
	                // any device re-aquisition will be handled above.  
	            catch(InputException)
	            {
	                return;
	            }
	           // UpdateUI();
	        }     
	  		 private void UpdateUI()
			{
				
				string strText = null;
				int[] slider = state.GetSlider();
				int[] pov = state.GetPointOfView();
				// Fill up text with which buttons are pressed
				byte[] buttons = state.GetButtons();
				int button = 0;
				foreach (byte b in buttons)
				{
					if (0!= (b & 0x80))
						strText += button.ToString("00 ");
					button++;
				}
			}
			 public void UpdateControls(DeviceObjectInstance d)
			{
				//this is where'd you'd check for stuff that the joystick has, but we don't need it.		
			}
			        		   		
    	double func_axis_limit(double axisvalue){
			 	if((axisvalue < deadzone)&&(axisvalue > -deadzone)){axisvalue = 0;}
			 	
			 	
			 	
	        if (axisvalue >= 1000){axisvalue = 1000;}
	        if (axisvalue <= -1000){axisvalue = -1000;}  	
	        return axisvalue;	
	    }
		#endregion
		
		
		
		
		
		#region Init
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void MainFormLoad(object sender, System.EventArgs e)
		{
            FindAvailableSerialPorts();
            MakeThrusterGraphs();

			#region Servo init
			servo1 = new AdvancedServo();
			servo1.Attach += new AttachEventHandler(servo1_Attach);
			try{
			servo1.open();
			}
			catch (PhidgetException pe)
            {
                MessageBox.Show(pe.ToString());
            }
			#endregion
			
			//InitDirectInput();

            cameraAngle1.MaxAngle = camMaxAngle;
            cameraAngle1.MinAngle = camMinAngle;
            cameraAngle1.FlipGraph = true;

            errorCheck.Enabled = true;
            joystickDetectionTimer.Enabled = true;
			timer1.Enabled = true;
			if(serialPort1.IsOpen){serialPort1.Close();}

            LoadSettings();

		}

        private void LoadSettings()
        {
            string port = Properties.Settings.Default.PololuPort;
            if (portBox.Items.Contains(port)) {
                portBox.SelectedItem = port;
            }
            bool revX = Properties.Settings.Default.ReverseX;
            bool revY = Properties.Settings.Default.ReverseY;
            bool revZ = Properties.Settings.Default.ReverseZ;
            bool revR = Properties.Settings.Default.ReverseRx;

            reverseX.Checked = revX;
            reverseY.Checked = revY;
            reverseZ.Checked = revZ;
            reverseR.Checked = revR;

            bool slider = Properties.Settings.Default.UseSlider;
            sliderRadio.Checked = slider;
            axisRadio.Checked = !slider;

            uint sway = Properties.Settings.Default.SwayButton;
            swayButtonNUD.Value = sway;
            
        }
        private void SaveSettings()
        {
            Properties.Settings.Default.PololuPort = portBox.Text;
            Properties.Settings.Default.ReverseX = reverseX.Checked;
            Properties.Settings.Default.ReverseY = reverseY.Checked;
            Properties.Settings.Default.ReverseZ = reverseZ.Checked;
            Properties.Settings.Default.ReverseRx = reverseR.Checked;
            Properties.Settings.Default.UseSlider = sliderRadio.Checked;
            Properties.Settings.Default.SwayButton = (uint)swayButtonNUD.Value;
            Properties.Settings.Default.Save();
        }

        PowerLevelBar plb1 = new PowerLevelBar();
        PowerLevelBar plb2 = new PowerLevelBar();
        PowerLevelBar plb3 = new PowerLevelBar();
        PowerLevelBar plb4 = new PowerLevelBar();

        private void MakeThrusterGraphs()
        {
            

            plb1.Dock = DockStyle.Fill;
            plb2.Dock = DockStyle.Fill;
            plb3.Dock = DockStyle.Fill;
            plb4.Dock = DockStyle.Fill;

            Label lb1 = new Label();
            Label lb2 = new Label();
            Label lb3 = new Label();
            Label lb4 = new Label();

            lb1.TextAlign = ContentAlignment.BottomCenter;
            lb2.TextAlign = ContentAlignment.BottomCenter;
            lb3.TextAlign = ContentAlignment.BottomCenter;
            lb4.TextAlign = ContentAlignment.BottomCenter;

            lb1.Text = "VP";
            lb2.Text = "HP";
            lb3.Text = "HS";
            lb4.Text = "VS";

            tableLayoutPanel1.Controls.Add(lb1);
            tableLayoutPanel1.Controls.Add(lb2);
            tableLayoutPanel1.Controls.Add(lb3);
            tableLayoutPanel1.Controls.Add(lb4);

            tableLayoutPanel1.SetColumn(lb1, 0);
            tableLayoutPanel1.SetColumn(lb2, 1);
            tableLayoutPanel1.SetColumn(lb3, 2);
            tableLayoutPanel1.SetColumn(lb4, 3);

            tableLayoutPanel1.SetRow(lb1, 1);
            tableLayoutPanel1.SetRow(lb2, 1);
            tableLayoutPanel1.SetRow(lb3, 1);
            tableLayoutPanel1.SetRow(lb4, 1);

            tableLayoutPanel1.Controls.Add(plb1);
            tableLayoutPanel1.Controls.Add(plb2);
            tableLayoutPanel1.Controls.Add(plb3);
            tableLayoutPanel1.Controls.Add(plb4);

            tableLayoutPanel1.SetColumn(plb1, 0);
            tableLayoutPanel1.SetColumn(plb2, 1);
            tableLayoutPanel1.SetColumn(plb3, 2);
            tableLayoutPanel1.SetColumn(plb4, 3);

            tableLayoutPanel1.SetRow(plb1, 0);
            tableLayoutPanel1.SetRow(plb2, 0);
            tableLayoutPanel1.SetRow(plb3, 0);
            tableLayoutPanel1.SetRow(plb4, 0);


            plb1.PowerLevel = 0;
            plb2.PowerLevel = 0;
            plb3.PowerLevel = 0;
            plb4.PowerLevel = 0;
        }

        private void FindAvailableSerialPorts()
        {
            System.IO.Ports.SerialPort sp = new System.IO.Ports.SerialPort();
            int maxCheck = 100;
            for (int i = 0; i < maxCheck; i++) {
                bool available = true;
                sp.PortName = "COM" + i;
                try { sp.Open(); }
                catch { available = false; }
                finally { sp.Close(); }
                if (available) { portBox.Items.Add(sp.PortName); }
            }
            
        }
		#endregion
		
		void Timer1Tick(object sender, System.EventArgs e)
		{
			
			
			GetData();  //retrieve updated values from the joystick
			if(servo1on){ //we check to see if our Phidget is still connected.
	        motorController.Text = "Connected";  //display the status of our phidget
	        }
	        else{
	        motorController.Text = "Not Connected"; //display the status of our phidget
	        }
			if(joyfound){ //This variable was set as either true or false back in a directX function
			joystick.Text = "Connected";  //we will display this information as well
                rawAxis.Text=
				"X:" + state.X + "\r\n"  +
				"Y:" + state.Y + "\r\n"  +
				"Z:" + state.Z + "\r\n"  +
				"Rx:" + state.Rx + "\r\n"+
				"Ry:" + state.Ry + "\r\n"+
				"Rz:" + state.Rz + "\r\n";
                rawAxis.Text += "POV: " + state.GetPointOfView()[0] + "\r\n";  //another value sent out for debugging.
			}
			else{
			joystick.Text = "Not Connected";//we will also display if there is no joystick connected
			}
			if(!serialPort1.IsOpen){ //we will also need to check to make sure that our Pololu is connected.
				try{
				serialPort1.Open();   //try/catch allows us to try to something which could crash the program, but continue on even if it fails.
				}
				catch(Exception 
                    c){}
			}
			int HorzPort; 				//there are some variable that we might need for this section, which are commonly used in ROVs
			int VertPort;				//there are some variable that we might need for this section, which are commonly used in ROVs
			int VertStar;				//there are some variable that we might need for this section, which are commonly used in ROVs
			int HorzStar;				//there are some variable that we might need for this section, which are commonly used in ROVs
			int[] servo = new int[8];	//there are some variable that we might need for this section, which are commonly used in ROVs
			
			#endregion
			
			//;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
			//
			//Customize this section for your control Scheme.
			//
			//;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
			
			//pololuEnable = false;  	//the pololu unit, is coded to automatically parse through values and send it out. 
										//setting this variable to false, allows the student to write thier own process for the pololu unit.
										//it defaults to true, which is the current state.

            int slider = (int)state.GetSlider()[0];


            pov = state.GetPointOfView();   //we would like to have input from the "top-hat" on the joystick.
            buttons = state.GetButtons();   //we would also like to have input from the various joystick buttons.

            bool[] pressed = new bool[buttons.Length];    	//the native format of button input from the joystick, is not a program friendly way
            for (int i = 0; i < buttons.Length; i++)
            {			// to read it.
                if (buttons[i] == 128) { pressed[i] = true; }	//what we will do is a little bit of programming magic, to make it more useable.
                else
                {
                    pressed[i] = false;
                }
            }

			HorzPort = (yAxisDirection*state.Y) + (rxAxisDirection*state.Rz); // Sets variable for PulseWidthModulation using the joysticks
            HorzStar = (yAxisDirection * state.Y) - (rxAxisDirection * state.Rz); // Sets variable for PWM using the joysticks

            //determine if the sway button has been pressed
            bool swayMode = false;
            int swayButton = (int)swayButtonNUD.Value;
            if (pressed[swayButton])
            {
                swayMode = true;
            }


            
            int swayComponent = (xAxisDirection * state.X);
            if (!swayMode) {
                swayComponent = 0;
            }


            if (axisRadio.Checked)
            {
                VertPort = (zAxisDirection * state.Z) + (swayComponent);  // Sets variable for PWM using the joysticks
                VertStar = (zAxisDirection * state.Z) - (swayComponent);  // Sets variable for PWM using the joysticks
            }else{
                VertPort = (zAxisDirection * slider) + (swayComponent);  // Sets variable for PWM using the joysticks
                VertStar = (zAxisDirection * slider) - (swayComponent);  // Sets variable for PWM using the joysticks
            }

			/*
			if(pressed[0]&&pressed[1]){beltSwitch = 572 ;}  //we want to do certain things when different joystick buttons are pressed.
			else{beltSwitch = -40 ;}
            */

            /*
			if(pressed[2]){									//we want to do certain things when different joystick buttons are pressed.
				if(!joyDelay){
					if(camSwitch == 572){camSwitch = -40;}  //this is a toggle of a relay. notice that it determines it's current state
					else{camSwitch = 572;}					// and uses that to decide what to do next.
				joyDelay = true;
				}
			}
			if(!pressed[2]){
				joyDelay = false;}
            */
            PerformJoystickClick(pressed[2], ref joyDelay, ref camSwitch);
            PerformJoystickClick(pressed[3], ref joyDelay2, ref altCamSwitch);

            /*
            if (pressed[3])
            {									//we want to do certain things when different joystick buttons are pressed.
                if (!joyDelay2)
                {
                    if (altCamSwitch == 572) { altCamSwitch = -40; }  //this is a toggle of a relay. notice that it determines it's current state
                    else { altCamSwitch = 572; }					// and uses that to decide what to do next.
                    joyDelay2 = true;
                }
            }
            if (!pressed[3])
            {
                joyDelay2 = false;
            }
            */



//			if(camSwitch == 572){axis.Text+="Camera B" + "\r\n";}else{axis.Text+="Camera A" + "\r\n";}  //we also want to display these values to help us debug our program.
//			if(altCamSwitch ==572){axis.Text+="Alt A" + "\r\n";}else{ axis.Text+="Alt B" + "\r\n";}	//we also want to display these values to help us debug our program.
			
            Image img = new Bitmap(cameraDiagram.Width,cameraDiagram.Height);
            CameraVisualizer.DrawVisual(ref img, (camSwitch != 572), (altCamSwitch == 572));
            cameraDiagram.Image = img;



			
			
			//camAngle = (int)((2.5)*(camAngle) + 3000);//analyze everything as a joystick input.
		    
		    int midPoint = 3040;
            int jumpAmt = 150;  // 10 is slow but easy to control. 25 is a bit faster, much choppier, still easy to control.
            //50 is a nicer speed,just as choppy as 25, same ease of use. 100 seems to match a slider, still choppy though. 150 is perfect. 200 deoesn't give any real improvement.
            //we want to know when a button has been released. after a delay of x, then modify the camera angle, however if the button is released, then restart the count.

            int delay = 1;
            
//            axis.Text += "counter: " + counter  + "\r\n";  //another value sent out for debugging.
            //axis.Text += "POV: " + hat + "\r\n";  //another value sent out for debugging.
            int hat = pov[0];
            if (hat == -1) { counter = 0; }
            else { counter++; }

            if (counter == delay)
            {

                if (hat == 0) { camAngle-=jumpAmt; }        //controlling a motor with poportional control is difficult without using a proportional input.
                if (hat == 9000) { camAngle = midPoint; }											//so we can work out a simple way to use buttons to control.
                if (hat == 18000) { camAngle+=jumpAmt; }
                if (hat == 27000) { camAngle = midPoint; }

                counter = 0;
            }

            camAngle = Limit_Device(camAngle, camMaxAngle, camMinAngle);  //many of the devices we may use have hardware based max and min values,
															// it is always good practice to ensure that they never get a value outside of their range.
            cameraAngle1.Angle = camAngle;
			
			
			int camVal = (int)((2.5)*(camSwitch) + 3000);//analyze everything as a joystick input.
			int altCamVal = (int)((2.5)*(altCamSwitch) + 3000);//analyze everything as a joystick input.
			
			//camAngle = (int)camBar.Value;




          


			//the array of Servo objects, is used by the pololu, remember earlier when we mentioned the pololuEnable variable,
			// this is how we would normally set the values for the pololu.
			servo[0] = camAngle;
			servo[1] = camVal; //battleswitch 1
			servo[2] = altCamVal; // battleswitch 2
			servo[3] = 3000;  //3000 is the midpoint on the device and corresponds to a neutral position.
			servo[4] = 3000;  //For example, when dealing with motors, this is a stop position.
			servo[5] = 3000; 
			servo[6] = 3000;
			servo[7] = 3000;
			
			
			
		
			
			
			
			
			//;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
			//
			//Do not Edit past this point.
			//
			//;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;			

		#region hardcode B
			
			
			#region Output
			if(operate.Checked){

                label6.Text = "On";
                label6.ForeColor = Color.Green;

			HorzPort = (int)func_axis_limit(HorzPort);
			VertPort = (int)func_axis_limit(VertPort);
			VertStar = (int)func_axis_limit(VertStar);
			HorzStar = (int)func_axis_limit(HorzStar);

            plb1.PowerLevel = VertPort;
            plb2.PowerLevel = HorzPort;
            plb3.PowerLevel = HorzStar;
            plb4.PowerLevel = VertStar;
/*
			axis.Text +=  "HorzPort: " + HorzPort + "\r\n";
			axis.Text += "VertPort: " + VertPort + "\r\n";
			axis.Text += "VertStar: " + VertStar + "\r\n";
			axis.Text += "HorzStar: " + HorzStar + "\r\n";
*/			
			if(servo1on){
                try
                {
                    servo1.servos[0].Position = Limit_Device((int)(((0.052) * (HorzPort)) + 119), Phidget_Max, Phidget_Min);//sets the hardware's pin 0.
                    servo1.servos[1].Position = Limit_Device((int)(((0.052) * (VertPort)) + 119), Phidget_Max, Phidget_Min);//sets the hardware's pin 1.
                    servo1.servos[2].Position = Limit_Device((int)(((0.052) * (VertStar)) + 119), Phidget_Max, Phidget_Min);//sets the hardware's pin 2.
                    servo1.servos[3].Position = Limit_Device((int)(((0.052) * (HorzStar)) + 119), Phidget_Max, Phidget_Min);//sets the hardware's pin 3.
                        //
                    /*
                    servo1.servos[4].Position = Limit_Device((int)(((0.052) * (HorzPort)) + 119), Phidget_Max, Phidget_Min);//sets the hardware's pin 4.
                    servo1.servos[5].Position = Limit_Device((int)(((0.052) * (VertPort)) + 119), Phidget_Max, Phidget_Min);//sets the hardware's pin 5.
                    servo1.servos[6].Position = Limit_Device((int)(((0.052) * (VertStar)) + 119), Phidget_Max, Phidget_Min);//sets the hardware's pin 6.
                    servo1.servos[7].Position = Limit_Device((int)(((0.052) * (HorzStar)) + 119), Phidget_Max, Phidget_Min);//sets the hardware's pin 7.
                    */
                    }
                catch(Exception r) {
                }
			}
			
			if(pololuEnable){
			for(int i =0;i<8;i++){
				int temp = Limit_Device(servo[i], Pololu_Max, Pololu_Min);
				byte[] message = Create_Message(temp , i);
					if(serialPort1.IsOpen){
            			serialPort1.Write(message , 0, 6);
            		}
			}
			}

            }
            else
            {
                label6.Text = "Off";
                label6.ForeColor = Color.Red;
            }
			#endregion
			
			
		}

        private void PerformJoystickClick(bool ButtonPressed,ref bool delayState, ref int battleSwitchVariable)
        {
            if (ButtonPressed)
            {									//we want to do certain things when different joystick buttons are pressed.
                if (!delayState)
                {
                    if (battleSwitchVariable == 572) { battleSwitchVariable = -40; }  //this is a toggle of a relay. notice that it determines it's current state
                    else { battleSwitchVariable = 572; }					// and uses that to decide what to do next.
                    delayState = true;
                }
            }
            if (!ButtonPressed)
            {
                delayState = false;
            }
        }

		void Button2Click(object sender, System.EventArgs e)
		{
            ReconnectJoystick();
		}

        private void ReconnectJoystick()
        {
            InitDirectInput();
            GetData();
            if (joyfound)
            {
                joystick.Text = "Connected";
                rawAxis.Text =
                    "X:" + state.X + "\r\n" +
                    "Y:" + state.Y + "\r\n" +
                    "Z:" + state.Z + "\r\n" +
                    "Rx:" + state.Rx + "\r\n" +
                    "Ry:" + state.Ry + "\r\n" +
                    "Rz:" + state.Rz + "\r\n";
            }
            else
            {
                joystick.Text = "Not Connected";
            }
        }
		
		void CheckBox1CheckedChanged(object sender, System.EventArgs e)
		{
			
		}
		
		#endregion

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (portBox.SelectedText != null) {
                if (serialPort1.IsOpen) { serialPort1.Close(); }
                serialPort1.PortName = portBox.Text;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (yAxisDirection == 1)
            {
                yAxisDirection = -1;
            }
            else {
                yAxisDirection = 1;
            }
        }

        private void reverseX_CheckedChanged(object sender, EventArgs e)
        {
            if (xAxisDirection == 1)
            {
                xAxisDirection = -1;
            }
            else
            {
                xAxisDirection = 1;
            }
        }

        private void reverseR_CheckedChanged(object sender, EventArgs e)
        {
            if (rxAxisDirection == 1)
            {
                rxAxisDirection = -1;
            }
            else
            {
                rxAxisDirection = 1;
            }
        }

        private void reverseZ_CheckedChanged(object sender, EventArgs e)
        {
            if (zAxisDirection == 1)
            {
                zAxisDirection = -1;
            }
            else
            {
                zAxisDirection = 1;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (!joyfound) {
                ReconnectJoystick();
            }
        }

        private void beltToggle_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ReconnectJoystick();
        }

        private void errorCheck_Tick(object sender, EventArgs e)
        {
            if (portBox.Text == string.Empty)
            {
                errorProvider1.SetError(pololuPanel, "Select a port for the pololu");
            }
            else {
                errorProvider1.SetError(pololuPanel, "");
            }


            if (motorController.Text != "Connected") {
                errorProvider1.SetError(motorController, "Connect a motor controller");
            } else {
                errorProvider1.SetError(motorController, "");
            }
            if (joystick.Text != "Connected")
            {
                errorProvider1.SetError(joystick, "Connect a joystick");
            }
            else
            {
                errorProvider1.SetError(joystick, "");
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }	
	}
}
