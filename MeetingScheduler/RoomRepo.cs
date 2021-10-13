using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MeetingScheduler
{
    public class RoomRepo
    {
        private string path;

        public IEnumerable<Room> Rooms { get; set; }

        public RoomRepo(string jsonfilepath)
        {
            path = jsonfilepath;
            LoadJson();
        }

        private void LoadJson()
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                Rooms = JsonConvert.DeserializeObject<Room[]>(json).ToHashSet();
            }
        }

        public IEnumerable<object> GetRoomsWithAvailableSchedules(DateTime day, int participants, int duration)
        {
            LoadJson();

            var availableRooms = Rooms.Where(x => x.Capacity >= participants);

            foreach(Room room in availableRooms)
            {
                room.Schedule = GetAvailabilitySchedule(room, day, duration);
            }

            return availableRooms.Select(x => new
            {
                x.RoomName,
                x.Capacity,
                AvailableFrom = x.AvailableFrom.ToString("HH:mm"),
                AvailableTo = x.AvailableTo.ToString("HH:mm"),
                AvailableMeetingSlots = x.Schedule.Select(z => new
                {
                    From = z.From.ToString("HH:mm"),
                    To = z.To.ToString("HH:mm")
                })
            });
        }

        private IEnumerable<Meeting> GetAvailabilitySchedule(Room room, DateTime day, int duration)
        {
            List<Meeting> availableMeetings = new List<Meeting>();
            var scheduleForTheDay = room.Schedule.Where(x => x.From.Date == day.Date && x.To.Date == day.Date).ToArray();
            
            for(DateTime timeOfDay = room.AvailableFrom; timeOfDay.AddMinutes(duration).TimeOfDay <= room.AvailableTo.TimeOfDay; timeOfDay = timeOfDay.AddMinutes(15))
            {
                DateTime startOfMeeting = timeOfDay;
                DateTime endOfMeeting = timeOfDay.AddMinutes(duration);

                var conflictingEvents = scheduleForTheDay.Where(x => 
                ( x.From.TimeOfDay < startOfMeeting.TimeOfDay && x.To.TimeOfDay > startOfMeeting.TimeOfDay) ||
                ( x.From.TimeOfDay < endOfMeeting.TimeOfDay && x.To.TimeOfDay > endOfMeeting.TimeOfDay) ||
                ( x.From.TimeOfDay >= startOfMeeting.TimeOfDay && x.To.TimeOfDay <= endOfMeeting.TimeOfDay) ||
                ( x.From.TimeOfDay <= startOfMeeting.TimeOfDay && x.To.TimeOfDay >= endOfMeeting.TimeOfDay));

                if(conflictingEvents.Count() == 0)
                {
                    availableMeetings.Add(new Meeting()
                    {
                        From = startOfMeeting,
                        To = endOfMeeting
                    }) ;
                }
            }

            return availableMeetings.ToArray();
        }
    }
}
