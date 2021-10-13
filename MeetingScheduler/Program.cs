using System;
using System.Linq;
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

            Console.Write("Number of Participants: ");
            int participants = int.Parse(Console.ReadLine());

            Console.Write("Duration (minutes): ");
            int duration = int.Parse(Console.ReadLine());

            Console.WriteLine(JsonConvert.SerializeObject(rooms.GetRoomsWithAvailableSchedules(date, participants, duration), Formatting.Indented));

            Console.Write("Select room by name: ");
            string roomname = Console.ReadLine().Trim();

            Console.Write("Specify slot Id: ");
            int slotId = int.Parse(Console.ReadLine());

            rooms.AddMeeting(roomname, slotId);
            rooms.SaveJson("./../../../OutputRooms.json");

        }
    }
}
