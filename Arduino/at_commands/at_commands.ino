#include <SoftwareSerial.h>

// hub 
SoftwareSerial BT(10,11); // RX, TX

// imu
// SoftwareSerial BT(D8, D9);  // RX, TX

void setup() {
  // Open serial communications and wait for port to open:
  Serial.begin(19200);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }
  // set the data rate for the SoftwareSerial port
  BT.begin(19200);

}

void loop() { // run over and over
  if (BT.available()) {
    Serial.write(BT.read());
  }
  if (Serial.available()) {
    BT.write(Serial.read());
  }
}
