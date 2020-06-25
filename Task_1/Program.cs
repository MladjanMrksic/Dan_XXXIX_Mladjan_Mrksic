﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Task_1
{
    class Program
    {
        static CancellationTokenSource cts;
        CancellationToken ct;
        static Random rnd = new Random();
        static EventWaitHandle songReminder = new AutoResetEvent(false);
        static EventWaitHandle commercials = new AutoResetEvent(false);
        static EventWaitHandle stopPlaying = new AutoResetEvent(false);
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
                    Console.WriteLine("Please chose an option from the main menu:\n1)Add new song\n2)List all songs\n3)Play a song\n4)Exit");
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
                    case 4:
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Please chose an existing option.");
                        break;
                }
            }
        }
        public void AddSong()
        {
            string author;
            do
            {
                Console.WriteLine("Enter the name of the author (Press ` to go back to main menu)");
                author = Console.ReadLine();
                if (author.ToUpper() == "`")
                {
                    Console.Clear();
                    pr.MainMenu();
                }                   
            } while (author == null);
            string song;
            do
            {
                Console.WriteLine("Enter the name of the song (Press ` to go back to main menu)");
                song = Console.ReadLine();
                if (song.ToUpper() == "`")
                {
                    Console.Clear();
                    pr.MainMenu();
                }
            } while (song == null);
            TimeSpan ts;
            string attempt;
            do
            {
                Console.WriteLine("Enter the duration of the song in format hours:minutes:seconds (Press ` to go back to main menu)");
                attempt = Console.ReadLine();
                if (attempt.ToUpper() == "`")
                {
                    Console.Clear();
                    pr.MainMenu();
                    
                }
            } while (TimeSpan.TryParseExact(attempt, @"hh\:mm\:ss", null, out ts) != true || ts == new TimeSpan(00,00,00));
            Console.WriteLine(ts.ToString());
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
            Console.Clear();
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
            cts = new CancellationTokenSource();
            ct = cts.Token;
            songReminder.Reset();
            commercials.Reset();
            stopPlaying.Reset();
            Thread t1 = new Thread(new ThreadStart(pr.SongReminder));
            t1.Start();
            Thread t2 = new Thread(new ThreadStart(pr.BrodcastCommercial));
            t2.Start();
            Thread t3 = new Thread(new ThreadStart(pr.StopPlaying));
            t3.Start();
            Console.Clear();
            pr.ListAllSongs();
            int choice;
            string attempt;
            do
            {
                Console.WriteLine("Please chose a song you would like to listen to (Press ` to go back to main menu)");
                attempt = Console.ReadLine();
                if (attempt.ToUpper() == "`")
                {
                    Console.Clear();
                    pr.MainMenu();
                }
            } while (int.TryParse(attempt, out choice)!= true && choice > songList.Count);
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("To stop the song, press Esc key.");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Playing song " + songList[choice].SongName+ " by " + songList[choice].Author + ". Song duration: " + songList[choice].Duration);
            lock (l)
            {
                songReminder.Set();
                commercials.Set();
                stopPlaying.Set();
                for (int i = 0; i < Convert.ToInt32(songList[choice].Duration.TotalMilliseconds); i++)
                {
                    if (ct.IsCancellationRequested == false)
                    {
                        Thread.Sleep(1);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            songReminder.WaitOne();
            commercials.WaitOne();
            stopPlaying.WaitOne();
            cts.Dispose();
        }
        public void SongReminder()
        {
            songReminder.WaitOne();
            while (Monitor.TryEnter(l) == false || ct.IsCancellationRequested == false)
            {
                Thread.Sleep(1000);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Song is still playing");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Monitor.Exit(l);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Song has finished playing.");
            Console.ForegroundColor = ConsoleColor.Gray;
            songReminder.Set();
        }
        public void BrodcastCommercial()
        {
            commercials.WaitOne();
            while (Monitor.TryEnter(l) == false || ct.IsCancellationRequested == false)
            {
                Thread.Sleep(200);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(commercialList[rnd.Next(0,5)]);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Monitor.Exit(l);
            Thread.Sleep(100);
            commercials.Set();
        }
        public void StopPlaying()
        {
            stopPlaying.WaitOne();
            do
            {
                while (!Console.KeyAvailable)
                {
                    // Do something
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            cts.Cancel();
            Thread.Sleep(200);
            stopPlaying.Set();
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
