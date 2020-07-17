using System;
using System.Collections;
using System.IO;

//William Jones
//Simple A* program
//Asks the user to input a maze and then uses the A* pathfinding algorithm to solve it
namespace Astar
{
    class AStarProgram
    {
        static readonly int IMPASSABLE_VALUE = 0;
        static readonly int MAX_DRAW_WIDTH = 20;

        static bool shouldPrintGrid = true;

        static int gridWidth = 0;
        static int gridHeight = 0;
        static int startX = 0;
        static int startY = 0;
        static int endX = 0;
        static int endY = 0;

        static Node[,] gridArray;
        static ArrayList checkNodes = new ArrayList();

        static void Main(string[] args)
        {
            //Get the file name and path and try to open the file
            Console.Write("Enter the file: ");
            string fileLocation = Console.ReadLine();
            try { openFile(fileLocation);}
            catch (Exception e) {Console.WriteLine("Bad file: " + e.ToString()); Environment.Exit(-1); }

            //The file has been loaded now ask for starting and ending locations
            try { getLocations(); }
            catch (Exception e) { Console.WriteLine("Bad input: " + e.ToString()); Environment.Exit(-1);}

            //Try to find the path
            try { findPath(); }
            catch (Exception e) { Console.WriteLine("Bad map: " + e.ToString()); Environment.Exit(-1); }

        }

        private static void getLocations()
        {
            //Collect the inputs, parse them, and make sure they're good values
            //Start X
            Console.Write("Input the x coordinate of the start (with 0 being left): ");
            if (int.TryParse(Console.ReadLine(), out startX))
            { 
                if (startX >= gridWidth) { throw new Exception("Start X is too large"); }
                if (startX < 0) { throw new Exception("Start X is too small"); }
            }
            //Start Y
            Console.Write("Input the y coordinate of the start (with 0 being top): ");
            if (int.TryParse(Console.ReadLine(), out startY))
            {
                if (startY >= gridHeight) { throw new Exception("Start y is too large"); }
                if (startY < 0) { throw new Exception("Start y is too small"); }
            }
            //End X
            Console.Write("Input the x coordinate of the end (with 0 being left): ");
            if (int.TryParse(Console.ReadLine(), out endX))
            {
                if (endX >= gridWidth) { throw new Exception("End x is too large"); }
                if (endX < 0) { throw new Exception("End x is too small"); }
            }
            //End Y
            Console.Write("Input the y coordinate of the end (with 0 being top): ");
            if (int.TryParse(Console.ReadLine(), out endY))
            {
                if (endY >= gridHeight) { throw new Exception("End y is too large"); }
                if (endY < 0) { throw new Exception("End y is too small"); }
            }
        }

        //Opens the file with the given string
        private static void openFile(string fileName)
        {
            using (StreamReader fileStream= new StreamReader(fileName))
            {
                //Get file header
                string headerLine = fileStream.ReadLine();
                if (headerLine != null)
                {
                    //Get the header line as an array of chars
                    string[] headerArray = headerLine.Split(" ");
                    //Parse header
                    if (headerArray.Length != 2)
                    {
                        throw new Exception("File parsing error, wrong number of arguments in header");
                    }
                    else
                    {
                        try
                        {
                            //Set the grid width and height
                            gridHeight = int.Parse(headerArray[0]);
                            gridWidth = int.Parse(headerArray[1]);

                            if (gridWidth > MAX_DRAW_WIDTH)
                            {
                                shouldPrintGrid = false;
                            }
                        } catch
                        {
                            throw new Exception("File parsing error, bad header value");
                        }
                    }
                }
                else
                {
                    throw new Exception("File parsing error, no header");
                }

                //Create the grid array
                gridArray = new Node[gridWidth, gridHeight];

                //Now loop through the rest of the file to fill in the grid
                int putY = 0; //Where to put the node
                string currentLine = "";
                while ((currentLine = fileStream.ReadLine()) != null)
                {
                    //Make sure there aren't too many rows
                    if (putY < gridHeight)
                    {
                        //Loop through the entire line
                        int putX = 0; //Where to put the node
                        foreach (char currentChar in currentLine.ToCharArray())
                        {
                            //Make sure the X value isn't too large
                            if (putX < gridWidth)
                            {
                                int currentInt = 0;
                                if (int.TryParse(currentChar.ToString(), out currentInt))
                                {
                                    //If the int can be parsed put it in the grid
                                    Node createdNode = new Node(currentInt, putX, putY);
                                    gridArray[putX, putY] = createdNode;
                                    createdNode.setManhattanDistance(endX, endY);

                                    //And print it out if it should be printed
                                    if (shouldPrintGrid)
                                    {
                                        Console.Write(currentInt);
                                        Console.Write(" ");
                                    }
                                }
                                else
                                {
                                    throw new Exception("Grid parsing error: Integer parsing error");
                                }
                                //Increment the x position
                                putX += 1;
                            }
                            else
                            {
                                throw new Exception("Grid parsing error: too many columns in row number " + putY);
                            }
                        }
                        //If there aren't enough columns then throw an error
                        if (putX < gridWidth)
                        {
                            throw new Exception("Grid parsing error: not enough columns in row number " + putY);
                        }
                        //Go to the next line in the grid array
                        putY += 1;

                        //Print out a new line if the grid isn't too wide
                        if (shouldPrintGrid) { Console.WriteLine(); }
                    }
                    else
                    {
                        throw new Exception("Grid parsing error: too many rows");
                    }
                }
                //Check to make sure there are enough lines
                if (putY < gridHeight)
                {
                    throw new Exception("Grid parsing error: not enough rows");
                }
            }
        }

