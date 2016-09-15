#ifndef POZYX_MULTITAG_POS_ORIENTATION_SERIAL_SYSTEM
#define POZYX_MULTITAG_POS_ORIENTATION_SERIAL_SYSTEM

// Constants
#define POZYX_NUM_REMOTE_TAGS 2
#define POZYX_NUM_ANCHORS 4

// Error macro definitions
#define POZYX_ERROR_BEGIN 0x0F
#define POZYX_ERROR_LED 0x10
#define POZYX_ERROR_SETUP 0x11
#define POZYX_ERROR_POSITIONING 0x12

// Pozyx Anchor and Tag IDs
#define MASTER_TAG_ID 0x6024
#define SLAVE1_TAG_ID 0x6026
#define SLAVE2_TAG_ID 0x6032

#define ANCHOR1_ID 0x600D
#define ANCHOR2_ID 0x604E
#define ANCHOR3_ID 0x6004
#define ANCHOR4_ID 0x6072

// Pozyx Anchor heights (in mm)
#define ANCHOR1_HEIGHT 2586
#define ANCHOR2_HEIGHT 1344
#define ANCHOR3_HEIGHT 1024
#define ANCHOR4_HEIGHT 1475

// Pozyx Anchor x coords (in mm)
#define ANCHOR1_X 0
#define ANCHOR2_X 0
#define ANCHOR3_X 3447
#define ANCHOR4_X 3237

// Pozyx Anchor y coords (in mm)
#define ANCHOR1_Y 0
#define ANCHOR2_Y -4309
#define ANCHOR3_Y -4186
#define ANCHOR4_Y 273

// Opcodes (8-bit)
#define OPCODE_NORMAL 0x0
#define OPCODE_CALIBRATION 0x1
#define OPCODE_POSITION 0x2
#define OPCODE_ORIENTATION 0x3

// Toggles
#define ANCHOR_AUTOSELECT 1

boolean_t PozyxLedBusy() 
{
	// pattern:
	// 1000;0100;0010;0001;0000
	int status = 0xF;
	status &= Pozyx.setLed(1, 1);
	delay(100);
	status &= Pozyx.setLed(1, 0);
	status &= Pozyx.setLed(2, 1);
	delay(100);
	status &= Pozyx.setLed(2, 0);
	status &= Pozyx.setLed(3, 1);
	delay(100);
	status &= Pozyx.setLed(3, 0);
	status &= Pozyx.setLed(4, 1);
	delay(100);
	status &= Pozyx.setLed(4, 0);
	return (status == POZYX_SUCCESS);
}

boolean_t PozyxLedBlink(int numBlinks) 
{
	// -- only controls LED_3 --
	// pattern:
	// long 500 millisecond blink followed by <numBlinks> 50 millisecond blinks
	int status = 0xF;
	status &= Pozyx.setLed(3, 1);
	delay(500);
	status &= Pozyx.setLed(3, 0);
	delay(50);
	for (int i = 0;  i < numBlinks; i++) 
	{
		status &= Pozyx.setLed(3, 1);
		delay(50);
		status &= Pozyx.setLed(3, 0);
		delay(50);
	}
	return (status == POZYX_SUCCESS);
}

void Arduino101LedBlink(int numBlinks) 
{
	// pattern:
	// long 500 millisecond blink followed by <numBlinks> 50 millisecond blinks
	digitalWrite(13, HIGH);
	delay(500);
	digitalWrite(13, LOW);
	delay(50);
	for (int i = 0; i < numBlinks; i++) 
	{
		digitalWrite(13, HIGH);
		delay(50);
		digitalWrite(13, LOW);
		delay(50);
	}
}

void errorBlinkLed()
{
	// some error happened, blink LEDs on Curie and Pozyx
	Arduino101LedBlink(status % 10);
	if (status != POZYX_ERROR_LED) {
		if (!PozyxLedBlink(status % 10))
		{
			status = POZYX_ERROR_LED;
		}
	}
}

void errorSetStatus(uint8_t defaultErrorCode = POZYX_ERROR_GENERAL, boolean_t printEnabled = false)
{
	if (!Pozyx.getErrorCode(&status))
	{
		if (printEnabled) 
		{
			Serial.println("Failed to retrieve error code from Pozyx");
		}
	}
	if (status == POZYX_ERROR_NONE)
	{
		status = defaultErrorCode;
	}
	if (printEnabled) 
	{
		Serial.print("Error code status: 0x");
		Serial.println(status, HEX);
	}
	return;
}

#endif