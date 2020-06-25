using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Task_1
{
    class Program
    {
        //Creating CancelationTokenSource and CancelationToken to be used to stop playing the song
        static CancellationTokenSource cts;
        CancellationToken ct;
        static Random rnd = new Random();
        //Created AutoReleaseEvents to synchronise threads working around song playing, commercial brodcasts and event for stoping the song
        static EventWaitHandle songReminder = new AutoResetEvent(false);
        static EventWaitHandle commercials = new AutoResetEvent(false);
        static EventWaitHandle stopPlaying = new AutoResetEvent(false);
        static object l = new object();
        static Program pr = new Program();
        static List<Song> songList = new List<Song>();
        static List<string> commercialList = new List<string>();
        static StreamReader sr;
        static StreamWriter sw;
        Thread t1;
        Thread t2;
        Thread t3;
        static string songsPath = @".../.../Music.txt";
        static string commercialsPath = @".../.../Commercials.txt";
        static void Main(string[] args)
        {
            Console.WriteLine("\t\t\tWelcome to AudioPlayer");
            //Loading all songs and comercials to lists for easier handling
            pr.LoadAllSongs();
            pr.LoadAllCommercials();
            pr.MainMenu();
            Console.ReadLine();
        }
        /// <summary>
        /// Main menu serves as UI for the user to navigate through
        /// </summary>
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
        /// <summary>
        /// This method adds new song to text file and to the list that hold the song for application use
        /// </summary>
        public void AddSong()
        {
            //Asking user for author name (name cannot be null) while also checking if the user wants to cancel action
            string author;
            do
            {
                Console.WriteLine("Enter the name of the author (Enter ` to go back to main menu)");
                author = Console.ReadLine();
                if (author.ToUpper() == "`")
                {
                    Console.Clear();
                    pr.MainMenu();
                }                   
            } while (author == null);
            //Asking user for song name (song name cannot be null) while also checking if the user wants to cancel action
            string song;
            do
            {
                Console.WriteLine("Enter the name of the song (Enter ` to go back to main menu)");
                song = Console.ReadLine();
                if (song.ToUpper() == "`")
                {
                    Console.Clear();
                    pr.MainMenu();
                }
            } while (song == null);
            //Asking user for song duration while also checking if the user wants to cancel action
            //Input is checked for null values, correct format and length musnt be longer than 1 day or less than 1 second
            TimeSpan ts;
            string attempt;
            do
            {
                Console.WriteLine("Enter the duration of the song in format hours:minutes:seconds (Enter ` to go back to main menu)");
                attempt = Console.ReadLine();
                if (attempt.ToUpper() == "`")
                {
                    Console.Clear();
                    pr.MainMenu();
                    
                }
            } while (TimeSpan.TryParseExact(attempt, @"hh\:mm\:ss", null, out ts) != true || ts == new TimeSpan(00,00,00));
            Console.WriteLine(ts.ToString());
            int choice;
            //Checking to make sure that user wants to add the song with given detail
            do
            {
                Console.WriteLine("Are you sure you want to add {0}:{1} {2}?\n1)Yes\n2)No", author, song, ts);
            } while (int.TryParse(Console.ReadLine(), out choice) == false && (choice == 1 || choice == 2));
            //If choice was "yes" the song is added to the list and to the text file
            if (choice == 1)
            {
                songList.Add(new Song(author, song, ts));
                using (sw = new StreamWriter(songsPath,append:true))
                {
                    sw.WriteLine(author + ":" + song + ":" + ts);
                }
            }
            //If user selected "no" the song is discarded and user is brought back to main menu
            else
                return;
        }
        /// <summary>
        /// This method reads all songs from list and displays them
        /// </summary>
        public void ListAllSongs()
        {
            Console.Clear();
            for (int i = 0; i < songList.Count; i++)
            {
                Console.WriteLine(i + ") " + songList[i].Author + " : " + songList[i].SongName + "  " + songList[i].Duration);
            }
        }
        /// <summary>
        /// This method loads all commercials from text file to list for easier handling
        /// </summary>
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
        /// <summary>
        /// This method loads all songs from text file to list for easier handling while also paying attention to format
        /// </summary>
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
        /// <summary>
        /// This method simulated playing of the selected song
        /// </summary>
        public void PlayASong()
        {
            //Making new objects for each song played
            cts = new CancellationTokenSource();
            ct = cts.Token;
            //Reseting AutoResetEvents so they are ready for each new song played
            songReminder.Reset();
            commercials.Reset();
            stopPlaying.Reset();
            //After each song played the threads are finished so new ones are created every time a song is played
            pr.t1 = new Thread(new ThreadStart(pr.SongReminder));
            t1.Start();
            pr.t2 = new Thread(new ThreadStart(pr.BrodcastCommercial));
            t2.Start();
            pr.t3 = new Thread(new ThreadStart(pr.StopPlaying));
            t3.Start();
            //Console.Clear and pring all songs for clarity
            Console.Clear();
            pr.ListAllSongs();
            int choice;
            string attempt;
            do
            {
                //User can chose whcih song to listen while also being able to cancel action and go back to main menu
                Console.WriteLine("Please chose a song you would like to listen to (Enter ` to go back to main menu)");
                attempt = Console.ReadLine();
                if (attempt.ToUpper() == "`")
                {
                    Console.Clear();
                    pr.MainMenu();
                }
            } while (int.TryParse(attempt, out choice)!= true || choice > songList.Count-1);
            Console.Clear();
            //User is notified that pressng Ecs will cancel the song
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("To stop the song, press Esc key.");
            Console.ForegroundColor = ConsoleColor.Gray;
            //User is notified which song is playing
            Console.WriteLine("Playing song " + songList[choice].SongName+ " by " + songList[choice].Author + ". Song duration: " + songList[choice].Duration);
            lock (l)
            {
                //Leting other methods know that they can start working
                songReminder.Set();
                commercials.Set();
                stopPlaying.Set();
                //Playing the song second by second while checking if the user decided to cancel action
                for (int i = 0; i < Convert.ToInt32(songList[choice].Duration.TotalMilliseconds); i++)
                {
                    if (ct.IsCancellationRequested == false)
                        Thread.Sleep(1);
                    else
                        break;
                }
            }
            //Waiting for all methods to finish up before going back to main menu
            songReminder.WaitOne();
            commercials.WaitOne();
            stopPlaying.WaitOne();
            cts.Dispose();
        }
        /// <summary>
        /// This methods prints a message every second while the song is playing
        /// </summary>
        public void SongReminder()
        {
            //Thread designated to operate this method has to wait untill the song starts playing to begin
            songReminder.WaitOne();
            //While the object is locked (i.e. a song is playing) this method keep pringing the message.
            //It also checks if cancel is invoked on CancelToken
            while (Monitor.TryEnter(l) == false || ct.IsCancellationRequested == false)
            {
                Thread.Sleep(1000);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Song is still playing");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            //Upon acquiring the lock it imediately releases it
            Monitor.Exit(l);
            //Lets the user know the song has ended or has been canceled
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Song has finished playing.");
            Console.ForegroundColor = ConsoleColor.Gray;
            //Lets the PlayASong method know that this thread has finished
            songReminder.Set();
        }
        public void BrodcastCommercial()
        {
            //This thread waits untill the song starts playing
            commercials.WaitOne();
            //While the object is locked (i.e. a song is playing) this method keep pringing the message.
            //It also checks if cancel is invoked on CancelToken
            while (Monitor.TryEnter(l) == false || ct.IsCancellationRequested == false)
            {
                Thread.Sleep(200);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(commercialList[rnd.Next(0,5)]);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            //Upon acquiring the lock it imediately releases it
            Monitor.Exit(l);
            Thread.Sleep(100);
            //Lets the PlayASong method know that this thread has finished
            commercials.Set();
        }
        /// <summary>
        /// This method "listens" to key presses. If Esc key is pressed while the song is playing CancelationTokenSource triggers Cancel
        /// which cancels all active tokens
        /// </summary>
        public void StopPlaying()
        {
            //This thread waits untill the song starts playing
            stopPlaying.WaitOne();            
            do
            {
                //While no keys are pressed, this method keeps looping
                while (!Console.KeyAvailable)
                {
                    // "Listens" for key presses
                }
                //If a key is pressed but it's not Esc, this will keep looping
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            //Once the Esc key is pressed, the loop is broken and CancelationTokenSource cancels all available tokens
            cts.Cancel();
            Thread.Sleep(200);
            //Lets the PlayASong method know that this thread has finished
            stopPlaying.Set();
        }
    }
    /// <summary>
    /// Class Song hold all the information about a specific song
    /// </summary>
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
