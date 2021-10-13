using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingScheduler
{
    public class Room
    {
        public string RoomName { get; set; }

        public int Capacity { get; set; }

        public DateTime AvailableFrom { get; set; }

        public DateTime AvailableTo { get; set; }

        public IEnumerable<Meeting> Schedule { get; set; } = new HashSet<Meeting>();
    }

    public class Meeting
    {
        public DateTime From { get; set; }

        public DateTime To { get; set; }
    }
}
