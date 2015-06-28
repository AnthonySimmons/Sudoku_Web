using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sudoku_Web.Models
{
    public static class SudokuModels
    {
        private static Sudoku.Sudoku sudokuGame;

        public static Sudoku.Sudoku SudokuGame
        {
            get
            {
                if (sudokuGame == null)
                {
                    sudokuGame = new Sudoku.Sudoku();
                    sudokuGame.NewGame();
                    sudokuGame.IsComplete();
                }
                return sudokuGame;
            }
        }
    }
}