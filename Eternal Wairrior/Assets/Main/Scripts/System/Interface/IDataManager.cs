public interface IDataManager<T>
{
    void SaveData(string key, T data);
    T LoadData(string key);
    bool DeleteData(string key);
    void ClearAll();
}