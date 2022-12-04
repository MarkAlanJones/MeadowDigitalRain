using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace MeadowDigitalRain
{
    public class MeadowApp : App<F7FeatherV1>
    {
        const int displayWidth = 240;
        const int displayHeight = 240;
        St7789 display;
        DigitalRain DR;

        public override Task Run()
        {
            Console.WriteLine($"new key ={DR.GetKey(12)}");
            while ( DR != null )
            {
                DR.DigitalRainAnim_loop();
            }
            return base.Run();
        }

        public override Task Initialize()
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
                width: displayWidth, height: displayHeight);

            DR = new DigitalRain(display, false, false);

            return base.Initialize();
        }

    }
}
