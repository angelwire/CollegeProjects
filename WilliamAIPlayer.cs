using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * William Jones
 * AI player for Henatatafle/Viking Chess, It's not too bad
 * Just two functions - getBoardScore and GetMove
 * It's made to work with other classes to manipulate the game board
 * In my Game prorgamming class this AI was able to successfully beat all the AIs of my classmates
 * */

namespace Hnefatafl
{
    class WilliamAIPlayer:Player
    {

        //Written from point of attacker
        const int KING_ATTACK_SINGLE_NEIGHBOR_SCORE = 4; //score for one neighboring enemy
        const int KING_ATTACK_DOUBLE_NEIGHBOR_SCORE = 12; //Score for two neighboring enemies
        const int KING_ATTACK_TRIPLE_NEIGHBOR_SCORE = 20; //Score for three neighboring enemies
        const int KING_DEFENDER_SCORE_MULTIPLY = 1; //points for each friendly neighbor
        const int KING_IS_ON_BORDER = -21; //points if the king is on the border
        const int KING_CENTER_DISTANCE_MULTIPLY = -2; //Points for each square the king is away from the throne
        const int ATTACKER_ON_EDGE_SCORE = 0; //points per attacker on edge
        const int ATTACKER_ON_CORNER_SCORE = 1; //points per attacker on corner
        const int DEFENDER_ON_EDGE_SCORE = 1; //points per defender on edge (defenders don't wanna be on the edge)
        const int ATTACKER_VALUE = 13; //points per attacker
        const int DEFENDER_VALUE = -15; //points per defender
        const int KING_ON_END_SPACE = -50000;//points if the king is on the end space
        const int KING_IS_CAPTURED = 50000;//points if the king is on the end space
        const int ATTACKER_COLUMN_COVERAGE_SCORE = 1; //Points for each column covered by the attackers
        const int ATTACKER_ROW_COVERAGE_SCORE = 1; //Points for each row covered by the attackers

        Random random = new Random(); //Just for randomizing stuff

        int maxDepth = 2; //How many moves ahead to check (keep at 2 or else it'll get reeally slow)

        public WilliamAIPlayer(Pieces side) : base(side)
        {
            Name = "William's Artificial W.I.T.";
        }

        public override Move GetMove(Board board)
        {
            //The initial value for the maximum score
            int maxScore = -999999;

            //The score for the current board
            int currentBoardScore = getBoardScore(board,99,maxScore);

            //The number of moves
            List<Move> moves = board.getMoveList();

            //Get a random move at first
            Move useMove = moves.ElementAt(random.Next(moves.Count));

            //A list of good moves
            List<Move> goodMoves = new List<Move>();
            
            //A move that should be used if there are no "good" moves
            Move standbyMove = useMove;

            foreach (Move currentMove in moves)
            {
                //The board to check
                Board checkBoard = board.DeepClone();

                //Make the move
                checkBoard.Move(currentMove);

                int boardScore = getBoardScore(checkBoard,
                    1,
                    currentBoardScore); //Get the score of the board

                //Console.WriteLine("Got a score of: " + boardScore);

                //Flip the values if the ai is a defender
                if (side == Pieces.DEFEND)
                { boardScore = -boardScore; }

                //If this score is at least tied with the best score
                if (boardScore >= maxScore)
                {
                    //If it's the best get rid of all the old good moves
                    if(boardScore > maxScore)
                    {
                        maxScore = boardScore;
                        goodMoves.Clear();
                    }
                    //Add this move to the list of good moves
                    goodMoves.Add(currentMove);
                }
            }
            //If there is at least one good move
            if (goodMoves.Count >= 1)
            {
                //The move to use will be a random move within the good move list
                useMove = goodMoves.ElementAt(random.Next(goodMoves.Count));
            }
            else
            {
                //Just get the initial random move if no good move was found
                useMove = standbyMove;
            }
            //Return the move
            return useMove;
        }

