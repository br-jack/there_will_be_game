bool atStarted = false;

void setup() {
  // Open serial communications and wait for port to open:
  Serial.begin(115200);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }
  // set the data rate for the HM-10 connected to the Serial 1 ports (19 and 18 = Rx1 and Tx1)
  Serial1.begin(115200);
  while (!Serial1) {
    ; // wait for serial1 port to connect. Needed for Bluetooth
  }

  Serial1.print("AT+START");
}

void loop() { // run over and over
  if (Serial1.available()) {
    if (atStarted) {
      // BT.print("AT+ROLE1");
      Serial1.print("AT+START");
      atStarted = true;
    }
       
    Serial.write(Serial1.read());
  }
  if (!Serial1) {
    atStarted = false;
  }
  if (Serial.available()) {
    Serial1.write(Serial.read());
  }
}