// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Player.cs" company="SharpChess.com">
//   SharpChess.com
// </copyright>
// <summary>
//   The player.
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

namespace SharpChess.Model
{
    #region Using

    using SharpChess.Model.AI;
    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// The player.
    /// </summary>
    public abstract class Player
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "Player" /> class.
        /// </summary>
        protected Player()
        {
            this.Clock = new PlayerClock();
            this.MaterialCount = 7;
            this.PawnCountInPlay = 8;
            this.Pieces = new Pieces();
            this._PieceTypes = new List<Piece.PieceNames>();
            this.CapturedEnemyPieces = new Pieces();
            this.Brain = new Brain(this);
        }

        #endregion

        #region Enums

        /// <summary>
        /// Player Colour Names
        /// </summary>
        public enum PlayerColourNames
        {
            /// <summary>
            ///   White colour.
            /// </summary>
            White,

            /// <summary>
            ///   Black colour.
            /// </summary>
            Black
        }

        /// <summary>
        /// Player intellegence: Human or Computer.
        /// </summary>
        public enum PlayerIntellegenceNames
        {
            /// <summary>
            ///   Human player.
            /// </summary>
            Human,

            /// <summary>
            ///   Computer player.
            /// </summary>
            Computer
        }

        /// <summary>
        /// Player status: in-check, statemate etc...
        /// </summary>
        public enum PlayerStatusNames
        {
            /// <summary>
            ///   Player game in progress.
            /// </summary>
            Normal,

            /// <summary>
            ///   Player is in check.
            /// </summary>
            InCheck,

            /// <summary>
            ///   Player is in stalemate.
            /// </summary>
            InStalemate,

            /// <summary>
            ///   Player is in check mate.
            /// </summary>
            InCheckMate
        }

        #endregion


        private List<Piece.PieceNames> _PieceTypes;


        #region Public Properties

        /// <summary>
        ///   Gets the player's chess brain. Contains all computer AI chess logic.
        /// </summary>
        public Brain Brain { get; private set; }

        /// <summary>
        ///   Gets a value indicating whether the player can claim a fifty-nove draw.
        /// </summary>
        public bool CanClaimFiftyMoveDraw
        {
            get
            {
                return Game.MoveHistory.Count > 0
                           ? Game.MoveHistory.Last.IsFiftyMoveDraw
                           : Game.FiftyMoveDrawBase >= 100;
            }
        }