        private int getBoardScore(Board inboard, int level, int targetScore)
        {
            //The current score for the board
            int currentScore = 0;

            //The pieces to iterate through
            Pieces[,] piecesArray = inboard.board;

            //The coverage for the attackers
            bool[] attackerColumnCoverage = new bool[Board.GRID_SIZE];
            bool[] attackerRowCoverage = new bool[Board.GRID_SIZE];
            int attackerRowCount = 0;
            int attackerColumnCount = 0;

            //Iterate along the Y first
            for (int yy = 0; yy < Board.GRID_SIZE; yy += 1)
            {
                //Iterate along the X
                for (int xx = 0; xx < Board.GRID_SIZE; xx += 1)
                {
                    //The piece at the current x and y position
                    Pieces currentPiece = piecesArray[xx, yy];

                    //Ignore it if it's empty
                    if (currentPiece != Pieces.EMPTY)
                    {
                        if (currentPiece == Pieces.ATTACK)
                        {
                            currentScore += ATTACKER_VALUE;

                            //Calculate board coverage
                            if (!attackerColumnCoverage[xx])
                            {
                                attackerColumnCoverage[xx] = true;
                                attackerColumnCount += 1;
                            }
                            if (!attackerRowCoverage[yy])
                            {
                                attackerRowCoverage[yy] = true;
                                attackerRowCount += 1;
                            }

                            //If the piece is on the border add score
                            if (xx == 0 || xx == Board.GRID_SIZE - 1 || yy == 0 || yy == Board.GRID_SIZE - 1)
                            { currentScore += ATTACKER_ON_EDGE_SCORE; }

                            //If the attacker is on the corner
                            if (((xx == 1 || xx == Board.GRID_SIZE - 2) && (yy == 1 || yy == Board.GRID_SIZE - 2))
                                || ((xx == 1 || xx == Board.GRID_SIZE - 2) && (yy == 1 || yy == Board.GRID_SIZE - 2)))
                            {
                                currentScore += ATTACKER_ON_CORNER_SCORE;
                            }
                        }
                        else if (currentPiece == Pieces.DEFEND)
                        {
                            currentScore += DEFENDER_VALUE;

                            //If the piece is on the border add score
                            if (xx == 0 || xx == Board.GRID_SIZE - 1 || yy == 0 || yy == Board.GRID_SIZE - 1)
                            { currentScore += DEFENDER_ON_EDGE_SCORE; }
                        }
                        else if (currentPiece == Pieces.KING)
                        {
                            //Console.WriteLine(String.Format("king is at ({0},{1})",xx,yy));
                            //Treat outside the border as enemy pieces
                            Pieces leftNeighbor = Pieces.ATTACK;
                            Pieces rightNeighbor = Pieces.ATTACK;
                            Pieces upNeighbor = Pieces.ATTACK;
                            Pieces downNeighbor = Pieces.ATTACK;

                            //Whether or not the king is on a border
                            bool leftBorder = (xx == 0);
                            bool rightBorder = (xx == Board.GRID_SIZE - 1);
                            bool bottomBorder = (yy == 0);
                            bool topBorder = (yy == Board.GRID_SIZE - 1);

                            //Set the neighboring pieces while not going outside of bounds
                            if (!leftBorder) { leftNeighbor = piecesArray[xx - 1, yy]; }
                            if (!rightBorder) { rightNeighbor = piecesArray[xx + 1, yy]; }
                            if (!bottomBorder) { downNeighbor = piecesArray[xx, yy - 1]; }
                            if (!topBorder) { upNeighbor = piecesArray[xx, yy + 1]; }


                            //Check to see if the king is on the border
                            if (leftBorder || rightBorder || topBorder || bottomBorder)
                            {
                                currentScore += KING_IS_ON_BORDER;
                            }

                            //Check to see if the king is on the escape space
                            if ((leftBorder || rightBorder) && (topBorder || bottomBorder))
                            {
                                return KING_ON_END_SPACE;
                            }

                            int kingAttackerNeighbors = 0;

                            //Add points for neighbors
                            switch (leftNeighbor)
                            {
                                case Pieces.ATTACK: kingAttackerNeighbors += 1; break;
                                case Pieces.DEFEND: currentScore += KING_DEFENDER_SCORE_MULTIPLY; break;
                            }
                            switch (rightNeighbor)
                            {
                                case Pieces.ATTACK: kingAttackerNeighbors += 1; break;
                                case Pieces.DEFEND: currentScore += KING_DEFENDER_SCORE_MULTIPLY; break;
                            }
                            switch (upNeighbor)
                            {
                                case Pieces.ATTACK: kingAttackerNeighbors += 1; break;
                                case Pieces.DEFEND: currentScore += KING_DEFENDER_SCORE_MULTIPLY; break;
                            }
                            switch (downNeighbor)
                            {
                                case Pieces.ATTACK: kingAttackerNeighbors += 1; break;
                                case Pieces.DEFEND: currentScore += KING_DEFENDER_SCORE_MULTIPLY; break;
                            }

                            switch(kingAttackerNeighbors)
                            {
                                case 1: currentScore += KING_ATTACK_SINGLE_NEIGHBOR_SCORE; break;
                                case 2: currentScore += KING_ATTACK_DOUBLE_NEIGHBOR_SCORE; break;
                                case 3: currentScore += KING_ATTACK_TRIPLE_NEIGHBOR_SCORE; break;
                                case 4: return KING_IS_CAPTURED;
                            }
                            //Add the king's distance to the score
                            currentScore += (Math.Abs(xx - Board.THRONE) + Math.Abs(yy - Board.THRONE)) * KING_CENTER_DISTANCE_MULTIPLY;
                        }
                    }
                }
            }

            //Now calculate the coverage
            currentScore += (attackerColumnCount * ATTACKER_COLUMN_COVERAGE_SCORE) +(attackerRowCount * ATTACKER_ROW_COVERAGE_SCORE);

            //Return the score if it's at the max depth
            if (level >= maxDepth)
            {
                return currentScore;
            }
            else
            {
                //The max and min score from subsequent moves
                int maxScore = -999999;
                int minScore = 999999;

                //Loop through every move in the new board
                foreach (Move checkMove in inboard.getMoveList())
                {
                    //Get the board
                    Board boardToCheck = inboard.DeepClone();

                    //Make the move
                    boardToCheck.Move(checkMove);

                    //Score the updated board
                    int newScore = getBoardScore(boardToCheck,
                        level + 1,
                        currentScore);

                    //Set the minimum or maximum score
                    if (newScore > maxScore)
                    {
                        maxScore = newScore;
                    }
                    if (newScore < minScore)
                    {
                        minScore = newScore;
                    }
                }
                //If I'm attacking return the minimum score
                if (side == Pieces.ATTACK)
                {
                    return minScore;
                }
                else //If I'm defending return the maximum score
                {
                    return maxScore;
                }
            }
        }
    }
}
