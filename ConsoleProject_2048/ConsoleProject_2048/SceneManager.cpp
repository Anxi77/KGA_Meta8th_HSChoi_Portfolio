#include "SceneManager.h"


void SceneManager::GameOverScene()
{
    system("cls");
    Borders();
    GameOverScreen::PrintGameover();
}

void SceneManager::StageScene(int size, int topscore, int currentscore, vector<vector<int>> grid, pair<int,int> newSpawn)
{
    Borders();
    StageScreen::Instructions();
    StageScreen::GridDisplay(size);
    StageScreen::TileDisplay(size, grid, newSpawn);
    StageScreen::Scores(topscore, currentscore);
}

void SceneManager::LobbyScene()
{
    LobbyScreen::PrintLogo();
}

SceneManager::SceneManager()
{
    SetConsoleCursorPosition(0, 0);
    ConsoleUtils::SetConsoleFontSize(24, 24);
    ConsoleUtils::SetConsoleBufferSize(82, 24);
    ConsoleUtils::SetConsoleWindowSize(82, 24);
    ConsoleUtils::CursorView();
}