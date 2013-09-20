/*
 * Crossflip solver
 * www.hacker.org/cross
 * (C) Patrick Stevens 2012, aka. laz0r
 * Version 2.0.1.1
 * 
 * Changes:
 *  In 2.0.1.1:
 *   Converted compile-time switches for benchmarking/single level solving into runtime switches
 *  In 2.0.1:
 *   Added in parallel packed int class, which is marginally faster
 *   Put backSubstitute into SystemOfEquations rather than being a separate method
 *  In 2.0.1 (RC):
 *   Bugfixes and cleaning up of Program so it contains fewer data manipulators
 *  In 2.0.1 (alpha):
 *   Added PackedInt storage of equations for a 2/5 speedup
 *  In 2.0.0 (alpha):
 *   Encapsulated behaviour of systems of equations into interfaces, one for a single equation and one for a system of such equations, so that internal behaviour is totally separate from external.
 *   Also added a small but extensible test suite
 *  In 1.1.2:
 *   Added get/set board to file functionality, so that we don't need to keep requesting boards from the server but can reproduce them locally
 *  In 1.1.1:
 *   Added error handling around submission of a solution, so that if the solution submission fails we keep trying it rather than re-solving the level
 *  In 1.1.0:
 *   improved memory access (using ref parameter passing)
 *   error handling so that if network connectivity is lost, program picks up where it left off
 *   two instances of this program can now be run concurrently (on different machines), as
 *       submission of solutions now specifies the level being submitted
 *  
 * Main body of program is an implementation of Gaussian elimination over GF(2).
 * 
 * TODO:
 * check whether any of the SystemOfEquations descendants would be more efficient if they implemented gaussianEliminate or backSubstitute
 * 
 * */

//#define doingStuffFromFile

