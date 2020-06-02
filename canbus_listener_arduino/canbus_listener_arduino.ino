// https://github.com/autowp/arduino-mcp2515
#include <can.h>
#include <mcp2515.h>

struct can_frame canMsg;
MCP2515 mcp2515(10);
unsigned long messageHeartbeat = 0;
#define NO_MESSAGE_HEARTBEAT 1000

void setup() {
  Serial.begin(115200);

  mcp2515.reset();
  mcp2515.setBitrate(CAN_500KBPS, MCP_16MHZ);
  mcp2515.setNormalMode();

  Serial.println("started");
}

void loop() {
  MCP2515::ERROR error = mcp2515.readMessage(&canMsg);
  switch(error) {
    case MCP2515::ERROR_OK:
      Serial.print(canMsg.can_id, HEX);
      Serial.print(" ");
      Serial.print(canMsg.can_dlc, HEX);
      Serial.print(" ");
      for (int i = 0; i < canMsg.can_dlc; i++) {
        Serial.print(canMsg.data[i], HEX);
        Serial.print(" ");
      }
      Serial.println();
      messageHeartbeat = millis();
      break;
    case MCP2515::ERROR_NOMSG:
      // ignore
      break;
    default:
      Serial.print("can error: ");
      Serial.println(error);
      break;
  }
  if (millis() - messageHeartbeat > NO_MESSAGE_HEARTBEAT) {
     messageHeartbeat = millis();
     Serial.println("no messages");
  }
}
