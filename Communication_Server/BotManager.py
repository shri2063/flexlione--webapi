import numpy as np
import json
import numpy as np
from pathlib import Path
import time


class BotManager:

    @staticmethod
    def GetMotorStatus(bot):

        try:
            original_file = open("bot_motor.json")
            copy = "copy-bot_motor.json"
            copy_file = open(copy, "w")
            copy_file.writelines(original_file.read())
            copy_file.write("]")
            copy_file.close()
            original_file.close()
            read_file = open(copy, )
            motor_command_logs = np.array(json.load(read_file))
            filter_arr = []
            for element in motor_command_logs:
                if element['BotId'] == bot:
                    filter_arr.append(element)

            return filter_arr[len(filter_arr) - 1]['Motor']

        except:
            time.sleep(0.1)
            GetMotorStatus(bot)



    @staticmethod
    def updateInFile(bot):
        try:
            file = Path("bot_position.json")
            if file.is_file():
                old_file = open(file, "a")
                old_file.write(",")
                old_file.write(json.dumps(bot.__dict__))
                old_file.write("\n")

            else:
                new_file = open("bot_position.json", "a")
                new_file.write("[")
                new_file.write(json.dumps(bot.__dict__))
                new_file.write("\n")


        except:
            time.sleep(0.1)
            BotManager.updateInFile(bot)



    @staticmethod
    def updateBotPosition(received_data):
        split = received_data.split("_")
        response_id = split[0]
        bot_id = split[1]
        station_id = split[2]
        node_id = split[3]
        time = split[4]
        speed = split[5]

        bot = Bot(bot_id, station_id, node_id, time,speed)
        BotManager().updateInFile(bot)
        return response_id


class Bot:

    def __init__(self, BotId, StationId, NodeId,Time,Speed):
        self.BotId = BotId
        self.StationId = StationId
        self.NodeId = NodeId
        self.Time = Time
        self.Speed = Speed

