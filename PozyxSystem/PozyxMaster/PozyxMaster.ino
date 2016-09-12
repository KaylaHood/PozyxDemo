/*
 Name:		PozyxSystem.ino
 Created:	8/11/2016 3:59:35 PM
 Author:	kehood
*/

#include "CPozyx_definitions.h"
#include "CPozyx.h"
#include "PozyxMaster.h"
#include <Wire.h>

namespace PozyxMaster 
{
	// -----
	// Variables and functions which are unique to the Pozyx master tag within the Pozyx system
	// -----
	int32_t anchor_heights[POZYX_NUM_ANCHORS]{ ANCHOR1_HEIGHT, ANCHOR2_HEIGHT, ANCHOR3_HEIGHT, ANCHOR4_HEIGHT }; // anchor heights in mm, in same order as IDs
	uint16_t anchors[POZYX_NUM_ANCHORS]{ ANCHOR1_ID, ANCHOR2_ID, ANCHOR3_ID, ANCHOR4_ID }; // anchor network IDs, in same order as heights
	uint16_t tags[POZYX_NUM_REMOTE_TAGS]{ SLAVE1_TAG_ID, SLAVE2_TAG_ID }; // remote tag network IDs
	uint8_t status = POZYX_ERROR_NONE;
	// setupStatus is true if setupPozyx() has been run but resetVariables() has not
	boolean_t setupStatus = false;

	void errorBlinkLed()
	{
		// some error happened, blink LEDs on Curie and Pozyx
		Arduino101LedBlink(status);
		if (status != POZYX_ERROR_LED) 
		{
			if (!PozyxLedBlink(status))
			{
				status = POZYX_ERROR_LED;
			}
		}
		return;
	}

	void errorSetStatus(uint8_t defaultErrorCode = POZYX_ERROR_GENERAL) 
	{
		if (!Pozyx.getErrorCode(&status)) 
		{
			Serial.println("Failed to retrieve error code from Pozyx");
		}
		if (status == POZYX_ERROR_NONE)
		{
			status = defaultErrorCode;
		}
		Serial.print("Error code status: 0x");
		Serial.println(status,HEX);
		return;
	}

	void handleError(const String& message, uint8_t defaultErrorCode = POZYX_ERROR_GENERAL) 
	{
		// print error information
		Serial.println("*****");
		Serial.print("*****ERROR - ");
		Serial.println(message);
		Serial.println("*****");
		errorSetStatus(defaultErrorCode);
		return;
	}

	void handleSerialMsg() 
	{
		char msg = Serial.read();
		if (msg == 's')
		{
			// do setup
			if (!pozyxSetup()) {
				Serial.println("Pozyx setup reported an error");
			}
			else {
				Serial.println("Pozyx setup completed successfully");
			}
			setupStatus = true;
		}
	}

	void resetVariables() 
	{
		anchor_heights[0] = ANCHOR1_HEIGHT;
		anchor_heights[1] = ANCHOR2_HEIGHT;
		anchor_heights[2] = ANCHOR3_HEIGHT;
		anchor_heights[3] = ANCHOR4_HEIGHT;
		anchors[0] = ANCHOR1_ID;
		anchors[1] = ANCHOR2_ID;
		anchors[2] = ANCHOR3_ID;
		anchors[3] = ANCHOR4_ID;
		tags[0] = SLAVE1_TAG_ID;
		tags[1] = SLAVE2_TAG_ID;
		status = POZYX_ERROR_NONE;
		setupStatus = false;
	}

