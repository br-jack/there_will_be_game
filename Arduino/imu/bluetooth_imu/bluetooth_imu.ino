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
  
  bool flipDirection;
  
  int fadeRate = 15;
  //this will usually be kept the same
  int fadeInterval = 30;
  
  int startStrength = 255;
  //not used with constant fade mode
  int endStrength = 255;

  int currentStrength = 255;
  
  RumbleMode mode { RumbleMode::Off };
};

RumbleInstance currentRumble;

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

void setDirection(bool flipDirection) {
  if (flipDirection) {
    digitalWrite(motor1_dir_pin, HIGH);
  }
  else {
    digitalWrite(motor1_dir_pin, LOW);
  }
}

void startRumble(void) {
  currentRumble.startMs = millis();
  currentRumble.fadeMs = 0;
  currentRumble.currentStrength = currentRumble.startStrength;

  setDirection(currentRumble.flipDirection);

  if (currentRumble.flipDirection) {
    Serial.println(F("info: Flip direction = true"));
  }
  else {
    Serial.println(F("info: Flip direction = false"));
  }

  analogWrite(motor1_spd_pin, currentRumble.currentStrength);

  Serial1.println(F("info:Rumble activated."));
}

void endRumble(void) { 
  //currentRumbleInstance.startMs = 0;
  //currentRumbleInstance.duration = 0;
  currentRumble.mode = RumbleMode::Off;
  //currentRumbleInstance.fadeMs = 0;
  //currentRumble.currentStrength = 0;

  //setDirection(currentRumble.flipDirection);

  analogWrite(motor1_spd_pin, 0);

  Serial1.println(F("info:Rumble deactivated."));
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

inline RumbleMode parseRumbleModeByte(char byte) {
  switch(byte) {
    case 'O': {
      return RumbleMode::Off;
    }
    case 'C': {
      return RumbleMode::Constant;
    }
    case 'U': {
      return RumbleMode::RampUp;
    }
    case 'D': {
      return RumbleMode::RampDown;
    }
    //TODO add option for off mode (e.g. to cut vibration off early)
    default: {
      break;
    }
  }

  return RumbleMode::Constant;
}

inline void fadeStep(int currentMs)
{
  //also handle millis wrap around
  if (currentRumble.fadeMs == 0 || currentMs < currentRumble.fadeMs) {
    currentRumble.fadeMs = currentMs;
  }
  else if ((currentMs - currentRumble.fadeMs) >= currentRumble.fadeInterval) {
    currentRumble.fadeMs = currentMs;

    if (currentRumble.currentStrength != currentRumble.endStrength) {
      if (currentRumble.mode == RumbleMode::RampUp) {
        currentRumble.currentStrength += currentRumble.fadeRate;
      }
      else {
        currentRumble.currentStrength -= currentRumble.fadeRate;
      }

      //structured this way to prevent repeated writes after reaching endStrength
      if (
        (currentRumble.mode == RumbleMode::RampUp && currentRumble.currentStrength > currentRumble.endStrength)
        || (currentRumble.mode == RumbleMode::RampDown && currentRumble.currentStrength < currentRumble.endStrength)) {
          currentRumble.currentStrength = currentRumble.endStrength;
        }

      analogWrite(motor1_spd_pin, currentRumble.currentStrength);
    }
  }
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
      if (currentRumble.mode == RumbleMode::RampUp || currentRumble.mode == RumbleMode::RampDown) {
        fadeStep(currentMs);
      }
    }
  }
}

inline void parseRumbleInput(void) {
  constexpr char rumbleChar = 'R';

  const int incomingByte = Serial1.read();

  //rumble string format: "RMa;b;c;d;e\n" where
  //M is mode
  //N is direction (must be either 0 or 1, not used in off mode)
  //a is duration (not used in off mode)
  //b is start strength (not used in off mode)
  //c is end strength (not used in constant or off modes)
  //d is fade rate (not used in constant or off modes)
  //e is fade interval (usually 30, not used in constant or off modes)
  //and the ; characters are separators
  //\n indicates the end of the message.
  //
  //NOTE: regardless of which rumble smode is used, the full message is always expected.
  if (incomingByte == (int)rumbleChar) {
    const char modeByte = (char) Serial1.read();

    currentRumble.mode = parseRumbleModeByte(modeByte);

    int test = Serial1.read();
    Serial.println(test);
    currentRumble.flipDirection = (bool) test;

    currentRumble.duration = Serial1.parseInt();
    //read separator, should be (';') semicolon character but checking would just waste time
    Serial1.read();
    
    currentRumble.startStrength = Serial1.parseInt();
    Serial1.read();
    
    currentRumble.endStrength = Serial1.parseInt();
    Serial1.read();
    
    currentRumble.fadeRate = Serial1.parseInt();
    Serial1.read();
    
    currentRumble.fadeInterval = Serial1.parseInt();
    
    //int newline = Serial1.read();

    //Serial1.println(duration);  
    if (currentRumble.mode != RumbleMode::Off) {
      startRumble();
    }
    else {
      endRumble();
    }
  }
  
  while (Serial1.available() > 0) {
    //discard newline character and any other remaining characters in buffer
    (void) Serial1.read();
  }
}

void loop(void) {  // run over and over

  delay(5);

  rumbleStep();

  outputSensorValues();

  if (Serial1.available() > 0) {
    parseRumbleInput();
  }

  // if (Serial1.available()) {
  //   Serial.write(Serial1.read());
  // }
}
