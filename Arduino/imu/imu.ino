

#include <SoftwareSerial.h>
// Basic demo for readings from Adafruit BNO08x
// Install this manually
#include <Adafruit_BNO08x.h>

// For SPI mode, we need a CS pin
#define BNO08X_CS 10
#define BNO08X_INT 9

// For SPI mode, we also need a RESET 
//#define BNO08X_RESET 5
// but not for I2C or UART
#define BNO08X_RESET -1
#define FAST_MODE

Adafruit_BNO08x  bno08x(BNO08X_RESET);
sh2_SensorValue_t sensorValue;

SoftwareSerial mySerial(1, 0); // RX, TX

void setup(void) {
  Serial.begin(19200);
  while (!Serial) delay(5);     // will pause Zero, Leonardo, etc until serial console opens

  Serial.println("Adafruit BNO08x test!");

  // Try to initialize!
  if (!bno08x.begin_I2C()) {
  //if (!bno08x.begin_UART(&Serial1)) {  // Requires a device with > 300 byte UART buffer!
  //if (!bno08x.begin_SPI(BNO08X_CS, BNO08X_INT)) {
    Serial.println("Failed to find BNO08x chip");
    while (1) { delay(5); }
  }
  Serial.println("BNO08x Found!");

  for (int n = 0; n < bno08x.prodIds.numEntries; n++) {
    Serial.print("Part ");
    Serial.print(bno08x.prodIds.entry[n].swPartNumber);
    Serial.print(": Version :");
    Serial.print(bno08x.prodIds.entry[n].swVersionMajor);
    Serial.print(".");
    Serial.print(bno08x.prodIds.entry[n].swVersionMinor);
    Serial.print(".");
    Serial.print(bno08x.prodIds.entry[n].swVersionPatch);
    Serial.print(" Build ");
    Serial.println(bno08x.prodIds.entry[n].swBuildNumber);
  }
    setReports();

  Serial.println("Reading events");
  delay(100);
}

// Here is where you define the sensor outputs you want to receive
void setReports(void) {
  Serial.println("Setting desired reports");
  if (! bno08x.enableReport(SH2_GAME_ROTATION_VECTOR)) {
    Serial.println("Could not enable game vector");
  }
  if (! bno08x.enableReport(SH2_LINEAR_ACCELERATION, 20000)) {
    Serial.println("Could not enable linear acceleration");
  }
}


void loop() { // run over and over

    delay(5);

  if (bno08x.wasReset()) {
    Serial.print("sensor was reset ");
    setReports();
  }
  
  if (! bno08x.getSensorEvent(&sensorValue)) {
    return;
  }
  // switch (sensorValue.sensorId) {
    
  //   case SH2_GAME_ROTATION_VECTOR:

  //     Serial.print("q:");
  //     Serial.print(sensorValue.un.gameRotationVector.real);
  //     Serial.print(":");
  //     Serial.print(sensorValue.un.gameRotationVector.i);
  //     Serial.print(":");
  //     Serial.print(sensorValue.un.gameRotationVector.j);
  //     Serial.print(":");
  //     Serial.println(sensorValue.un.gameRotationVector.k);
  //     break;

  //   case SH2_LINEAR_ACCELERATION:

  //     Serial.print("a:");
  //     Serial.print(sensorValue.un.linearAcceleration.x);
  //     Serial.print(":");
  //     Serial.print(sensorValue.un.linearAcceleration.y);
  //     Serial.print(":");
  //     Serial.println(sensorValue.un.linearAcceleration.z);
  //     break;

  // }


}