	boolean_t pozyxSetup()
	{
		boolean_t result = true;

		// ----
		// Initial setup
		// ----
		// reset Pozyx to default configurations, to ensure proper setup
		Pozyx.resetSystem();
		if(setupStatus) 
		{
			// reset PozyxMaster's variables
			resetVariables();
		}

		uint8_t tmpStatus;
		uint8_t counter = 0;
		do 
		{
			counter++;
			Serial.print("\"begin()\" try number: ");
			Serial.println(counter);
			tmpStatus = Pozyx.begin(true);
			delay(500);
		} while (!tmpStatus && counter < 10);

		if (!tmpStatus)
		{
			// there was an error with the begin process (major failure occurred)
			handleError("Fatal \"begin\" error, reset required", POZYX_ERROR_BEGIN);
			result = false;
		}

		// take control of LED_3 on Pozyx (this LED is not used by the Pozyx system as of firmware 1.0)
		if(!Pozyx.setLedConfig(POZYX_LED_CTRL_LED3))
		{
			// error with "setLedConfig"
			handleError("Something went wrong in \"setLedConfig\"");
			result = false;
		}

		// -----
		// Anchor Calibration
		// -----
		Serial.println("Performing auto anchor calibration...");

		// clear all previous devices in the device list
		if (!Pozyx.clearDevices()) 
		{
			// error with clearDevices
			handleError("Something went wrong with \"clearDevices\"");
			result = false;
		}

		if (!Pozyx.doDiscovery(POZYX_DISCOVERY_ALL_DEVICES)) 
		{
			// error with doDiscovery
			handleError("Something went wrong with \"doDiscovery\"");
			result = false;
		}

		uint8_t anchorCalibrationSuccess;
		anchorCalibrationSuccess = Pozyx.doAnchorCalibration(POZYX_2_5D, 10, POZYX_NUM_ANCHORS, anchors, anchor_heights);
		delay(1000);

		if (!anchorCalibrationSuccess) 
		{
			// error with "doAnchorCalibration"
			handleError("Something went wrong with \"doAnchorCalibration\"");
			result = false;
		}
		
		uint8_t num_anchors = 0;
		if (!Pozyx.getDeviceListSize(&num_anchors)) 
		{
			// error with "getDeviceListSize"
			handleError("Something went wrong with \"getDeviceListSize\"");
			result = false;
		}
		Serial.println("Number of anchors in internal list: ");
		Serial.println(num_anchors);

		if (num_anchors != POZYX_NUM_ANCHORS) 
		{
			Serial.print("Only found ");
			Serial.print(num_anchors);
			Serial.print(" out of ");
			Serial.print(POZYX_NUM_ANCHORS);
			Serial.println(" anchors");
			handleError("Didn't find all anchors");
			result = false;
		}

		if (!Pozyx.setSelectionOfAnchors(POZYX_ANCHOR_SEL_MANUAL, 4)) 
		{
			handleError("Something went wrong with \"setSelectionOfAnchors\"");
			result = false;
		}

		return result;
	}

	void printCoordinates(coordinates_t coor)
	{
		Serial.print("x_mm: ");
		Serial.print(coor.x);
		Serial.print("\t");
		Serial.print("y_mm: ");
		Serial.print(coor.y);
		Serial.print("\t");
		Serial.print("z_mm: ");
		Serial.print(coor.z);
		Serial.println();
	}

	boolean_t printCalibrationResult()
	{
		uint8_t list_size;

		if (!Pozyx.getDeviceListSize(&list_size)) 
		{
			handleError("Something went wrong with \"getDeviceListSize\"");
			return false;
		}
		Serial.print("Device list size: ");
		Serial.println(list_size);

		if (list_size == 0)
		{
			handleError("Something went wrong with Calibration, number of devices was zero");
			return false;
		}

		uint16_t device_ids[list_size];
		if (!Pozyx.getDeviceIds(device_ids, list_size)) 
		{
			handleError("Something went wrong in \"getDeviceIds\"");
			return false;
		}

		coordinates_t anchor_coor;
		for (int i = 0; i < list_size; i++)
		{
			Serial.print("ANCHOR,");
			Serial.print("0x");
			Serial.print(device_ids[i], HEX);
			Serial.print(",");
			if (!Pozyx.getDeviceCoordinates(device_ids[i], &anchor_coor)) 
			{
				handleError("Something went wrong in \"getDeviceCoordinates\"");
				return false;
			}
			Serial.print(anchor_coor.x);
			Serial.print(",");
			Serial.print(anchor_coor.y);
			Serial.print(",");
			Serial.println(anchor_coor.z);
		}

		return true;
	}
}

using namespace PozyxMaster;

// -----
// Arduino setup() and loop() functions
// -----

void setup() 
{
	Serial.begin(BAUD_RATE);
	while (!Serial); // wait for serial port to connect

	// ensure that Arduino 101's pin 13 is operating as output for LED control
	pinMode(13, OUTPUT);

	Serial.println("Press \"s\" to initialize Pozyx");
}

void loop() 
{
	if (Serial.available())
	{
		// manage Serial message
		handleSerialMsg();
	}
	else if (status != POZYX_ERROR_NONE) 
	{
		errorBlinkLed();
	}
	else if (setupStatus)
	{
		// do positioning and orientation getting from each active tag
		coordinates_t position;
		uint8_t tmpStatus = Pozyx.doPositioning(&position, POZYX_3D);
		delay(100);

		if (tmpStatus != POZYX_SUCCESS)
		{
			handleError("Something went wrong in \"doPositioning\"", POZYX_ERROR_POSITIONING);
			return;
		}
		// print out the result of positioning
		printCoordinates(position);
	}
	return;
}

