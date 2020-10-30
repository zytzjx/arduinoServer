// (c) Michael Schoeffler 2017, http://www.mschoeffler.de
#define FASTLED_INTERNAL
#include <FastLED.h>

#include <Adafruit_NeoPixel.h>
#ifdef __AVR__
 #include <avr/power.h> // Required for 16 MHz Adafruit Trinket
#endif


#define DATA_PIN 3
#define LED_TYPE WS2812B
#define COLOR_ORDER GRB
#define NUM_LEDS 144
#define BRIGHTNESS 128
#define PHONELEDCNT 8
#define MAX_GROUP 18
#define MAX_PHONE_GROUP 16

#define DATA_PIN_STATUS 5
#define NUM_LEDS_STATUS 8

struct RGB {
  byte r;
  byte g;
  byte b;
};


CRGB leds[NUM_LEDS];
//CRGB leds1[NUM_LEDS];
//CRGB ledsstatus[NUM_LEDS_STATUS];
int phoneclrcnt=0;
struct RGB serialleds[MAX_PHONE_GROUP+1];
struct RGB leds_bak[NUM_LEDS];

// When setting up the NeoPixel library, we tell it how many pixels,
// and which pin to use to send signals. Note that for older NeoPixel
// strips you might need to change the third parameter -- see the
// strandtest example for more information on possible values.
Adafruit_NeoPixel pixels(NUM_LEDS_STATUS, DATA_PIN_STATUS, NEO_GRB + NEO_KHZ800);

int keys[MAX_PHONE_GROUP] = {6,7,8,9,10,11,12,13,2,4,A0,A1,A2,A3,A4,A5};

void setup() {
    Serial.begin(9600);
    Serial.println("Smart-Receiving");
    Serial.println("version: 1.0.0");
    for(int i = 0; i < MAX_PHONE_GROUP; ++i){
      pinMode(keys[i], INPUT);
    }
    delay(1000);
    FastLED.addLeds<NEOPIXEL, DATA_PIN>(leds, NUM_LEDS);
    FastLED.setBrightness(BRIGHTNESS);
    //FastLED.addLeds<NEOPIXEL, DATA_PIN_STATUS>(ledsstatus, NUM_LEDS_STATUS);
    //FastLED.setBrightness(255);
    #if defined(__AVR_ATtiny85__) && (F_CPU == 16000000)
      clock_prescale_set(clock_div_1);
    #endif
  // END of Trinket-specific code.

  pixels.begin(); // INITIALIZE NeoPixel strip object (REQUIRED)
  pixels.setBrightness(BRIGHTNESS);
}

// switches off all LEDs
void showProgramCleanUp(long delayTime) {
  for (int i = 0; i < NUM_LEDS; ++i) {
    leds[i] = CRGB::Black;
  }
  FastLED.show();
  delay(delayTime);
}
// switches off all status LEDs
void showPhoneStatusCleanUp(long delayTime){
    for (int i = 0; i < NUM_LEDS_STATUS; i++){
      pixels.setPixelColor(i, pixels.Color(0, 0, 0));
    }
    pixels.show();   // Send the updated pixel colors to the hardware.
    delay(delayTime);
}

/*void testLEDTwoLine(){
  showProgramCleanUp(1000);
   for (int i = 0; i < NUM_LEDS; i++) {
      leds[i] = CRGB::Red;
   }
   FastLED.show();
   for (int i = 0; i < NUM_LEDS_STATUS; i++) {
       ledsstatus[i] =  CRGB(random8(), random8(), random8());
        FastLED.show();
        delay(100);
   }

   delay(2000);
}
*/
/*void showPhoneLeds(int igroup, long delayTime){
   if (igroup < MAX_GROUP){
      //showProgramCleanUp(delayTime);
      for (int i = 0; i < NUM_LEDS; ) {
        if(i == igroup*PHONELEDCNT && phoneclrcnt > 0){
          for(int j = 0; (j< PHONELEDCNT) && (i < NUM_LEDS); j++){
             leds[i] = serialleds[j%phoneclrcnt];
             i++;
          }
        }
        leds[i] = CRGB::Black;
        i++;
        
      }
      FastLED.show();
      delay(delayTime);
   }
}*/

