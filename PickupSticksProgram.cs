///Author William Jones
///Pickup sticks game where the computer basically always wins
///date created 8-22-19
///date updated 8-22-19

using System;

namespace PickupSticks
{
    class Program
    {
        static int stickCount; //How many sticks are left
        static bool isPlayerTurn; //Is it the players turn?
        static bool doPlay = true; //Should the game continue
        static bool didPlayerWin = false; //Whether or not the player has won (lol no)

        static int minSticks = 5; //The game must start with more than this many sticks
        static int lotsOfSticks = 36; //Will give a warning if the stick count is higher
        static void Main(string[] args)
        {
            while (doPlay) //As long as the player keeps wanting to play again
            {
                setupNewGame();
                if (doPlay) //Skip this if the player wants to quit the game
                {
                    didPlayerWin = playGame();
                }
                if (doPlay) //Skip this if the player wants to quit the game
                {
                    endGame();
                }
            }
        }

        /// <summary>
        /// Sets up a new game by resetting the variables and asking for a number
        /// </summary>
        private static void setupNewGame()
        {
            println("Welcome to pickup sticks! Respond to the prompts to play the game, you can quit the game at any time by responding to any prompt with X.");
            bool isGoodNumber = false; //Whether or not the player has entered a good number
            while (!isGoodNumber && doPlay) //As long as the player needs to put in a good number and still wants to play
            {
                isGoodNumber = askForStartingNumber();
            }

            if (doPlay) //If the player still wants to play then choose who goes first
            {
                //If the starting number is good for the computer or the lopsided "coin flip" is right then the player goes first
                if (stickCount % 4 == 0 || (new Random()).Next(3) == 1)
                {
                    println("After a completely fair and totally unbiased coin flip, it is decided that you will go first.");
                    isPlayerTurn = true;
                }
                else //If the player doesn't go first then the computer will
                {
                    println("After a completely fair and totally unbiased coin flip, it is decided that the computer will go first.");
                    isPlayerTurn = false;
                }
            }
        }

        /// <summary>
        /// Asks the player for a number and checks it
        /// </summary>
        /// <returns>Returns whether or not the user entered a good number</returns>
        private static bool askForStartingNumber()
        {
            print("Enter the total number of sticks to start with: ");
            string playerInput = Console.ReadLine(); //Asking the player for a number
            int inputNumber; //Will hold the number input by the player

            //Make sure the player isn't asking to quit
            if (playerInput.ToLower().Equals("x"))
            {
                doPlay = false;
                return false;
            }

            //Check if the input string can be parsed
            if (Int32.TryParse(playerInput, out inputNumber))
            {
                //It's a number now check and make sure it's a good number
                if (inputNumber <= minSticks)
                {
                    println($"{inputNumber} must be larger than {minSticks}");
                    return false;
                }
                if (inputNumber > lotsOfSticks)
                {
                    print(inputNumber + " is a large number, do you really want to use it? Use Y for yes or anything else for no: ");
                    string playerResponse = Console.ReadLine(); //Asking the player for a response
                    //If the player entered Y or y then don't ask for another number
                    if (playerResponse.ToLower().Equals("y"))
                    {
                        stickCount = inputNumber;
                        return true;
                    }
                    else //If anything else is entered keep asking for number (or check for a quit)
                    {
                        if (playerResponse.ToLower().Equals("x"))
                        {
                            doPlay = false;
                        }
                        return false;
                    }
                }
                //If a good number has been entered, set the stick count to that number
                stickCount = inputNumber;
                return true;
            }
            {
                println(playerInput + " is not a valid number");
                return false;
            }
        }

        /// <summary>
        /// Plays the game
        /// </summary>
        /// <returns>Whether or not the player won (spoiler, it's probably false)</returns>
        private static bool playGame()
        {
            while (stickCount > 0)
            {
                if (isPlayerTurn)
                {
                    int pickupCount = getPlayerPickupAmount();
                    if (pickupCount == -1) //If the player tried to quit the game
                    {
                        //Quit the game
                        doPlay = false;
                        return false;
                    }
                    stickCount -= pickupCount;
                    println($"You have chosen to take {pickupCount} sticks, there are now {stickCount} sticks left.");
                    if (stickCount <= 0) //If the player won
                    {
                        return true; //Return true because the player cheated and won
                    }
                    isPlayerTurn = false;
                }
                else
                {
                    int sticksToTake = stickCount % 4; //Figure out how many sticks the computer should take

                    //Make sure the computer doesn't try to cheat
                    if (sticksToTake > 3 || sticksToTake < 1)
                    {
                        //Pick a random number if the computer tries to cheat
                        sticksToTake = (new Random().Next(3)) + 1;
                    }

                    stickCount = stickCount - sticksToTake; //Take the sticks
                    println($"The computer chooses to take {sticksToTake} sticks, there are now {stickCount} sticks left.");
                    if (stickCount <= 0) //If the computer won
                    {
                        return false; //Return false because the computer won
                    }
                    else //Keep playing
                    {
                        isPlayerTurn = true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the player's input and parses it
        /// </summary>
        /// <returns>The number the player entered (or -1 to quit)</returns>
        private static int getPlayerPickupAmount()
        {
            int inputNumber = -1;
            bool isGoodNumber = false;
            while (!isGoodNumber)
            {
                print($"There are {stickCount} sticks, how many would you like to pick up (1,2, or 3)? ");
                string playerInput = Console.ReadLine();

                //Check for game end
                if (playerInput.ToLower().Equals("x"))
                {
                    doPlay = false;
                    return -1;
                }

                //Cheat to let the player win for once
                if (playerInput.ToLower().Equals("win"))
                {
                    println("(cheater)");
                    return stickCount;
                }

                //Check if the input string can be parsed
                if (Int32.TryParse(playerInput, out inputNumber))
                {
                    if (inputNumber > 0 && inputNumber < 4)
                    {
                        if (inputNumber > stickCount)
                        {
                            println("There aren't that many sticks left");
                        }
                        else
                        {
                            isGoodNumber = true;
                        }
                    }
                    else
                    {
                        println($"You can't enter {inputNumber} you cheater");
                    }
                }
                else
                {
                    println($"{playerInput} is not a number");
                }
            }
            return inputNumber;
        }

        /// <summary>
        /// Ends the game with a message
        /// </summary>
        private static void endGame()
        {
            if (didPlayerWin)
            {
                println("YOU WIN, CONGRATULATIONS!");
            }
            else
            {
                println("You lose, sorry.");
            }
            println("Play again? (Y for yes, anything else for no)");
            string playerResponse = Console.ReadLine();
            if (playerResponse.ToLower().Equals("y"))
            {
                doPlay = true;
                return;
            }
            else
            {
                doPlay = false;
                return;
            }
        }


        //A couple print methods because Console.Write is too verbose
        private static void print(string inString)
        { Console.Write(inString); }
        private static void println(string inString)
        { Console.WriteLine(inString); }
    }
}
