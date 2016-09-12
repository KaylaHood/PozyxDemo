#ifndef POZYX_MULTITAG_POS_ORIENTATION_SERIAL_SLAVE
#define POZYX_MULTITAG_POS_ORIENTATION_SERIAL_SLAVE

#define ARDUINO 10609

#include "PozyxSystem.h"
#include <Arduino.h>
#include "CPozyx.h"
#include "CPozyx_definitions.h"

#define TAG_IS_SLAVE1 0
#define TAG_IS_SLAVE2 1

namespace PozyxSlave
{
	extern int32_t anchor_heights[POZYX_NUM_ANCHORS]; // anchor heights in mm, in same order as IDs
	extern uint16_t anchors[POZYX_NUM_ANCHORS]; // anchor network IDs, in same order as heights

#if TAG_IS_SLAVE1
	extern uint16_t tags[POZYX_NUM_REMOTE_TAGS]; // tag network IDs
#elif TAG_IS_SLAVE2
	extern uint16_t tags[POZYX_NUM_REMOTE_TAGS]; // tag network IDs
#endif

	extern uint8_t status;
	extern boolean_t setupCompleted;

	extern coordinates_t position;

	boolean_t pozyxSetup();

	void errorBlinkLed();

	void errorSetStatus(uint8_t defaultErrorCode = POZYX_ERROR_GENERAL);
}
#endif
