using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Create the maze cell with four walls, one in each direction.
public class MazeCell
{
    public bool Visited = false;

    public GameObject upWall;
    public GameObject downWall;
    public GameObject leftWall;
    public GameObject rightWall;
}

public class Maze : MonoBehaviour
{
    // Min size of the maze
    public int Rows = 2;
    public int Columns = 2;

    // User input to regenerate
    public InputField UserWidth;
    public InputField UserHeight;

    // Start location
    private int currentRow = 0;
    private int currentColumn = 0;

    private bool mazeComplete = false;

    public GameObject Wall;
    public GameObject Floor;

    private MazeCell[,] grid;

    // Indexes start position to loop through the grid. 
    int i = 0;
    int j = 0;

    void Start() 
    {
        GenerateGrid();
    }

    // Main function that call all the others.
    // When the user regenerates the maze will call this function
    void GenerateGrid()
    {
        foreach (Transform transform in transform)
        {
            Destroy(transform.gameObject);
        }

        CreateMaze();
        CameraPosition();

        currentRow = 0;
        currentColumn = 0;
        mazeComplete = false;

        // Start of the algorithm
       while (!mazeComplete)
        {
            Kill();
            Hunt();
        }
    }

    // Create all walls and the floor.
    void CreateMaze() 
    {
        float size = Wall.transform.localScale.x;
        grid = new MazeCell[Rows, Columns];
        
        
        i = 0;
        while (i < Rows)
        {
            j = 0;
            while (j < Columns)
            {   
                GameObject floor = Instantiate(Floor, new Vector3(j * size, 0, -i * size), Quaternion.identity);

                GameObject UpWall = Instantiate(Wall, new Vector3(j * size, 1.30f, -i * size + 1.20f), Quaternion.identity);
                UpWall.GetComponent<Renderer>().material.color = new Color(0.75f, 0.39f, 0.39f);

                GameObject DownWall = Instantiate(Wall, new Vector3(j * size, 1.30f, -i * size - 1.20f), Quaternion.identity);
                DownWall.GetComponent<Renderer>().material.color = new Color(0.54f, 0.47f, 0.47f);

                GameObject LeftWall = Instantiate(Wall, new Vector3(j * size -1.20f, 1.30f, -i * size), Quaternion.Euler(0, 90, 0));

                GameObject RightWall = Instantiate(Wall, new Vector3(j * size +1.20f, 1.30f, -i * size), Quaternion.Euler(0, 90, 0));
                RightWall.GetComponent<Renderer>().material.color = new Color(0.94f, 0.66f, 0.62f);

                // Link the walls with the current cell.
                grid[i, j] = new MazeCell();
                grid[i, j].upWall = UpWall;
                grid[i, j].downWall = DownWall;
                grid[i, j].leftWall = LeftWall;
                grid[i, j].rightWall = RightWall;

                // Make all objects linked with a parent, making possible destroy everything in a easy way after.
                floor.transform.parent = transform;
                UpWall.transform.parent = transform;
                DownWall.transform.parent = transform;
                LeftWall.transform.parent = transform;
                RightWall.transform.parent = transform;

                //Creates the entrance of the maze
                if (j == 0 && i == 0)
                    Destroy(LeftWall);

                //Creates the exit of the maze
                if (i == Rows - 1 && j == Columns - 1)
                    Destroy(RightWall);
                j++;
            }
            i++;
        }
    }

    // Position the camera at the center of the maze, and adequates the zoom to fit all on the screen.
    void CameraPosition() 
    {
        float size = Wall.transform.localScale.x;
        Vector3 cameraPosition = Camera.main.transform.position;
        cameraPosition.x = Mathf.Round(Columns / 2) * size;
        cameraPosition.y = Mathf.Max(2, Mathf.Max(Rows, Columns) * 3f);
        cameraPosition.z = -Mathf.Round(Rows / 2) * size;
        Camera.main.transform.position = cameraPosition;

    }

