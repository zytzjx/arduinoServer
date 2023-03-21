// (c) Michael Schoeffler 2017, http://www.mschoeffler.de
// LED all light time is 5 ms
/*
 * version:2.0.4 it is same as 2.0.3. Only 2.0.3 has two versions.
*/
#define FASTLED_INTERNAL
#include <FastLED.h>

#include <Adafruit_NeoPixel.h>
#ifdef __AVR__
#include <avr/power.h> // Required for 16 MHz Adafruit Trinket
#endif

#define DATA_PIN 3        // Single LED, Status Led. CPU IO
#define NUM_LEDS_STATUS 10 // Status LED Count, it is same as Fixture support count.

#define LED_TYPE WS2812B
//#define COLOR_ORDER GRB

#define BRIGHTNESS 128
#define BRIGHTNESS_SAVE 32

#define DATA_PIN_STATUS 5 // Strip Control, CUP IO
#define NUM_LEDS 144      // Strip Led Count
#define PHONELEDCNT 8     // Every Port LED NUMBER

#define MAX_PHONE_GROUP 16 // MAX SUPPORT 16 PORTS
int keys[MAX_PHONE_GROUP] = {6, 7, 8, 9, 10, 11, 12, 13, 2, 4, A0, A1, A2, A3, A4, A5};
int stripLedIndex[NUM_LEDS_STATUS]={1,16,30,45,60,74,89,103,118,133};

bool bShow = false;
bool bShowing = false;
bool bCancel = false;
unsigned long  lightStarttime=0;
unsigned long  lightInterval=5000;
bool bRecvN = false;

enum MODE
{
    Normal,
    Debug,
    DebugStop
};
MODE gDebugMode = Normal;

struct RGB
{
    byte r;
    byte g;
    byte b;
};

CRGB leds[NUM_LEDS];
int phoneclrcnt = 0;
struct RGB serialleds[MAX_PHONE_GROUP + 1];  // Serial Receive LED Status
struct RGB leds_bak[NUM_LEDS];               // Backup Strip LED
struct RGB leds_status_bak[NUM_LEDS_STATUS]; // Backup single LED status

// When setting up the NeoPixel library, we tell it how many pixels,
// and which pin to use to send signals. Note that for older NeoPixel
// strips you might need to change the third parameter -- see the
// strandtest example for more information on possible values.
Adafruit_NeoPixel pixels(NUM_LEDS_STATUS, DATA_PIN_STATUS, NEO_GRB + NEO_KHZ800);

#define VERSION "version: 2.0.4"    //For 10 ports, begin 2
//#define VERSION "version: 3.0.3"      //For 2 ports, begin 3
/// 1.0.1 #5707 requirement


int Btn[MAX_PHONE_GROUP]={0};
bool startTestMode=false;
  
bool digitalReadA7()
{
    return analogRead(A7) > 400 ? true : false;
}

void LEDStrip(int onoff, bool show, bool rcord=true)
{
    CRGB::HTMLColorCode led = CRGB::White;
    struct RGB clr = {255, 255, 255};
    if (onoff == 0)
    {
        led = CRGB::Black;
        clr = {0, 0, 0};
    }
    for (int i = 0; i < NUM_LEDS; ++i)
    {
        leds[i] = led;
        if(rcord){ 
          leds_bak[i] = clr;
        }
    }
    if (show)
    {
        FastLED.show();
    }
}

void LEDStatus(int onoff, bool show, bool rcord=true)
{
    uint32_t clr = pixels.Color(0, 0, 0);
    struct RGB colr = {0, 0, 0};
    if (onoff != 0)
    {
        clr = pixels.Color(255, 255, 255);
        colr = {255, 255, 255};
    }
    for (int i = 0; i < NUM_LEDS_STATUS; i++)
    {
        pixels.setPixelColor(i, clr);
        if(rcord){ 
          leds_status_bak[i] = colr;
        }
    }
    if (show)
    {
        pixels.show(); // Send the updated pixel colors to the hardware.
    }
}

void ShowAllLedLight(){
  if (bShow && !bShowing){
    bShowing = true;
    FastLED.setBrightness(BRIGHTNESS_SAVE);
    pixels.setBrightness(BRIGHTNESS_SAVE);
    LEDStrip(1, true);
    LEDStatus(1, true);
    bShow = false;
    lightStarttime = millis();
  }
  if (bShowing && bCancel){
      bShowing = false;
      LEDStrip(0, true);
      LEDStatus(0, true);
  }
  
  if (millis()  > lightInterval + lightStarttime){
    if (bShowing){
      bShowing = false;
      LEDStrip(0, true);
      LEDStatus(0, true);
      FastLED.setBrightness(BRIGHTNESS);
      pixels.setBrightness(BRIGHTNESS);
    }
  }
}

