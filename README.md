# ESP8266_SunClock_VB.Net
Streams a graphic canvas from .Net server to an ESP8266 connected to a small LCD panel.

The idea being that you create a 32bit full color canvas in the .Net page on a server using the full resources of multi-core CPU, fast internet access, 32bit color.

Then that canvas is converted to a simple RGB565 image that the ESP8266 requests.

Right now only the ST7735 is supported, but I plan to do a M5Stack-Core Grey and Core2 version too.

------------------------------

The class **ST7735Canvas** object is used to convert the full color canvas into the RBG 5,6,5 (16 bit) colors of the ST7735 LCD panel.
The class can also get web images from the internet, and convert them to send to the ESP8266 too!

For debugging - when visited by a PC, the page request URL will return a PNG rather than the ESP8266 formatted binary data. (which just appears as scrambled text) this is because the "user-agent" header is checked to exist.
The ESP8266 doesn't send one... so the page knows what's asking for the data.

e.g, visiting this page returns the canvas image as a PNG: https://untamed.zone/ESP8266/ST7735.aspx

![image](https://user-images.githubusercontent.com/1586332/126346069-2944fd1d-13b5-4846-bfbc-4d91da2e81d8.png)

To do:
Different ESP's can request different pages that return an image formatted to their specific LCD panel. So a whole range of [LCDName]Canvas.vb files could be written, and the same .Net canvas used as the image source for all of them.

https://www.youtube.com/watch?v=4tJzSSl2Ew8

![image](https://user-images.githubusercontent.com/1586332/126342925-4686c873-17a9-428d-ad9f-e83409c3f6f8.png)

![image](https://user-images.githubusercontent.com/1586332/126342969-663d6c93-8c0f-45ab-bada-b88ecf86ba3f.png)
