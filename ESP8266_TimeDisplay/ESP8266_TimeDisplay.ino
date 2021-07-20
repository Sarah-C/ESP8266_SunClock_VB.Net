
#include "SSD1306Brzo.h"
#include <ESP8266WiFi.h>

String ssid="";
String password="";
const char* url="ESP8266.asp";
const char* host="untamed.zone";

SSD1306Brzo display(0x3c, 12, 13);

WiFiClient client;
const int httpPort = 80;

void setup(void){
  Serial.begin(74880);

  display.init();
  display.flipScreenVertically();
  display.setTextAlignment(TEXT_ALIGN_LEFT);
  display.setFont(ArialMT_Plain_10);
  display.setLogBuffer(6,25);

  /*
  int aa=32;
    while(true){
    ++aa;
    display.println("Test: ");
    //if((aa%15)==0) display.print(10);
    if(aa==129) aa=32;
    display.clear();
    display.drawLogBuffer(0,0);
    display.display();
    delay(50);
  };
while(true){};*/
  
  Serial.print("\n");
  Serial.setDebugOutput(true);
  int n = WiFi.scanNetworks();
  for (int i = 0; i < n; ++i){
    if(WiFi.SSID(i)=="CIPFA Staff"){
       ssid="CIPFA Staff";
       password="CIPMAN77";
       break;
    }
    if(WiFi.SSID(i)=="Assombalonga"){
       ssid="Assombalonga";
       password="C17h27no2";
       break;
    }
  }
  if(ssid==""){
    display.print("No know network!");
    display.drawLogBuffer(0,0);
    display.display();
  }else{
    display.println("Found: " + ssid);
    display.drawLogBuffer(0,0);
    display.display();
  }
  
  Serial.print("Connecting to " );
  Serial.println(ssid.c_str());
  if (String(WiFi.SSID()) != String(ssid)) {
    display.println("Connecting.");
    display.drawLogBuffer(0,0);
    display.display();
    WiFi.begin(ssid.c_str(), password.c_str());
  }
  while (WiFi.status() != WL_CONNECTED) {
    delay(50);
    Serial.print(".");
    display.drawLogBuffer(0,0);
    display.display();
  }
  Serial.println("");
  Serial.println("Connected! IP address: ");
  display.println("IP: " + WiFi.localIP().toString());
  Serial.println(WiFi.localIP());
  display.drawLogBuffer(0,0);
  display.display();
}
 
void loop(void){
  int frameDelay = 5;
  
  if (!client.connect(host, 80)) {
    Serial.println("connection failed");
    return;
    } 
  client.print(String("GET ") + "/ESP8266/ESP8266.aspx HTTP/1.1\r\n" +
               "Host: untamed.co.uk\r\n" + 
               "Connection: close\r\n\r\n\r\n");
  delay(50);
  String line ="";
  bool started = false;
  int bufferOffset = 0;

  for(int tries=0; tries<50 || !started;tries++){
    
    while(client.available()){
      if(!started){
        String temp = client.readStringUntil('\r');
        if(temp.indexOf("DataFollows:") >= 0) {
          Serial.print("HERE!");
          started = true;
          frameDelay = atoi(client.readStringUntil('\r').c_str());
          Serial.print("Delay: ");
          Serial.println(frameDelay);
          client.read();}
      }else{
        char b = client.read();
        if(bufferOffset != 1024) {display.buffer[bufferOffset] = (byte)b;}
        if(bufferOffset < 1024) ++bufferOffset;
      }  
    }  

    if(!started) {delay(100); Serial.println("Not available.. " + tries);}
  }

  display.display();
  delay(frameDelay);
}
