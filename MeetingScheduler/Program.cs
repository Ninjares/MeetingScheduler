using System;
using Newtonsoft.Json;

namespace MeetingScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            var rooms = new RoomRepo("./../../../rooms.json");
            Console.WriteLine("Meeting details:");
            Console.Write("Date (dd/mm/yyyy): ");
            DateTime date = DateTime.Parse(Console.ReadLine());
            while (true)
            {
                Console.Write("Number of Participants: ");
                int participants = int.Parse(Console.ReadLine());
                Console.Write("Duration (minutes): ");
                int duration = int.Parse(Console.ReadLine());
                Console.WriteLine(JsonConvert.SerializeObject(rooms.GetRoomsWithAvailableSchedules(date, participants, duration), Formatting.Indented));
            }

        }
    }
}
