
this is web service for Arduino HardWare comunication. Its functions include get Button Status and set strip(8 Leds) and Status(1 Led) colors;
all of request or response data format is JSON.

GET
http://localhost:3420/getkeys
return all label button status
http://localhost:3420/getkey?id=XXX
return some label button status, include Label xxx


http://localhost:3420/callback?port=XXXX
setting callback , xxxx TCP Server Listen Port.

http://localhost:3420/rmcallback?port=XXXX
remove callback , xxxx TCP Server Listen Port.

TCP Server will receive Key Clicked.
'release:{label}\r\n'    server will receive key squance.

if TCP Server shutdown , retry connect or remove by settings config.

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


serialport raw data:
A1,2,128,0,128,0,255,128,
A15,8,128,0,128,0,255,128,128,0,128,0,255,128,128,0,128,0,255,128,128,0,128,0,255,128,
B0,8,128,0,128,0,255,128,128,0,128,0,255,128,128,0,128,0,255,128,128,0,128,0,255,128,
B0,8,222,122,0,0,255,0,255,128,0,0,128,255,0,0,0,0,0,0,0,0,0,0,0,0,


version:1.0.0.8
add cleanup interface


version:1.0.0.13
Add 
/count interface to get current use serial ports
http://localhost:3420/count
return json

Add
TCP Server will receive Key Clicked.
'pressed:{label}\r\n'    server will receive key sequence.


http://localhost:3420/version
Get Fixture Version
return Json


http://localhost:3420/serialstatus
Get Serial Ports Status
return Json


http://localhost:3420/querycallback
Get Register callback
return Json