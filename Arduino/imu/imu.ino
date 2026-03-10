#include <SoftwareSerial.h>
// Basic demo for readings from Adafruit BNO08x
// Install this manually
#include <Adafruit_BNO08x.h>

// For SPI mode, we need a CS pin
#define BNO08X_CS 10
#define BNO08X_INT 9

// For SPI mode, we also need a RESET
// #define BNO08X_RESET 5
// but not for I2C or UART
#define BNO08X_RESET -1
#define FAST_MODE

Adafruit_BNO08x bno08x(BNO08X_RESET);
sh2_SensorValue_t sensorValue;

SoftwareSerial BT(D8, D9);  // RX, TX

void setup(void) {
  //Without this inital delay it will usually never connect until you press the reset button
  delay(100);

  // Open serial communications and wait for port to open:
  // Serial.begin(19200);
  // while (!Serial) {
  //   ; // wait for serial port to connect. Needed for native USB port only
  // }

  // BT.begin(9600);
  // BT.print("AT+ROLE0");
  // BT.end();
  BT.begin(19200);
  
  while (!BT) delay(5);
  BT.println("Adafruit BNO08x test!");

  // Try to initialize!
  if (!bno08x.begin_I2C()) {
    //if (!bno08x.begin_UART(&Serial1)) {  // Requires a device with > 300 byte UART buffer!
    //if (!bno08x.begin_SPI(BNO08X_CS, BNO08X_INT)) {
    BT.println("Failed to find BNO08x chip");
    while (1) { delay(5); }
  }
  BT.println("BNO08x Found!");

  for (int n = 0; n < bno08x.prodIds.numEntries; n++) {
    BT.print("Part ");
    BT.print(bno08x.prodIds.entry[n].swPartNumber);
    BT.print(": Version :");
    BT.print(bno08x.prodIds.entry[n].swVersionMajor);
    BT.print(".");
    BT.print(bno08x.prodIds.entry[n].swVersionMinor);
    BT.print(".");
    BT.print(bno08x.prodIds.entry[n].swVersionPatch);
    BT.print(" Build ");
    BT.println(bno08x.prodIds.entry[n].swBuildNumber);
  }
  setReports();

  BT.println("Reading events");
  delay(100);
}

// Here is where you define the sensor outputs you want to receive
void setReports(void) {
  BT.println("Setting desired reports");
  if (!bno08x.enableReport(SH2_GAME_ROTATION_VECTOR)) {
    BT.println("Could not enable game vector");
  }
  if (!bno08x.enableReport(SH2_LINEAR_ACCELERATION, 20000)) {
    BT.println("Could not enable linear acceleration");
  }
}


void loop() {  // run over and over

  delay(5);

  // if (BT.available()) {
  //   Serial.write(BT.read());
  // }
  // if (Serial.available()) {
  //   BT.write(Serial.read());
  // }

  if (bno08x.wasReset()) {
    BT.print("sensor was reset ");
    setReports();
  }

  if (!bno08x.getSensorEvent(&sensorValue)) {
    return;
  }
  switch (sensorValue.sensorId) {

    case SH2_GAME_ROTATION_VECTOR:

      BT.print("q:");
      BT.print(sensorValue.un.gameRotationVector.real);
      BT.print(":");
      BT.print(sensorValue.un.gameRotationVector.i);
      BT.print(":");
      BT.print(sensorValue.un.gameRotationVector.j);
      BT.print(":");
      BT.println(sensorValue.un.gameRotationVector.k);
      break;

    case SH2_LINEAR_ACCELERATION:

      BT.print("a:");
      BT.print(sensorValue.un.linearAcceleration.x);
      BT.print(":");
      BT.print(sensorValue.un.linearAcceleration.y);
      BT.print(":");
      BT.println(sensorValue.un.linearAcceleration.z);
      break;
  }
}
