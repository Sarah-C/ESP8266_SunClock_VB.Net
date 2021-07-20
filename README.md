# ESP8266_SunClock_VB.Net
Streams a graphic canvas from .Net server to an ESP8266.

The idea being that you create a 32bit full color canvas in the .Net page.

The class **ESP8266Canvas** is used to convert the full color canvas into the RBG 5,6,5 (16 bit) colors of the ST7735 LCD panel.
The class can also get web images from the internet, and convert them to send to the ESP8266 too!

(I've no idea what the "get script tag values from web page" is doing in there....)

https://www.youtube.com/watch?v=4tJzSSl2Ew8

![image](https://user-images.githubusercontent.com/1586332/126342925-4686c873-17a9-428d-ad9f-e83409c3f6f8.png)

![image](https://user-images.githubusercontent.com/1586332/126342969-663d6c93-8c0f-45ab-bada-b88ecf86ba3f.png)
