
#include <ESP8266WiFi.h>
#include <Adafruit_ST7735.h> // Hardware-specific library
#include <SPI.h>

#define TFT_CS     5
#define TFT_RST    15
#define TFT_DC     2

String ssid="";
String password="";
const char* url="ESP8266.asp";
const char* host="untamed.co.uk";

uint16_t lineData[20480];
int lineDataPtr = 0; 

byte b1 = 0, b2 = 0;
uint16_t color = 0;

Adafruit_ST7735 tft = Adafruit_ST7735(TFT_CS,  TFT_DC, TFT_RST);

WiFiClient client;
const int httpPort = 80;

inline void setCS(bool level) {
  digitalWrite(5, level);
}

inline void setRS(bool level) {
  digitalWrite(2, level);
}

inline void setDataBits(uint16_t bits) {
  const uint32_t mask = ~((SPIMMOSI << SPILMOSI) | (SPIMMISO << SPILMISO));
  bits--;
  SPI1U1 = ((SPI1U1 & mask) | ((bits << SPILMOSI) | (bits << SPILMISO)));
}

void setup(void){
  Serial.begin(74880);

//SPI.setFrequency(500000);
  tft.initR(INITR_BLACKTAB);   // initialize a ST7735S chip, black tab
  tft.setRotation(2);
  tft.fillScreen(ST7735_BLACK);

  Serial.print("\n");
  Serial.setDebugOutput(true);

  tft.setCursor(0, 0);
  tft.setTextColor(65534);
  tft.setTextWrap(true);
  tft.println("Scanning networks...");
  tft.println("");

  int n = WiFi.scanNetworks();

  tft.fillScreen(ST7735_BLACK);
  tft.setCursor(0,0);
  
  for (int i = 0; i < n; ++i){
    tft.print(i+1);
    tft.print(" : ");
    tft.println(WiFi.SSID(i));

    if(WiFi.SSID(i)=="Thelma"){
       ssid="Thelma";
       password="**";
       break;
    }
    
    if(WiFi.SSID(i)=="CIPFA Staff"){
       ssid="CompanyStaff";
       password="**";
       break;
    }
    
    if(WiFi.SSID(i)=="Assombalonga"){
       ssid="Assombalonga";
       password="**";
       break;
    }   

  }
  
  delay(2000);
  
  if(ssid==""){
    tft.println("No know network!");
  }else{
    tft.println("");
    tft.println("Identified: ");
    tft.println(ssid);
    tft.println("Logging in.");
  }

  tft.println("");
  
  Serial.print("Connecting to " );
  Serial.println(ssid.c_str());
  tft.println("Connecting to " );
  tft.println(ssid.c_str());
  tft.println("");
  
  if (String(WiFi.SSID()) != String(ssid)) {
    tft.println("Connecting.");
    WiFi.begin(ssid.c_str(), password.c_str());
  }
  while (WiFi.status() != WL_CONNECTED) {
    delay(150);
    Serial.print(".");
    Serial.print(WiFi.status());
    tft.print(".");
  }
  
  Serial.println("Connected! IP address: ");
  Serial.println(WiFi.localIP());
  tft.println("");
  tft.println("Connected!");
  tft.println("IP: " + WiFi.localIP().toString());
  tft.println("");
  tft.println("Requesting data...");
  delay(2000);
  SPI.setFrequency(30000000);
}


void loop(void){
  int frameDelay = 5; 
  if (!client.connect(host, 80)) {
    Serial.println("connection failed");
    return;
    } 
  client.print(String("GET ") + "/ESP8266/ST7735.aspx HTTP/1.1\r\n" +
               "Host: untamed.co.uk\r\n" + 
               "Connection: close\r\n\r\n\r\n");
  delay(50);
  String line ="";
  bool started = false;

  tft.setAddrWindow(0,0,127,160);
  
  while(client.available() || client.connected()){
    if(!started){// The top two linse are a "flag" to show were the data stream starts, and then the length of the delay in text.
      String temp = client.readStringUntil('\r');
      if(temp.indexOf("DataFollows:") >= 0) {
        Serial.print("HERE!");
        started = true;
        frameDelay = atoi(client.readStringUntil('\r').c_str());
        Serial.print("Delay: ");
        Serial.println(frameDelay);
        client.read();
        setRS(true);
        setCS(false);
        setDataBits(16);}
    }else{
      if(client.available()>1){//If the client has bytes available - read a couple in.

        lineData[lineDataPtr++] = client.read() | (uint16_t) client.read() << 8;
        if(lineDataPtr==20480){
          for(int loc=0;loc<20480;loc++){
            lineDataPtr = 0;
            while(SPI1CMD & SPIBUSY) {}
            SPI1W0 = lineData[loc]; 
            SPI1CMD |= SPIBUSY;
          }
        }
        
      } else {
        delay(1);// Wait for more data in the buffer
      }
    }//End of post "started" pixel collect
  }// No longer connected, and no longer any data pending.

  if(!started) {delay(100); Serial.print("Connection not available... trying again ");}

  setCS(true);
  delay(frameDelay);
}




