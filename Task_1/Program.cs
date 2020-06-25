using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_1
{
    class Program
    {
        static List<Song> songList = new List<Song>();
        static StreamReader sr;
        static StreamWriter sw;
        static string songsPath = @".../.../Music.txt";
        static string commercialsPath = @".../.../Commercials.txt";
        static void Main(string[] args)
        {
            Console.WriteLine("\t\t\tWelcome to AudioPlayer");
            TimeSpan ts = new TimeSpan(1, 2, 3);
            Console.WriteLine(ts.ToString());
            Console.ReadLine();
        }
        public void MainMenu()
        {
            int choice;
            while (true)
            {
                do
                {
                    Console.WriteLine("Please chose and option from the main menu:\n1)Add new song\n2)List all songs");
                } while (int.TryParse(Console.ReadLine(), out choice)!=true);
                switch (choice)
                {
                    case 1:

                        break;
                    case 2:

                        break;
                    default:
                        Console.WriteLine("Please chose an existing option.");
                        break;
                }
            }
        }
        public void AddSong()
        {
            Console.WriteLine("Enter the name of the author:");
            string author = Console.ReadLine();
            Console.WriteLine("Enter the name of the song:");
            string song = Console.ReadLine();
            TimeSpan ts;
            do
            {
                Console.WriteLine("Enter the duration of the song in format hours:minutes:seconds :");
            } while (TimeSpan.TryParse(Console.ReadLine(),out ts) != true);
            int choice;
            do
            {
                Console.WriteLine(@"Are you sure you want to add {0}:{1} {2}?\n1)Yes\n2)No", author, song, ts);
            } while (int.TryParse(Console.ReadLine(), out choice) == false && (choice == 1 || choice == 2));
            if (choice == 1)
            {
                songList.Add(new Song(author, song, ts));
                using (sw = new StreamWriter(songsPath))
                {
                    sw.WriteLine(author + ":" + song + " " + ts);
                }
            }
            else
            {
                return;
            }
        }
    }
    public class Song
    {
        string Author;
        string SongName;
        TimeSpan Duration;

        public Song(string author, string songName, TimeSpan duration)
        {
            Author = author;
            SongName = songName;
            Duration = duration;
        }
    }
}
