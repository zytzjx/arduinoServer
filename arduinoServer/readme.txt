
this is web service for Arduino HardWare comunication. Its functions include get Button Status and set strip(8 Leds) and Status(1 Led) colors;
all of request or response data format is JSON.

GET
http://localhost:3420/getkeys
return all label button status
http://localhost:3420/getkey?id=XXX
return some label button status, include Label xxx

POST
http://localhost:3420/leds
{"status":"strip",  "label":3,  "colors":[[r,g,b],[r,g,b],[r,g,b]]}
this set one strip. Only set One strip. and turn off the other strip at the same time.
colors MAX cout is 8. MIN is 1. The example is 3. r, g, b is RGB COLOR SYSTEM VALUE.

example:
{"status":"strip",  "label":1,  "colors":[[128,0,128],[0,255,128]]}

{"status":"strip",  "label":1,  "colors":[[128,0,128],[0,255,128]]}
{"status":"status",  "labels":[{"label":3,  "color":[r,g,b]},{"label":4,  "color":[r,g,b]}]}
status allow every Leds turn on. so if you want to turn off the Status LED. set color is  [0,0,0] 
status only change the label field identy. and the others will keep old status.

example:
{"status":"status",  "labels":[{"label":3,  "color":[255,128,0]},{"label":4,  "color":[0,128,255]}]}


