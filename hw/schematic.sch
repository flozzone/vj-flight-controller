EESchema Schematic File Version 2
LIBS:power
LIBS:device
LIBS:switches
LIBS:relays
LIBS:motors
LIBS:transistors
LIBS:conn
LIBS:linear
LIBS:regul
LIBS:74xx
LIBS:cmos4000
LIBS:adc-dac
LIBS:memory
LIBS:xilinx
LIBS:microcontrollers
LIBS:dsp
LIBS:microchip
LIBS:analog_switches
LIBS:motorola
LIBS:texas
LIBS:intel
LIBS:audio
LIBS:interface
LIBS:digital-audio
LIBS:philips
LIBS:display
LIBS:cypress
LIBS:siliconi
LIBS:opto
LIBS:atmel
LIBS:contrib
LIBS:valves
LIBS:arduino_nano
LIBS:mpu-6050
EELAYER 25 0
EELAYER END
$Descr User 8268 8268
encoding utf-8
Sheet 1 1
Title "vj-flight-controller"
Date ""
Rev ""
Comp ""
Comment1 "Schematic by Florin Hillebrand"
Comment2 "Designed by Juri Berlanda"
Comment3 ""
Comment4 ""
$EndDescr
$Comp
L Arduino_Nano V3
U 1 1 5AF54994
P 2600 3850
F 0 "V3" H 2600 3850 60  0000 C CNN
F 1 "Arduino_Nano" H 2600 4000 60  0000 C CNN
F 2 "" H 2600 3850 60  0000 C CNN
F 3 "" H 2600 3850 60  0000 C CNN
	1    2600 3850
	1    0    0    -1  
$EndComp
$Comp
L MPU-6050 U?
U 1 1 5AF54A21
P 5500 3950
F 0 "U?" H 5850 3250 60  0000 C CNN
F 1 "MPU-6050" H 5250 3250 60  0000 C CNN
F 2 "" H 5500 3700 60  0000 C CNN
F 3 "" H 5500 3700 60  0000 C CNN
	1    5500 3950
	-1   0    0    -1  
$EndComp
Wire Wire Line
	3300 4700 4100 4700
Wire Wire Line
	4100 4700 4100 3150
Wire Wire Line
	4100 3150 4750 3150
Wire Wire Line
	3300 4600 4200 4600
Wire Wire Line
	4200 4600 4200 3250
Wire Wire Line
	4200 3250 4750 3250
Wire Wire Line
	3300 3000 4000 3000
Wire Wire Line
	4000 3000 4000 3650
Wire Wire Line
	4000 3650 4750 3650
$Comp
L +5V #PWR?
U 1 1 5AF54F32
P 6450 3000
F 0 "#PWR?" H 6450 2850 50  0001 C CNN
F 1 "+5V" H 6450 3140 50  0000 C CNN
F 2 "" H 6450 3000 50  0001 C CNN
F 3 "" H 6450 3000 50  0001 C CNN
	1    6450 3000
	1    0    0    -1  
$EndComp
$Comp
L +5V #PWR?
U 1 1 5AF54F76
P 1750 3050
F 0 "#PWR?" H 1750 2900 50  0001 C CNN
F 1 "+5V" H 1750 3190 50  0000 C CNN
F 2 "" H 1750 3050 50  0001 C CNN
F 3 "" H 1750 3050 50  0001 C CNN
	1    1750 3050
	1    0    0    -1  
$EndComp
$Comp
L GND #PWR?
U 1 1 5AF54F94
P 1750 5050
F 0 "#PWR?" H 1750 4800 50  0001 C CNN
F 1 "GND" H 1750 4900 50  0000 C CNN
F 2 "" H 1750 5050 50  0001 C CNN
F 3 "" H 1750 5050 50  0001 C CNN
	1    1750 5050
	1    0    0    -1  
$EndComp
$Comp
L GND #PWR?
U 1 1 5AF54FC4
P 6550 3850
F 0 "#PWR?" H 6550 3600 50  0001 C CNN
F 1 "GND" H 6550 3700 50  0000 C CNN
F 2 "" H 6550 3850 50  0001 C CNN
F 3 "" H 6550 3850 50  0001 C CNN
	1    6550 3850
	1    0    0    -1  
$EndComp
Wire Wire Line
	1950 3200 1750 3200
Wire Wire Line
	1750 3200 1750 3050
Wire Wire Line
	1950 4850 1750 4850
Wire Wire Line
	1750 4850 1750 5050
Wire Wire Line
	6300 3150 6450 3150
Wire Wire Line
	6450 3150 6450 3000
Wire Wire Line
	6300 3700 6550 3700
Wire Wire Line
	6550 3700 6550 3850
$EndSCHEMATC
