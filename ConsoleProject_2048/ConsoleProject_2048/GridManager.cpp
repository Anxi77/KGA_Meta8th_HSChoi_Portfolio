#include "GridManager.h"

GridManager::GridManager(int size) : gridGen(size), gridMove(gridGen) 
{
    initialize();
}

void GridManager::initialize() 
{
    gridGen.GridInitialLize();
}

bool GridManager::move()
{
    bEsc = false;
    bReturn = false;
    bool moved = false;
    bool bMoveTrig = true;
    while (bMoveTrig)
    {
        if (_kbhit())
        {
            int ch = _getch();
            if (ch == 27)
            {
                bEsc = true;
                bMoveTrig = false;
            }
            else if (ch == 82 || ch == 114)
            {
                bReturn = true;
                bMoveTrig = false;
            }
            else if (ch == 224)
            {
                ch = _getch();
                switch (ch)
                {
                case 72:
                    moved = gridMove.moveGridUP();
                    bMoveTrig = false;
                    break;
                case 80:
                    moved = gridMove.moveGridDown();
                    bMoveTrig = false;
                    break;
                case 75:
                    moved = gridMove.moveGridLeft();
                    bMoveTrig = false;
                    break;
                case 77:
                    moved = gridMove.moveGridRight();
                    bMoveTrig = false;
                    break;
                }
            }
            else 
            {
                for (int i = 0; i < 7; ++i)
                {
                    SetConsoleCursorPosition(5, 15);
                    cout << "                ";
                    Sleep(77);
                    SetConsoleCursorPosition(5, 15);
                    SetColor(ConsoleColor::Red);
                    cout << "Invalid Input !!";
                    Sleep(77);
                    DefaultColor();
                }
                SetConsoleCursorPosition(5, 15);
                cout << "                ";
            }
        }
    }
    return moved;
}

bool GridManager::SpawnNewNumber() 
{
    bool result = gridGen.SpawnRandomNumber();
    if (result) 
    {
        newSpawn = gridGen.getNewSpawn();
    }
    return result;
}

void GridManager::printGrid() 
{
    gridGen.testprint();
}

vector<vector<int>>& GridManager::getGrid()
{
    return gridGen.getGrid();
}

int GridManager::getCurrentScore(int size)
{
    int CurrentScore = 0;
    vector<vector<int>>& grid = gridGen.getGrid();
    for(int y = 0; y < size; ++y)
    {
        for(int x = 0; x < size; ++x)
        {
            CurrentScore += grid[y][x];
        }
    }

    return CurrentScore;
}

void GridManager::setGrid(const vector<vector<int>>& newGrid)
{
    gridGen.setGrid(newGrid);
}



