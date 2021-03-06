﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;

namespace ImageComposeEditorAutomation
{
    class Program
    {
        static void Main(string[] args)
        {
            var composeApp = new ComposeAppService();
            if (args[0] == "compose") {
                Console.WriteLine("composing...");
                composeApp.Compose(args.Skip(1).ToArray(), m => Console.WriteLine(m), i => drawTextProgressBar(1, 100));
            }
            else if(args[0] == "process" || args[0] == "processAndSave") 
            {                
                Console.WriteLine("process...");
                var num = args.Length > 1 ? int.Parse(args[1]) : 3;
                var extension = args.Length > 2 ? args[2] : "*.JPG";
                if (args.Length > 3)
                    Directory.SetCurrentDirectory(args[3]);
                var files = GroupFiles(extension, num, ignoreStichInName: true);
                int total = files.Count;
                int count = 0;
                foreach (var item in files)
                {
                    count++;
                    Console.WriteLine(string.Format("composing {0} of {1}....", count, total));
                    var saveProject = args[0] == "processAndSave";
                    composeApp.Compose(item, m => Console.WriteLine(m), i => drawTextProgressBar(i, 100), saveProject: saveProject);
                }
            }
            Console.WriteLine("Finished.");
        }

        private static List<string[]> GroupFiles(string extension, int groupNum, bool ignoreStichInName = false)
        {
            string[] filePaths = Directory.GetFiles(Directory.GetCurrentDirectory(), extension, SearchOption.TopDirectoryOnly);
            string[] stichFilePaths = null;

            if (ignoreStichInName)
            {
                stichFilePaths = filePaths.Where(f => IsStitchResult(Path.GetFileName(f))).Select(s => Path.GetFileName(s).ToLower()).ToArray();
                filePaths = filePaths.Where(f => !IsStitchResult(Path.GetFileName(f))).ToArray();              
            }

            var grouped = filePaths.Select((value, index) => new { value, index })
                    .GroupBy(x => x.index / groupNum, x => Path.GetFileName(x.value)).Select(g => g.ToArray()).ToList();

            if (ignoreStichInName)
            {
                grouped = grouped.Where(g => !FileAlreadyInStich(g[0], stichFilePaths)).ToList();
            }

            return grouped;
        }

        private static bool FileAlreadyInStich(string fileName, string[] stichFilePaths)
        {

            var stichName = Path.GetFileNameWithoutExtension(fileName) + "_stitch" + Path.GetExtension(fileName);
            return stichFilePaths.Contains(stichName.ToLower());
        }

        private static bool IsStitchResult(string fileName)
        {
            return fileName.Contains("_stitch");            
        }

        private static void drawTextProgressBar(int progress, int total)
        {
            ////draw empty progress bar
            //Console.CursorLeft = 0;
            //Console.Write("["); //start
            //Console.CursorLeft = 32;
            //Console.Write("]"); //end
            //Console.CursorLeft = 1;
            //float onechunk = 30.0f / total;

            ////draw filled part
            //int position = 1;
            //for (int i = 0; i < onechunk * progress; i++)
            //{
            //    Console.BackgroundColor = ConsoleColor.Gray;
            //    Console.CursorLeft = position++;
            //    Console.Write(" ");
            //}

            ////draw unfilled part
            //for (int i = position; i <= 31; i++)
            //{
            //    Console.BackgroundColor = ConsoleColor.Green;
            //    Console.CursorLeft = position++;
            //    Console.Write(" ");
            //}

            ////draw totals
            //Console.CursorLeft = 35;
            //Console.BackgroundColor = ConsoleColor.Black;
            Console.CursorLeft = 15;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }
    }
}