        //The base method for finding a path using A*
        private static void findPath()
        {
            bool foundPath = false; //Whether or not a path has been found
            Node startingNode = getNodeAtPosition(startX, startY); //The starting node
            startingNode.setPath(true); //Tell the starting node it's on the final path
            Node currentNode = startingNode; //It should start at the beginning
            Node endingNode = getNodeAtPosition(endX, endY); //It should end at the end

            while (!foundPath)
            {
                //Circle the current node
                //and set the estimated distances for all nodes surrounding the current node
                currentNode.setCircled(true);
                for (int ii = 0; ii < 8; ii += 1)
                {
                    checkNeighborNode(currentNode, ii);
                }

                Node nextNode;
                //Get the next node if there is one in the check list
                if (checkNodes.Count > 0) { nextNode = (Node)checkNodes[0]; }
                else { nextNode = null; }

                //If the nextNode isn't null
                if (nextNode != null)
                {
                    //Remove it from the check list because we're using it now
                    checkNodes.RemoveAt(0);
                    //Circle it
                    nextNode.setCircled(true);
                    //Now set it as the current node
                    currentNode = nextNode;

                    //Now see if we've found the end
                    if (currentNode == endingNode)
                    {
                        Console.WriteLine("Found the end node");
                        Console.WriteLine("Total distance is: " + currentNode.getDistanceFromStart());
                        Console.WriteLine("The backtracking path is: ");
                        foundPath = true;
                        //We know for sure that we've found the end because the end node was in the top of the check list
                    }
                }
                else //We're all out of nodes to check and there isn't an end anywhere
                {
                    throw new Exception("End not reachable");
                }
            }

            //Now that the path is found, backtrack and build the path
            Node backTrackCurrent = currentNode;
            Console.WriteLine(backTrackCurrent.getSimpleString());
            while (backTrackCurrent != startingNode)
            {
                backTrackCurrent.setPath(true);
                backTrackCurrent = backTrackCurrent.getParent();
                Console.WriteLine(backTrackCurrent.getSimpleString());
            }

            //Print the path if it can be printed
            if (shouldPrintGrid)
            {
                printPath();
            }
        }

        //Gets the node at the given position and makes sure it exists
        private static Node getNodeAtPosition(int inX, int inY)
        {
            Node returnNode = null;
            if (inX < gridArray.GetLength(0) && inY < gridArray.GetLength(1))
            {
                returnNode = gridArray[inX, inY];
            }

            //Throw an exception
            if (returnNode == null)
            {
                throw new Exception("Invalid position of (" + inX + "," + inY + ")");
            }
            return returnNode;
        }

        //Prints out the entire path
        private static void printPath()
        {
            //Loop through every node in the grid
            for (int yy = 0; yy < gridHeight; yy += 1)
            {
                //Add a new line
                Console.WriteLine();
                for (int xx = 0; xx < gridWidth; xx += 1)
                {
                    //Get the current node
                    Node currentNode = getNodeAtPosition(xx, yy);
                    string printString = "";

                    //If it's on the path print an X if not print a -
                    if (currentNode.getPath())
                    {
                        printString = "X";
                    }
                    else
                    {
                        printString = "-";
                    }
                    Console.Write(printString);
                }
            }
        }

