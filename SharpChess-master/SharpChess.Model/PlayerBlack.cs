// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerBlack.cs" company="SharpChess.com">
//   SharpChess.com
// </copyright>
// <summary>
//   The player black.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#region License

// SharpChess
// Copyright (C) 2012 SharpChess.com
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChess.Model
{
    /// <summary>
    /// The player black.
    /// </summary>
    public class PlayerBlack : Player
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerBlack"/> class.
        /// </summary>
        public PlayerBlack()
        {
            this.Colour = PlayerColourNames.Black;
            this.Intellegence = PlayerIntellegenceNames.Computer;

            this.SetPiecesAtStartingPositions();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets PawnAttackLeftOffset.
        /// </summary>
        public override int PawnAttackLeftOffset
        {
            get
            {
                return -17;
            }
        }

        /// <summary>
        /// Gets PawnAttackRightOffset.
        /// </summary>
        public override int PawnAttackRightOffset
        {
            get
            {
                return -15;
            }
        }

        /// <summary>
        /// Gets PawnForwardOffset.
        /// </summary>
        public override int PawnForwardOffset
        {
            get
            {
                return -16;
            }
        }

        #endregion

        #region Methods
        private void SetChess960Positions()
        {
            Console.WriteLine("Starting Chess960 piece placement...");

            Random random = new Random();
            List<int> availablePositions = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };

            if (availablePositions == null || availablePositions.Count == 0)
            {
                throw new Exception("Chess960 setup failed: Available positions list is null or empty.");
            }

            // Ensure bishops are placed on opposite-colored squares
            int bishop1 = availablePositions[random.Next(0, 4) * 2]; // Even index
            availablePositions.Remove(bishop1);
            int bishop2 = availablePositions[random.Next(0, 4) * 2 + 1]; // Odd index
            availablePositions.Remove(bishop2);

            // Place the queen
            int queen = availablePositions[random.Next(availablePositions.Count)];
            availablePositions.Remove(queen);

            // Place knights
            int knight1 = availablePositions[random.Next(availablePositions.Count)];
            availablePositions.Remove(knight1);
            int knight2 = availablePositions[random.Next(availablePositions.Count)];
            availablePositions.Remove(knight2);

            // Place rooks and king (king must be between rooks)
            availablePositions = availablePositions.OrderBy(x => random.Next()).ToList();
            int rook1 = availablePositions[0];
            int king = availablePositions[1];
            int rook2 = availablePositions[2];

            Console.WriteLine($"Chess960 positions -> King: {king}, Rooks: {rook1}, {rook2}, Bishops: {bishop1}, {bishop2}, Queen: {queen}");

            this.Pieces.Add(this.King = new Piece(Piece.PieceNames.King, this, king, 0, Piece.PieceIdentifierCodes.WhiteKing));
            this.Pieces.Add(new Piece(Piece.PieceNames.Queen, this, queen, 0, Piece.PieceIdentifierCodes.WhiteQueen));
            this.Pieces.Add(new Piece(Piece.PieceNames.Rook, this, rook1, 0, Piece.PieceIdentifierCodes.WhiteQueensRook));
            this.Pieces.Add(new Piece(Piece.PieceNames.Rook, this, rook2, 0, Piece.PieceIdentifierCodes.WhiteKingsRook));
            this.Pieces.Add(new Piece(Piece.PieceNames.Bishop, this, bishop1, 0, Piece.PieceIdentifierCodes.WhiteQueensBishop));
            this.Pieces.Add(new Piece(Piece.PieceNames.Bishop, this, bishop2, 0, Piece.PieceIdentifierCodes.WhiteKingsBishop));
            this.Pieces.Add(new Piece(Piece.PieceNames.Knight, this, knight1, 0, Piece.PieceIdentifierCodes.WhiteQueensKnight));
            this.Pieces.Add(new Piece(Piece.PieceNames.Knight, this, knight2, 0, Piece.PieceIdentifierCodes.WhiteKingsKnight));

            // Pawns remain unchanged
            for (int i = 0; i < 8; i++)
            {
                this.Pieces.Add(new Piece(Piece.PieceNames.Pawn, this, i, 1, (Piece.PieceIdentifierCodes)((int)Piece.PieceIdentifierCodes.WhitePawn1 + i)));
            }

            Console.WriteLine("Chess960 Setup Complete.");
        }


        public override void SetPiecesAtStartingPositions()
        {
            if (Game.CurrentMode == Game.ChessMode.Chess960)
            {
                SetChess960Positions();
            }
            else
            {
                base.SetPiecesAtStartingPositions(); // Traditional setup
            }
            Console.WriteLine($"[DEBUG] Setting up pieces for {this.Colour}");

            this.Pieces.Add(this.King = new Piece(Piece.PieceNames.King, this, 4, 0, Piece.PieceIdentifierCodes.WhiteKing));
            if (this.King == null)
            {
                throw new Exception($"[ERROR] King was NOT assigned for {this.Colour}!");
            }

            if (Game.CurrentMode == Game.ChessMode.Chess960)
            {
                SetChess960Positions();  // 🔹 Make sure this method exists
            }
            else
            {
                Console.WriteLine($"Setting up traditional pieces for {this.Colour}...");
                this.Pieces.Add(this.King = new Piece(Piece.PieceNames.King, this, 4, 0, Piece.PieceIdentifierCodes.WhiteKing));
                this.Pieces.Add(new Piece(Piece.PieceNames.Queen, this, 3, 0, Piece.PieceIdentifierCodes.WhiteQueen));
                this.Pieces.Add(new Piece(Piece.PieceNames.Rook, this, 0, 0, Piece.PieceIdentifierCodes.WhiteQueensRook));
                this.Pieces.Add(new Piece(Piece.PieceNames.Rook, this, 7, 0, Piece.PieceIdentifierCodes.WhiteKingsRook));
                this.Pieces.Add(new Piece(Piece.PieceNames.Bishop, this, 2, 0, Piece.PieceIdentifierCodes.WhiteQueensBishop));
                this.Pieces.Add(new Piece(Piece.PieceNames.Bishop, this, 5, 0, Piece.PieceIdentifierCodes.WhiteKingsBishop));
                this.Pieces.Add(new Piece(Piece.PieceNames.Knight, this, 1, 0, Piece.PieceIdentifierCodes.WhiteQueensKnight));
                this.Pieces.Add(new Piece(Piece.PieceNames.Knight, this, 6, 0, Piece.PieceIdentifierCodes.WhiteKingsKnight));

                for (int i = 0; i < 8; i++)
                {
                    this.Pieces.Add(new Piece(Piece.PieceNames.Pawn, this, i, 1, (Piece.PieceIdentifierCodes)((int)Piece.PieceIdentifierCodes.WhitePawn1 + i)));
                }
            }

        }

        #endregion
    }
}