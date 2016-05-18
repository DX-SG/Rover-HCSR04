using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover
{
    public class TwoMotorsDriver
    {
        private readonly Motor _leftMotor;
        private readonly Motor _rightMotor;

        public TwoMotorsDriver(Motor leftMotor, Motor rightMotor)
        {
            _leftMotor = leftMotor;
            _rightMotor = rightMotor;
        }

        public void Stop()
        {
            _leftMotor.Stop();
            _rightMotor.Stop();
        }

        public void MoveForward()
        {
            _leftMotor.MoveForward();
            _rightMotor.MoveForward();
        }

        public void MoveBackward()
        {
            _leftMotor.MoveBackward();
            _rightMotor.MoveBackward();
        }
        /// <summary>
        /// This method will turn the robot right for the state amount of time in milliseconds
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public async Task TurnRightAsync(int milliseconds)
        {
            _leftMotor.MoveForward();
            _rightMotor.MoveBackward();

            await Task.Delay(TimeSpan.FromMilliseconds(milliseconds));

            _leftMotor.Stop();
            _rightMotor.Stop();
        }
        /// <summary>
        /// This method will turn the robot left for the state amount of time in milliseconds
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public async Task TurnLeftAsync(int milliseconds)
        {
            _leftMotor.MoveBackward();
            _rightMotor.MoveForward();

            await Task.Delay(TimeSpan.FromMilliseconds(milliseconds));

            _leftMotor.Stop();
            _rightMotor.Stop();
        }
    }
}