        /// <summary>
        /// Checks the neiboring nodes
        /// </summary>
        /// <param name="inDirection">0-7, counter-clockwise starting at 0 degrees. (degrees = direction*45)</param>
        private static void checkNeighborNode(Node inNode, int inDirection)
        {
            int nodeX = inNode.getX();
            int nodeY = inNode.getY();
            int checkX = nodeX;
            int checkY = nodeY;
            //Simple equation to set the distance multiplication
            float distanceMultiplication = (inDirection % 2==0) ? 1.0f : 1.414f;

            //Get the neighboring node's position
            switch(inDirection)
            {
                case 0: checkX += 1; break;
                case 1: checkX += 1; checkY -= 1; break;
                case 2: checkY -= 1; break;
                case 3: checkX -= 1; checkY -= 1; break;
                case 4: checkX -= 1; break;
                case 5: checkX -= 1; checkY += 1; break;
                case 6: checkY += 1; break;
                case 7: checkX += 1; checkY += 1; break;
            }
            //Only continue if the check position is still on the grid
            if (checkX < gridWidth && checkX >= 0 && checkY < gridHeight && checkY >= 0)
            {
                //Now that we know it's still on the grid, get the node at that position
                Node checkNode = getNodeAtPosition(checkX, checkY);
                //Now only continue if the checkNode hasn't been circled and it's passable
                if (!checkNode.getCircled() && checkNode.getValue() != IMPASSABLE_VALUE)
                {
                    //Now calculate the various values for the check node
                    float currentNodeDistanceFromStart = inNode.getDistanceFromStart();
                    float toNodeDistance = checkNode.getValue() * distanceMultiplication;
                    float estimatedDistance = currentNodeDistanceFromStart + toNodeDistance + checkNode.getManhattanDistance();

                    //Update the estimated distance if it'll be smaller
                    //or if it hasn't been set yet
                    if (estimatedDistance < checkNode.getEstimatedDistance() || checkNode.getEstimatedDistance() < 0)
                    {
                        checkNode.setParent(inNode);
                        checkNode.setDistanceFromStart(currentNodeDistanceFromStart + toNodeDistance);
                        checkNode.setEstimatedDistance(estimatedDistance);
                        addToCheckList(checkNode);
                    }
                }
            }
        }

        //Adds a node to the check list and orders it according to its estimated distance
        private static void addToCheckList(Node inNode)
        {
            //If it's already in the checkList then delete it
            if (checkNodes.Contains(inNode))
            {
                checkNodes.Remove(inNode);
            }
            //Now try to insert it as early as possible to the 0th index will always be the lowest
            bool inserted = false;
            for (int ii = 0; ii< checkNodes.Count; ii+=1)
            {
                if (inNode.getEstimatedDistance() < ((Node) checkNodes[ii]).getEstimatedDistance())
                {
                    checkNodes.Insert(ii, inNode);
                    inserted = true;
                    break;
                }
            }
            //If it wasn't inserted then that means it's the largest so add it to the end
            if (inserted == false)
            {
                checkNodes.Add(inNode);
            }
        }
    }

    //A simple node class to keep things easier to manage
    class Node
    {
        //The node's properties
        private float myValue = 0.0f; //the value
        private int myX = 0; //the X position on the grid
        private int myY = 0; //the Y position on the grid
        private Node myParent = null; //the node that this node should point to on the path
        private bool myCircled = false; //Whether or not the node is "circled"
        private float myDistanceFromStart = 0; //The node's distance from the start node
        private int myManhattanDistance = -1; //The node's manhattan distance to the end
        private float myEstimatedDistance = -1; //The node's current estimated distance
        private bool isOnPath = false; //Whether or not the node is on the final path

        //Constructor to get the value and the x and y positions
        public Node (int value, int x, int y)
        {
            myValue = value;
            myX = x;
            myY = y;
        }

        //Simple math for the manhattan distance
        public void setManhattanDistance(int endPosX, int endPosY)
        {
            myManhattanDistance = Math.Abs(endPosX - myX) + Math.Abs(endPosY - myY);
        }

        //Getters and setters, lots of them
        public int getX() { return myX; }
        public int getY() { return myY; }
        public void setParent(Node inParent) { myParent = inParent; }
        public Node getParent() { return myParent;}
        public void setEstimatedDistance(float inEstimated) { myEstimatedDistance = inEstimated; }
        public float getEstimatedDistance() { return myEstimatedDistance; }
        public void setDistanceFromStart(float inDistance) { myDistanceFromStart = inDistance; }
        public float getDistanceFromStart() { return myDistanceFromStart; }
        public int getManhattanDistance() { return myManhattanDistance; }
        public float getValue() { return myValue; }
        public string getSimpleString() { return "[" + myX + "," + myY + "]"; }
        public bool getCircled() { return myCircled; }
        public void setCircled(bool inValue) { myCircled = inValue; }
        public bool getPath() { return isOnPath; }
        public void setPath(bool inValue) { isOnPath = inValue; }

        //A toString method for debugging
        override
        public string ToString()
        {
            return "["+ myX +","+ myY +"] val:" + myValue.ToString()
                + " est:" + myEstimatedDistance
                + " man:" + myManhattanDistance;
        }

    }
}