        /// <summary>
        ///   Gets a value indicating whether CanClaimInsufficientMaterialDraw.
        /// </summary>
        public bool CanClaimInsufficientMaterialDraw
        {
            get
            {
                // Return true if K vs K, K vs K+B, K vs K+N
                if (Game.PlayerWhite.Pieces.Count > 2 || Game.PlayerBlack.Pieces.Count > 2)
                {
                    return false;
                }

                if (Game.PlayerWhite.Pieces.Count == 2 && Game.PlayerBlack.Pieces.Count == 2)
                {
                    return false;
                }

                if (Game.PlayerWhite.Pieces.Count == 1 && Game.PlayerBlack.Pieces.Count == 1)
                {
                    return true;
                }

                Player playerTwoPieces = Game.PlayerWhite.Pieces.Count == 2 ? Game.PlayerWhite : Game.PlayerBlack;
                Piece pieceNotKing = playerTwoPieces.Pieces.Item(0).Name == Piece.PieceNames.King
                                         ? playerTwoPieces.Pieces.Item(1)
                                         : playerTwoPieces.Pieces.Item(0);

                switch (pieceNotKing.Name)
                {
                    case Piece.PieceNames.Bishop:
                    case Piece.PieceNames.Knight:
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        ///   Gets a value indicating whether CanClaimThreeMoveRepetitionDraw.
        /// </summary>
        public bool CanClaimThreeMoveRepetitionDraw
        {
            get
            {
                return this.CanClaimMoveRepetitionDraw(3);
            }
        }

        /// <summary>
        ///   Gets a value indicating whether the player has any legal moves.
        /// </summary>
        public bool CanMove
        {
            get
            {
                foreach (Piece piece in this.Pieces)
                {
                    Moves moves = new Moves();
                    piece.GenerateLegalMoves(moves);
                    if (moves.Count > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        ///   Gets a list of captured enemy pieces.
        /// </summary>
        public Pieces CapturedEnemyPieces { get; private set; }

        /// <summary>
        ///   Gets the sum of captured piece basic value.
        /// </summary>
        public int CapturedEnemyPiecesTotalBasicValue
        {
            get
            {
                int intValue = 0;
                foreach (Piece piece in this.CapturedEnemyPieces)
                {
                    intValue += piece.BasicValue;
                }

                return intValue;
            }
        }

        /// <summary>
        ///   Gets the player's clock.
        /// </summary>
        public PlayerClock Clock { get; private set; }

        /// <summary>
        ///   Gets or sets the player's colour.
        /// </summary>
        public PlayerColourNames Colour { get; protected set; }

        /// <summary>
        ///   Gets or sets a value indicating whether the player has castled yet.
        /// </summary>
        public bool HasCastled { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether the player's intellegence is human or computer.
        /// </summary>
        public PlayerIntellegenceNames Intellegence { get; set; }

        /// <summary>
        ///   Gets a value indicating whether the player is in check.
        /// </summary>
        public bool IsInCheck
        {
            get
            {
                return HashTableCheck.QueryandCachePlayerInCheckStatusForPosition(
                    Board.HashCodeA, Board.HashCodeB, this);
            }
        }

        /// <summary>
        ///   Gets a value indicating whether the player is in checkmate.
        /// </summary>
        public bool IsInCheckMate
        {
            get
            {
                if (!this.IsInCheck)
                {
                    return false;
                }

                // TODO consider also caching checkmate values
                Moves moves = new Moves();
                this.GenerateLegalMoves(moves);
                return moves.Count == 0;
            }
        }

        /// <summary>
        ///   Gets or sets the player's King piece.
        /// </summary>
        public Piece King { get; protected set; }

        /// <summary>
        ///   Gets a counter fo the number of material (non-Pawn) pieces on the board.
        /// </summary>
        public int MaterialCount { get; private set; }

        /// <summary>
        ///   Gets the opposing player.
        /// </summary>
        public Player OpposingPlayer
        {
            get
            {
                return this.Colour == PlayerColourNames.White ? Game.PlayerBlack : Game.PlayerWhite;
            }
        }

        /// <summary>
        ///   Gets the ordinal square offset for a pawn attack to the left.
        /// </summary>
        public abstract int PawnAttackLeftOffset { get; }

        /// <summary>
        ///   Gets the ordinal square offset for a pawn attack to the right.
        /// </summary>
        public abstract int PawnAttackRightOffset { get; }

        /// <summary>
        ///   Gets or sets the number of pawns in play.
        /// </summary>
        public int PawnCountInPlay { get; set; }

        /// <summary>
        ///   Gets the ordinal square offset for a pawn advancing one square forward.
        /// </summary>
        public abstract int PawnForwardOffset { get; }

        /// <summary>
        ///   Gets all the player's pieces in play.
        /// </summary>
        public Pieces Pieces { get; private set; }


        /// get a list of all piecetypes
        public List<Piece.PieceNames> PieceTypes()
        {
            this._PieceTypes.Clear();

            foreach (Piece piece in this.Pieces)
            {
                if (!this._PieceTypes.Contains(piece.Name))
                    _PieceTypes.Add(piece.Name);
            }
            return this._PieceTypes;
        }

        /// <summary>
        ///   Gets positional points for all the player's pieces in play, including matieral value.
        ///   Forms the basis of the Positon Evaluation function http://chessprogramming.wikispaces.com/Evaluation
        /// </summary>
        public int Points
        {
            get
            {
                return this.PositionPoints + this.TotalPieceValue;
            }
        }

        /// <summary>
        ///   Gets the evaulation of the player's position, excluding material piece value.
        /// </summary>
        public int PositionPoints
        {
            get
            {
                int intPoints = 0;

                // Start with pawn points.
                intPoints += this.PositionPointsJustPawns;

                int intBishopCount = 0;
                int intRookCount = 0;
                for (int intIndex = this.Pieces.Count - 1; intIndex >= 0; intIndex--)
                {
                    Piece piece = this.Pieces.Item(intIndex);

                    // Don't include pawns again!
                    if (piece.Name != Piece.PieceNames.Pawn)
                    {
                        intPoints += piece.PositionalPoints;

                        // Count the number of bishops and rooks for later use.
                        switch (piece.Name)
                        {
                            case Piece.PieceNames.Bishop:
                                intBishopCount++;
                                break;

                            case Piece.PieceNames.Rook:
                                intRookCount++;
                                break;
                        }
                    }
                }

                // Big bonus for having bishop pair.
                if (intBishopCount >= 2)
                {
                    intPoints += 500;
                }

                // Bonus for having rook pair.
                if (intRookCount >= 2)
                {
                    intPoints += 100;
                }

                // Multiple attack bonus
                // for (intIndex=this.OtherPlayer.m_colPieces.Count-1; intIndex>=0; intIndex--)
                // {
                // piece = this.OtherPlayer.m_colPieces.Item(intIndex);
                // intPoints += m_aintAttackBonus[piece.Square.NoOfAttacksBy(this)];
                // }


                // Factor in human 3 move repition draw condition
                // If this player is "human" then a draw if scored high, else a draw is scored low.
                // TODO 3MR not working, investigate.i.e. computer doesn't avoid move.
                if (Game.MoveHistory.Count > 0 && Game.MoveHistory.Last.IsThreeMoveRepetition)
                {
                    intPoints += this.Intellegence == PlayerIntellegenceNames.Human ? 1000000000 : 0;
                }

                if (this.HasCastled)
                {
                    intPoints += 117;
                }
                else
                {
                    if (this.King.HasMoved)
                    {
                        intPoints -= 247;
                    }
                    else
                    {
                        Piece pieceRook;
                        pieceRook = this.Colour == PlayerColourNames.White ? Board.GetPiece(7, 0) : Board.GetPiece(7, 7);
                        if (pieceRook == null || pieceRook.Name != Piece.PieceNames.Rook
                            || pieceRook.Player.Colour != this.Colour || pieceRook.HasMoved)
                        {
                            intPoints -= 107;
                        }

                        pieceRook = this.Colour == PlayerColourNames.White ? Board.GetPiece(0, 0) : Board.GetPiece(0, 7);
                        if (pieceRook == null || pieceRook.Name != Piece.PieceNames.Rook
                            || pieceRook.Player.Colour != this.Colour || pieceRook.HasMoved)
                        {
                            intPoints -= 107;
                        }
                    }
                }

                if (this.IsInCheck)
                {
                    if (this.IsInCheckMate)
                    {
                        intPoints -= 999999999;
                    }
                }

                return intPoints;
            }
        }

        /// <summary>
        ///   Gets the evaulation of the player's position, excluding material piece value, just for pawns.
        /// </summary>
        private int PositionPointsJustPawns
        {
            get
            {
                // Check is pawn position if cached in Hash Table.
                int pawnPositionScore = HashTablePawn.ProbeHash(Board.HashCodeA, Board.HashCodeB, this.Colour);

                if (pawnPositionScore == HashTablePawn.NotFoundInHashTable)
                {
                    // Not cached, so calculate it.
                    pawnPositionScore = 0;
                    for (int intIndex = this.Pieces.Count - 1; intIndex >= 0; intIndex--)
                    {
                        Piece piece = this.Pieces.Item(intIndex);
                        if (piece.Name == Piece.PieceNames.Pawn)
                        {
                            pawnPositionScore += piece.PositionalPoints;
                        }
                    }
                }

                // Record positional score in pawn hash table.
                HashTablePawn.RecordHash(Board.HashCodeA, Board.HashCodeB, pawnPositionScore, this.Colour);

                return pawnPositionScore;
            }
        }

        /// <summary>
        ///   Gets Score for the player's current position, centered around zero. A positive score indicate this player is ahead.
        ///   This is the SharpChess Evaluation function http://chessprogramming.wikispaces.com/Evaluation
        /// </summary>
        public int Score
        {
            get
            {
                return this.Points - this.OpposingPlayer.Points;
            }
        }

        /// <summary>
        ///   Gets the player game status: check, stalemate or checkmate.
        /// </summary>
        public PlayerStatusNames Status
        {
            get
            {
                if (this.IsInCheckMate)
                {
                    return PlayerStatusNames.InCheckMate;
                }

                if (!this.CanMove)
                {
                    return PlayerStatusNames.InStalemate;
                }

                if (this.IsInCheck)
                {
                    return PlayerStatusNames.InCheck;
                }

                return PlayerStatusNames.Normal;
            }
        }

        /// <summary>
        ///   Gets the sum of the player's piece value.
        /// </summary>
        public int TotalPieceValue
        {
            get
            {
                int intValue = 0;
                foreach (Piece piece in this.Pieces)
                {
                    intValue += piece.Value;
                }

                return intValue;
            }
        }
        
        public bool CanClaimMoveRepetitionDraw(int numberOfMoves)
        {
            if (Game.MoveHistory.Count == 0)
            {
                return false;
            }

            // if (this.Colour==Game.MoveHistory.Last.Piece.Player.Colour)
            // {
            // return false;
            // }
            int intRepetitionCount = 1;
            int intIndex = Game.MoveHistory.Count - 1;
            for (; intIndex >= 0; intIndex--, intIndex--)
            {
                Move move = Game.MoveHistory[intIndex];
                if (move.HashCodeA == Board.HashCodeA && move.HashCodeB == Board.HashCodeB)
                {
                    if (intRepetitionCount >= numberOfMoves)
                    {
                        return true;
                    }

                    intRepetitionCount++;
                }

                if (move.Piece.Name == Piece.PieceNames.Pawn || move.PieceCaptured != null)
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// The capture all pieces.
        /// </summary>
        public void CaptureAllPieces()
        {
            for (int intIndex = this.Pieces.Count - 1; intIndex >= 0; intIndex--)
            {
                Piece piece = this.Pieces.Item(intIndex);
                piece.Capture();
            }
        }

        /// <summary>
        /// The decrease material count.
        /// </summary>
        public void DecreaseMaterialCount()
        {
            this.MaterialCount--;
        }

        /// <summary>
        /// The decrease pawn count.
        /// </summary>
        public void DecreasePawnCount()
        {
            this.PawnCountInPlay--;
        }

        /// <summary>
        /// The demote all pieces.
        /// </summary>
        public void DemoteAllPieces()
        {
            for (int intIndex = this.Pieces.Count - 1; intIndex >= 0; intIndex--)
            {
                Piece piece = this.Pieces.Item(intIndex);
                if (piece.HasBeenPromoted)
                {
                    piece.Demote();
                }
            }
        }

        /// <summary>
        /// Determine check status.
        /// </summary>
        /// <returns>
        /// Return check status.
        /// </returns>
        public bool DetermineCheckStatus()
        {
            Console.WriteLine($"Checking King status for {this.Colour}...");

            if (this.King == null)
            {
                throw new Exception($"ERROR: King is NULL for {this.Colour}!");
            }

            return ((PieceKing)this.King.Top).DetermineCheckStatus();
        }


        /// <summary>
        /// Generate "lazy" moves for all pieces. Lazy means we include moves that put our own king in check.
        /// </summary>
        /// <param name="moves">
        /// Move list to be filled with moves.
        /// </param>
        /// <param name="movesType">
        /// Type of moves to be generated. e.g. all or just captures.
        /// </param>
        public void GenerateLazyMoves(Moves moves, Moves.MoveListNames movesType)
        {
            foreach (Piece piece in this.Pieces)
            {
                piece.GenerateLazyMoves(moves, movesType);
            }
        }

        /// <summary>
        /// Generate legal moves. i.e. exclude moves that would put own king in check.
        /// </summary>
        /// <param name="moves">
        /// The moves.
        /// </param>
        public void GenerateLegalMoves(Moves moves)
        {
            this.GenerateLazyMoves(moves, Moves.MoveListNames.All);

            // ✅ Create a temporary list to store moves to remove
            List<Move> movesToRemove = new List<Move>();

            for (int intIndex = moves.Count - 1; intIndex >= 0; intIndex--)
            {
                Move move = moves[intIndex];
                Move moveUndo = move.Piece.Move(move.Name, move.To);

                if (move.Piece.Player.IsInCheck)
                {
                    movesToRemove.Add(move); // ✅ Don't remove directly inside the loop
                }

                Model.Move.Undo(moveUndo);
            }

            // ✅ Now remove them safely AFTER the loop
            foreach (Move move in movesToRemove)
            {
                moves.Remove(move);
            }
        }

        /// <summary>
        /// Determines of the player has the specified piece.
        /// </summary>
        /// <param name="piecename">
        /// The piecename.
        /// </param>
        /// <returns>
        /// True if player has the piece.
        /// </returns>
        public bool HasPieceName(Piece.PieceNames piecename)
        {
            if (piecename == Piece.PieceNames.Pawn && this.PawnCountInPlay > 0)
            {
                return true;
            }

            foreach (Piece piece in this.Pieces)
            {
                if (piece.Name == piecename)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The increase material count.
        /// </summary>
        public void IncreaseMaterialCount()
        {
            this.MaterialCount++;
        }

        /// <summary>
        /// The increase pawn count.
        /// </summary>
        public void IncreasePawnCount()
        {
            this.PawnCountInPlay++;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The set pieces at starting positions.
        /// </summary>
        public virtual void SetPiecesAtStartingPositions()
        {
            Console.WriteLine("Default Player Setup Called.");
        }

        #endregion
    }
}