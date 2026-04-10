// Basic demo for readings from Adafruit BNO08x
// Install this manually
#include <Adafruit_BNO08x.h>
#include <string.h>

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

void setup(void) {
  //Without this inital delay it will usually never connect until you press the reset button
  delay(100);

  // Open serial communications and wait for port to open:
  Serial.begin(115200);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }

  while (!Serial) delay(5);

  delay(20);

  Serial.println(F("info:Adafruit BNO08x test!"));

  // Try to initialize!
  if (!bno08x.begin_I2C()) {
    //if (!bno08x.begin_UART(&Serial)) {  // Requires a device with > 300 byte UART buffer!
    //if (!bno08x.begin_SPI(BNO08X_CS, BNO08X_INT)) {
    Serial.println(F("info:Failed to find BNO08x chip"));
    while (1) {
      delay(120); 
      //see https://github.com/adafruit/Adafruit_BNO08x/issues/34#issuecomment-2533685723
      rp2040.reboot();
    }
  }
  Serial.println(F("info:BNO08x Found!"));

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

  Serial.println(F("info:Reading events"));

  delay(100);
}

// Here is where you define the sensor outputs you want to receive
void setReports(void) {
  Serial.println(F("info:Setting desired reports"));
  if (!bno08x.enableReport(SH2_GAME_ROTATION_VECTOR)) {
    Serial.println(F("info:Could not enable game vector"));
  }
  if (!bno08x.enableReport(SH2_LINEAR_ACCELERATION, 20000)) {
    Serial.println(F("info:Could not enable linear acceleration"));
  }

  //https://github.com/sparkfun/SparkFun_BNO08x_Arduino_Library/issues/2
  //delay(100); // This delay allows enough time for the BNO085 to accept the new configuration and clear its reset status
}

inline void outputSensorValues(void) {
  if (bno08x.wasReset()) {
    Serial.print(F("info:Sensor was reset "));
    setReports();
  }

  if (!bno08x.getSensorEvent(&sensorValue)) {
    //Serial.println("info:Unable to get sensor event!");
    return;
  }
  switch (sensorValue.sensorId) {

    case SH2_GAME_ROTATION_VECTOR:
      {
        String q;
        q.concat("q:");
        q.concat(sensorValue.un.gameRotationVector.real);
        q.concat(":");
        q.concat(sensorValue.un.gameRotationVector.i);
        q.concat(":");
        q.concat(sensorValue.un.gameRotationVector.j);
        q.concat(":");
        q.concat(sensorValue.un.gameRotationVector.k);


        Serial.println(q);
        break;
      }


    case SH2_LINEAR_ACCELERATION:
      {
        String q;
        q.concat("a:");
        q.concat(sensorValue.un.linearAcceleration.x);
        q.concat(":");
        q.concat(sensorValue.un.linearAcceleration.y);
        q.concat(":");
        q.concat(sensorValue.un.linearAcceleration.z);
        Serial.println(q);
        break;
      }
  }
}

void loop(void) {  // run over and over

  delay(5);

  outputSensorValues();

  // if (Serial.available()) {
  //   Serial.write(Serial.read());
  // }
}
