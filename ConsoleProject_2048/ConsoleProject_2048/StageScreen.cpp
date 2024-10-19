#include "StageScreen.h"

void topscorePrint(unsigned int topscore)
{
    SetColor(ConsoleColor::LightGreen);
    if(topscore < 100)
    {
        ConsoleUtils::SetConsoleCursorPosition(71, 9);
        cout << topscore;
    }
    else if (topscore < 10000)
    {
        ConsoleUtils::SetConsoleCursorPosition(70, 9);
        cout << topscore;
    }
    else if (topscore < 1000000)
    {
        SetColor(ConsoleColor::Yellow);
        ConsoleUtils::SetConsoleCursorPosition(69, 9);
        cout << topscore;
    }
    else if (topscore < 2147483647)
    {
        SetColor(ConsoleColor::LightRed);
        ConsoleUtils::SetConsoleCursorPosition(68, 9);
        cout << topscore;
    }
    else
    {
        SetColor(ConsoleColor::LightRed);
        ConsoleUtils::SetConsoleCursorPosition(65, 9);
        cout << " You're CRAZY";
    }
}

void currentscorePrint(unsigned int currentscore)
{
    SetColor(ConsoleColor::LightGreen);
    if (currentscore < 100)
    {
        ConsoleUtils::SetConsoleCursorPosition(71, 4);
        cout << currentscore;
    }
    else if (currentscore < 10000)
    {
        ConsoleUtils::SetConsoleCursorPosition(70, 4);
        cout << currentscore;
    }
    else if (currentscore < 1000000)
    {
        SetColor(ConsoleColor::Yellow);
        ConsoleUtils::SetConsoleCursorPosition(69, 4);
        cout << currentscore;
    }
    else if (currentscore < 2147483647)
    {
        SetColor(ConsoleColor::LightRed);
        ConsoleUtils::SetConsoleCursorPosition(68, 4);
        cout << currentscore;
    }
    else
    {
        SetColor(ConsoleColor::LightRed);
        ConsoleUtils::SetConsoleCursorPosition(65, 4);
        cout << " You're CRAZY";
    }
}

void StageScreen::GridDisplay(int size)
{
    for (int i = 0; i < size; ++i)
    {
        int xPosition;
        int yPosition;
        if(size == 5)
        {
            xPosition = 27;
            yPosition = i * 4;
        }
        if(size == 4)
        {
            xPosition = 28;
            yPosition = (i * 4) + 2;
        }
        ConsoleUtils::SetConsoleCursorPosition(xPosition,1+yPosition);
        for (int top = 0; top < size; ++top)
        {
            cout << "┏" << "━" << "━" << "━" << "━" << "┓";
        }
        ConsoleUtils::SetConsoleCursorPosition(xPosition, 2 + yPosition);
        for (int middle = 0; middle < size; ++middle)
        {
            cout << "│" << " " << " " << " " << " " << "│";
        }
        ConsoleUtils::SetConsoleCursorPosition(xPosition, 3 + yPosition);
        for (int middle = 0; middle < size; ++middle)
        {
            cout << "│" << " " << " " << " " << " " << "│";
        }
        ConsoleUtils::SetConsoleCursorPosition(xPosition, 4 + yPosition);
        for (int bottom = 0; bottom < size; ++bottom)
        {
            cout << "┗" << "━" << "━" << "━" << "━" << "┛";
        }
        cout << endl;
    }
}

