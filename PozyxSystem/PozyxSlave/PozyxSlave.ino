/*
 Name:		PozyxSlave.ino
 Created:	8/31/2016 11:36:48 AM
 Author:	kehood
*/

#include <Pozyx_definitions.h>
#include <Pozyx.h>
#include <Wire.h>
#include "PozyxSlave.h"

namespace PozyxSlave 
{
	int32_t anchor_heights[POZYX_NUM_ANCHORS]{ ANCHOR1_HEIGHT, ANCHOR2_HEIGHT, ANCHOR3_HEIGHT, ANCHOR4_HEIGHT }; // anchor heights in mm, in same order as IDs
	uint16_t anchors[POZYX_NUM_ANCHORS]{ ANCHOR1_ID, ANCHOR2_ID, ANCHOR3_ID, ANCHOR4_ID }; // anchor network IDs, in same order as heights

#if TAG_IS_SLAVE1
	uint16_t tags[POZYX_NUM_REMOTE_TAGS]{ MASTER_TAG_ID, SLAVE2_TAG_ID }; // tag network IDs
#elif TAG_IS_SLAVE2
	uint16_t tags[POZYX_NUM_REMOTE_TAGS]{ MASTER_TAG_ID, SLAVE1_TAG_ID }; // tag network IDs
#endif

	uint8_t status = POZYX_ERROR_NONE;
	boolean_t setupCompleted = false;

	coordinates_t position;

	boolean_t pozyxSetup()
	{
		// clear all previous devices in the device list
		if (!Pozyx.clearDevices()) 
		{
			errorSetStatus(POZYX_ERROR_SETUP);
			return false;
		}

		if (!Pozyx.doAnchorCalibration())
		{
			errorSetStatus(POZYX_ERROR_SETUP);
			return false;
		}

		delay(1000);

		uint8_t register_num_anchors = (ANCHOR_AUTOSELECT << 7) | POZYX_NUM_ANCHORS;
		if (!Pozyx.regWrite(POZYX_POS_NUM_ANCHORS, &register_num_anchors, 1)) 
		{
			errorSetStatus(POZYX_ERROR_SETUP);
			return false;
		}

		if (!Pozyx.regRead(POZYX_POS_NUM_ANCHORS, &register_num_anchors, 1)) 
		{
			errorSetStatus(POZYX_ERROR_SETUP);
			return false;
		}

		return true;
	}
}

using namespace PozyxSlave;

void setup() 
{
	// ensure that Arduino 101's pin 13 is operating as output for LED control
	pinMode(13, OUTPUT);

	if (!Pozyx.begin(false, MODE_INTERRUPT, POZYX_INT_MASK_ALL, POZYX_INT_PIN0)) 
	{
		status = POZYX_ERROR_BEGIN;
		errorSetStatus();
	}

	// take control of LED_3 on Pozyx (this LED is not used by the Pozyx system as of firmware 1.0)
	if (!Pozyx.setLedConfig(POZYX_LED_CTRL_LED3)) 
	{
		errorSetStatus();
	}
}

// the loop function runs over and over again until power down or reset
void loop() 
{
	if (status != POZYX_ERROR_NONE) 
	{
		errorBlinkLed();
	}
	else if (setupCompleted)
	{
		// do positioning and orientation getting from each active tag
		if (!Pozyx.doPositioning(&position, POZYX_2_5D, 1000)) 
		{
			status = POZYX_ERROR_POSITIONING;
			errorSetStatus();
			return;
		}
	}
	else 
	{
		// poll for message from master
		uint8_t msg;
		if (!Pozyx.readRXBufferData(&msg, 1)) 
		{
			errorSetStatus();
			return;
		}
		if (msg == 's') 
		{
			// setup was requested
			setupCompleted = pozyxSetup();
		}
	}
}


