#ifndef POZYX_MULTITAG_POS_ORIENTATION_SERIAL_MASTER
#define POZYX_MULTITAG_POS_ORIENTATION_SERIAL_MASTER

#define ARDUINO 10609
#define BAUD_RATE 115200

#include "PozyxSystem.h"
#include <Arduino.h>

namespace PozyxMaster 
{
	typedef struct __attribute__((packed))_serial_orientation_message
	{
		uint8_t opcode;
		uint8_t pozyx_ID;
		uint64_t timestamp;
		quaternion_t orientation;
	}serial_orientation_message_t;

	typedef struct __attribute__((packed))_serial_position_message
	{
		uint8_t opcode;
		uint8_t pozyx_ID;
		uint64_t timestamp;
		coordinates_t position;
	}serial_position_message_t; 

	extern int32_t anchor_heights[POZYX_NUM_ANCHORS]; // anchor heights in mm, in same order as IDs
	extern uint16_t anchors[POZYX_NUM_ANCHORS]; // anchor network IDs, in same order as heights
	extern uint16_t tags[POZYX_NUM_REMOTE_TAGS]; // remote tag network IDs
	extern uint8_t status;
	extern boolean_t setupStatus;

	void handleError(const String& message, uint8_t defaultErrorCode);

	void handleSerialMsg();

	void resetVariables();

	boolean_t manualAnchorCalibration();

	boolean_t pozyxSetup();

	void printCoordinates(coordinates_t coor);

	boolean_t printCalibrationResult();
}
#endif