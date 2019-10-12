using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.CognitiveServices.Speech;
using System.Diagnostics;

namespace VoiceCommand
{
    class Program
    {
        static class LevenshteinDistance
        {
            /// <summary>
            /// Compute the distance between two strings.
            /// </summary>
            public static int Compute(string s, string t)
            {
                int n = s.Length;
                int m = t.Length;
                int[,] d = new int[n + 1, m + 1];

                // Step 1
                if (n == 0)
                {
                    return m;
                }

                if (m == 0)
                {
                    return n;
                }

                // Step 2
                for (int i = 0; i <= n; d[i, 0] = i++)
                {
                }

                for (int j = 0; j <= m; d[0, j] = j++)
                {
                }

                // Step 3
                for (int i = 1; i <= n; i++)
                {
                    //Step 4
                    for (int j = 1; j <= m; j++)
                    {
                        // Step 5
                        int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                        // Step 6
                        d[i, j] = Math.Min(
                            Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                            d[i - 1, j - 1] + cost);
                    }
                }
                // Step 7
                return d[n, m];
            }
        }

        public static class JaroWinklerDistance
        {
            /* The Winkler modification will not be applied unless the 
             * percent match was at or above the mWeightThreshold percent 
             * without the modification. 
             * Winkler's paper used a default value of 0.7
             */
            private static readonly double mWeightThreshold = 0.7;

            /* Size of the prefix to be concidered by the Winkler modification. 
             * Winkler's paper used a default value of 4
             */
            private static readonly int mNumChars = 4;


            /// <summary>
            /// Returns the Jaro-Winkler distance between the specified  
            /// strings. The distance is symmetric and will fall in the 
            /// range 0 (perfect match) to 1 (no match). 
            /// </summary>
            /// <param name="aString1">First String</param>
            /// <param name="aString2">Second String</param>
            /// <returns></returns>
            public static double distance(string aString1, string aString2)
            {
                return 1.0 - proximity(aString1, aString2);
            }


            /// <summary>
            /// Returns the Jaro-Winkler distance between the specified  
            /// strings. The distance is symmetric and will fall in the 
            /// range 0 (no match) to 1 (perfect match). 
            /// </summary>
            /// <param name="aString1">First String</param>
            /// <param name="aString2">Second String</param>
            /// <returns></returns>
            public static double proximity(string aString1, string aString2)
            {
                int lLen1 = aString1.Length;
                int lLen2 = aString2.Length;
                if (lLen1 == 0)
                    return lLen2 == 0 ? 1.0 : 0.0;

                int lSearchRange = Math.Max(0, Math.Max(lLen1, lLen2) / 2 - 1);

                // default initialized to false
                bool[] lMatched1 = new bool[lLen1];
                bool[] lMatched2 = new bool[lLen2];

                int lNumCommon = 0;
                for (int i = 0; i < lLen1; ++i)
                {
                    int lStart = Math.Max(0, i - lSearchRange);
                    int lEnd = Math.Min(i + lSearchRange + 1, lLen2);
                    for (int j = lStart; j < lEnd; ++j)
                    {
                        if (lMatched2[j]) continue;
                        if (aString1[i] != aString2[j])
                            continue;
                        lMatched1[i] = true;
                        lMatched2[j] = true;
                        ++lNumCommon;
                        break;
                    }
                }
                if (lNumCommon == 0) return 0.0;

                int lNumHalfTransposed = 0;
                int k = 0;
                for (int i = 0; i < lLen1; ++i)
                {
                    if (!lMatched1[i]) continue;
                    while (!lMatched2[k]) ++k;
                    if (aString1[i] != aString2[k])
                        ++lNumHalfTransposed;
                    ++k;
                }
                // System.Diagnostics.Debug.WriteLine("numHalfTransposed=" + numHalfTransposed);
                int lNumTransposed = lNumHalfTransposed / 2;

                // System.Diagnostics.Debug.WriteLine("numCommon=" + numCommon + " numTransposed=" + numTransposed);
                double lNumCommonD = lNumCommon;
                double lWeight = (lNumCommonD / lLen1
                                 + lNumCommonD / lLen2
                                 + (lNumCommon - lNumTransposed) / lNumCommonD) / 3.0;

                if (lWeight <= mWeightThreshold) return lWeight;
                int lMax = Math.Min(mNumChars, Math.Min(aString1.Length, aString2.Length));
                int lPos = 0;
                while (lPos < lMax && aString1[lPos] == aString2[lPos])
                    ++lPos;
                if (lPos == 0) return lWeight;
                return lWeight + 0.1 * lPos * (1.0 - lWeight);

            }


        }