void showPhoneLeds(int index,long delayTime)
{
  setPhoneLeds(index, delayTime);
  for (int i = 0; i < NUM_LEDS; i++) {
     leds[i] = CRGB(leds_bak[i].r, leds_bak[i].g, leds_bak[i].b);
  }
  FastLED.show();
  delay(delayTime);
}

void setPhoneLeds(int index, long delayTime){
   if (index < NUM_LEDS){
      //showProgramCleanUp(delayTime);
      for (int i = 0; i < NUM_LEDS; ) {
        //Serial.print("a");
        if(i == index && phoneclrcnt > 0){
          for(int j = 0; (j< PHONELEDCNT) && (i < NUM_LEDS); j++){
            int iii = j%phoneclrcnt;
             leds_bak[i].r = serialleds[iii].r; 
             leds_bak[i].g = serialleds[iii].g;
             leds_bak[i].b = serialleds[iii].b;
             i++;
          }
          break;
        }
        //leds[i] = CRGB::Black;
        i++;
        
      }
      //FastLED.show();
      //delay(delayTime);
   }
}


void showPhoneLedsStatus(int index, long delayTime){
   /*for (int i = 0; i < NUM_LEDS_STATUS; ) {
        if(i<phoneclrcnt){
          serialleds[i] = serialleds[i];
        }else{
          serialleds[i] = CRGB::Black;
        }
        i++;
        
      }
      FastLED.show();
      delay(delayTime);
      */
    for (int i = 0; i < NUM_LEDS_STATUS; i++){
      if(i < phoneclrcnt)
      {
        pixels.setPixelColor(i, pixels.Color(serialleds[i].r, serialleds[i].g, serialleds[i].b));
      }else{
        pixels.setPixelColor(i, pixels.Color(0, 0, 0));
      }
    }
    pixels.show();   // Send the updated pixel colors to the hardware.
    delay(delayTime);
}


void loop() {
  int incomingByte = 0;
  int index = 0;
  int clrindex = 0;
  int r, g, b;
  bool bRecv = false;
  bool bFullprotocol = false;
  if (Serial.available() > 0) {
    incomingByte = Serial.read();
    if ((incomingByte=='A')||(incomingByte == 'B')){ 
       index = Serial.parseInt(); 
       phoneclrcnt = Serial.parseInt();
       if(phoneclrcnt<=MAX_PHONE_GROUP)
       {
          while(clrindex<phoneclrcnt){ 
           serialleds[clrindex].r = Serial.parseInt();
           serialleds[clrindex].g = Serial.parseInt();
           serialleds[clrindex].b = Serial.parseInt();
           //serialleds[clrindex++] = CRGB(r,g,b);
           clrindex++;
         }
       }
       if (clrindex==phoneclrcnt)
          bRecv = true;
    }else if (incomingByte=='C'){      
       bRecv = true;    
    }else if (incomingByte == '\r'){
      bFullprotocol = true;
    }
  }
  ///*
  Serial.print("I,");
  for(int i = 0; i < MAX_PHONE_GROUP;++i){
    Serial.print(digitalRead(keys[i]), DEC);  Serial.print(',');
  }
  Serial.println();
  //*/
  if (bRecv){
    
    if (incomingByte=='A'){
      showPhoneLeds(index, 0);
    }else if (incomingByte =='B'){
      showPhoneLedsStatus(index, 0);
    }else if (incomingByte == 'C'){
      showProgramCleanUp(0);
      showPhoneStatusCleanUp(0);
    }
  
  }
  else{
    delay(50);
  }


}
