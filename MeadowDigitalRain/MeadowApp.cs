using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Hardware;
using Meadow.Units;
using System;

namespace MeadowDigitalRain
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        const int displayWidth = 240;
        const int displayHeight = 240;
        St7789 display;
        DigitalRain DR;

        public MeadowApp()
        {
            Initialize();
            Console.WriteLine($"new key ={DR.GetKey(12)}");

            while (true)
            {
                DR.DigitalRainAnim_loop();
            }
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            var config = new SpiClockConfiguration(St7789.DefaultSpiBusSpeed, SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            display = new St7789(
                device: Device,
                spiBus: spiBus,
                chipSelectPin: null,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: displayWidth, height: displayHeight)
            {
                IgnoreOutOfBoundsPixels = true
            };

            DR = new DigitalRain(display, false, false);
        }
    }
}