        public static void CmdExecuter(string comm, string app, int arg)
        {
            Process p = new Process();
            switch (arg)
            {
                case 0: // Open
                    p.StartInfo.FileName = app;
                    Process.Start(@comm);
                    break;
                case 1: // Close
                    Process.GetProcessesByName(app)[0].Kill();                    
                    break;
                case 2: // Search
                    p.StartInfo.FileName = app;
                    Process.Start(@comm, app);
                    break;
                case 3: // Google
                    p.StartInfo.FileName = app;
                    Process.Start(@comm, "google.com/search?q="+ app);
                    break;
                case 4: // Play
                    Process.Start(@comm);
                    break;
                default:
                    break;
            }   
            
        }
        public static async Task RecognizeSpeechAsync(bool auth)
        {

            var config = SpeechConfig.FromSubscription("c7ac72200af34cda9947c0c022207e2d", "westus");

            // Creates a speech recognizer.
            using (var recognizer = new SpeechRecognizer(config))
            {
                string pathPrograms = "c:\\Users\\bedir\\OneDrive\\Masaüstü\\Programlar"; //
                string pathGames = "c:\\Users\\bedir\\OneDrive\\Masaüstü\\Oyunlar";
                string[] commands = { "open", "close", "search", "google", "play" }; // "Open" runs an application. "Search" opens a website. "Google" makes a search on google. "Play" opens a game.
                string[] programs = Directory.GetFiles(@pathPrograms); //Getting list of all files in the programs directory
                string[] games    = Directory.GetFiles(@pathGames); //Getting list of all files in the games directory

                // Starts speech recognition, and returns after a single utterance is recognized. The end of a
                // single utterance is determined by listening for silence at the end or until a maximum of 15
                // seconds of audio is processed.  The task returns the recognition text as result. 
                // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
                // shot recognition like command or query. 
                // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                var result = await recognizer.RecognizeOnceAsync();

                string speech = result.Text.Substring(0, result.Text.Length - 1); //Getting the phrase as text. Removing the last charachter which is a dot.

                if (speech == "Hello") // First you need to say "Hello" before giving a command. 
                {
                    Console.WriteLine("Hello Sir. Tell me what to do!");
                    RecognizeSpeechAsync(true).Wait();
                }
                else if (auth)
                {
                    if (speech == "Exit")
                    {
                        Console.WriteLine("Terminating the program");
                        Environment.Exit(1);
                    }
  
                    int firstIndex = speech.IndexOf(" "); // Divide the phrase to two from first space charachter. 
                    string[] phrase = { speech.Substring(0, firstIndex), speech.Substring(firstIndex+1) }; // First phrase is command. Second phrase is the desired app to run.
                    // Example: if the phrase is "Open Google Chrome" then "Open" is the command and the "Google Chrome" is the app to run.

                    double maxDistanceComm = 999; // In Levenshtein algorithm, lower the score means higher similarity. So i set max distance 999.
                    double maxDistanceApp  = 999;
                    int selectedCommand    = 0;
                    string desiredApp      = "";
                    double jaroComm;
                    int commandsLength = commands.Length;
                    
                    for (int i = 0; i < commandsLength; i++)
                    {
                        jaroComm = LevenshteinDistance.Compute(commands[i], phrase[0]);
                        if (jaroComm < maxDistanceComm)
                        {
                            maxDistanceComm = jaroComm;
                            selectedCommand = i;
                        }
                    }

                    string fileName = "";
                    string gameName = "";
                    string selectedFileName = "";
                    string selectedGameName = "";
                    if (selectedCommand == 0 || selectedCommand == 1) // If the command is "Open" or "Close"
                    {
                        int programsLength = programs.Length;
                        for (int i = 0; i < programsLength; i++)
                        {
                            int lastIndex = programs[i].LastIndexOf("\\");
                            fileName = programs[i].Substring(lastIndex); //Application names come with directory so i remove it
                            jaroComm = LevenshteinDistance.Compute(fileName, phrase[1]); //Getting similarity score between phrase and applications
                            if (jaroComm < maxDistanceApp)
                            {
                                maxDistanceApp = jaroComm;
                                desiredApp = programs[i];
                                selectedFileName = fileName; // Getting the most similar application to phrase that given.
                            }
                        }
                        CmdExecuter(desiredApp, selectedFileName, selectedCommand);
                        /* 
                         1.parameter = App name with path
                         2.parameter = App name 
                         3.parameter = "Open" command
                        */
                    }
                    else if (selectedCommand == 4) // If the command is "Play"
                    {
                        int gamesLength = games.Length;
                        for (int i = 0; i < gamesLength; i++)
                        {
                            int lastIndex = games[i].LastIndexOf("\\");
                            gameName = games[i].Substring(lastIndex);
                            jaroComm = LevenshteinDistance.Compute(gameName, phrase[1]);
                            if (jaroComm < maxDistanceApp)
                            {
                                maxDistanceApp = jaroComm;
                                desiredApp = games[i];
                                selectedGameName = gameName;
                            }
                        }
                        CmdExecuter(desiredApp, selectedGameName, selectedCommand);
                        /* 
                         1.parameter = Game name with path
                         2.parameter = Game name 
                         3.parameter = "Play" command
                        */
                    }
                    else // If the command is "Google" or "Search"
                    {
                        CmdExecuter(pathPrograms + "\\Google Chrome.lnk", phrase[1], selectedCommand);
                        /* 
                         1.parameter = browser path
                         2.parameter = search phrase or website 
                         3.parameter = "Google" or "Search" command
                        */
                    }
                }
                RecognizeSpeechAsync(false).Wait();
            }
        }
        static void Main(string[] args)
        {
            RecognizeSpeechAsync(false);
            Console.ReadLine();
        }
    }
}
