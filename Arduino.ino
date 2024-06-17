const int pinNumber = 2;
const int baudRate = 9600;

void setup() {
  Serial.begin(baudRate);
  pinMode(pinNumber, OUTPUT);
  digitalWrite(pinNumber, HIGH);
}

void loop() {
  if (Serial.available() > 0) {
    String received = Serial.readStringUntil('\n');
    Serial.print("Received: ");
    Serial.println(received); 

    switch (received.toInt())
    {
      case 1:
        digitalWrite(pinNumber, HIGH);
        break;
      case 0:
        digitalWrite(pinNumber, LOW);
        break;
      default:
        digitalWrite(pinNumber, LOW);
        break;
    }
  }
}
