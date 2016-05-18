using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Rover
{
    public class UltrasonicDistanceSensor
    {
        private readonly GpioPin _gpioPinTrig;
        private readonly GpioPin _gpioPinEcho;
        private Stopwatch _stopwatch;

        private double? _distance; 

        public UltrasonicDistanceSensor(int trigGpioPin, int echoGpioPin)
        {
         

            var gpio = GpioController.GetDefault();

            _gpioPinTrig = gpio.OpenPin(trigGpioPin);
            _gpioPinEcho = gpio.OpenPin(echoGpioPin);
            _gpioPinTrig.SetDriveMode(GpioPinDriveMode.Output);
            _gpioPinEcho.SetDriveMode(GpioPinDriveMode.Input);
            _gpioPinTrig.Write(GpioPinValue.Low);

            _stopwatch = new Stopwatch();
            _gpioPinEcho.ValueChanged += GpioPinEcho_ValueChanged;

        }

        private void GpioPinEcho_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {

                TimeSpan timeBetween = _stopwatch.Elapsed;
                _distance = timeBetween.TotalMilliseconds * 17.15;
        }


        public async Task<double> GetDistanceInCmAsync(int timeoutInMilliseconds)
        {
            _stopwatch = new Stopwatch();
            ManualResetEvent mre = new ManualResetEvent(false);
            mre.WaitOne(100);
            _distance = null;
            try
            {
                _stopwatch.Reset();

                // turn on the pulse
                _gpioPinTrig.Write(GpioPinValue.High);
                await Task.Delay(10);
                _gpioPinTrig.Write(GpioPinValue.Low);

                _stopwatch.Start();


                for (var i = 0; i < timeoutInMilliseconds / 100; i++)
                {
                    if (_distance.HasValue)
                        return _distance.Value;

                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            }
            finally
            {
                _stopwatch.Stop();
            }
            return double.MaxValue;
        }

    }
}
