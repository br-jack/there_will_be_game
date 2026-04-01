#include <SoftwareSerial.h>
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

const int motor1DIRPin = A3;  // Motor 1 Direction pin of dual motor driver connected to digital pin 29
const int motor1SPDPin = A2;  // Motor 1 Speed PWM pin of dual motor driver connected to digital pin 28
const int motor2SPDPin = A1;  // Motor 2 Speed PWM pin of dual motor driver connected to digital pin 27
const int motor2DIRPin = A0;  // Motor 2 Direction pin of dual motor driver connected to digital pin 26

bool rumbleOn = false;
unsigned long rumbleStartMs;
unsigned long rumbleDuration;

Adafruit_BNO08x bno08x(BNO08X_RESET);
sh2_SensorValue_t sensorValue;

void setup(void) {
  //Without this inital delay it will usually never connect until you press the reset button
  delay(100);

  // Open serial communications and wait for port to open:
  // Serial.begin(19200);
  // while (!Serial) {
  //   ; // wait for serial port to connect. Needed for native USB port only
  // }

  // Serial1.begin(9600);
  // Serial1.print("AT+ROLE0");
  // Serial1.end();
  Serial1.begin(115200);

  while (!Serial1) delay(5);

  delay(20);

  Serial1.println("Adafruit BNO08x test!");

  // Try to initialize!
  if (!bno08x.begin_I2C()) {
    //if (!bno08x.begin_UART(&Serial1)) {  // Requires a device with > 300 byte UART buffer!
    //if (!bno08x.begin_SPI(BNO08X_CS, BNO08X_INT)) {
    Serial1.println("Failed to find BNO08x chip");
    while (1) {
      delay(120); 
      //see https://github.com/adafruit/Adafruit_BNO08x/issues/34#issuecomment-2533685723
      rp2040.reboot();
    }
  }
  Serial1.println("BNO08x Found!");

  for (int n = 0; n < bno08x.prodIds.numEntries; n++) {
    Serial1.print("Part ");
    Serial1.print(bno08x.prodIds.entry[n].swPartNumber);
    Serial1.print(": Version :");
    Serial1.print(bno08x.prodIds.entry[n].swVersionMajor);
    Serial1.print(".");
    Serial1.print(bno08x.prodIds.entry[n].swVersionMinor);
    Serial1.print(".");
    Serial1.print(bno08x.prodIds.entry[n].swVersionPatch);
    Serial1.print(" Build ");
    Serial1.println(bno08x.prodIds.entry[n].swBuildNumber);
  }
  setReports();

  Serial1.println("Reading events");

  pinMode(motor1DIRPin, OUTPUT);
  pinMode(motor2DIRPin, OUTPUT);

  digitalWrite(motor1DIRPin, HIGH);

  delay(100);
}

// Here is where you define the sensor outputs you want to receive
void setReports(void) {
  Serial1.println("Setting desired reports");
  if (!bno08x.enableReport(SH2_GAME_ROTATION_VECTOR)) {
    Serial1.println("Could not enable game vector");
  }
  if (!bno08x.enableReport(SH2_LINEAR_ACCELERATION, 20000)) {
    Serial1.println("Could not enable linear acceleration");
  }
}

void startRumble(int duration) {
  // fade in from min to max in increments of 5 points:
  // for (int fadeValue = 0; fadeValue <= 255; fadeValue += 5) {
    // sets the value (range from 0 to 255):
    // wait for 30 milliseconds to see the dimming effect
    // delay(120);
  // }

  // fade out from max to min in increments of 5 points:
  // for (int fadeValue = 255; fadeValue >= 0; fadeValue -= 5) {
    // sets the value (range from 0 to 255):
    // analogWrite(motor1SPDPin, fadeValue);
    // wait for 30 milliseconds to see the dimming effect
    // delay(120);
  // }

  rumbleOn = true;
  rumbleStartMs = millis();
  //NOTE: assume duration is unsigned
  rumbleDuration = duration;
  Serial1.println("Rumble activated.");

  analogWrite(motor1SPDPin, 255);
}

void endRumble() {
  // fade in from min to max in increments of 5 points:
  // for (int fadeValue = 0; fadeValue <= 255; fadeValue += 5) {
    // sets the value (range from 0 to 255):
    // wait for 30 milliseconds to see the dimming effect
    // delay(120);
  // }

  // fade out from max to min in increments of 5 points:
  // for (int fadeValue = 255; fadeValue >= 0; fadeValue -= 5) {
    // sets the value (range from 0 to 255):
    // analogWrite(motor1SPDPin, fadeValue);
    // wait for 30 milliseconds to see the dimming effect
    // delay(120);
  // }

  rumbleOn = false;
  rumbleStartMs = 0;
  rumbleDuration = 0;
  Serial1.println("Rumble deactivated.");

  analogWrite(motor1SPDPin, 0);
}


void loop() {  // run over and over

  delay(5);

  if (bno08x.wasReset()) {
    Serial1.print("sensor was reset ");
    setReports();
  }

  if (!bno08x.getSensorEvent(&sensorValue)) {
    //Serial1.println("Unable to get sensor event!");
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


        Serial1.println(q);
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
        Serial1.println(q);
        break;
      }
  }

  if (Serial1.available() > 0) {
    //Serial1.write(Serial.read());
    const String rumbleString = Serial1.readStringUntil('\n');

    //rumble string format: "Vx\n" where x is the duration in ms
    if (rumbleString[0] == 'V') {
      const int duration = rumbleString.substring(1).toInt();  
      startRumble(duration);
    }
  }

  if (rumbleOn) {
    const unsigned long currentMs = millis();

    //handle millis wrap around (unlikely to occur)
    if (currentMs < rumbleStartMs) {
      rumbleStartMs = 0;
    }

    if ((currentMs - rumbleStartMs) >= rumbleDuration) {
      endRumble();
    }
  }

  // if (Serial.available()) {
  //   Serial1.write(Serial.read());
  // }
}
