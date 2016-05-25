using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Rover
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        //running some task on different thread
        private BackgroundWorker _worker;
        private CoreDispatcher _dispatcher;

        //this is a flag to stop the robot when the application is closed.
        private bool _finish;


        //this is the starting point.
        public MainPage()
        {
            InitializeComponent();

            //you won't really need to touch these, these are just for the lifecycle of the application, and loading/unloading your logic and sensors when the application is ready/closed.
            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;
        }

        //when the UI is loaded
        private void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //dispatcher is another thread to update the UI, you will not need to touch this
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            //we would like the motor to be running on a background thread
            _worker = new BackgroundWorker();
            _worker.DoWork += DoWork;
            _worker.RunWorkerAsync();
        }

        //this only runs when the application is exit
        private void MainPage_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //this flag will stop the robot from moving forward
            _finish = true;

        }
        /// <summary>
        /// For the entire challenge, it is pretty safe to say you only need to change the algorithm in this method
        /// If you would like to venture outside of this method, by all means :)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DoWork(object sender, DoWorkEventArgs e)
        {
            //instantiate the Motor and Ultrasonic sensors. You will not need to change the GPIO pin numbers below. 
            //The numbers below are GPIO pins which the Raspberry Pi uses to control the motor and sensors
            var driver = new TwoMotorsDriver(new Motor(27, 22), new Motor(5, 6));
            var ultrasonicDistanceSensor = new UltrasonicDistanceSensor(23, 24);


            //logging
            await WriteLog("Moving forward");

            #region you can start editing the region from below
            while (true)
            {
                //set distance to initial value of 200cm
                var distance = 200.0;
                driver.MoveForward();

                //if distance is more than 100cm, keep moving
                while (distance > 100.0)
                {
                    // you don't need to change the parameter 1000 for timeout. 
                    //this is for the ultrasonic sensor to response
                    distance = await ultrasonicDistanceSensor.GetDistanceInCmAsync(1000);
                    await WriteLog(distance + "");
                }

                //when distance lesser than 100cm from front obstacle
                SuperStop(driver);

                double currentDistance = distance;
                DateTime dt = DateTime.Now;
                driver.MoveForward();

                //v = d/t
                while (distance > 50.0)
                {
                    // you don't need to change the parameter 1000 for timeout. 
                    //this is for the ultrasonic sensor to response
                    distance = await ultrasonicDistanceSensor.GetDistanceInCmAsync(1000);
                }
                //when distance lesser than 50cm from front obstacle
                SuperStop(driver);

                //calculate, this is the part where you might want to think of the logic
                int timeDiff = DateTime.Now.Subtract(dt).Milliseconds;
                double velocity = (currentDistance - distance) / timeDiff;
                double x = Math.PI * (currentDistance - distance);
                double timeTurn = x / velocity;
                await WriteLog("Turning Left");
                await driver.TurnLeftAsync((int)(Math.Round(timeTurn)));
                SuperStop(driver);
                distance = 999.9;
                await WriteLog("Completed Turn and Going Forward");
                driver.MoveForward();

                //keep moving forward till end of finish line
                while (distance > 30.0)
                {
                    // you don't need to change the parameter 1000 for timeout. 
                    //this is for the ultrasonic sensor to response
                    distance = await ultrasonicDistanceSensor.GetDistanceInCmAsync(1000);
                }

                #endregion

                await WriteLog("Stopping");
                SuperStop(driver);

                await Task.Delay(3000);
            }
        }

        //stop the rover
        private void SuperStop(Rover.TwoMotorsDriver driver)
        {
            try
            {
                driver.Stop();
            }
            catch (Exception) { }
        }


        private async Task WriteLog(string text)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
            // Log.Text = $"{text} | " + Log.Text
            Log.Text = $"{text}  ";
            });
        }
    }
}
