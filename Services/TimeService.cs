using System;

namespace SmartGearOnline.Services
{
    public class TimeService : ITimeService
    {
        public string GetCurrentTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }
}