void StageScreen::Scores(int topscore, int currentScore)
{
    SetColor(ConsoleColor::Gray);
    ConsoleUtils::SetConsoleCursorPosition(63, 1);
    cout << "┏" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "┓";
    ConsoleUtils::SetConsoleCursorPosition(63, 2);
    cout << "│" << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << "│";
    ConsoleUtils::SetConsoleCursorPosition(63, 3);
    cout << "│" << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << "│";
    ConsoleUtils::SetConsoleCursorPosition(63, 4);
    cout << "│" << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << "│";
    ConsoleUtils::SetConsoleCursorPosition(63, 5);
    cout << "┗" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "┛";
    ConsoleUtils::SetConsoleCursorPosition(65, 2);
    DefaultColor();
    cout << "Current Score" << endl;
    SetColor(ConsoleColor::Gray);
    ConsoleUtils::SetConsoleCursorPosition(65, 4);
    cout << "[" << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << "]";
    DefaultColor();
    currentscorePrint(currentScore);
    DefaultColor();
    SetColor(ConsoleColor::Gray);
    ConsoleUtils::SetConsoleCursorPosition(63, 6);
    cout << "┏" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "┓";
    ConsoleUtils::SetConsoleCursorPosition(63, 7);
    cout << "│" << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << "│";
    ConsoleUtils::SetConsoleCursorPosition(63, 8);
    cout << "│" << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << "│";
    ConsoleUtils::SetConsoleCursorPosition(63, 9);
    cout << "│" << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << " " << "│";
    ConsoleUtils::SetConsoleCursorPosition(63, 10);
    cout << "┗" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "━" << "┛";
    DefaultColor();
    ConsoleUtils::SetConsoleCursorPosition(65, 7);
    cout << " " << " " << "Top Score" << endl;
    ConsoleUtils::SetConsoleCursorPosition(65, 9);
    SetColor(ConsoleColor::Gray);
    cout << "[" << " " << " " << " " << " " << " " << " " << " " << " " << " "<< " " << " " << "]";
    DefaultColor();
    topscorePrint(topscore);
    DefaultColor();
}

void StageScreen::TileDisplay(int size,vector<vector<int>>& grid, pair<int, int> newTilePos)
{
    for(int y = 0; y < size; ++y)
    {
        for (int x = 0; x < size; ++x) 
        {
            if (grid[y][x] != 0) 
            {
                int xPos;
                int yPos;
                int value = grid[y][x];
                if (size == 4) 
                {
                    xPos = 29 + (x * 6);
                    yPos = 4 + (y * 4);
                }
                else
                {
                    xPos = 28 + (x * 6);
                    yPos = 2 + (y * 4);
                }
                string nums = to_string(value);
                if (value < 10) 
                {
                    SetColor(ConsoleColor::Gray);
                    if (y == newTilePos.first && x == newTilePos.second)
                    {
                        SetColor(ConsoleColor::LightRed);
                    }
                    ConsoleUtils::SetConsoleCursorPosition(xPos + 2, yPos + 1);
                    cout << value;
                    DefaultColor();
                }
                else if (value < 100) 
                {
                    SetColor(ConsoleColor::LightYellow);
                    ConsoleUtils::SetConsoleCursorPosition(xPos + 1, yPos + 1);
                    cout << value;
                    DefaultColor();
                }
                else if (value < 1000) 
                {
                    SetColor(ConsoleColor::Yellow);
                    ConsoleUtils::SetConsoleCursorPosition(xPos + 1, yPos + 1);
                    cout << value;
                    DefaultColor();
                }
                //string으로 value를 받아서 앞 두자리 , 뒷 두자리 추출 후 2숫자씩 표현되게
                else if (value < 10000)
                {
                    SetColor(ConsoleColor::LightRed);
                    ConsoleUtils::SetConsoleCursorPosition(xPos + 1, yPos);
                    cout << nums.substr(0, 2);
                    ConsoleUtils::SetConsoleCursorPosition(xPos + 1, yPos + 1);
                    cout << nums.substr(2, 2);
                    DefaultColor();
                }
                else
                {
                    SetColor(ConsoleColor::LightPurple);
                    ConsoleUtils::SetConsoleCursorPosition(xPos + 1, yPos);
                    cout << nums.substr(0, 3);
                    ConsoleUtils::SetConsoleCursorPosition(xPos + 2, yPos + 1);
                    cout << nums.substr(3, 2);
                    if (nums.length() > 5)
                    {
                        ConsoleUtils::SetConsoleCursorPosition(xPos + 3, yPos + 1);
                        cout << "K";
                    }
                    DefaultColor();
                }
            }
        }
    }
}

void StageScreen::Instructions()
{
    SetConsoleCursorPosition(7,2);
    cout << "ESC : QUIT";
    SetConsoleCursorPosition(3,4);
    cout << "↑ ↓ → ←  To Move";
    SetConsoleCursorPosition(6,6);
    cout << "New Tile : ";
    SetColor(ConsoleColor::Red);
    cout << "RED";
    SetColor(ConsoleColor::Aqua);
    SetConsoleCursorPosition(3, 8);
    cout << "Press R to Go Back 1 Move";
    DefaultColor();
}