    // Perform a random walk, carving passages to unvisited neighbors, until the current cell has no unvisited neighbors.
    // Starts with the top left (0, 0), and each time it runs again will start at the given position from the hunt phase.
    // The order of the check is Up -> Down -> Left -> Right
    void Kill() 
    {
        while (UnvisitedNeighbour(currentRow, currentColumn))
        {
            int direction = Random.Range(0, 4);
            if (direction == 0 && currentRow > 0 && !grid[currentRow - 1, currentColumn].Visited)
            {
                Destroy(grid[currentRow, currentColumn].upWall);
                grid[currentRow, currentColumn].Visited = true;
                currentRow--;
                Destroy(grid[currentRow, currentColumn].downWall);
            }
            else if (direction == 1 && currentRow < Rows - 1 && !grid[currentRow + 1, currentColumn].Visited)
            {
                Destroy(grid[currentRow, currentColumn].downWall);
                grid[currentRow, currentColumn].Visited = true;
                currentRow++;
                Destroy(grid[currentRow, currentColumn].upWall);
            }
            else if (direction == 2 && currentColumn > 0 && !grid[currentRow, currentColumn - 1].Visited)
            {
                Destroy(grid[currentRow, currentColumn].leftWall);
                grid[currentRow, currentColumn].Visited = true;
                currentColumn--;
                Destroy(grid[currentRow, currentColumn].rightWall);
            }
            else if (direction == 3 && currentColumn < Columns - 1 && !grid[currentRow, currentColumn + 1].Visited)
            {
                Destroy(grid[currentRow, currentColumn].rightWall);
                grid[currentRow, currentColumn].Visited = true;
                currentColumn++;
                Destroy(grid[currentRow, currentColumn].leftWall);
            }
        }
    }


    // Will check if the current cell has any unvisited neighbour around it.
    // At same order Top -> Down -> Left - Right.
    bool UnvisitedNeighbour(int currentRow, int currentColumn) 
    {
        if (currentRow > 0 && !grid[currentRow - 1, currentColumn].Visited)
            return true;
        else if (currentRow < Rows - 1 && !grid[currentRow + 1, currentColumn].Visited)
            return true;
        else if (currentColumn > 0 && !grid[currentRow, currentColumn - 1].Visited)
            return true;
        else if (currentColumn < Columns - 1 && !grid[currentRow, currentColumn + 1].Visited)
            return true;
        return false;
    }

    // Will loop through the entire grid (from top left) to try to find an unvisited cell.
    // If finds it will pass to the RemoveWall function to carve a path between the found cell and a visited room.
    void Hunt() 
    {
        int i = 0;
        int j = 0;

        mazeComplete = true;
        
        while (i < Rows)
        {
            j = 0;
            while (j < Columns) 
            {
                if (!grid[i, j].Visited && VisitedNeighbour(i, j))
                {
                    currentRow = i;
                    currentColumn = j;
                    grid[currentRow, currentColumn].Visited = true;
                    RemoveWall();
                    mazeComplete = false;
                    return;
                }
                j++;
            }
            i++;
        }
    }

    // Will check if the current cell has any visited neighbour around it.
    // At same order Top -> Down -> Left - Right.
    bool VisitedNeighbour(int currentRow, int currentColumn) 
    {
        if (currentRow > 0 && grid[currentRow - 1, currentColumn].Visited)
            return true;
        else if (currentRow < Rows - 1 && grid[currentRow + 1, currentColumn].Visited)
            return true;
        else if (currentColumn > 0 && grid[currentRow, currentColumn - 1].Visited)
            return true;
        else if (currentColumn < Columns - 1 && grid[currentRow, currentColumn + 1].Visited)
            return true;
        return false;
    }

    // For sure there's at least one direction to destroy walls (a visited cell around).
    // It will try random directions till it finds one that suits the conditions carving a path and exiting.
    void RemoveWall()
    {
        bool Removed = false;

        while (!Removed)
        {
            int direction = Random.Range(0, 4);
            if (direction == 0 && currentRow > 0 && grid[currentRow - 1, currentColumn].Visited)
            {
                Destroy(grid[currentRow, currentColumn].upWall);
                Destroy(grid[currentRow - 1, currentColumn].downWall);
                Removed = true;
            }
            else if (direction == 1 && currentRow < Rows - 1 && grid[currentRow + 1, currentColumn].Visited)
            {
                Destroy(grid[currentRow, currentColumn].downWall);
                Destroy(grid[currentRow + 1, currentColumn].upWall);
                Removed = true;
            }
            else if (direction == 2 && currentColumn > 0 && grid[currentRow, currentColumn - 1].Visited) 
            {
                Destroy(grid[currentRow, currentColumn].leftWall);
                Destroy(grid[currentRow, currentColumn - 1].rightWall);
                Removed = true;
            }
            else if (direction == 3 && currentColumn < Columns - 1 && grid[currentRow, currentColumn + 1].Visited)
            {
                Destroy(grid[currentRow, currentColumn].rightWall);
                Destroy(grid[currentRow, currentColumn + 1].leftWall);
                Removed = true;
            }
        }
    }

    // Will be used when the user chooses a Height and Width.
    // It'll restart the code with the new Rows and Columns
    public void RegenerateButton() 
    {
        int rows = 2;
        int columns = 2;

        if (int.TryParse(UserHeight.text, out rows))
            Rows = rows;
        if (int.TryParse(UserWidth.text, out columns))
            Columns = columns;

        GenerateGrid();  
    } 
}
