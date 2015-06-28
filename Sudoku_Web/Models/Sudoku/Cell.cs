using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

/// <summary>
/// Author: Anthony Simmons (AnthonySimmons99@gmail.com)
/// October 2014
/// Description: Object used to store the data and operations
/// necessary for each cell in the sudoku grid.
/// The sudoku grid will have an array of 81 Cells
/// </summary>

namespace Sudoku_Web.Models.Sudoku
{
    public class Cell : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _value;

        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                //OnPropertyChanged ("Value");
            }
        }

        public int Row;

        public int Column;

        public int Box;

        public bool ReadOnly = true;

        public bool Highlight = false;

        public string Id
        {
            get
            {
                return Row.ToString() + "_" + Column.ToString();
            }
        }

        public List<int> Possibles = new List<int>();

        public string PossiblesString
        {
            get
            {
                var str = String.Empty;

                foreach (int val in Possibles)
                {
                    str += val.ToString() + ",";
                }

                return str;
            }
        }

        public Cell()
        {
            for (int i = 1; i <= 9; i++)
            {
                Possibles.Add(i);
            }
        }


        public Cell(Cell source)
        {
            CopyCell(source);
        }

        public void CopyCell(Cell source)
        {
            Possibles = new List<int>(source.Possibles);
            Value = source.Value;
            Row = source.Row;
            Column = source.Column;
            Box = source.Box;
            ReadOnly = source.ReadOnly;
            Highlight = source.Highlight;
        }

        public void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler copy = PropertyChanged;
            if (copy != null)
            {
                copy(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
