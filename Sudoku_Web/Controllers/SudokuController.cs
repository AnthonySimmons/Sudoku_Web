using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sudoku_Web.Models;

namespace Sudoku_Web.Controllers
{
    public class SudokuController : Controller
    {
        // GET: Sudoku
        public ActionResult Sudoku()
        {
            return View(model: SudokuModels.SudokuGame);
        }

        private bool WithinRange(int num)
        {
            return num >= 0 && num < 9;
        }

        [HttpGet]
        public ActionResult GetSelectedCell(int i, int j)
        {
            if (WithinRange(i) && WithinRange(j))
            {
                SudokuModels.SudokuGame.SelectedCell = SudokuModels.SudokuGame.Cells[i, j];
            }
            return RedirectToAction("Sudoku");
        }

        [HttpGet]
        public ActionResult GetCellValue(int i, int j, string val)
        {
            int value = 0;
            if (int.TryParse(val, out value))
            {
                SudokuModels.SudokuGame.Cells[i, j].Value = value;
                SudokuModels.SudokuGame.SelectedCell = SudokuModels.SudokuGame.Cells[i, j];
                SudokuModels.SudokuGame.ResetCellPossibles();
            }
            return RedirectToAction("Sudoku");
        }

        [HttpPost]
        public ActionResult PostNewGame(string difficulty)
        {
            Models.Sudoku.Difficulty diff;
            if (Enum.TryParse(difficulty, out diff))
            {
                SudokuModels.SudokuGame.difficulty = diff;
            }

            SudokuModels.SudokuGame.NewGame();
            return RedirectToAction("Sudoku");
        }

        [HttpPost]
        public ActionResult PostHint()
        {
            SudokuModels.SudokuGame.RevealOneCell();
            return RedirectToAction("Sudoku");
        }

        [HttpPost]
        public ActionResult PostSolve()
        {
            SudokuModels.SudokuGame.RevealAllCells();
            return RedirectToAction("Sudoku");
        }
    }
}