void setup()
{
    Serial.begin(9600);
    while (!Serial) {
      ; // wait for serial port to connect. Needed for native USB
    }
    Serial.println(F("Smart-Receiving"));
    Serial.println(F(VERSION));
    for (int i = 0; i < MAX_PHONE_GROUP; ++i)
    {
        pinMode(keys[i], INPUT);
    }
    //delay(1000);
    FastLED.addLeds<NEOPIXEL, DATA_PIN>(leds, NUM_LEDS);
    FastLED.setBrightness(BRIGHTNESS_SAVE);

#if defined(__AVR_ATtiny85__) && (F_CPU == 16000000)
    clock_prescale_set(clock_div_1);
#endif
    // END of Trinket-specific code.

    pixels.begin(); // INITIALIZE NeoPixel strip object (REQUIRED)
    pixels.setBrightness(BRIGHTNESS_SAVE);
/*
    //unsigned long myTime = millis();
    LEDStrip(1, true);
    LEDStatus(1, true);
    //Serial.println( millis() - myTime);
    for (int ll = 0; ll < 4; ll++)
    {
        delay(1000);
    }  
    LEDStrip(0, true);
    LEDStatus(0, true);
    FastLED.setBrightness(BRIGHTNESS);
    pixels.setBrightness(BRIGHTNESS);
*/
    lightStarttime = millis();
    startTestMode = digitalReadA7();
    /*if (startTestMode) {
      bShow = true;
      bRecvN = true;
      lightInterval = 1000;
    }*/
}

void showPhoneLeds(int index, long delayTime, bool show)
{
    setPhoneLeds(index);
    for (int i = 0; i < NUM_LEDS; i++)
    {
        leds[i] = CRGB(leds_bak[i].r, leds_bak[i].g, leds_bak[i].b);
    }
    if (show)
    {
        FastLED.show();
        delay(delayTime);
    }
}

void setPhoneLeds(int index)
{
    if (index < NUM_LEDS)
    {
        for (int i = 0; i < NUM_LEDS;)
        {
            if (i == index && phoneclrcnt > 0)
            {
                for (int j = 0; (j < PHONELEDCNT) && (i < NUM_LEDS); j++)
                {
                    int iii = j % phoneclrcnt;
                    leds_bak[i].r = serialleds[iii].r;
                    leds_bak[i].g = serialleds[iii].g;
                    leds_bak[i].b = serialleds[iii].b;
                    i++;
                }
                break;
            }
            i++;
        }
    }
}

void setPhoneLeds_testCmd(int index)
{
    if (index < NUM_LEDS)
    {
        for (int i = 0; i < NUM_LEDS;i++)
        { 
            leds_bak[i] = {
                0,
                0,
                0,
            };
        }
        for (int j = 0; j <PHONELEDCNT; j++){ 
           leds_bak[j+index]= serialleds[0];
        }
          
    }
}

void showPhoneLedsStatus_testCmd(int index){
  for (int i = 0; i < NUM_LEDS_STATUS; i++){
    
    leds_status_bak[i] = {
                0,
                0,
                0,
            };
  }
  if (index <NUM_LEDS_STATUS) 
    leds_status_bak[index] = serialleds[0];
}

void TestCmd(int stripeindex, int stdex)
{
   setPhoneLeds_testCmd(stripeindex);
   showPhoneLedsStatus_testCmd(stdex);
   RestoreStripLeds();
   RestoreStatusLeds();
}


// Debug Mode to Normal Mode Restore status
void RestoreStripLeds()
{
    for (int i = 0; i < NUM_LEDS; i++)
    {
        leds[i] = CRGB(leds_bak[i].r, leds_bak[i].g, leds_bak[i].b);
    }
    FastLED.show();
}
// Debug Mode to Normal Mode Restore status
void RestoreStatusLeds()
{
    for (int i = 0; i < NUM_LEDS_STATUS; i++)
    {
        pixels.setPixelColor(i, pixels.Color(leds_status_bak[i].r, leds_status_bak[i].g, leds_status_bak[i].b));
    }
    pixels.show(); // Send the updated pixel colors to the hardware.
}

void showPhoneLedsStatus(long delayTime, bool show)//int index, 
{
    for (int i = 0; i < NUM_LEDS_STATUS; i++)
    {
        if (i < phoneclrcnt)
        {
            pixels.setPixelColor(i, pixels.Color(serialleds[i].r, serialleds[i].g, serialleds[i].b));
            leds_status_bak[i] = serialleds[i];
        }
        else
        {
            pixels.setPixelColor(i, pixels.Color(0, 0, 0));
            leds_status_bak[i] = {
                0,
                0,
                0,
            };
        }
    }
    if (show)
    {
        pixels.show(); // Send the updated pixel colors to the hardware.
        delay(delayTime);
    }
}

