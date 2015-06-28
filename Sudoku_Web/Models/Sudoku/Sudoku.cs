using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sudoku_Web.Models.Sudoku
{
    

    public class Sudoku
    {
        public DateTime startTime;

        public DateTime endTime;

        int NumCells;

        public List<Cell> UserSolvedCells = new List<Cell>();

        Dictionary<Difficulty, int> DifficultyCells = new Dictionary<Difficulty, int>();

        public Cell[,] Cells = new Cell[9, 9];
        
        public bool Completed = false;

        public bool Creating = false;

        public Difficulty difficulty;

        public bool HasUpdates = false;

        public Sudoku solution;

        public Sudoku nextSudoku;

        public Cell SelectedCell = new Cell();

        public TimeSpan gameTime;

        public int TimeCount;

        public string TimeString
        {
            get
            {
                //TimeSpan completionTime = DateTime.Now.Subtract (startTime);
                //completionTime = completionTime.Add (gameTime);
                int min = TimeCount / 60;
                int sec = TimeCount % 60;
                string timeStr = min + ":";
                if (sec <= 9)
                {
                    timeStr += "0";
                }
                timeStr += sec;
                return timeStr;
            }
        }


        public delegate bool UpdateProgressBarEvent(int progress);
        public event UpdateProgressBarEvent UpdateProgressBar;

        public delegate bool FinishCreateEvent();
        public event FinishCreateEvent FinishCreate;

        public Sudoku()
        {
            Clear();
            DifficultyCells.Add(Difficulty.Easy, 30);
            DifficultyCells.Add(Difficulty.Medium, 40);
            DifficultyCells.Add(Difficulty.Hard, 50);
        }

        public Sudoku(Sudoku source)
        {
            CopyCells(source);
            UpdateProgressBar = source.UpdateProgressBar;
        }


        public void UndoCell()
        {
            if (UserSolvedCells.Count > 0)
            {
                Cell cell = UserSolvedCells.Last();
                Cells[cell.Row, cell.Column].Value = 0;
                UserSolvedCells.RemoveAt(UserSolvedCells.Count - 1);
                ResetCellPossibles();
            }
        }


        public void UpdateGameTime()
        {
            TimeSpan completionTime = DateTime.Now.Subtract(startTime);
            gameTime = completionTime.Add(gameTime);
        }

        private void CopyCells(Sudoku source)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Cells[i, j] == null)
                    {
                        Cells[i, j] = new Cell();
                    }
                    Cells[i, j].CopyCell(source.Cells[i, j]);
                }
            }
        }

        public void CreateNextSudoku(bool useThread)
        {
            nextSudoku = new Sudoku();

            if (useThread)
            {
                nextSudoku.CreateSudokuThread();
            }
            else
            {
                nextSudoku.CreateSudoku();
            }
        }

        public void CopyNextSudoku()
        {
            if (nextSudoku == null)
            {
                nextSudoku = new Sudoku();
                nextSudoku.NewGame();
            }
            if (solution == null)
            {
                solution = new Sudoku();
            }


            CopyCells(nextSudoku);
            solution.CopyCells(nextSudoku.solution);
            Completed = false;
            SelectedCell = null;

            //CreateNextSudoku (true);

            //RemoveCells();
            //ResetCellPossibles();
            //Creating = false;
            HasUpdates = true;
            //FinishCreating ();
            //SetReadOnly ();

        }

        public void NewGame()
        {
            UserSolvedCells = new List<Cell>();
            CreateSudoku();
            ResetCellPossibles();
            RemoveCells();
            SetReadOnly();
            SaveToFile(SudokuFactory.SavedSudokuFile);
            File.Delete(SudokuFactory.GameLogFile);
            startTime = DateTime.Now;
            gameTime = new TimeSpan();
        }

        public void Clear()
        {
            gameTime = new TimeSpan();
            startTime = DateTime.Now;
            Completed = false;
            NumCells = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Cells[i, j] == null)
                    {
                        Cells[i, j] = new Cell();
                    }
                    Cells[i, j].Row = i;
                    Cells[i, j].Column = j;
                    Cells[i, j].Box = GetBoxNum(i, j);
                    Cells[i, j].Value = 0;
                    Cells[i, j].ReadOnly = false;
                    Cells[i, j].Possibles = GetFullList();
                }
            }
        }

        public string GetSudokuString()
        {
            string str = String.Empty;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    str += Cells[i, j].Value.ToString() + "|";
                }
            }
            return str;
        }

        public void CreateSudokuThread()
        {
            Thread thread = new Thread(CreateSudoku);
            thread.Start();
        }


        public void CreateSudoku()
        {
            Clear();
            Completed = false;
            Creating = true;

            
            while (!IsComplete())
            {
                if (!CreateRandom())
                {
                    Clear();
                }
            }

            solution = new Sudoku(this);

        }

        private void FinishCreating()
        {
            FinishCreateEvent copy = FinishCreate;
            if (copy != null)
            {
                copy();
            }
        }

        private void UpdateProgress()
        {
            var copy = UpdateProgressBar;
            if (copy != null)
            {
                copy(NumCells);
            }
        }

        public void SetReadOnly()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Cells[i, j].ReadOnly = (Cells[i, j].Value != 0);
                }
            }
        }

        public bool FillOnlySpot()
        {
            bool solved = false;
            for (int i = 0; i < 9; i++)
            {
                if (solved = FillOnlySpotRows(i))
                {
                    break;
                }
                if (solved = FillOnlySpotColumns(i))
                {
                    break;
                }
                if (solved = FillOnlySpotBox(i))
                {
                    break;
                }
            }
            if (solved)
            {
                UpdateCellPossibles();
            }
            return solved;
        }


        public bool FillOnlySpotRows(int row)
        {
            Dictionary<int, int> possibleCount = new Dictionary<int, int>();
            Dictionary<int, Cell> onlySpots = new Dictionary<int, Cell>();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Cells[row, i].Possibles.Contains(j + 1) && Cells[row, i].Value == 0)
                    {
                        if (!possibleCount.Keys.Contains(j + 1))
                        {
                            possibleCount.Add(j + 1, 1);
                            onlySpots.Add(j + 1, Cells[row, i]);
                        }
                        else
                        {
                            possibleCount[j + 1]++;
                        }
                    }
                }
            }

            bool solved = false;
            foreach (int i in possibleCount.Keys)
            {
                if (possibleCount[i] == 1 && onlySpots[i].Value == 0 && onlySpots[i].Possibles.Contains(i))
                {
                    solved = true;
                    onlySpots[i].Value = i;
                    break;
                }
            }
            //UpdateCellPossibles();
            return solved;
        }


        public bool FillOnlySpotColumns(int row)
        {
            Dictionary<int, int> possibleCount = new Dictionary<int, int>();
            Dictionary<int, Cell> onlySpots = new Dictionary<int, Cell>();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Cells[i, row].Possibles.Contains(j + 1) && Cells[i, row].Value == 0)
                    {
                        if (!possibleCount.Keys.Contains(j + 1))
                        {
                            possibleCount.Add(j + 1, 1);
                            onlySpots.Add(j + 1, Cells[i, row]);
                        }
                        else
                        {
                            possibleCount[j + 1]++;
                        }
                    }
                }
            }

            bool solved = false;
            foreach (int i in possibleCount.Keys)
            {
                if (possibleCount[i] == 1 && onlySpots[i].Value == 0 && onlySpots[i].Possibles.Contains(i))
                {
                    solved = true;
                    onlySpots[i].Value = i;
                    break;
                }
            }
            //UpdateCellPossibles();
            return solved;
        }



        public bool FillOnlySpotBox(int box)
        {

            Dictionary<int, int> possibleCount = new Dictionary<int, int>();
            Dictionary<int, Cell> onlySpots = new Dictionary<int, Cell>();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Cells[i, j].Box == box)
                    {
                        for (int k = 0; k < 9; k++)
                        {
                            if (Cells[i, j].Possibles.Contains(k + 1) && Cells[i, j].Value == 0)
                            {
                                if (!possibleCount.Keys.Contains(k + 1))
                                {
                                    possibleCount.Add(k + 1, 1);
                                    onlySpots.Add(k + 1, Cells[i, j]);
                                }
                                else
                                {
                                    possibleCount[k + 1]++;
                                }
                            }
                        }
                    }
                }
            }

            bool solved = false;
            foreach (int i in possibleCount.Keys)
            {
                if (possibleCount[i] == 1 && onlySpots[i].Value == 0 && onlySpots[i].Possibles.Contains(i))
                {
                    solved = true;
                    onlySpots[i].Value = i;
                    break;
                }
            }
            //UpdateCellPossibles();
            return solved;
        }

        public List<Cell> GetUnsolvedCells()
        {
            List<Cell> unsolved = new List<Cell>();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Cells[i, j].Value == 0)
                    {
                        unsolved.Add(new Cell(Cells[i, j]));
                    }
                }
            }

            return unsolved;
        }

        

        public bool RevealAllCells()
        {
            bool reveal = false;

            if (solution != null)
            {
                CopyCells(solution);
            }

            return reveal;
        }

        public bool RevealOneCell()
        {
            bool reveal = false;
            if (solution != null)
            {
                List<Cell> unsolvedCells = GetUnsolvedCells();
                if (unsolvedCells.Count > 0)
                {
                    if (false && SelectedCell != null && !SelectedCell.ReadOnly
                        && SelectedCell.Value != solution.Cells[SelectedCell.Row, SelectedCell.Column].Value)
                    {
                        Cells[SelectedCell.Row, SelectedCell.Column].Value =
                            solution.Cells[SelectedCell.Row, SelectedCell.Column].Value;
                        UserSolvedCells.Add(Cells[SelectedCell.Row, SelectedCell.Column]);
                    }
                    else
                    {
                        Random rand = new Random(Guid.NewGuid().GetHashCode());
                        int index = rand.Next(0, unsolvedCells.Count);
                        int i = unsolvedCells[index].Row;
                        int j = unsolvedCells[index].Column;
                        if (Cells[i, j].Value == 0)
                        {
                            Cells[i, j].Value = solution.Cells[i, j].Value;
                            UserSolvedCells.Add(Cells[i, j]);
                        }
                    }
                }
            }
            UpdateCellPossibles();
            return reveal;
        }


        public bool SolveOneCell()
        {
            bool solve = false;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Cells[i, j].Value == 0 && Cells[i, j].Possibles.Count == 1)
                    {
                        Cells[i, j].Value = Cells[i, j].Possibles[0];
                        solve = true;
                        NumCells++;
                        break;
                    }
                }
                if (solve)
                {
                    break;
                }
            }

            if (!solve)
            {
                solve = FillOnlySpot();
            }

            UpdateCellPossibles();
            return solve;
        }


        public void SaveSolution(string filename)
        {
            if (solution != null)
            {
                solution.SaveToFile(filename);
            }
            else
            {
                File.Delete(filename);
                File.Create(filename);
            }
        }

        public void LoadStartTime(string filename)
        {
            if (File.Exists(filename))
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    string timeString = reader.ReadLine();
                    int timeCount;
                    if (Int32.TryParse(timeString, out timeCount))
                    {
                        TimeCount = timeCount;
                    }
                }
            }
        }

        public void SaveStartTime(string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine(TimeCount.ToString());
            }
        }

        public void LoadSolution(string filename)
        {
            if (solution == null)
            {
                solution = new Sudoku();
            }
            solution.LoadFromFile(filename);
        }

        public void SaveToFile(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                //string allText = String.Empty;
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    for (int i = 0; i < 9; i++)
                    {
                        string line = String.Empty;
                        for (int j = 0; j < 9; j++)
                        {
                            int val = Cells[i, j].Value;

                            if (Cells[i, j].ReadOnly)
                            {
                                val *= -1;
                            }

                            line += val.ToString();
                            if (j != 8)
                            {
                                line += " ";
                            }
                        }
                        writer.WriteLine(line);
                        //allText += line + "\n";
                    }
                }
                //File.WriteAllText (fullname, allText);
            }
        }

        public void LoadFromFile(string filename)
        {
            string fullname = Path.Combine(Directory.GetCurrentDirectory(), filename);
            if (File.Exists(filename) && Path.GetExtension(fullname) == ".txt")
            {
                using (StreamReader reader = File.OpenText(fullname))
                {
                    int row = 0;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] numsArr = line.Split(' ');
                        int col = 0;
                        foreach (string str in numsArr)
                        {
                            bool IsReadOnly = false;
                            char numChar = str.First();
                            if (str.Length == 2 && str.StartsWith("-"))
                            {
                                IsReadOnly = true;
                                numChar = str.Last();
                            }
                            if (char.IsDigit(numChar))
                            {
                                int num;
                                if (Int32.TryParse(numChar.ToString(), out num))
                                {
                                    //Saved as negative number means its Readonly (Auto Generated)
                                    Cells[row, col].Value = num;
                                    Cells[row, col].ReadOnly = IsReadOnly;
                                    col++;
                                }
                                else
                                {

                                }
                            }
                        }
                        row++;
                    }
                }
            }

            if (!IsValid())
            {
                Clear();
            }
            ResetCellPossibles();
        }


        private List<int> GetFullList()
        {
            return new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        }

        #region Create


        public bool RemoveCells()
        {
            int cellsRemove = DifficultyCells[difficulty];

            int i = 0;
            while (i < cellsRemove)
            {
                Random rand = new Random(Guid.NewGuid().GetHashCode());
                int p = rand.Next(0, 9);
                int q = rand.Next(0, 9);
                if (Cells[p, q].Value != 0)
                {
                    Cells[p, q].Value = 0;
                    Cells[p, q].ReadOnly = false;
                    i++;
                }
            }

            return true;
        }


        public bool CreateRandom()
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            bool progress = false;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Cells[i, j].Value == 0 && Cells[i, j].Possibles.Count > 0)
                    {
                        int index = rand.Next(0, Cells[i, j].Possibles.Count);
                        Cells[i, j].Value = Cells[i, j].Possibles[index];
                        if (!IsValid())
                        {
                            Cells[i, j].Possibles.Remove(Cells[i, j].Value);
                            Cells[i, j].Value = 0;
                            ResetCellPossibles();
                        }
                        else
                        {
                            progress = true;
                            NumCells++;
                            UpdateProgress();
                            UpdateCellPossibles();
                            while (SolveOneCell())
                            {
                                UpdateProgress();
                            }
                        }
                    }
                }
            }
            return progress;

        }


        public void MirrorGrid()
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            int hor = rand.Next(0, 2);
            int ver = rand.Next(0, 2);

            for (int i = 0; i < 9; i++)
            {
                int p = 0, q = 8;
                while (p <= q)
                {
                    if (hor == 1)
                    {
                        int tmp = Cells[i, q].Value;
                        Cells[i, q].Value = Cells[i, p].Value;
                        Cells[i, p].Value = tmp;
                    }
                    if (ver == 1)
                    {
                        int tmp = Cells[q, i].Value;
                        Cells[q, i].Value = Cells[p, i].Value;
                        Cells[p, i].Value = tmp;
                    }
                    p++;
                    q--;
                }
            }
        }

        #endregion Create


        public bool IsComplete()
        {
            Completed = CheckAllCompleteCells();
            return Completed;
        }

        public bool CheckAllCompleteCells()
        {
            bool complete = true;
            NumCells = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Cells[i, j].Value == 0)
                    {
                        complete = false;
                        break;
                    }
                }
                if (!complete)
                {
                    break;
                }
            }

            return complete;
        }

        public bool IsValid()
        {
            bool valid = true;

            if (!IsValidBox())
            {
                valid = false;
            }
            else
            {
                for (int i = 0; i < 9; i++)
                {
                    if (!IsValidRow(i) || !IsValidColumn(i) || !IsValidPossibles(i))
                    {
                        valid = false;
                        break;
                    }
                }
            }
            return valid;
        }

        public bool IsValidPossibles(int row)
        {
            bool valid = true;
            for (int j = 0; j < 9; j++)
            {
                if (Cells[row, j].Possibles.Count == 0 && Cells[row, j].Value == 0)
                {
                    valid = false;
                }
            }
            return valid;
        }

        public bool IsValidRow(int row)
        {
            List<int> nums = GetFullList();
            bool valid = true;
            for (int i = 0; i < 9; i++)
            {
                if (nums.Contains(Cells[row, i].Value))
                {
                    nums.Remove(Cells[row, i].Value);
                }
                else if (Cells[row, i].Value != 0)
                {
                    valid = false;
                    break;
                }
            }
            if (nums.Any())
            {
                //valid = false;
            }

            return valid;
        }


        public bool IsValidColumn(int column)
        {
            List<int> nums = GetFullList();
            bool valid = true;
            for (int i = 0; i < 9; i++)
            {
                if (nums.Contains(Cells[i, column].Value))
                {
                    nums.Remove(Cells[i, column].Value);
                }
                else if (Cells[i, column].Value != 0)
                {
                    valid = false;
                    break;
                }
            }
            if (nums.Any())
            {
                //valid = false;
            }

            return valid;
        }

        public bool IsValidBox()
        {
            bool valid = true;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        for (int l = 0; l < 9; l++)
                        {
                            if (i != k && k != l
                                && Cells[i, j].Box == Cells[k, l].Box
                                && Cells[i, j].Value == Cells[k, l].Value
                                && Cells[i, j].Value != 0)
                            {
                                valid = false;
                                return valid;
                            }
                        }
                    }
                }
            }

            return valid;
        }

        public void UpdateCellHighlights()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Cells[i, j].Highlight = false;
                    if (SelectedCell != null)
                    {
                        if (SelectedCell.Row == i || SelectedCell.Column == j
                           || Cells[i, j].Box == SelectedCell.Box)
                        {
                            Cells[i, j].Highlight = true;
                        }
                    }
                }
            }
        }

        public void UpdateCellPossiblesThread()
        {
            Thread thread = new Thread(ResetCellPossibles);
            thread.Start();
        }

        public void UpdateCellPossibles()
        {
            for (int i = 0; i < 9; i++)
            {
                UpdateRowPossibles(i);
                UpdateColumnPossibles(i);
                UpdateBoxPossibles(i);
            }
        }

        public void ResetCellPossibles()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Cells[i, j].Possibles = GetFullList();
                }
                UpdateRowPossibles(i);
                UpdateColumnPossibles(i);
                UpdateBoxPossibles(i);
            }
        }

        public void UpdateRowPossibles(int row)
        {
            List<int> rowValues = new List<int>();

            for (int i = 0; i < 9; i++)
            {
                rowValues.Add(Cells[row, i].Value);
            }

            for (int i = 0; i < 9; i++)
            {
                //Cells[row, i].Possibles = GetFullList();
                Cells[row, i].Possibles.RemoveAll(m => rowValues.Contains(m) && m != Cells[row, i].Value);
                //Cells [row, i].Highlight = (SelectedCell != null && SelectedCell.Row == row);
            }
        }


        public void UpdateColumnPossibles(int row)
        {
            List<int> rowValues = new List<int>();

            for (int i = 0; i < 9; i++)
            {
                rowValues.Add(Cells[i, row].Value);
            }

            for (int i = 0; i < 9; i++)
            {
                //Cells[i, row].Possibles = GetFullList();
                Cells[i, row].Possibles.RemoveAll(m => rowValues.Contains(m) && m != Cells[i, row].Value);
                //Cells[i, row].Highlight = (SelectedCell != null && SelectedCell.Column == row);
            }
        }

        public void UpdateBoxPossibles(int box)
        {
            for (int j = 0; j < 9; j++)
            {
                for (int k = 0; k < 9; k++)
                {
                    List<int> boxValues = GetValuesInBox(Cells[j, k].Box);
                    //Cells[j, k].Possibles = GetFullList();
                    Cells[j, k].Possibles.RemoveAll(m => boxValues.Contains(m) && m != Cells[j, k].Value);
                    //Cells[j, k].Highlight = (SelectedCell != null && SelectedCell.Box == box);
                }
            }
        }


        private List<int> GetValuesInBox(int box)
        {
            List<int> boxValues = new List<int>();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Cells[i, j].Box == box && Cells[i, j].Value != 0)
                    {
                        boxValues.Add(Cells[i, j].Value);
                    }
                    else
                    {

                    }
                }
            }

            return boxValues;
        }


        private int GetBoxNum(int i, int j)
        {
            int box = 0;
            if (i < 3 && j < 3)
            {
                box = 1;
            }
            else if (i >= 3 && i < 6 && j < 3)
            {
                box = 2;
            }
            else if (i >= 6 && j < 3)
            {
                box = 3;
            }
            else if (i < 3 && j >= 3 && j < 6)
            {
                box = 4;
            }
            else if (i >= 3 && i < 6 && j >= 3 && j < 6)
            {
                box = 5;
            }
            else if (i >= 6 && j >= 3 && j < 6)
            {
                box = 6;
            }
            else if (i < 3 && j >= 6)
            {
                box = 7;
            }
            else if (i >= 6 && j >= 3 && j < 6)
            {
                box = 8;
            }
            else if (i >= 6 && j >= 6)
            {
                box = 9;
            }

            return box;
        }

    }
}
