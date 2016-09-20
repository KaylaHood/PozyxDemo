from pypozyx import *
import serial.tools.list_ports
import time

master_id = SingleRegister(0x6032,2)

num_tags = 2
tag_ids = [0x6026,0x6024]

num_anchors = 4
anchor_ids = [0x600D,0x604E,0x6004,0x6072]
# anchor coordinates
anchor_coords = [
    DeviceCoordinates(anchor_ids[0],0,Coordinates(0,0,2586)),
    DeviceCoordinates(anchor_ids[1],0,Coordinates(0,4309,344)),
    DeviceCoordinates(anchor_ids[2],0,Coordinates(3447,4186,1024)),
    DeviceCoordinates(anchor_ids[3],0,Coordinates(3237,-273,1475))
    ]

port = serial.tools.list_ports.comports()[0]

class PozyxComms ():

    def __init__(self, port):
        with PozyxSerial(port.device) as pozyx:
            if not pozyx.ser.is_open:
                print("ERROR: Unable to connect to pozyx, wrong port: ", port)
                raise SystemExit
            else:
                status = self.setup(pozyx)
                if(status == POZYX_FAILURE):
                    print("Pozyx Comms failed to initialize. Please reconnect and try again")
                    raise SystemExit
                try:
                    while True:
                        self.loop(pozyx)
                except (KeyboardInterrupt, SystemExit):
                    raise SystemExit

    def setup(self, pozyx):
        print("Welcome to Pozyx Comms")
        tries = 0
        status = self.doSetup(pozyx)
        while (status == POZYX_FAILURE and tries < 60):
            print("Setup failed. Trying again... (try #",tries,")",sep="")
            # reset devices
            self.resetAllDevices(pozyx)
            # wait for reset to finish
            time.sleep(1)
            # do setup again
            status = self.doSetup(pozyx)
            # increment "tries"
            tries += 1
        return status

    def loop(self, pozyx):
        self.tagRanging(pozyx)

    def tagRanging(self,pozyx):
        for id in tag_ids:
            print("Ranging tag ID: 0x",format(id,'04x'),sep="")
            range = DeviceRange()
            status = pozyx.doRanging(id,range)
            if(status == POZYX_FAILURE):
                # ranging failed
                print("ERROR: Ranging failed")
                time.sleep(0.5)
            else:
                print(range)
                time.sleep(0.5)

    def tagPositioning(self,pozyx):
        return

    def resetAllDevices(self,pozyx):
        # reset remote devices first
        for id in tag_ids:
            pozyx.resetSystem(id)
        # then reset master device
        pozyx.resetSystem()

    def doSetup(self,pozyx):
        # get master ID (this is the pozyx connected to the computer)
        pozyx.getNetworkId(master_id)
        print("Master ID: 0x",format(master_id.data[0],'04x'),sep="")
        # clear internal device list
        pozyx.clearDevices()
        return self.setupRemoteDevices(pozyx)

    def setupRemoteDevices(self,pozyx):
        status = self.setupRemoteTags(pozyx)
        if(status == POZYX_FAILURE):
            return POZYX_FAILURE
        return self.testRemoteDevices(pozyx)

    def setupRemoteTags(self,pozyx):
        # add tags to device list
        for id in tag_ids:
            device_coords = DeviceCoordinates(id,1,Coordinates(0,0,0))
            status = pozyx.addDevice(device_coords,id)
            if(status == POZYX_FAILURE):
                print("ERROR: Unable to add tag 0x",format(id,'04x'),sep="")
                return POZYX_FAILURE
        return POZYX_SUCCESS

    def setupRemoteAnchors(self,pozyx):
        # add anchors to device list
        for i in range (0,num_anchors):
            status = pozyx.addDevice(anchor_coords[i],anchor_ids[i])
            if(status == POZYX_FAILURE):
                print("ERROR: Unable to add anchor 0x",format(anchor_ids[i],'04x'),sep="")
                return POZYX_FAILURE
        return POZYX_SUCCESS

    def testRemoteDevices(self,pozyx):
        status = self.testRemoteTags(pozyx)
        if(status == POZYX_FAILURE):
            return POZYX_FAILURE
        return self.testRemoteAnchors(pozyx)

    def testRemoteTags(self,pozyx):
        print("Testing tag connections")
        # confirm value in POZYX_WHO_AM_I register
        for id in tag_ids:
            whoami = SingleRegister(0,1)
            pozyx.getWhoAmI(whoami,id)
            print("WhoAmI result of ID 0x", format(id,'04x'),": 0x",format(whoami.data[0],'02x'),sep="")
            if(whoami.data[0] != 0x43):
                print("ERROR: Remote WhoAmI value was invalid")
                return POZYX_FAILURE
        # confirm POZYX_ST_RESULT register
        for id in tag_ids:
            st_result = SingleRegister(0,1)
            pozyx.getSelftest(st_result,id)
            print("Selftest result of ID 0x", format(id,'04x'),": 0b",format(st_result.data[0],'08b'),sep="")
            if(st_result.data[0] != 0b00111111):
                print("ERROR: Remote selftest result indicated device error")
                return POZYX_FAILURE
        return POZYX_SUCCESS

    def testRemoteAnchors(self,pozyx):
        print("Testing anchor connections")
        # confirm value in POZYX_WHO_AM_I register
        for id in anchor_ids:
            whoami = SingleRegister(0,1)
            pozyx.getWhoAmI(whoami,id)
            print("WhoAmI result of ID 0x", format(id,'04x'),": 0x",format(whoami.data[0],'02x'),sep="")
            if(whoami.data[0] != 0x43):
                print("ERROR: Remote WhoAmI value was invalid")
                return POZYX_FAILURE
        # confirm POZYX_ST_RESULT register
        for id in anchor_ids:
            st_result = SingleRegister(0,1)
            pozyx.getSelftest(st_result,id)
            print("Selftest result of ID 0x", format(id,'04x'),": 0b",format(st_result.data[0],'08b'),sep="")
            if((st_result.data[0] & 0b00100001) != 0b00100001):
                print("ERROR: Remote selftest result indicated device error")
                return POZYX_FAILURE
        return POZYX_SUCCESS

if __name__ == "__main__":
    PozyxComms(port)