void setStripLedTest(int index, CRGB rgb)
{
  for(int i=index;i<index+PHONELEDCNT+7;i++)
  {
      if (i<index+PHONELEDCNT)
      {
        leds[i] = rgb;
      }else{
        leds[i] = CRGB(0, 0, 0);
      }
  }
}

void TestModeLight()
{
    leds[0] = CRGB(0, 0, 0);
    for (int i = 0; i < NUM_LEDS_STATUS; i++)
    {
        if (Btn[i] == 1 )
        {
            pixels.setPixelColor(i, pixels.Color(128, 128, 128));
            //leds[i] = CRGB(128, 128, 128);
            setStripLedTest(stripLedIndex[i], CRGB(128, 128, 128));
        }
        else
        {
            pixels.setPixelColor(i, pixels.Color(0, 0, 0));
            setStripLedTest(stripLedIndex[i], CRGB(0, 0, 0));        
        }
    }

    pixels.show(); // Send the updated pixel colors to the hardware.
    FastLED.show();
}

void TestMode()
{
  for (int i = 0; i < MAX_PHONE_GROUP; ++i)
  {
    Btn[i]=digitalRead(keys[i]);
  }
  TestModeLight();
}


void loop()
{
    if (startTestMode && digitalReadA7())
    {
      TestMode();
      return; 
    }
    int incomingByte = 0;
    int index = 0;
    int clrindex = 0;
    //int r, g, b;
    bool bRecv = false;
    //bool bFullprotocol = false;
    if (Serial.available() > 0)
    {
        incomingByte = Serial.read();
        if ((incomingByte == 'A') || (incomingByte == 'B'))
        {
            index = Serial.parseInt();
            phoneclrcnt = Serial.parseInt();
            if (phoneclrcnt <= MAX_PHONE_GROUP)
            {
                while (clrindex < phoneclrcnt)
                {
                    serialleds[clrindex].r = Serial.parseInt();
                    serialleds[clrindex].g = Serial.parseInt();
                    serialleds[clrindex].b = Serial.parseInt();
                    // serialleds[clrindex++] = CRGB(r,g,b);
                    clrindex++;
                }
            }
            if (clrindex == phoneclrcnt)
                bRecv = true;
        }
        else if (incomingByte == 'T')
        {
            index = Serial.parseInt(); //strip index
            phoneclrcnt = Serial.parseInt();//this is status index
           
            serialleds[0].r = Serial.parseInt();
            serialleds[0].g = Serial.parseInt();
            serialleds[0].b = Serial.parseInt();
            bRecv = true;
        }
        else if (incomingByte == 'C' || incomingByte == 'L')
        {
            bRecv = true;
        }
        else if (incomingByte == '\r')
        {
            //bFullprotocol = true;
        }
        else if (incomingByte == 'V')
        {
            Serial.println(VERSION);
        }else if (incomingByte == 'N'){
            bRecvN = true;
        }
    }


    if(millis() > lightStarttime + 400){
        if (!bRecvN){
           bShow = true;
           bRecvN = true;
        }
    }
    ShowAllLedLight();
    ///*
    Serial.print("I,");
    for (int i = 0; i < MAX_PHONE_GROUP; ++i)
    {
        Serial.print(digitalRead(keys[i]), DEC);
        Serial.print(',');
    }
    Serial.println();
    //*/
    if (digitalReadA7())
    {
        if (gDebugMode != Debug)
        {
            LEDStrip(1, true, false);
            LEDStatus(1, true, false);
        }
        gDebugMode = Debug;
    }
    else
    {
        if (gDebugMode == Debug)
        {
            gDebugMode = Normal;
            RestoreStripLeds();
            RestoreStatusLeds();
        }
    }

    //*/
    if (bRecv)
    {
        if (incomingByte == 'A')
        {
            showPhoneLeds(index, 0, gDebugMode == Normal);
        }
        else if (incomingByte == 'B')
        {
            showPhoneLedsStatus(0, gDebugMode == Normal);//index,
        }
        else if (incomingByte == 'C')
        {
            LEDStrip(0, gDebugMode == Normal);
            LEDStatus(0, gDebugMode == Normal);
        }
        else if (incomingByte == 'L')
        {
            LEDStrip(1, gDebugMode == Normal);
            LEDStatus(1, gDebugMode == Normal);
        }else if (incomingByte == 'T'){
            TestCmd(index, phoneclrcnt);
        }
    }
    else
    {
        delay(50);
    }
}