//#define writeOutDebugs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Crossflip
{

    /// <summary>
    /// On a board, whether a cell is lit (On), unlit (Off) or greyed-out as Unselectable.
    /// </summary>
    public enum CellState { On = 1, Off = 0, Unselectable = 2 };

    /// <summary>
    /// interface all Equations must match (ie. structures which represent one row of the Gaussian elimination matrix)
    /// </summary>
    public interface IEquation
    {
        byte coefficient(int index);
        void add(IEquation newEqn);
        byte[] ToByteArray();
        byte RHS();
    }

    /// <summary>
    /// interface all systems of equations must match (these will probably be represented internally by IEquations, but all that is specified here is that they must be able to interact via IEquations)
    /// </summary>
    public interface IEquationSystem
    {
        byte coefficient(int row, int col);

        void gaussianEliminate();

        byte[] backSubstitute();

        void swapTwoEquations(int a, int b);

        byte[] nthRowOfEquations(int n);

        void replaceEquation(int rowNum, IEquation newEquation);

        void addTo(int rowAddedToIndex, int rowAddingIndex);
    }

    /// <summary>
    /// Base class for a System of Equations, based around an array of IEquation[] for internal storage
    /// </summary>
    public abstract class SystemOfEquations : IEquationSystem
    {
        /// <summary>
        /// The number of equations which the system is storing
        /// </summary>
        public int numberOfEquations
        {
            get { return _equationSystem.Length; }
        }

        protected IEquation[] _equationSystem;

        public IEquation[] EquationSystem
        { get { return _equationSystem; } }

        public byte[,] LHS
        {
            get
            {
                byte[,] ans = new byte[numberOfEquations, numberOfEquations];
                for (int i = 0; i < numberOfEquations; i++)
                {
                    byte[] currentRow = _equationSystem[i].ToByteArray();
                    for (int j = 0; j < numberOfEquations; j++)
                        ans[i, j] = currentRow[j];
                }
                return ans;
            }
        }

        public byte[] RHS
        {
            get
            {
                byte[] ans = new byte[numberOfEquations];
                for (int i = 0; i < numberOfEquations; i++)
                    ans[i] = _equationSystem[i].RHS();
                return ans;
            }
        }

        /// <summary>
        /// Finds the coefficient (1 or 0) of the col'th variable in the row'th equation
        /// </summary>
        /// <param name="row">Which row the variable is in</param>
        /// <param name="col">Which variable we need</param>
        /// <returns>The coefficient of that variable</returns>
        public byte coefficient(int row, int col)
        {
            return _equationSystem[row].coefficient(col);
        }

        /// <summary>
        /// Swaps the specified equations in both LHS and RHS
        /// </summary>
        /// <param name="a">the index of one of the rows</param>
        /// <param name="b">the index of the other row</param>
        virtual public void swapTwoEquations(int a, int b)
        {
            IEquation equ = _equationSystem[a];
            _equationSystem[a] = _equationSystem[b];
            _equationSystem[b] = equ;
        }

        /// <summary>
        /// Fetches a certain row of the LHS
        /// </summary>
        /// <param name="n">which row to fetch</param>
        /// <returns>the coefficients of the given row</returns>
        virtual public byte[] nthRowOfEquations(int n)
        {
            return _equationSystem[n].ToByteArray();
        }

        /// <summary>
        /// After gaussian elimination, returns the solution to the system of equations
        /// </summary>
        virtual public byte[] backSubstitute()
        {
            byte[] result = new byte[numberOfEquations];
            byte[] tempRHS = RHS;

            for (int row = numberOfEquations - 1; row >= 0; row--)
            {
                //eliminate matrix[row][row]
                if (coefficient(row, row) == 0)
                {
                    result[row] = 0; //don't need to eliminate; 0 is already cancelling out
                }
                else
                {
                    //eliminate this 1
                    if (tempRHS[row] == 0)
                    {
                        //then substituting this variable will not affect the matrix
                        result[row] = 0;
                    }
                    else //the result of this line is 1, so we're going to have to switch any other rows incorporating this variable in equals
                    {
                        for (int i = 0; i < row; i++)
                        {
                            if (coefficient(i, row) == 1)
                                tempRHS[i] ^= 1; //flip that bit
                        }
                        result[row] = 1;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Replaces a given equation with a new one
        /// </summary>
        /// <param name="rowNum">the index of the equation to replace</param>
        /// <param name="newEquation">the new equation</param>
        virtual public void replaceEquation(int rowNum, IEquation newEquation)
        {
            _equationSystem[rowNum] = newEquation;
        }

        /// <summary>
        /// The equivalent of matrix[rowAddedToIndex] += matrix[rowAddingIndex] (mod 2) - does this for both LHS and RHS
        /// </summary>
        /// <param name="rowAddedToIndex">the index of the row which is being added to</param>
        /// <param name="rowAddingIndex">the index of the operand</param>
        virtual public void addTo(int rowAddedToIndex, int rowAddingIndex)
        {
            _equationSystem[rowAddedToIndex].add(_equationSystem[rowAddingIndex]);
        }

        //from IEquations
        /// <summary>
        /// Performs Gaussian elimination on the equations so that they are in upper triangular with as many 1's on the diagonal as possible
        /// </summary>
        virtual public void gaussianEliminate()
        {
            //see page 2 of http://dde.binghamton.edu/filler/mct/hw/1/assignment.pdf for the algorithm; alternatively, look at the GaussianEliminationAlgorithm text file in this solution
            int n = numberOfEquations;
            int m = numberOfEquations;
            int col;
            int row = 0;

            for (col = 0; col < m; col++) //for every column [they work on a non-augmented matrix]
            {
                int maxrow = row;
                for (int k = n - 1; k >= row; k--)
                {
                    if (coefficient(k, col) == 1)
                    {
                        maxrow = k;
                        break;
                    }

                }

                if (coefficient(maxrow, col) == 1)
                {
                    //swap rows i and maxi in LHS and RHS, but do not change the value of i
                    swapTwoEquations(maxrow, row);
                    for (int u = row + 1; u < m /*-1*/; u++)
                    {
                        //Add A[u,i] * row i to row u, do this for BOTH, matrix A and RHS vector b
                        if (coefficient(u, col) == 1)
                            addTo(u, row);
                    }
                }
                else //more than one solution exists
                    row--;

                row++;
            }

            //we'll skip the checking whether there is a solution, because we know there is one

            //now, we've eliminated, but we need to make sure there are 1's on the diagonals

            for (int i = 0; i < n; i++)
            {
                if (coefficient(i, i) == 1) continue;

                //now one of the bottom rows will be 0's
                //so shove every row downwards

                for (int rownum = n - 1; rownum > i; rownum--)
                    swapTwoEquations(rownum, rownum - 1);

            }

            return;
        }
    }

    /// <summary>
    /// An example (which is currently our best) of an equation with the coefficients stored as an array of byte
    /// </summary>
    public class ByteStoredEquation : IEquation
    {
        private byte[] _LHS;
        private byte _RHS;

        public int numberOfCoefficients
        { get { return _LHS.Length; } }

        public byte coefficient(int index)
        {
            return _LHS[index];
        }

        public void add(IEquation newEquation)
        {
            for (int i = 0; i < numberOfCoefficients; i++)
            {
                _LHS[i] ^= newEquation.coefficient(i);
            }

            _RHS ^= newEquation.RHS();
        }

        public void xor(int leftMostIndex)
        {
            for (int i = leftMostIndex; i < numberOfCoefficients; i++)
                _LHS[i] ^= 1;
            _RHS ^= 1;
        }

        public byte[] ToByteArray()
        {
            return _LHS;
        }

        public byte RHS()
        {
            return _RHS;
        }

        public ByteStoredEquation(byte[] coefficients, byte rhs)
            : base()
        {
            _RHS = rhs;
            _LHS = coefficients;
        }
    }

    /// <summary>
    /// A system of equations based around the ByteStoredEquation, which should demonstrate that almost nothing needs to be done to the SystemOfEquations to instantiate it, other than to implement a constructor - although efficiency may well require rewriting other functions
    /// </summary>
    public class ByteStoredSystemOfEquations : SystemOfEquations
    {
        public ByteStoredSystemOfEquations(Board b)
            : base()
        {
            _equationSystem = new IEquation[b.boardLength - b.numberOfUnselectables];

            //we will need to scale all the indices down so that whenever there is an unselectable square, the subsequent indices are subtracted by 1

            //start populating result matrix
            for (int index = 0; index < b.boardLength; index++)
            {
                byte[] result = new byte[b.boardLength - b.numberOfUnselectables];

                Point pointConsidering = new Point(index, ref b);
                if (b.square(pointConsidering.firstDimension, pointConsidering.secondDimension) == CellState.Unselectable)
                    continue;
                //get the list of points which will flip the current point
                Point[] pointsAffected = b.getPositionsFlippedByGivenPosition(pointConsidering);
                int[] indicesAffected = new int[pointsAffected.Length];
                for (int i = 0; i < pointsAffected.Length; i++)
                {
                    int pos = pointsAffected[i].ToZeroIndexedPosition();
                    indicesAffected[i] = b.scaledIndices[pos];
                }

                foreach (int i in indicesAffected)
                    result[i] = 1;

                _equationSystem[b.scaledIndices[index]] = new ByteStoredEquation(result, Convert.ToByte((b.square(pointConsidering.firstDimension, pointConsidering.secondDimension) == CellState.Off)));
            }
        }
    }

    /// <summary>
    /// A silly class which stores equations as characters, just to demonstrate the power of the interface - note that there is no other code than the constructor
    /// </summary>
    public class CharStoredSystemOfEquations : SystemOfEquations
    {
        public CharStoredSystemOfEquations(Board b)
            : base()
        {
            _equationSystem = new IEquation[b.boardLength - b.numberOfUnselectables];

            //we will need to scale all the indices down so that whenever there is an unselectable square, the subsequent indices are subtracted by 1

            //start populating result matrix
            for (int index = 0; index < b.boardLength; index++)
            {
                byte[] result = new byte[b.boardLength - b.numberOfUnselectables];

                Point pointConsidering = new Point(index, b);
                //get the list of points which will flip the current point
                Point[] pointsAffected = b.getPositionsFlippedByGivenPosition(pointConsidering);
                int[] indicesAffected = new int[pointsAffected.Length];
                for (int i = 0; i < pointsAffected.Length; i++)
                {
                    int pos = pointsAffected[i].ToZeroIndexedPosition();
                    indicesAffected[i] = b.scaledIndices[pos];
                }

                foreach (int i in indicesAffected)
                    result[i] = 1;

                _equationSystem[b.scaledIndices[index]] = new CharStoredEquation(result, Convert.ToByte((b.square(pointConsidering.firstDimension, pointConsidering.secondDimension) == CellState.Off)));
            }
        }
    }

    /// <summary>
    /// the base class for an equation stored as characters
    /// </summary>
    public class CharStoredEquation : IEquation
    {
        private char[] _LHS;
        private char _RHS;

        private byte charToByte(char ch)
        {
            return (byte)((byte)(ch) % 2);
        }

        public int numberOfCoefficients
        { get { return _LHS.Length / 2; } }

        public byte coefficient(int index)
        {
            return charToByte(_LHS[index * 2]);
        }

        public void add(IEquation newEquation)
        {
            for (int i = 0; i < numberOfCoefficients; i++)
            {
                _LHS[i * 2] += (char)(newEquation.coefficient(i));
            }
            _RHS += (char)(newEquation.RHS());
        }

        public void xor(int leftMostIndex)
        {
            for (int i = leftMostIndex; i < _LHS.Length; i++)
                _LHS[i] += (char)1;
        }

        public byte[] ToByteArray()
        {
            byte[] result = new byte[numberOfCoefficients];
            for (int i = 0; i < numberOfCoefficients * 2; i += 2)
                result[i / 2] = charToByte(_LHS[i]);
            return result;
        }

        public byte RHS()
        {
            return charToByte(_RHS);
        }

        public CharStoredEquation(byte[] coefficients, byte rhs)
            : base()
        {
            _RHS = (char)(rhs + 60);
            _LHS = new char[coefficients.Length * 2];
            for (int i = 0; i < numberOfCoefficients; i++)
                _LHS[i * 2] = (char)(coefficients[i] + 60);
        }
    }

    public class PackedIntStoredEquation : IEquation
    {
        byte _RHS;
        public byte RHS()
        {
            return _RHS;
        }

        private int _numberOfCoefficients;
        public int numberOfCoefficients
        {
            get { return _numberOfCoefficients; }
        }

        ulong[] _LHS;
        public byte[] ToByteArray()
        {
            byte[] result = new byte[numberOfCoefficients];
            for (int i = 0; i < numberOfCoefficients; i++)
                result[i] = coefficient(i);
            return result;
        }

        private int whichByteOfUlongArr(int pos)
        {
            return (int)Math.Floor((float)pos / 64);
        }

        public byte coefficient(int pos)
        {
            //work out which element in the _LHS it's stored in
            int elem = whichByteOfUlongArr(pos);

            //now it's in the elemth position
            int howFarAlong = 63 - (pos % 64);
            return (byte)((_LHS[elem] >> howFarAlong) & 1);
        }

        public void add(PackedIntStoredEquation eq)
        {
            _RHS ^= eq.RHS();
            for (int i = 0; i < _LHS.Length; i++)
                _LHS[i] ^= eq._LHS[i];
        }

        public void add(IEquation eq)
        {
            if (eq.GetType() == this.GetType())
                add((PackedIntStoredEquation)eq);
            else
                throw new Exception("Not implemented!");
        }

        public PackedIntStoredEquation(byte[] coefficients, byte rhs)
            : base()
        {
            _RHS = rhs;
            _numberOfCoefficients = coefficients.Length;
            _LHS = new ulong[(int)Math.Ceiling((float)_numberOfCoefficients / 64)];

            for (int i = 0; i < numberOfCoefficients; i++)
            {
                int howFarAlong = 63 - (i % 64);
                _LHS[whichByteOfUlongArr(i)] ^= ((ulong)coefficients[i] << howFarAlong);
            }
        }
    }

    public class PackedIntStoredSystemOfEquations : SystemOfEquations
    {
        public PackedIntStoredSystemOfEquations(Board b)
            : base()
        {
            _equationSystem = new IEquation[b.boardLength - b.numberOfUnselectables];

            //we will need to scale all the indices down so that whenever there is an unselectable square, the subsequent indices are subtracted by 1

            //start populating result matrix
            for (int index = 0; index < b.boardLength; index++)
            {
                if (b.locationsOfUnselectables.Contains<int>(index))
                    continue;

                byte[] result = new byte[b.boardLength - b.numberOfUnselectables];

                Point pointConsidering = new Point(index, b);
                //get the list of points which will flip the current point
                Point[] pointsAffected = b.getPositionsFlippedByGivenPosition(pointConsidering);
                int[] indicesAffected = new int[pointsAffected.Length];
                for (int i = 0; i < pointsAffected.Length; i++)
                {
                    int pos = pointsAffected[i].ToZeroIndexedPosition();
                    indicesAffected[i] = b.scaledIndices[pos];
                }

                foreach (int i in indicesAffected)
                    result[i] = 1;

                _equationSystem[b.scaledIndices[index]] = new PackedIntStoredEquation(result, Convert.ToByte((b.square(pointConsidering.firstDimension, pointConsidering.secondDimension) == CellState.Off)));
            }
        }
    }

    public abstract class ParallelSystemOfEquations : SystemOfEquations
    {
        override public void gaussianEliminate()
        {
            //see page 2 of http://dde.binghamton.edu/filler/mct/hw/1/assignment.pdf for the algorithm; alternatively, look at the GaussianEliminationAlgorithm text file in this solution
            int n = numberOfEquations;
            int m = numberOfEquations;
            int col;
            int row = 0;

            for (col = 0; col < m; col++) //for every column [they work on a non-augmented matrix]
            {
                int maxrow = row;
                for (int k = n - 1; k >= row; k--)
                {
                    if (coefficient(k, col) == 1)
                    {
                        maxrow = k;
                        break;
                    }

                }

                if (coefficient(maxrow, col) == 1)
                {
                    //swap rows i and maxi in LHS and RHS, but do not change the value of i
                    swapTwoEquations(maxrow, row);
                    Parallel.For(row + 1, m, u => { if (coefficient(u, col) == 1) addTo(u, row); });
                    /*for (int u = row + 1; u < m; u++)
                    {
                        //Add A[u,i] * row i to row u, do this for BOTH, matrix A and RHS vector b
                        if (coefficient(u, col) == 1)
                            addTo(u, row);
                    }*/
                }
                else //more than one solution exists
                    row--;

                row++;
            }

            //we'll skip the checking whether there is a solution, because we know there is one

            //now, we've eliminated, but we need to make sure there are 1's on the diagonals

            for (int i = 0; i < n; i++)
            {
                if (coefficient(i, i) == 1) continue;

                //now one of the bottom rows will be 0's
                //so shove every row downwards

                for (int rownum = n - 1; rownum > i; rownum--)
                    swapTwoEquations(rownum, rownum - 1);

            }

            return;
        }
    }

    public class PackedIntStoredParallelSystemOfEquations : ParallelSystemOfEquations
    {
        public PackedIntStoredParallelSystemOfEquations(Board b)
            : base()
        {
            _equationSystem = new IEquation[b.boardLength - b.numberOfUnselectables];

            //we will need to scale all the indices down so that whenever there is an unselectable square, the subsequent indices are subtracted by 1

            //start populating result matrix
            for (int index = 0; index < b.boardLength; index++)
            {
                if (b.locationsOfUnselectables.Contains<int>(index))
                    continue;

                byte[] result = new byte[b.boardLength - b.numberOfUnselectables];

                Point pointConsidering = new Point(index, b);
                //get the list of points which will flip the current point
                Point[] pointsAffected = b.getPositionsFlippedByGivenPosition(pointConsidering);
                int[] indicesAffected = new int[pointsAffected.Length];
                for (int i = 0; i < pointsAffected.Length; i++)
                {
                    int pos = pointsAffected[i].ToZeroIndexedPosition();
                    indicesAffected[i] = b.scaledIndices[pos];
                }

                foreach (int i in indicesAffected)
                    result[i] = 1;

                _equationSystem[b.scaledIndices[index]] = new PackedIntStoredEquation(result, Convert.ToByte((b.square(pointConsidering.firstDimension, pointConsidering.secondDimension) == CellState.Off)));
            }
        }
    }

    /// <summary>
    /// The class used to represent the game board.
    /// </summary>
    public class Board
    {

        private int level;
        private int _firstIndexDimension;
        private int[] _unselectables;
        private int[] _scaledIndices;

        CellState[,] _internalBoard;

        public int firstIndexDimension
        {
            get { return this._firstIndexDimension; }
        }
        public int secondIndexDimension
        {
            get { return boardLength / firstIndexDimension; }
        }

        public int boardLength
        {
            get { return this._internalBoard.Length; }
        }

        private void setMainVariables(string boardVariables)
        {
            string choppedBoardInit = boardVariables.Substring(boardVariables.IndexOf("\"") + 1);
            choppedBoardInit = choppedBoardInit.Remove(choppedBoardInit.IndexOf("\""));
            string[] boardState = choppedBoardInit.Split(',');
            _internalBoard = new CellState[boardState.Length, boardState[0].Length];
            _firstIndexDimension = boardState.Length;

            for (int i = 0; i < boardState.Length; i++)
                for (int j = 0; j < boardState[0].Length; j++)
                    switch (boardState[i][j])
                    {
                        case '0': _internalBoard[i, j] = CellState.On; break;
                        case '1': _internalBoard[i, j] = CellState.Off; break;
                        default: _internalBoard[i, j] = CellState.Unselectable; break;
                    }

            string levelStr = boardVariables.Substring(boardVariables.LastIndexOf(" ") + 1);
            level = Convert.ToInt32(levelStr.Remove(levelStr.Length - 1));


            #region unselectablestuff
            //populate unselectables
            _unselectables = positionsOnBoardWhichMustBeZero();
            int[] totalNumberOfUnselectablesEncountered = new int[this.boardLength];

            //populate array of totalNumberOfUns...
            for (int index = 0; index < this.boardLength; index++)
            {
                Point pointConsidering = new Point(index, this);
                if (square(pointConsidering.firstDimension, pointConsidering.secondDimension) == CellState.Unselectable)
                {
                    if (index == 0)
                        totalNumberOfUnselectablesEncountered[index] = 1;
                    else
                        totalNumberOfUnselectablesEncountered[index] = totalNumberOfUnselectablesEncountered[index - 1] + 1;
                    continue;
                }

                if (index != 0) totalNumberOfUnselectablesEncountered[index] = totalNumberOfUnselectablesEncountered[index - 1];
            }

            _scaledIndices = new int[this.boardLength];
            _scaledIndices[0] = totalNumberOfUnselectablesEncountered[0];
            for (int i = 1; i < this.boardLength; i++)
                _scaledIndices[i] = i - totalNumberOfUnselectablesEncountered[i];
            #endregion
        }

        /// <summary>
        /// For a given position, returns an array of all the points which would be flipped if the user clicked on that position.
        /// </summary>
        /// <param name="pointClicked">the point the user clicked</param>
        /// <returns>the array of all the points flipped</returns>
        public Point[] getPositionsFlippedByGivenPosition(Point pointClicked)
        {
            if (square(pointClicked.firstDimension, pointClicked.secondDimension) == CellState.Unselectable)
                return new Point[0];

            int indexOfMaximumFirstDimension = pointClicked.firstDimension;
            while ((indexOfMaximumFirstDimension < firstIndexDimension) && (square(indexOfMaximumFirstDimension, pointClicked.secondDimension) != CellState.Unselectable))
                indexOfMaximumFirstDimension++;
            // if (indexOfMaximumFirstDimension != inputBoard.firstIndexDimension)
            indexOfMaximumFirstDimension--;

            int indexOfMinimumFirstDimension = pointClicked.firstDimension;
            while ((indexOfMinimumFirstDimension >= 0) && (square(indexOfMinimumFirstDimension, pointClicked.secondDimension) != CellState.Unselectable))
                indexOfMinimumFirstDimension--;
            indexOfMinimumFirstDimension++;

            int indexOfMinimumSecondDimension = pointClicked.secondDimension;
            while ((indexOfMinimumSecondDimension >= 0) && (square(pointClicked.firstDimension, indexOfMinimumSecondDimension) != CellState.Unselectable))
                indexOfMinimumSecondDimension--;
            indexOfMinimumSecondDimension++;

            int indexOfMaximumSecondDimension = pointClicked.secondDimension;
            while ((indexOfMaximumSecondDimension < secondIndexDimension) && (square(pointClicked.firstDimension, indexOfMaximumSecondDimension) != CellState.Unselectable))
                indexOfMaximumSecondDimension++;
            // if (indexOfMaximumSecondDimension != inputBoard.secondIndexDimension)
            indexOfMaximumSecondDimension--;

            Point[] result = new Point[(indexOfMaximumSecondDimension - indexOfMinimumSecondDimension) + (indexOfMaximumFirstDimension - indexOfMinimumFirstDimension) + 1];
            int currResultPtr = 0;

            for (int i = indexOfMinimumFirstDimension; i <= indexOfMaximumFirstDimension; i++)
            {
                result[currResultPtr] = new Point(i, pointClicked.secondDimension, this);
                currResultPtr++;
            }
            for (int j = indexOfMinimumSecondDimension; j <= indexOfMaximumSecondDimension; j++)
            {
                if (j == pointClicked.secondDimension) continue;
                result[currResultPtr] = new Point(pointClicked.firstDimension, j, this);
                currResultPtr++;
            }

            return result;
        }


        /// <summary>
        /// Constructor for the Board
        /// </summary>
        /// <param name="boardVariables">Either a filepath or the source of the hacker.org page</param>
        public Board(string boardVariables)
            : base()
        {
            if (boardVariables[boardVariables.Length - 4] == '.')
            //it's a filepath
            {
                string[] fromFile = new string[0];

                fromFile = File.ReadAllLines(boardVariables.ToString());

                //file format was the 1's and 0's of the board with commas inserted, then a newline,
                setMainVariables("<script>var boardinit = \"" + fromFile[0] + "\";var level = " + fromFile[1] + ";");
            }
            else
                setMainVariables(boardVariables);
        }

        public int[] locationsOfUnselectables
        {
            get { return _unselectables; }
        }

        public int numberOfUnselectables
        {
            get { return locationsOfUnselectables.Length; }
        }

        public int[] scaledIndices
        {
            get { return _scaledIndices; }
        }

        public void writeOutBoard()
        {
            for (int i = 0; i < firstIndexDimension; i++)
            {
                for (int j = 0; j < secondIndexDimension; j++)
                {
                    switch (_internalBoard[i, j])
                    {
                        case CellState.Off: Console.Write('.'); break;
                        case CellState.On: Console.Write('0'); break;
                        case CellState.Unselectable: Console.Write('*'); break;
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        /// <summary>
        /// changes the board as if we had clicked on the given position
        /// </summary>
        /// <param name="position">the position we clicked on, zero-indexed</param>
        public void makeMove(int position)
        {
            Point where = new Point(position, this);
            Point[] flipped = getPositionsFlippedByGivenPosition(where);
            foreach (Point p in flipped)
            {
                this._internalBoard[p.firstDimension, p.secondDimension] ^= (CellState)1;
            }
        }

        /// <summary>
        /// Returns the indices of all unselectable grid locations
        /// </summary>
        /// <returns>a list of {a1,a2...} where a1 is the first unselectable index, etc</returns>
        public int[] positionsOnBoardWhichMustBeZero()
        {
            List<int> result = new List<int>();
            for (int i = 0; i < firstIndexDimension; i++)
                for (int j = 0; j < secondIndexDimension; j++)
                    if (square(i, j) == CellState.Unselectable)
                        result.Add(new Point(i, j, this).ToZeroIndexedPosition());
            return result.ToArray();
        }


        public void setBoardToFile(string filePath)
        {
            StringBuilder boardtxt = new StringBuilder(boardLength);
            //file format is the 1's and 0's of the board, 1 = off, 0 = on, 2 = unselectable
            for (int x = 0; x < firstIndexDimension; x++)
            {
                for (int y = 0; y < secondIndexDimension; y++)
                {
                    char ch = ' ';
                    switch (square(x, y))
                    {
                        case CellState.Off:
                            ch = '1';
                            break;
                        case CellState.On:
                            ch = '0';
                            break;
                        case CellState.Unselectable:
                            ch = '2';
                            break;
                    }
                    boardtxt.Append(ch);
                }
                boardtxt.Append(',');
            }
            boardtxt.Remove(boardtxt.Length - 1, 1);
            File.WriteAllText(filePath, boardtxt.ToString());
        }

        public void makeAllMoves(byte[] answer)
        {
            for (int i = 0; i < answer.Length; i++)
                if (answer[i] == 1)
                    makeMove(i);
        }

        public bool checkSolution(byte[] answer)
        {
            makeAllMoves(answer);
            for (int x = 0; x < firstIndexDimension; x++)
                for (int y = 0; y < secondIndexDimension; y++)
                    if (_internalBoard[x, y] == CellState.Off)
                        return false;
            return true;
        }

        public CellState square(int x, int y)
        {
            return _internalBoard[x, y];
        }

    }

    /// <summary>
    /// Represents a position on a Board.
    /// </summary>
    public class Point
    {
        public int firstDimension;
        public int secondDimension;
        public int dimensionOfBoard;

        public Point(int x, int y, ref Board b)
        {
            firstDimension = x;
            secondDimension = y;
            dimensionOfBoard = b.secondIndexDimension;
        }

        public Point(int x, int y, Board b)
        {
            firstDimension = x;
            secondDimension = y;
            dimensionOfBoard = b.secondIndexDimension;
        }

        public Point(int zeroIndex, ref Board b)
        {
            dimensionOfBoard = b.secondIndexDimension;
            int remainder;
            int quotient = Math.DivRem(zeroIndex, b.secondIndexDimension, out remainder);
            firstDimension = quotient;
            secondDimension = remainder;
        }

        public Point(int zeroIndex, Board b)
        {
            dimensionOfBoard = b.secondIndexDimension;
            int remainder;
            int quotient = Math.DivRem(zeroIndex, b.secondIndexDimension, out remainder);
            firstDimension = quotient;
            secondDimension = remainder;
        }

        public int ToZeroIndexedPosition()
        {
            return firstDimension * dimensionOfBoard + secondDimension;
        }
    }

    public class LevelRetriever
    {
        bool workingFromFile = false;

        /// <summary>
        /// SPW used instead of password, as spw=blah
        /// </summary>
        const string spw = "1c3c78cfd6370722ba30f4c043f0ea78";

        /// <summary>
        /// Username, as user=blah
        /// </summary>
        const string username = "laz0r";

        /// <summary>
        /// URL of the page we are submitting to
        /// </summary>
        const string crossflipSubmitPage = "http://www.hacker.org/cross/index.php";

        /// <summary>
        /// Performs an HTTP POST request to the given page
        /// </summary>
        /// <param name="location">the URL to POST to</param>
        /// <param name="data">the query string to supply (eg. "user=uname&password=foobar")</param>
        /// <returns>the page source returned from the server</returns>
        static string GetPage(string location, string data)
        {
            try
            {
                WebRequest req = WebRequest.Create(location);
                req.Method = "POST";
                req.ContentLength = data.Length;
                req.ContentType = "application/x-www-form-urlencoded";
                Stream dataStream = req.GetRequestStream();
                dataStream.Write(Encoding.ASCII.GetBytes(data), 0, data.Length);
                dataStream.Close();
                WebResponse resp = req.GetResponse();
                return new StreamReader(resp.GetResponseStream()).ReadToEnd();
            }
            catch (WebException)
            {
                return null;
            }
            return null;
        }

        /// <summary>
        /// Retrieves the game board for a given level, from the server
        /// </summary>
        /// <param name="level">the level number (eg. supplying 1 gives the first puzzle)</param>
        /// <returns>the Board structure containing the board</returns>
        public Board getBoard(int level)
        {
            if (workingFromFile)
                return new Board(level.ToString() + ".txt");
            else
                return getBoard(level, username, spw);
        }

        /// <summary>
        /// Retrieves the game board for a given level, from the server
        /// </summary>
        /// <param name="level">the level number</param>
        /// <param name="username">username to be used</param>
        /// <param name="spw">SPW of the user specified</param>
        /// <returns></returns>
        static Board getBoard(int level, string username, string spw)
        {
            string rawSource = GetPage(crossflipSubmitPage, "name=" + username + "&spw=" + spw + "&go=Go+To+Level&gotolevel=" + level.ToString());
            Console.WriteLine("Page retrieved!");
            string endChoppedOff = rawSource.Substring(0, rawSource.IndexOf("</script>"));
            string boardData = endChoppedOff.Substring(endChoppedOff.IndexOf("<script>") + 8);
            return new Board(boardData);
        }

        /// <summary>
        /// Submits a solution once found
        /// </summary>
        /// <param name="answer">the result, in the form of a list of {a1,a2...} where ai = 1 iff we have to click position i</param>
        /// <param name="level">the level of the puzzle</param>
        /// <returns>the page source received from the server</returns>
        public string submitSolution(byte[] answer, int level)
        {
            if (workingFromFile)
                return getBoard(level).checkSolution(answer).ToString();
            else
            {
                StringBuilder ans = new StringBuilder("go=Go+To+Level&gotolevel=" + level.ToString());
                ans.Append("&lvl=");
                ans.Append(level.ToString());
                ans.Append("&sol=");
                for (int i = 0; i < answer.Length; i++)
                    ans.Append(answer[i].ToString());
                ans.Append("&name=");
                ans.Append(username);
                ans.Append("&spw=");
                ans.Append(spw);

                return GetPage(crossflipSubmitPage, ans.ToString());
            }
        }

        /// <summary>
        /// Sets up a new LevelRetriever
        /// </summary>
        /// <param name="fromFile">true iff we wish to access levels stored in .txt files</param>
        public LevelRetriever(bool fromFile)
            : base()
        {
            workingFromFile = fromFile;
        }

        /// <summary>
        /// Checks whether the server's response to our submission marks the solution as correct.
        /// </summary>
        /// <param name="response">the string the server sent</param>
        /// <returns>true iff the response was correct</returns>
        public static bool responseWasCorrect(string response)
        {
            return (response.StartsWith("<HTML") || response.StartsWith("<html") || response.Equals("True"));
        }

    }

    class Program
    {
        static string matrixToString(byte[,] matrix, int firstMatrixDimension)
        {
            StringBuilder res = new StringBuilder();
            for (int row = 0; row < firstMatrixDimension; row++)
            {
                for (int col = 0; col < matrix.Length / firstMatrixDimension; col++)
                {
                    res.Append(matrix[row, col].ToString());
                }
                res.Append('\n');
            }
            return res.ToString();
        }

        /// <summary>
        /// creates a complete byte array, adding back in the variables we took out because they were unchangeable
        /// </summary>
        /// <param name="solution">the values to be assigned to the variables which are actually variable</param>
        /// <param name="board">the board we're playing on</param>
        /// <returns>a complete solution, to be passed to submitSolution</returns>
        static byte[] addInRemovedIndices(byte[] solution, Board board)
        {
            byte[] result = new byte[board.boardLength];
            int currVariableCounter = 0;
            for (int i = 0; i < board.boardLength; i++)
            {
                if (board.locationsOfUnselectables.Contains<int>(i))
                    result[i] = 0;
                else
                {
                    result[i] = solution[currVariableCounter];
                    currVariableCounter++;
                }
            }
            return result;
        }

        static byte[] solveLevel(Board levelBoard)
        {

            Stopwatch st = new Stopwatch();
            st.Start();

            PackedIntStoredParallelSystemOfEquations system = new PackedIntStoredParallelSystemOfEquations(levelBoard);
            system.gaussianEliminate();

            //now we need to perform and return LinearSolve[matrix, indicesRequired, Modulus->2]
            byte[] solved = system.backSubstitute();

            //add back in the indices we've taken out because they're unreachable
            byte[] finalResult = addInRemovedIndices(solved, levelBoard);

            st.Stop();

            Console.Write(st.ElapsedMilliseconds);
            Console.WriteLine(".");
            Console.WriteLine();
            return finalResult;
        }
#if doingStuffFromFile
        static bool workFromFile = true;
#else
        static bool workFromFile = false;
#endif

        /// <summary>
        /// Runs a benchmark on solving a given level
        /// </summary>
        /// <param name="level">the level we want to test</param>
        /// <returns>the number of milliseconds taken per solution of that level</returns>
        static float benchmark(int level)
        {
            LevelRetriever lev = new LevelRetriever(workFromFile);
            Board levelBoard = lev.getBoard(level);

            Stopwatch st = new Stopwatch();
            st.Start();
            for (int i = 0; i < 10; i++)
            {
                solveLevel(levelBoard);
            }
            st.Stop();
            return st.ElapsedMilliseconds / 10;
        }

        /// <summary>
        /// Benchmarks the solution method for solving level 120.
        /// </summary>
        /// <returns></returns>
        static float benchmark()
        {
            return benchmark(300);
        }

        /// <summary>
        /// Requests user input as to the level to start calculating with
        /// </summary>
        /// <param name="currentbestlevel">the current highest level we can access on this puzzle</param>
        /// <returns>the level the user wishes to solve</returns>
        static int getStartingLevel(int currentbestlevel)
        {
            bool accepted = false;
            int level = 0;

            do
            {
                accepted = false;
                Console.Write("Which level do you want to solve from? I think {0} is the current... >> ", currentbestlevel);
                try
                {
                    string st = Console.ReadLine();
                    if (String.IsNullOrEmpty(st))
                        level = currentbestlevel;
                    else
                        level = Convert.ToInt32(st);
                    accepted = true;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Please enter an integer...");
                    accepted = false;
                }
            }
            while (accepted == false);
            return level;
        }

        static void Main(string[] args)
        {
            Console.Write("Please enter \"b\" for benchmark, \"s\" for single level solving, or any other for solving continually >>");
            string whichAction = Console.ReadLine();

            if (whichAction.StartsWith("b"))
                Console.WriteLine(benchmark());

            else if (whichAction.StartsWith("s"))
            {
                Console.Write("Please enter the level to get >> ");
                int level = Convert.ToInt32(Console.ReadLine());

                LevelRetriever levRet = new LevelRetriever(workFromFile);
                Board levelBoard = levRet.getBoard(level);
                byte[] finalResult = solveLevel(levelBoard);
                string response = levRet.submitSolution(finalResult, level);
                if (LevelRetriever.responseWasCorrect(response))
                    Console.WriteLine("Submitted successfully.");
                else
                    Console.WriteLine(response);
            }

            else
            {
                int level = getStartingLevel(572);

                for (int i = level; i <= 643; i++)
                {
                    try
                    {
                        Console.WriteLine("---- Starting level {0} ----", i);
                        LevelRetriever levRet = new LevelRetriever(workFromFile);
                        Board b = levRet.getBoard(i);
                        byte[] finalResult = solveLevel(b);
                        bool submitted = false;
                        string response = "";

                        do
                        {
                            submitted = true;
                            try
                            {
                                response = levRet.submitSolution(finalResult, i);
                            }
                            catch
                            {
                                submitted = false;
                            }
                        } while (submitted == false);

                        if (LevelRetriever.responseWasCorrect(response))
                            Console.WriteLine("Submitted {0} successfully.", i);
                        else
                        {
                            Console.WriteLine("Error at level {0}:", i);
                            Console.WriteLine(response);
                        }
                    }
                    catch
                    {
                        i--;
                    }
                }
            }

            Console.WriteLine("Finished!");
            Console.ReadLine();
        }

    }
}
