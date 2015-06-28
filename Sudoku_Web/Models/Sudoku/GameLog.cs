using System;
using System.IO;
using System.Collections.Generic;

namespace Sudoku_Web.Models.Sudoku
{
    public class GameRecord
    {
        public Difficulty Difficulty;
        public DateTime DateCompleted;

        public string TimeString;

        public string RecordString
        {
            get
            {
                return String.Format("{0} {1} {2}", DateCompleted.ToShortDateString(), TimeString, Difficulty.ToString());
            }
        }

        public GameRecord()
        {

        }

        public GameRecord(Sudoku sudoku)
        {
            DateCompleted = DateTime.Now;
            Difficulty = sudoku.difficulty;
            TimeString = sudoku.TimeString;
        }

        public GameRecord(string recordString)
        {
            ParseGameRecord(recordString);
        }

        public bool ParseGameRecord(string recordString)
        {
            //DateCompleted CompletionTime Difficulty
            string[] strArr = recordString.Split(new char[] { ' ' });
            bool success = false;
            if (strArr.Length > 2)
            {
                success = DateTime.TryParse(strArr[0], out DateCompleted);
                TimeString = strArr[1];
                success &= Enum.TryParse(strArr[2], out Difficulty);
            }
            return success;
        }


        public static int Compare(GameRecord x, GameRecord y)
        {
            int num = 0;
            if (x.DateCompleted > y.DateCompleted)
            {
                num = 1;
            }
            else
            {
                num = -1;
            }
            return num;
        }

    }

    public class GameLog
    {
        public List<GameRecord> GameRecords = new List<GameRecord>();

        public GameLog()
        {

        }


        public void AddRecord(Sudoku sudoku)
        {
            GameRecords.Add(new GameRecord(sudoku));
        }

        public string GetDisplayString()
        {
            string displayString = String.Empty;
            int max = 10;
            int count = 0;

            GameRecords.Sort(GameRecord.Compare);

            foreach (GameRecord record in GameRecords)
            {
                displayString += record.RecordString + "\n";
                if (++count > max)
                {
                    break;
                }
            }

            return displayString;
        }

        public void LoadGameRecords(string filename)
        {
            if (File.Exists(filename))
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    //DateCompleted CompletionTime Difficulty \n
                    string allText = reader.ReadToEnd();
                    string[] strArr = allText.Split('\n');

                    foreach (string line in strArr)
                    {
                        GameRecord record = new GameRecord();
                        if (record.ParseGameRecord(line))
                        {
                            GameRecords.Add(record);
                        }
                    }
                }
            }
        }

        public void SaveGameRecords(string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (GameRecord record in GameRecords)
                {
                    writer.WriteLine(record.RecordString);
                }
            }
        }

    }
}

