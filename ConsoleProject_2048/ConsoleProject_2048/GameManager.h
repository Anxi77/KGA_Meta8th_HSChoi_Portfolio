#pragma once
#include "SceneManager.h"
#include "GridManager.h"
#include "ConsoleUtils.h"
#include <fstream>
#include <string>

using namespace ConsoleUtils;

class GameManager
{
public:
	static GameManager& getInstance() 
	{
		static GameManager instance;
		return instance;
	}
	void GameStart();
private:
	GameManager();
	~GameManager();
	GameManager(const GameManager&) = delete;
	GameManager& operator=(const GameManager&) = delete;
	bool GameVali(vector<vector<int>>& grid, int size);
	void SaveGame(int topscore, const vector<vector<int>>& grid);
	void LoadGame(int& topscore, vector<vector<int>>& grid);
	int topscore;
	vector<vector<int>> savedGrid;
	vector<vector<int>> previousGrid;
	const string SAVE_FILE = "2048_save.txt";
};

