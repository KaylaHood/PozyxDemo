#ifndef POZYX_MULTITAG_POS_ORIENTATION_SERIAL_SYSTEM
#define POZYX_MULTITAG_POS_ORIENTATION_SERIAL_SYSTEM

#include "CPozyx_definitions.h"
#include "CPozyx.h"

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
#define ANCHOR1_HEIGHT 0
#define ANCHOR2_HEIGHT 503
#define ANCHOR3_HEIGHT 264
#define ANCHOR4_HEIGHT 191

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
	// long 500 millisecond blink followed by <numBlinks> 100 millisecond blinks
	int status = 0xF;
	status &= Pozyx.setLed(3, 1);
	delay(500);
	status &= Pozyx.setLed(3, 0);
	delay(200);
	for (int i = 0;  i < numBlinks; i++) 
	{
		status &= Pozyx.setLed(3, 1);
		delay(200);
		status &= Pozyx.setLed(3, 0);
		delay(200);
	}
	return (status == POZYX_SUCCESS);
}

void Arduino101LedBlink(int numBlinks) 
{
	// pattern:
	// long 500 millisecond blink followed by <numBlinks> 100 millisecond blinks
	digitalWrite(13, HIGH);
	delay(500);
	digitalWrite(13, LOW);
	delay(200);
	for (int i = 0; i < numBlinks; i++) 
	{
		digitalWrite(13, HIGH);
		delay(200);
		digitalWrite(13, LOW);
		delay(200);
	}
}

#endif