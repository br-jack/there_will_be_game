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

constexpr int motor1DIRPin = A3;  // Motor 1 Direction pin of dual motor driver connected to digital pin 29
constexpr int motor1SPDPin = A2;  // Motor 1 Speed PWM pin of dual motor driver connected to digital pin 28
constexpr int motor2SPDPin = A1;  // Motor 2 Speed PWM pin of dual motor driver connected to digital pin 27
constexpr int motor2DIRPin = A0;  // Motor 2 Direction pin of dual motor driver connected to digital pin 26

enum class RumbleMode {
  Off,
  Constant,
  RampUp,
  RampDown,
};

struct RumbleInstance {
  unsigned long startMs;
  unsigned long duration;
  int strength = 255;
  int fadeMs;
  RumbleMode mode { RumbleMode::Off };
};

constexpr int rumble_fade_interval = 30;

RumbleInstance currentRumbleInstance;

Adafruit_BNO08x bno08x(BNO08X_RESET);
sh2_SensorValue_t sensorValue;

void setup(void) {
  //Without this inital delay it will usually never connect until you press the reset button
  delay(100);

  // Open serial communications and wait for port to open:
  // Serial.begin(115200);
  // while (!Serial) {
  //   ; // wait for serial port to connect. Needed for native USB port only
  // }

  // Serial.begin(9600);
  // Serial.print("AT+ROLE0");
  // Serial.end();
  Serial.begin(115200);

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

  pinMode(motor1DIRPin, OUTPUT);

  digitalWrite(motor1DIRPin, HIGH);

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

void startRumble(RumbleMode mode, int duration) {
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

  //NOTE: reusing the same struct instance for performance
  currentRumbleInstance.mode = mode;
  currentRumbleInstance.startMs = millis();
  //NOTE: assume duration is unsigned
  currentRumbleInstance.duration = duration;
  if (mode == RumbleMode::RampUp) {
    currentRumbleInstance.strength = 0;
  }
  else {
    currentRumbleInstance.strength = 255;
  }
  currentRumbleInstance.fadeMs = 0;
  Serial.println(F("info:Rumble activated."));

  analogWrite(motor1SPDPin, currentRumbleInstance.strength);
}

void endRumble(void) {
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

  //currentRumbleInstance.startMs = 0;
  //currentRumbleInstance.duration = 0;
  currentRumbleInstance.mode = RumbleMode::Off;
  //currentRumbleInstance.fadeMs = 0;
  Serial.println(F("info:Rumble deactivated."));

  analogWrite(motor1SPDPin, 0);
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

inline RumbleMode parseRumbleModeByte(byte byte) {
  switch(byte) {
    case (int)'C': {
      return RumbleMode::Constant;
    }
    case (int)'U': {
      return RumbleMode::RampUp;
    }
    case (int)'D': {
      return RumbleMode::RampDown;
    }
    default: {
      break;
    }
  }

  return RumbleMode::Constant;
}

void loop(void) {  // run over and over

  delay(5);

  outputSensorValues();

  if (Serial.available() > 3) {
    constexpr char rumbleChar = 'R';

    const int incomingByte = Serial.read();

    //rumble string format: "RMx\n" where M is the mode and x is the duration in ms
    if (incomingByte == (int)rumbleChar) {

      const int modeByte = Serial.read();

      const int duration = Serial.parseInt();

      //Serial.println(duration);  
      startRumble(parseRumbleModeByte(modeByte), duration);
    }
    
    while (Serial.available() > 0) {
      //discard newline character and any other remaining characters in buffer
      (void) Serial.read();
    }
  }

  if (currentRumbleInstance.mode != RumbleMode::Off) {
    const unsigned long currentMs = millis();

    //handle millis wrap around (unlikely to occur)
    if (currentMs < currentRumbleInstance.startMs) {
      currentRumbleInstance.startMs = 0;
    }

    if ((currentMs - currentRumbleInstance.startMs) >= currentRumbleInstance.duration) {
      endRumble();
    }
    else {
      switch(currentRumbleInstance.mode) {
        case RumbleMode::RampUp: {
          //also handle millis wrap around
          if (currentRumbleInstance.fadeMs == 0 || currentMs < currentRumbleInstance.fadeMs) {
            currentRumbleInstance.fadeMs = currentMs;
          }
          else if ((currentMs - currentRumbleInstance.fadeMs) >= rumble_fade_interval) {
            currentRumbleInstance.fadeMs = currentMs;
            if (currentRumbleInstance.strength < 255) {
              currentRumbleInstance.strength++;
              analogWrite(motor1SPDPin, currentRumbleInstance.strength);
            }
          }

          break;
        }

        case RumbleMode::RampDown: {
          //also handle millis wrap around
          if (currentRumbleInstance.fadeMs == 0 || currentMs < currentRumbleInstance.fadeMs) {
            currentRumbleInstance.fadeMs = currentMs;
          }
          else if ((currentMs - currentRumbleInstance.fadeMs) >= rumble_fade_interval) {
            currentRumbleInstance.fadeMs = currentMs;
            if (currentRumbleInstance.strength > 0) {
              currentRumbleInstance.strength -= 5;
              analogWrite(motor1SPDPin, currentRumbleInstance.strength);
            }
          }

          break;
        }
      }
    }
  }

  // if (Serial.available()) {
  //   Serial.write(Serial.read());
  // }
}
