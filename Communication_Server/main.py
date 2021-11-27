import socketserver
import logging
import time
from datetime import datetime
from BotManager import BotManager

logging.basicConfig(level=logging.INFO, filename='datagrams.log')
botManger = BotManager()
print(botManger.GetMotorStatus("1"))
"""

botManger.updateBotPosition("20000000000-100000000000-2000000000000")
"""



class MyUDPHandler(socketserver.BaseRequestHandler):


    def handle(self):
        data = self.request[0].strip()
        socket = self.request[1]
        #print("{} wrote:".format(self.client_address[0]))
        #print(data)
        logging.info(str(datetime.now()) + " " + data.decode('utf-8'))
        response_id = botManger.updateBotPosition(data.decode('utf-8'))
        print(str(response_id) + "_" + str(botManger.GetMotorStatus("1")))
        socket.sendto((str(response_id) + "_" + str(botManger.GetMotorStatus("1"))).encode(), self.client_address)


with socketserver.UDPServer(("192.168.1.104", 9999), MyUDPHandler) as server:
    server.serve_forever()




