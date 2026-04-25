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

constexpr int motor1_spd_pin = A0;  // Motor 1 Speed PWM pin of dual motor driver connected to digital pin 26
constexpr int motor1_dir_pin = A1;  // Motor 1 Direction pin of dual motor driver connected to digital pin 27
constexpr int motor2_spd_pin = A2;  // Motor 2 Speed PWM pin of dual motor driver connected to digital pin 28
constexpr int motor2_dir_pin = A3;  // Motor 2 Direction pin of dual motor driver connected to digital pin 29

enum class RumbleMode {
  Off,
  Constant,
  RampUp,
  RampDown,
};

struct RumbleInstance {
  unsigned long startMs;
  int fadeMs;
  unsigned long duration;
  
  int fadeInterval = 30;
  int fadeRate = 15;
  
  int startStrength = 255;
  //not used with constant fade mode
  int endStrength = 255;

  int currentStrength = 255;
  
  RumbleMode mode { RumbleMode::Off };
};

constexpr int rumble_fade_interval = 30;
constexpr int rumble_fade_rate = 15;

RumbleInstance currentRumble;

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

  // Serial1.begin(9600);
  // Serial1.print("AT+ROLE0");
  // Serial1.end();
  Serial1.begin(115200);

  while (!Serial1) delay(5);

  delay(20);

  Serial1.println(F("info:Adafruit BNO08x test!"));

  // Try to initialize!
  if (!bno08x.begin_I2C()) {
    //if (!bno08x.begin_UART(&Serial1)) {  // Requires a device with > 300 byte UART buffer!
    //if (!bno08x.begin_SPI(BNO08X_CS, BNO08X_INT)) {
    Serial1.println(F("info:Failed to find BNO08x chip"));
    while (1) {
      delay(120); 
      //see https://github.com/adafruit/Adafruit_BNO08x/issues/34#issuecomment-2533685723
      rp2040.reboot();
    }
  }
  Serial1.println(F("info:BNO08x Found!"));

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

  Serial1.println(F("info:Reading events"));

  pinMode(motor1_dir_pin, OUTPUT);

  digitalWrite(motor1_dir_pin, HIGH);

  delay(100);
}

// Here is where you define the sensor outputs you want to receive
void setReports(void) {
  Serial1.println(F("info:Setting desired reports"));
  if (!bno08x.enableReport(SH2_GAME_ROTATION_VECTOR)) {
    Serial1.println(F("info:Could not enable game vector"));
  }
  if (!bno08x.enableReport(SH2_LINEAR_ACCELERATION, 20000)) {
    Serial1.println(F("info:Could not enable linear acceleration"));
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
  currentRumble.mode = mode;
  currentRumble.startMs = millis();
  //NOTE: assume duration is unsigned
  currentRumble.duration = duration;

  if (mode == RumbleMode::RampUp) {
    currentRumble.startStrength = 0;
    currentRumble.endStrength = 255;
  }
  else {
    //TODO make these configurable
    currentRumble.startStrength = 255;
    currentRumble.endStrength = 255;
  }
  currentRumble.fadeMs = 0;

  currentRumble.currentStrength = currentRumble.startStrength;

  analogWrite(motor1_spd_pin, currentRumble.currentStrength);

  Serial1.println(F("info:Rumble activated."));
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
  currentRumble.mode = RumbleMode::Off;
  //currentRumbleInstance.fadeMs = 0;
  Serial1.println(F("info:Rumble deactivated."));

  analogWrite(motor1_spd_pin, 0);
}

inline void outputSensorValues(void) {
  if (bno08x.wasReset()) {
    Serial1.print(F("info:Sensor was reset "));
    setReports();
  }

  if (!bno08x.getSensorEvent(&sensorValue)) {
    //Serial1.println("info:Unable to get sensor event!");
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

inline void rumbleStep(void) {
  if (currentRumble.mode != RumbleMode::Off) {
    const unsigned long currentMs = millis();

    //handle millis wrap around (unlikely to occur)
    if (currentMs < currentRumble.startMs) {
      currentRumble.startMs = 0;
    }

    if ((currentMs - currentRumble.startMs) >= currentRumble.duration) {
      endRumble();
    }
    else {
      switch(currentRumble.mode) {
        case RumbleMode::RampUp: {
          //also handle millis wrap around
          if (currentRumble.fadeMs == 0 || currentMs < currentRumble.fadeMs) {
            currentRumble.fadeMs = currentMs;
          }
          else if ((currentMs - currentRumble.fadeMs) >= rumble_fade_interval) {
            currentRumble.fadeMs = currentMs;
            if (currentRumble.currentStrength < currentRumble.endStrength) {
              currentRumble.currentStrength += rumble_fade_rate;

              //structured this way to prevent repeated writes after reaching endStrength
              if (currentRumble.currentStrength > currentRumble.endStrength) {
                currentRumble.currentStrength = currentRumble.endStrength;
              }

              analogWrite(motor1_spd_pin, currentRumble.currentStrength);
            }
          }

          break;
        }

        //TODO reuse code that is similar to rampup in function
        case RumbleMode::RampDown: {
          //also handle millis wrap around
          if (currentRumble.fadeMs == 0 || currentMs < currentRumble.fadeMs) {
            currentRumble.fadeMs = currentMs;
          }
          else if ((currentMs - currentRumble.fadeMs) >= rumble_fade_interval) {
            currentRumble.fadeMs = currentMs;
            if (currentRumble.currentStrength > currentRumble.endStrength) {
              currentRumble.currentStrength += rumble_fade_rate;

              //structured this way to prevent repeated writes after reaching endStrength
              if (currentRumble.currentStrength < currentRumble.endStrength) {
                currentRumble.currentStrength = currentRumble.endStrength;
              }

              analogWrite(motor1_spd_pin, currentRumble.currentStrength);
            }
          }

          break;
        }
      }
    }
  }
}

inline void checkRumbleInput(void) {
  if (Serial1.available() > 3) {
    constexpr char rumbleChar = 'R';

    const int incomingByte = Serial1.read();

    //rumble string format: "RMx\n" where M is the mode and x is the duration in ms
    if (incomingByte == (int)rumbleChar) {

      const int modeByte = Serial1.read();

      const int duration = Serial1.parseInt();

      //Serial1.println(duration);  
      startRumble(parseRumbleModeByte(modeByte), duration);
    }
    
    while (Serial1.available() > 0) {
      //discard newline character and any other remaining characters in buffer
      (void) Serial1.read();
    }
  }
}

void loop(void) {  // run over and over

  delay(5);

  rumbleStep();

  outputSensorValues();

  checkRumbleInput();

  // if (Serial1.available()) {
  //   Serial.write(Serial1.read());
  // }
}
