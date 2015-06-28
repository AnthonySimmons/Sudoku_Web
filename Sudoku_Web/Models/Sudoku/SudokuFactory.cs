using System;
using System.IO;

namespace Sudoku_Web.Models.Sudoku
{
    public static class SudokuFactory
    {
        public static int Width, Height;

        public static string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private static string gameLogFile;

        public static string GameLogFile
        {
            get
            {
                string dirname = Path.Combine(AppDataPath, "Sudoku");
                gameLogFile = Path.Combine(dirname, "GameLog.txt");
                if (!Directory.Exists(dirname))
                {
                    Directory.CreateDirectory(dirname);
                }
                return gameLogFile;
            }
        }

        private static string savedGameTimeFile;

        public static string SavedGameTimeFile
        {
            get
            {
                string dirname = Path.Combine(AppDataPath, "Sudoku");
                savedGameTimeFile = Path.Combine(dirname, "GameTime.txt");
                if (!Directory.Exists(dirname))
                {
                    Directory.CreateDirectory(dirname);
                }
                return savedGameTimeFile;
            }
        }

        private static string savedSudokuFile;

        public static string SavedSudokuFile
        {
            get
            {
                string dirname = Path.Combine(AppDataPath, "Sudoku");
                savedSudokuFile = Path.Combine(dirname, "sudoku.txt");
                if (!Directory.Exists(dirname))
                {
                    Directory.CreateDirectory(dirname);
                }
                return savedSudokuFile;
            }
        }

        private static string savedSolutionFile;

        public static string SavedSolutionFile
        {
            get
            {
                string dirname = Path.Combine(AppDataPath, "Sudoku");
                savedSolutionFile = Path.Combine(dirname, "solution.txt");
                if (!Directory.Exists(dirname))
                {
                    Directory.CreateDirectory(dirname);
                }
                return savedSolutionFile;
            }
        }


  


        private static Sudoku sudoku;

        public static Sudoku Sudoku
        {
            get
            {
                if (sudoku == null)
                {
                    sudoku = new Sudoku();
                    //sudoku.CreateNextSudoku (false);
                    sudoku.LoadFromFile(SavedSudokuFile);
                }
                return sudoku;
            }
        }


        private static GameLog gameLog;

        public static GameLog GameLog
        {
            get
            {
                if (gameLog == null)
                {
                    gameLog = new GameLog();
                    gameLog.LoadGameRecords(GameLogFile);
                }
                return gameLog;
            }
        }
    }
}

