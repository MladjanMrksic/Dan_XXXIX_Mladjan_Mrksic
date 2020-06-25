using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Task_1
{
    class Program
    {
        static Random rnd = new Random();
        static EventWaitHandle songReminder = new AutoResetEvent(false);
        static EventWaitHandle commercials = new AutoResetEvent(false);
        static object l = new object();
        static Program pr = new Program();
        static List<Song> songList = new List<Song>();
        static List<string> commercialList = new List<string>();
        static StreamReader sr;
        static StreamWriter sw;
        static string songsPath = @".../.../Music.txt";
        static string commercialsPath = @".../.../Commercials.txt";
        static void Main(string[] args)
        {
            Console.WriteLine("\t\t\tWelcome to AudioPlayer");
            pr.LoadAllSongs();
            pr.LoadAllCommercials();
            pr.MainMenu();
            Console.ReadLine();
        }
        public void MainMenu()
        {
            int choice;
            while (true)
            {
                do
                {
                    Console.WriteLine("Please chose and option from the main menu:\n1)Add new song\n2)List all songs\n3)Play a song");
                } while (int.TryParse(Console.ReadLine(), out choice)!=true);
                switch (choice)
                {
                    case 1:
                        pr.AddSong();
                        break;
                    case 2:
                        pr.ListAllSongs();
                        break;
                    case 3:
                        pr.PlayASong();
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
                Console.WriteLine("Are you sure you want to add {0}:{1} {2}?\n1)Yes\n2)No", author, song, ts);
            } while (int.TryParse(Console.ReadLine(), out choice) == false && (choice == 1 || choice == 2));
            if (choice == 1)
            {
                songList.Add(new Song(author, song, ts));
                using (sw = new StreamWriter(songsPath,append:true))
                {
                    sw.WriteLine(author + ":" + song + ":" + ts);
                }
            }
            else
                return;
        }
        public void ListAllSongs()
        {
            for (int i = 1; i < songList.Count; i++)
            {
                Console.WriteLine(i + ") " + songList[i].Author + ":" + songList[i].SongName + " " + songList[i].Duration);
            }
        }
        public void LoadAllCommercials()
        {
            using (sr = new StreamReader(commercialsPath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    commercialList.Add(line);
                }
            }
        }
        public void LoadAllSongs()
        {
            using (sr = new StreamReader(songsPath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] temp = line.Split(':');
                    TimeSpan tempTS = TimeSpan.Parse(temp[2] + ":" + temp[3] + ":" + temp[4]);
                    songList.Add(new Song(temp[0], temp[1], tempTS));
                }
            }
        }
        public void PlayASong()
        {
            Thread t1 = new Thread(new ThreadStart(pr.SongReminder));
            t1.Start();
            Thread t2 = new Thread(new ThreadStart(pr.BrodcastCommercial));
            t2.Start();
            pr.ListAllSongs();
            int choice;
            do
            {
                Console.WriteLine("Please chose a song you would like to listen to");
            } while (int.TryParse(Console.ReadLine(), out choice)!= true && choice > songList.Count);
            Console.WriteLine("Playing song " + songList[choice].SongName+ " by " + songList[choice].Author + ". Song duration: " + songList[choice].Duration);
            lock (l)
            {
                songReminder.Set();
                commercials.Set();
                Thread.Sleep(Convert.ToInt32(songList[choice].Duration.TotalMilliseconds));
            }
            songReminder.WaitOne();
            commercials.WaitOne();
        }
        public void SongReminder()
        {
            songReminder.WaitOne();
            while (Monitor.TryEnter(l) == false)
            {
                Thread.Sleep(1000);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Song is still playing");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Monitor.Exit(l);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Song has finished playing.");
            Console.ForegroundColor = ConsoleColor.Gray;
            songReminder.Set();
        }
        public void BrodcastCommercial()
        {
            commercials.WaitOne();
            while (Monitor.TryEnter(l) == false)
            {
                Thread.Sleep(200);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(commercialList[rnd.Next(0,5)]);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Thread.Sleep(100);
            commercials.Set();
        }
    }
    public class Song
    {
        internal string Author;
        internal string SongName;
        internal TimeSpan Duration;

        public Song(string author, string songName, TimeSpan duration)
        {
            Author = author;
            SongName = songName;
            Duration = duration;
        }
    }
}
