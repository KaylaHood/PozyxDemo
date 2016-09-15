from pypozyx import *
import serial.tools.list_ports

port = serial.tools.list_ports.comports()[0]

class PozyxPositioning ():
    def __init__(self, port):
