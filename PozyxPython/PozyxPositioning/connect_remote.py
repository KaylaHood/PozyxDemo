from pypozyx import *
import serial.tools.list_ports

master_id = None
num_tags = 2
tag_ids = [0x6026,0x6024]
port = serial.tools.list_ports.comports()[0]

class PozyxComms ():

    def __init__(self, port):
        try:
            self.pozyx = PozyxSerial(port)
        except:
            print("ERROR: Unable to connect to pozyx, wrong port: ", port)
            raise SystemExit

        self.setup()
        while True:
            self.loop()

    def setup(self):
        print("Welcome to Pozyx Comms")
        # get master ID (this is the pozyx connected to the computer)
        master_id = self.pozyx.getNetworkId()
        # clear internal device list
        self.pozyx.clearDevices()
        # identify remote tags
        status = self.pozyx.doDiscovery(POZYX_DISCOVERY_TAGS_ONLY,num_tags)
        if status == POZYX_FAILURE:
            # if discovery of remote tags fails, try manually finding tags
            print("ERROR: Automatic discovery of remote devices failed.")
            raise SystemExit
        else:
            # get IDs of remote tags
            self.pozyx.getTagIds(tag_ids)

    def loop(self):
        for id in tag_ids:
            range = SingleRegister()
            status = self.pozyx.doRanging(id,range)
            if status == POZYX_FAILURE:
                # ranging failed
                print("ERROR: Ranging failed")
                raise SystemExit
            else:
                print ("Range of id ", id, ": ", range)

if __name__ == "__main__":
    PozyxComms(port)
