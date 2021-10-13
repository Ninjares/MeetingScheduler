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
        public IEnumerable<Room> Rooms { get; set; }

        private IEnumerable<Room> Availability { get; set; }

        public RoomRepo(string jsonfilepath)
        {
            LoadJson(jsonfilepath);
        }

        private void LoadJson(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string json = reader.ReadToEnd();
                Rooms = JsonConvert.DeserializeObject<Room[]>(json).ToHashSet();
            }
        }

        public IEnumerable<object> GetRoomsWithAvailableSchedules(DateTime day, int participants, int duration)
        {
            //Deep copy
            var availableRooms = Rooms.Where(x => x.Capacity >= participants).Select(x => new Room()
            {

                RoomName = x.RoomName,
                Capacity = x.Capacity,
                AvailableFrom = x.AvailableFrom,
                AvailableTo = x.AvailableTo,
                Schedule = x.Schedule.Select(z => new Meeting()
                {
                    From = z.From,
                    To = z.To
                }).ToList()
            }).ToList();

            foreach(Room room in availableRooms)
            {
                room.Schedule = GetAvailabilitySchedule(room, day, duration).ToList();
            }

            Availability = availableRooms;
            return availableRooms.Select(x => new
            {
                x.RoomName,
                x.Capacity,
                AvailableFrom = x.AvailableFrom.ToString("HH:mm"),
                AvailableTo = x.AvailableTo.ToString("HH:mm"),
                AvailableMeetingSlots = x.Schedule.Select((z, i) => new
                {
                    Id = i+1,
                    From = z.From.ToString("HH:mm"),
                    To = z.To.ToString("HH:mm")
                }).ToArray()
            });
        }

        private IEnumerable<Meeting> GetAvailabilitySchedule(Room room, DateTime day, int duration)
        {
            List<Meeting> availableMeetings = new List<Meeting>();
            var scheduleForTheDay = room.Schedule.Where(x => x.From.Date == day.Date && x.To.Date == day.Date).ToArray();
            
            for(
                DateTime timeOfDay = new DateTime(day.Year, day.Month, day.Day, room.AvailableFrom.Hour, room.AvailableFrom.Minute, room.AvailableFrom.Second); 
                timeOfDay.AddMinutes(duration).TimeOfDay <= room.AvailableTo.TimeOfDay; 
                timeOfDay = timeOfDay.AddMinutes(15)
               )
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

            return availableMeetings.ToList();
        }

        public void AddMeeting(string RoomName, int slotId)
        {
            var room = Availability.FirstOrDefault(x => x.RoomName == RoomName);

            if (room == null)
            {
                if (Rooms.FirstOrDefault(x => x.RoomName == RoomName) == null) throw new NullReferenceException($"Room \"{RoomName}\" doesn't exist.");
                else throw new ArgumentException($"Room {RoomName} doesn't fit the requirements.");
            }

            var scheduleslot = room.Schedule.ToArray()[slotId-1];
            if (scheduleslot == null) throw new NullReferenceException("Slot doesn't exist");

            Rooms.FirstOrDefault(x => x.RoomName == RoomName).Schedule.Add(scheduleslot);
            Rooms.FirstOrDefault(x => x.RoomName == RoomName).Schedule = Rooms.FirstOrDefault(x => x.RoomName == RoomName).Schedule.OrderBy(x => x.From.TimeOfDay).ToList();


            Console.WriteLine($"Successfully added meeting \n{JsonConvert.SerializeObject(scheduleslot, Formatting.Indented)}\n to room \"{RoomName}\"");
        }

        public void SaveJson(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                var toserialize = Rooms.Select(x => new
                {
                    x.RoomName,
                    x.Capacity,
                    AvailableFrom = x.AvailableFrom.ToString("HH:mm"),
                    AvailableTo = x.AvailableTo.ToString("HH:mm"),
                    x.Schedule
                });
                writer.Write(JsonConvert.SerializeObject(toserialize, Formatting.Indented));
            }
        }
    }
}
